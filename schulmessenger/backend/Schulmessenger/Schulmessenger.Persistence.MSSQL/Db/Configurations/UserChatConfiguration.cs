using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Persistence.MSSQL.Db.Configurations;

public class UserChatConfiguration: IEntityTypeConfiguration<UserChat>
{
    public void Configure(EntityTypeBuilder<UserChat> builder)
    {
        builder.ToTable("UserChat");
        builder.HasKey(u => new { u.UserId, u.ChatId });
        
        builder
            .HasOne(uc => uc.Chat)
            .WithMany(c => c.UserChats)
            .HasForeignKey(uc => uc.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasOne(uc => uc.User)
            .WithMany(u => u.UserChats)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}