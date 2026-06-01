using Glamour.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController(IPedidoRepository pedidoRepo) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.TotalVendasHoje = await pedidoRepo.TotalVendasHojeAsync();
        ViewBag.PedidosPendentes = await pedidoRepo.TotalPedidosPendentesAsync();
        return View();
    }
}
