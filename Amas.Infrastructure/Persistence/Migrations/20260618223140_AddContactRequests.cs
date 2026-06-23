using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Amas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContactRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contact_requests",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactRequestNumber = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Phone = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    RequestType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Message = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    SourcePage = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IpAddressHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CaptchaProvider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    CaptchaTokenProvided = table.Column<bool>(type: "boolean", nullable: false),
                    WebhookDelivered = table.Column<bool>(type: "boolean", nullable: false),
                    WebhookDeliveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WebhookError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact_requests", x => x.Id);
                });

            migrationBuilder.Sql("""
                ALTER TABLE "core"."contact_requests" ALTER COLUMN "ContactRequestNumber" RESTART WITH 1001;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_contact_requests_ContactRequestNumber",
                schema: "core",
                table: "contact_requests",
                column: "ContactRequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contact_requests_CreatedAt",
                schema: "core",
                table: "contact_requests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_contact_requests_Email",
                schema: "core",
                table: "contact_requests",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_contact_requests_Status",
                schema: "core",
                table: "contact_requests",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contact_requests",
                schema: "core");
        }
    }
}
