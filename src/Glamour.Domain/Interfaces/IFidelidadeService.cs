namespace Glamour.Domain.Interfaces;

public interface IFidelidadeService
{
    Task CreditarPontosAsync(string usuarioId, decimal valorPedido);
    Task<int> ObterPontosAsync(string usuarioId);
    Task<bool> RegatarPontosAsync(string usuarioId, int pontos);
}
