namespace Glamour.Application.DTOs;

public record ProdutoDto(
    Guid Id, string Nome, string Slug, string Descricao,
    decimal Preco, decimal? PrecoPromo, int Estoque,
    string CategoriaId, string CategoriaNome,
    string Marca, string? Volume, string? Genero,
    bool Ativo, bool Destaque,
    IEnumerable<ProdutoImagemDto> Imagens,
    double NotaMedia, int TotalAvaliacoes);

public record ProdutoImagemDto(Guid Id, string Url, int Ordem, bool Principal);

public record ProdutoListagemDto(
    Guid Id, string Nome, string Slug, decimal Preco, decimal? PrecoPromo,
    string Marca, string? Volume, bool Destaque, int Estoque,
    string? UrlImagemPrincipal);

public record CriarProdutoDto(
    string Nome, string Slug, string Descricao,
    decimal Preco, decimal? PrecoPromo, int Estoque,
    Guid CategoriaId, string Marca, string? Volume, string? Genero, bool Destaque);

public record AtualizarProdutoDto(
    Guid Id, string Nome, string Slug, string Descricao,
    decimal Preco, decimal? PrecoPromo, int Estoque,
    Guid CategoriaId, string Marca, string? Volume, string? Genero, bool Destaque);
