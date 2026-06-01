using Glamour.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin/avaliacoes")]
public class AvaliacoesController(AvaliacaoService avaliacaoService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var pendentes = await avaliacaoService.ObterPendentesAsync();
        return View(pendentes);
    }

    [HttpPost("aprovar/{id}")]
    public async Task<IActionResult> Aprovar(Guid id)
    {
        await avaliacaoService.AprovarAsync(id);
        TempData["Sucesso"] = "Avaliação aprovada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("remover/{id}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await avaliacaoService.RemoverAsync(id);
        TempData["Sucesso"] = "Avaliação removida.";
        return RedirectToAction(nameof(Index));
    }
}
