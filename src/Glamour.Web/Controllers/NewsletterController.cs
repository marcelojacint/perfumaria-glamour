using Glamour.Domain.Entities;
using Glamour.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Controllers;

[Route("newsletter")]
public class NewsletterController(IRepository<Newsletter> repo) : Controller
{
    [HttpPost("inscrever")]
    public async Task<IActionResult> Inscrever(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return RedirectToAction("Index", "Home");

        var existente = await repo.BuscarAsync(n => n.Email == email.ToLower().Trim());
        if (!existente.Any())
        {
            await repo.AdicionarAsync(new Newsletter(email));
            await repo.SalvarAsync();
        }

        TempData["NewsletterSucesso"] = "Obrigado! Você está inscrito na nossa newsletter.";
        return RedirectToAction("Index", "Home");
    }
}
