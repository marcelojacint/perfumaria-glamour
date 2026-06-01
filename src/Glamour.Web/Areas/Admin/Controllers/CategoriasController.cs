using Glamour.Application.DTOs;
using Glamour.Application.Services;
using Glamour.Domain.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin/categorias")]
public class CategoriasController(CategoriaService categoriaService, NotificacaoContext notificacoes) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categorias = await categoriaService.ObterTodasAsync();
        return View(categorias);
    }

    [HttpGet("criar")]
    public IActionResult Criar() => View();

    [HttpPost("criar")]
    public async Task<IActionResult> Criar(CriarCategoriaDto dto)
    {
        await categoriaService.CriarAsync(dto);
        TempData["Sucesso"] = "Categoria criada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("editar/{id}")]
    public async Task<IActionResult> Editar(Guid id)
    {
        var cats = await categoriaService.ObterTodasAsync();
        var cat = cats.FirstOrDefault(c => c.Id == id);
        if (cat == null) return NotFound();
        return View(cat);
    }

    [HttpPost("editar")]
    public async Task<IActionResult> Editar(AtualizarCategoriaDto dto)
    {
        await categoriaService.AtualizarAsync(dto);
        if (!notificacoes.Valido)
        {
            foreach (var n in notificacoes.Notificacoes)
                ModelState.AddModelError(n.Campo, n.Mensagem);
            return View();
        }
        TempData["Sucesso"] = "Categoria atualizada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("toggle/{id}")]
    public async Task<IActionResult> Toggle(Guid id)
    {
        await categoriaService.ToggleAtivoAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
