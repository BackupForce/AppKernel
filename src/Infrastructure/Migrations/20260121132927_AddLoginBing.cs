using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddLoginBing : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "line_user_id",
            schema: "public",
            table: "users");

        migrationBuilder.DropColumn(
            name: "normalized_line_user_id",
            schema: "public",
            table: "users");

        migrationBuilder.CreateTable(
            name: "user_login_bindings",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                provider = table.Column<int>(type: "integer", nullable: false),
                provider_key = table.Column<string>(type: "text", nullable: false),
                normalized_provider_key = table.Column<string>(type: "text", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_login_bindings", x => x.id);
                table.ForeignKey(
                    name: "fk_user_login_bindings_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ux_login_bindings_tenant_provider_key",
            schema: "public",
            table: "user_login_bindings",
            columns: new[] { "tenant_id", "provider", "normalized_provider_key" },
            unique: true,
            filter: "tenant_id IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ux_login_bindings_user_provider",
            schema: "public",
            table: "user_login_bindings",
            columns: new[] { "user_id", "provider" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "user_login_bindings",
            schema: "public");

        migrationBuilder.AddColumn<string>(
            name: "line_user_id",
            schema: "public",
            table: "users",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "normalized_line_user_id",
            schema: "public",
            table: "users",
            type: "text",
            nullable: true);
    }
}
