using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Amas.Application.Abstractions;
using Amas.Application.Common;
using Amas.Domain.Core;

namespace Amas.Application.ContactRequests;

public sealed partial class ContactRequestService(
    IContactRequestRepository contactRequests,
    IContactRequestNotifier notifier) : IContactRequestService
{
    private static readonly HashSet<string> AllowedRequestTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Producto personalizado",
        "Cotización",
        "Impresión 3D",
        "Papelería creativa",
        "Sublimación",
        "Otro"
    };

    public async Task<Result<ContactRequestReceivedDto>> CreateAsync(CreateContactRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            return Result<ContactRequestReceivedDto>.Success(new ContactRequestReceivedDto(0, ContactRequestStatuses.New, DateTimeOffset.UtcNow));
        }

        var validation = Validate(request);
        if (validation is not null)
        {
            return Result<ContactRequestReceivedDto>.Failure(validation);
        }

        var contactRequest = new ContactRequest
        {
            FullName = CleanRequired(request.FullName),
            Email = CleanRequired(request.Email).ToLowerInvariant(),
            Phone = CleanOptional(request.Phone),
            RequestType = CleanRequired(request.RequestType),
            Message = CleanRequired(request.Message),
            SourcePage = CleanOptional(request.SourcePage) ?? "landing",
            Status = ContactRequestStatuses.New,
            IpAddressHash = HashIpAddress(request.IpAddress),
            UserAgent = Truncate(CleanOptional(request.UserAgent), 500),
            CaptchaProvider = string.IsNullOrWhiteSpace(request.CaptchaToken) ? null : "Turnstile",
            CaptchaTokenProvided = !string.IsNullOrWhiteSpace(request.CaptchaToken)
        };

        await contactRequests.AddAsync(contactRequest, cancellationToken);
        await contactRequests.SaveChangesAsync(cancellationToken);

        var notification = await notifier.NotifyAsync(contactRequest, cancellationToken);
        var webhookDisabled = string.Equals(notification.Error, "Webhook disabled.", StringComparison.OrdinalIgnoreCase);
        contactRequest.WebhookDelivered = notification.Delivered;
        contactRequest.WebhookDeliveredAt = notification.Delivered ? DateTimeOffset.UtcNow : null;
        contactRequest.WebhookError = notification.Delivered || webhookDisabled ? null : Truncate(notification.Error, 1000);
        contactRequest.Status = notification.Delivered || webhookDisabled ? ContactRequestStatuses.New : ContactRequestStatuses.WebhookPending;
        await contactRequests.SaveChangesAsync(cancellationToken);

        return Result<ContactRequestReceivedDto>.Success(new ContactRequestReceivedDto(
            contactRequest.ContactRequestNumber,
            contactRequest.Status,
            contactRequest.CreatedAt));
    }

    private static string? Validate(CreateContactRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length is < 3 or > 120)
        {
            return "Full name is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Trim().Length > 180 || !EmailRegex().IsMatch(request.Email.Trim()))
        {
            return "Email is not valid.";
        }

        if (!string.IsNullOrWhiteSpace(request.Phone) && request.Phone.Trim().Length > 80)
        {
            return "Phone is too long.";
        }

        if (string.IsNullOrWhiteSpace(request.RequestType) || !AllowedRequestTypes.Contains(request.RequestType.Trim()))
        {
            return "Request type is not valid.";
        }

        if (string.IsNullOrWhiteSpace(request.Message) || request.Message.Trim().Length is < 15 or > 1200)
        {
            return "Message is required.";
        }

        if (!string.IsNullOrWhiteSpace(request.SourcePage) && request.SourcePage.Trim().Length > 260)
        {
            return "Source page is too long.";
        }

        return null;
    }

    private static string CleanRequired(string value) => value.Trim();

    private static string? CleanOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Truncate(string? value, int maxLength) =>
        value is null || value.Length <= maxLength ? value : value[..maxLength];

    private static string? HashIpAddress(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return null;
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(ipAddress.Trim()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
