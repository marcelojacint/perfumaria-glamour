namespace Glamour.Domain.Entities;

public class ProdutoImagem : BaseEntity
{
    public Guid ProdutoId { get; private set; }
    public Produto Produto { get; private set; } = null!;
    public string Url { get; private set; } = string.Empty;
    public int Ordem { get; private set; }
    public bool Principal { get; private set; }

    protected ProdutoImagem() { }

    public ProdutoImagem(Guid produtoId, string url, int ordem, bool principal = false)
    {
        ProdutoId = produtoId;
        Url = url;
        Ordem = ordem;
        Principal = principal;
    }

    public void MarcarPrincipal() => Principal = true;
    public void RemoverPrincipal() => Principal = false;
}
