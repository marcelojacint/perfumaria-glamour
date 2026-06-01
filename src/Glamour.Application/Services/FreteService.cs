using System.Globalization;
using System.Text;
using Glamour.Application.DTOs;
using Microsoft.Extensions.Configuration;

namespace Glamour.Application.Services;

public class FreteService(IConfiguration config)
{
    private string CidadeAtendida => config["Frete:CidadeAtendida"] ?? "Cacimba de Dentro";
    private string UfAtendida => config["Frete:UfAtendida"] ?? "PB";
    private decimal ValorMinimoGratis => decimal.TryParse(config["Frete:ValorMinimoFreteGratis"], NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 150m;
    private decimal TaxaLocal => decimal.TryParse(config["Frete:TaxaEntregaLocal"], NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 4.99m;

    public ResultadoFrete Calcular(string? cidade, string? uf, decimal subtotal)
    {
        var dentro = MesmaLocalidade(cidade, uf);

        if (!dentro)
        {
            return new ResultadoFrete(
                DentroDaArea: false,
                Gratis: false,
                Valor: 0,
                Mensagem: "Para a sua região, entre em contato com a loja para combinar a entrega. Você também pode optar por retirar na loja.");
        }

        if (subtotal >= ValorMinimoGratis)
        {
            return new ResultadoFrete(true, true, 0,
                $"Frete grátis para {CidadeAtendida}/{UfAtendida}!");
        }

        var faltam = ValorMinimoGratis - subtotal;
        return new ResultadoFrete(true, false, TaxaLocal,
            $"Entrega em {CidadeAtendida}/{UfAtendida}: R$ {TaxaLocal.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"))}. " +
            $"Faltam R$ {faltam.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"))} para frete grátis.");
    }

    public bool MesmaLocalidade(string? cidade, string? uf) =>
        Normalizar(cidade) == Normalizar(CidadeAtendida) &&
        (uf ?? "").Trim().ToUpperInvariant() == UfAtendida.ToUpperInvariant();

    private static string Normalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";
        var semAcento = new string(texto.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());
        return semAcento.Trim().ToLowerInvariant();
    }
}
