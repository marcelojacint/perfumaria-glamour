using Glamour.Domain.Entities;

namespace Glamour.Domain.Interfaces;

public interface IPedidoRepository : IRepository<Pedido>
{
    Task<Pedido?> ObterComItensAsync(Guid pedidoId);
    Task<IEnumerable<Pedido>> ObterPorUsuarioAsync(string usuarioId);
    Task<(IEnumerable<Pedido> Pedidos, int Total)> ListarAdminAsync(
        string? busca, string? status, DateTime? de, DateTime? ate,
        int pagina, int tamanhoPagina);
    Task<decimal> TotalVendasHojeAsync();
    Task<int> TotalPedidosPendentesAsync();
}
