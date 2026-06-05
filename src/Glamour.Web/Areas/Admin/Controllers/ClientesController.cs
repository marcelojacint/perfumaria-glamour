using Glamour.Application.Services;
using Glamour.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin/clientes")]
public class ClientesController(
    UserManager<ApplicationUser> userManager,
    PedidoService pedidoService) : Controller
{
    [HttpGet]
    public IActionResult Index(string? busca, int pagina = 1)
    {
        var query = userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(u => u.Email!.Contains(busca) || u.Nome.Contains(busca));

        var total = query.Count();
        const int tam = 20;
        var clientes = query
            .OrderByDescending(u => u.CriadoEm)
            .Skip((pagina - 1) * tam).Take(tam).ToList();

        ViewBag.Clientes = clientes;
        ViewBag.Total = total;
        ViewBag.PaginaAtual = pagina;
        ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)tam);
        return View();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detalhe(string id)
    {
        var usuario = await userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();
        var pedidos = await pedidoService.ObterPorUsuarioAsync(id);
        ViewBag.Pedidos = pedidos;
        return View(usuario);
    }

    [HttpPost("toggle-ativo/{id}")]
    public async Task<IActionResult> ToggleAtivo(string id)
    {
        var usuario = await userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();

        if (usuario.Ativo && await userManager.IsInRoleAsync(usuario, "Admin"))
        {
            TempData["Erro"] = "Não é possível inativar uma conta de administrador.";
            return RedirectToAction(nameof(Index));
        }

        usuario.Ativo = !usuario.Ativo;
        await userManager.UpdateAsync(usuario);
        TempData["Sucesso"] = usuario.Ativo ? "Conta ativada." : "Conta inativada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("excluir/{id}")]
    public async Task<IActionResult> Excluir(string id)
    {
        var usuario = await userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();

        if (usuario.Ativo)
        {
            TempData["Erro"] = "Só é possível excluir um cliente inativo. Inative a conta antes.";
            return RedirectToAction(nameof(Index));
        }

        if (await userManager.IsInRoleAsync(usuario, "Admin"))
        {
            TempData["Erro"] = "Não é possível excluir uma conta de administrador.";
            return RedirectToAction(nameof(Index));
        }

        var resultado = await userManager.DeleteAsync(usuario);
        TempData[resultado.Succeeded ? "Sucesso" : "Erro"] =
            resultado.Succeeded ? "Cliente excluído." : "Não foi possível excluir o cliente.";
        return RedirectToAction(nameof(Index));
    }
}
