using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class MdfySomething : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_gaming_tickets_tenant_id_member_id_created_at",
            schema: "public",
            table: "Gaming_Tickets");

        migrationBuilder.AddColumn<decimal>(
            name: "price_snapshot",
            schema: "public",
            table: "Gaming_Tickets",
            type: "numeric(18,2)",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<Guid>(
            name: "ticket_template_id",
            schema: "public",
            table: "Gaming_Tickets",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.AddColumn<string>(
            name: "prize_name_snapshot",
            schema: "public",
            table: "Gaming_RedeemRecords",
            type: "character varying(128)",
            maxLength: 128,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<int>(
            name: "redeem_valid_days",
            schema: "public",
            table: "Gaming_PrizeRules",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "expires_at",
            schema: "public",
            table: "Gaming_PrizeAwards",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "is_manually_closed",
            schema: "public",
            table: "Gaming_Draws",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTime>(
            name: "manual_close_at",
            schema: "public",
            table: "Gaming_Draws",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "manual_close_reason",
            schema: "public",
            table: "Gaming_Draws",
            type: "character varying(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "redeem_valid_days",
            schema: "public",
            table: "Gaming_Draws",
            type: "integer",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "Gaming_DrawAllowedTicketTemplates",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                ticket_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_gaming_draw_allowed_ticket_templates", x => x.id));

        migrationBuilder.CreateTable(
            name: "Gaming_DrawPrizeMappings",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                match_count = table.Column<int>(type: "integer", nullable: false),
                prize_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_gaming_draw_prize_mappings", x => x.id));

        migrationBuilder.CreateTable(
            name: "Gaming_PrizeAwardOptions",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                prize_award_id = table.Column<Guid>(type: "uuid", nullable: false),
                prize_id = table.Column<Guid>(type: "uuid", nullable: false),
                prize_name_snapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                prize_cost_snapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_gaming_prize_award_options", x => x.id));

        migrationBuilder.CreateTable(
            name: "Gaming_TicketTemplates",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                type = table.Column<int>(type: "integer", nullable: false),
                price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                max_lines_per_ticket = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_gaming_ticket_templates", x => x.id));

        migrationBuilder.CreateIndex(
            name: "ix_gaming_tickets_tenant_id_member_id_draw_id_created_at",
            schema: "public",
            table: "Gaming_Tickets",
            columns: new[] { "tenant_id", "member_id", "draw_id", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_gaming_draw_allowed_ticket_templates_tenant_id_draw_id_ticket_",
            schema: "public",
            table: "Gaming_DrawAllowedTicketTemplates",
            columns: new[] { "tenant_id", "draw_id", "ticket_template_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_gaming_draw_prize_mappings_tenant_id_draw_id_match_count_priz",
            schema: "public",
            table: "Gaming_DrawPrizeMappings",
            columns: new[] { "tenant_id", "draw_id", "match_count", "prize_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_gaming_prize_award_options_tenant_id_prize_award_id_prize_id",
            schema: "public",
            table: "Gaming_PrizeAwardOptions",
            columns: new[] { "tenant_id", "prize_award_id", "prize_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_gaming_ticket_templates_tenant_id_code",
            schema: "public",
            table: "Gaming_TicketTemplates",
            columns: new[] { "tenant_id", "code" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Gaming_DrawAllowedTicketTemplates",
            schema: "public");

        migrationBuilder.DropTable(
            name: "Gaming_DrawPrizeMappings",
            schema: "public");

        migrationBuilder.DropTable(
            name: "Gaming_PrizeAwardOptions",
            schema: "public");

        migrationBuilder.DropTable(
            name: "Gaming_TicketTemplates",
            schema: "public");

        migrationBuilder.DropIndex(
            name: "ix_gaming_tickets_tenant_id_member_id_draw_id_created_at",
            schema: "public",
            table: "Gaming_Tickets");

        migrationBuilder.DropColumn(
            name: "price_snapshot",
            schema: "public",
            table: "Gaming_Tickets");

        migrationBuilder.DropColumn(
            name: "ticket_template_id",
            schema: "public",
            table: "Gaming_Tickets");

        migrationBuilder.DropColumn(
            name: "prize_name_snapshot",
            schema: "public",
            table: "Gaming_RedeemRecords");

        migrationBuilder.DropColumn(
            name: "redeem_valid_days",
            schema: "public",
            table: "Gaming_PrizeRules");

        migrationBuilder.DropColumn(
            name: "expires_at",
            schema: "public",
            table: "Gaming_PrizeAwards");

        migrationBuilder.DropColumn(
            name: "is_manually_closed",
            schema: "public",
            table: "Gaming_Draws");

        migrationBuilder.DropColumn(
            name: "manual_close_at",
            schema: "public",
            table: "Gaming_Draws");

        migrationBuilder.DropColumn(
            name: "manual_close_reason",
            schema: "public",
            table: "Gaming_Draws");

        migrationBuilder.DropColumn(
            name: "redeem_valid_days",
            schema: "public",
            table: "Gaming_Draws");

        migrationBuilder.CreateIndex(
            name: "ix_gaming_tickets_tenant_id_member_id_created_at",
            schema: "public",
            table: "Gaming_Tickets",
            columns: new[] { "tenant_id", "member_id", "created_at" });
    }
}
