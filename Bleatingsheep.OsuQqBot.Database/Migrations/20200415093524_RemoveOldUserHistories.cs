using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class RemoveOldUserHistories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserHistories");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    AccuracyPercent = table.Column<double>(type: "double", nullable: false),
                    Count100 = table.Column<int>(type: "int", nullable: false),
                    Count300 = table.Column<int>(type: "int", nullable: false),
                    Count50 = table.Column<int>(type: "int", nullable: false),
                    CountRankA = table.Column<int>(type: "int", nullable: false),
                    CountRankS = table.Column<int>(type: "int", nullable: false),
                    CountRankSH = table.Column<int>(type: "int", nullable: false),
                    CountRankSS = table.Column<int>(type: "int", nullable: false),
                    CountRankSSH = table.Column<int>(type: "int", nullable: false),
                    CountryCode = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    CountryRank = table.Column<int>(type: "int", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Level = table.Column<double>(type: "double", nullable: false),
                    Name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    Performance = table.Column<double>(type: "double", nullable: false),
                    PlayCount = table.Column<int>(type: "int", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    RankedScore = table.Column<long>(type: "bigint", nullable: false),
                    TotalScore = table.Column<long>(type: "bigint", nullable: false),
                    TotalSecondsPlayed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHistories", x => new { x.Id, x.Date, x.Mode });
                });
        }
    }
}
