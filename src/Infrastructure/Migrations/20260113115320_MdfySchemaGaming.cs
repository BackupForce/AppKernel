using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class MdfySchemaGaming : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_gaming_ticket_lines_gaming_tickets_ticket_id",
            schema: "public",
            table: "Gaming_TicketLines");

        migrationBuilder.DropPrimaryKey(
            name: "pk_gaming_ticket_templates",
            schema: "public",
            table: "Gaming_TicketTemplates");

        migrationBuilder.DropPrimaryKey(
            name: "pk_gaming_tickets",
            schema: "public",
            table: "Gaming_Tickets");

        migrationBuilder.DropPrimaryKey(
            name: "pk_gaming_ticket_lines",
            schema: "public",
            table: "Gaming_TicketLines");

        migrationBuilder.DropPrimaryKey(
            name: "pk_gaming_redeem_records",
            schema: "public",
            table: "Gaming_RedeemRecords");

        migrationBuilder.DropPrimaryKey(
            name: "pk_gaming_prizes",
            schema: "public",
            table: "Gaming_Prizes");

        migrationBuilder.DropPrimaryKey(
            name: "pk_gaming_prize_rules",
            schema: "public",
            table: "Gaming_PrizeRules");

        migrationBuilder.DropPrimaryKey(
            name: "pk_gaming_prize_awards",
            schema: "public",
            table: "Gaming_PrizeAwards");

        migrationBuilder.DropPrimaryKey(
            name: "pk_gaming_prize_award_options",
            schema: "public",
            table: "Gaming_PrizeAwardOptions");

        migrationBuilder.DropPrimaryKey(
            name: "pk_gaming_draws",
            schema: "public",
            table: "Gaming_Draws");

        migrationBuilder.DropPrimaryKey(
            name: "pk_gaming_draw_prize_mappings",
            schema: "public",
            table: "Gaming_DrawPrizeMappings");

        migrationBuilder.DropPrimaryKey(
            name: "pk_gaming_draw_allowed_ticket_templates",
            schema: "public",
            table: "Gaming_DrawAllowedTicketTemplates");

        migrationBuilder.EnsureSchema(
            name: "gaming");

        migrationBuilder.RenameTable(
            name: "Gaming_TicketTemplates",
            schema: "public",
            newName: "ticket_templates",
            newSchema: "gaming");

        migrationBuilder.RenameTable(
            name: "Gaming_Tickets",
            schema: "public",
            newName: "tickets",
            newSchema: "gaming");

        migrationBuilder.RenameTable(
            name: "Gaming_TicketLines",
            schema: "public",
            newName: "ticket_lines",
            newSchema: "gaming");

        migrationBuilder.RenameTable(
            name: "Gaming_RedeemRecords",
            schema: "public",
            newName: "redeem_records",
            newSchema: "gaming");

        migrationBuilder.RenameTable(
            name: "Gaming_Prizes",
            schema: "public",
            newName: "prizes",
            newSchema: "gaming");

        migrationBuilder.RenameTable(
            name: "Gaming_PrizeRules",
            schema: "public",
            newName: "prize_rules",
            newSchema: "gaming");

        migrationBuilder.RenameTable(
            name: "Gaming_PrizeAwards",
            schema: "public",
            newName: "prize_awards",
            newSchema: "gaming");

        migrationBuilder.RenameTable(
            name: "Gaming_PrizeAwardOptions",
            schema: "public",
            newName: "prize_award_options",
            newSchema: "gaming");

        migrationBuilder.RenameTable(
            name: "Gaming_Draws",
            schema: "public",
            newName: "draws",
            newSchema: "gaming");

        migrationBuilder.RenameTable(
            name: "Gaming_DrawPrizeMappings",
            schema: "public",
            newName: "draw_prize_mappings",
            newSchema: "gaming");

        migrationBuilder.RenameTable(
            name: "Gaming_DrawAllowedTicketTemplates",
            schema: "public",
            newName: "draw_allowed_ticket_templates",
            newSchema: "gaming");

        migrationBuilder.RenameIndex(
            name: "ix_gaming_ticket_templates_tenant_id_code",
            schema: "gaming",
            table: "ticket_templates",
            newName: "ix_ticket_templates_tenant_id_code");

        migrationBuilder.RenameIndex(
            name: "ix_gaming_tickets_tenant_id_member_id_draw_id_created_at",
            schema: "gaming",
            table: "tickets",
            newName: "ix_tickets_tenant_id_member_id_draw_id_created_at");

        migrationBuilder.RenameIndex(
            name: "ix_gaming_ticket_lines_ticket_id_line_index",
            schema: "gaming",
            table: "ticket_lines",
            newName: "ix_ticket_lines_ticket_id_line_index");

        migrationBuilder.RenameIndex(
            name: "ix_gaming_redeem_records_prize_award_id",
            schema: "gaming",
            table: "redeem_records",
            newName: "ix_redeem_records_prize_award_id");

        migrationBuilder.RenameIndex(
            name: "ix_gaming_prizes_tenant_id_name",
            schema: "gaming",
            table: "prizes",
            newName: "ix_prizes_tenant_id_name");

        migrationBuilder.RenameIndex(
            name: "ix_gaming_prize_rules_tenant_id_game_type_match_count_is_active",
            schema: "gaming",
            table: "prize_rules",
            newName: "ix_prize_rules_tenant_id_game_type_match_count_is_active");

        migrationBuilder.RenameIndex(
            name: "ix_gaming_prize_awards_tenant_id_draw_id_ticket_id_line_index",
            schema: "gaming",
            table: "prize_awards",
            newName: "ix_prize_awards_tenant_id_draw_id_ticket_id_line_index");

        migrationBuilder.RenameIndex(
            name: "ix_gaming_prize_award_options_tenant_id_prize_award_id_prize_id",
            schema: "gaming",
            table: "prize_award_options",
            newName: "ix_prize_award_options_tenant_id_prize_award_id_prize_id");

        migrationBuilder.RenameIndex(
            name: "ix_gaming_draws_tenant_id_status",
            schema: "gaming",
            table: "draws",
            newName: "ix_draws_tenant_id_status");

        migrationBuilder.RenameIndex(
            name: "ix_gaming_draw_prize_mappings_tenant_id_draw_id_match_count_priz",
            schema: "gaming",
            table: "draw_prize_mappings",
            newName: "ix_draw_prize_mappings_tenant_id_draw_id_match_count_prize_id");

        migrationBuilder.RenameIndex(
            name: "ix_gaming_draw_allowed_ticket_templates_tenant_id_draw_id_ticket_",
            schema: "gaming",
            table: "draw_allowed_ticket_templates",
            newName: "ix_draw_allowed_ticket_templates_tenant_id_draw_id_ticket_temp");

        migrationBuilder.AddPrimaryKey(
            name: "pk_ticket_templates",
            schema: "gaming",
            table: "ticket_templates",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_tickets",
            schema: "gaming",
            table: "tickets",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_ticket_lines",
            schema: "gaming",
            table: "ticket_lines",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_redeem_records",
            schema: "gaming",
            table: "redeem_records",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_prizes",
            schema: "gaming",
            table: "prizes",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_prize_rules",
            schema: "gaming",
            table: "prize_rules",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_prize_awards",
            schema: "gaming",
            table: "prize_awards",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_prize_award_options",
            schema: "gaming",
            table: "prize_award_options",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_draws",
            schema: "gaming",
            table: "draws",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_draw_prize_mappings",
            schema: "gaming",
            table: "draw_prize_mappings",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_draw_allowed_ticket_templates",
            schema: "gaming",
            table: "draw_allowed_ticket_templates",
            column: "id");

        migrationBuilder.AddForeignKey(
            name: "fk_ticket_lines_tickets_ticket_id",
            schema: "gaming",
            table: "ticket_lines",
            column: "ticket_id",
            principalSchema: "gaming",
            principalTable: "tickets",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_ticket_lines_tickets_ticket_id",
            schema: "gaming",
            table: "ticket_lines");

        migrationBuilder.DropPrimaryKey(
            name: "pk_tickets",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropPrimaryKey(
            name: "pk_ticket_templates",
            schema: "gaming",
            table: "ticket_templates");

        migrationBuilder.DropPrimaryKey(
            name: "pk_ticket_lines",
            schema: "gaming",
            table: "ticket_lines");

        migrationBuilder.DropPrimaryKey(
            name: "pk_redeem_records",
            schema: "gaming",
            table: "redeem_records");

        migrationBuilder.DropPrimaryKey(
            name: "pk_prizes",
            schema: "gaming",
            table: "prizes");

        migrationBuilder.DropPrimaryKey(
            name: "pk_prize_rules",
            schema: "gaming",
            table: "prize_rules");

        migrationBuilder.DropPrimaryKey(
            name: "pk_prize_awards",
            schema: "gaming",
            table: "prize_awards");

        migrationBuilder.DropPrimaryKey(
            name: "pk_prize_award_options",
            schema: "gaming",
            table: "prize_award_options");

        migrationBuilder.DropPrimaryKey(
            name: "pk_draws",
            schema: "gaming",
            table: "draws");

        migrationBuilder.DropPrimaryKey(
            name: "pk_draw_prize_mappings",
            schema: "gaming",
            table: "draw_prize_mappings");

        migrationBuilder.DropPrimaryKey(
            name: "pk_draw_allowed_ticket_templates",
            schema: "gaming",
            table: "draw_allowed_ticket_templates");

        migrationBuilder.RenameTable(
            name: "tickets",
            schema: "gaming",
            newName: "Gaming_Tickets",
            newSchema: "public");

        migrationBuilder.RenameTable(
            name: "ticket_templates",
            schema: "gaming",
            newName: "Gaming_TicketTemplates",
            newSchema: "public");

        migrationBuilder.RenameTable(
            name: "ticket_lines",
            schema: "gaming",
            newName: "Gaming_TicketLines",
            newSchema: "public");

        migrationBuilder.RenameTable(
            name: "redeem_records",
            schema: "gaming",
            newName: "Gaming_RedeemRecords",
            newSchema: "public");

        migrationBuilder.RenameTable(
            name: "prizes",
            schema: "gaming",
            newName: "Gaming_Prizes",
            newSchema: "public");

        migrationBuilder.RenameTable(
            name: "prize_rules",
            schema: "gaming",
            newName: "Gaming_PrizeRules",
            newSchema: "public");

        migrationBuilder.RenameTable(
            name: "prize_awards",
            schema: "gaming",
            newName: "Gaming_PrizeAwards",
            newSchema: "public");

        migrationBuilder.RenameTable(
            name: "prize_award_options",
            schema: "gaming",
            newName: "Gaming_PrizeAwardOptions",
            newSchema: "public");

        migrationBuilder.RenameTable(
            name: "draws",
            schema: "gaming",
            newName: "Gaming_Draws",
            newSchema: "public");

        migrationBuilder.RenameTable(
            name: "draw_prize_mappings",
            schema: "gaming",
            newName: "Gaming_DrawPrizeMappings",
            newSchema: "public");

        migrationBuilder.RenameTable(
            name: "draw_allowed_ticket_templates",
            schema: "gaming",
            newName: "Gaming_DrawAllowedTicketTemplates",
            newSchema: "public");

        migrationBuilder.RenameIndex(
            name: "ix_tickets_tenant_id_member_id_draw_id_created_at",
            schema: "public",
            table: "Gaming_Tickets",
            newName: "ix_gaming_tickets_tenant_id_member_id_draw_id_created_at");

        migrationBuilder.RenameIndex(
            name: "ix_ticket_templates_tenant_id_code",
            schema: "public",
            table: "Gaming_TicketTemplates",
            newName: "ix_gaming_ticket_templates_tenant_id_code");

        migrationBuilder.RenameIndex(
            name: "ix_ticket_lines_ticket_id_line_index",
            schema: "public",
            table: "Gaming_TicketLines",
            newName: "ix_gaming_ticket_lines_ticket_id_line_index");

        migrationBuilder.RenameIndex(
            name: "ix_redeem_records_prize_award_id",
            schema: "public",
            table: "Gaming_RedeemRecords",
            newName: "ix_gaming_redeem_records_prize_award_id");

        migrationBuilder.RenameIndex(
            name: "ix_prizes_tenant_id_name",
            schema: "public",
            table: "Gaming_Prizes",
            newName: "ix_gaming_prizes_tenant_id_name");

        migrationBuilder.RenameIndex(
            name: "ix_prize_rules_tenant_id_game_type_match_count_is_active",
            schema: "public",
            table: "Gaming_PrizeRules",
            newName: "ix_gaming_prize_rules_tenant_id_game_type_match_count_is_active");

        migrationBuilder.RenameIndex(
            name: "ix_prize_awards_tenant_id_draw_id_ticket_id_line_index",
            schema: "public",
            table: "Gaming_PrizeAwards",
            newName: "ix_gaming_prize_awards_tenant_id_draw_id_ticket_id_line_index");

        migrationBuilder.RenameIndex(
            name: "ix_prize_award_options_tenant_id_prize_award_id_prize_id",
            schema: "public",
            table: "Gaming_PrizeAwardOptions",
            newName: "ix_gaming_prize_award_options_tenant_id_prize_award_id_prize_id");

        migrationBuilder.RenameIndex(
            name: "ix_draws_tenant_id_status",
            schema: "public",
            table: "Gaming_Draws",
            newName: "ix_gaming_draws_tenant_id_status");

        migrationBuilder.RenameIndex(
            name: "ix_draw_prize_mappings_tenant_id_draw_id_match_count_prize_id",
            schema: "public",
            table: "Gaming_DrawPrizeMappings",
            newName: "ix_gaming_draw_prize_mappings_tenant_id_draw_id_match_count_priz");

        migrationBuilder.RenameIndex(
            name: "ix_draw_allowed_ticket_templates_tenant_id_draw_id_ticket_temp",
            schema: "public",
            table: "Gaming_DrawAllowedTicketTemplates",
            newName: "ix_gaming_draw_allowed_ticket_templates_tenant_id_draw_id_ticket_");

        migrationBuilder.AddPrimaryKey(
            name: "pk_gaming_tickets",
            schema: "public",
            table: "Gaming_Tickets",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_gaming_ticket_templates",
            schema: "public",
            table: "Gaming_TicketTemplates",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_gaming_ticket_lines",
            schema: "public",
            table: "Gaming_TicketLines",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_gaming_redeem_records",
            schema: "public",
            table: "Gaming_RedeemRecords",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_gaming_prizes",
            schema: "public",
            table: "Gaming_Prizes",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_gaming_prize_rules",
            schema: "public",
            table: "Gaming_PrizeRules",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_gaming_prize_awards",
            schema: "public",
            table: "Gaming_PrizeAwards",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_gaming_prize_award_options",
            schema: "public",
            table: "Gaming_PrizeAwardOptions",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_gaming_draws",
            schema: "public",
            table: "Gaming_Draws",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_gaming_draw_prize_mappings",
            schema: "public",
            table: "Gaming_DrawPrizeMappings",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_gaming_draw_allowed_ticket_templates",
            schema: "public",
            table: "Gaming_DrawAllowedTicketTemplates",
            column: "id");

        migrationBuilder.AddForeignKey(
            name: "fk_gaming_ticket_lines_gaming_tickets_ticket_id",
            schema: "public",
            table: "Gaming_TicketLines",
            column: "ticket_id",
            principalSchema: "public",
            principalTable: "Gaming_Tickets",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }
}
