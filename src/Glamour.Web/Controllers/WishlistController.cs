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
        var produtos = await produtoService.ObterListagemPorIdsAsync(ids);
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
