using Microsoft.EntityFrameworkCore;
using System;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    internal class NewbieContext : DbContext
    {
        public DbSet<Chart> Charts { get; private set; }
        public DbSet<ChartBeatmap> ChartMaps { get; private set; }
        public DbSet<ChartValidGroup> ChartValidGroups { get; private set; }
        public DbSet<ChartAdministrator> ChartAdministrators { get; private set; }
        public DbSet<ChartCommit> Commits { get; private set; }

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
        }
    }
}
