using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class mdfyRedeemed : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "redeemed_at_utc",
            schema: "gaming",
            table: "ticket_line_results",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "redeemed_by_user_id",
            schema: "gaming",
            table: "ticket_line_results",
            type: "uuid",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "redeemed_at_utc",
            schema: "gaming",
            table: "ticket_line_results");

        migrationBuilder.DropColumn(
            name: "redeemed_by_user_id",
            schema: "gaming",
            table: "ticket_line_results");
    }
}
