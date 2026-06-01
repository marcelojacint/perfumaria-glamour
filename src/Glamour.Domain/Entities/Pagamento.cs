using Glamour.Domain.Enums;

namespace Glamour.Domain.Entities;

public class Pagamento : BaseEntity
{
    public Guid PedidoId { get; private set; }
    public Pedido Pedido { get; private set; } = null!;
    public MetodoPagamento Metodo { get; private set; }
    public StatusPagamento Status { get; private set; } = StatusPagamento.Pendente;
    public string? TransacaoExternaId { get; private set; }
    public decimal Valor { get; private set; }
    public string? QrCodePix { get; private set; }
    public DateTime? ProcessadoEm { get; private set; }

    protected Pagamento() { }

    public Pagamento(Guid pedidoId, MetodoPagamento metodo, decimal valor)
    {
        PedidoId = pedidoId;
        Metodo = metodo;
        Valor = valor;
    }

    public void Confirmar(string transacaoExternaId)
    {
        Status = StatusPagamento.Aprovado;
        TransacaoExternaId = transacaoExternaId;
        ProcessadoEm = DateTime.UtcNow;
        MarcarAtualizado();
    }

    public void Recusar()
    {
        Status = StatusPagamento.Recusado;
        ProcessadoEm = DateTime.UtcNow;
        MarcarAtualizado();
    }

    public void Estornar()
    {
        Status = StatusPagamento.Estornado;
        MarcarAtualizado();
    }

    public void DefinirQrCodePix(string qrCode)
    {
        QrCodePix = qrCode;
        MarcarAtualizado();
    }
}
