using Amas.Application.Common;

namespace Amas.Application.Inventory;

public interface IInventoryService
{
    Task<Result<IReadOnlyList<InventoryItemDto>>> ListItemsAsync(CancellationToken cancellationToken);
    Task<Result<InventoryItemDto>> GetItemByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<InventoryItemDto>> CreateItemAsync(CreateInventoryItemRequest request, CancellationToken cancellationToken);
    Task<Result<InventoryItemDto>> UpdateItemAsync(Guid id, UpdateInventoryItemRequest request, CancellationToken cancellationToken);
    Task<Result<InventoryItemDto>> UploadItemImageAsync(Guid id, UploadInventoryItemImageFile file, CancellationToken cancellationToken);
    Task<Result<InventoryItemDto>> DeleteItemImageAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<InventoryMovementDto>> CreateMovementAsync(Guid itemId, CreateInventoryMovementRequest request, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<InventoryMovementDto>>> ListMovementsAsync(Guid itemId, CancellationToken cancellationToken);
}
