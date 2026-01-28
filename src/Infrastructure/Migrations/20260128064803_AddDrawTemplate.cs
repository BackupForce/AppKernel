using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddDrawTemplate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "source_template_id",
            schema: "gaming",
            table: "draws",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "source_template_version",
            schema: "gaming",
            table: "draws",
            type: "integer",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "draw_templates",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                game_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                version = table.Column<int>(type: "integer", nullable: false),
                is_locked = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_templates", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "draw_template_allowed_ticket_templates",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                template_id = table.Column<Guid>(type: "uuid", nullable: false),
                ticket_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_template_allowed_ticket_templates", x => x.id);
                table.ForeignKey(
                    name: "fk_draw_template_allowed_ticket_templates_draw_templates_templ",
                    column: x => x.template_id,
                    principalSchema: "gaming",
                    principalTable: "draw_templates",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "draw_template_play_types",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                template_id = table.Column<Guid>(type: "uuid", nullable: false),
                play_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_template_play_types", x => x.id);
                table.ForeignKey(
                    name: "fk_draw_template_play_types_draw_templates_template_id",
                    column: x => x.template_id,
                    principalSchema: "gaming",
                    principalTable: "draw_templates",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "draw_template_prize_tiers",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                template_id = table.Column<Guid>(type: "uuid", nullable: false),
                play_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                tier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                prize_id_snapshot = table.Column<Guid>(type: "uuid", nullable: true),
                prize_name_snapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                prize_cost_snapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                prize_redeem_valid_days_snapshot = table.Column<int>(type: "integer", nullable: true),
                prize_description_snapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_template_prize_tiers", x => x.id);
                table.ForeignKey(
                    name: "fk_draw_template_prize_tiers_draw_templates_template_id",
                    column: x => x.template_id,
                    principalSchema: "gaming",
                    principalTable: "draw_templates",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_draw_template_allowed_ticket_templates_template_id",
            schema: "gaming",
            table: "draw_template_allowed_ticket_templates",
            column: "template_id");

        migrationBuilder.CreateIndex(
            name: "ix_draw_template_allowed_ticket_templates_tenant_id_template_i",
            schema: "gaming",
            table: "draw_template_allowed_ticket_templates",
            columns: new[] { "tenant_id", "template_id", "ticket_template_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_draw_template_play_types_template_id",
            schema: "gaming",
            table: "draw_template_play_types",
            column: "template_id");

        migrationBuilder.CreateIndex(
            name: "ix_draw_template_play_types_tenant_id_template_id_play_type_co",
            schema: "gaming",
            table: "draw_template_play_types",
            columns: new[] { "tenant_id", "template_id", "play_type_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_draw_template_prize_tiers_template_id",
            schema: "gaming",
            table: "draw_template_prize_tiers",
            column: "template_id");

        migrationBuilder.CreateIndex(
            name: "ix_draw_template_prize_tiers_tenant_id_template_id_play_type_c",
            schema: "gaming",
            table: "draw_template_prize_tiers",
            columns: new[] { "tenant_id", "template_id", "play_type_code", "tier" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_draw_templates_tenant_id_game_code_name",
            schema: "gaming",
            table: "draw_templates",
            columns: new[] { "tenant_id", "game_code", "name" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "draw_template_allowed_ticket_templates",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "draw_template_play_types",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "draw_template_prize_tiers",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "draw_templates",
            schema: "gaming");

        migrationBuilder.DropColumn(
            name: "source_template_id",
            schema: "gaming",
            table: "draws");

        migrationBuilder.DropColumn(
            name: "source_template_version",
            schema: "gaming",
            table: "draws");
    }
}
