using Glamour.Domain.Enums;

namespace Glamour.Domain.Entities;

public class Pedido : BaseEntity
{
    public string UsuarioId { get; private set; } = string.Empty;
    public StatusPedido Status { get; private set; } = StatusPedido.Pendente;
    public decimal Subtotal { get; private set; }
    public decimal Frete { get; private set; }
    public decimal Desconto { get; private set; }
    public decimal Total { get; private set; }
    public TipoEntrega TipoEntrega { get; private set; }
    public Guid? EnderecoId { get; private set; }
    public Endereco? Endereco { get; private set; }
    public MetodoPagamento MetodoPagamento { get; private set; }
    public string? CupomCodigo { get; private set; }
    public string? CodigoRastreio { get; private set; }
    public string? Observacoes { get; private set; }
    public OrigemPedido Origem { get; private set; } = OrigemPedido.Site;
    public string? NomeCliente { get; private set; }

    private readonly List<PedidoItem> _itens = [];
    public IReadOnlyCollection<PedidoItem> Itens => _itens.AsReadOnly();

    public Pagamento? Pagamento { get; private set; }

    protected Pedido() { }

    public Pedido(string usuarioId, TipoEntrega tipoEntrega, MetodoPagamento metodoPagamento,
        Guid? enderecoId, decimal frete, string? cupomCodigo = null, string? observacoes = null)
    {
        UsuarioId = usuarioId;
        TipoEntrega = tipoEntrega;
        MetodoPagamento = metodoPagamento;
        EnderecoId = enderecoId;
        Frete = tipoEntrega == TipoEntrega.RetiradaNaLoja ? 0 : frete;
        CupomCodigo = cupomCodigo;
        Observacoes = observacoes;
    }

    public void AdicionarItem(PedidoItem item)
    {
        _itens.Add(item);
        RecalcularTotal();
    }

    public void AplicarDesconto(decimal desconto)
    {
        Desconto = desconto;
        RecalcularTotal();
    }

    public void DefinirFrete(decimal frete)
    {
        Frete = TipoEntrega == TipoEntrega.RetiradaNaLoja ? 0 : frete;
        RecalcularTotal();
    }

    private void RecalcularTotal()
    {
        Subtotal = _itens.Sum(i => i.Subtotal);
        Total = Subtotal + Frete - Desconto;
        if (Total < 0) Total = 0;
        MarcarAtualizado();
    }

    public void AtualizarStatus(StatusPedido status)
    {
        Status = status;
        MarcarAtualizado();
    }

    public void DefinirRastreio(string codigo)
    {
        CodigoRastreio = codigo;
        Status = StatusPedido.Enviado;
        MarcarAtualizado();
    }

    public void Cancelar()
    {
        if (Status == StatusPedido.Entregue)
            throw new InvalidOperationException("Pedido já entregue não pode ser cancelado.");
        Status = StatusPedido.Cancelado;
        MarcarAtualizado();
    }

    public void RegistrarVendaLoja(string? nomeCliente)
    {
        Origem = OrigemPedido.Loja;
        NomeCliente = string.IsNullOrWhiteSpace(nomeCliente) ? null : nomeCliente.Trim();
        Status = StatusPedido.Entregue;
        MarcarAtualizado();
    }
}
