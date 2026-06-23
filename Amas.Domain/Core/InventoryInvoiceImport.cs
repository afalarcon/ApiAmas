using Amas.Domain.Common;

namespace Amas.Domain.Core;

public sealed class InventoryInvoiceImport : AuditableEntity
{
    public long InvoiceImportNumber { get; set; }
    public string Status { get; set; } = InventoryInvoiceImportStatuses.Uploaded;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string StorageProvider { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? SupplierName { get; set; }
    public string? SupplierTaxId { get; set; }
    public Guid? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTimeOffset? InvoiceDate { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? TaxTotal { get; set; }
    public decimal? Total { get; set; }
    public string? ExtractionProvider { get; set; }
    public string? ExtractedJson { get; set; }
    public string? Notes { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset? ConfirmedAt { get; set; }
    public string? ConfirmedBy { get; set; }
    public List<InventoryInvoiceImportLine> Lines { get; set; } = [];
}

public static class InventoryInvoiceImportStatuses
{
    public const string Uploaded = "Uploaded";
    public const string Processing = "Processing";
    public const string Processed = "Processed";
    public const string NeedsReview = "NeedsReview";
    public const string ReadyToConfirm = "ReadyToConfirm";
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";
    public const string Failed = "Failed";
}
