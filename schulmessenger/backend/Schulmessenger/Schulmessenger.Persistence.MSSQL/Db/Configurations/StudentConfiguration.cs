using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Persistence.MSSQL.Db.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Student");
        builder.HasKey(s => s.UserId);
        builder.Property(s => s.UserId).ValueGeneratedNever();
        
        builder
            .HasOne(s => s.User)
            .WithOne(u => u.Student)
            .HasForeignKey<Student>(s => s.UserId) // Shared-Key mit User
            .OnDelete(DeleteBehavior.Restrict);
        
        builder
            .HasOne(s => s.Class)
            .WithMany(c => c.Students)
            .HasForeignKey(s => s.ClassId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}