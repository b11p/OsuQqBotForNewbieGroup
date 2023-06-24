using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddRecommendationPP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "UpdateSchedules",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<double>(
                name: "Performance",
                table: "Recommendations",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "BotUserFields",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "BotGroupFields",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "UpdateSchedules");

            migrationBuilder.DropColumn(
                name: "Performance",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "BotUserFields");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "BotGroupFields");
        }
    }
}
