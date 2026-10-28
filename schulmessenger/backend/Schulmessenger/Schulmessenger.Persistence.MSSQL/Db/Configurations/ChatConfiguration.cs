using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Persistence.MSSQL.Db.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        builder.ToTable("Chat");
        builder.HasKey(c => c.ChatId);
        builder.Property(c => c.ChatId).ValueGeneratedOnAdd();
        builder.Property(c => c.Name).HasMaxLength(50);
        builder.Property(c => c.Description).HasMaxLength(250);

        builder
            .HasOne(c => c.LastMessage)
            .WithMany()
            .HasForeignKey(c => c.LastMessageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}