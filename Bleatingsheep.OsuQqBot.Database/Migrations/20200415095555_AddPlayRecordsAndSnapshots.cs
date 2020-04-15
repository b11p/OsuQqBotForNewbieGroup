using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class AddPlayRecordsAndSnapshots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPlayRecords",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    PlayNumber = table.Column<int>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    Record_BeatmapId = table.Column<int>(nullable: true),
                    Record_Score = table.Column<long>(nullable: true),
                    Record_MaxCombo = table.Column<int>(nullable: true),
                    Record_Count50 = table.Column<int>(nullable: true),
                    Record_Count100 = table.Column<int>(nullable: true),
                    Record_Count300 = table.Column<int>(nullable: true),
                    Record_CountMiss = table.Column<int>(nullable: true),
                    Record_CountKatu = table.Column<int>(nullable: true),
                    Record_CountGeki = table.Column<int>(nullable: true),
                    Record_Perfect = table.Column<bool>(nullable: true),
                    Record_EnabledMods = table.Column<int>(nullable: true),
                    Record_UserId = table.Column<int>(nullable: true),
                    Record_Date = table.Column<DateTime>(nullable: true),
                    Record_Rank = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlayRecords", x => new { x.UserId, x.Mode, x.PlayNumber });
                });

            migrationBuilder.CreateTable(
                name: "UserSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    Date = table.Column<DateTimeOffset>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserInfo_Id = table.Column<long>(nullable: true),
                    UserInfo_Name = table.Column<string>(nullable: true),
                    UserInfo_JoinDate = table.Column<DateTime>(nullable: true),
                    UserInfo_Count300 = table.Column<int>(nullable: true),
                    UserInfo_Count100 = table.Column<int>(nullable: true),
                    UserInfo_Count50 = table.Column<int>(nullable: true),
                    UserInfo_PlayCount = table.Column<int>(nullable: true),
                    UserInfo_RankedScore = table.Column<long>(nullable: true),
                    UserInfo_TotalScore = table.Column<long>(nullable: true),
                    UserInfo_Rank = table.Column<int>(nullable: true),
                    UserInfo_Level = table.Column<double>(nullable: true),
                    UserInfo_Performance = table.Column<double>(nullable: true),
                    UserInfo_AccuracyPercent = table.Column<double>(nullable: true),
                    UserInfo_CountRankSS = table.Column<int>(nullable: true),
                    UserInfo_CountRankSSH = table.Column<int>(nullable: true),
                    UserInfo_CountRankS = table.Column<int>(nullable: true),
                    UserInfo_CountRankSH = table.Column<int>(nullable: true),
                    UserInfo_CountRankA = table.Column<int>(nullable: true),
                    UserInfo_CountryCode = table.Column<string>(nullable: true),
                    UserInfo_TotalSecondsPlayed = table.Column<int>(nullable: true),
                    UserInfo_CountryRank = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSnapshots", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPlayRecords");

            migrationBuilder.DropTable(
                name: "UserSnapshots");
        }
    }
}
