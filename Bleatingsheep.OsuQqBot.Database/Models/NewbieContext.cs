using Bleatingsheep.Osu.PerformancePlus;
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

        #region User snapshots and play records.
        public DbSet<UserPlayRecord> UserPlayRecords { get; private set; }
        public DbSet<UserSnapshot> UserSnapshots { get; private set; }
        public DbSet<PlayRecordQueryTemp> PlayRecordQueryTemps { get; private set; }
        public DbSet<UpdateSchedule> UpdateSchedules { get; private set; }
        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql($"server={Server};port={Port};database={ServerInfo.Database};user={User};pwd={Password};SslMode=VerifyCA;",
                options => options.EnableRetryOnFailure());
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

            modelBuilder.Entity<RelationshipInfo>()
                .HasKey(r => new { r.UserId, r.Relationship });

            //modelBuilder.Entity<BeatmapPlus>()
            //    .HasKey(bp => bp.Id);

            modelBuilder.Entity<BeatmapPlus>().Property(bp => bp.Id).ValueGeneratedNever();

            modelBuilder.Entity<UserPlayRecord>().HasKey(r => new { r.UserId, r.Mode, r.PlayNumber });
            modelBuilder.Entity<UserPlayRecord>().OwnsOne(r => r.Record);

            modelBuilder.Entity<UserSnapshot>().OwnsOne(r => r.UserInfo);
            modelBuilder.Entity<UserSnapshot>().HasIndex(r => new { r.UserId, r.Mode, r.Date });

            modelBuilder.Entity<PlayRecordQueryTemp>()
                .HasIndex(t => new { t.UserId, t.Mode })
                .IsUnique();

            modelBuilder.Entity<UpdateSchedule>()
                .HasKey(s => new { s.UserId, s.Mode });
        }
    }
}
