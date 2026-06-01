namespace Glamour.Application.DTOs;

public record EnderecoDto(
    Guid Id, string CEP, string Logradouro, string Numero,
    string? Complemento, string Bairro, string Cidade, string UF, string? Apelido, bool Principal);

public record CriarEnderecoDto(
    string CEP, string Logradouro, string Numero,
    string? Complemento, string Bairro, string Cidade, string UF, string? Apelido);
