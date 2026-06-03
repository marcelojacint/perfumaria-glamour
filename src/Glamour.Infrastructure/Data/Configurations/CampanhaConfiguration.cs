using Glamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Glamour.Infrastructure.Data.Configurations;

public class CampanhaConfiguration : IEntityTypeConfiguration<Campanha>
{
    public void Configure(EntityTypeBuilder<Campanha> builder)
    {
        builder.ToTable("campanhas");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Titulo).IsRequired().HasMaxLength(120);
        builder.Property(c => c.Subtitulo).HasMaxLength(200);
        builder.Property(c => c.ImagemUrl).HasMaxLength(400);
        builder.Property(c => c.Link).IsRequired().HasMaxLength(300);
        builder.HasIndex(c => new { c.Ativa, c.Ordem });
    }
}
