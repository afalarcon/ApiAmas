using Amas.Application.Common;

namespace Amas.Application.Suppliers;

public interface ISupplierService
{
    Task<Result<IReadOnlyList<SupplierDto>>> ListAsync(CancellationToken cancellationToken);
    Task<Result<SupplierDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<SupplierDto>> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken);
    Task<Result<SupplierDto>> UpdateAsync(Guid id, UpdateSupplierRequest request, CancellationToken cancellationToken);
}
