using System.Security.Claims;
using Amas.Api.Contracts;
using Amas.Application.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Amas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/inventory/invoices")]
public sealed class InventoryInvoicesController(
    IInvoiceImportService invoiceImports,
    IInvoiceExtractionMonitor extractionMonitor,
    OpenAiInvoiceOptions openAiOptions) : ControllerBase
{
    [HttpGet("extractor/status")]
    public ActionResult<ApiResponse<InvoiceExtractorStatusDto>> ExtractorStatus()
    {
        extractionMonitor.Configure(openAiOptions);
        return Ok(ApiResponse<InvoiceExtractorStatusDto>.Success(extractionMonitor.GetStatus()));
    }

    [HttpGet("imports")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvoiceImportDto>>>> Imports(CancellationToken cancellationToken)
    {
        var result = await invoiceImports.ListAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<InvoiceImportDto>>.Success(result.Data!));
    }

    [HttpGet("imports/{id:guid}")]
    public async Task<ActionResult<ApiResponse<InvoiceImportDto>>> Import(Guid id, CancellationToken cancellationToken)
    {
        var result = await invoiceImports.GetByIdAsync(id, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<InvoiceImportDto>.Success(result.Data!))
            : NotFound(ApiResponse<InvoiceImportDto>.Failure(result.Error!));
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<InvoiceImportDto>>> Upload(
        [FromForm] UploadInvoiceImportForm form,
        CancellationToken cancellationToken)
    {
        if (form.File.Length == 0)
        {
            return BadRequest(ApiResponse<InvoiceImportDto>.Failure("Invoice file is required."));
        }

        await using var stream = form.File.OpenReadStream();
        var result = await invoiceImports.UploadAsync(
            new UploadInvoiceImportRequest(
                form.File.FileName,
                form.File.ContentType,
                form.File.Length,
                stream,
                form.SupplierName,
                form.SupplierTaxId,
                form.InvoiceNumber,
                form.InvoiceDate,
                form.Notes,
                CurrentActor()),
            cancellationToken);

        return result.Succeeded
            ? Created($"/api/v1/inventory/invoices/imports/{result.Data!.Id}", ApiResponse<InvoiceImportDto>.Success(result.Data))
            : BadRequest(ApiResponse<InvoiceImportDto>.Failure(result.Error!));
    }

    [HttpPost("imports/{id:guid}/lines")]
    public async Task<ActionResult<ApiResponse<InvoiceImportLineDto>>> AddLine(
        Guid id,
        CreateInvoiceImportLineRequest request,
        CancellationToken cancellationToken)
    {
        var result = await invoiceImports.AddLineAsync(id, request, cancellationToken);
        if (!result.Succeeded)
        {
            return ToLineError(result.Error!);
        }

        return Created(
            $"/api/v1/inventory/invoices/imports/{id}/lines/{result.Data!.Id}",
            ApiResponse<InvoiceImportLineDto>.Success(result.Data));
    }

    [HttpPut("imports/{id:guid}/lines/{lineId:guid}")]
    public async Task<ActionResult<ApiResponse<InvoiceImportLineDto>>> UpdateLine(
        Guid id,
        Guid lineId,
        UpdateInvoiceImportLineRequest request,
        CancellationToken cancellationToken)
    {
        var result = await invoiceImports.UpdateLineAsync(id, lineId, request, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<InvoiceImportLineDto>.Success(result.Data!))
            : ToLineError(result.Error!);
    }

    [HttpPost("imports/{id:guid}/confirm")]
    public async Task<ActionResult<ApiResponse<InvoiceImportDto>>> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var result = await invoiceImports.ConfirmAsync(id, new ConfirmInvoiceImportRequest(CurrentActor()), cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<InvoiceImportDto>.Success(result.Data!))
            : ToImportError(result.Error!);
    }

    [HttpPost("imports/{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<InvoiceImportDto>>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await invoiceImports.CancelAsync(id, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<InvoiceImportDto>.Success(result.Data!))
            : ToImportError(result.Error!);
    }

    private ActionResult<ApiResponse<InvoiceImportDto>> ToImportError(string error) =>
        error == "Invoice import not found."
            ? NotFound(ApiResponse<InvoiceImportDto>.Failure(error))
            : BadRequest(ApiResponse<InvoiceImportDto>.Failure(error));

    private ActionResult<ApiResponse<InvoiceImportLineDto>> ToLineError(string error) =>
        error is "Invoice import not found." or "Invoice import line not found." or "Inventory item not found."
            ? NotFound(ApiResponse<InvoiceImportLineDto>.Failure(error))
            : BadRequest(ApiResponse<InvoiceImportLineDto>.Failure(error));

    private string CurrentActor() =>
        User.FindFirstValue(ClaimTypes.Email) ??
        User.FindFirstValue("email") ??
        User.Identity?.Name ??
        "admin";
}

public sealed class UploadInvoiceImportForm
{
    public IFormFile File { get; set; } = default!;
    public string? SupplierName { get; set; }
    public string? SupplierTaxId { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTimeOffset? InvoiceDate { get; set; }
    public string? Notes { get; set; }
}
