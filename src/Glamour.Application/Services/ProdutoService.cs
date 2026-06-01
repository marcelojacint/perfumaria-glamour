using Glamour.Application.DTOs;
using Glamour.Domain.Entities;
using Glamour.Domain.Interfaces;
using Glamour.Domain.Notifications;

namespace Glamour.Application.Services;

public class ProdutoService(IProdutoRepository produtoRepo, ICategoriaRepository categoriaRepo, NotificacaoContext notificacoes)
{
    public async Task<ProdutoDto?> ObterPorIdAsync(Guid id)
    {
        var produto = await produtoRepo.ObterPorIdAsync(id);
        return produto == null ? null : MapDto(produto);
    }

    public async Task<ProdutoDto?> ObterPorSlugAsync(string slug)
    {
        var produto = await produtoRepo.ObterPorSlugAsync(slug);
        return produto == null ? null : MapDto(produto);
    }

    public async Task<(IEnumerable<ProdutoListagemDto> Produtos, int Total)> ListarAsync(
        string? busca, Guid? categoriaId, string? genero,
        decimal? precoMin, decimal? precoMax,
        string ordenarPor = "novo", bool descrescente = true,
        int pagina = 1, int tamanhoPagina = 12)
    {
        var (produtos, total) = await produtoRepo.ListarAsync(busca, categoriaId, genero,
            precoMin, precoMax, ordenarPor, descrescente, pagina, tamanhoPagina);
        return (produtos.Select(MapListagemDto), total);
    }

    public async Task<IEnumerable<ProdutoListagemDto>> ObterDestaqueAsync(int quantidade = 8) =>
        (await produtoRepo.ObterDestaqueAsync(quantidade)).Select(MapListagemDto);

    public async Task<IEnumerable<ProdutoListagemDto>> ObterPromocoesAsync(int quantidade = 8) =>
        (await produtoRepo.ObterPromocoesAsync(quantidade)).Select(MapListagemDto);

    public async Task<IEnumerable<ProdutoListagemDto>> ObterRelacionadosAsync(Guid produtoId, Guid categoriaId, int quantidade = 4) =>
        (await produtoRepo.ObterRelacionadosAsync(produtoId, categoriaId, quantidade)).Select(MapListagemDto);

    public async Task<Guid> CriarAsync(CriarProdutoDto dto)
    {
        var categoria = await categoriaRepo.ObterPorIdAsync(dto.CategoriaId);
        if (categoria == null) { notificacoes.Adicionar("CategoriaId", "Categoria não encontrada."); return Guid.Empty; }

        var produto = new Produto(dto.Nome, dto.Slug, dto.Descricao, dto.Preco,
            dto.Estoque, dto.CategoriaId, dto.Marca, dto.Volume, dto.Genero);
        produto.Atualizar(dto.Nome, dto.Slug, dto.Descricao, dto.Preco, dto.PrecoPromo,
            dto.CategoriaId, dto.Marca, dto.Volume, dto.Genero, dto.Destaque);

        await produtoRepo.AdicionarAsync(produto);
        await produtoRepo.SalvarAsync();
        return produto.Id;
    }

    public async Task<bool> AtualizarAsync(AtualizarProdutoDto dto)
    {
        var produto = await produtoRepo.ObterPorIdAsync(dto.Id);
        if (produto == null) { notificacoes.Adicionar("Id", "Produto não encontrado."); return false; }

        produto.Atualizar(dto.Nome, dto.Slug, dto.Descricao, dto.Preco, dto.PrecoPromo,
            dto.CategoriaId, dto.Marca, dto.Volume, dto.Genero, dto.Destaque);

        await produtoRepo.AtualizarAsync(produto);
        await produtoRepo.SalvarAsync();
        return true;
    }

    public async Task<bool> RemoverAsync(Guid id)
    {
        var produto = await produtoRepo.ObterPorIdAsync(id);
        if (produto == null) { notificacoes.Adicionar("Id", "Produto não encontrado."); return false; }
        produto.Desativar();
        await produtoRepo.AtualizarAsync(produto);
        await produtoRepo.SalvarAsync();
        return true;
    }

    private static ProdutoDto MapDto(Produto p) => new(
        p.Id, p.Nome, p.Slug, p.Descricao, p.Preco, p.PrecoPromo, p.Estoque,
        p.CategoriaId.ToString(), p.Categoria?.Nome ?? "",
        p.Marca, p.Volume, p.Genero, p.Ativo, p.Destaque,
        p.Imagens.Select(i => new ProdutoImagemDto(i.Id, i.Url, i.Ordem, i.Principal)),
        p.Avaliacoes.Any() ? p.Avaliacoes.Average(a => a.Nota) : 0,
        p.Avaliacoes.Count);

    private static ProdutoListagemDto MapListagemDto(Produto p) => new(
        p.Id, p.Nome, p.Slug, p.Preco, p.PrecoPromo, p.Marca, p.Volume, p.Destaque, p.Estoque,
        p.Imagens.FirstOrDefault(i => i.Principal)?.Url ?? p.Imagens.FirstOrDefault()?.Url);
}
