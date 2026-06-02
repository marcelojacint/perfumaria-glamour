using Glamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Glamour.Infrastructure.Data.Configurations;

public class ConfiguracaoHeroConfiguration : IEntityTypeConfiguration<ConfiguracaoHero>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoHero> builder)
    {
        builder.ToTable("configuracao_hero");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Eyebrow).HasMaxLength(120);
        builder.Property(c => c.Titulo).HasMaxLength(160);
        builder.Property(c => c.TituloDestaque).HasMaxLength(120);
        builder.Property(c => c.Subtitulo).HasMaxLength(400);
        builder.Property(c => c.CorDestaque).HasMaxLength(20);
        builder.Property(c => c.CorTexto).HasMaxLength(20);
        builder.Property(c => c.ImagemFundoUrl).HasMaxLength(400);
    }
}
