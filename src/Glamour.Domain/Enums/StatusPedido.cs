namespace Glamour.Domain.Enums;

public enum StatusPedido
{
    Pendente = 0,
    AguardandoPagamento = 1,
    PagamentoAprovado = 2,
    EmPreparacao = 3,
    Enviado = 4,
    Entregue = 5,
    Cancelado = 6,
    Estornado = 7
}
