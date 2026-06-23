using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Amas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisibleNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddVisibleNumber(migrationBuilder, "core", "suppliers", "SupplierNumber", "suppliers_suppliernumber_seq");
            AddVisibleNumber(migrationBuilder, "core", "products", "ProductNumber", "products_productnumber_seq");
            AddVisibleNumber(migrationBuilder, "core", "inventory_movements", "InventoryMovementNumber", "inventory_movements_inventorymovementnumber_seq");
            AddVisibleNumber(migrationBuilder, "core", "inventory_items", "InventoryItemNumber", "inventory_items_inventoryitemnumber_seq");
            AddVisibleNumber(migrationBuilder, "core", "inventory_invoice_imports", "InvoiceImportNumber", "inventory_invoice_imports_invoiceimportnumber_seq");
            AddVisibleNumber(migrationBuilder, "core", "categories", "CategoryNumber", "categories_categorynumber_seq");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_SupplierNumber",
                schema: "core",
                table: "suppliers",
                column: "SupplierNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_ProductNumber",
                schema: "core",
                table: "products",
                column: "ProductNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventory_movements_InventoryMovementNumber",
                schema: "core",
                table: "inventory_movements",
                column: "InventoryMovementNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventory_items_InventoryItemNumber",
                schema: "core",
                table: "inventory_items",
                column: "InventoryItemNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventory_invoice_imports_InvoiceImportNumber",
                schema: "core",
                table: "inventory_invoice_imports",
                column: "InvoiceImportNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_CategoryNumber",
                schema: "core",
                table: "categories",
                column: "CategoryNumber",
                unique: true);
        }

        private static void AddVisibleNumber(
            MigrationBuilder migrationBuilder,
            string schema,
            string table,
            string column,
            string sequence)
        {
            migrationBuilder.Sql($"""
                ALTER TABLE "{schema}"."{table}" ADD COLUMN "{column}" bigint;
                CREATE SEQUENCE "{schema}"."{sequence}" START WITH 1001 INCREMENT BY 1;

                WITH numbered AS (
                    SELECT "Id", nextval('"{schema}"."{sequence}"') AS value
                    FROM "{schema}"."{table}"
                    ORDER BY "CreatedAt", "Id"
                )
                UPDATE "{schema}"."{table}" target
                SET "{column}" = numbered.value
                FROM numbered
                WHERE target."Id" = numbered."Id";

                ALTER TABLE "{schema}"."{table}" ALTER COLUMN "{column}" SET NOT NULL;
                ALTER TABLE "{schema}"."{table}" ALTER COLUMN "{column}" SET DEFAULT nextval('"{schema}"."{sequence}"');
                ALTER SEQUENCE "{schema}"."{sequence}" OWNED BY "{schema}"."{table}"."{column}";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_suppliers_SupplierNumber",
                schema: "core",
                table: "suppliers");

            migrationBuilder.DropIndex(
                name: "IX_products_ProductNumber",
                schema: "core",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_inventory_movements_InventoryMovementNumber",
                schema: "core",
                table: "inventory_movements");

            migrationBuilder.DropIndex(
                name: "IX_inventory_items_InventoryItemNumber",
                schema: "core",
                table: "inventory_items");

            migrationBuilder.DropIndex(
                name: "IX_inventory_invoice_imports_InvoiceImportNumber",
                schema: "core",
                table: "inventory_invoice_imports");

            migrationBuilder.DropIndex(
                name: "IX_categories_CategoryNumber",
                schema: "core",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "SupplierNumber",
                schema: "core",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "ProductNumber",
                schema: "core",
                table: "products");

            migrationBuilder.DropColumn(
                name: "InventoryMovementNumber",
                schema: "core",
                table: "inventory_movements");

            migrationBuilder.DropColumn(
                name: "InventoryItemNumber",
                schema: "core",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "InvoiceImportNumber",
                schema: "core",
                table: "inventory_invoice_imports");

            migrationBuilder.DropColumn(
                name: "CategoryNumber",
                schema: "core",
                table: "categories");
        }
    }
}
