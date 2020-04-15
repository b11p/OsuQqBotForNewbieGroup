﻿using Bleatingsheep.Osu.PerformancePlus;
using Bleatingsheep.OsuMixedApi;
using Microsoft.EntityFrameworkCore;
using static Bleatingsheep.OsuQqBot.Database.Models.ServerInfo;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class NewbieContext : DbContext
    {
        public DbSet<Beatmap> CachedBeatmaps { get; private set; }
        public DbSet<OperationHistory> Histories { get; private set; }
        public DbSet<BindingInfo> Bindings { get; private set; }

        public DbSet<PlusHistory> PlusHistories { get; private set; }
        public DbSet<RelationshipInfo> Relationships { get; private set; }
        public DbSet<BeatmapPlus> BeatmapPlusCache { get; private set; }
        public DbSet<WebLog> WebLogs { get; private set; }
        public DbSet<UserHistoryInfo> UserHistories { get; private set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql($"server={Server};port={Port};database={ServerInfo.Database};user={User};pwd={Password};SslMode=VerifyCA;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<Osu.ApiClient.UserEvent>();

            var beatmaps = modelBuilder.Entity<Beatmap>();
            beatmaps
                .Ignore(b => b.PlayCount)
                .Ignore(b => b.PassCount);
            beatmaps
                .HasKey(b => new { b.Id, b.Mode });
            beatmaps.Property("DifficultyName").IsRequired();
            beatmaps.Property("FileMD5").IsRequired();
            beatmaps.Property("Artist").IsRequired();
            beatmaps.Property("Title").IsRequired();
            beatmaps.Property("Creator").IsRequired();
            beatmaps.Property("Source").IsRequired();
            beatmaps.Property("Tags").IsRequired();
            beatmaps.HasIndex(b => b.FileMD5).HasName("index_md5");

            //modelBuilder.Entity<PlusHistory>()
            //    .Property(ph => ph.Date)
            //    .HasConversion(
            //        ph => ph.ToUnixTimeSeconds(),
            //        t => DateTimeOffset.FromUnixTimeSeconds(t)
            //    );
            modelBuilder.Entity<PlusHistory>()
                .HasKey(ph => new { ph.Id, ph.Date });
            modelBuilder.Entity<PlusHistory>()
                .Property(ph => ph.Name)
                .IsRequired();

            modelBuilder.Entity<UserHistoryInfo>()
                .HasKey(h => new { h.Id, h.Date, h.Mode });
            modelBuilder.Entity<UserHistoryInfo>()
                .Property(h => h.Name)
                .IsRequired();
            modelBuilder.Entity<UserHistoryInfo>()
                .Property(h => h.CountryCode)
                .IsRequired();

            modelBuilder.Entity<RelationshipInfo>()
                .HasKey(r => new { r.UserId, r.Relationship });

            //modelBuilder.Entity<BeatmapPlus>()
            //    .HasKey(bp => bp.Id);

            modelBuilder.Entity<BeatmapPlus>().Property(bp => bp.Id).ValueGeneratedNever();
        }
    }
}
