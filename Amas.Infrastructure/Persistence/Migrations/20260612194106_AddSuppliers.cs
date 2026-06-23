using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Amas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSuppliers : Migration
    {
        private static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        private static readonly (Guid Id, string Code, string Description)[] Permissions =
        [
            (Guid.Parse("22222222-2222-2222-2222-222222222221"), "suppliers.read", "Ver proveedores."),
            (Guid.Parse("22222222-2222-2222-2222-222222222222"), "suppliers.create", "Crear proveedores."),
            (Guid.Parse("22222222-2222-2222-2222-222222222223"), "suppliers.update", "Actualizar proveedores.")
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId",
                schema: "core",
                table: "inventory_invoice_imports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierTaxId",
                schema: "core",
                table: "inventory_invoice_imports",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "suppliers",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    TaxId = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ContactName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    Email = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    Phone = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Address = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    City = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Country = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_suppliers_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "core",
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inventory_invoice_imports_SupplierId",
                schema: "core",
                table: "inventory_invoice_imports",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_CategoryId",
                schema: "core",
                table: "suppliers",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_Name",
                schema: "core",
                table: "suppliers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_TaxId",
                schema: "core",
                table: "suppliers",
                column: "TaxId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_inventory_invoice_imports_suppliers_SupplierId",
                schema: "core",
                table: "inventory_invoice_imports",
                column: "SupplierId",
                principalSchema: "core",
                principalTable: "suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            foreach (var permission in Permissions)
            {
                migrationBuilder.Sql($"""
                    INSERT INTO identity.permissions ("Id", "Code", "Description", "CreatedAt", "UpdatedAt")
                    VALUES ('{permission.Id}', '{permission.Code}', '{permission.Description}', NOW(), NULL)
                    ON CONFLICT ("Id") DO NOTHING;

                    INSERT INTO identity.role_permissions ("RoleId", "PermissionId")
                    VALUES ('{AdminRoleId}', '{permission.Id}')
                    ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var permission in Permissions)
            {
                migrationBuilder.Sql($"""
                    DELETE FROM identity.role_permissions WHERE "PermissionId" = '{permission.Id}';
                    DELETE FROM identity.permissions WHERE "Id" = '{permission.Id}';
                    """);
            }

            migrationBuilder.DropForeignKey(
                name: "FK_inventory_invoice_imports_suppliers_SupplierId",
                schema: "core",
                table: "inventory_invoice_imports");

            migrationBuilder.DropTable(
                name: "suppliers",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "IX_inventory_invoice_imports_SupplierId",
                schema: "core",
                table: "inventory_invoice_imports");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                schema: "core",
                table: "inventory_invoice_imports");

            migrationBuilder.DropColumn(
                name: "SupplierTaxId",
                schema: "core",
                table: "inventory_invoice_imports");
        }
    }
}
