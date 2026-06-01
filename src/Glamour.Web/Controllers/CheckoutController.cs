using Glamour.Application.DTOs;
using Glamour.Application.Services;
using Glamour.Domain.Enums;
using Glamour.Domain.Interfaces;
using Glamour.Domain.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

[Authorize]
[Route("checkout")]
public class CheckoutController(
    PedidoService pedidoService,
    EnderecoService enderecoService,
    ICarrinhoService carrinhoService,
    NotificacaoContext notificacoes) : Controller
{
    private string CarrinhoId => HttpContext.Session.GetString("CarrinhoId") ?? "";
    private string UsuarioId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var itens = await carrinhoService.ObterCarrinhoAsync(CarrinhoId);
        if (!itens.Any()) return RedirectToAction("Index", "Carrinho");

        ViewBag.Enderecos = await enderecoService.ListarPorUsuarioAsync(UsuarioId);
        return View(itens);
    }

    [HttpPost]
    public async Task<IActionResult> Finalizar(
        string tipoEntrega, string metodoPagamento,
        Guid? enderecoId,
        // Campos para novo endereço de entrega
        string? cep, string? logradouro, string? numero,
        string? complemento, string? bairro, string? cidade, string? uf,
        string? cupomCodigo, string? observacoes)
    {
        if (!Enum.TryParse<TipoEntrega>(tipoEntrega, out var tipo))
            tipo = TipoEntrega.Entrega;

        if (!Enum.TryParse<MetodoPagamento>(metodoPagamento, out var metodo))
            metodo = MetodoPagamento.Dinheiro;

        Guid? enderecoFinalId = null;

        if (tipo == TipoEntrega.Entrega)
        {
            if (enderecoId.HasValue && enderecoId != Guid.Empty)
            {
                enderecoFinalId = enderecoId;
            }
            else if (!string.IsNullOrWhiteSpace(logradouro) && !string.IsNullOrWhiteSpace(cep))
            {
                var novoEndereco = new CriarEnderecoDto(
                    cep!, logradouro!, numero ?? "S/N",
                    complemento, bairro ?? "", cidade ?? "", uf ?? "SP", null);
                enderecoFinalId = await enderecoService.CriarAsync(UsuarioId, novoEndereco);
            }
            else
            {
                TempData["Erro"] = "Informe o endereço de entrega.";
                return RedirectToAction(nameof(Index));
            }
        }

        var dto = new CriarPedidoDto(tipo, metodo, enderecoFinalId, cupomCodigo, observacoes, CarrinhoId);
        var pedidoId = await pedidoService.CriarAsync(dto, UsuarioId);

        if (pedidoId == Guid.Empty)
        {
            foreach (var n in notificacoes.Notificacoes)
                TempData["Erro"] = n.Mensagem;
            return RedirectToAction(nameof(Index));
        }

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
