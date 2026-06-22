using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Data;

public class ApplicationDbContext : IdentityDbContext<User, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public ApplicationDbContext() { }
    
    public DbSet<Car> Cars { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Banner> Banners { get; set; } 
    public DbSet<Chat> Chats { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<FaqItem> Faqs { get; set; }
    public DbSet<Page> Pages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    
        // Конфигурации
        builder.ApplyConfiguration(new Configurations.UserConfiguration());
        builder.ApplyConfiguration(new Configurations.CarConfiguration());
        builder.ApplyConfiguration(new Configurations.BookingConfiguration());
        builder.ApplyConfiguration(new Configurations.DocumentConfiguration());
        builder.ApplyConfiguration(new Configurations.BannerConfiguration()); // <-- Добавляем конфигурацию
        builder.ApplyConfiguration(new Configurations.ChatConfiguration());
        builder.ApplyConfiguration(new Configurations.ChatMessageConfiguration());
        builder.ApplyConfiguration(new Configurations.PageConfiguration());
        builder.ApplyConfiguration(new Configurations.CarImageConfiguration());
        // Каскадное удаления мусора чата
        builder.Entity<Chat>()
            .HasMany(c => c.Messages)
            .WithOne(m => m.Chat)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
        // FAQ
        builder.Entity<FaqItem>(entity =>
        {
            entity.Property(f => f.Question).IsRequired().HasMaxLength(500);
            entity.Property(f => f.Answer).IsRequired();
            entity.Property(f => f.Category).HasMaxLength(100);
            entity.HasIndex(f => f.IsActive);
            entity.HasIndex(f => f.Category);
        });
    }
}