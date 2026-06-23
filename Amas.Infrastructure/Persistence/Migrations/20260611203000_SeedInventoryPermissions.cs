using Amas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Amas.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AmasDbContext))]
[Migration("20260611203000_SeedInventoryPermissions")]
public partial class SeedInventoryPermissions : Migration
{
    private static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly (Guid Id, string Code, string Description)[] Permissions =
    [
        (Guid.Parse("22222222-2222-2222-2222-222222222219"), "inventory.read", "Ver Kardex de inventario y movimientos."),
        (Guid.Parse("22222222-2222-2222-2222-222222222220"), "inventory.invoices.read", "Ver y gestionar facturas de entrada.")
    ];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
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

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var permission in Permissions)
        {
            migrationBuilder.Sql($"""
                DELETE FROM identity.role_permissions WHERE "PermissionId" = '{permission.Id}';
                DELETE FROM identity.permissions WHERE "Id" = '{permission.Id}';
                """);
        }
    }
}
