namespace Amas.Application.Inventory;

public sealed record InvoiceImportDto(
    Guid Id,
    long InvoiceImportNumber,
    string Status,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string Url,
    Guid? SupplierId,
    string? SupplierName,
    string? SupplierTaxId,
    string? SupplierStatus,
    string? InvoiceNumber,
    DateTimeOffset? InvoiceDate,
    decimal? Subtotal,
    decimal? TaxTotal,
    decimal? Total,
    string? ExtractionProvider,
    string? ExtractedJson,
    string? Notes,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ConfirmedAt,
    string? ConfirmedBy,
    IReadOnlyList<InvoiceImportLineDto> Lines);

public sealed record InvoiceImportLineDto(
    Guid Id,
    int LineNumber,
    string Status,
    string MatchStatus,
    int MatchConfidence,
    Guid? InventoryItemId,
    string? InventoryItemName,
    string? InventoryItemSku,
    string? RawText,
    string? ExtractedSku,
    string ExtractedName,
    decimal Quantity,
    decimal? UnitCost,
    decimal? TaxPercent,
    decimal? TaxAmount,
    decimal? LineTotal,
    string? Notes);

public sealed record UploadInvoiceImportRequest(
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    Stream Content,
    string? SupplierName,
    string? SupplierTaxId,
    string? InvoiceNumber,
    DateTimeOffset? InvoiceDate,
    string? Notes,
    string CreatedBy);

public sealed record CreateInvoiceImportLineRequest(
    Guid? InventoryItemId,
    string? RawText,
    string? ExtractedSku,
    string ExtractedName,
    decimal Quantity,
    decimal? UnitCost,
    decimal? TaxPercent,
    decimal? TaxAmount,
    decimal? LineTotal,
    string? Notes,
    bool Ignore);

public sealed record UpdateInvoiceImportLineRequest(
    Guid? InventoryItemId,
    string? RawText,
    string? ExtractedSku,
    string ExtractedName,
    decimal Quantity,
    decimal? UnitCost,
    decimal? TaxPercent,
    decimal? TaxAmount,
    decimal? LineTotal,
    string? Notes,
    bool Ignore);

public sealed record ConfirmInvoiceImportRequest(string ConfirmedBy);

public sealed record InvoiceExtractorStatusDto(
    string Provider,
    string Model,
    bool Enabled,
    bool Configured,
    bool Available,
    string Status,
    string? Message,
    string? LastErrorCode,
    DateTimeOffset? LastCheckedAt);
