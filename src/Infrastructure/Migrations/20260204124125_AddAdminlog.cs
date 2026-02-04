using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddAdminlog : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "admin");

        migrationBuilder.AlterColumn<DateTime>(
            name: "grant_open_at_utc",
            schema: "gaming",
            table: "draw_groups",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");

        migrationBuilder.AlterColumn<DateTime>(
            name: "grant_close_at_utc",
            schema: "gaming",
            table: "draw_groups",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");

        migrationBuilder.AddColumn<DateTime>(
            name: "disabled_at_utc",
            schema: "gaming",
            table: "draw_groups",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "enabled_at_utc",
            schema: "gaming",
            table: "draw_groups",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "admin_operation_logs",
            schema: "admin",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                target_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                target_id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                operator_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                operator_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                dedup_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_admin_operation_logs", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_admin_operation_logs_dedup_key",
            schema: "admin",
            table: "admin_operation_logs",
            column: "dedup_key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_admin_operation_logs_tenant_id_operator_user_id_occurred_at",
            schema: "admin",
            table: "admin_operation_logs",
            columns: new[] { "tenant_id", "operator_user_id", "occurred_at_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_admin_operation_logs_tenant_id_target_type_target_id_occurr",
            schema: "admin",
            table: "admin_operation_logs",
            columns: new[] { "tenant_id", "target_type", "target_id", "occurred_at_utc" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "admin_operation_logs",
            schema: "admin");

        migrationBuilder.DropColumn(
            name: "disabled_at_utc",
            schema: "gaming",
            table: "draw_groups");

        migrationBuilder.DropColumn(
            name: "enabled_at_utc",
            schema: "gaming",
            table: "draw_groups");

        migrationBuilder.AlterColumn<DateTime>(
            name: "grant_open_at_utc",
            schema: "gaming",
            table: "draw_groups",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "grant_close_at_utc",
            schema: "gaming",
            table: "draw_groups",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone",
            oldNullable: true);
    }
}
