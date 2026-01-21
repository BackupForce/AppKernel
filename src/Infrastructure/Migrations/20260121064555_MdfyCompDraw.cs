using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class MdfyCompDraw : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_campaign_draws_campaign_id_draw_id",
            schema: "gaming",
            table: "campaign_draws");

        migrationBuilder.CreateIndex(
            name: "ix_campaign_draws_campaign_id",
            schema: "gaming",
            table: "campaign_draws",
            column: "campaign_id");

        migrationBuilder.CreateIndex(
            name: "ix_campaign_draws_tenant_id_campaign_id_draw_id",
            schema: "gaming",
            table: "campaign_draws",
            columns: new[] { "tenant_id", "campaign_id", "draw_id" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_campaign_draws_campaign_id",
            schema: "gaming",
            table: "campaign_draws");

        migrationBuilder.DropIndex(
            name: "ix_campaign_draws_tenant_id_campaign_id_draw_id",
            schema: "gaming",
            table: "campaign_draws");

        migrationBuilder.CreateIndex(
            name: "ix_campaign_draws_campaign_id_draw_id",
            schema: "gaming",
            table: "campaign_draws",
            columns: new[] { "campaign_id", "draw_id" },
            unique: true);
    }
}
