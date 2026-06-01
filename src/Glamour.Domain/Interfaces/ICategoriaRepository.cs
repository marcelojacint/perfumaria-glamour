using Glamour.Domain.Entities;

namespace Glamour.Domain.Interfaces;

public interface ICategoriaRepository : IRepository<Categoria>
{
    Task<Categoria?> ObterPorSlugAsync(string slug);
    Task<IEnumerable<Categoria>> ObterAtivasAsync();
}
