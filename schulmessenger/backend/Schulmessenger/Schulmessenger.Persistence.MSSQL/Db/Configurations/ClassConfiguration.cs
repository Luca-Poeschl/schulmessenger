using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Persistence.MSSQL.Db.Configurations;

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Class");
        builder.HasKey(c => c.ClassId);
        builder.Property(c => c.ClassId).ValueGeneratedOnAdd();
        builder.Property(c => c.ClassName).HasMaxLength(9);
    }
}