using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class mdfyAddMultiGame : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "draw_prize_mappings",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "prize_award_options",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "prize_rules",
            schema: "gaming");

        migrationBuilder.AddColumn<string>(
            name: "game_code",
            schema: "gaming",
            table: "tickets",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "play_type_code",
            schema: "gaming",
            table: "tickets",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "game_code",
            schema: "gaming",
            table: "prize_awards",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "play_type_code",
            schema: "gaming",
            table: "prize_awards",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<decimal>(
            name: "prize_cost_snapshot",
            schema: "gaming",
            table: "prize_awards",
            type: "numeric(18,2)",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<string>(
            name: "prize_description_snapshot",
            schema: "gaming",
            table: "prize_awards",
            type: "character varying(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "prize_name_snapshot",
            schema: "gaming",
            table: "prize_awards",
            type: "character varying(128)",
            maxLength: 128,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<int>(
            name: "prize_redeem_valid_days_snapshot",
            schema: "gaming",
            table: "prize_awards",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "prize_tier",
            schema: "gaming",
            table: "prize_awards",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "game_code",
            schema: "gaming",
            table: "draws",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateTable(
            name: "draw_enabled_play_types",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                play_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_enabled_play_types", x => x.id);
                table.ForeignKey(
                    name: "fk_draw_enabled_play_types_draws_draw_id",
                    column: x => x.draw_id,
                    principalSchema: "gaming",
                    principalTable: "draws",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "draw_prize_pool_items",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                play_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                tier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                prize_id_snapshot = table.Column<Guid>(type: "uuid", nullable: true),
                prize_name_snapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                prize_cost_snapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                prize_redeem_valid_days_snapshot = table.Column<int>(type: "integer", nullable: true),
                prize_description_snapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                draw_id1 = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_prize_pool_items", x => x.id);
                table.ForeignKey(
                    name: "fk_draw_prize_pool_items_draws_draw_id",
                    column: x => x.draw_id,
                    principalSchema: "gaming",
                    principalTable: "draws",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_draw_prize_pool_items_draws_draw_id1",
                    column: x => x.draw_id1,
                    principalSchema: "gaming",
                    principalTable: "draws",
                    principalColumn: "id");
            });

        migrationBuilder.CreateIndex(
            name: "ix_draw_enabled_play_types_draw_id",
            schema: "gaming",
            table: "draw_enabled_play_types",
            column: "draw_id");

        migrationBuilder.CreateIndex(
            name: "ix_draw_enabled_play_types_tenant_id_draw_id_play_type_code",
            schema: "gaming",
            table: "draw_enabled_play_types",
            columns: new[] { "tenant_id", "draw_id", "play_type_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_draw_prize_pool_items_draw_id",
            schema: "gaming",
            table: "draw_prize_pool_items",
            column: "draw_id");

        migrationBuilder.CreateIndex(
            name: "ix_draw_prize_pool_items_draw_id1",
            schema: "gaming",
            table: "draw_prize_pool_items",
            column: "draw_id1");

        migrationBuilder.CreateIndex(
            name: "ix_draw_prize_pool_items_tenant_id_draw_id_play_type_code_tier",
            schema: "gaming",
            table: "draw_prize_pool_items",
            columns: new[] { "tenant_id", "draw_id", "play_type_code", "tier" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "draw_enabled_play_types",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "draw_prize_pool_items",
            schema: "gaming");

        migrationBuilder.DropColumn(
            name: "game_code",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropColumn(
            name: "play_type_code",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropColumn(
            name: "game_code",
            schema: "gaming",
            table: "prize_awards");

        migrationBuilder.DropColumn(
            name: "play_type_code",
            schema: "gaming",
            table: "prize_awards");

        migrationBuilder.DropColumn(
            name: "prize_cost_snapshot",
            schema: "gaming",
            table: "prize_awards");

        migrationBuilder.DropColumn(
            name: "prize_description_snapshot",
            schema: "gaming",
            table: "prize_awards");

        migrationBuilder.DropColumn(
            name: "prize_name_snapshot",
            schema: "gaming",
            table: "prize_awards");

        migrationBuilder.DropColumn(
            name: "prize_redeem_valid_days_snapshot",
            schema: "gaming",
            table: "prize_awards");

        migrationBuilder.DropColumn(
            name: "prize_tier",
            schema: "gaming",
            table: "prize_awards");

        migrationBuilder.DropColumn(
            name: "game_code",
            schema: "gaming",
            table: "draws");

        migrationBuilder.CreateTable(
            name: "draw_prize_mappings",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                match_count = table.Column<int>(type: "integer", nullable: false),
                prize_id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_draw_prize_mappings", x => x.id));

        migrationBuilder.CreateTable(
            name: "prize_award_options",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                prize_award_id = table.Column<Guid>(type: "uuid", nullable: false),
                prize_cost_snapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                prize_id = table.Column<Guid>(type: "uuid", nullable: false),
                prize_name_snapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_prize_award_options", x => x.id));

        migrationBuilder.CreateTable(
            name: "prize_rules",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                effective_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                effective_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                game_type = table.Column<int>(type: "integer", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                match_count = table.Column<int>(type: "integer", nullable: false),
                prize_id = table.Column<Guid>(type: "uuid", nullable: false),
                redeem_valid_days = table.Column<int>(type: "integer", nullable: true),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_prize_rules", x => x.id));

        migrationBuilder.CreateIndex(
            name: "ix_draw_prize_mappings_tenant_id_draw_id_match_count_prize_id",
            schema: "gaming",
            table: "draw_prize_mappings",
            columns: new[] { "tenant_id", "draw_id", "match_count", "prize_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_prize_award_options_tenant_id_prize_award_id_prize_id",
            schema: "gaming",
            table: "prize_award_options",
            columns: new[] { "tenant_id", "prize_award_id", "prize_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_prize_rules_tenant_id_game_type_match_count_is_active",
            schema: "gaming",
            table: "prize_rules",
            columns: new[] { "tenant_id", "game_type", "match_count", "is_active" });
    }
}
