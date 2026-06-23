using Amas.Api.Contracts;
using Amas.Application.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Amas.Api.Controllers;

[ApiController]
[Route("api/v1/inventory")]
public sealed class InventoryController(IInventoryService inventory) : ControllerBase
{
    [HttpGet("items")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InventoryItemDto>>>> GetItems(CancellationToken cancellationToken)
    {
        var result = await inventory.ListItemsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<InventoryItemDto>>.Success(result.Data!));
    }

    [HttpGet("items/{id:guid}")]
    public async Task<ActionResult<ApiResponse<InventoryItemDto>>> GetItem(Guid id, CancellationToken cancellationToken)
    {
        var result = await inventory.GetItemByIdAsync(id, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<InventoryItemDto>.Success(result.Data!))
            : NotFound(ApiResponse<InventoryItemDto>.Failure(result.Error!));
    }

    [HttpPost("items")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<InventoryItemDto>>> CreateItem(
        CreateInventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await inventory.CreateItemAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Error == "Product not found."
                ? NotFound(ApiResponse<InventoryItemDto>.Failure(result.Error))
                : BadRequest(ApiResponse<InventoryItemDto>.Failure(result.Error!));
        }

        return CreatedAtAction(nameof(GetItem), new { id = result.Data!.Id }, ApiResponse<InventoryItemDto>.Success(result.Data));
    }

    [HttpPut("items/{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<InventoryItemDto>>> UpdateItem(
        Guid id,
        UpdateInventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await inventory.UpdateItemAsync(id, request, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Error is "Inventory item not found." or "Product not found."
                ? NotFound(ApiResponse<InventoryItemDto>.Failure(result.Error))
                : BadRequest(ApiResponse<InventoryItemDto>.Failure(result.Error!));
        }

        return Ok(ApiResponse<InventoryItemDto>.Success(result.Data!));
    }

    [HttpPut("items/{id:guid}/image")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<InventoryItemDto>>> UploadItemImage(
        Guid id,
        [FromForm] UploadInventoryItemImageForm request,
        CancellationToken cancellationToken)
    {
        if (request.File is null)
        {
            return BadRequest(ApiResponse<InventoryItemDto>.Failure("Image is required."));
        }

        await using var stream = request.File.OpenReadStream();
        var result = await inventory.UploadItemImageAsync(
            id,
            new UploadInventoryItemImageFile(
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                stream),
            cancellationToken);

        if (!result.Succeeded)
        {
            return result.Error == "Inventory item not found."
                ? NotFound(ApiResponse<InventoryItemDto>.Failure(result.Error))
                : BadRequest(ApiResponse<InventoryItemDto>.Failure(result.Error!));
        }

        return Ok(ApiResponse<InventoryItemDto>.Success(result.Data!));
    }

    [HttpDelete("items/{id:guid}/image")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<InventoryItemDto>>> DeleteItemImage(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await inventory.DeleteItemImageAsync(id, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<InventoryItemDto>.Success(result.Data!))
            : NotFound(ApiResponse<InventoryItemDto>.Failure(result.Error!));
    }

    [HttpGet("items/{id:guid}/movements")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InventoryMovementDto>>>> GetMovements(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await inventory.ListMovementsAsync(id, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<IReadOnlyList<InventoryMovementDto>>.Success(result.Data!))
            : NotFound(ApiResponse<IReadOnlyList<InventoryMovementDto>>.Failure(result.Error!));
    }

    [HttpPost("items/{id:guid}/movements")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<InventoryMovementDto>>> CreateMovement(
        Guid id,
        CreateInventoryMovementRequest request,
        CancellationToken cancellationToken)
    {
        var result = await inventory.CreateMovementAsync(id, request, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Error == "Inventory item not found."
                ? NotFound(ApiResponse<InventoryMovementDto>.Failure(result.Error))
                : BadRequest(ApiResponse<InventoryMovementDto>.Failure(result.Error!));
        }

        return Created(
            $"/api/v1/inventory/items/{id}/movements/{result.Data!.Id}",
            ApiResponse<InventoryMovementDto>.Success(result.Data));
    }
}

public sealed class UploadInventoryItemImageForm
{
    public IFormFile? File { get; init; }
}
