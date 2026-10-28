using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Persistence.MSSQL.Db.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Message");
        builder.HasKey(u => u.MessageId);
        builder.Property(u => u.MessageId).ValueGeneratedOnAdd();

        builder.Property(u => u.Content).HasMaxLength(4000).IsRequired();
        
        builder
            .HasOne(u => u.Sender)
            .WithMany(u => u.Messages)
            .HasForeignKey(u => u.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder
            .HasOne(u => u.Chat)
            .WithMany(u => u.Messages)
            .HasForeignKey(u => u.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}