using Glamour.Application.DTOs;
using Glamour.Application.Services;
using Glamour.Domain.Enums;
using Glamour.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

[Authorize]
[Route("checkout")]
public class CheckoutController(
    PedidoService pedidoService,
    ICarrinhoService carrinhoService) : Controller
{
    private string CarrinhoId => HttpContext.Session.GetString("CarrinhoId") ?? "";

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var itens = await carrinhoService.ObterCarrinhoAsync(CarrinhoId);
        if (!itens.Any()) return RedirectToAction("Index", "Carrinho");
        return View(itens);
    }

    [HttpPost]
    public async Task<IActionResult> Finalizar(
        string tipoEntrega, string metodoPagamento,
        Guid? enderecoId, string? cupomCodigo, string? observacoes)
    {
        if (!Enum.TryParse<TipoEntrega>(tipoEntrega, out var tipo))
            tipo = TipoEntrega.Entrega;

        if (!Enum.TryParse<MetodoPagamento>(metodoPagamento, out var metodo))
            metodo = MetodoPagamento.Dinheiro;

        var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

        var dto = new CriarPedidoDto(tipo, metodo, enderecoId, cupomCodigo, observacoes, CarrinhoId);
        var pedidoId = await pedidoService.CriarAsync(dto, usuarioId);

        if (pedidoId == Guid.Empty)
            return RedirectToAction(nameof(Index));

        return RedirectToAction(nameof(Confirmacao), new { id = pedidoId });
    }

    [HttpGet("confirmacao/{id}")]
    public async Task<IActionResult> Confirmacao(Guid id)
    {
        var pedido = await pedidoService.ObterAsync(id);
        if (pedido == null) return NotFound();
        return View(pedido);
    }
}
