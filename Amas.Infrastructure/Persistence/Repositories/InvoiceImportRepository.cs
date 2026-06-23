using Amas.Application.Abstractions;
using Amas.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace Amas.Infrastructure.Persistence.Repositories;

public sealed class InvoiceImportRepository(AmasDbContext dbContext) : IInvoiceImportRepository
{
    public async Task<IReadOnlyList<InventoryInvoiceImport>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.InventoryInvoiceImports
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Lines)
            .ThenInclude(x => x.InventoryItem)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<InventoryInvoiceImport?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.InventoryInvoiceImports
            .Include(x => x.Supplier)
            .Include(x => x.Lines)
            .ThenInclude(x => x.InventoryItem)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(InventoryInvoiceImport import, CancellationToken cancellationToken) =>
        dbContext.InventoryInvoiceImports.AddAsync(import, cancellationToken).AsTask();

    public Task AddLineAsync(InventoryInvoiceImportLine line, CancellationToken cancellationToken) =>
        dbContext.InventoryInvoiceImportLines.AddAsync(line, cancellationToken).AsTask();

    public void RemoveLine(InventoryInvoiceImportLine line) =>
        dbContext.InventoryInvoiceImportLines.Remove(line);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
