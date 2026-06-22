using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Configurations;

public class CarImageConfiguration : IEntityTypeConfiguration<CarImage>
{
    public void Configure(EntityTypeBuilder<CarImage> builder)
    {
        builder.Property(i => i.ImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(i => i.FileName).HasMaxLength(255);
        builder.Property(i => i.DisplayOrder).HasDefaultValue(0);
        builder.Property(i => i.IsMain).HasDefaultValue(false);
        
        builder.HasIndex(i => new { i.CarId, i.DisplayOrder });
        builder.HasIndex(i => i.IsMain);
    }
}