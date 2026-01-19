using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddSlot : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "prize_name_snapshot",
            schema: "gaming",
            table: "draw_prize_pool_items",
            type: "character varying(128)",
            maxLength: 128,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(128)",
            oldMaxLength: 128);

        migrationBuilder.AlterColumn<decimal>(
            name: "prize_cost_snapshot",
            schema: "gaming",
            table: "draw_prize_pool_items",
            type: "numeric(18,2)",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "prize_name_snapshot",
            schema: "gaming",
            table: "draw_prize_pool_items",
            type: "character varying(128)",
            maxLength: 128,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "character varying(128)",
            oldMaxLength: 128,
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "prize_cost_snapshot",
            schema: "gaming",
            table: "draw_prize_pool_items",
            type: "numeric(18,2)",
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldNullable: true);
    }
}
