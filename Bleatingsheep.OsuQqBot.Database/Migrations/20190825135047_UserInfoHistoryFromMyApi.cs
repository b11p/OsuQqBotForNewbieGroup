using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class UserInfoHistoryFromMyApi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserHistories",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    JoinDate = table.Column<DateTime>(nullable: false),
                    Count300 = table.Column<int>(nullable: false),
                    Count100 = table.Column<int>(nullable: false),
                    Count50 = table.Column<int>(nullable: false),
                    PlayCount = table.Column<int>(nullable: false),
                    RankedScore = table.Column<long>(nullable: false),
                    TotalScore = table.Column<long>(nullable: false),
                    Rank = table.Column<int>(nullable: false),
                    Level = table.Column<double>(nullable: false),
                    Performance = table.Column<double>(nullable: false),
                    AccuracyPercent = table.Column<double>(nullable: false),
                    CountRankSS = table.Column<int>(nullable: false),
                    CountRankSSH = table.Column<int>(nullable: false),
                    CountRankS = table.Column<int>(nullable: false),
                    CountRankSH = table.Column<int>(nullable: false),
                    CountRankA = table.Column<int>(nullable: false),
                    CountryCode = table.Column<string>(nullable: false),
                    TotalSecondsPlayed = table.Column<int>(nullable: false),
                    CountryRank = table.Column<int>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    Date = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHistories", x => new { x.Id, x.Date, x.Mode });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserHistories");
        }
    }
}
