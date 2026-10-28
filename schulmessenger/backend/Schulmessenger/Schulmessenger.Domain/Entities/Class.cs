namespace Schulmessenger.Domain.Entities;

public class Class
{
    public Guid ClassId { get; set; }
    public required string ClassName { get; set; }
    
    public ICollection<Student> Students { get; } = [];
}