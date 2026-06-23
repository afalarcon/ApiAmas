using Amas.Application.Abstractions;
using Amas.Domain.Core;

namespace Amas.Api.Tests;

internal sealed class TestContactRequestNotifier : IContactRequestNotifier
{
    public Task<ContactRequestNotificationResult> NotifyAsync(ContactRequest contactRequest, CancellationToken cancellationToken) =>
        Task.FromResult(ContactRequestNotificationResult.Success());
}
