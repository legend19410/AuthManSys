using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthManSys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedAtToUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add AssignedAt column to AspNetUserRoles table
            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "AspNetUserRoles",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Add AssignedBy column to AspNetUserRoles table
            migrationBuilder.AddColumn<int>(
                name: "AssignedBy",
                table: "AspNetUserRoles",
                type: "int",
                nullable: true);

            // Add Description column to AspNetRoles table
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AspNetRoles",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            // Add CreatedAt column to AspNetRoles table
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetRoles",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Add CreatedBy column to AspNetRoles table
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "AspNetRoles",
                type: "int",
                nullable: true);

            // Add indexes
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

            // Update existing records with default values (current timestamp)
            migrationBuilder.Sql(@"
                UPDATE AspNetUserRoles
                SET AssignedAt = CONVERT_TZ(UTC_TIMESTAMP(), 'UTC', '-05:00');

                UPDATE AspNetRoles
                SET CreatedAt = CONVERT_TZ(UTC_TIMESTAMP(), 'UTC', '-05:00');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_AssignedAt",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_UserId_AssignedAt",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_CreatedAt",
                table: "AspNetRoles");

            // Drop columns from AspNetUserRoles
            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "AssignedBy",
                table: "AspNetUserRoles");

            // Drop columns from AspNetRoles
            migrationBuilder.DropColumn(
                name: "Description",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AspNetRoles");
        }
    }
}