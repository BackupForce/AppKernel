using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddComp : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_tickets_tenant_id_member_id_draw_id_created_at",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.AlterColumn<long>(
            name: "total_cost",
            schema: "gaming",
            table: "tickets",
            type: "bigint",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "bigint");

        migrationBuilder.AlterColumn<Guid>(
            name: "ticket_template_id",
            schema: "gaming",
            table: "tickets",
            type: "uuid",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uuid");

        migrationBuilder.AlterColumn<decimal>(
            name: "price_snapshot",
            schema: "gaming",
            table: "tickets",
            type: "numeric(18,2)",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)");

        migrationBuilder.AlterColumn<Guid>(
            name: "draw_id",
            schema: "gaming",
            table: "tickets",
            type: "uuid",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uuid");

        migrationBuilder.AddColumn<Guid>(
            name: "campaign_id",
            schema: "gaming",
            table: "tickets",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "cancelled_at_utc",
            schema: "gaming",
            table: "tickets",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "cancelled_by_user_id",
            schema: "gaming",
            table: "tickets",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "cancelled_reason",
            schema: "gaming",
            table: "tickets",
            type: "character varying(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "issued_at_utc",
            schema: "gaming",
            table: "tickets",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<int>(
            name: "issued_by_type",
            schema: "gaming",
            table: "tickets",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<Guid>(
            name: "issued_by_user_id",
            schema: "gaming",
            table: "tickets",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "issued_reason",
            schema: "gaming",
            table: "tickets",
            type: "character varying(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "submission_status",
            schema: "gaming",
            table: "tickets",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<DateTime>(
            name: "submitted_at_utc",
            schema: "gaming",
            table: "tickets",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "campaigns",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                game_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                play_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                grant_open_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                grant_close_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_campaigns", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "ticket_draws",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                participation_status = table.Column<int>(type: "integer", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                evaluated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                settled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                redeemed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_ticket_draws", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "ticket_line_results",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                line_index = table.Column<int>(type: "integer", nullable: false),
                prize_tier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                payout = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                settled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_ticket_line_results", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "campaign_draws",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                campaign_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_campaign_draws", x => x.id);
                table.ForeignKey(
                    name: "fk_campaign_draws_campaigns_campaign_id",
                    column: x => x.campaign_id,
                    principalSchema: "gaming",
                    principalTable: "campaigns",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_tickets_tenant_id_member_id_created_at",
            schema: "gaming",
            table: "tickets",
            columns: new[] { "tenant_id", "member_id", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_campaign_draws_campaign_id_draw_id",
            schema: "gaming",
            table: "campaign_draws",
            columns: new[] { "campaign_id", "draw_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_campaign_draws_tenant_id_campaign_id",
            schema: "gaming",
            table: "campaign_draws",
            columns: new[] { "tenant_id", "campaign_id" });

        migrationBuilder.CreateIndex(
            name: "ix_campaigns_tenant_id_game_code_play_type_code_grant_open_at_",
            schema: "gaming",
            table: "campaigns",
            columns: new[] { "tenant_id", "game_code", "play_type_code", "grant_open_at_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_ticket_draws_tenant_id_draw_id_participation_status",
            schema: "gaming",
            table: "ticket_draws",
            columns: new[] { "tenant_id", "draw_id", "participation_status" });

        migrationBuilder.CreateIndex(
            name: "ix_ticket_draws_ticket_id_draw_id",
            schema: "gaming",
            table: "ticket_draws",
            columns: new[] { "ticket_id", "draw_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_ticket_line_results_tenant_id_ticket_id_draw_id_line_index",
            schema: "gaming",
            table: "ticket_line_results",
            columns: new[] { "tenant_id", "ticket_id", "draw_id", "line_index" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "campaign_draws",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "ticket_draws",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "ticket_line_results",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "campaigns",
            schema: "gaming");

        migrationBuilder.DropIndex(
            name: "ix_tickets_tenant_id_member_id_created_at",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropColumn(
            name: "campaign_id",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropColumn(
            name: "cancelled_at_utc",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropColumn(
            name: "cancelled_by_user_id",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropColumn(
            name: "cancelled_reason",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropColumn(
            name: "issued_at_utc",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropColumn(
            name: "issued_by_type",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropColumn(
            name: "issued_by_user_id",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropColumn(
            name: "issued_reason",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropColumn(
            name: "submission_status",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropColumn(
            name: "submitted_at_utc",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.AlterColumn<long>(
            name: "total_cost",
            schema: "gaming",
            table: "tickets",
            type: "bigint",
            nullable: false,
            defaultValue: 0L,
            oldClrType: typeof(long),
            oldType: "bigint",
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "ticket_template_id",
            schema: "gaming",
            table: "tickets",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "price_snapshot",
            schema: "gaming",
            table: "tickets",
            type: "numeric(18,2)",
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "draw_id",
            schema: "gaming",
            table: "tickets",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_tickets_tenant_id_member_id_draw_id_created_at",
            schema: "gaming",
            table: "tickets",
            columns: new[] { "tenant_id", "member_id", "draw_id", "created_at" });
    }
}
