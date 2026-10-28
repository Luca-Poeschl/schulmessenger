using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Models;

public record ChatCreateDto(string? Name, string? Description, ChatType ChatType, ICollection<UserOnChatCreateDto> Users);