using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class mdfyDrawAddDrawCode : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "draw_code",
            schema: "gaming",
            table: "draws",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "ix_draws_tenant_id_game_code_draw_code",
            schema: "gaming",
            table: "draws",
            columns: new[] { "tenant_id", "game_code", "draw_code" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_draws_tenant_id_game_code_draw_code",
            schema: "gaming",
            table: "draws");

        migrationBuilder.DropColumn(
            name: "draw_code",
            schema: "gaming",
            table: "draws");
    }
}
