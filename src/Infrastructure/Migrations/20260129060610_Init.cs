using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class Init : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "public");

        migrationBuilder.EnsureSchema(
            name: "gaming");

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
            name: "campaigns",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                game_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                play_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                grant_open_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                grant_close_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_campaigns", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "draw_allowed_ticket_templates",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                ticket_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_allowed_ticket_templates", x => x.id);
            });

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

        migrationBuilder.CreateTable(
            name: "draw_templates",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                game_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                version = table.Column<int>(type: "integer", nullable: false),
                is_locked = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_templates", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "draws",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                game_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                sales_open_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                draw_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                sales_close_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                draw_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                winning_numbers_raw = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                server_seed_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                server_seed = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                algorithm = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                derived_input = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                is_manually_closed = table.Column<bool>(type: "boolean", nullable: false),
                manual_close_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                manual_close_reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                settled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                settled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                redeem_valid_days = table.Column<int>(type: "integer", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                source_template_id = table.Column<Guid>(type: "uuid", nullable: true),
                source_template_version = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draws", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "groups",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                external_key = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_groups", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "node",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_node", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "outbox_messages",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                content = table.Column<string>(type: "jsonb", nullable: false),
                created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                error = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_outbox_messages", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "prize_awards",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                game_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                play_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                line_index = table.Column<int>(type: "integer", nullable: false),
                matched_count = table.Column<int>(type: "integer", nullable: false),
                prize_tier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                prize_id = table.Column<Guid>(type: "uuid", nullable: false),
                prize_name_snapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                prize_cost_snapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                prize_redeem_valid_days_snapshot = table.Column<int>(type: "integer", nullable: true),
                prize_description_snapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                status = table.Column<int>(type: "integer", nullable: false),
                awarded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                redeemed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_prize_awards", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "prizes",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_prizes", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "redeem_records",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                prize_award_id = table.Column<Guid>(type: "uuid", nullable: false),
                prize_id = table.Column<Guid>(type: "uuid", nullable: false),
                prize_name_snapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                cost_snapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                redeemed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                note = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_redeem_records", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "resource_nodes",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                external_key = table.Column<string>(type: "text", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                parent_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_resource_nodes", x => x.id);
                table.ForeignKey(
                    name: "fk_resource_nodes_resource_nodes_parent_id",
                    column: x => x.parent_id,
                    principalSchema: "public",
                    principalTable: "resource_nodes",
                    principalColumn: "id");
            });

        migrationBuilder.CreateTable(
            name: "role",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "text", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_role", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "tenant_game_entitlements",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                game_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                enabled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                disabled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tenant_game_entitlements", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "tenant_play_entitlements",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                game_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                play_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                enabled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                disabled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tenant_play_entitlements", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "tenants",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                time_zone_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tenants", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "ticket_draws",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                participation_status = table.Column<int>(type: "integer", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                evaluated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                settled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                redeemed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_ticket_draws", x => x.id);
            });

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

        migrationBuilder.CreateTable(
            name: "ticket_line_results",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                line_index = table.Column<int>(type: "integer", nullable: false),
                prize_tier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                payout = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                settled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_ticket_line_results", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "ticket_templates",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                type = table.Column<int>(type: "integer", nullable: false),
                price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                max_lines_per_ticket = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_ticket_templates", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "tickets",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: true),
                game_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                play_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                campaign_id = table.Column<Guid>(type: "uuid", nullable: true),
                ticket_template_id = table.Column<Guid>(type: "uuid", nullable: true),
                price_snapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                total_cost = table.Column<long>(type: "bigint", nullable: true),
                issued_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                issued_by_type = table.Column<int>(type: "integer", nullable: false),
                issued_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                issued_reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                issued_note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                submission_status = table.Column<int>(type: "integer", nullable: false),
                submitted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                submitted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                submitted_client_reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                submitted_note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                cancelled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                cancelled_reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                cancelled_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_tickets", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "users",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                normalized_email = table.Column<string>(type: "text", nullable: false),
                password_hash = table.Column<string>(type: "text", nullable: false),
                has_public_profile = table.Column<bool>(type: "boolean", nullable: false),
                type = table.Column<int>(type: "integer", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                email = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
                table.CheckConstraint("CK_user_type", "\"type\" IN (0, 1, 2)");
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

        migrationBuilder.CreateTable(
            name: "campaign_draws",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                campaign_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_campaign_draws", x => x.id);
                table.ForeignKey(
                    name: "fk_campaign_draws_campaigns_campaign_id",
                    column: x => x.campaign_id,
                    principalSchema: "gaming",
                    principalTable: "campaigns",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "draw_template_allowed_ticket_templates",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                template_id = table.Column<Guid>(type: "uuid", nullable: false),
                ticket_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_template_allowed_ticket_templates", x => x.id);
                table.ForeignKey(
                    name: "fk_draw_template_allowed_ticket_templates_draw_templates_templ",
                    column: x => x.template_id,
                    principalSchema: "gaming",
                    principalTable: "draw_templates",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "draw_template_play_types",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                template_id = table.Column<Guid>(type: "uuid", nullable: false),
                play_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_template_play_types", x => x.id);
                table.ForeignKey(
                    name: "fk_draw_template_play_types_draw_templates_template_id",
                    column: x => x.template_id,
                    principalSchema: "gaming",
                    principalTable: "draw_templates",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "draw_template_prize_tiers",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                template_id = table.Column<Guid>(type: "uuid", nullable: false),
                play_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                tier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                prize_id_snapshot = table.Column<Guid>(type: "uuid", nullable: true),
                prize_name_snapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                prize_cost_snapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                prize_payout_snapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                prize_redeem_valid_days_snapshot = table.Column<int>(type: "integer", nullable: true),
                prize_description_snapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_template_prize_tiers", x => x.id);
                table.ForeignKey(
                    name: "fk_draw_template_prize_tiers_draw_templates_template_id",
                    column: x => x.template_id,
                    principalSchema: "gaming",
                    principalTable: "draw_templates",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "draw_enabled_play_types",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                play_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_enabled_play_types", x => x.id);
                table.ForeignKey(
                    name: "fk_draw_enabled_play_types_draws_draw_id",
                    column: x => x.draw_id,
                    principalSchema: "gaming",
                    principalTable: "draws",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "draw_prize_pool_items",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                play_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                tier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                prize_id_snapshot = table.Column<Guid>(type: "uuid", nullable: true),
                prize_name_snapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                prize_cost_snapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                prize_payout_snapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                prize_redeem_valid_days_snapshot = table.Column<int>(type: "integer", nullable: true),
                prize_description_snapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_draw_prize_pool_items", x => x.id);
                table.ForeignKey(
                    name: "fk_draw_prize_pool_items_draws_draw_id",
                    column: x => x.draw_id,
                    principalSchema: "gaming",
                    principalTable: "draws",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "node_relation",
            schema: "public",
            columns: table => new
            {
                ancestor_node_id = table.Column<Guid>(type: "uuid", nullable: false),
                descendant_node_id = table.Column<Guid>(type: "uuid", nullable: false),
                depth = table.Column<int>(type: "integer", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_node_relation", x => new { x.ancestor_node_id, x.descendant_node_id });
                table.ForeignKey(
                    name: "fk_node_relation_node_ancestor_node_id",
                    column: x => x.ancestor_node_id,
                    principalSchema: "public",
                    principalTable: "node",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_node_relation_node_descendant_node_id",
                    column: x => x.descendant_node_id,
                    principalSchema: "public",
                    principalTable: "node",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "permission_assignments",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                subject_type = table.Column<int>(type: "integer", nullable: false),
                decision = table.Column<int>(type: "integer", nullable: false),
                subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                permission_code = table.Column<string>(type: "text", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                node_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_permission_assignments", x => x.id);
                table.ForeignKey(
                    name: "fk_permission_assignments_resource_nodes_node_id",
                    column: x => x.node_id,
                    principalSchema: "public",
                    principalTable: "resource_nodes",
                    principalColumn: "id");
            });

        migrationBuilder.CreateTable(
            name: "permission",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "text", nullable: false),
                description = table.Column<string>(type: "text", nullable: false),
                scope = table.Column<int>(type: "integer", nullable: false),
                role_id = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_permission", x => x.id);
                table.ForeignKey(
                    name: "fk_permission_role_role_id",
                    column: x => x.role_id,
                    principalSchema: "public",
                    principalTable: "role",
                    principalColumn: "id");
            });

        migrationBuilder.CreateTable(
            name: "ticket_lines",
            schema: "gaming",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                line_index = table.Column<int>(type: "integer", nullable: false),
                numbers_raw = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                play_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_ticket_lines", x => x.id);
                table.ForeignKey(
                    name: "fk_ticket_lines_tickets_ticket_id",
                    column: x => x.ticket_id,
                    principalSchema: "gaming",
                    principalTable: "tickets",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "members",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: true),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                member_no = table.Column<string>(type: "text", nullable: false),
                display_name = table.Column<string>(type: "text", nullable: false),
                status = table.Column<short>(type: "smallint", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_members", x => x.id);
                table.ForeignKey(
                    name: "fk_members_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "users",
                    principalColumn: "id");
            });

        migrationBuilder.CreateTable(
            name: "role_user",
            schema: "public",
            columns: table => new
            {
                roles_id = table.Column<int>(type: "integer", nullable: false),
                users_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_role_user", x => new { x.roles_id, x.users_id });
                table.ForeignKey(
                    name: "fk_role_user_role_roles_id",
                    column: x => x.roles_id,
                    principalSchema: "public",
                    principalTable: "role",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_role_user_users_users_id",
                    column: x => x.users_id,
                    principalSchema: "public",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_groups",
            schema: "public",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                group_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_groups", x => new { x.user_id, x.group_id });
                table.ForeignKey(
                    name: "fk_user_groups_groups_group_id",
                    column: x => x.group_id,
                    principalSchema: "public",
                    principalTable: "groups",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_user_groups_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_login_bindings",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                provider = table.Column<int>(type: "integer", nullable: false),
                provider_key = table.Column<string>(type: "text", nullable: false),
                normalized_provider_key = table.Column<string>(type: "text", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_login_bindings", x => x.id);
                table.ForeignKey(
                    name: "fk_user_login_bindings_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "public",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "member_activity_log",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "text", nullable: false),
                ip = table.Column<string>(type: "text", nullable: true),
                user_agent = table.Column<string>(type: "text", nullable: true),
                operator_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                payload = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_member_activity_log", x => x.id);
                table.ForeignKey(
                    name: "fk_member_activity_log_members_member_id",
                    column: x => x.member_id,
                    principalSchema: "public",
                    principalTable: "members",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "member_asset_balance",
            schema: "public",
            columns: table => new
            {
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                asset_code = table.Column<string>(type: "text", nullable: false),
                balance = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_member_asset_balance", x => new { x.member_id, x.asset_code });
                table.ForeignKey(
                    name: "fk_member_asset_balance_members_member_id",
                    column: x => x.member_id,
                    principalSchema: "public",
                    principalTable: "members",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "member_asset_ledger",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                asset_code = table.Column<string>(type: "text", nullable: false),
                type = table.Column<short>(type: "smallint", nullable: false),
                amount = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                before_balance = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                after_balance = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                reference_type = table.Column<string>(type: "text", nullable: true),
                reference_id = table.Column<string>(type: "text", nullable: true),
                operator_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                remark = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_member_asset_ledger", x => x.id);
                table.ForeignKey(
                    name: "fk_member_asset_ledger_members_member_id",
                    column: x => x.member_id,
                    principalSchema: "public",
                    principalTable: "members",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "member_point_balance",
            schema: "public",
            columns: table => new
            {
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                balance = table.Column<long>(type: "bigint", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_member_point_balance", x => x.member_id);
                table.ForeignKey(
                    name: "fk_member_point_balance_members_member_id",
                    column: x => x.member_id,
                    principalSchema: "public",
                    principalTable: "members",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "member_point_ledger",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<short>(type: "smallint", nullable: false),
                amount = table.Column<long>(type: "bigint", nullable: false),
                before_balance = table.Column<long>(type: "bigint", nullable: false),
                after_balance = table.Column<long>(type: "bigint", nullable: false),
                reference_type = table.Column<string>(type: "text", nullable: true),
                reference_id = table.Column<string>(type: "text", nullable: true),
                operator_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                remark = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_member_point_ledger", x => x.id);
                table.ForeignKey(
                    name: "fk_member_point_ledger_members_member_id",
                    column: x => x.member_id,
                    principalSchema: "public",
                    principalTable: "members",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_auth_sessions_tenant_user",
            schema: "public",
            table: "auth_sessions",
            columns: new[] { "tenant_id", "user_id" });

        migrationBuilder.CreateIndex(
            name: "ix_campaign_draws_campaign_id",
            schema: "gaming",
            table: "campaign_draws",
            column: "campaign_id");

        migrationBuilder.CreateIndex(
            name: "ix_campaign_draws_tenant_id_campaign_id",
            schema: "gaming",
            table: "campaign_draws",
            columns: new[] { "tenant_id", "campaign_id" });

        migrationBuilder.CreateIndex(
            name: "ix_campaign_draws_tenant_id_campaign_id_draw_id",
            schema: "gaming",
            table: "campaign_draws",
            columns: new[] { "tenant_id", "campaign_id", "draw_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_campaigns_tenant_id_game_code_play_type_code_grant_open_at_",
            schema: "gaming",
            table: "campaigns",
            columns: new[] { "tenant_id", "game_code", "play_type_code", "grant_open_at_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_draw_allowed_ticket_templates_tenant_id_draw_id_ticket_temp",
            schema: "gaming",
            table: "draw_allowed_ticket_templates",
            columns: new[] { "tenant_id", "draw_id", "ticket_template_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_draw_enabled_play_types_draw_id",
            schema: "gaming",
            table: "draw_enabled_play_types",
            column: "draw_id");

        migrationBuilder.CreateIndex(
            name: "ix_draw_enabled_play_types_tenant_id_draw_id_play_type_code",
            schema: "gaming",
            table: "draw_enabled_play_types",
            columns: new[] { "tenant_id", "draw_id", "play_type_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_draw_prize_pool_items_draw_id",
            schema: "gaming",
            table: "draw_prize_pool_items",
            column: "draw_id");

        migrationBuilder.CreateIndex(
            name: "ix_draw_prize_pool_items_tenant_id_draw_id_play_type_code_tier",
            schema: "gaming",
            table: "draw_prize_pool_items",
            columns: new[] { "tenant_id", "draw_id", "play_type_code", "tier" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_draw_template_allowed_ticket_templates_template_id",
            schema: "gaming",
            table: "draw_template_allowed_ticket_templates",
            column: "template_id");

        migrationBuilder.CreateIndex(
            name: "ix_draw_template_allowed_ticket_templates_tenant_id_template_i",
            schema: "gaming",
            table: "draw_template_allowed_ticket_templates",
            columns: new[] { "tenant_id", "template_id", "ticket_template_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_draw_template_play_types_template_id",
            schema: "gaming",
            table: "draw_template_play_types",
            column: "template_id");

        migrationBuilder.CreateIndex(
            name: "ix_draw_template_play_types_tenant_id_template_id_play_type_co",
            schema: "gaming",
            table: "draw_template_play_types",
            columns: new[] { "tenant_id", "template_id", "play_type_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_draw_template_prize_tiers_template_id",
            schema: "gaming",
            table: "draw_template_prize_tiers",
            column: "template_id");

        migrationBuilder.CreateIndex(
            name: "ix_draw_template_prize_tiers_tenant_id_template_id_play_type_c",
            schema: "gaming",
            table: "draw_template_prize_tiers",
            columns: new[] { "tenant_id", "template_id", "play_type_code", "tier" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_draw_templates_tenant_id_game_code_name",
            schema: "gaming",
            table: "draw_templates",
            columns: new[] { "tenant_id", "game_code", "name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_draws_tenant_id_game_code_draw_code",
            schema: "gaming",
            table: "draws",
            columns: new[] { "tenant_id", "game_code", "draw_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_draws_tenant_id_status",
            schema: "gaming",
            table: "draws",
            columns: new[] { "tenant_id", "status" });

        migrationBuilder.CreateIndex(
            name: "ix_member_activity_log_action_created_at",
            schema: "public",
            table: "member_activity_log",
            columns: new[] { "action", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_member_activity_log_member_id_created_at",
            schema: "public",
            table: "member_activity_log",
            columns: new[] { "member_id", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_member_asset_ledger_member_id_asset_code_created_at",
            schema: "public",
            table: "member_asset_ledger",
            columns: new[] { "member_id", "asset_code", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_member_point_ledger_member_id_created_at",
            schema: "public",
            table: "member_point_ledger",
            columns: new[] { "member_id", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_member_point_ledger_reference_type_reference_id",
            schema: "public",
            table: "member_point_ledger",
            columns: new[] { "reference_type", "reference_id" });

        migrationBuilder.CreateIndex(
            name: "ix_members_user_id",
            schema: "public",
            table: "members",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ux_members_tenant_id_member_no",
            schema: "public",
            table: "members",
            columns: new[] { "tenant_id", "member_no" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_members_tenant_id_user_id",
            schema: "public",
            table: "members",
            columns: new[] { "tenant_id", "user_id" },
            unique: true,
            filter: "user_id IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_node_relation_descendant_node_id",
            schema: "public",
            table: "node_relation",
            column: "descendant_node_id");

        migrationBuilder.CreateIndex(
            name: "ix_permission_role_id",
            schema: "public",
            table: "permission",
            column: "role_id");

        migrationBuilder.CreateIndex(
            name: "ix_permission_assignments_node_id",
            schema: "public",
            table: "permission_assignments",
            column: "node_id");

        migrationBuilder.CreateIndex(
            name: "ix_permission_assignments_permission_code",
            schema: "public",
            table: "permission_assignments",
            column: "permission_code");

        migrationBuilder.CreateIndex(
            name: "ix_permission_assignments_subject_type_subject_id",
            schema: "public",
            table: "permission_assignments",
            columns: new[] { "subject_type", "subject_id" });

        migrationBuilder.CreateIndex(
            name: "ix_permission_assignments_tenant_id",
            schema: "public",
            table: "permission_assignments",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_prize_awards_tenant_id_draw_id_ticket_id_line_index",
            schema: "gaming",
            table: "prize_awards",
            columns: new[] { "tenant_id", "draw_id", "ticket_id", "line_index" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_prizes_tenant_id_name",
            schema: "gaming",
            table: "prizes",
            columns: new[] { "tenant_id", "name" });

        migrationBuilder.CreateIndex(
            name: "ix_redeem_records_prize_award_id",
            schema: "gaming",
            table: "redeem_records",
            column: "prize_award_id",
            unique: true);

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

        migrationBuilder.CreateIndex(
            name: "ix_resource_nodes_parent_id",
            schema: "public",
            table: "resource_nodes",
            column: "parent_id");

        migrationBuilder.CreateIndex(
            name: "ix_resource_nodes_tenant_id",
            schema: "public",
            table: "resource_nodes",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ux_resource_nodes_tenant_id_external_key",
            schema: "public",
            table: "resource_nodes",
            columns: new[] { "tenant_id", "external_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_role_user_users_id",
            schema: "public",
            table: "role_user",
            column: "users_id");

        migrationBuilder.CreateIndex(
            name: "ux_tenant_game_entitlements_tenant_game",
            schema: "gaming",
            table: "tenant_game_entitlements",
            columns: new[] { "tenant_id", "game_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_tenant_play_entitlements_tenant_game_play",
            schema: "gaming",
            table: "tenant_play_entitlements",
            columns: new[] { "tenant_id", "game_code", "play_type_code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_tenants_code",
            schema: "public",
            table: "tenants",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_ticket_draws_tenant_id_draw_id_participation_status",
            schema: "gaming",
            table: "ticket_draws",
            columns: new[] { "tenant_id", "draw_id", "participation_status" });

        migrationBuilder.CreateIndex(
            name: "ix_ticket_draws_ticket_id_draw_id",
            schema: "gaming",
            table: "ticket_draws",
            columns: new[] { "ticket_id", "draw_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_ticket_idempotency_records_tenant_id_idempotency_key_operat",
            schema: "gaming",
            table: "ticket_idempotency_records",
            columns: new[] { "tenant_id", "idempotency_key", "operation" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_ticket_line_results_tenant_id_ticket_id_draw_id_line_index",
            schema: "gaming",
            table: "ticket_line_results",
            columns: new[] { "tenant_id", "ticket_id", "draw_id", "line_index" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_ticket_lines_ticket_id_line_index",
            schema: "gaming",
            table: "ticket_lines",
            columns: new[] { "ticket_id", "line_index" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_ticket_templates_tenant_id_code",
            schema: "gaming",
            table: "ticket_templates",
            columns: new[] { "tenant_id", "code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_tickets_tenant_id_member_id_created_at",
            schema: "gaming",
            table: "tickets",
            columns: new[] { "tenant_id", "member_id", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_user_groups_group_id",
            schema: "public",
            table: "user_groups",
            column: "group_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_groups_user_id_group_id",
            schema: "public",
            table: "user_groups",
            columns: new[] { "user_id", "group_id" });

        migrationBuilder.CreateIndex(
            name: "ux_login_bindings_tenant_provider_key",
            schema: "public",
            table: "user_login_bindings",
            columns: new[] { "tenant_id", "provider", "normalized_provider_key" },
            unique: true,
            filter: "tenant_id IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ux_login_bindings_user_provider",
            schema: "public",
            table: "user_login_bindings",
            columns: new[] { "user_id", "provider" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_users_tenant_id",
            schema: "public",
            table: "users",
            column: "tenant_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "campaign_draws",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "draw_allowed_ticket_templates",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "draw_enabled_play_types",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "draw_prize_pool_items",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "draw_sequences",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "draw_template_allowed_ticket_templates",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "draw_template_play_types",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "draw_template_prize_tiers",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "member_activity_log",
            schema: "public");

        migrationBuilder.DropTable(
            name: "member_asset_balance",
            schema: "public");

        migrationBuilder.DropTable(
            name: "member_asset_ledger",
            schema: "public");

        migrationBuilder.DropTable(
            name: "member_point_balance",
            schema: "public");

        migrationBuilder.DropTable(
            name: "member_point_ledger",
            schema: "public");

        migrationBuilder.DropTable(
            name: "node_relation",
            schema: "public");

        migrationBuilder.DropTable(
            name: "outbox_messages",
            schema: "public");

        migrationBuilder.DropTable(
            name: "permission",
            schema: "public");

        migrationBuilder.DropTable(
            name: "permission_assignments",
            schema: "public");

        migrationBuilder.DropTable(
            name: "prize_awards",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "prizes",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "redeem_records",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "refresh_token_records",
            schema: "public");

        migrationBuilder.DropTable(
            name: "role_user",
            schema: "public");

        migrationBuilder.DropTable(
            name: "tenant_game_entitlements",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "tenant_play_entitlements",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "tenants",
            schema: "public");

        migrationBuilder.DropTable(
            name: "ticket_draws",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "ticket_idempotency_records",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "ticket_line_results",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "ticket_lines",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "ticket_templates",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "user_groups",
            schema: "public");

        migrationBuilder.DropTable(
            name: "user_login_bindings",
            schema: "public");

        migrationBuilder.DropTable(
            name: "campaigns",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "draws",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "draw_templates",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "members",
            schema: "public");

        migrationBuilder.DropTable(
            name: "node",
            schema: "public");

        migrationBuilder.DropTable(
            name: "resource_nodes",
            schema: "public");

        migrationBuilder.DropTable(
            name: "auth_sessions",
            schema: "public");

        migrationBuilder.DropTable(
            name: "role",
            schema: "public");

        migrationBuilder.DropTable(
            name: "tickets",
            schema: "gaming");

        migrationBuilder.DropTable(
            name: "groups",
            schema: "public");

        migrationBuilder.DropTable(
            name: "users",
            schema: "public");
    }
}
