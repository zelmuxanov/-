using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;

namespace CarRental.DAL.Configurations;

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Title)
            .HasMaxLength(200);
            
        builder.Property(b => b.Description)
            .HasMaxLength(500);
            
        builder.Property(b => b.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(b => b.Link)
            .HasMaxLength(200);
            
        builder.Property(b => b.ButtonText)
            .HasMaxLength(50);
            
        builder.Property(b => b.BannerType)
            .HasConversion<int>()
            .HasDefaultValue(BannerType.MainCarousel)
            .HasSentinel((BannerType)(-1)) // <-- ДОБАВЛЯЕМ SENTINEL VALUE
            .IsRequired();
            
        builder.Property(b => b.IsActive)
            .HasDefaultValue(true);
            
        builder.Property(b => b.DisplayOrder)
            .HasDefaultValue(0);
            
        // Индексы для оптимизации запросов
        builder.HasIndex(b => b.IsActive);
        builder.HasIndex(b => b.DisplayOrder);
        builder.HasIndex(b => b.BannerType);
        builder.HasIndex(b => new { b.IsActive, b.BannerType, b.DisplayOrder });
        
        builder.Property(b => b.MediaType)
            .HasConversion<int>()
            .HasDefaultValue(BannerMediaType.Image);

        builder.Property(b => b.VideoUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(b => b.ObjectFit).HasMaxLength(20).HasDefaultValue("cover");
        builder.Property(b => b.ObjectPosition).HasMaxLength(50).HasDefaultValue("center");

        //Движуха для гибкой настройки видео
        builder.Property(b => b.VideoAutoplay)
            .HasDefaultValue(true);
        builder.Property(b => b.VideoMuted)
            .HasDefaultValue(true);
        builder.Property(b => b.VideoLoop)
            .HasDefaultValue(false);
        builder.Property(b => b.VideoControls)
            .HasDefaultValue(false);
        
        builder.ToTable("Banners");
    }
}