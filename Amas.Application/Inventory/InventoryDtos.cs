namespace Amas.Application.Inventory;

public sealed record InventoryItemDto(
    Guid Id,
    long InventoryItemNumber,
    Guid? ProductId,
    string? ProductName,
    string Name,
    string Sku,
    string Type,
    string Unit,
    decimal CurrentStock,
    decimal MinimumStock,
    bool IsActive,
    string? ImageUrl,
    string? ImageFileName,
    string? ImageContentType,
    long? ImageSizeBytes,
    bool IsBelowMinimum);

public sealed record UploadInventoryItemImageFile(
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content);

public sealed record InventoryMovementDto(
    Guid Id,
    long InventoryMovementNumber,
    Guid InventoryItemId,
    string MovementType,
    decimal Quantity,
    decimal StockAfter,
    decimal? UnitCost,
    string? Reason,
    string? Reference,
    DateTimeOffset OccurredAt);

public sealed record CreateInventoryItemRequest(
    Guid? ProductId,
    string Name,
    string Sku,
    string Type,
    string Unit,
    decimal InitialStock,
    decimal MinimumStock,
    bool IsActive);

public sealed record UpdateInventoryItemRequest(
    Guid? ProductId,
    string Name,
    string Sku,
    string Type,
    string Unit,
    decimal MinimumStock,
    bool IsActive);

public sealed record CreateInventoryMovementRequest(
    string MovementType,
    decimal Quantity,
    decimal? UnitCost,
    string? Reason,
    string? Reference);
