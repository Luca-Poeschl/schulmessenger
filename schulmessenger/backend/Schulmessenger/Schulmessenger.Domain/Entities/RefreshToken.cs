namespace Schulmessenger.Domain.Entities;

public class RefreshToken
{
    public Guid RefreshTokenId { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}