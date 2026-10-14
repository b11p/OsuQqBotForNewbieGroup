using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    /// <inheritdoc />
    public partial class IndexUpdateSchedule_NextUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserPlayRecords_Record_Date",
                table: "UserPlayRecords",
                column: "Record_Date");

            migrationBuilder.CreateIndex(
                name: "IX_UpdateSchedules_NextUpdate",
                table: "UpdateSchedules",
                column: "NextUpdate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserPlayRecords_Record_Date",
                table: "UserPlayRecords");

            migrationBuilder.DropIndex(
                name: "IX_UpdateSchedules_NextUpdate",
                table: "UpdateSchedules");
        }
    }
}
