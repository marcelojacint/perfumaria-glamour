using Glamour.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Glamour.Web.Controllers;

public class HomeController(ProdutoService produtoService, CategoriaService categoriaService) : Controller
{
    [OutputCache(PolicyName = "home")]
    public async Task<IActionResult> Index()
    {
        ViewBag.Destaques = await produtoService.ObterDestaqueAsync(8);
        ViewBag.Categorias = await categoriaService.ObterAtivasAsync();
        return View();
    }

    [Route("erro")]
    public IActionResult Erro() => View();
}
