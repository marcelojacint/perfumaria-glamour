using System.Linq.Expressions;
using Glamour.Domain.Entities;
using Glamour.Domain.Interfaces;
using Glamour.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Glamour.Infrastructure.Repositories;

public class BaseRepository<T>(GlamourDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly GlamourDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<T?> ObterPorIdAsync(Guid id) =>
        await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> ObterTodosAsync() =>
        await _dbSet.ToListAsync();

    public async Task<IEnumerable<T>> BuscarAsync(Expression<Func<T, bool>> predicado) =>
        await _dbSet.Where(predicado).ToListAsync();

    public async Task AdicionarAsync(T entidade) =>
        await _dbSet.AddAsync(entidade);

    public Task AtualizarAsync(T entidade)
    {
        _dbSet.Update(entidade);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(T entidade)
    {
        _dbSet.Remove(entidade);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAsync() =>
        await _context.SaveChangesAsync();
}
