namespace HayatiArac.Modules.Messaging.Application.DTOs;

public sealed record MessageDto(
    Guid Id,
    Guid SenderId,
    string Content,
    bool IsRead,
    DateTime SentAt
);
