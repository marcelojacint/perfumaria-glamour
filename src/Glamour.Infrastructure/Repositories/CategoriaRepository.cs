using Glamour.Domain.Entities;
using Glamour.Domain.Interfaces;
using Glamour.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Glamour.Infrastructure.Repositories;

public class CategoriaRepository(GlamourDbContext context) : BaseRepository<Categoria>(context), ICategoriaRepository
{
    public async Task<Categoria?> ObterPorSlugAsync(string slug) =>
        await _dbSet.AsNoTracking().FirstOrDefaultAsync(c => c.Slug == slug);

    public async Task<IEnumerable<Categoria>> ObterAtivasAsync() =>
        await _dbSet.AsNoTracking().Where(c => c.Ativo).OrderBy(c => c.Ordem).ThenBy(c => c.Nome).ToListAsync();
}
