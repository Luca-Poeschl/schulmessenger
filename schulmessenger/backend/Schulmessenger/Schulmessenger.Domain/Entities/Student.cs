namespace Schulmessenger.Domain.Entities;

public class Student
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid? ClassId { get; set; }
    public Class? Class { get; set; }
}