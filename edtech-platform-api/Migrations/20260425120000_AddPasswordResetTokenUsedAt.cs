using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace edtech_platform_api.Migrations
{
    public partial class AddPasswordResetTokenUsedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UsedAt",
                table: "PasswordResetTokens",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsedAt",
                table: "PasswordResetTokens");
        }
    }
}
