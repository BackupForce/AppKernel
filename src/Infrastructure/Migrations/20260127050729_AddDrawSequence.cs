using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddDrawSequence : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "draw_sequences",
            schema: "gaming",
            columns: table => new
            {
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                game_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                next_value = table.Column<long>(type: "bigint", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_sequences", x => new { x.tenant_id, x.game_code });
                table.CheckConstraint("ck_draw_sequences_next_value", "next_value >= 1");
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "draw_sequences",
            schema: "gaming");
    }
}
