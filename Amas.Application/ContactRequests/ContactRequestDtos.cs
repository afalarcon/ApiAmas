namespace Amas.Application.ContactRequests;

public sealed record ContactRequestDto(
    Guid Id,
    long ContactRequestNumber,
    string FullName,
    string Email,
    string? Phone,
    string RequestType,
    string Message,
    string SourcePage,
    string Status,
    bool WebhookDelivered,
    DateTimeOffset ReceivedAt);

public sealed record ContactRequestReceivedDto(
    long ContactRequestNumber,
    string Status,
    DateTimeOffset ReceivedAt);

public sealed record CreateContactRequest(
    string FullName,
    string Email,
    string? Phone,
    string RequestType,
    string Message,
    string? SourcePage,
    string? CaptchaToken,
    string? Website,
    string? IpAddress,
    string? UserAgent);
