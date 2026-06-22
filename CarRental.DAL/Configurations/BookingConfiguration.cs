using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;

namespace CarRental.DAL.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.Property(b => b.TotalPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.Notes)
            .HasMaxLength(500);

        // Связи
        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Car)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.CarId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
