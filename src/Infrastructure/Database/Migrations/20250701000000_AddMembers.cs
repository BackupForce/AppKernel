using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddMembers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ⚠️ 注意：Migration 為不可逆變更，請在部署前確認備份與變更窗口。
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
            unique: true);
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
            name: "members",
            schema: "public");
    }
}
