using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarRental.Domain.Entities;

namespace CarRental.DAL.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.HasKey(cm => cm.Id);

        // Внешние ключи
        builder.HasOne(cm => cm.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(cm => cm.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.SenderUser)
            .WithMany()
            .HasForeignKey(cm => cm.SenderUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(cm => cm.SenderAdmin)
            .WithMany()
            .HasForeignKey(cm => cm.SenderAdminId)
            .OnDelete(DeleteBehavior.SetNull);

        // Свойства
        builder.Property(cm => cm.Message)
            .IsRequired();

        builder.Property(cm => cm.MessageType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(cm => cm.AttachmentUrl)
            .HasMaxLength(500);

        builder.Property(cm => cm.AttachmentType)
            .HasMaxLength(50);

        // Индексы
        builder.HasIndex(cm => cm.ChatId);
        builder.HasIndex(cm => cm.MessageType);
        builder.HasIndex(cm => cm.IsRead);
        builder.HasIndex(cm => cm.CreatedAt);
    }
}