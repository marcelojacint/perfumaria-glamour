using Glamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Glamour.Infrastructure.Data.Configurations;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produtos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Slug).IsRequired().HasMaxLength(220);
        builder.Property(p => p.Descricao).IsRequired().HasMaxLength(4000);
        builder.Property(p => p.Preco).HasPrecision(18, 2);
        builder.Property(p => p.PrecoPromo).HasPrecision(18, 2);
        builder.Property(p => p.Marca).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Volume).HasMaxLength(50);
        builder.Property(p => p.Genero).HasMaxLength(30);

        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => new { p.CategoriaId, p.Ativo });

        builder.HasMany(p => p.Imagens).WithOne(i => i.Produto).HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Avaliacoes).WithOne(a => a.Produto).HasForeignKey(a => a.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
