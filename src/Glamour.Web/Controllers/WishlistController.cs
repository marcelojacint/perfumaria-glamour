using Glamour.Application.Services;
using Glamour.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

[Authorize]
[Route("favoritos")]
public class WishlistController(IWishlistService wishlist, ProdutoService produtoService) : Controller
{
    private string UsuarioId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var ids = await wishlist.ObterAsync(UsuarioId);
        var produtos = new List<Glamour.Application.DTOs.ProdutoListagemDto>();
        foreach (var id in ids)
        {
            var p = await produtoService.ObterPorIdAsync(id);
            if (p != null)
                produtos.Add(new(p.Id, p.Nome, p.Slug, p.Preco, p.PrecoPromo, p.Marca,
                    p.Volume, p.Destaque, p.Estoque,
                    p.Imagens.FirstOrDefault(i => i.Principal)?.Url ?? p.Imagens.FirstOrDefault()?.Url));
        }
        return View(produtos);
    }

    [HttpPost("toggle/{produtoId}")]
    public async Task<IActionResult> Toggle(Guid produtoId, string? returnUrl)
    {
        if (await wishlist.ContemAsync(UsuarioId, produtoId))
            await wishlist.RemoverAsync(UsuarioId, produtoId);
        else
            await wishlist.AdicionarAsync(UsuarioId, produtoId);

        return LocalRedirect(returnUrl ?? "/favoritos");
    }
}
