namespace Glamour.Application.DTOs;

/// <summary>
/// Resultado do cálculo de frete para um endereço de entrega.
/// </summary>
public record ResultadoFrete(
    bool DentroDaArea,   // está na área de entrega atendida automaticamente
    bool Gratis,         // frete grátis (atingiu o valor mínimo)
    decimal Valor,       // valor do frete (0 se grátis)
    string Mensagem);    // mensagem amigável ao cliente
