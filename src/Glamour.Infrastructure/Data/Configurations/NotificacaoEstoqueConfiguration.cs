using Glamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Glamour.Infrastructure.Data.Configurations;

public class NotificacaoEstoqueConfiguration : IEntityTypeConfiguration<NotificacaoEstoque>
{
    public void Configure(EntityTypeBuilder<NotificacaoEstoque> builder)
    {
        builder.ToTable("notificacoes_estoque");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(n => new { n.ProdutoId, n.Email }).IsUnique();
    }
}
