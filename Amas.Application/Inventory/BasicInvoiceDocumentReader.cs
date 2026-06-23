using Amas.Application.Abstractions;
using System.Text.Json;

namespace Amas.Application.Inventory;

public sealed class BasicInvoiceDocumentReader : IInvoiceDocumentReader
{
    public Task<InvoiceReadResult> ReadAsync(InvoiceReadRequest request, CancellationToken cancellationToken)
    {
        var notes = request.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
            ? "PDF recibido. La extraccion automatica se conectara en la siguiente fase; completa las lineas manualmente."
            : "Imagen recibida. La extraccion OCR se conectara en la siguiente fase; completa las lineas manualmente.";

        var extractedJson = JsonSerializer.Serialize(new
        {
            provider = "basic-placeholder",
            status = "NeedsReview",
            fileName = request.FileName,
            contentType = request.ContentType,
            supplier = (string?)null,
            invoiceNumber = (string?)null,
            invoiceDate = (DateTimeOffset?)null,
            totals = new
            {
                subtotal = (decimal?)null,
                taxTotal = (decimal?)null,
                total = (decimal?)null
            },
            lines = Array.Empty<object>(),
            notes
        });

        return Task.FromResult(new InvoiceReadResult(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            "basic-placeholder",
            extractedJson,
            [],
            notes));
    }
}
