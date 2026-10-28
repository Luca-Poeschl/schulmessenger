using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Persistence.MSSQL.Db.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshToken");
        builder.HasKey(r => r.RefreshTokenId);
        builder.Property(r => r.RefreshTokenId).ValueGeneratedOnAdd();

        builder.Property(r => r.Token).IsRequired().HasMaxLength(256);
        builder.HasIndex(r => r.Token).IsUnique();

        builder.Property(r => r.ExpiresAt).IsRequired();

        builder.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}