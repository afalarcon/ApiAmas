using Amas.Domain.Core;

namespace Amas.Application.Abstractions;

public interface IContactRequestNotifier
{
    Task<ContactRequestNotificationResult> NotifyAsync(ContactRequest contactRequest, CancellationToken cancellationToken);
}

public sealed record ContactRequestNotificationResult(bool Delivered, string? Error)
{
    public static ContactRequestNotificationResult Success() => new(true, null);
    public static ContactRequestNotificationResult Skipped(string reason) => new(false, reason);
    public static ContactRequestNotificationResult Failure(string error) => new(false, error);
}
