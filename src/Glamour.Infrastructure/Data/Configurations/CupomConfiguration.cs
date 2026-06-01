using Glamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Glamour.Infrastructure.Data.Configurations;

public class CupomConfiguration : IEntityTypeConfiguration<Cupom>
{
    public void Configure(EntityTypeBuilder<Cupom> builder)
    {
        builder.ToTable("cupons");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Codigo).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Valor).HasPrecision(18, 2);
        builder.Property(c => c.ValorMinimoPedido).HasPrecision(18, 2);
        builder.Property(c => c.TipoDesconto).HasConversion<string>();
        builder.HasIndex(c => c.Codigo).IsUnique();
    }
}
