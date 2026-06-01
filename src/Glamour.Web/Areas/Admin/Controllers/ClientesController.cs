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
        usuario.Ativo = !usuario.Ativo;
        await userManager.UpdateAsync(usuario);
        TempData["Sucesso"] = usuario.Ativo ? "Conta ativada." : "Conta bloqueada.";
        return RedirectToAction(nameof(Index));
    }
}
