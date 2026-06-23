using Amas.Domain.Core;

namespace Amas.Application.Abstractions;

public interface IContactRequestRepository
{
    Task AddAsync(ContactRequest contactRequest, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
