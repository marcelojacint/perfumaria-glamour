using Glamour.Domain.Entities;
using Glamour.Domain.Interfaces;
using Glamour.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Glamour.Infrastructure.Repositories;

public class ProdutoRepository(GlamourDbContext context) : BaseRepository<Produto>(context), IProdutoRepository
{
    public async Task<Produto?> ObterPorSlugAsync(string slug) =>
        await _dbSet.AsNoTracking().Include(p => p.Imagens).Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Slug == slug);

    public async Task<IEnumerable<Produto>> ObterPorIdsAsync(IEnumerable<Guid> ids)
    {
        var lista = ids.Distinct().ToList();
        if (lista.Count == 0) return [];
        return await _dbSet.AsNoTracking().Include(p => p.Imagens)
            .Where(p => lista.Contains(p.Id))
            .ToListAsync();
    }

    public async Task<(IEnumerable<Produto> Produtos, int Total)> ListarAsync(
        string? busca, Guid? categoriaId, string? genero,
        decimal? precoMin, decimal? precoMax,
        string ordenarPor, bool descrescente,
        int pagina, int tamanhoPagina)
    {
        var query = _dbSet.AsNoTracking().Include(p => p.Imagens).Include(p => p.Categoria)
            .Where(p => p.Ativo).AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(p => EF.Functions.ILike(p.Nome, $"%{busca}%")
                || EF.Functions.ILike(p.Marca, $"%{busca}%"));

        if (categoriaId.HasValue)
            query = query.Where(p => p.CategoriaId == categoriaId.Value);

        if (!string.IsNullOrWhiteSpace(genero))
            query = query.Where(p => p.Genero == genero);

        if (precoMin.HasValue)
            query = query.Where(p => p.Preco >= precoMin.Value);

        if (precoMax.HasValue)
            query = query.Where(p => p.Preco <= precoMax.Value);

        query = ordenarPor switch
        {
            "preco" => descrescente ? query.OrderByDescending(p => p.Preco) : query.OrderBy(p => p.Preco),
            "nome" => descrescente ? query.OrderByDescending(p => p.Nome) : query.OrderBy(p => p.Nome),
            _ => query.OrderByDescending(p => p.CriadoEm)
        };

        var total = await query.CountAsync();
        var produtos = await query.Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToListAsync();
        return (produtos, total);
    }

    public async Task<IEnumerable<Produto>> ObterDestaqueAsync(int quantidade) =>
        await _dbSet.AsNoTracking().Include(p => p.Imagens).Where(p => p.Ativo && p.Destaque)
            .OrderByDescending(p => p.CriadoEm).Take(quantidade).ToListAsync();

    public async Task<IEnumerable<Produto>> ObterPromocoesAsync(int quantidade) =>
        await _dbSet.AsNoTracking().Include(p => p.Imagens)
            .Where(p => p.Ativo && p.PrecoPromo != null && p.PrecoPromo < p.Preco)
            .OrderByDescending(p => (p.Preco - p.PrecoPromo!.Value) / p.Preco)
            .Take(quantidade).ToListAsync();

    public async Task<IEnumerable<Produto>> ObterRelacionadosAsync(Guid produtoId, Guid categoriaId, int quantidade) =>
        await _dbSet.AsNoTracking().Include(p => p.Imagens)
            .Where(p => p.Ativo && p.CategoriaId == categoriaId && p.Id != produtoId)
            .OrderBy(_ => EF.Functions.Random()).Take(quantidade).ToListAsync();
}
