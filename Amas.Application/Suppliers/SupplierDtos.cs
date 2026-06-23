namespace Amas.Application.Suppliers;

public sealed record SupplierDto(
    Guid Id,
    long SupplierNumber,
    string Name,
    string? TaxId,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country,
    Guid? CategoryId,
    string? CategoryName,
    string Status,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record CreateSupplierRequest(
    string Name,
    string? TaxId,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country,
    Guid? CategoryId,
    string Status,
    string? Notes);

public sealed record UpdateSupplierRequest(
    string Name,
    string? TaxId,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country,
    Guid? CategoryId,
    string Status,
    string? Notes);
