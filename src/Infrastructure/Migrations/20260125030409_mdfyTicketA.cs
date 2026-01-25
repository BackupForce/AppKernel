using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class mdfyTicketA : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "play_type_code",
            schema: "gaming",
            table: "tickets",
            type: "character varying(32)",
            maxLength: 32,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(32)",
            oldMaxLength: 32);

        migrationBuilder.AddColumn<string>(
            name: "play_type_code",
            schema: "gaming",
            table: "ticket_lines",
            type: "character varying(32)",
            maxLength: 32,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "play_type_code",
            schema: "gaming",
            table: "ticket_lines");

        migrationBuilder.AlterColumn<string>(
            name: "play_type_code",
            schema: "gaming",
            table: "tickets",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "character varying(32)",
            oldMaxLength: 32,
            oldNullable: true);
    }
}
