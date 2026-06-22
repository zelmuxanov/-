using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;

namespace CarRental.DAL.Configurations;

public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.Property(c => c.Brand)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Model)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Color)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(c => c.PricePerDay)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.HasMany(c => c.Images)
            .WithOne(i => i.Car)
            .HasForeignKey(i => i.CarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.PricePerDay15).HasColumnType("decimal(18,2)");
        builder.Property(c => c.PricePerDay30).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Deposit).HasColumnType("decimal(18,2)");
        builder.Property(c => c.MileageLimitPerDay).HasDefaultValue(250);
        builder.Property(c => c.OverMileagePricePerKm).HasColumnType("decimal(18,2)");
        builder.Property(c => c.UnlimitedMileagePrice).HasColumnType("decimal(18,2)");
    }
}
