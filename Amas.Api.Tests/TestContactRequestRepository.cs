using Amas.Application.Abstractions;
using Amas.Domain.Core;

namespace Amas.Api.Tests;

internal sealed class TestContactRequestRepository : IContactRequestRepository
{
    private readonly List<ContactRequest> contactRequests = [];
    private long nextNumber = 1000;

    public IReadOnlyList<ContactRequest> Items => contactRequests;

    public Task AddAsync(ContactRequest contactRequest, CancellationToken cancellationToken)
    {
        contactRequests.Add(contactRequest);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        foreach (var item in contactRequests.Where(x => x.ContactRequestNumber == 0))
        {
            item.ContactRequestNumber = ++nextNumber;
        }

        return Task.CompletedTask;
    }
}
