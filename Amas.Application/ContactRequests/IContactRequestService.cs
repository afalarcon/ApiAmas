using Amas.Application.Common;

namespace Amas.Application.ContactRequests;

public interface IContactRequestService
{
    Task<Result<ContactRequestReceivedDto>> CreateAsync(CreateContactRequest request, CancellationToken cancellationToken);
}
