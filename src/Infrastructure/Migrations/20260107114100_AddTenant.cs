using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddTenant : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "tenants",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_tenants", x => x.id));

        migrationBuilder.CreateIndex(
            name: "ix_user_tenants_tenant_id",
            schema: "public",
            table: "user_tenants",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_tenants_code",
            schema: "public",
            table: "tenants",
            column: "code",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "fk_user_tenants_tenants_tenant_id",
            schema: "public",
            table: "user_tenants",
            column: "tenant_id",
            principalSchema: "public",
            principalTable: "tenants",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_user_tenants_tenants_tenant_id",
            schema: "public",
            table: "user_tenants");

        migrationBuilder.DropTable(
            name: "tenants",
            schema: "public");

        migrationBuilder.DropIndex(
            name: "ix_user_tenants_tenant_id",
            schema: "public",
            table: "user_tenants");
    }
}
