namespace Glamour.Web.Models;

public record MetodosPagamentoVm(string Campo, (string Val, string Nome, string Icon)[] Itens);
