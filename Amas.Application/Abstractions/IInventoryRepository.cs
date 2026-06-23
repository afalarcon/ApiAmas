using Amas.Domain.Core;

namespace Amas.Application.Abstractions;

public interface IInventoryRepository
{
    Task<IReadOnlyList<InventoryItem>> ListItemsAsync(CancellationToken cancellationToken);
    Task<InventoryItem?> GetItemByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken);
    Task AddItemAsync(InventoryItem item, CancellationToken cancellationToken);
    Task AddMovementAsync(InventoryMovement movement, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
