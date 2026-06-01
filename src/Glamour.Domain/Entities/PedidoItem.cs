namespace Glamour.Domain.Entities;

public class PedidoItem : BaseEntity
{
    public Guid PedidoId { get; private set; }
    public Pedido Pedido { get; private set; } = null!;
    public Guid ProdutoId { get; private set; }
    public Produto Produto { get; private set; } = null!;
    public string NomeProduto { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public decimal Subtotal => Quantidade * PrecoUnitario;

    protected PedidoItem() { }

    public PedidoItem(Guid pedidoId, Guid produtoId, string nomeProduto, int quantidade, decimal precoUnitario)
    {
        PedidoId = pedidoId;
        ProdutoId = produtoId;
        NomeProduto = nomeProduto;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
    }
}
