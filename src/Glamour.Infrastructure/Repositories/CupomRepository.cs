using Glamour.Domain.Entities;
using Glamour.Domain.Interfaces;
using Glamour.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Glamour.Infrastructure.Repositories;

public class CupomRepository(GlamourDbContext context) : BaseRepository<Cupom>(context), ICupomRepository
{
    public async Task<Cupom?> ObterPorCodigoAsync(string codigo) =>
        await _dbSet.FirstOrDefaultAsync(c => c.Codigo == codigo.ToUpper());
}
