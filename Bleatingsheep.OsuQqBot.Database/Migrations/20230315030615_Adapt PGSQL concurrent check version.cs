using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    /// <inheritdoc />
    public partial class AdaptPGSQLconcurrentcheckversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Version",
                table: "UpdateSchedules",
                newName: "xmin");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "BotUserFields",
                newName: "xmin");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "BotGroupFields",
                newName: "xmin");

            migrationBuilder.AlterColumn<uint>(
                name: "xmin",
                table: "UpdateSchedules",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true,
                oldNullable: true);

            migrationBuilder.AlterColumn<uint>(
                name: "xmin",
                table: "BotUserFields",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true,
                oldNullable: true);

            migrationBuilder.AlterColumn<uint>(
                name: "xmin",
                table: "BotGroupFields",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "xmin",
                table: "UpdateSchedules",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "xmin",
                table: "BotUserFields",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "xmin",
                table: "BotGroupFields",
                newName: "Version");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Version",
                table: "UpdateSchedules",
                type: "bytea",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Version",
                table: "BotUserFields",
                type: "bytea",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Version",
                table: "BotGroupFields",
                type: "bytea",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true);
        }
    }
}
