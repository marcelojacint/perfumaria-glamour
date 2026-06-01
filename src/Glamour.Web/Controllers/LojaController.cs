using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

public class LojaController(IConfiguration config) : Controller
{
    [Route("sobre")]
    public IActionResult Sobre()
    {
        ViewBag.Title = "Sobre Nós";
        ViewBag.MetaDescription = "Conheça a Glamour Perfumaria e Acessórios — nossa história, valores e compromisso com a sofisticação.";
        return View();
    }

    [Route("contato")]
    public IActionResult Contato()
    {
        ViewBag.Title = "Contato";
        var loja = config.GetSection("Loja");
        ViewBag.Telefone = loja["Telefone"];
        ViewBag.Email = loja["Email"];
        ViewBag.EnderecoLoja = loja["EnderecoLoja"];
        ViewBag.HorarioFuncionamento = loja["HorarioFuncionamento"];
        return View();
    }

    [Route("politica-de-privacidade")]
    public IActionResult Privacidade()
    {
        ViewBag.Title = "Política de Privacidade";
        return View();
    }
}
