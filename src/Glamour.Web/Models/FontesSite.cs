namespace Glamour.Web.Models;

public static class FontesSite
{
    public const string Padrao = "Playfair Display";

    public static readonly IReadOnlyDictionary<string, string> Disponiveis = new Dictionary<string, string>
    {
        ["Playfair Display"] = "Elegante e clássica (padrão)",
        ["Cormorant Garamond"] = "Refinada, ar de luxo",
        ["Cinzel"] = "Maiúsculas, estilo grife",
        ["Bodoni Moda"] = "Alto contraste, editorial",
        ["Montserrat"] = "Moderna e limpa"
    };

    public static string Validar(string? fonte) =>
        !string.IsNullOrWhiteSpace(fonte) && Disponiveis.ContainsKey(fonte) ? fonte : Padrao;

    public static string GoogleFontsParam(string fonte) =>
        Uri.EscapeDataString(Validar(fonte)).Replace("%20", "+").Replace(" ", "+");
}
