using Glamour.Application.Services;
using Glamour.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin/configuracoes")]
public class ConfiguracoesController(
    IConfiguration config,
    IWebHostEnvironment env,
    HeroService heroService,
    ImagemService imagemService) : Controller
{
    private string ArquivoConfig => Path.Combine(env.ContentRootPath, "appsettings.json");

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var loja = config.GetSection("Loja");
        ViewBag.Nome = loja["Nome"];
        ViewBag.Telefone = loja["Telefone"];
        ViewBag.Email = loja["Email"];
        ViewBag.EnderecoLoja = loja["EnderecoLoja"];
        ViewBag.HorarioFuncionamento = loja["HorarioFuncionamento"];
        ViewBag.LimiteFreteGratis = config["FreteGratis:Limite"] ?? "150";
        ViewBag.Hero = await heroService.ObterAsync();
        return View();
    }

    [HttpPost("hero")]
    public async Task<IActionResult> SalvarHero(
        string? eyebrow, string? titulo, string? tituloDestaque, string? subtitulo,
        string? corDestaque, string? corTexto, IFormFile? imagem, bool removerImagem = false)
    {
        await heroService.AtualizarAsync(eyebrow, titulo, tituloDestaque, subtitulo, corDestaque, corTexto);

        if (imagem is { Length: > 0 })
        {
            var (ok, urlOuErro) = await imagemService.SalvarImagemAsync(imagem, "hero");
            if (ok)
                await heroService.DefinirImagemAsync(urlOuErro);
            else
                TempData["Erro"] = urlOuErro;
        }
        else if (removerImagem)
        {
            await heroService.DefinirImagemAsync(null);
        }

        TempData["Sucesso"] ??= "Banner principal atualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("hero/restaurar")]
    public async Task<IActionResult> RestaurarHero()
    {
        await heroService.RestaurarPadraoAsync();
        TempData["Sucesso"] = "Banner principal restaurado para o padrão.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Salvar(
        string nome, string telefone, string email,
        string enderecoLoja, string horarioFuncionamento,
        string limiteFreteGratis)
    {
        var conteudo = await System.IO.File.ReadAllTextAsync(ArquivoConfig);
        var json = System.Text.Json.JsonDocument.Parse(conteudo);
        var root = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(conteudo)!;

        var novaLoja = new Dictionary<string, string>
        {
            ["Nome"] = nome,
            ["Telefone"] = telefone,
            ["Email"] = email,
            ["EnderecoLoja"] = enderecoLoja,
            ["HorarioFuncionamento"] = horarioFuncionamento
        };

        root["Loja"] = novaLoja;
        root["FreteGratis"] = new Dictionary<string, string> { ["Limite"] = limiteFreteGratis };

        var opts = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
        await System.IO.File.WriteAllTextAsync(ArquivoConfig, System.Text.Json.JsonSerializer.Serialize(root, opts));

        TempData["Sucesso"] = "Configurações salvas. Reinicie o app para aplicar.";
        return RedirectToAction(nameof(Index));
    }
}
