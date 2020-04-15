using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class RemoveChartsAndUserGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChartAdministrators");

            migrationBuilder.DropTable(
                name: "ChartTries");

            migrationBuilder.DropTable(
                name: "ChartValidGroups");

            migrationBuilder.DropTable(
                name: "GroupMemberInfo");

            migrationBuilder.DropTable(
                name: "ChartBeatmaps");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Charts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Charts",
                columns: table => new
                {
                    ChartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChartCreator = table.Column<long>(type: "bigint", nullable: false),
                    ChartDescription = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    ChartName = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    IsRunning = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaximumPerformance = table.Column<double>(type: "double", nullable: true),
                    Public = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RecommendPerformance = table.Column<double>(type: "double", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charts", x => x.ChartId);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Name = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    HadBeenWelcome = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "ChartAdministrators",
                columns: table => new
                {
                    ChartId = table.Column<int>(type: "int", nullable: false),
                    Administrator = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartAdministrators", x => new { x.ChartId, x.Administrator });
                    table.ForeignKey(
                        name: "FK_ChartAdministrators_Charts_ChartId",
                        column: x => x.ChartId,
                        principalTable: "Charts",
                        principalColumn: "ChartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChartBeatmaps",
                columns: table => new
                {
                    ChartId = table.Column<int>(type: "int", nullable: false),
                    BeatmapId = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    AllowsFail = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BannedMods = table.Column<int>(type: "int", nullable: false),
                    ForceMods = table.Column<int>(type: "int", nullable: false),
                    RequiredMods = table.Column<int>(type: "int", nullable: false),
                    ScoreCalculation = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartBeatmaps", x => new { x.ChartId, x.BeatmapId, x.Mode });
                    table.ForeignKey(
                        name: "FK_ChartBeatmaps_Charts_ChartId",
                        column: x => x.ChartId,
                        principalTable: "Charts",
                        principalColumn: "ChartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChartValidGroups",
                columns: table => new
                {
                    ChartId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartValidGroups", x => new { x.ChartId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_ChartValidGroups_Charts_ChartId",
                        column: x => x.ChartId,
                        principalTable: "Charts",
                        principalColumn: "ChartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupMemberInfo",
                columns: table => new
                {
                    GroupName = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMemberInfo", x => new { x.GroupName, x.UserId });
                    table.ForeignKey(
                        name: "FK_GroupMemberInfo_Groups_GroupName",
                        column: x => x.GroupName,
                        principalTable: "Groups",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMemberInfo_Members_UserId",
                        column: x => x.UserId,
                        principalTable: "Members",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChartTries",
                columns: table => new
                {
                    ChartId = table.Column<int>(type: "int", nullable: false),
                    BeatmapId = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Accuracy = table.Column<double>(type: "double", nullable: false),
                    Combo = table.Column<int>(type: "int", nullable: false),
                    Mods = table.Column<int>(type: "int", nullable: false),
                    PPWhenCommit = table.Column<double>(type: "double", nullable: false),
                    Rank = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    Score = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartTries", x => new { x.ChartId, x.BeatmapId, x.Mode, x.Date, x.UserId });
                    table.ForeignKey(
                        name: "FK_ChartTries_Charts_ChartId",
                        column: x => x.ChartId,
                        principalTable: "Charts",
                        principalColumn: "ChartId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChartTries_ChartBeatmaps_ChartId_BeatmapId_Mode",
                        columns: x => new { x.ChartId, x.BeatmapId, x.Mode },
                        principalTable: "ChartBeatmaps",
                        principalColumns: new[] { "ChartId", "BeatmapId", "Mode" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberInfo_UserId",
                table: "GroupMemberInfo",
                column: "UserId");
        }
    }
}
