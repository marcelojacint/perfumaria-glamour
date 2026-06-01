using Glamour.Application.Services;
using Glamour.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

public class CatalogoController(
    ProdutoService produtoService,
    CategoriaService categoriaService,
    AvaliacaoService avaliacaoService,
    IWishlistService wishlist) : Controller
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

        var avaliacoes = await avaliacaoService.ObterAprovadosAsync(produto.Id);
        ViewBag.Avaliacoes = avaliacoes;

        var relacionados = await produtoService.ObterRelacionadosAsync(produto.Id, Guid.Parse(produto.CategoriaId), 4);
        ViewBag.Relacionados = relacionados;

        // SEO
        ViewBag.MetaDescription = $"{produto.Nome} — {produto.Marca}, {produto.Volume}. {produto.Descricao.Split('.').FirstOrDefault()?.Trim()}.";
        ViewBag.MetaKeywords = $"{produto.Nome}, {produto.Marca}, perfume, {produto.Genero}, {produto.CategoriaNome}";
        ViewBag.OgType = "product";
        ViewBag.OgImage = produto.Imagens.FirstOrDefault(i => i.Principal)?.Url ?? produto.Imagens.FirstOrDefault()?.Url;

        bool naWishlist = false;
        if (User.Identity?.IsAuthenticated == true)
        {
            var uid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            naWishlist = await wishlist.ContemAsync(uid, produto.Id);
        }
        ViewBag.NaWishlist = naWishlist;

        return View(produto);
    }
}
