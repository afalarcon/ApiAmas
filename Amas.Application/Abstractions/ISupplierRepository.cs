using Amas.Domain.Core;

namespace Amas.Application.Abstractions;

public interface ISupplierRepository
{
    Task<IReadOnlyList<Supplier>> ListAsync(CancellationToken cancellationToken);
    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Supplier?> FindByTaxIdAsync(string taxId, CancellationToken cancellationToken);
    Task<Supplier?> FindByNameAsync(string name, CancellationToken cancellationToken);
    Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken);
    Task AddAsync(Supplier supplier, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
