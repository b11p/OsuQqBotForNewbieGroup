using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPlayRecordId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPlayRecords",
                table: "UserPlayRecords");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "UserPlayRecords",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPlayRecords",
                table: "UserPlayRecords",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserPlayRecords_UserId_Mode_PlayNumber",
                table: "UserPlayRecords",
                columns: new[] { "UserId", "Mode", "PlayNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPlayRecords",
                table: "UserPlayRecords");

            migrationBuilder.DropIndex(
                name: "IX_UserPlayRecords_UserId_Mode_PlayNumber",
                table: "UserPlayRecords");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserPlayRecords");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPlayRecords",
                table: "UserPlayRecords",
                columns: new[] { "UserId", "Mode", "PlayNumber" });
        }
    }
}
