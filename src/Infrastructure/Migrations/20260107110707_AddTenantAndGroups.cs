using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddTenantAndGroups : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "groups",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                external_key = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_groups", x => x.id));

        migrationBuilder.CreateTable(
            name: "resource_nodes",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                external_key = table.Column<string>(type: "text", nullable: false),
                parent_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_resource_nodes", x => x.id);
                table.ForeignKey(
                    name: "fk_resource_nodes_resource_nodes_parent_id",
                    column: x => x.parent_id,
                    principalSchema: "public",
                    principalTable: "resource_nodes",
                    principalColumn: "id");
            });

        migrationBuilder.CreateTable(
            name: "user_tenants",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_tenants", x => x.id);
                table.ForeignKey(
                    name: "fk_user_tenants_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_groups",
            schema: "public",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                group_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_groups", x => new { x.user_id, x.group_id });
                table.ForeignKey(
                    name: "fk_user_groups_groups_group_id",
                    column: x => x.group_id,
                    principalSchema: "public",
                    principalTable: "groups",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_user_groups_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "permission_assignments",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                subject_type = table.Column<int>(type: "integer", nullable: false),
                decision = table.Column<int>(type: "integer", nullable: false),
                subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                permission_code = table.Column<string>(type: "text", nullable: false),
                node_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_permission_assignments", x => x.id);
                table.ForeignKey(
                    name: "fk_permission_assignments_resource_nodes_node_id",
                    column: x => x.node_id,
                    principalSchema: "public",
                    principalTable: "resource_nodes",
                    principalColumn: "id");
            });

        migrationBuilder.CreateIndex(
            name: "ix_permission_assignments_node_id",
            schema: "public",
            table: "permission_assignments",
            column: "node_id");

        migrationBuilder.CreateIndex(
            name: "ix_permission_assignments_permission_code",
            schema: "public",
            table: "permission_assignments",
            column: "permission_code");

        migrationBuilder.CreateIndex(
            name: "ix_permission_assignments_subject_type_subject_id",
            schema: "public",
            table: "permission_assignments",
            columns: new[] { "subject_type", "subject_id" });

        migrationBuilder.CreateIndex(
            name: "ix_resource_nodes_external_key",
            schema: "public",
            table: "resource_nodes",
            column: "external_key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_resource_nodes_parent_id",
            schema: "public",
            table: "resource_nodes",
            column: "parent_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_groups_group_id",
            schema: "public",
            table: "user_groups",
            column: "group_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_groups_user_id_group_id",
            schema: "public",
            table: "user_groups",
            columns: new[] { "user_id", "group_id" });

        migrationBuilder.CreateIndex(
            name: "ix_user_tenants_user_id_tenant_id",
            schema: "public",
            table: "user_tenants",
            columns: new[] { "user_id", "tenant_id" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "permission_assignments",
            schema: "public");

        migrationBuilder.DropTable(
            name: "user_groups",
            schema: "public");

        migrationBuilder.DropTable(
            name: "user_tenants",
            schema: "public");

        migrationBuilder.DropTable(
            name: "resource_nodes",
            schema: "public");

        migrationBuilder.DropTable(
            name: "groups",
            schema: "public");
    }
}
