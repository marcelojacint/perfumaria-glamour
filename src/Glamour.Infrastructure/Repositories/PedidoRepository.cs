using Glamour.Domain.Entities;
using Glamour.Domain.Enums;
using Glamour.Domain.Interfaces;
using Glamour.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Glamour.Infrastructure.Repositories;

public class PedidoRepository(GlamourDbContext context) : BaseRepository<Pedido>(context), IPedidoRepository
{
    public async Task<Pedido?> ObterComItensAsync(Guid pedidoId) =>
        await _dbSet
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .Include(p => p.Endereco)
            .Include(p => p.Pagamento)
            .FirstOrDefaultAsync(p => p.Id == pedidoId);

    public async Task<IEnumerable<Pedido>> ObterPorUsuarioAsync(string usuarioId) =>
        await _dbSet.AsNoTracking().Include(p => p.Itens).Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.CriadoEm).ToListAsync();

    public async Task<(IEnumerable<Pedido> Pedidos, int Total)> ListarAdminAsync(
        string? busca, string? status, DateTime? de, DateTime? ate,
        int pagina, int tamanhoPagina, bool incluirItens = false)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();
        if (incluirItens) query = query.Include(p => p.Itens);

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(p => p.Id.ToString().Contains(busca)
                || p.UsuarioId.Contains(busca));

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<StatusPedido>(status, out var s))
            query = query.Where(p => p.Status == s);

        if (de.HasValue) query = query.Where(p => p.CriadoEm >= de.Value);
        if (ate.HasValue) query = query.Where(p => p.CriadoEm <= ate.Value);

        var total = await query.CountAsync();
        var pedidos = await query.OrderByDescending(p => p.CriadoEm)
            .Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToListAsync();
        return (pedidos, total);
    }

    public async Task<decimal> TotalVendasHojeAsync()
    {
        var hoje = DateTime.UtcNow.Date;
        return await _dbSet.Where(p => p.CriadoEm >= hoje && p.Status != StatusPedido.Cancelado)
            .SumAsync(p => p.Total);
    }

    public async Task<int> TotalPedidosPendentesAsync() =>
        await _dbSet.CountAsync(p => p.Status == StatusPedido.Pendente || p.Status == StatusPedido.AguardandoPagamento);
}
