using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class RemoveBeatmapCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachedBeatmaps");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Version",
                table: "UpdateSchedules",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp(6)",
                oldNullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Version",
                table: "UpdateSchedules",
                type: "timestamp(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldRowVersion: true,
                oldNullable: true)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.CreateTable(
                name: "CachedBeatmaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    AR = table.Column<double>(type: "double", nullable: false),
                    Approved = table.Column<int>(type: "int", nullable: false),
                    ApprovedDateOffset = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    Artist = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    Bpm = table.Column<double>(type: "double", nullable: false),
                    CS = table.Column<double>(type: "double", nullable: false),
                    Creator = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    DifficultyName = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    FavoriteCount = table.Column<int>(type: "int", nullable: false),
                    FileMD5 = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
                    Genre = table.Column<int>(type: "int", nullable: false),
                    HP = table.Column<double>(type: "double", nullable: false),
                    HitLength = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<int>(type: "int", nullable: false),
                    LastUpdateOffset = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    MaxCombo = table.Column<int>(type: "int", nullable: true),
                    OD = table.Column<double>(type: "double", nullable: false),
                    SetId = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    Stars = table.Column<double>(type: "double", nullable: false),
                    Tags = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    Title = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    TotalLength = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedBeatmaps", x => new { x.Id, x.Mode });
                });

            migrationBuilder.CreateIndex(
                name: "index_md5",
                table: "CachedBeatmaps",
                column: "FileMD5");
        }
    }
}
