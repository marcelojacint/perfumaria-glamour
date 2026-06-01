using Glamour.Application.Services;
using Glamour.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin/imagens")]
public class ImagensController(ImagemService imagemService) : Controller
{
    [HttpPost("upload/{produtoId}")]
    public async Task<IActionResult> Upload(Guid produtoId, IFormFile arquivo, bool principal = false)
    {
        if (arquivo == null || arquivo.Length == 0)
        {
            TempData["Erro"] = "Selecione um arquivo.";
            return RedirectToAction("Editar", "Produtos", new { id = produtoId });
        }

        var (ok, resultado) = await imagemService.SalvarImagemAsync(arquivo);
        if (!ok)
        {
            TempData["Erro"] = resultado;
            return RedirectToAction("Editar", "Produtos", new { id = produtoId });
        }

        await imagemService.AdicionarImagemProdutoAsync(produtoId, resultado!, principal);
        TempData["Sucesso"] = "Imagem adicionada.";
        return RedirectToAction("Editar", "Produtos", new { id = produtoId });
    }

    [HttpPost("url/{produtoId}")]
    public async Task<IActionResult> AdicionarUrl(Guid produtoId, string url, bool principal = false)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            TempData["Erro"] = "URL inválida.";
            return RedirectToAction("Editar", "Produtos", new { id = produtoId });
        }

        await imagemService.AdicionarImagemProdutoAsync(produtoId, url, principal);
        TempData["Sucesso"] = "Imagem adicionada.";
        return RedirectToAction("Editar", "Produtos", new { id = produtoId });
    }
}
