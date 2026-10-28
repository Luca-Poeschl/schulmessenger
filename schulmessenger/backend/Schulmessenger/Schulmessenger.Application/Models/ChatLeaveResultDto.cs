namespace Schulmessenger.Application.Models;

public record ChatLeaveResultDto(MessageDto? LeaveMessage, MessageDto? PromotionMessage = null, Guid? NewAdminId = null);