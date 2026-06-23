using System.Net.Http.Json;
using Amas.Application.Abstractions;
using Amas.Domain.Core;
using Microsoft.Extensions.Options;

namespace Amas.Infrastructure.Notifications;

public sealed class WebhookContactRequestNotifier(
    HttpClient httpClient,
    IOptions<ContactWebhookOptions> options) : IContactRequestNotifier
{
    public async Task<ContactRequestNotificationResult> NotifyAsync(ContactRequest contactRequest, CancellationToken cancellationToken)
    {
        var webhookOptions = options.Value;
        if (!webhookOptions.Enabled || string.IsNullOrWhiteSpace(webhookOptions.Url))
        {
            return ContactRequestNotificationResult.Skipped("Webhook disabled.");
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(webhookOptions.TimeoutSeconds, 2, 30)));

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, webhookOptions.Url)
            {
                Content = JsonContent.Create(new ContactRequestWebhookPayload(
                    "contact.created",
                    contactRequest.Id,
                    contactRequest.ContactRequestNumber,
                    contactRequest.FullName,
                    contactRequest.Email,
                    contactRequest.Phone,
                    contactRequest.RequestType,
                    contactRequest.Message,
                    contactRequest.SourcePage,
                    contactRequest.CreatedAt))
            };

            if (!string.IsNullOrWhiteSpace(webhookOptions.Secret))
            {
                request.Headers.Add("X-Amas-Webhook-Secret", webhookOptions.Secret);
            }

            var response = await httpClient.SendAsync(request, timeout.Token);
            return response.IsSuccessStatusCode
                ? ContactRequestNotificationResult.Success()
                : ContactRequestNotificationResult.Failure($"Webhook returned {(int)response.StatusCode}.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ContactRequestNotificationResult.Failure("Webhook timeout.");
        }
        catch (Exception ex)
        {
            return ContactRequestNotificationResult.Failure(ex.Message);
        }
    }

    private sealed record ContactRequestWebhookPayload(
        string Event,
        Guid Id,
        long Number,
        string FullName,
        string Email,
        string? Phone,
        string RequestType,
        string Message,
        string SourcePage,
        DateTimeOffset CreatedAt);
}
