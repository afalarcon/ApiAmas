using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Amas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductGalleryAndInventoryImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_product_images_ProductId",
                schema: "core",
                table: "product_images");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                schema: "core",
                table: "product_images",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                schema: "core",
                table: "product_images",
                type: "character varying(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                schema: "core",
                table: "product_images",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "SizeBytes",
                schema: "core",
                table: "product_images",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "StoragePath",
                schema: "core",
                table: "product_images",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StorageProvider",
                schema: "core",
                table: "product_images",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                schema: "core",
                table: "inventory_items",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                schema: "core",
                table: "inventory_items",
                type: "character varying(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ImageSizeBytes",
                schema: "core",
                table: "inventory_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageStoragePath",
                schema: "core",
                table: "inventory_items",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageStorageProvider",
                schema: "core",
                table: "inventory_items",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "core",
                table: "inventory_items",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "product_categories",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_categories_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "core",
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_categories_products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "core",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_images_ProductId_IsPrimary",
                schema: "core",
                table: "product_images",
                columns: new[] { "ProductId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_product_images_ProductId_SortOrder",
                schema: "core",
                table: "product_images",
                columns: new[] { "ProductId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_product_categories_CategoryId_ProductId",
                schema: "core",
                table: "product_categories",
                columns: new[] { "CategoryId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_categories_CategoryId_SortOrder",
                schema: "core",
                table: "product_categories",
                columns: new[] { "CategoryId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_product_categories_ProductId",
                schema: "core",
                table: "product_categories",
                column: "ProductId");

            migrationBuilder.Sql("""
                INSERT INTO core.product_categories ("Id", "ProductId", "CategoryId", "SortOrder", "IsFeatured", "CreatedAt", "UpdatedAt")
                SELECT gen_random_uuid(), product_data."Id", product_data."CategoryId", product_data."SortOrder", FALSE, NOW(), NULL
                FROM (
                    SELECT
                        p."Id",
                        p."CategoryId",
                        ROW_NUMBER() OVER (PARTITION BY p."CategoryId" ORDER BY p."Name", p."CreatedAt")::integer AS "SortOrder"
                    FROM core.products p
                    WHERE p."CategoryId" IS NOT NULL
                ) product_data
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM core.product_categories pc
                    WHERE pc."ProductId" = product_data."Id"
                      AND pc."CategoryId" = product_data."CategoryId"
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_categories",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "IX_product_images_ProductId_IsPrimary",
                schema: "core",
                table: "product_images");

            migrationBuilder.DropIndex(
                name: "IX_product_images_ProductId_SortOrder",
                schema: "core",
                table: "product_images");

            migrationBuilder.DropColumn(
                name: "ContentType",
                schema: "core",
                table: "product_images");

            migrationBuilder.DropColumn(
                name: "FileName",
                schema: "core",
                table: "product_images");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                schema: "core",
                table: "product_images");

            migrationBuilder.DropColumn(
                name: "SizeBytes",
                schema: "core",
                table: "product_images");

            migrationBuilder.DropColumn(
                name: "StoragePath",
                schema: "core",
                table: "product_images");

            migrationBuilder.DropColumn(
                name: "StorageProvider",
                schema: "core",
                table: "product_images");

            migrationBuilder.DropColumn(
                name: "ImageContentType",
                schema: "core",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "ImageFileName",
                schema: "core",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "ImageSizeBytes",
                schema: "core",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "ImageStoragePath",
                schema: "core",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "ImageStorageProvider",
                schema: "core",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "core",
                table: "inventory_items");

            migrationBuilder.CreateIndex(
                name: "IX_product_images_ProductId",
                schema: "core",
                table: "product_images",
                column: "ProductId");
        }
    }
}
