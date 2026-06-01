using Glamour.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Glamour.Web.Controllers;

[Route("busca")]
public class BuscaController(ProdutoService produtoService) : Controller
{
    [HttpGet("sugestoes")]
    [EnableRateLimiting("api")]
    public async Task<IActionResult> Sugestoes(string? q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(new List<object>());

        var (produtos, _) = await produtoService.ListarAsync(q, null, null, null, null, "nome", false, 1, 6);
        var resultado = produtos.Select(p => new
        {
            id = p.Id,
            nome = p.Nome,
            marca = p.Marca,
            preco = p.PrecoPromo ?? p.Preco,
            slug = p.Slug,
            imagem = p.UrlImagemPrincipal
        });
        return Json(resultado);
    }
}
