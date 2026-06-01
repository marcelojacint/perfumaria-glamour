using FluentValidation;
using Glamour.Application.DTOs;

namespace Glamour.Application.Validators;

public class CriarProdutoValidator : AbstractValidator<CriarProdutoDto>
{
    public CriarProdutoValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200).WithMessage("Nome é obrigatório (máx. 200 caracteres).");
        RuleFor(x => x.Slug).NotEmpty().Matches(@"^[a-z0-9\-]+$").WithMessage("Slug deve conter apenas letras minúsculas, números e hífens.");
        RuleFor(x => x.Preco).GreaterThan(0).WithMessage("Preço deve ser maior que zero.");
        RuleFor(x => x.PrecoPromo).LessThan(x => x.Preco).When(x => x.PrecoPromo.HasValue).WithMessage("Preço promocional deve ser menor que o preço normal.");
        RuleFor(x => x.Estoque).GreaterThanOrEqualTo(0).WithMessage("Estoque não pode ser negativo.");
        RuleFor(x => x.CategoriaId).NotEmpty().WithMessage("Selecione uma categoria.");
        RuleFor(x => x.Marca).NotEmpty().MaximumLength(100);
    }
}
