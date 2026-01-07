using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class MdfyTenant : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_resource_nodes_external_key",
            schema: "public",
            table: "resource_nodes");

        migrationBuilder.AddColumn<Guid>(
            name: "tenant_id",
            schema: "public",
            table: "resource_nodes",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.AddColumn<Guid>(
            name: "tenant_id",
            schema: "public",
            table: "permission_assignments",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.CreateIndex(
            name: "ix_resource_nodes_tenant_id",
            schema: "public",
            table: "resource_nodes",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ux_resource_nodes_tenant_id_external_key",
            schema: "public",
            table: "resource_nodes",
            columns: new[] { "tenant_id", "external_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_permission_assignments_tenant_id",
            schema: "public",
            table: "permission_assignments",
            column: "tenant_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_resource_nodes_tenant_id",
            schema: "public",
            table: "resource_nodes");

        migrationBuilder.DropIndex(
            name: "ux_resource_nodes_tenant_id_external_key",
            schema: "public",
            table: "resource_nodes");

        migrationBuilder.DropIndex(
            name: "ix_permission_assignments_tenant_id",
            schema: "public",
            table: "permission_assignments");

        migrationBuilder.DropColumn(
            name: "tenant_id",
            schema: "public",
            table: "resource_nodes");

        migrationBuilder.DropColumn(
            name: "tenant_id",
            schema: "public",
            table: "permission_assignments");

        migrationBuilder.CreateIndex(
            name: "ix_resource_nodes_external_key",
            schema: "public",
            table: "resource_nodes",
            column: "external_key",
            unique: true);
    }
}
