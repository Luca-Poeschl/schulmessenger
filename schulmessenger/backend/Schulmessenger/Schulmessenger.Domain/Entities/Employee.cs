namespace Schulmessenger.Domain.Entities;

public class Employee
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}