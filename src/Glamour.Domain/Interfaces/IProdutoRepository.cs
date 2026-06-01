using Glamour.Domain.Entities;

namespace Glamour.Domain.Interfaces;

public interface IProdutoRepository : IRepository<Produto>
{
    Task<Produto?> ObterPorSlugAsync(string slug);
    Task<IEnumerable<Produto>> ObterPorIdsAsync(IEnumerable<Guid> ids);
    Task<(IEnumerable<Produto> Produtos, int Total)> ListarAsync(
        string? busca, Guid? categoriaId, string? genero,
        decimal? precoMin, decimal? precoMax,
        string ordenarPor, bool descrescente,
        int pagina, int tamanhoPagina);
    Task<IEnumerable<Produto>> ObterDestaqueAsync(int quantidade);
    Task<IEnumerable<Produto>> ObterPromocoesAsync(int quantidade);
    Task<IEnumerable<Produto>> ObterRelacionadosAsync(Guid produtoId, Guid categoriaId, int quantidade);
}
