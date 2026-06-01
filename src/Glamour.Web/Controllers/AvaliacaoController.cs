using Glamour.Application.DTOs;
using Glamour.Application.Services;
using Glamour.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

[Authorize]
[Route("avaliacoes")]
public class AvaliacaoController(
    AvaliacaoService avaliacaoService,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpPost("criar")]
    public async Task<IActionResult> Criar(CriarAvaliacaoDto dto, string? slug)
    {
        var usuario = await userManager.GetUserAsync(User);
        if (usuario != null)
            await avaliacaoService.CriarAsync(dto, usuario.Id, usuario.Nome);

        return RedirectToAction("Detalhe", "Catalogo", new { slug });
    }
}
