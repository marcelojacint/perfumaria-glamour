using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin/configuracoes")]
public class ConfiguracoesController(IConfiguration config, IWebHostEnvironment env) : Controller
{
    private string ArquivoConfig => Path.Combine(env.ContentRootPath, "appsettings.json");

    [HttpGet]
    public IActionResult Index()
    {
        var loja = config.GetSection("Loja");
        ViewBag.Nome = loja["Nome"];
        ViewBag.Telefone = loja["Telefone"];
        ViewBag.Email = loja["Email"];
        ViewBag.EnderecoLoja = loja["EnderecoLoja"];
        ViewBag.HorarioFuncionamento = loja["HorarioFuncionamento"];
        ViewBag.LimiteFreteGratis = config["FreteGratis:Limite"] ?? "150";
        return View();
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
