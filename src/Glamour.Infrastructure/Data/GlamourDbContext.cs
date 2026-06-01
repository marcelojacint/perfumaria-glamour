using Glamour.Domain.Entities;
using Glamour.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Glamour.Infrastructure.Data;

public class GlamourDbContext : IdentityDbContext<ApplicationUser>
{
    public GlamourDbContext(DbContextOptions<GlamourDbContext> options) : base(options) { }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<ProdutoImagem> ProdutoImagens => Set<ProdutoImagem>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
    public DbSet<Cupom> Cupons => Set<Cupom>();
    public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();
    public DbSet<NotificacaoEstoque> NotificacoesEstoque => Set<NotificacaoEstoque>();
    public DbSet<Newsletter> Newsletters => Set<Newsletter>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(GlamourDbContext).Assembly);

        // Renomeia tabelas Identity para snake_case / português
        builder.Entity<ApplicationUser>().ToTable("usuarios");
    }
}
