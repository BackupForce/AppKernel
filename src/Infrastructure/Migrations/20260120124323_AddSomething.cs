using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddSomething : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "auth_sessions",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                last_used_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                revoked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                revoke_reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                device_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_auth_sessions", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "refresh_token_records",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                session_id = table.Column<Guid>(type: "uuid", nullable: false),
                token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                revoked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                revoked_reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                replaced_by_token_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_refresh_token_records", x => x.id);
                table.ForeignKey(
                    name: "fk_refresh_token_records_auth_sessions_session_id",
                    column: x => x.session_id,
                    principalSchema: "public",
                    principalTable: "auth_sessions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_auth_sessions_tenant_user",
            schema: "public",
            table: "auth_sessions",
            columns: new[] { "tenant_id", "user_id" });

        migrationBuilder.CreateIndex(
            name: "ix_refresh_token_records_session_id",
            schema: "public",
            table: "refresh_token_records",
            column: "session_id");

        migrationBuilder.CreateIndex(
            name: "ux_refresh_token_records_token_hash",
            schema: "public",
            table: "refresh_token_records",
            column: "token_hash",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "refresh_token_records",
            schema: "public");

        migrationBuilder.DropTable(
            name: "auth_sessions",
            schema: "public");
    }
}
