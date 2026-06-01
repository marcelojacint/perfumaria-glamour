namespace Glamour.Domain.Entities;

public class Categoria : BaseEntity
{
    public string Nome { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public bool Ativo { get; private set; } = true;
    public int Ordem { get; private set; }

    private readonly List<Produto> _produtos = [];
    public IReadOnlyCollection<Produto> Produtos => _produtos.AsReadOnly();

    protected Categoria() { }

    public Categoria(string nome, string slug, int ordem = 0)
    {
        Nome = nome;
        Slug = slug.ToLowerInvariant();
        Ordem = ordem;
    }

    public void Atualizar(string nome, string slug, int ordem)
    {
        Nome = nome;
        Slug = slug.ToLowerInvariant();
        Ordem = ordem;
        MarcarAtualizado();
    }

    public void Ativar() { Ativo = true; MarcarAtualizado(); }
    public void Desativar() { Ativo = false; MarcarAtualizado(); }
}
