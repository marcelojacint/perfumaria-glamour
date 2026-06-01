namespace Glamour.Application.DTOs;

public record UsuarioDto(string Id, string Nome, string Email, string? Telefone, bool Ativo, DateTime CriadoEm);

public record RegistrarUsuarioDto(string Nome, string Email, string Senha, string ConfirmacaoSenha, string? Telefone);

public record LoginDto(string Email, string Senha, bool LembrarMe);

public record AlterarSenhaDto(string SenhaAtual, string NovaSenha, string ConfirmacaoNovaSenha);
