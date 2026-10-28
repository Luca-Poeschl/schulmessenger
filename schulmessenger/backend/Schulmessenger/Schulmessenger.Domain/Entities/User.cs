namespace Schulmessenger.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; }

    public ICollection<Message> Messages { get; } = [];
    public ICollection<UserChat> UserChats { get; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; } = [];

    public Employee? Employee { get; set; }
    public Student? Student { get; set; }
}