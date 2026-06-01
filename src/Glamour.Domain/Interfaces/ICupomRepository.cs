using Glamour.Domain.Entities;

namespace Glamour.Domain.Interfaces;

public interface ICupomRepository : IRepository<Cupom>
{
    Task<Cupom?> ObterPorCodigoAsync(string codigo);
}
