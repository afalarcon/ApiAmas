using Amas.Domain.Common;

namespace Amas.Domain.Core;

public sealed class InventoryMovement : AuditableEntity
{
    public long InventoryMovementNumber { get; set; }
    public Guid InventoryItemId { get; set; }
    public InventoryItem InventoryItem { get; set; } = default!;
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal StockAfter { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Reason { get; set; }
    public string? Reference { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
