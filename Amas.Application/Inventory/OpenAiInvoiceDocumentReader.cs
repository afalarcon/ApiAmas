using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Amas.Application.Abstractions;

namespace Amas.Application.Inventory;

public sealed class OpenAiInvoiceDocumentReader(
    OpenAiInvoiceOptions options,
    IInvoiceExtractionMonitor monitor,
    BasicInvoiceDocumentReader fallback) : IInvoiceDocumentReader
{
    private static readonly HttpClient Client = new();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<InvoiceReadResult> ReadAsync(InvoiceReadRequest request, CancellationToken cancellationToken)
    {
        monitor.Configure(options);

        if (!options.Enabled || string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return await fallback.ReadAsync(request, cancellationToken);
        }

        try
        {
            using var response = await Client.SendAsync(BuildRequest(request), cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var (code, message) = ExtractError(response.StatusCode, responseJson);
                monitor.RecordFailure("openai", ResolveModel(), code, message);
                return BuildFailureResult(request, code, monitor.GetStatus().Message ?? message);
            }

            var outputText = ExtractOutputText(responseJson);
            if (string.IsNullOrWhiteSpace(outputText))
            {
                monitor.RecordFailure("openai", ResolveModel(), "empty_response", "OpenAI no devolvio texto estructurado para la factura.");
                return BuildFailureResult(request, "empty_response", "OpenAI no devolvio informacion estructurada. Completa las lineas manualmente.");
            }

            var extracted = JsonSerializer.Deserialize<OpenAiInvoiceExtraction>(outputText, JsonOptions);
            if (extracted is null)
            {
                monitor.RecordFailure("openai", ResolveModel(), "invalid_json", "OpenAI devolvio un JSON no compatible.");
                return BuildFailureResult(request, "invalid_json", "La respuesta de OpenAI no pudo interpretarse. Completa las lineas manualmente.");
            }

            monitor.RecordSuccess("openai", ResolveModel());
            return new InvoiceReadResult(
                extracted.SupplierName,
                extracted.SupplierNit,
                extracted.InvoiceNumber,
                ParseDate(extracted.InvoiceDate),
                extracted.Subtotal,
                extracted.TaxTotal,
                extracted.Total,
                "openai",
                outputText,
                MapLines(extracted.Lines),
                extracted.Notes);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            monitor.RecordFailure("openai", ResolveModel(), "request_failed", "No fue posible conectar con OpenAI para leer la factura.");
            return BuildFailureResult(request, "request_failed", $"{ex.Message}. Completa las lineas manualmente.");
        }
    }

    private HttpRequestMessage BuildRequest(InvoiceReadRequest request)
    {
        request.Content.Position = 0;
        using var buffer = new MemoryStream();
        request.Content.CopyTo(buffer);
        var base64 = Convert.ToBase64String(buffer.ToArray());

        object filePart = request.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
            ? new
            {
                type = "input_file",
                filename = request.FileName,
                file_data = $"data:{request.ContentType};base64,{base64}"
            }
            : new
            {
                type = "input_image",
                image_url = $"data:{request.ContentType};base64,{base64}"
            };

        var payload = new
        {
            model = ResolveModel(),
            input = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "input_text",
                            text = Prompt
                        },
                        filePart
                    }
                }
            },
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "invoice_extraction",
                    strict = true,
                    schema = Schema
                }
            }
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, ResolveResponsesUrl())
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey.Trim());
        return httpRequest;
    }

    private string ResolveModel() =>
        string.IsNullOrWhiteSpace(options.Model) ? "gpt-4.1-mini" : options.Model.Trim();

    private string ResolveResponsesUrl() =>
        string.IsNullOrWhiteSpace(options.ResponsesUrl) ? "https://api.openai.com/v1/responses" : options.ResponsesUrl.Trim();

    private static (string Code, string Message) ExtractError(HttpStatusCode statusCode, string responseJson)
    {
        try
        {
            using var document = JsonDocument.Parse(responseJson);
            var error = document.RootElement.TryGetProperty("error", out var errorElement) ? errorElement : default;
            var code = error.ValueKind == JsonValueKind.Object && error.TryGetProperty("code", out var codeElement)
                ? codeElement.GetString()
                : null;
            var message = error.ValueKind == JsonValueKind.Object && error.TryGetProperty("message", out var messageElement)
                ? messageElement.GetString()
                : null;

            return (
                string.IsNullOrWhiteSpace(code) ? statusCode.ToString() : code!,
                string.IsNullOrWhiteSpace(message) ? $"OpenAI respondio {statusCode}." : message!);
        }
        catch (JsonException)
        {
            return (statusCode.ToString(), $"OpenAI respondio {statusCode}.");
        }
    }

    private static string? ExtractOutputText(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        if (!document.RootElement.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in content.EnumerateArray())
            {
                if (contentItem.TryGetProperty("type", out var type) &&
                    type.GetString() == "output_text" &&
                    contentItem.TryGetProperty("text", out var text))
                {
                    return text.GetString();
                }

                if (contentItem.TryGetProperty("text", out var fallbackText))
                {
                    return fallbackText.GetString();
                }
            }
        }

        return null;
    }

    private static InvoiceReadResult BuildFailureResult(InvoiceReadRequest request, string errorCode, string notes)
    {
        var extractedJson = JsonSerializer.Serialize(new
        {
            provider = "openai",
            status = "NeedsReview",
            errorCode,
            fileName = request.FileName,
            contentType = request.ContentType,
            lines = Array.Empty<object>(),
            notes
        }, JsonOptions);

        return new InvoiceReadResult(null, null, null, null, null, null, null, "openai", extractedJson, [], notes);
    }

    private static DateTimeOffset? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateOnly.TryParse(value, out var dateOnly))
        {
            return new DateTimeOffset(dateOnly.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        }

        return DateTimeOffset.TryParse(value, out var parsed)
            ? parsed.ToUniversalTime()
            : null;
    }

    private static IReadOnlyList<InvoiceReadLine> MapLines(IReadOnlyList<OpenAiInvoiceLine>? lines) =>
        lines?
            .Where(line => !string.IsNullOrWhiteSpace(line.Name) && line.Quantity > 0)
            .Select((line, index) => new InvoiceReadLine(
                line.LineNumber > 0 ? line.LineNumber : index + 1,
                line.RawText,
                line.Sku,
                line.Name!.Trim(),
                line.Quantity,
                line.UnitCost,
                line.TaxPercent,
                line.TaxAmount,
                line.LineTotal))
            .ToList() ?? [];

    private const string Prompt = """
Extrae la informacion de esta factura de compra para alimentar un Kardex de inventario.
Responde solo JSON valido con los campos del esquema.
Lee proveedor, NIT si aparece, numero de factura, fecha, subtotal, IVA/impuestos, total y cada producto.
Para cada linea usa el codigo visible como sku si existe, conserva la descripcion original como name, cantidad, unidad, valor unitario, porcentaje de IVA, valor IVA y total de linea.
Si un dato no aparece, usa null. No inventes productos ni valores.
""";

    private static readonly object Schema = new
    {
        type = "object",
        additionalProperties = false,
        properties = new
        {
            supplierName = new { type = new[] { "string", "null" } },
            supplierNit = new { type = new[] { "string", "null" } },
            invoiceNumber = new { type = new[] { "string", "null" } },
            invoiceDate = new { type = new[] { "string", "null" } },
            subtotal = new { type = new[] { "number", "null" } },
            taxTotal = new { type = new[] { "number", "null" } },
            total = new { type = new[] { "number", "null" } },
            currency = new { type = new[] { "string", "null" } },
            notes = new { type = new[] { "string", "null" } },
            lines = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new
                    {
                        lineNumber = new { type = "integer" },
                        rawText = new { type = new[] { "string", "null" } },
                        sku = new { type = new[] { "string", "null" } },
                        name = new { type = new[] { "string", "null" } },
                        quantity = new { type = "number" },
                        unit = new { type = new[] { "string", "null" } },
                        unitCost = new { type = new[] { "number", "null" } },
                        taxPercent = new { type = new[] { "number", "null" } },
                        taxAmount = new { type = new[] { "number", "null" } },
                        lineTotal = new { type = new[] { "number", "null" } }
                    },
                    required = new[] { "lineNumber", "rawText", "sku", "name", "quantity", "unit", "unitCost", "taxPercent", "taxAmount", "lineTotal" }
                }
            }
        },
        required = new[] { "supplierName", "supplierNit", "invoiceNumber", "invoiceDate", "subtotal", "taxTotal", "total", "currency", "notes", "lines" }
    };

    private sealed record OpenAiInvoiceExtraction(
        string? SupplierName,
        string? SupplierNit,
        string? InvoiceNumber,
        string? InvoiceDate,
        decimal? Subtotal,
        decimal? TaxTotal,
        decimal? Total,
        string? Currency,
        string? Notes,
        IReadOnlyList<OpenAiInvoiceLine>? Lines);

    private sealed record OpenAiInvoiceLine(
        int LineNumber,
        string? RawText,
        string? Sku,
        string? Name,
        decimal Quantity,
        string? Unit,
        decimal? UnitCost,
        decimal? TaxPercent,
        decimal? TaxAmount,
        decimal? LineTotal);
}
