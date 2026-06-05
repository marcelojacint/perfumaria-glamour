using Glamour.Application.DTOs;
using Glamour.Application.Services;
using Glamour.Domain.Interfaces;
using Glamour.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Glamour.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin/pedidos")]
public class PedidosController(
    PedidoService pedidoService,
    IPedidoRepository pedidoRepo,
    Glamour.Domain.Notifications.NotificacaoContext notificacoes,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? busca, string? status, DateTime? de, DateTime? ate, int pagina = 1)
    {
        var (pedidos, total) = await pedidoRepo.ListarAdminAsync(busca, status, de, ate, pagina, 20);
        var lista = pedidos.ToList();

        var ids = lista.Select(p => p.UsuarioId).Distinct().ToList();
        var clientes = await userManager.Users
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => new ClienteResumo(u.Nome, u.Telefone ?? u.PhoneNumber));

        ViewBag.Pedidos = lista;
        ViewBag.Clientes = clientes;
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

        var cliente = await userManager.FindByIdAsync(pedido.UsuarioId);
        ViewBag.NomeCliente = cliente?.Nome ?? "Cliente";
        ViewBag.EmailCliente = cliente?.Email ?? "";
        ViewBag.TelefoneCliente = cliente?.Telefone ?? cliente?.PhoneNumber ?? "";
        return View(pedido);
    }

    [HttpPost("status")]
    public async Task<IActionResult> AtualizarStatus(AtualizarStatusPedidoDto dto)
    {
        await pedidoService.AtualizarStatusAsync(dto);
        TempData["Sucesso"] = "Status atualizado.";
        return RedirectToAction(nameof(Detalhe), new { id = dto.PedidoId });
    }

    [HttpPost("{id}/excluir")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        if (await pedidoService.ExcluirAsync(id))
        {
            TempData["Sucesso"] = "Pedido excluído.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var n in notificacoes.Notificacoes)
            TempData["Erro"] = n.Mensagem;
        return RedirectToAction(nameof(Detalhe), new { id });
    }

    [HttpPost("{id}/rastreio")]
    public async Task<IActionResult> DefinirRastreio(Guid id, string codigoRastreio)
    {
        var pedido = await pedidoRepo.ObterComItensAsync(id);
        if (pedido != null)
        {
            pedido.DefinirRastreio(codigoRastreio);
            await pedidoRepo.AtualizarAsync(pedido);
            await pedidoRepo.SalvarAsync();
            TempData["Sucesso"] = $"Código de rastreio {codigoRastreio} salvo e pedido marcado como Enviado.";
        }
        return RedirectToAction(nameof(Detalhe), new { id });
    }
}

public record ClienteResumo(string Nome, string? Telefone);
