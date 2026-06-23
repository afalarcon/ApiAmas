using Amas.Domain.Automation;
using Amas.Domain.Common;
using Amas.Domain.Core;
using Amas.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Amas.Infrastructure.Persistence;

public sealed class AmasDbContext(DbContextOptions<AmasDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryImage> CategoryImages => Set<CategoryImage>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<InventoryInvoiceImport> InventoryInvoiceImports => Set<InventoryInvoiceImport>();
    public DbSet<InventoryInvoiceImportLine> InventoryInvoiceImportLines => Set<InventoryInvoiceImportLine>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Configuration> Configurations => Set<Configuration>();
    public DbSet<WorkflowEvent> WorkflowEvents => Set<WorkflowEvent>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("core");

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", "identity");
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(180).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(180).IsRequired();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles", "identity");
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions", "identity");
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens", "identity");
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.Property(x => x.TokenHash).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles", "identity");
            entity.HasKey(x => new { x.UserId, x.RoleId });
            entity.HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions", "identity");
            entity.HasKey(x => new { x.RoleId, x.PermissionId });
            entity.HasOne(x => x.Role)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Permission)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.Property(x => x.CategoryNumber).UseIdentityByDefaultColumn();
            entity.HasIndex(x => x.CategoryNumber).IsUnique();
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(140).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
        });

        modelBuilder.Entity<CategoryImage>(entity =>
        {
            entity.ToTable("category_images");
            entity.Property(x => x.Url).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.StorageProvider).HasMaxLength(80).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(120).IsRequired();
            entity.Property(x => x.AltText).HasMaxLength(250);
            entity.HasIndex(x => new { x.CategoryId, x.SortOrder });
            entity.HasOne(x => x.Category)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.Property(x => x.ProductNumber).UseIdentityByDefaultColumn();
            entity.HasIndex(x => x.ProductNumber).IsUnique();
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(220).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.Sku).HasMaxLength(80);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("product_categories");
            entity.HasIndex(x => new { x.CategoryId, x.ProductId }).IsUnique();
            entity.HasIndex(x => new { x.CategoryId, x.SortOrder });
            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductCategories)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Category)
                .WithMany(x => x.ProductCategories)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("product_images");
            entity.Property(x => x.Url).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.StorageProvider).HasMaxLength(80).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(120).IsRequired();
            entity.Property(x => x.AltText).HasMaxLength(250);
            entity.HasIndex(x => new { x.ProductId, x.SortOrder });
            entity.HasIndex(x => new { x.ProductId, x.IsPrimary });
            entity.HasOne(x => x.Product)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ContactRequest>(entity =>
        {
            entity.ToTable("contact_requests");
            entity.Property(x => x.ContactRequestNumber)
                .UseIdentityByDefaultColumn()
                .HasIdentityOptions(startValue: 1001);
            entity.HasIndex(x => x.ContactRequestNumber).IsUnique();
            entity.HasIndex(x => x.Email);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.CreatedAt);
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(80);
            entity.Property(x => x.RequestType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(1200).IsRequired();
            entity.Property(x => x.SourcePage).HasMaxLength(260).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.IpAddressHash).HasMaxLength(128);
            entity.Property(x => x.UserAgent).HasMaxLength(500);
            entity.Property(x => x.CaptchaProvider).HasMaxLength(80);
            entity.Property(x => x.WebhookError).HasMaxLength(1000);
            entity.Property(x => x.ReviewedBy).HasMaxLength(180);
            entity.Property(x => x.Notes).HasMaxLength(1000);
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.ToTable("inventory_items");
            entity.Property(x => x.InventoryItemNumber).UseIdentityByDefaultColumn();
            entity.HasIndex(x => x.InventoryItemNumber).IsUnique();
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.HasIndex(x => x.ProductId).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Sku).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Unit).HasMaxLength(40).IsRequired();
            entity.Property(x => x.CurrentStock).HasPrecision(18, 3);
            entity.Property(x => x.MinimumStock).HasPrecision(18, 3);
            entity.Property(x => x.ImageUrl).HasMaxLength(1000);
            entity.Property(x => x.ImageStoragePath).HasMaxLength(1000);
            entity.Property(x => x.ImageStorageProvider).HasMaxLength(80);
            entity.Property(x => x.ImageFileName).HasMaxLength(260);
            entity.Property(x => x.ImageContentType).HasMaxLength(120);
            entity.HasOne(x => x.Product)
                .WithOne(x => x.InventoryItem)
                .HasForeignKey<InventoryItem>(x => x.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.ToTable("inventory_movements");
            entity.Property(x => x.InventoryMovementNumber).UseIdentityByDefaultColumn();
            entity.HasIndex(x => x.InventoryMovementNumber).IsUnique();
            entity.Property(x => x.MovementType).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.StockAfter).HasPrecision(18, 3);
            entity.Property(x => x.UnitCost).HasPrecision(18, 2);
            entity.Property(x => x.Reason).HasMaxLength(500);
            entity.Property(x => x.Reference).HasMaxLength(160);
            entity.HasIndex(x => new { x.InventoryItemId, x.OccurredAt });
            entity.HasOne(x => x.InventoryItem)
                .WithMany(x => x.Movements)
                .HasForeignKey(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("suppliers");
            entity.Property(x => x.SupplierNumber).UseIdentityByDefaultColumn();
            entity.HasIndex(x => x.SupplierNumber).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.TaxId).HasMaxLength(80);
            entity.Property(x => x.ContactName).HasMaxLength(180);
            entity.Property(x => x.Email).HasMaxLength(180);
            entity.Property(x => x.Phone).HasMaxLength(80);
            entity.Property(x => x.Address).HasMaxLength(260);
            entity.Property(x => x.City).HasMaxLength(120);
            entity.Property(x => x.Country).HasMaxLength(120);
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.TaxId).IsUnique();
            entity.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InventoryInvoiceImport>(entity =>
        {
            entity.ToTable("inventory_invoice_imports");
            entity.Property(x => x.InvoiceImportNumber).UseIdentityByDefaultColumn();
            entity.HasIndex(x => x.InvoiceImportNumber).IsUnique();
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.OriginalFileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.StoredFileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(120).IsRequired();
            entity.Property(x => x.StorageProvider).HasMaxLength(80).IsRequired();
            entity.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Url).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.SupplierName).HasMaxLength(180);
            entity.Property(x => x.SupplierTaxId).HasMaxLength(80);
            entity.Property(x => x.InvoiceNumber).HasMaxLength(120);
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.TaxTotal).HasPrecision(18, 2);
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.ExtractionProvider).HasMaxLength(80);
            entity.Property(x => x.ExtractedJson).HasColumnType("jsonb");
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.CreatedBy).HasMaxLength(180).IsRequired();
            entity.Property(x => x.ConfirmedBy).HasMaxLength(180);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.SupplierId);
            entity.HasIndex(x => x.InvoiceNumber);
            entity.HasOne(x => x.Supplier)
                .WithMany(x => x.InvoiceImports)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InventoryInvoiceImportLine>(entity =>
        {
            entity.ToTable("inventory_invoice_import_lines");
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.MatchStatus).HasMaxLength(40).IsRequired();
            entity.Property(x => x.RawText).HasMaxLength(1000);
            entity.Property(x => x.ExtractedSku).HasMaxLength(120);
            entity.Property(x => x.ExtractedName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitCost).HasPrecision(18, 2);
            entity.Property(x => x.TaxPercent).HasPrecision(7, 3);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => new { x.InventoryInvoiceImportId, x.LineNumber }).IsUnique();
            entity.HasOne(x => x.InventoryInvoiceImport)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.InventoryInvoiceImportId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.InventoryItem)
                .WithMany()
                .HasForeignKey(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.Property(x => x.FullName).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(180);
            entity.Property(x => x.Phone).HasMaxLength(80);
        });

        modelBuilder.Entity<Quote>(entity =>
        {
            entity.ToTable("quotes");
            entity.Property(x => x.Status).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Total).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.Property(x => x.Status).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Total).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.ToTable("configurations");
            entity.HasIndex(x => x.Key).IsUnique();
            entity.Property(x => x.Key).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Value).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
        });

        modelBuilder.Entity<WorkflowEvent>(entity =>
        {
            entity.ToTable("workflow_events", "automation");
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications", "automation");
            entity.Property(x => x.Channel).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Recipient).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(2000).IsRequired();
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs", "automation");
            entity.Property(x => x.Actor).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(180).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(180).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(80);
            entity.Property(x => x.DetailsJson).HasColumnType("jsonb");
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
