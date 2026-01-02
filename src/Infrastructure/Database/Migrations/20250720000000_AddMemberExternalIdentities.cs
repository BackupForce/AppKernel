using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddMemberExternalIdentities : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ⚠️ 注意：Migration 為不可逆變更，請在部署前確認備份與變更窗口。
        migrationBuilder.CreateTable(
            name: "member_external_identities",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                provider = table.Column<string>(type: "text", nullable: false),
                external_user_id = table.Column<string>(type: "text", nullable: false),
                external_user_name = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_member_external_identities", x => x.id);
                table.ForeignKey(
                    name: "fk_member_external_identities_members_member_id",
                    column: x => x.member_id,
                    principalSchema: "public",
                    principalTable: "members",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_member_external_identities_member_id",
            schema: "public",
            table: "member_external_identities",
            column: "member_id");

        migrationBuilder.CreateIndex(
            name: "ix_member_external_identities_provider_external_user_id",
            schema: "public",
            table: "member_external_identities",
            columns: new[] { "provider", "external_user_id" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "member_external_identities",
            schema: "public");
    }
}
