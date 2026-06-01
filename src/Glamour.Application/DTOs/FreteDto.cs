namespace Glamour.Application.DTOs;

public record ResultadoFrete(
    bool DentroDaArea,
    bool Gratis,
    decimal Valor,
    string Mensagem);
