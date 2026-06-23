namespace Amas.Application.Abstractions;

public interface IInvoiceDocumentReader
{
    Task<InvoiceReadResult> ReadAsync(InvoiceReadRequest request, CancellationToken cancellationToken);
}

public sealed record InvoiceReadRequest(
    string FileName,
    string ContentType,
    Stream Content);

public sealed record InvoiceReadResult(
    string? SupplierName,
    string? SupplierTaxId,
    string? InvoiceNumber,
    DateTimeOffset? InvoiceDate,
    decimal? Subtotal,
    decimal? TaxTotal,
    decimal? Total,
    string ExtractionProvider,
    string ExtractedJson,
    IReadOnlyList<InvoiceReadLine> Lines,
    string? Notes);

public sealed record InvoiceReadLine(
    int LineNumber,
    string? RawText,
    string? Sku,
    string Name,
    decimal Quantity,
    decimal? UnitCost,
    decimal? TaxPercent,
    decimal? TaxAmount,
    decimal? LineTotal);
