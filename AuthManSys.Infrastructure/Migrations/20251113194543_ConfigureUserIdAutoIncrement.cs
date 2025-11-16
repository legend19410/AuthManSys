using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthManSys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureUserIdAutoIncrement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add unique index for UserId
            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserId",
                table: "AspNetUsers",
                column: "UserId",
                unique: true);

            // For MySQL: Alter the existing UserId column to add AUTO_INCREMENT
            migrationBuilder.Sql("ALTER TABLE AspNetUsers MODIFY COLUMN UserId int NOT NULL AUTO_INCREMENT;");

            // Set the AUTO_INCREMENT starting value to 1 more than the current max
            migrationBuilder.Sql(@"
                SET @max_id = COALESCE((SELECT MAX(UserId) FROM AspNetUsers), 0);
                SET @sql = CONCAT('ALTER TABLE AspNetUsers AUTO_INCREMENT = ', @max_id + 1);
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove AUTO_INCREMENT from UserId column
            migrationBuilder.Sql("ALTER TABLE AspNetUsers MODIFY COLUMN UserId int NOT NULL;");

            // Drop the unique index
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_UserId",
                table: "AspNetUsers");
        }
    }
}
