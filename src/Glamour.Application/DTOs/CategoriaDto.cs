namespace Glamour.Application.DTOs;

public record CategoriaDto(Guid Id, string Nome, string Slug, bool Ativo, int Ordem);

public record CriarCategoriaDto(string Nome, string Slug, int Ordem);
public record AtualizarCategoriaDto(Guid Id, string Nome, string Slug, int Ordem);
