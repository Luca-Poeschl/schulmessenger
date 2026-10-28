namespace Schulmessenger.Domain.Entities;

public class Chat
{
    public Guid ChatId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }

    public ChatType ChatType { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Guid? LastMessageId { get; set; }
    public Message? LastMessage { get; set; }

    public ICollection<Message> Messages { get; } = [];

    public ICollection<UserChat> UserChats { get; } = [];
}