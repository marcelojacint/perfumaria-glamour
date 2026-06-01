using FluentValidation;
using Glamour.Application.DTOs;

namespace Glamour.Application.Validators;

public class RegistrarUsuarioValidator : AbstractValidator<RegistrarUsuarioDto>
{
    public RegistrarUsuarioValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome é obrigatório.").MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("E-mail inválido.");
        RuleFor(x => x.Senha).NotEmpty().MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres.");
        RuleFor(x => x.ConfirmacaoSenha).Equal(x => x.Senha).WithMessage("As senhas não coincidem.");
        RuleFor(x => x.Telefone).Matches(@"^\(?\d{2}\)?\s?\d{4,5}-?\d{4}$").When(x => !string.IsNullOrEmpty(x.Telefone)).WithMessage("Telefone inválido.");
    }
}
