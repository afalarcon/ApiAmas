using Amas.Application.Abstractions;
using Amas.Application.Common;
using Amas.Domain.Core;

namespace Amas.Application.Suppliers;

public sealed class SupplierService(ISupplierRepository suppliers) : ISupplierService
{
    public async Task<Result<IReadOnlyList<SupplierDto>>> ListAsync(CancellationToken cancellationToken)
    {
        var items = await suppliers.ListAsync(cancellationToken);
        return Result<IReadOnlyList<SupplierDto>>.Success(items.Select(Map).ToList());
    }

    public async Task<Result<SupplierDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await suppliers.GetByIdAsync(id, cancellationToken);
        return supplier is null
            ? Result<SupplierDto>.Failure("Supplier not found.")
            : Result<SupplierDto>.Success(Map(supplier));
    }

    public async Task<Result<SupplierDto>> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(request.Name, request.CategoryId, request.Status, cancellationToken);
        if (validation is not null)
        {
            return Result<SupplierDto>.Failure(validation);
        }

        var supplier = new Supplier();
        Apply(supplier, request.Name, request.TaxId, request.ContactName, request.Email, request.Phone, request.Address, request.City, request.Country, request.CategoryId, request.Status, request.Notes);

        await suppliers.AddAsync(supplier, cancellationToken);
        await suppliers.SaveChangesAsync(cancellationToken);

        return Result<SupplierDto>.Success(Map(supplier));
    }

    public async Task<Result<SupplierDto>> UpdateAsync(Guid id, UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        var supplier = await suppliers.GetByIdAsync(id, cancellationToken);
        if (supplier is null)
        {
            return Result<SupplierDto>.Failure("Supplier not found.");
        }

        var validation = await ValidateAsync(request.Name, request.CategoryId, request.Status, cancellationToken);
        if (validation is not null)
        {
            return Result<SupplierDto>.Failure(validation);
        }

        Apply(supplier, request.Name, request.TaxId, request.ContactName, request.Email, request.Phone, request.Address, request.City, request.Country, request.CategoryId, request.Status, request.Notes);
        await suppliers.SaveChangesAsync(cancellationToken);

        return Result<SupplierDto>.Success(Map(supplier));
    }

    private async Task<string?> ValidateAsync(string name, Guid? categoryId, string status, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Supplier name is required.";
        }

        if (categoryId.HasValue && !await suppliers.CategoryExistsAsync(categoryId.Value, cancellationToken))
        {
            return "Category not found.";
        }

        if (status is not SupplierStatuses.Active and not SupplierStatuses.NeedsCompletion and not SupplierStatuses.Inactive)
        {
            return "Supplier status is not valid.";
        }

        return null;
    }

    private static void Apply(
        Supplier supplier,
        string name,
        string? taxId,
        string? contactName,
        string? email,
        string? phone,
        string? address,
        string? city,
        string? country,
        Guid? categoryId,
        string status,
        string? notes)
    {
        supplier.Name = name.Trim();
        supplier.TaxId = Clean(taxId);
        supplier.ContactName = Clean(contactName);
        supplier.Email = Clean(email);
        supplier.Phone = Clean(phone);
        supplier.Address = Clean(address);
        supplier.City = Clean(city);
        supplier.Country = Clean(country);
        supplier.CategoryId = categoryId;
        supplier.Status = status;
        supplier.Notes = Clean(notes);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static SupplierDto Map(Supplier supplier) =>
        new(
            supplier.Id,
            supplier.SupplierNumber,
            supplier.Name,
            supplier.TaxId,
            supplier.ContactName,
            supplier.Email,
            supplier.Phone,
            supplier.Address,
            supplier.City,
            supplier.Country,
            supplier.CategoryId,
            supplier.Category?.Name,
            supplier.Status,
            supplier.Notes,
            supplier.CreatedAt,
            supplier.UpdatedAt);
}
