namespace Glamour.Domain.Entities;

public class Campanha : BaseEntity
{
    public string Titulo { get; private set; } = string.Empty;
    public string? Subtitulo { get; private set; }
    public string? ImagemUrl { get; private set; }
    public string Link { get; private set; } = "/catalogo";
    public int Ordem { get; private set; }
    public bool Ativa { get; private set; } = true;

    protected Campanha() { }

    public Campanha(string titulo, string? subtitulo, string? link, int ordem)
    {
        Titulo = titulo?.Trim() ?? "";
        Subtitulo = string.IsNullOrWhiteSpace(subtitulo) ? null : subtitulo.Trim();
        Link = string.IsNullOrWhiteSpace(link) ? "/catalogo" : link.Trim();
        Ordem = ordem;
        Ativa = true;
    }

    public void Atualizar(string titulo, string? subtitulo, string? link, int ordem, bool ativa)
    {
        Titulo = titulo?.Trim() ?? "";
        Subtitulo = string.IsNullOrWhiteSpace(subtitulo) ? null : subtitulo.Trim();
        Link = string.IsNullOrWhiteSpace(link) ? "/catalogo" : link.Trim();
        Ordem = ordem;
        Ativa = ativa;
        MarcarAtualizado();
    }

    public void DefinirImagem(string? url)
    {
        ImagemUrl = string.IsNullOrWhiteSpace(url) ? null : url.Trim();
        MarcarAtualizado();
    }
}
