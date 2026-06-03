namespace Glamour.Application.DTOs;

public record CampanhaDto(
    Guid Id,
    string Titulo,
    string? Subtitulo,
    string? ImagemUrl,
    string Link,
    int Ordem,
    bool Ativa);
