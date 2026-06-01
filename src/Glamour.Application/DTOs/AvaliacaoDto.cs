namespace Glamour.Application.DTOs;

public record AvaliacaoDto(
    Guid Id, Guid ProdutoId, string NomeUsuario,
    int Nota, string? Comentario, bool Aprovada, DateTime CriadoEm);

public record CriarAvaliacaoDto(Guid ProdutoId, int Nota, string? Comentario);
