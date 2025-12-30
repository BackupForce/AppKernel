using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddNode : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "password_hash",
            schema: "public",
            table: "users",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateTable(
            name: "node",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_node", x => x.id));

        migrationBuilder.CreateTable(
            name: "role",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_role", x => x.id));

        migrationBuilder.CreateTable(
            name: "node_relation",
            schema: "public",
            columns: table => new
            {
                ancestor_node_id = table.Column<Guid>(type: "uuid", nullable: false),
                descendant_node_id = table.Column<Guid>(type: "uuid", nullable: false),
                depth = table.Column<int>(type: "integer", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_node_relation", x => new { x.ancestor_node_id, x.descendant_node_id });
                table.ForeignKey(
                    name: "fk_node_relation_node_ancestor_node_id",
                    column: x => x.ancestor_node_id,
                    principalSchema: "public",
                    principalTable: "node",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_node_relation_node_descendant_node_id",
                    column: x => x.descendant_node_id,
                    principalSchema: "public",
                    principalTable: "node",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "permission",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "text", nullable: false),
                role_id = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_permission", x => x.id);
                table.ForeignKey(
                    name: "fk_permission_role_role_id",
                    column: x => x.role_id,
                    principalSchema: "public",
                    principalTable: "role",
                    principalColumn: "id");
            });

        migrationBuilder.CreateTable(
            name: "role_user",
            schema: "public",
            columns: table => new
            {
                roles_id = table.Column<int>(type: "integer", nullable: false),
                users_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_role_user", x => new { x.roles_id, x.users_id });
                table.ForeignKey(
                    name: "fk_role_user_role_roles_id",
                    column: x => x.roles_id,
                    principalSchema: "public",
                    principalTable: "role",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_role_user_users_users_id",
                    column: x => x.users_id,
                    principalSchema: "public",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_node_relation_descendant_node_id",
            schema: "public",
            table: "node_relation",
            column: "descendant_node_id");

        migrationBuilder.CreateIndex(
            name: "ix_permission_role_id",
            schema: "public",
            table: "permission",
            column: "role_id");

        migrationBuilder.CreateIndex(
            name: "ix_role_user_users_id",
            schema: "public",
            table: "role_user",
            column: "users_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "node_relation",
            schema: "public");

        migrationBuilder.DropTable(
            name: "permission",
            schema: "public");

        migrationBuilder.DropTable(
            name: "role_user",
            schema: "public");

        migrationBuilder.DropTable(
            name: "node",
            schema: "public");

        migrationBuilder.DropTable(
            name: "role",
            schema: "public");

        migrationBuilder.DropColumn(
            name: "password_hash",
            schema: "public",
            table: "users");
    }
}
