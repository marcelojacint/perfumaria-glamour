using Glamour.Domain.Interfaces;
using Glamour.Infrastructure.Data;
using Glamour.Infrastructure.Identity;
using Glamour.Infrastructure.Repositories;
using Glamour.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Glamour.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<GlamourDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(GlamourDbContext).Assembly.FullName)));

        services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
        {
            opt.Password.RequireDigit = true;
            opt.Password.RequiredLength = 8;
            opt.Password.RequireUppercase = false;
            opt.Password.RequireNonAlphanumeric = false;
            opt.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<GlamourDbContext>()
        .AddDefaultTokenProviders();

        var redisConnection = config.GetConnectionString("Redis") ?? "localhost:6379";
        var redisOptions = ConfigurationOptions.Parse(redisConnection);
        redisOptions.AbortOnConnectFail = false;
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions));

        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<ICupomRepository, CupomRepository>();
        services.AddScoped<ICarrinhoService, CarrinhoRedisService>();
        services.AddScoped<IWishlistService, WishlistRedisService>();
        services.AddScoped<IFidelidadeService, FidelidadeService>();
        services.AddHttpClient<IWhatsAppService, CallMeBotWhatsAppService>();
        services.AddScoped<IRepository<Glamour.Domain.Entities.Endereco>, BaseRepository<Glamour.Domain.Entities.Endereco>>();
        services.AddScoped<IRepository<Glamour.Domain.Entities.Avaliacao>, BaseRepository<Glamour.Domain.Entities.Avaliacao>>();
        services.AddScoped<IRepository<Glamour.Domain.Entities.NotificacaoEstoque>, BaseRepository<Glamour.Domain.Entities.NotificacaoEstoque>>();
        services.AddScoped<IRepository<Glamour.Domain.Entities.Newsletter>, BaseRepository<Glamour.Domain.Entities.Newsletter>>();
        services.AddScoped<IRepository<Glamour.Domain.Entities.ProdutoImagem>, BaseRepository<Glamour.Domain.Entities.ProdutoImagem>>();
        services.AddScoped<IRepository<Glamour.Domain.Entities.ConfiguracaoHero>, BaseRepository<Glamour.Domain.Entities.ConfiguracaoHero>>();
        services.AddScoped<IRepository<Glamour.Domain.Entities.Campanha>, BaseRepository<Glamour.Domain.Entities.Campanha>>();

        return services;
    }
}
