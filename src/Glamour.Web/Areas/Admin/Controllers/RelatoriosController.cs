using System.Text;
using Glamour.Domain.Enums;
using Glamour.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Glamour.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin/relatorios")]
public class RelatoriosController(IPedidoRepository pedidoRepo) : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet("vendas-csv")]
    public async Task<IActionResult> VendasCsv(DateTime? de, DateTime? ate)
    {
        var inicio = de ?? DateTime.UtcNow.AddDays(-30);
        var fim = ate ?? DateTime.UtcNow;

        var (pedidos, _) = await pedidoRepo.ListarAdminAsync(null, null, inicio, fim, 1, 10000);

        var sb = new StringBuilder();
        sb.AppendLine("ID;Data;Status;Entrega;Pagamento;Subtotal;Frete;Desconto;Total");

        foreach (var p in pedidos.Where(p => p.Status != StatusPedido.Cancelado))
        {
            sb.AppendLine(
                $"{p.Id.ToString()[..8].ToUpper()};" +
                $"{p.CriadoEm:dd/MM/yyyy HH:mm};" +
                $"{p.Status};" +
                $"{p.TipoEntrega};" +
                $"{p.MetodoPagamento};" +
                $"R$ {p.Subtotal:N2};" +
                $"R$ {p.Frete:N2};" +
                $"R$ {p.Desconto:N2};" +
                $"R$ {p.Total:N2}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        var fileName = $"vendas_{inicio:yyyyMMdd}_{fim:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }

    [HttpGet("produtos-csv")]
    public async Task<IActionResult> ProdutosCsv(DateTime? de, DateTime? ate)
    {
        var inicio = de ?? DateTime.UtcNow.AddDays(-30);
        var fim = ate ?? DateTime.UtcNow;

        var (pedidos, _) = await pedidoRepo.ListarAdminAsync(null, StatusPedido.Entregue.ToString(), inicio, fim, 1, 10000);

        var sb = new StringBuilder();
        sb.AppendLine("Produto;Quantidade;Total");

        var itens = pedidos.SelectMany(p => p.Itens)
            .GroupBy(i => i.NomeProduto)
            .Select(g => new { Nome = g.Key, Qtd = g.Sum(i => i.Quantidade), Total = g.Sum(i => i.Subtotal) })
            .OrderByDescending(x => x.Total);

        foreach (var item in itens)
            sb.AppendLine($"{item.Nome};{item.Qtd};R$ {item.Total:N2}");

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", $"produtos_{inicio:yyyyMMdd}_{fim:yyyyMMdd}.csv");
    }
}
