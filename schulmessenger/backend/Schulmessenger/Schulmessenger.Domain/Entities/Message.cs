namespace Schulmessenger.Domain.Entities;

public class Message
{
    public Guid MessageId { get; set; }
    public required string Content { get; set; }
    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsSystemMessage { get; set; }
    public bool IsDeleted { get; set; }
    
    public Guid? SenderId { get; set; }
    public User? Sender { get; set; }
    
    public Guid ChatId { get; set; }
    public Chat Chat { get; set; } = null!;
}