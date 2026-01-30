using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class mdfyMemberProfile : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "pk_member_profiles",
            schema: "public",
            table: "member_profiles");

        migrationBuilder.AddColumn<Guid>(
            name: "id",
            schema: "public",
            table: "member_profiles",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.AddPrimaryKey(
            name: "pk_member_profiles",
            schema: "public",
            table: "member_profiles",
            column: "id");

        migrationBuilder.CreateIndex(
            name: "ux_member_profiles_member_id",
            schema: "public",
            table: "member_profiles",
            column: "member_id",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "pk_member_profiles",
            schema: "public",
            table: "member_profiles");

        migrationBuilder.DropIndex(
            name: "ux_member_profiles_member_id",
            schema: "public",
            table: "member_profiles");

        migrationBuilder.DropColumn(
            name: "id",
            schema: "public",
            table: "member_profiles");

        migrationBuilder.AddPrimaryKey(
            name: "pk_member_profiles",
            schema: "public",
            table: "member_profiles",
            column: "member_id");
    }
}
