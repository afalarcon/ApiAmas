using Amas.Domain.Common;

namespace Amas.Domain.Core;

public sealed class ContactRequest : AuditableEntity
{
    public long ContactRequestNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string SourcePage { get; set; } = string.Empty;
    public string Status { get; set; } = ContactRequestStatuses.New;
    public string? IpAddressHash { get; set; }
    public string? UserAgent { get; set; }
    public string? CaptchaProvider { get; set; }
    public bool CaptchaTokenProvided { get; set; }
    public bool WebhookDelivered { get; set; }
    public DateTimeOffset? WebhookDeliveredAt { get; set; }
    public string? WebhookError { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public string? Notes { get; set; }
}

public static class ContactRequestStatuses
{
    public const string New = "New";
    public const string WebhookPending = "WebhookPending";
    public const string InReview = "InReview";
    public const string Contacted = "Contacted";
    public const string Closed = "Closed";
    public const string Spam = "Spam";
}
