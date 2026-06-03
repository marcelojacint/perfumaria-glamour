using Glamour.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

public class HomeController(ProdutoService produtoService, CategoriaService categoriaService, HeroService heroService, CampanhaService campanhaService) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Hero = await heroService.ObterAsync();
        ViewBag.Campanhas = await campanhaService.ListarAtivasAsync();
        ViewBag.Promocoes = await produtoService.ObterPromocoesAsync(8);
        ViewBag.Destaques = await produtoService.ObterDestaqueAsync(8);
        ViewBag.Categorias = await categoriaService.ObterAtivasAsync();
        return View();
    }

    [Route("erro")]
    public IActionResult Erro() => View();
}
