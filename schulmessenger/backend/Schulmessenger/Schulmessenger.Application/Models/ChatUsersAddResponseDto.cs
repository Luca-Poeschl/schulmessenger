namespace Schulmessenger.Application.Models;

public record ChatUsersAddResponseDto(UserOnChatInfoDto AddedUser, MessageDto SystemMessage);