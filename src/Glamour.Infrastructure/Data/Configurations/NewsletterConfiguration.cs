using Glamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Glamour.Infrastructure.Data.Configurations;

public class NewsletterConfiguration : IEntityTypeConfiguration<Newsletter>
{
    public void Configure(EntityTypeBuilder<Newsletter> builder)
    {
        builder.ToTable("newsletters");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(n => n.Email).IsUnique();
    }
}
