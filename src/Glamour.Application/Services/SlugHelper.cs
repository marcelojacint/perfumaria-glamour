using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Glamour.Application.Services;

public static partial class SlugHelper
{
    public static string Gerar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";

        var semAcento = new string(texto.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());

        var slug = NaoAlfaNumerico().Replace(semAcento.ToLowerInvariant(), "-").Trim('-');
        return slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NaoAlfaNumerico();
}
