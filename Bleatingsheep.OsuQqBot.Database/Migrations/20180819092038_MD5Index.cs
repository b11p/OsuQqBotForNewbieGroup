using Microsoft.EntityFrameworkCore.Migrations;

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class MD5Index : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FileMD5",
                table: "CachedBeatmaps",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "index_md5",
                table: "CachedBeatmaps",
                column: "FileMD5");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "index_md5",
                table: "CachedBeatmaps");

            migrationBuilder.AlterColumn<string>(
                name: "FileMD5",
                table: "CachedBeatmaps",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
