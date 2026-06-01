using Glamour.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

public class CatalogoController(ProdutoService produtoService, CategoriaService categoriaService) : Controller
{
    [Route("catalogo")]
    public async Task<IActionResult> Index(
        string? busca, Guid? categoriaId, string? genero,
        decimal? precoMin, decimal? precoMax,
        string ordenarPor = "novo", int pagina = 1)
    {
        var (produtos, total) = await produtoService.ListarAsync(
            busca, categoriaId, genero, precoMin, precoMax, ordenarPor, true, pagina, 12);

        ViewBag.Produtos = produtos;
        ViewBag.Total = total;
        ViewBag.PaginaAtual = pagina;
        ViewBag.TotalPaginas = (int)Math.Ceiling(total / 12.0);
        ViewBag.Categorias = await categoriaService.ObterAtivasAsync();
        ViewBag.Filtros = new { busca, categoriaId, genero, precoMin, precoMax, ordenarPor };
        return View();
    }

    [Route("produto/{slug}")]
    public async Task<IActionResult> Detalhe(string slug)
    {
        var produto = await produtoService.ObterPorSlugAsync(slug);
        if (produto == null) return NotFound();
        return View(produto);
    }
}
