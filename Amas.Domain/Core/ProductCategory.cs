using Amas.Domain.Common;

namespace Amas.Domain.Core;

public sealed class ProductCategory : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public int SortOrder { get; set; }
    public bool IsFeatured { get; set; }
}
