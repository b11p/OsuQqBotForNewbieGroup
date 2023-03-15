using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOldConcurrencyCheckFieldDueToType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "UpdateSchedules");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "BotUserFields");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "BotGroupFields");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "UpdateSchedules",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "BotUserFields",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "BotGroupFields",
                type: "bytea",
                rowVersion: true,
                nullable: true);
        }
    }
}
