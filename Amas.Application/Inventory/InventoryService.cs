using Amas.Application.Abstractions;
using Amas.Application.Common;
using Amas.Domain.Core;
using FluentValidation;

namespace Amas.Application.Inventory;

public sealed class InventoryService(
    IInventoryRepository inventory,
    IImageStorage imageStorage,
    ICacheService cache,
    IValidator<CreateInventoryItemRequest> createItemValidator,
    IValidator<UpdateInventoryItemRequest> updateItemValidator,
    IValidator<CreateInventoryMovementRequest> movementValidator) : IInventoryService
{
    private static readonly TimeSpan InventoryTtl = TimeSpan.FromMinutes(10);

    public async Task<Result<IReadOnlyList<InventoryItemDto>>> ListItemsAsync(CancellationToken cancellationToken)
    {
        var cached = await cache.GetAsync<IReadOnlyList<InventoryItemDto>>(CacheKeys.InventoryItems, cancellationToken);
        if (cached is not null)
        {
            return Result<IReadOnlyList<InventoryItemDto>>.Success(cached);
        }

        var items = (await inventory.ListItemsAsync(cancellationToken)).Select(MapItem).ToList();
        await cache.SetAsync(CacheKeys.InventoryItems, items, InventoryTtl, cancellationToken);

        return Result<IReadOnlyList<InventoryItemDto>>.Success(items);
    }

    public async Task<Result<InventoryItemDto>> GetItemByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await inventory.GetItemByIdAsync(id, cancellationToken);
        return item is null
            ? Result<InventoryItemDto>.Failure("Inventory item not found.")
            : Result<InventoryItemDto>.Success(MapItem(item));
    }

    public async Task<Result<InventoryItemDto>> CreateItemAsync(CreateInventoryItemRequest request, CancellationToken cancellationToken)
    {
        var validation = await createItemValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<InventoryItemDto>.Failure(validation.Errors[0].ErrorMessage);
        }

        if (request.ProductId.HasValue && !await inventory.ProductExistsAsync(request.ProductId.Value, cancellationToken))
        {
            return Result<InventoryItemDto>.Failure("Product not found.");
        }

        var item = new InventoryItem
        {
            ProductId = request.ProductId,
            Name = request.Name.Trim(),
            Sku = request.Sku.Trim(),
            Type = NormalizeItemType(request.Type),
            Unit = request.Unit.Trim(),
            CurrentStock = request.InitialStock,
            MinimumStock = request.MinimumStock,
            IsActive = request.IsActive
        };

        await inventory.AddItemAsync(item, cancellationToken);

        if (request.InitialStock > 0)
        {
            await inventory.AddMovementAsync(new InventoryMovement
            {
                InventoryItem = item,
                MovementType = InventoryMovementTypes.Entry,
                Quantity = request.InitialStock,
                StockAfter = request.InitialStock,
                Reason = "Initial stock",
                Reference = "initial"
            }, cancellationToken);
        }

        await inventory.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.InventoryItems, cancellationToken);

        return Result<InventoryItemDto>.Success(MapItem(item));
    }

    public async Task<Result<InventoryItemDto>> UpdateItemAsync(Guid id, UpdateInventoryItemRequest request, CancellationToken cancellationToken)
    {
        var validation = await updateItemValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<InventoryItemDto>.Failure(validation.Errors[0].ErrorMessage);
        }

        var item = await inventory.GetItemByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return Result<InventoryItemDto>.Failure("Inventory item not found.");
        }

        if (request.ProductId.HasValue && !await inventory.ProductExistsAsync(request.ProductId.Value, cancellationToken))
        {
            return Result<InventoryItemDto>.Failure("Product not found.");
        }

        item.ProductId = request.ProductId;
        item.Name = request.Name.Trim();
        item.Sku = request.Sku.Trim();
        item.Type = NormalizeItemType(request.Type);
        item.Unit = request.Unit.Trim();
        item.MinimumStock = request.MinimumStock;
        item.IsActive = request.IsActive;

        await inventory.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.InventoryItems, cancellationToken);

        return Result<InventoryItemDto>.Success(MapItem(item));
    }

    public async Task<Result<InventoryItemDto>> UploadItemImageAsync(
        Guid id,
        UploadInventoryItemImageFile file,
        CancellationToken cancellationToken)
    {
        var item = await inventory.GetItemByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return Result<InventoryItemDto>.Failure("Inventory item not found.");
        }

        if (file.SizeBytes <= 0)
        {
            return Result<InventoryItemDto>.Failure($"Image '{file.FileName}' is empty.");
        }

        StoredImageFile stored;
        try
        {
            stored = await imageStorage.SaveAsync(
                new ImageStorageRequest(
                    $"inventory/items/{id:N}",
                    file.FileName,
                    file.ContentType,
                    file.SizeBytes,
                    file.Content),
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return Result<InventoryItemDto>.Failure(ex.Message);
        }

        item.ImageUrl = stored.Url;
        item.ImageStoragePath = stored.StoragePath;
        item.ImageStorageProvider = stored.StorageProvider;
        item.ImageFileName = stored.FileName;
        item.ImageContentType = stored.ContentType;
        item.ImageSizeBytes = stored.SizeBytes;

        await inventory.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.InventoryItems, cancellationToken);

        return Result<InventoryItemDto>.Success(MapItem(item));
    }

    public async Task<Result<InventoryItemDto>> DeleteItemImageAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await inventory.GetItemByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return Result<InventoryItemDto>.Failure("Inventory item not found.");
        }

        item.ImageUrl = null;
        item.ImageStoragePath = null;
        item.ImageStorageProvider = null;
        item.ImageFileName = null;
        item.ImageContentType = null;
        item.ImageSizeBytes = null;

        await inventory.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.InventoryItems, cancellationToken);

        return Result<InventoryItemDto>.Success(MapItem(item));
    }

    public async Task<Result<InventoryMovementDto>> CreateMovementAsync(
        Guid itemId,
        CreateInventoryMovementRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await movementValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<InventoryMovementDto>.Failure(validation.Errors[0].ErrorMessage);
        }

        var item = await inventory.GetItemByIdAsync(itemId, cancellationToken);
        if (item is null)
        {
            return Result<InventoryMovementDto>.Failure("Inventory item not found.");
        }

        var movementType = NormalizeMovementType(request.MovementType);
        var signedQuantity = movementType == InventoryMovementTypes.Exit ? -request.Quantity : request.Quantity;
        var stockAfter = item.CurrentStock + signedQuantity;
        if (stockAfter < 0)
        {
            return Result<InventoryMovementDto>.Failure("Inventory stock cannot be negative.");
        }

        item.CurrentStock = stockAfter;

        var movement = new InventoryMovement
        {
            InventoryItemId = item.Id,
            MovementType = movementType,
            Quantity = request.Quantity,
            StockAfter = stockAfter,
            UnitCost = request.UnitCost,
            Reason = request.Reason?.Trim(),
            Reference = request.Reference?.Trim(),
            OccurredAt = DateTimeOffset.UtcNow
        };

        await inventory.AddMovementAsync(movement, cancellationToken);
        await inventory.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.InventoryItems, cancellationToken);

        return Result<InventoryMovementDto>.Success(MapMovement(movement));
    }

    public async Task<Result<IReadOnlyList<InventoryMovementDto>>> ListMovementsAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await inventory.GetItemByIdAsync(itemId, cancellationToken);
        return item is null
            ? Result<IReadOnlyList<InventoryMovementDto>>.Failure("Inventory item not found.")
            : Result<IReadOnlyList<InventoryMovementDto>>.Success(item.Movements
                .OrderByDescending(x => x.OccurredAt)
                .Select(MapMovement)
                .ToList());
    }

    private static string NormalizeItemType(string type) =>
        type.Trim().ToLowerInvariant() switch
        {
            "product" => InventoryItemTypes.Product,
            "supply" => InventoryItemTypes.Supply,
            "element" => InventoryItemTypes.Element,
            _ => type.Trim()
        };

    private static string NormalizeMovementType(string type) =>
        type.Trim().ToLowerInvariant() switch
        {
            "entry" => InventoryMovementTypes.Entry,
            "entrada" => InventoryMovementTypes.Entry,
            "exit" => InventoryMovementTypes.Exit,
            "salida" => InventoryMovementTypes.Exit,
            _ => type.Trim()
        };

    private static InventoryItemDto MapItem(InventoryItem item) =>
        new(
            item.Id,
            item.InventoryItemNumber,
            item.ProductId,
            item.Product?.Name,
            item.Name,
            item.Sku,
            item.Type,
            item.Unit,
            item.CurrentStock,
            item.MinimumStock,
            item.IsActive,
            item.ImageUrl,
            item.ImageFileName,
            item.ImageContentType,
            item.ImageSizeBytes,
            item.CurrentStock <= item.MinimumStock);

    private static InventoryMovementDto MapMovement(InventoryMovement movement) =>
        new(
            movement.Id,
            movement.InventoryMovementNumber,
            movement.InventoryItemId,
            movement.MovementType,
            movement.Quantity,
            movement.StockAfter,
            movement.UnitCost,
            movement.Reason,
            movement.Reference,
            movement.OccurredAt);
}

public static class InventoryItemTypes
{
    public const string Product = "Product";
    public const string Supply = "Supply";
    public const string Element = "Element";
}

public static class InventoryMovementTypes
{
    public const string Entry = "Entry";
    public const string Exit = "Exit";
}
