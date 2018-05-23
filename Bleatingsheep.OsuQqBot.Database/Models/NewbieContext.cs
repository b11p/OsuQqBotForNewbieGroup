using Bleatingsheep.OsuMixedApi;
using Microsoft.EntityFrameworkCore;
using System;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    internal class NewbieContext : DbContext
    {
        public DbSet<Chart> Charts { get; private set; }
        public DbSet<ChartBeatmap> ChartBeatmaps { get; private set; }
        public DbSet<ChartValidGroup> ChartValidGroups { get; private set; }
        public DbSet<ChartAdministrator> ChartAdministrators { get; private set; }
        public DbSet<ChartTry> ChartCommits { get; private set; }
        public DbSet<Beatmap> CachedBeatmaps { get; private set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string path = @"Sheep Bot Data\Newbie.db";
            string fullName = System.IO.Path.Combine(desktop, path);
            optionsBuilder.UseSqlite($"Data Source={fullName}");
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
                .HasKey(c => new { c.ChartId, c.BeatmapId, c.Mode, c.Date, c.Uid });

            var beatmaps = modelBuilder.Entity<Beatmap>();
            beatmaps
                .Ignore(b => b.PlayCount)
                .Ignore(b => b.PassCount);
            beatmaps
                .HasKey(b => new { b.Bid, b.Mode });
            beatmaps.Property("DifficultyName").IsRequired();
            beatmaps.Property("FileMD5").IsRequired();
            beatmaps.Property("Artist").IsRequired();
            beatmaps.Property("Title").IsRequired();
            beatmaps.Property("Creator").IsRequired();
            beatmaps.Property("Source").IsRequired();
            beatmaps.Property("Tags").IsRequired();
        }
    }
}
