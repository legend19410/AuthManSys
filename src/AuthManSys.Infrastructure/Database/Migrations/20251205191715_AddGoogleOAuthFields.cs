using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthManSys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleOAuthFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleEmail",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GoogleId",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "GoogleLinkedAt",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GooglePictureUrl",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsGoogleAccount",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "AspNetUserRoles",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "AssignedBy",
                table: "AspNetUserRoles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetRoles",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "AspNetRoles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AspNetRoles",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_AssignedAt",
                table: "AspNetUserRoles",
                column: "AssignedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UserId_AssignedAt",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "AssignedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_CreatedAt",
                table: "AspNetRoles",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_AssignedAt",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_UserId_AssignedAt",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_CreatedAt",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "GoogleEmail",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GoogleId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GoogleLinkedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GooglePictureUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsGoogleAccount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "AssignedBy",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AspNetRoles");
        }
    }
}
