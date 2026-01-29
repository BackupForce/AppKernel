using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class mdfyDrawStatusSte : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "prize_payout_snapshot",
            schema: "gaming",
            table: "draw_template_prize_tiers",
            type: "numeric(18,2)",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<decimal>(
            name: "prize_payout_snapshot",
            schema: "gaming",
            table: "draw_prize_pool_items",
            type: "numeric(18,2)",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "prize_payout_snapshot",
            schema: "gaming",
            table: "draw_template_prize_tiers");

        migrationBuilder.DropColumn(
            name: "prize_payout_snapshot",
            schema: "gaming",
            table: "draw_prize_pool_items");
    }
}
