using Microsoft.EntityFrameworkCore;
using Schulmessenger.Application.Interfaces.Db;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Persistence.MSSQL.Db;

public class SchulmessengerDbContext : DbContext, ISchulmessengerDbContext
{
    public DbSet<Chat> Chat => Set<Chat>();
    public DbSet<Message> Message => Set<Message>();
    public DbSet<User> User => Set<User>();
    public DbSet<Student> Student => Set<Student>();
    public DbSet<Employee> Employee => Set<Employee>();
    public DbSet<UserChat> UserChat => Set<UserChat>();
    public DbSet<Class> Class => Set<Class>();
    public DbSet<RefreshToken> RefreshToken => Set<RefreshToken>();
    
    protected SchulmessengerDbContext()
    {
    }

    public SchulmessengerDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("schulmessenger");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchulmessengerDbContext).Assembly);

        modelBuilder.Entity<Chat>()
            .Property(m => m.ChatType)
            .HasConversion<string>(); // In der DB als string
    }
}