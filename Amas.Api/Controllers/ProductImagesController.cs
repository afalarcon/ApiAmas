using Amas.Api.Contracts;
using Amas.Application.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Amas.Api.Controllers;

[ApiController]
[Route("api/v1/products/{productId:guid}/images")]
public sealed class ProductImagesController(IProductImageService productImages) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductImageDto>>>> Get(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var result = await productImages.ListAsync(productId, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<IReadOnlyList<ProductImageDto>>.Success(result.Data!))
            : NotFound(ApiResponse<IReadOnlyList<ProductImageDto>>.Failure(result.Error!));
    }

    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductImageDto>>>> Upload(
        Guid productId,
        [FromForm] UploadProductImagesForm request,
        CancellationToken cancellationToken)
    {
        if (request.Files.Count == 0)
        {
            return BadRequest(ApiResponse<IReadOnlyList<ProductImageDto>>.Failure("At least one image is required."));
        }

        var streams = new List<Stream>(request.Files.Count);
        try
        {
            var files = request.Files.Select(file =>
            {
                var stream = file.OpenReadStream();
                streams.Add(stream);
                return new UploadProductImageFile(
                    file.FileName,
                    file.ContentType,
                    file.Length,
                    stream,
                    request.AltText);
            }).ToList();

            var result = await productImages.UploadAsync(productId, files, cancellationToken);
            if (!result.Succeeded)
            {
                return result.Error == "Product not found."
                    ? NotFound(ApiResponse<IReadOnlyList<ProductImageDto>>.Failure(result.Error))
                    : BadRequest(ApiResponse<IReadOnlyList<ProductImageDto>>.Failure(result.Error!));
            }

            return Created(
                $"/api/v1/products/{productId}/images",
                ApiResponse<IReadOnlyList<ProductImageDto>>.Success(result.Data!));
        }
        finally
        {
            foreach (var stream in streams)
            {
                await stream.DisposeAsync();
            }
        }
    }

    [HttpPut("reorder")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductImageDto>>>> Reorder(
        Guid productId,
        IReadOnlyList<ReorderProductImageRequest> request,
        CancellationToken cancellationToken)
    {
        var result = await productImages.ReorderAsync(productId, request, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<IReadOnlyList<ProductImageDto>>.Success(result.Data!))
            : BadRequest(ApiResponse<IReadOnlyList<ProductImageDto>>.Failure(result.Error!));
    }

    [HttpPut("{imageId:guid}/primary")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ProductImageDto>>> SetPrimary(
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var result = await productImages.SetPrimaryAsync(productId, imageId, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<ProductImageDto>.Success(result.Data!))
            : NotFound(ApiResponse<ProductImageDto>.Failure(result.Error!));
    }

    [HttpDelete("{imageId:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Delete(
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var result = await productImages.DeleteAsync(productId, imageId, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse.Success())
            : NotFound(ApiResponse.Failure(result.Error!));
    }
}

public sealed class UploadProductImagesForm
{
    public List<IFormFile> Files { get; init; } = [];
    public string? AltText { get; init; }
}
