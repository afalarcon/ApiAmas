using Amas.Domain.Common;

namespace Amas.Domain.Core;

public sealed class InventoryInvoiceImportLine : AuditableEntity
{
    public Guid InventoryInvoiceImportId { get; set; }
    public InventoryInvoiceImport InventoryInvoiceImport { get; set; } = default!;
    public Guid? InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public int LineNumber { get; set; }
    public string Status { get; set; } = InventoryInvoiceImportLineStatuses.NeedsReview;
    public string MatchStatus { get; set; } = InventoryInvoiceImportLineMatchStatuses.NoMatch;
    public int MatchConfidence { get; set; }
    public string? RawText { get; set; }
    public string? ExtractedSku { get; set; }
    public string ExtractedName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TaxPercent { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? LineTotal { get; set; }
    public string? Notes { get; set; }
}

public static class InventoryInvoiceImportLineStatuses
{
    public const string NeedsReview = "NeedsReview";
    public const string Ready = "Ready";
    public const string Ignored = "Ignored";
    public const string Imported = "Imported";
}

public static class InventoryInvoiceImportLineMatchStatuses
{
    public const string Exact = "Exact";
    public const string Probable = "Probable";
    public const string NoMatch = "NoMatch";
    public const string Manual = "Manual";
}
