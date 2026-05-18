using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddTicketClainEvents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ticket_claim_events",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                starts_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ends_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                total_quota = table.Column<int>(type: "integer", nullable: false),
                total_claimed = table.Column<int>(type: "integer", nullable: false),
                per_member_quota = table.Column<int>(type: "integer", nullable: false),
                scope_type = table.Column<int>(type: "integer", nullable: false),
                scope_id = table.Column<Guid>(type: "uuid", nullable: false),
                ticket_template_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_ticket_claim_events", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "ticket_claim_member_counters",
            schema: "gaming",
            columns: table => new
            {
                event_id = table.Column<Guid>(type: "uuid", nullable: false),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                claimed_count = table.Column<int>(type: "integer", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_ticket_claim_member_counters", x => new { x.event_id, x.member_id });
            });

        migrationBuilder.CreateTable(
            name: "ticket_claim_records",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                event_id = table.Column<Guid>(type: "uuid", nullable: false),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                issued_ticket_ids = table.Column<string>(type: "jsonb", nullable: true),
                claimed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_ticket_claim_records", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_ticket_claim_events_tenant_id_status_starts_at_utc_ends_at_",
            schema: "gaming",
            table: "ticket_claim_events",
            columns: new[] { "tenant_id", "status", "starts_at_utc", "ends_at_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_ticket_claim_member_counters_event_id_member_id",
            schema: "gaming",
            table: "ticket_claim_member_counters",
            columns: new[] { "event_id", "member_id" });

        migrationBuilder.CreateIndex(
            name: "ix_ticket_claim_records_tenant_id_event_id_claimed_at_utc",
            schema: "gaming",
            table: "ticket_claim_records",
            columns: new[] { "tenant_id", "event_id", "claimed_at_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_ticket_claim_records_tenant_id_event_id_member_id_idempoten",
            schema: "gaming",
            table: "ticket_claim_records",
            columns: new[] { "tenant_id", "event_id", "member_id", "idempotency_key" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ticket_claim_events",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "ticket_claim_member_counters",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "ticket_claim_records",
            schema: "gaming");
    }
}
