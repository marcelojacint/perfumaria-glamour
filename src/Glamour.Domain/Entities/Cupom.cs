using Glamour.Domain.Enums;

namespace Glamour.Domain.Entities;

public class Cupom : BaseEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public TipoDesconto TipoDesconto { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime? Validade { get; private set; }
    public int UsoMaximo { get; private set; }
    public int UsoAtual { get; private set; }
    public decimal? ValorMinimoPedido { get; private set; }
    public bool Ativo { get; private set; } = true;

    public bool Valido => Ativo
        && (Validade == null || Validade > DateTime.UtcNow)
        && (UsoMaximo == 0 || UsoAtual < UsoMaximo);

    protected Cupom() { }

    public Cupom(string codigo, TipoDesconto tipo, decimal valor,
        DateTime? validade, int usoMaximo, decimal? valorMinimoPedido = null)
    {
        Codigo = codigo.ToUpper();
        TipoDesconto = tipo;
        Valor = valor;
        Validade = validade;
        UsoMaximo = usoMaximo;
        ValorMinimoPedido = valorMinimoPedido;
    }

    public decimal CalcularDesconto(decimal totalPedido)
    {
        if (!Valido) return 0;
        if (ValorMinimoPedido.HasValue && totalPedido < ValorMinimoPedido) return 0;

        return TipoDesconto == TipoDesconto.Percentual
            ? Math.Round(totalPedido * (Valor / 100), 2)
            : Math.Min(Valor, totalPedido);
    }

    public void IncrementarUso()
    {
        UsoAtual++;
        MarcarAtualizado();
    }

    public void Desativar() { Ativo = false; MarcarAtualizado(); }
}
