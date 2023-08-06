﻿// <auto-generated />
using System;
using System.Text.Json;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    [DbContext(typeof(NewbieContext))]
    partial class NewbieContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Bleatingsheep.Osu.PerformancePlus.BeatmapPlus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.Property<float>("Accuracy")
                        .HasColumnType("real");

                    b.Property<float>("AimFlow")
                        .HasColumnType("real");

                    b.Property<float>("AimJump")
                        .HasColumnType("real");

                    b.Property<float>("AimTotal")
                        .HasColumnType("real");

                    b.Property<float>("Precision")
                        .HasColumnType("real");

                    b.Property<float>("Speed")
                        .HasColumnType("real");

                    b.Property<float>("Stamina")
                        .HasColumnType("real");

                    b.Property<float>("Stars")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.ToTable("BeatmapPlusCache");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.BeatmapInfoCacheEntry", b =>
                {
                    b.Property<int>("BeatmapId")
                        .HasColumnType("integer");

                    b.Property<int>("Mode")
                        .HasColumnType("integer");

                    b.Property<BeatmapInfo>("BeatmapInfo")
                        .HasColumnType("jsonb");

                    b.Property<DateTimeOffset>("CacheDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("ExpirationDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("BeatmapId", "Mode");

                    b.HasIndex("ExpirationDate");

                    b.ToTable("BeatmapInfoCache");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.BindingInfo", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<int>("OsuId")
                        .IsConcurrencyToken()
                        .HasColumnType("integer");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("UserId");

                    b.ToTable("Bindings");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.BotGroupField", b =>
                {
                    b.Property<long>("GroupId")
                        .HasColumnType("bigint");

                    b.Property<string>("FieldName")
                        .HasColumnType("text");

                    b.Property<JsonDocument>("Data")
                        .HasColumnType("jsonb");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("GroupId", "FieldName");

                    b.HasIndex("FieldName");

                    b.ToTable("BotGroupFields");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.BotUserField", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<string>("FieldName")
                        .HasColumnType("text");

                    b.Property<JsonDocument>("Data")
                        .HasColumnType("jsonb");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("UserId", "FieldName");

                    b.HasIndex("FieldName");

                    b.ToTable("BotUserFields");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.DuplicateAuthentication", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("AccessToken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("SelfId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("SelfId")
                        .IsUnique();

                    b.ToTable("DuplicateAuthentication");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.MessageEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("GroupId")
                        .HasColumnType("bigint");

                    b.Property<string>("Raw")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.OperationHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Operation")
                        .HasColumnType("integer");

                    b.Property<string>("Operator")
                        .HasColumnType("text");

                    b.Property<long?>("OperatorId")
                        .HasColumnType("bigint");

                    b.Property<string>("Remark")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("User")
                        .HasColumnType("text");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("Histories");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.PlayRecordQueryTemp", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Mode")
                        .HasColumnType("integer");

                    b.Property<int>("StartNumber")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "Mode")
                        .IsUnique();

                    b.ToTable("PlayRecordQueryTemps");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.PlusHistory", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Accuracy")
                        .HasColumnType("integer");

                    b.Property<int>("AimFlow")
                        .HasColumnType("integer");

                    b.Property<int>("AimJump")
                        .HasColumnType("integer");

                    b.Property<int>("AimTotal")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Performance")
                        .HasColumnType("integer");

                    b.Property<int>("Precision")
                        .HasColumnType("integer");

                    b.Property<int>("Speed")
                        .HasColumnType("integer");

                    b.Property<int>("Stamina")
                        .HasColumnType("integer");

                    b.HasKey("Id", "Date");

                    b.ToTable("PlusHistories");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.RecommendationEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<long>("Left")
                        .HasColumnType("bigint");

                    b.Property<int>("Mode")
                        .HasColumnType("integer");

                    b.Property<double>("Performance")
                        .HasColumnType("double precision");

                    b.Property<long>("Recommendation")
                        .HasColumnType("bigint");

                    b.Property<double>("RecommendationDegree")
                        .HasColumnType("double precision");

                    b.HasKey("Id");

                    b.HasAlternateKey("Mode", "Left", "Recommendation");

                    b.HasIndex("Mode", "Left");

                    b.ToTable("Recommendations");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.RelationshipInfo", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<string>("Relationship")
                        .HasColumnType("text");

                    b.Property<int>("Target")
                        .IsConcurrencyToken()
                        .HasColumnType("integer");

                    b.HasKey("UserId", "Relationship");

                    b.ToTable("Relationships");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.UpdateSchedule", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<int>("Mode")
                        .HasColumnType("integer");

                    b.Property<int>("ActiveIndex")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("NextUpdate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("UserId", "Mode");

                    b.ToTable("UpdateSchedules");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.UserPlayRecord", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("Mode")
                        .HasColumnType("integer");

                    b.Property<int>("PlayNumber")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "Mode", "PlayNumber");

                    b.ToTable("UserPlayRecords");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.UserSnapshot", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTimeOffset>("Date")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Mode")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "Mode", "Date");

                    b.ToTable("UserSnapshots");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.WebLog", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("IPAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Kind")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Location")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("Time")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Token")
                        .HasColumnType("text");

                    b.Property<string>("User")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("WebLogs");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.UserPlayRecord", b =>
                {
                    b.OwnsOne("Bleatingsheep.Osu.ApiClient.UserRecent", "Record", b1 =>
                        {
                            b1.Property<long>("UserPlayRecordId")
                                .HasColumnType("bigint");

                            b1.Property<int>("BeatmapId")
                                .HasColumnType("integer");

                            b1.Property<int>("Count100")
                                .HasColumnType("integer");

                            b1.Property<int>("Count300")
                                .HasColumnType("integer");

                            b1.Property<int>("Count50")
                                .HasColumnType("integer");

                            b1.Property<int>("CountGeki")
                                .HasColumnType("integer");

                            b1.Property<int>("CountKatu")
                                .HasColumnType("integer");

                            b1.Property<int>("CountMiss")
                                .HasColumnType("integer");

                            b1.Property<DateTime>("Date")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<int>("EnabledMods")
                                .HasColumnType("integer");

                            b1.Property<int>("MaxCombo")
                                .HasColumnType("integer");

                            b1.Property<bool>("Perfect")
                                .HasColumnType("boolean");

                            b1.Property<string>("Rank")
                                .HasColumnType("text");

                            b1.Property<long>("Score")
                                .HasColumnType("bigint");

                            b1.Property<int>("UserId")
                                .HasColumnType("integer");

                            b1.HasKey("UserPlayRecordId");

                            b1.ToTable("UserPlayRecords");

                            b1.WithOwner()
                                .HasForeignKey("UserPlayRecordId");
                        });

                    b.Navigation("Record")
                        .IsRequired();
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.UserSnapshot", b =>
                {
                    b.OwnsOne("Bleatingsheep.Osu.ApiClient.UserInfo", "UserInfo", b1 =>
                        {
                            b1.Property<long>("UserSnapshotId")
                                .HasColumnType("bigint");

                            b1.Property<double>("AccuracyPercent")
                                .HasColumnType("double precision");

                            b1.Property<int>("Count100")
                                .HasColumnType("integer");

                            b1.Property<int>("Count300")
                                .HasColumnType("integer");

                            b1.Property<int>("Count50")
                                .HasColumnType("integer");

                            b1.Property<int>("CountRankA")
                                .HasColumnType("integer");

                            b1.Property<int>("CountRankS")
                                .HasColumnType("integer");

                            b1.Property<int>("CountRankSH")
                                .HasColumnType("integer");

                            b1.Property<int>("CountRankSS")
                                .HasColumnType("integer");

                            b1.Property<int>("CountRankSSH")
                                .HasColumnType("integer");

                            b1.Property<string>("CountryCode")
                                .HasColumnType("text");

                            b1.Property<int>("CountryRank")
                                .HasColumnType("integer");

                            b1.Property<long>("Id")
                                .HasColumnType("bigint");

                            b1.Property<DateTime>("JoinDate")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<double>("Level")
                                .HasColumnType("double precision");

                            b1.Property<string>("Name")
                                .HasColumnType("text");

                            b1.Property<double>("Performance")
                                .HasColumnType("double precision");

                            b1.Property<int>("PlayCount")
                                .HasColumnType("integer");

                            b1.Property<int>("Rank")
                                .HasColumnType("integer");

                            b1.Property<long>("RankedScore")
                                .HasColumnType("bigint");

                            b1.Property<long>("TotalScore")
                                .HasColumnType("bigint");

                            b1.Property<int>("TotalSecondsPlayed")
                                .HasColumnType("integer");

                            b1.HasKey("UserSnapshotId");

                            b1.ToTable("UserSnapshots");

                            b1.WithOwner()
                                .HasForeignKey("UserSnapshotId");
                        });

                    b.Navigation("UserInfo")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
