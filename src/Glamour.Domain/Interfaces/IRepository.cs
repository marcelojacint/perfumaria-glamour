using System.Linq.Expressions;
using Glamour.Domain.Entities;

namespace Glamour.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<T>> ObterTodosAsync();
    Task<IEnumerable<T>> BuscarAsync(Expression<Func<T, bool>> predicado);
    Task AdicionarAsync(T entidade);
    Task AtualizarAsync(T entidade);
    Task RemoverAsync(T entidade);
    Task<int> SalvarAsync();
}
