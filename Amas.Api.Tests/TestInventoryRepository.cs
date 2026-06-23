using Amas.Application.Abstractions;
using Amas.Domain.Core;

namespace Amas.Api.Tests;

internal sealed class TestInventoryRepository : IInventoryRepository
{
    private readonly List<InventoryItem> items = [];

    public Task<IReadOnlyList<InventoryItem>> ListItemsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<InventoryItem>>(items.OrderBy(x => x.Name).ToList());

    public Task<InventoryItem?> GetItemByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(items.FirstOrDefault(x => x.Id == id));

    public Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken) =>
        Task.FromResult(false);

    public Task AddItemAsync(InventoryItem item, CancellationToken cancellationToken)
    {
        items.Add(item);
        return Task.CompletedTask;
    }

    public Task AddMovementAsync(InventoryMovement movement, CancellationToken cancellationToken)
    {
        var item = items.FirstOrDefault(x => x.Id == movement.InventoryItemId) ?? movement.InventoryItem;
        if (item.Id == Guid.Empty)
        {
            item = items.First(x => x == movement.InventoryItem);
            movement.InventoryItemId = item.Id;
        }

        item.Movements.Add(movement);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
