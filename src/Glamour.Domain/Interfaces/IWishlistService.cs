namespace Glamour.Domain.Interfaces;

public interface IWishlistService
{
    Task<List<Guid>> ObterAsync(string usuarioId);
    Task AdicionarAsync(string usuarioId, Guid produtoId);
    Task RemoverAsync(string usuarioId, Guid produtoId);
    Task<bool> ContemAsync(string usuarioId, Guid produtoId);
}
