using Amas.Application.Abstractions;
using Amas.Domain.Core;

namespace Amas.Infrastructure.Persistence.Repositories;

public sealed class ContactRequestRepository(AmasDbContext dbContext) : IContactRequestRepository
{
    public Task AddAsync(ContactRequest contactRequest, CancellationToken cancellationToken) =>
        dbContext.ContactRequests.AddAsync(contactRequest, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
