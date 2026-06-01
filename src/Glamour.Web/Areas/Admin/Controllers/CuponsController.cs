using Glamour.Application.DTOs;
using Glamour.Domain.Entities;
using Glamour.Domain.Enums;
using Glamour.Domain.Interfaces;
using Glamour.Domain.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin/cupons")]
public class CuponsController(ICupomRepository cupomRepo, NotificacaoContext notificacoes) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cupons = await cupomRepo.ObterTodosAsync();
        return View(cupons.OrderByDescending(c => c.CriadoEm));
    }

    [HttpGet("criar")]
    public IActionResult Criar() => View();

    [HttpPost("criar")]
    public async Task<IActionResult> Criar(
        string codigo, string tipoDesconto, decimal valor,
        DateTime? validade, int usoMaximo, decimal? valorMinimoPedido)
    {
        if (!Enum.TryParse<TipoDesconto>(tipoDesconto, out var tipo))
            tipo = TipoDesconto.Percentual;

        var cupom = new Cupom(codigo, tipo, valor, validade?.ToUniversalTime(), usoMaximo, valorMinimoPedido);
        await cupomRepo.AdicionarAsync(cupom);
        await cupomRepo.SalvarAsync();
        TempData["Sucesso"] = $"Cupom {codigo.ToUpper()} criado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("desativar/{id}")]
    public async Task<IActionResult> Desativar(Guid id)
    {
        var cupom = await cupomRepo.ObterPorIdAsync(id);
        if (cupom != null)
        {
            cupom.Desativar();
            await cupomRepo.AtualizarAsync(cupom);
            await cupomRepo.SalvarAsync();
        }
        TempData["Sucesso"] = "Cupom desativado.";
        return RedirectToAction(nameof(Index));
    }
}
