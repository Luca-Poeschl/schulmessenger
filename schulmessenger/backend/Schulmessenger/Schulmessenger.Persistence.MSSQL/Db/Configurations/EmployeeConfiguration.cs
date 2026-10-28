using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Persistence.MSSQL.Db.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employee");
        builder.HasKey(e => e.UserId);
        builder.Property(e => e.UserId).ValueGeneratedNever();
        
        builder.
            HasOne(e => e.User)
            .WithOne(u => u.Employee)
            .HasForeignKey<Employee>(e => e.UserId) // Shared-Key mit User
            .OnDelete(DeleteBehavior.Restrict);
    }
}