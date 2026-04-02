using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace edtech_platform_api.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceAndIpToUserSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "UserSessions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IPAddress",
                table: "UserSessions",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "IPAddress",
                table: "UserSessions");
        }
    }
}
