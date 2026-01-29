using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddMemberProfile : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "display_name",
            schema: "public",
            table: "user_login_bindings",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "email",
            schema: "public",
            table: "user_login_bindings",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "picture_url",
            schema: "public",
            table: "user_login_bindings",
            type: "character varying(2048)",
            maxLength: 2048,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "member_addresses",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                receiver_name = table.Column<string>(type: "text", nullable: false),
                phone_number = table.Column<string>(type: "text", nullable: false),
                country = table.Column<string>(type: "text", nullable: false),
                city = table.Column<string>(type: "text", nullable: false),
                district = table.Column<string>(type: "text", nullable: false),
                address_line = table.Column<string>(type: "text", nullable: false),
                is_default = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_member_addresses", x => x.id);
                table.ForeignKey(
                    name: "fk_member_addresses_members_member_id",
                    column: x => x.member_id,
                    principalSchema: "public",
                    principalTable: "members",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "member_profiles",
            schema: "public",
            columns: table => new
            {
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                real_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                gender = table.Column<short>(type: "smallint", nullable: false),
                phone_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                phone_verified = table.Column<bool>(type: "boolean", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_member_profiles", x => x.member_id);
                table.ForeignKey(
                    name: "fk_member_profiles_members_member_id",
                    column: x => x.member_id,
                    principalSchema: "public",
                    principalTable: "members",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_member_addresses_member_id",
            schema: "public",
            table: "member_addresses",
            column: "member_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "member_addresses",
            schema: "public");

        migrationBuilder.DropTable(
            name: "member_profiles",
            schema: "public");

        migrationBuilder.DropColumn(
            name: "display_name",
            schema: "public",
            table: "user_login_bindings");

        migrationBuilder.DropColumn(
            name: "email",
            schema: "public",
            table: "user_login_bindings");

        migrationBuilder.DropColumn(
            name: "picture_url",
            schema: "public",
            table: "user_login_bindings");
    }
}
