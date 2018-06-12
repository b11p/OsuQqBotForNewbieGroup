﻿using Bleatingsheep.OsuMixedApi;
using Microsoft.EntityFrameworkCore;
using System;
using static Bleatingsheep.OsuQqBot.Database.Models.ServerInfo;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    internal class NewbieContext : DbContext
    {
        public DbSet<Chart> Charts { get; private set; }
        public DbSet<ChartBeatmap> ChartBeatmaps { get; private set; }
        public DbSet<ChartValidGroup> ChartValidGroups { get; private set; }
        public DbSet<ChartAdministrator> ChartAdministrators { get; private set; }
        public DbSet<ChartTry> ChartTries { get; private set; }
        public DbSet<Beatmap> CachedBeatmaps { get; private set; }
        public DbSet<OperationHistory> Histories { get; private set; }
        public DbSet<BindingInfo> Bindings { get; private set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql($"server={Server};port={Port};database={ServerInfo.Database};user={User};pwd={Password};SslMode=Required;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChartAdministrator>()
                .HasKey(a => new { a.ChartId, a.Administrator });

            modelBuilder.Entity<ChartBeatmap>()
                .HasKey(m => new { m.ChartId, m.BeatmapId, m.Mode });

            modelBuilder.Entity<ChartValidGroup>()
                .HasKey(g => new { g.ChartId, g.GroupId });

            modelBuilder.Entity<ChartTry>()
                .HasKey(c => new { c.ChartId, c.BeatmapId, c.Mode, c.Date, c.UserId });

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
            //beatmaps.Property("LastUpdateOffset").HasDefaultValue(DateTimeOffset.MinValue);
        }
    }
}
