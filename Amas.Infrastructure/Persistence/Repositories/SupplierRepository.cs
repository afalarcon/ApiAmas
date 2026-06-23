using Amas.Application.Abstractions;
using Amas.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace Amas.Infrastructure.Persistence.Repositories;

public sealed class SupplierRepository(AmasDbContext dbContext) : ISupplierRepository
{
    public async Task<IReadOnlyList<Supplier>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Suppliers
            .AsNoTracking()
            .Include(x => x.Category)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Suppliers
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Supplier?> FindByTaxIdAsync(string taxId, CancellationToken cancellationToken) =>
        dbContext.Suppliers.FirstOrDefaultAsync(x => x.TaxId == taxId, cancellationToken);

    public Task<Supplier?> FindByNameAsync(string name, CancellationToken cancellationToken) =>
        dbContext.Suppliers.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower(), cancellationToken);

    public Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken) =>
        dbContext.Categories.AnyAsync(x => x.Id == categoryId, cancellationToken);

    public Task AddAsync(Supplier supplier, CancellationToken cancellationToken) =>
        dbContext.Suppliers.AddAsync(supplier, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
