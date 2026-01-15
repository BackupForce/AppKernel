using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddGamingEntitlements : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "tenant_game_entitlements",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                game_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                enabled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                disabled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tenant_game_entitlements", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "tenant_play_entitlements",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                game_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                play_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                enabled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                disabled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tenant_play_entitlements", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ux_tenant_game_entitlements_tenant_game",
            schema: "gaming",
            table: "tenant_game_entitlements",
            columns: new[] { "tenant_id", "game_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_tenant_play_entitlements_tenant_game_play",
            schema: "gaming",
            table: "tenant_play_entitlements",
            columns: new[] { "tenant_id", "game_code", "play_type_code" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "tenant_game_entitlements",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "tenant_play_entitlements",
            schema: "gaming");
    }
}
