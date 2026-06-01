using Glamour.Application.DTOs;
using Glamour.Application.Services;
using Glamour.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin/pedidos")]
public class PedidosController(PedidoService pedidoService, IPedidoRepository pedidoRepo) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? busca, string? status, DateTime? de, DateTime? ate, int pagina = 1)
    {
        var (pedidos, total) = await pedidoRepo.ListarAdminAsync(busca, status, de, ate, pagina, 20);
        ViewBag.Pedidos = pedidos;
        ViewBag.Total = total;
        ViewBag.PaginaAtual = pagina;
        ViewBag.TotalPaginas = (int)Math.Ceiling(total / 20.0);
        return View();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detalhe(Guid id)
    {
        var pedido = await pedidoService.ObterAsync(id);
        if (pedido == null) return NotFound();
        return View(pedido);
    }

    [HttpPost("status")]
    public async Task<IActionResult> AtualizarStatus(AtualizarStatusPedidoDto dto)
    {
        await pedidoService.AtualizarStatusAsync(dto);
        TempData["Sucesso"] = "Status atualizado.";
        return RedirectToAction(nameof(Detalhe), new { id = dto.PedidoId });
    }
}
