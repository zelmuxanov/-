using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        builder.ToTable("Chats");

        builder.HasKey(c => c.Id);

        // Пользователь (опционально - может быть null для гостей)
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Свойства
        builder.Property(c => c.Topic)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.TempUserId)
            .IsRequired(false);

        builder.Property(c => c.TempUserName)
            .HasMaxLength(100);

        builder.Property(c => c.TempUserEmail)
            .HasMaxLength(100);

        builder.Property(c => c.TempUserPhone)
            .HasMaxLength(20);

        builder.HasMany(c => c.Messages)
                   .WithOne(m => m.Chat)
                   .HasForeignKey(m => m.ChatId)
                   .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.TempUserId);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.LastMessageAt);
        builder.HasIndex(c => c.TempUserExpiry); // Для очистки просроченных чатов
    }
}