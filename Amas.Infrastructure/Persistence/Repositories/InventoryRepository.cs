using Amas.Application.Abstractions;
using Amas.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace Amas.Infrastructure.Persistence.Repositories;

public sealed class InventoryRepository(AmasDbContext dbContext) : IInventoryRepository
{
    public async Task<IReadOnlyList<InventoryItem>> ListItemsAsync(CancellationToken cancellationToken) =>
        await dbContext.InventoryItems
            .AsNoTracking()
            .Include(x => x.Product)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public Task<InventoryItem?> GetItemByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.InventoryItems
            .Include(x => x.Product)
            .Include(x => x.Movements)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(x => x.Id == productId, cancellationToken);

    public Task AddItemAsync(InventoryItem item, CancellationToken cancellationToken) =>
        dbContext.InventoryItems.AddAsync(item, cancellationToken).AsTask();

    public Task AddMovementAsync(InventoryMovement movement, CancellationToken cancellationToken) =>
        dbContext.InventoryMovements.AddAsync(movement, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
