using Glamour.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

[Route("quiz")]
public class QuizController(ProdutoService produtoService) : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost("resultado")]
    public async Task<IActionResult> Resultado(
        string ocasiao, string intensidade, string familia, string genero)
    {
        // Mapeamento simples de respostas → slugs recomendados
        var recomendados = (ocasiao, intensidade, familia, genero) switch
        {
            (_, _, "floral", "feminino") => new[] { "la-vie-est-belle", "black-opium", "bombshell" },
            (_, _, "amadeirado", _) => new[] { "oud-wood", "bleu-de-chanel", "sauvage" },
            (_, "suave", _, "masculino") => new[] { "acqua-di-gio", "invictus", "bleu-de-chanel" },
            (_, "intenso", _, "masculino") => new[] { "sauvage", "bleu-de-chanel", "invictus" },
            ("noturno", _, _, _) => new[] { "black-opium", "oud-wood", "sauvage" },
            (_, _, "citrico", _) => new[] { "acqua-di-gio", "bleu-de-chanel", "bombshell" },
            _ => new[] { "libre", "la-vie-est-belle", "sauvage" }
        };

        var produtos = new List<Glamour.Application.DTOs.ProdutoDto?>();
        foreach (var slug in recomendados)
        {
            var p = await produtoService.ObterPorSlugAsync(slug);
            if (p != null) produtos.Add(p);
        }

        ViewBag.Respostas = new { ocasiao, intensidade, familia, genero };
        return View(produtos.Where(p => p != null).Take(3).ToList());
    }
}
