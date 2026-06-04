using Glamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Glamour.Infrastructure.Data.Configurations;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("pedidos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.UsuarioId).IsRequired().HasMaxLength(450);
        builder.Property(p => p.Subtotal).HasPrecision(18, 2);
        builder.Property(p => p.Frete).HasPrecision(18, 2);
        builder.Property(p => p.Desconto).HasPrecision(18, 2);
        builder.Property(p => p.Total).HasPrecision(18, 2);
        builder.Property(p => p.CupomCodigo).HasMaxLength(50);
        builder.Property(p => p.CodigoRastreio).HasMaxLength(50);
        builder.Property(p => p.Status).HasConversion<string>();
        builder.Property(p => p.Origem).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.NomeCliente).HasMaxLength(120);

        builder.HasIndex(p => new { p.UsuarioId, p.CriadoEm });

        builder.HasMany(p => p.Itens).WithOne(i => i.Pedido).HasForeignKey(i => i.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Pagamento).WithOne(pg => pg.Pedido).HasForeignKey<Pagamento>(pg => pg.PedidoId);
        builder.Property(p => p.TipoEntrega).HasConversion<string>();
        builder.Property(p => p.MetodoPagamento).HasConversion<string>();
        builder.Property(p => p.MetodoPagamentoPromocao).HasConversion<string>();
        builder.Property(p => p.ValorEmPromocao).HasPrecision(18, 2);

        builder.HasOne(p => p.Endereco).WithMany().HasForeignKey(p => p.EnderecoId)
            .OnDelete(DeleteBehavior.Restrict).IsRequired(false);
    }
}
