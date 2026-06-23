using Amas.Application.Abstractions;
using Amas.Application.Common;
using Amas.Domain.Core;

namespace Amas.Application.Inventory;

public sealed class InvoiceImportService(
    IInvoiceImportRepository invoiceImports,
    IInventoryRepository inventory,
    ISupplierRepository suppliers,
    IInvoiceFileStorage fileStorage,
    IInvoiceDocumentReader documentReader,
    ICacheService cache) : IInvoiceImportService
{
    private const long MaxInvoiceFileBytes = 10 * 1024 * 1024;
    private static readonly string[] AllowedContentTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    public async Task<Result<IReadOnlyList<InvoiceImportDto>>> ListAsync(CancellationToken cancellationToken)
    {
        var imports = await invoiceImports.ListAsync(cancellationToken);
        return Result<IReadOnlyList<InvoiceImportDto>>.Success(imports.Select(MapImport).ToList());
    }

    public async Task<Result<InvoiceImportDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var import = await invoiceImports.GetByIdAsync(id, cancellationToken);
        return import is null
            ? Result<InvoiceImportDto>.Failure("Invoice import not found.")
            : Result<InvoiceImportDto>.Success(MapImport(import));
    }

    public async Task<Result<InvoiceImportDto>> UploadAsync(
        UploadInvoiceImportRequest request,
        CancellationToken cancellationToken)
    {
        if (request.SizeBytes <= 0 || request.SizeBytes > MaxInvoiceFileBytes)
        {
            return Result<InvoiceImportDto>.Failure("Invoice file size is not valid.");
        }

        if (!AllowedContentTypes.Contains(request.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return Result<InvoiceImportDto>.Failure("Invoice file content type is not supported.");
        }

        await using var buffer = new MemoryStream();
        await request.Content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        var importId = Guid.NewGuid();
        var stored = await fileStorage.SaveAsync(
            new InvoiceFileStorageRequest(
                $"inventory/invoices/{importId}",
                request.OriginalFileName,
                request.ContentType,
                request.SizeBytes,
                buffer),
            cancellationToken);

        buffer.Position = 0;
        var read = await documentReader.ReadAsync(
            new InvoiceReadRequest(request.OriginalFileName, request.ContentType, buffer),
            cancellationToken);
        var supplier = await ResolveSupplierAsync(request.SupplierName, request.SupplierTaxId, read.SupplierName, read.SupplierTaxId, cancellationToken);

        var import = new InventoryInvoiceImport
        {
            Id = importId,
            Status = read.Lines.Count > 0
                ? InventoryInvoiceImportStatuses.NeedsReview
                : InventoryInvoiceImportStatuses.NeedsReview,
            OriginalFileName = request.OriginalFileName.Trim(),
            StoredFileName = stored.FileName,
            ContentType = stored.ContentType,
            SizeBytes = stored.SizeBytes,
            StorageProvider = stored.StorageProvider,
            StoragePath = stored.StoragePath,
            Url = stored.Url,
            SupplierId = supplier?.Id,
            SupplierName = supplier?.Name ?? FirstNonEmpty(request.SupplierName, read.SupplierName),
            SupplierTaxId = supplier?.TaxId ?? FirstNonEmpty(request.SupplierTaxId, read.SupplierTaxId),
            InvoiceNumber = FirstNonEmpty(request.InvoiceNumber, read.InvoiceNumber),
            InvoiceDate = NormalizeUtc(request.InvoiceDate ?? read.InvoiceDate),
            Subtotal = read.Subtotal,
            TaxTotal = read.TaxTotal,
            Total = read.Total,
            ExtractionProvider = read.ExtractionProvider,
            ExtractedJson = read.ExtractedJson,
            Notes = FirstNonEmpty(request.Notes, read.Notes),
            CreatedBy = request.CreatedBy.Trim()
        };

        var items = await inventory.ListItemsAsync(cancellationToken);
        import.Lines = read.Lines.Select(line => BuildLine(import, line, items)).ToList();

        await invoiceImports.AddAsync(import, cancellationToken);
        await invoiceImports.SaveChangesAsync(cancellationToken);

        return Result<InvoiceImportDto>.Success(MapImport(import));
    }

    private async Task<Supplier?> ResolveSupplierAsync(
        string? requestSupplierName,
        string? requestSupplierTaxId,
        string? readSupplierName,
        string? readSupplierTaxId,
        CancellationToken cancellationToken)
    {
        var taxId = NormalizeTaxId(FirstNonEmpty(requestSupplierTaxId, readSupplierTaxId));
        var name = FirstNonEmpty(requestSupplierName, readSupplierName);

        if (!string.IsNullOrWhiteSpace(taxId))
        {
            var existingByTaxId = await suppliers.FindByTaxIdAsync(taxId, cancellationToken);
            if (existingByTaxId is not null)
            {
                return existingByTaxId;
            }
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            var existingByName = await suppliers.FindByNameAsync(name, cancellationToken);
            if (existingByName is not null)
            {
                return existingByName;
            }
        }

        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(taxId))
        {
            return null;
        }

        var supplier = new Supplier
        {
            Name = string.IsNullOrWhiteSpace(name) ? $"Proveedor {taxId}" : name.Trim(),
            TaxId = taxId,
            Status = SupplierStatuses.NeedsCompletion,
            Notes = "Creado automaticamente desde lectura de factura. Completar datos en administrador de proveedores."
        };

        await suppliers.AddAsync(supplier, cancellationToken);
        return supplier;
    }

    public async Task<Result<InvoiceImportLineDto>> AddLineAsync(
        Guid importId,
        CreateInvoiceImportLineRequest request,
        CancellationToken cancellationToken)
    {
        var import = await invoiceImports.GetByIdAsync(importId, cancellationToken);
        if (import is null)
        {
            return Result<InvoiceImportLineDto>.Failure("Invoice import not found.");
        }

        if (!CanEdit(import))
        {
            return Result<InvoiceImportLineDto>.Failure("Invoice import cannot be edited.");
        }

        var validation = await ValidateLineAsync(request.InventoryItemId, request.ExtractedName, request.Quantity, request.Ignore, cancellationToken);
        if (validation is not null)
        {
            return Result<InvoiceImportLineDto>.Failure(validation);
        }

        var line = new InventoryInvoiceImportLine
        {
            InventoryInvoiceImportId = import.Id,
            LineNumber = import.Lines.Count == 0 ? 1 : import.Lines.Max(x => x.LineNumber) + 1
        };

        ApplyLine(line, request.InventoryItemId, request.RawText, request.ExtractedSku, request.ExtractedName, request.Quantity, request.UnitCost, request.TaxPercent, request.TaxAmount, request.LineTotal, request.Notes, request.Ignore);
        await invoiceImports.AddLineAsync(line, cancellationToken);
        import.Status = ResolveImportStatus(import.Lines.Append(line));
        import.ExtractedJson = BuildExtractedJson(import.Lines.Append(line));
        await invoiceImports.SaveChangesAsync(cancellationToken);

        return Result<InvoiceImportLineDto>.Success(MapLine(line));
    }

    public async Task<Result<InvoiceImportLineDto>> UpdateLineAsync(
        Guid importId,
        Guid lineId,
        UpdateInvoiceImportLineRequest request,
        CancellationToken cancellationToken)
    {
        var import = await invoiceImports.GetByIdAsync(importId, cancellationToken);
        if (import is null)
        {
            return Result<InvoiceImportLineDto>.Failure("Invoice import not found.");
        }

        if (!CanEdit(import))
        {
            return Result<InvoiceImportLineDto>.Failure("Invoice import cannot be edited.");
        }

        var line = import.Lines.FirstOrDefault(x => x.Id == lineId);
        if (line is null)
        {
            return Result<InvoiceImportLineDto>.Failure("Invoice import line not found.");
        }

        var validation = await ValidateLineAsync(request.InventoryItemId, request.ExtractedName, request.Quantity, request.Ignore, cancellationToken);
        if (validation is not null)
        {
            return Result<InvoiceImportLineDto>.Failure(validation);
        }

        ApplyLine(line, request.InventoryItemId, request.RawText, request.ExtractedSku, request.ExtractedName, request.Quantity, request.UnitCost, request.TaxPercent, request.TaxAmount, request.LineTotal, request.Notes, request.Ignore);
        import.Status = ResolveImportStatus(import.Lines);
        import.ExtractedJson = BuildExtractedJson(import.Lines);
        await invoiceImports.SaveChangesAsync(cancellationToken);

        return Result<InvoiceImportLineDto>.Success(MapLine(line));
    }

    public async Task<Result<InvoiceImportDto>> ConfirmAsync(
        Guid importId,
        ConfirmInvoiceImportRequest request,
        CancellationToken cancellationToken)
    {
        var import = await invoiceImports.GetByIdAsync(importId, cancellationToken);
        if (import is null)
        {
            return Result<InvoiceImportDto>.Failure("Invoice import not found.");
        }

        if (import.Status == InventoryInvoiceImportStatuses.Confirmed)
        {
            return Result<InvoiceImportDto>.Failure("Invoice import is already confirmed.");
        }

        if (import.Status == InventoryInvoiceImportStatuses.Cancelled)
        {
            return Result<InvoiceImportDto>.Failure("Invoice import is cancelled.");
        }

        var readyLines = import.Lines
            .Where(x => x.Status == InventoryInvoiceImportLineStatuses.Ready)
            .ToList();

        if (readyLines.Count == 0)
        {
            return Result<InvoiceImportDto>.Failure("Invoice import has no ready lines.");
        }

        foreach (var line in readyLines)
        {
            if (!line.InventoryItemId.HasValue)
            {
                return Result<InvoiceImportDto>.Failure("One or more lines are missing inventory item.");
            }

            var item = await inventory.GetItemByIdAsync(line.InventoryItemId.Value, cancellationToken);
            if (item is null)
            {
                return Result<InvoiceImportDto>.Failure("One or more inventory items do not exist.");
            }

            item.CurrentStock += line.Quantity;
            await inventory.AddMovementAsync(new InventoryMovement
            {
                InventoryItemId = item.Id,
                MovementType = InventoryMovementTypes.Entry,
                Quantity = line.Quantity,
                StockAfter = item.CurrentStock,
                UnitCost = line.UnitCost,
                Reason = line.TaxPercent.HasValue ? $"Invoice import IVA {line.TaxPercent:0.##}%" : "Invoice import",
                Reference = import.InvoiceNumber ?? import.OriginalFileName,
                OccurredAt = DateTimeOffset.UtcNow
            }, cancellationToken);

            line.Status = InventoryInvoiceImportLineStatuses.Imported;
        }

        import.Status = InventoryInvoiceImportStatuses.Confirmed;
        import.ConfirmedAt = DateTimeOffset.UtcNow;
        import.ConfirmedBy = request.ConfirmedBy.Trim();

        await invoiceImports.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.InventoryItems, cancellationToken);

        return Result<InvoiceImportDto>.Success(MapImport(import));
    }

    public async Task<Result<InvoiceImportDto>> CancelAsync(Guid importId, CancellationToken cancellationToken)
    {
        var import = await invoiceImports.GetByIdAsync(importId, cancellationToken);
        if (import is null)
        {
            return Result<InvoiceImportDto>.Failure("Invoice import not found.");
        }

        if (import.Status == InventoryInvoiceImportStatuses.Confirmed)
        {
            return Result<InvoiceImportDto>.Failure("Invoice import is already confirmed.");
        }

        import.Status = InventoryInvoiceImportStatuses.Cancelled;
        await invoiceImports.SaveChangesAsync(cancellationToken);

        return Result<InvoiceImportDto>.Success(MapImport(import));
    }

    private async Task<string?> ValidateLineAsync(
        Guid? inventoryItemId,
        string extractedName,
        decimal quantity,
        bool ignore,
        CancellationToken cancellationToken)
    {
        if (ignore)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(extractedName))
        {
            return "Invoice line name is required.";
        }

        if (quantity <= 0)
        {
            return "Invoice line quantity must be greater than zero.";
        }

        if (!inventoryItemId.HasValue)
        {
            return "Invoice line inventory item is required.";
        }

        return await inventory.GetItemByIdAsync(inventoryItemId.Value, cancellationToken) is null
            ? "Inventory item not found."
            : null;
    }

    private static InventoryInvoiceImportLine BuildLine(
        InventoryInvoiceImport import,
        InvoiceReadLine readLine,
        IReadOnlyList<InventoryItem> items)
    {
        var match = MatchInventoryItem(readLine, items);
        var line = new InventoryInvoiceImportLine
        {
            InventoryInvoiceImport = import,
            LineNumber = readLine.LineNumber,
            RawText = readLine.RawText,
            ExtractedSku = readLine.Sku?.Trim(),
            ExtractedName = readLine.Name.Trim(),
            Quantity = readLine.Quantity,
            UnitCost = readLine.UnitCost,
            TaxPercent = readLine.TaxPercent,
            TaxAmount = readLine.TaxAmount,
            LineTotal = readLine.LineTotal,
            InventoryItemId = match.Item?.Id,
            MatchStatus = match.Status,
            MatchConfidence = match.Confidence
        };

        line.Status = line.InventoryItemId.HasValue && line.Quantity > 0
            ? InventoryInvoiceImportLineStatuses.Ready
            : InventoryInvoiceImportLineStatuses.NeedsReview;

        return line;
    }

    private static (InventoryItem? Item, string Status, int Confidence) MatchInventoryItem(
        InvoiceReadLine line,
        IReadOnlyList<InventoryItem> items)
    {
        if (!string.IsNullOrWhiteSpace(line.Sku))
        {
            var skuMatch = items.FirstOrDefault(item => string.Equals(item.Sku, line.Sku.Trim(), StringComparison.OrdinalIgnoreCase));
            if (skuMatch is not null)
            {
                return (skuMatch, InventoryInvoiceImportLineMatchStatuses.Exact, 100);
            }
        }

        var name = line.Name.Trim();
        if (!string.IsNullOrWhiteSpace(name))
        {
            var exactName = items.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
            if (exactName is not null)
            {
                return (exactName, InventoryInvoiceImportLineMatchStatuses.Exact, 95);
            }

            var probable = items.FirstOrDefault(item =>
                item.Name.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                name.Contains(item.Name, StringComparison.OrdinalIgnoreCase));
            if (probable is not null)
            {
                return (probable, InventoryInvoiceImportLineMatchStatuses.Probable, 70);
            }
        }

        return (null, InventoryInvoiceImportLineMatchStatuses.NoMatch, 0);
    }

    private static void ApplyLine(
        InventoryInvoiceImportLine line,
        Guid? inventoryItemId,
        string? rawText,
        string? extractedSku,
        string extractedName,
        decimal quantity,
        decimal? unitCost,
        decimal? taxPercent,
        decimal? taxAmount,
        decimal? lineTotal,
        string? notes,
        bool ignore)
    {
        line.InventoryItemId = inventoryItemId;
        line.RawText = string.IsNullOrWhiteSpace(rawText) ? null : rawText.Trim();
        line.ExtractedSku = string.IsNullOrWhiteSpace(extractedSku) ? null : extractedSku.Trim();
        line.ExtractedName = extractedName.Trim();
        line.Quantity = quantity;
        line.UnitCost = unitCost;
        line.TaxPercent = taxPercent;
        line.TaxAmount = taxAmount;
        line.LineTotal = lineTotal;
        line.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        line.MatchStatus = inventoryItemId.HasValue
            ? InventoryInvoiceImportLineMatchStatuses.Manual
            : InventoryInvoiceImportLineMatchStatuses.NoMatch;
        line.MatchConfidence = inventoryItemId.HasValue ? 100 : 0;
        line.Status = ignore
            ? InventoryInvoiceImportLineStatuses.Ignored
            : InventoryInvoiceImportLineStatuses.Ready;
    }

    private static string ResolveImportStatus(IEnumerable<InventoryInvoiceImportLine> lines)
    {
        var relevantLines = lines.Where(x => x.Status != InventoryInvoiceImportLineStatuses.Ignored).ToList();
        return relevantLines.Count > 0 && relevantLines.All(x => x.Status == InventoryInvoiceImportLineStatuses.Ready)
            ? InventoryInvoiceImportStatuses.ReadyToConfirm
            : InventoryInvoiceImportStatuses.NeedsReview;
    }

    private static bool CanEdit(InventoryInvoiceImport import) =>
        import.Status is not InventoryInvoiceImportStatuses.Confirmed and not InventoryInvoiceImportStatuses.Cancelled;

    private static string? FirstNonEmpty(string? first, string? second) =>
        !string.IsNullOrWhiteSpace(first) ? first.Trim() :
        !string.IsNullOrWhiteSpace(second) ? second.Trim() :
        null;

    private static string? NormalizeTaxId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = new string(value.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static DateTimeOffset? NormalizeUtc(DateTimeOffset? value) =>
        value.HasValue ? value.Value.ToUniversalTime() : null;

    private static InvoiceImportDto MapImport(InventoryInvoiceImport import) =>
        new(
            import.Id,
            import.InvoiceImportNumber,
            import.Status,
            import.OriginalFileName,
            import.ContentType,
            import.SizeBytes,
            import.Url,
            import.SupplierId,
            import.SupplierName,
            import.SupplierTaxId,
            import.Supplier?.Status,
            import.InvoiceNumber,
            import.InvoiceDate,
            import.Subtotal,
            import.TaxTotal,
            import.Total,
            import.ExtractionProvider,
            import.ExtractedJson,
            import.Notes,
            import.CreatedBy,
            import.CreatedAt,
            import.ConfirmedAt,
            import.ConfirmedBy,
            import.Lines.OrderBy(x => x.LineNumber).Select(MapLine).ToList());

    private static InvoiceImportLineDto MapLine(InventoryInvoiceImportLine line) =>
        new(
            line.Id,
            line.LineNumber,
            line.Status,
            line.MatchStatus,
            line.MatchConfidence,
            line.InventoryItemId,
            line.InventoryItem?.Name,
            line.InventoryItem?.Sku,
            line.RawText,
            line.ExtractedSku,
            line.ExtractedName,
            line.Quantity,
            line.UnitCost,
            line.TaxPercent,
            line.TaxAmount,
            line.LineTotal,
            line.Notes);

    private static string BuildExtractedJson(IEnumerable<InventoryInvoiceImportLine> lines) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            provider = "manual-review",
            status = "NeedsReview",
            lines = lines.OrderBy(x => x.LineNumber).Select(line => new
            {
                lineNumber = line.LineNumber,
                status = line.Status,
                matchStatus = line.MatchStatus,
                matchConfidence = line.MatchConfidence,
                inventoryItemId = line.InventoryItemId,
                rawText = line.RawText,
                sku = line.ExtractedSku,
                name = line.ExtractedName,
                quantity = line.Quantity,
                unitCost = line.UnitCost,
                taxPercent = line.TaxPercent,
                taxAmount = line.TaxAmount,
                lineTotal = line.LineTotal,
                notes = line.Notes
            })
        });
}
