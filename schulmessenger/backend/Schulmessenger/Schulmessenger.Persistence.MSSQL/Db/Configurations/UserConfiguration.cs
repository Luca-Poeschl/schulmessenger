using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Persistence.MSSQL.Db.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.UserId).ValueGeneratedOnAdd();
    }
}