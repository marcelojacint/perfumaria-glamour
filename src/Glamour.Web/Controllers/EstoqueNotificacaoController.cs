using Glamour.Domain.Entities;
using Glamour.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

[Route("notificacao-estoque")]
public class EstoqueNotificacaoController(IRepository<NotificacaoEstoque> repo) : Controller
{
    [HttpPost("cadastrar")]
    public async Task<IActionResult> Cadastrar(Guid produtoId, string email, string? returnSlug)
    {
        if (string.IsNullOrWhiteSpace(email) || produtoId == Guid.Empty)
            return RedirectToAction("Detalhe", "Catalogo", new { slug = returnSlug });

        var existente = await repo.BuscarAsync(n => n.ProdutoId == produtoId && n.Email == email.ToLower());
        if (!existente.Any())
        {
            await repo.AdicionarAsync(new NotificacaoEstoque(produtoId, email));
            await repo.SalvarAsync();
        }

        TempData["Sucesso"] = "Você será notificado quando o produto voltar ao estoque!";
        return RedirectToAction("Detalhe", "Catalogo", new { slug = returnSlug });
    }
}
