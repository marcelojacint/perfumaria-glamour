namespace Glamour.Domain.Entities;

public class Produto : BaseEntity
{
    public string Nome { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public decimal? PrecoPromo { get; private set; }
    public int Estoque { get; private set; }
    public Guid CategoriaId { get; private set; }
    public Categoria Categoria { get; private set; } = null!;
    public string Marca { get; private set; } = string.Empty;
    public string? Volume { get; private set; }
    public string? Genero { get; private set; }
    public bool Ativo { get; private set; } = true;
    public bool Destaque { get; private set; }

    private readonly List<ProdutoImagem> _imagens = [];
    public IReadOnlyCollection<ProdutoImagem> Imagens => _imagens.AsReadOnly();

    private readonly List<Avaliacao> _avaliacoes = [];
    public IReadOnlyCollection<Avaliacao> Avaliacoes => _avaliacoes.AsReadOnly();

    public decimal PrecoEfetivo => PrecoPromo ?? Preco;

    protected Produto() { }

    public Produto(string nome, string slug, string descricao, decimal preco, int estoque,
        Guid categoriaId, string marca, string? volume = null, string? genero = null)
    {
        Nome = nome;
        Slug = slug.ToLowerInvariant();
        Descricao = descricao;
        Preco = preco;
        Estoque = estoque;
        CategoriaId = categoriaId;
        Marca = marca;
        Volume = volume;
        Genero = genero;
    }

    public void Atualizar(string nome, string slug, string descricao, decimal preco,
        decimal? precoPromo, Guid categoriaId, string marca, string? volume, string? genero, bool destaque)
    {
        Nome = nome;
        Slug = slug.ToLowerInvariant();
        Descricao = descricao;
        Preco = preco;
        PrecoPromo = precoPromo;
        CategoriaId = categoriaId;
        Marca = marca;
        Volume = volume;
        Genero = genero;
        Destaque = destaque;
        MarcarAtualizado();
    }

    public bool DebitarEstoque(int quantidade)
    {
        if (Estoque < quantidade) return false;
        Estoque -= quantidade;
        MarcarAtualizado();
        return true;
    }

    public void CreditarEstoque(int quantidade)
    {
        Estoque += quantidade;
        MarcarAtualizado();
    }

    public void AdicionarImagem(ProdutoImagem imagem) => _imagens.Add(imagem);

    public void Ativar() { Ativo = true; MarcarAtualizado(); }
    public void Desativar() { Ativo = false; MarcarAtualizado(); }
}
