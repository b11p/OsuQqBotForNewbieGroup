using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class ToMySQL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bindings",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    OsuId = table.Column<int>(nullable: false),
                    Source = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bindings", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "CachedBeatmaps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    AR = table.Column<double>(nullable: false),
                    Approved = table.Column<int>(nullable: false),
                    ApprovedDateOffset = table.Column<DateTimeOffset>(nullable: true),
                    Artist = table.Column<string>(nullable: false),
                    Bpm = table.Column<double>(nullable: false),
                    CS = table.Column<double>(nullable: false),
                    Creator = table.Column<string>(nullable: false),
                    DifficultyName = table.Column<string>(nullable: false),
                    FavoriteCount = table.Column<int>(nullable: false),
                    FileMD5 = table.Column<string>(nullable: false),
                    Genre = table.Column<int>(nullable: false),
                    HP = table.Column<double>(nullable: false),
                    HitLength = table.Column<int>(nullable: false),
                    Language = table.Column<int>(nullable: false),
                    LastUpdateOffset = table.Column<DateTimeOffset>(nullable: false),
                    MaxCombo = table.Column<int>(nullable: true),
                    OD = table.Column<double>(nullable: false),
                    SetId = table.Column<int>(nullable: false),
                    Source = table.Column<string>(nullable: false),
                    Stars = table.Column<double>(nullable: false),
                    Tags = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    TotalLength = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedBeatmaps", x => new { x.Id, x.Mode });
                });

            migrationBuilder.CreateTable(
                name: "Charts",
                columns: table => new
                {
                    ChartId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChartCreator = table.Column<long>(nullable: false),
                    ChartDescription = table.Column<string>(nullable: false),
                    ChartName = table.Column<string>(nullable: false),
                    EndTime = table.Column<DateTimeOffset>(nullable: true),
                    IsRunning = table.Column<bool>(nullable: false),
                    MaximumPerformance = table.Column<double>(nullable: true),
                    Public = table.Column<bool>(nullable: false),
                    RecommendPerformance = table.Column<double>(nullable: false),
                    StartTime = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charts", x => x.ChartId);
                });

            migrationBuilder.CreateTable(
                name: "Histories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(nullable: false),
                    Operation = table.Column<int>(nullable: false),
                    Operator = table.Column<string>(nullable: true),
                    OperatorId = table.Column<long>(nullable: false),
                    Remark = table.Column<string>(nullable: true),
                    User = table.Column<string>(nullable: true),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Histories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChartAdministrators",
                columns: table => new
                {
                    ChartId = table.Column<int>(nullable: false),
                    Administrator = table.Column<long>(nullable: false)
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
                    ChartId = table.Column<int>(nullable: false),
                    BeatmapId = table.Column<int>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    AllowsFail = table.Column<bool>(nullable: false),
                    BannedMods = table.Column<int>(nullable: false),
                    ForceMods = table.Column<int>(nullable: false),
                    RequiredMods = table.Column<int>(nullable: false),
                    ScoreCalculation = table.Column<string>(nullable: true)
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
                    ChartId = table.Column<int>(nullable: false),
                    GroupId = table.Column<long>(nullable: false)
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
                name: "ChartTries",
                columns: table => new
                {
                    ChartId = table.Column<int>(nullable: false),
                    BeatmapId = table.Column<int>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    Date = table.Column<long>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    Accuracy = table.Column<double>(nullable: false),
                    Combo = table.Column<int>(nullable: false),
                    Mods = table.Column<int>(nullable: false),
                    PPWhenCommit = table.Column<double>(nullable: false),
                    Rank = table.Column<string>(nullable: false),
                    Score = table.Column<long>(nullable: false)
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bindings");

            migrationBuilder.DropTable(
                name: "CachedBeatmaps");

            migrationBuilder.DropTable(
                name: "ChartAdministrators");

            migrationBuilder.DropTable(
                name: "ChartTries");

            migrationBuilder.DropTable(
                name: "ChartValidGroups");

            migrationBuilder.DropTable(
                name: "Histories");

            migrationBuilder.DropTable(
                name: "ChartBeatmaps");

            migrationBuilder.DropTable(
                name: "Charts");
        }
    }
}
