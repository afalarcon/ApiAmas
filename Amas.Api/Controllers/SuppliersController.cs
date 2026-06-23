using Amas.Api.Contracts;
using Amas.Application.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Amas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/suppliers")]
public sealed class SuppliersController(ISupplierService suppliers) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SupplierDto>>>> Get(CancellationToken cancellationToken)
    {
        var result = await suppliers.ListAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SupplierDto>>.Success(result.Data!));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await suppliers.GetByIdAsync(id, cancellationToken);
        return result.Succeeded
            ? Ok(ApiResponse<SupplierDto>.Success(result.Data!))
            : NotFound(ApiResponse<SupplierDto>.Failure(result.Error!));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> Create(CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        var result = await suppliers.CreateAsync(request, cancellationToken);
        return result.Succeeded
            ? Created($"/api/v1/suppliers/{result.Data!.Id}", ApiResponse<SupplierDto>.Success(result.Data))
            : BadRequest(ApiResponse<SupplierDto>.Failure(result.Error!));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> Update(Guid id, UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        var result = await suppliers.UpdateAsync(id, request, cancellationToken);
        if (!result.Succeeded)
        {
            return result.Error == "Supplier not found."
                ? NotFound(ApiResponse<SupplierDto>.Failure(result.Error))
                : BadRequest(ApiResponse<SupplierDto>.Failure(result.Error!));
        }

        return Ok(ApiResponse<SupplierDto>.Success(result.Data!));
    }
}
