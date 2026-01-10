using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class addLine : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "user_tenants",
            schema: "public");

        migrationBuilder.AddColumn<string>(
            name: "line_user_id",
            schema: "public",
            table: "users",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "normalized_email",
            schema: "public",
            table: "users",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "normalized_line_user_id",
            schema: "public",
            table: "users",
            type: "text",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "line_user_id",
            schema: "public",
            table: "users");

        migrationBuilder.DropColumn(
            name: "normalized_email",
            schema: "public",
            table: "users");

        migrationBuilder.DropColumn(
            name: "normalized_line_user_id",
            schema: "public",
            table: "users");

        migrationBuilder.CreateTable(
            name: "user_tenants",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_tenants", x => x.id);
                table.ForeignKey(
                    name: "fk_user_tenants_tenants_tenant_id",
                    column: x => x.tenant_id,
                    principalSchema: "public",
                    principalTable: "tenants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_user_tenants_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_user_tenants_tenant_id",
            schema: "public",
            table: "user_tenants",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_tenants_user_id_tenant_id",
            schema: "public",
            table: "user_tenants",
            columns: new[] { "user_id", "tenant_id" },
            unique: true);
    }
}
