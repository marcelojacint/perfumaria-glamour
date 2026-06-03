using Glamour.Application.Services;
using Glamour.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin/campanhas")]
public class CampanhasController(CampanhaService campanhaService, ImagemService imagemService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var campanhas = await campanhaService.ListarTodasAsync();
        return View(campanhas);
    }

    [HttpGet("criar")]
    public IActionResult Criar() => View("Form", null);

    [HttpPost("criar")]
    public async Task<IActionResult> Criar(string titulo, string? subtitulo, string? link, int ordem, IFormFile? imagem)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            TempData["Erro"] = "Informe o título da campanha.";
            return RedirectToAction(nameof(Criar));
        }

        var id = await campanhaService.CriarAsync(titulo, subtitulo, link, ordem);

        if (imagem is { Length: > 0 })
        {
            var (ok, urlOuErro) = await imagemService.SalvarImagemAsync(imagem, "campanhas");
            if (ok) await campanhaService.DefinirImagemAsync(id, urlOuErro);
            else TempData["Erro"] = urlOuErro;
        }

        TempData["Sucesso"] ??= "Campanha criada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("editar/{id}")]
    public async Task<IActionResult> Editar(Guid id)
    {
        var campanha = await campanhaService.ObterAsync(id);
        if (campanha == null) return NotFound();
        return View("Form", campanha);
    }

    [HttpPost("editar")]
    public async Task<IActionResult> Editar(Guid id, string titulo, string? subtitulo, string? link, int ordem,
        bool ativa = false, IFormFile? imagem = null, bool removerImagem = false)
    {
        await campanhaService.AtualizarAsync(id, titulo, subtitulo, link, ordem, ativa);

        if (imagem is { Length: > 0 })
        {
            var (ok, urlOuErro) = await imagemService.SalvarImagemAsync(imagem, "campanhas");
            if (ok) await campanhaService.DefinirImagemAsync(id, urlOuErro);
            else TempData["Erro"] = urlOuErro;
        }
        else if (removerImagem)
        {
            await campanhaService.DefinirImagemAsync(id, null);
        }

        TempData["Sucesso"] ??= "Campanha atualizada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("remover/{id}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        await campanhaService.RemoverAsync(id);
        TempData["Sucesso"] = "Campanha removida.";
        return RedirectToAction(nameof(Index));
    }
}
