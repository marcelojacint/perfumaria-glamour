namespace Glamour.Domain.Entities;

public class ConfiguracaoHero : BaseEntity
{
    public static readonly Guid IdSingleton = new("11111111-1111-1111-1111-111111111111");

    public const string EyebrowPadrao = "Nova Coleção 2026";
    public const string TituloPadrao = "A Essência da";
    public const string TituloDestaquePadrao = "Elegância";
    public const string SubtituloPadrao = "Perfumes exclusivos e acessórios sofisticados para quem valoriza o refinamento.";
    public const string CorDestaquePadrao = "#D4AF37";
    public const string CorTextoPadrao = "#FFFFFF";
    public const string FonteTituloPadrao = "Playfair Display";

    public string Eyebrow { get; private set; } = EyebrowPadrao;
    public string Titulo { get; private set; } = TituloPadrao;
    public string TituloDestaque { get; private set; } = TituloDestaquePadrao;
    public string Subtitulo { get; private set; } = SubtituloPadrao;
    public string CorDestaque { get; private set; } = CorDestaquePadrao;
    public string CorTexto { get; private set; } = CorTextoPadrao;
    public string FonteTitulo { get; private set; } = FonteTituloPadrao;
    public string? ImagemFundoUrl { get; private set; }

    protected ConfiguracaoHero() { }

    public static ConfiguracaoHero Padrao()
    {
        var configuracao = new ConfiguracaoHero();
        configuracao.Id = IdSingleton;
        return configuracao;
    }

    public void Atualizar(string? eyebrow, string? titulo, string? tituloDestaque,
        string? subtitulo, string? corDestaque, string? corTexto)
    {
        Eyebrow = eyebrow?.Trim() ?? "";
        Titulo = titulo?.Trim() ?? "";
        TituloDestaque = tituloDestaque?.Trim() ?? "";
        Subtitulo = subtitulo?.Trim() ?? "";
        CorDestaque = string.IsNullOrWhiteSpace(corDestaque) ? CorDestaquePadrao : corDestaque.Trim();
        CorTexto = string.IsNullOrWhiteSpace(corTexto) ? CorTextoPadrao : corTexto.Trim();
        MarcarAtualizado();
    }

    public void DefinirImagem(string? url)
    {
        ImagemFundoUrl = string.IsNullOrWhiteSpace(url) ? null : url.Trim();
        MarcarAtualizado();
    }

    public void DefinirFonte(string? fonte)
    {
        FonteTitulo = string.IsNullOrWhiteSpace(fonte) ? FonteTituloPadrao : fonte.Trim();
        MarcarAtualizado();
    }

    public void RestaurarPadrao()
    {
        Eyebrow = EyebrowPadrao;
        Titulo = TituloPadrao;
        TituloDestaque = TituloDestaquePadrao;
        Subtitulo = SubtituloPadrao;
        CorDestaque = CorDestaquePadrao;
        CorTexto = CorTextoPadrao;
        FonteTitulo = FonteTituloPadrao;
        ImagemFundoUrl = null;
        MarcarAtualizado();
    }
}
