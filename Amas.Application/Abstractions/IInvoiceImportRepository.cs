using Amas.Domain.Core;

namespace Amas.Application.Abstractions;

public interface IInvoiceImportRepository
{
    Task<IReadOnlyList<InventoryInvoiceImport>> ListAsync(CancellationToken cancellationToken);
    Task<InventoryInvoiceImport?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(InventoryInvoiceImport import, CancellationToken cancellationToken);
    Task AddLineAsync(InventoryInvoiceImportLine line, CancellationToken cancellationToken);
    void RemoveLine(InventoryInvoiceImportLine line);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
