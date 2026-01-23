using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddTicketSomething : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "issued_note",
            schema: "gaming",
            table: "tickets",
            type: "character varying(512)",
            maxLength: 512,
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "submitted_by_user_id",
            schema: "gaming",
            table: "tickets",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "submitted_client_reference",
            schema: "gaming",
            table: "tickets",
            type: "character varying(128)",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "submitted_note",
            schema: "gaming",
            table: "tickets",
            type: "character varying(512)",
            maxLength: 512,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "ticket_idempotency_records",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                operation = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                request_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                response_payload = table.Column<string>(type: "text", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_ticket_idempotency_records", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_ticket_idempotency_records_tenant_id_idempotency_key_operat",
            schema: "gaming",
            table: "ticket_idempotency_records",
            columns: new[] { "tenant_id", "idempotency_key", "operation" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ticket_idempotency_records",
            schema: "gaming");

        migrationBuilder.DropColumn(
            name: "issued_note",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropColumn(
            name: "submitted_by_user_id",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropColumn(
            name: "submitted_client_reference",
            schema: "gaming",
            table: "tickets");

        migrationBuilder.DropColumn(
            name: "submitted_note",
            schema: "gaming",
            table: "tickets");
    }
}
