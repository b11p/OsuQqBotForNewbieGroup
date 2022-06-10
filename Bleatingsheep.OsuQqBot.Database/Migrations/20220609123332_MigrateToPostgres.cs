using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class MigrateToPostgres : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeatmapPlusCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Stars = table.Column<float>(type: "real", nullable: false),
                    AimTotal = table.Column<float>(type: "real", nullable: false),
                    AimJump = table.Column<float>(type: "real", nullable: false),
                    AimFlow = table.Column<float>(type: "real", nullable: false),
                    Precision = table.Column<float>(type: "real", nullable: false),
                    Speed = table.Column<float>(type: "real", nullable: false),
                    Stamina = table.Column<float>(type: "real", nullable: false),
                    Accuracy = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeatmapPlusCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bindings",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    OsuId = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bindings", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "DuplicateAuthentication",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SelfId = table.Column<long>(type: "bigint", nullable: false),
                    AccessToken = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateAuthentication", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Histories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    User = table.Column<string>(type: "text", nullable: true),
                    Operation = table.Column<int>(type: "integer", nullable: false),
                    OperatorId = table.Column<long>(type: "bigint", nullable: true),
                    Operator = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Remark = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Histories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Raw = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayRecordQueryTemps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    StartNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayRecordQueryTemps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Performance = table.Column<int>(type: "integer", nullable: false),
                    AimTotal = table.Column<int>(type: "integer", nullable: false),
                    AimJump = table.Column<int>(type: "integer", nullable: false),
                    AimFlow = table.Column<int>(type: "integer", nullable: false),
                    Precision = table.Column<int>(type: "integer", nullable: false),
                    Speed = table.Column<int>(type: "integer", nullable: false),
                    Stamina = table.Column<int>(type: "integer", nullable: false),
                    Accuracy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlusHistories", x => new { x.Id, x.Date });
                });

            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    Left = table.Column<long>(type: "bigint", nullable: false),
                    Recommendation = table.Column<long>(type: "bigint", nullable: false),
                    RecommendationDegree = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.Id);
                    table.UniqueConstraint("AK_Recommendations_Mode_Left_Recommendation", x => new { x.Mode, x.Left, x.Recommendation });
                });

            migrationBuilder.CreateTable(
                name: "Relationships",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Relationship = table.Column<string>(type: "text", nullable: false),
                    Target = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relationships", x => new { x.UserId, x.Relationship });
                });

            migrationBuilder.CreateTable(
                name: "UpdateSchedules",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    NextUpdate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActiveIndex = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateSchedules", x => new { x.UserId, x.Mode });
                });

            migrationBuilder.CreateTable(
                name: "UserPlayRecords",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PlayNumber = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    Record_BeatmapId = table.Column<int>(type: "integer", nullable: false),
                    Record_Score = table.Column<long>(type: "bigint", nullable: false),
                    Record_MaxCombo = table.Column<int>(type: "integer", nullable: false),
                    Record_Count50 = table.Column<int>(type: "integer", nullable: false),
                    Record_Count100 = table.Column<int>(type: "integer", nullable: false),
                    Record_Count300 = table.Column<int>(type: "integer", nullable: false),
                    Record_CountMiss = table.Column<int>(type: "integer", nullable: false),
                    Record_CountKatu = table.Column<int>(type: "integer", nullable: false),
                    Record_CountGeki = table.Column<int>(type: "integer", nullable: false),
                    Record_Perfect = table.Column<bool>(type: "boolean", nullable: false),
                    Record_EnabledMods = table.Column<int>(type: "integer", nullable: false),
                    Record_UserId = table.Column<int>(type: "integer", nullable: false),
                    Record_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Record_Rank = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlayRecords", x => new { x.UserId, x.Mode, x.PlayNumber });
                });

            migrationBuilder.CreateTable(
                name: "UserSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserInfo_Id = table.Column<long>(type: "bigint", nullable: false),
                    UserInfo_Name = table.Column<string>(type: "text", nullable: true),
                    UserInfo_JoinDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserInfo_Count300 = table.Column<int>(type: "integer", nullable: false),
                    UserInfo_Count100 = table.Column<int>(type: "integer", nullable: false),
                    UserInfo_Count50 = table.Column<int>(type: "integer", nullable: false),
                    UserInfo_PlayCount = table.Column<int>(type: "integer", nullable: false),
                    UserInfo_RankedScore = table.Column<long>(type: "bigint", nullable: false),
                    UserInfo_TotalScore = table.Column<long>(type: "bigint", nullable: false),
                    UserInfo_Rank = table.Column<int>(type: "integer", nullable: false),
                    UserInfo_Level = table.Column<double>(type: "double precision", nullable: false),
                    UserInfo_Performance = table.Column<double>(type: "double precision", nullable: false),
                    UserInfo_AccuracyPercent = table.Column<double>(type: "double precision", nullable: false),
                    UserInfo_CountRankSS = table.Column<int>(type: "integer", nullable: false),
                    UserInfo_CountRankSSH = table.Column<int>(type: "integer", nullable: false),
                    UserInfo_CountRankS = table.Column<int>(type: "integer", nullable: false),
                    UserInfo_CountRankSH = table.Column<int>(type: "integer", nullable: false),
                    UserInfo_CountRankA = table.Column<int>(type: "integer", nullable: false),
                    UserInfo_CountryCode = table.Column<string>(type: "text", nullable: true),
                    UserInfo_TotalSecondsPlayed = table.Column<int>(type: "integer", nullable: false),
                    UserInfo_CountryRank = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    User = table.Column<string>(type: "text", nullable: true),
                    Token = table.Column<string>(type: "text", nullable: true),
                    Time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IPAddress = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    Kind = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateAuthentication_SelfId",
                table: "DuplicateAuthentication",
                column: "SelfId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayRecordQueryTemps_UserId_Mode",
                table: "PlayRecordQueryTemps",
                columns: new[] { "UserId", "Mode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_Mode_Left",
                table: "Recommendations",
                columns: new[] { "Mode", "Left" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSnapshots_UserId_Mode_Date",
                table: "UserSnapshots",
                columns: new[] { "UserId", "Mode", "Date" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeatmapPlusCache");

            migrationBuilder.DropTable(
                name: "Bindings");

            migrationBuilder.DropTable(
                name: "DuplicateAuthentication");

            migrationBuilder.DropTable(
                name: "Histories");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "PlayRecordQueryTemps");

            migrationBuilder.DropTable(
                name: "PlusHistories");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "Relationships");

            migrationBuilder.DropTable(
                name: "UpdateSchedules");

            migrationBuilder.DropTable(
                name: "UserPlayRecords");

            migrationBuilder.DropTable(
                name: "UserSnapshots");

            migrationBuilder.DropTable(
                name: "WebLogs");
        }
    }
}
