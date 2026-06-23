using Amas.Domain.Common;

namespace Amas.Domain.Core;

public sealed class InventoryItem : AuditableEntity
{
    public long InventoryItemNumber { get; set; }
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal MinimumStock { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }
    public string? ImageStoragePath { get; set; }
    public string? ImageStorageProvider { get; set; }
    public string? ImageFileName { get; set; }
    public string? ImageContentType { get; set; }
    public long? ImageSizeBytes { get; set; }
    public List<InventoryMovement> Movements { get; set; } = [];
}
