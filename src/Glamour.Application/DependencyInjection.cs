using FluentValidation;
using Glamour.Application.Services;
using Glamour.Application.Validators;
using Glamour.Domain.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace Glamour.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegistrarUsuarioValidator>();
        services.AddScoped<NotificacaoContext>();
        services.AddScoped<ProdutoService>();
        services.AddScoped<CategoriaService>();
        services.AddScoped<PedidoService>();
        services.AddScoped<EnderecoService>();
        services.AddScoped<AvaliacaoService>();
        services.AddScoped<FreteService>();
        return services;
    }
}
