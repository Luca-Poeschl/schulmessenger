namespace Schulmessenger.Domain.Entities;

public class UserChat
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid ChatId { get; set; }
    public Chat Chat { get; set; } = null!;

    public bool IsAdmin { get; set; }
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
}