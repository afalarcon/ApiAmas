using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Amas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceExtractionJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtractedJson",
                schema: "core",
                table: "inventory_invoice_imports",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractionProvider",
                schema: "core",
                table: "inventory_invoice_imports",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                schema: "core",
                table: "inventory_invoice_import_lines",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxPercent",
                schema: "core",
                table: "inventory_invoice_import_lines",
                type: "numeric(7,3)",
                precision: 7,
                scale: 3,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtractedJson",
                schema: "core",
                table: "inventory_invoice_imports");

            migrationBuilder.DropColumn(
                name: "ExtractionProvider",
                schema: "core",
                table: "inventory_invoice_imports");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                schema: "core",
                table: "inventory_invoice_import_lines");

            migrationBuilder.DropColumn(
                name: "TaxPercent",
                schema: "core",
                table: "inventory_invoice_import_lines");
        }
    }
}
