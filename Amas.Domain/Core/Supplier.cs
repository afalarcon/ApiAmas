using Amas.Domain.Common;

namespace Amas.Domain.Core;

public sealed class Supplier : AuditableEntity
{
    public long SupplierNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public string Status { get; set; } = SupplierStatuses.Active;
    public string? Notes { get; set; }
    public List<InventoryInvoiceImport> InvoiceImports { get; set; } = [];
}

public static class SupplierStatuses
{
    public const string Active = "Active";
    public const string NeedsCompletion = "NeedsCompletion";
    public const string Inactive = "Inactive";
}
