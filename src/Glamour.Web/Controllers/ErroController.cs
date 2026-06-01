using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

[Route("erro")]
public class ErroController : Controller
{
    [HttpGet("{codigo?}")]
    public IActionResult Index(int? codigo)
    {
        if (codigo == 404)
            return View("Error404");
        return View("Erro");
    }
}
