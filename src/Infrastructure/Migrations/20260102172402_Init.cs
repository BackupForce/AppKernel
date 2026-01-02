using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class Init : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "public");

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
            name: "outbox_messages",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                content = table.Column<string>(type: "jsonb", nullable: false),
                created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                error = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table => table.PrimaryKey("pk_outbox_messages", x => x.id));

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
            name: "users",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                password_hash = table.Column<string>(type: "text", nullable: false),
                has_public_profile = table.Column<bool>(type: "boolean", nullable: false),
                email = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_users", x => x.id));

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
                description = table.Column<string>(type: "text", nullable: false),
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
            name: "members",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: true),
                member_no = table.Column<string>(type: "text", nullable: false),
                display_name = table.Column<string>(type: "text", nullable: false),
                status = table.Column<short>(type: "smallint", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_members", x => x.id);
                table.ForeignKey(
                    name: "fk_members_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "users",
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

        migrationBuilder.CreateTable(
            name: "member_activity_log",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "text", nullable: false),
                ip = table.Column<string>(type: "text", nullable: true),
                user_agent = table.Column<string>(type: "text", nullable: true),
                operator_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                payload = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_member_activity_log", x => x.id);
                table.ForeignKey(
                    name: "fk_member_activity_log_members_member_id",
                    column: x => x.member_id,
                    principalSchema: "public",
                    principalTable: "members",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "member_asset_balance",
            schema: "public",
            columns: table => new
            {
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                asset_code = table.Column<string>(type: "text", nullable: false),
                balance = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_member_asset_balance", x => new { x.member_id, x.asset_code });
                table.ForeignKey(
                    name: "fk_member_asset_balance_members_member_id",
                    column: x => x.member_id,
                    principalSchema: "public",
                    principalTable: "members",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "member_asset_ledger",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                asset_code = table.Column<string>(type: "text", nullable: false),
                type = table.Column<short>(type: "smallint", nullable: false),
                amount = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                before_balance = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                after_balance = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                reference_type = table.Column<string>(type: "text", nullable: true),
                reference_id = table.Column<string>(type: "text", nullable: true),
                operator_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                remark = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_member_asset_ledger", x => x.id);
                table.ForeignKey(
                    name: "fk_member_asset_ledger_members_member_id",
                    column: x => x.member_id,
                    principalSchema: "public",
                    principalTable: "members",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "member_point_balance",
            schema: "public",
            columns: table => new
            {
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                balance = table.Column<long>(type: "bigint", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_member_point_balance", x => x.member_id);
                table.ForeignKey(
                    name: "fk_member_point_balance_members_member_id",
                    column: x => x.member_id,
                    principalSchema: "public",
                    principalTable: "members",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "member_point_ledger",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<short>(type: "smallint", nullable: false),
                amount = table.Column<long>(type: "bigint", nullable: false),
                before_balance = table.Column<long>(type: "bigint", nullable: false),
                after_balance = table.Column<long>(type: "bigint", nullable: false),
                reference_type = table.Column<string>(type: "text", nullable: true),
                reference_id = table.Column<string>(type: "text", nullable: true),
                operator_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                remark = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_member_point_ledger", x => x.id);
                table.ForeignKey(
                    name: "fk_member_point_ledger_members_member_id",
                    column: x => x.member_id,
                    principalSchema: "public",
                    principalTable: "members",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_member_activity_log_action_created_at",
            schema: "public",
            table: "member_activity_log",
            columns: new[] { "action", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_member_activity_log_member_id_created_at",
            schema: "public",
            table: "member_activity_log",
            columns: new[] { "member_id", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_member_asset_ledger_member_id_asset_code_created_at",
            schema: "public",
            table: "member_asset_ledger",
            columns: new[] { "member_id", "asset_code", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_member_point_ledger_member_id_created_at",
            schema: "public",
            table: "member_point_ledger",
            columns: new[] { "member_id", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_member_point_ledger_reference_type_reference_id",
            schema: "public",
            table: "member_point_ledger",
            columns: new[] { "reference_type", "reference_id" });

        migrationBuilder.CreateIndex(
            name: "ix_members_member_no",
            schema: "public",
            table: "members",
            column: "member_no",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_members_user_id",
            schema: "public",
            table: "members",
            column: "user_id",
            unique: true,
            filter: "user_id IS NOT NULL");

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
            name: "member_activity_log",
            schema: "public");

        migrationBuilder.DropTable(
            name: "member_asset_balance",
            schema: "public");

        migrationBuilder.DropTable(
            name: "member_asset_ledger",
            schema: "public");

        migrationBuilder.DropTable(
            name: "member_point_balance",
            schema: "public");

        migrationBuilder.DropTable(
            name: "member_point_ledger",
            schema: "public");

        migrationBuilder.DropTable(
            name: "node_relation",
            schema: "public");

        migrationBuilder.DropTable(
            name: "outbox_messages",
            schema: "public");

        migrationBuilder.DropTable(
            name: "permission",
            schema: "public");

        migrationBuilder.DropTable(
            name: "role_user",
            schema: "public");

        migrationBuilder.DropTable(
            name: "members",
            schema: "public");

        migrationBuilder.DropTable(
            name: "node",
            schema: "public");

        migrationBuilder.DropTable(
            name: "role",
            schema: "public");

        migrationBuilder.DropTable(
            name: "users",
            schema: "public");
    }
}
