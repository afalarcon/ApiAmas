using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Amas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceImports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inventory_invoice_imports",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    StoredFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageProvider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SupplierName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    InvoiceDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TaxTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConfirmedBy = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_invoice_imports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_invoice_import_lines",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryInvoiceImportId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    MatchStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    MatchConfidence = table.Column<int>(type: "integer", nullable: false),
                    RawText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ExtractedSku = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ExtractedName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_invoice_import_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inventory_invoice_import_lines_inventory_invoice_imports_In~",
                        column: x => x.InventoryInvoiceImportId,
                        principalSchema: "core",
                        principalTable: "inventory_invoice_imports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_inventory_invoice_import_lines_inventory_items_InventoryIte~",
                        column: x => x.InventoryItemId,
                        principalSchema: "core",
                        principalTable: "inventory_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inventory_invoice_import_lines_InventoryInvoiceImportId_Lin~",
                schema: "core",
                table: "inventory_invoice_import_lines",
                columns: new[] { "InventoryInvoiceImportId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventory_invoice_import_lines_InventoryItemId",
                schema: "core",
                table: "inventory_invoice_import_lines",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_invoice_imports_InvoiceNumber",
                schema: "core",
                table: "inventory_invoice_imports",
                column: "InvoiceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_invoice_imports_Status",
                schema: "core",
                table: "inventory_invoice_imports",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventory_invoice_import_lines",
                schema: "core");

            migrationBuilder.DropTable(
                name: "inventory_invoice_imports",
                schema: "core");
        }
    }
}
