using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Interfaces.Db;

public interface ISchulmessengerDbContext
{
    public DbSet<Chat> Chat { get; }
    public DbSet<Message> Message { get; }
    public DbSet<User> User { get; }
    public DbSet<Student> Student { get; }
    public DbSet<Employee> Employee { get; }
    public DbSet<UserChat> UserChat { get; }
    public DbSet<Class> Class { get; }
    public DbSet<RefreshToken> RefreshToken { get; }
    
    public DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}