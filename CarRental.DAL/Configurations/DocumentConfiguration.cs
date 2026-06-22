using CarRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarRental.DAL.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.FileName).IsRequired().HasMaxLength(255);
        builder.Property(d => d.FilePath).IsRequired().HasMaxLength(500);
        
        builder.Property(d => d.FileName2).HasMaxLength(255);
        builder.Property(d => d.FilePath2).HasMaxLength(500);
        
        builder.Property(d => d.DocumentType).IsRequired();
        builder.Property(d => d.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
        builder.Property(d => d.Description).HasMaxLength(500);
        builder.Property(d => d.VerifiedAt).IsRequired(false);
        
        builder.Property(d => d.DocumentNumber).HasMaxLength(50);
        builder.Property(d => d.IssuedBy).HasMaxLength(200);
        builder.Property(d => d.BirthDate).HasMaxLength(20);
        builder.Property(d => d.PlaceOfBirth).HasMaxLength(200);
        builder.Property(d => d.RegistrationAddress).HasMaxLength(500);
        
        builder.HasIndex(d => d.DocumentNumber);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.UserId);

        builder.HasOne(d => d.User)
            .WithMany(u => u.Documents)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}