using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class UpdateRecommendation2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Recommendations_Left_Recommendation",
                table: "Recommendations");

            migrationBuilder.DropIndex(
                name: "IX_Recommendations_Left",
                table: "Recommendations");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Version",
                table: "UpdateSchedules",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp(6)",
                oldNullable: true)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Recommendations_Mode_Left_Recommendation",
                table: "Recommendations",
                columns: new[] { "Mode", "Left", "Recommendation" });

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_Mode_Left",
                table: "Recommendations",
                columns: new[] { "Mode", "Left" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Recommendations_Mode_Left_Recommendation",
                table: "Recommendations");

            migrationBuilder.DropIndex(
                name: "IX_Recommendations_Mode_Left",
                table: "Recommendations");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Version",
                table: "UpdateSchedules",
                type: "timestamp(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldRowVersion: true,
                oldNullable: true)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Recommendations_Left_Recommendation",
                table: "Recommendations",
                columns: new[] { "Left", "Recommendation" });

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_Left",
                table: "Recommendations",
                column: "Left");
        }
    }
}
