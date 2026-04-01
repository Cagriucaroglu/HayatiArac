namespace HayatiArac.Modules.Messaging.Application.DTOs;

public sealed record ConversationDto(
    Guid Id,
    Guid AdvertId,
    string AdvertTitle,
    Guid OtherParticipantId,
    string OtherParticipantName,
    string? LastMessageContent,
    DateTime LastMessageAt,
    int UnreadCount
);
