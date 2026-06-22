using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Configurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.Property(p => p.Content)
            .IsRequired();

        builder.Property(p => p.MetaDescription)
            .HasMaxLength(300);

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.Property(p => p.DisplayOrder)
            .HasDefaultValue(0);

        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.Slug);

        builder.ToTable("Pages");
    }
}