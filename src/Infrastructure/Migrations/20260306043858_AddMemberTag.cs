using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddMemberTag : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "member_tag_bindings",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_member_tag_bindings", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "member_tags_catalog",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                tag_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                display_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_member_tags_catalog", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "ticket_claim_event_tag_rules",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                event_id = table.Column<Guid>(type: "uuid", nullable: false),
                tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_ticket_claim_event_tag_rules", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_member_tag_bindings_tenant_member",
            schema: "public",
            table: "member_tag_bindings",
            columns: new[] { "tenant_id", "member_id" });

        migrationBuilder.CreateIndex(
            name: "ix_member_tag_bindings_tenant_tag",
            schema: "public",
            table: "member_tag_bindings",
            columns: new[] { "tenant_id", "tag_id" });

        migrationBuilder.CreateIndex(
            name: "ux_member_tag_bindings_tenant_member_tag",
            schema: "public",
            table: "member_tag_bindings",
            columns: new[] { "tenant_id", "member_id", "tag_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_member_tags_catalog_tenant_id_is_active",
            schema: "public",
            table: "member_tags_catalog",
            columns: new[] { "tenant_id", "is_active" });

        migrationBuilder.CreateIndex(
            name: "ux_member_tags_catalog_tenant_id_tag_code",
            schema: "public",
            table: "member_tags_catalog",
            columns: new[] { "tenant_id", "tag_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_ticket_claim_event_tag_rules_tenant_event",
            schema: "gaming",
            table: "ticket_claim_event_tag_rules",
            columns: new[] { "tenant_id", "event_id" });

        migrationBuilder.CreateIndex(
            name: "ix_ticket_claim_event_tag_rules_tenant_tag",
            schema: "gaming",
            table: "ticket_claim_event_tag_rules",
            columns: new[] { "tenant_id", "tag_id" });

        migrationBuilder.CreateIndex(
            name: "ux_ticket_claim_event_tag_rules_tenant_event_tag",
            schema: "gaming",
            table: "ticket_claim_event_tag_rules",
            columns: new[] { "tenant_id", "event_id", "tag_id" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "member_tag_bindings",
            schema: "public");

        migrationBuilder.DropTable(
            name: "member_tags_catalog",
            schema: "public");

        migrationBuilder.DropTable(
            name: "ticket_claim_event_tag_rules",
            schema: "gaming");
    }
}
