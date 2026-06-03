using Glamour.Application.DTOs;
using Glamour.Application.Services;
using Glamour.Domain.Enums;
using Glamour.Domain.Notifications;
using Glamour.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin/vendas")]
public class VendasController(
    ProdutoService produtoService,
    PedidoService pedidoService,
    UserManager<ApplicationUser> userManager,
    NotificacaoContext notificacoes) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var (produtos, _) = await produtoService.ListarAsync(null, null, null, null, null, "nome", false, 1, 500);
        ViewBag.Produtos = produtos.Where(p => p.Estoque > 0).ToList();
        return View();
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar(Guid[] produtoId, int[] quantidade,
        MetodoPagamento metodoPagamento, string? nomeCliente, decimal desconto = 0, string? observacoes = null)
    {
        var itens = produtoId.Zip(quantidade, (p, q) => new ItemVendaLojaDto(p, q)).ToList();
        var adminId = userManager.GetUserId(User)!;

        var dto = new RegistrarVendaLojaDto(itens, metodoPagamento, nomeCliente, desconto, observacoes);
        var id = await pedidoService.RegistrarVendaLojaAsync(dto, adminId);

        if (id == Guid.Empty)
        {
            foreach (var n in notificacoes.Notificacoes)
                TempData["Erro"] = n.Mensagem;
            return RedirectToAction(nameof(Index));
        }

        TempData["Sucesso"] = "Venda registrada e estoque atualizado.";
        return RedirectToAction("Detalhe", "Pedidos", new { area = "Admin", id });
    }
}
