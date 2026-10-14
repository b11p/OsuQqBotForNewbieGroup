using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Bleatingsheep.Osu.PerformancePlus;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class NewbieContext : DbContext
    {
        private static readonly ConcurrentDictionary<string, NpgsqlDataSource> s_dataSourceCache = new();

        public NewbieContext()
        {
        }

        public NewbieContext(DbContextOptions<NewbieContext> options)
            : base(options)
        {
        }

        public DbSet<OperationHistory> Histories { get; private set; }
        public DbSet<BindingInfo> Bindings { get; private set; }
        public DbSet<BotUserField> BotUserFields { get; private set; }
        public DbSet<BotGroupField> BotGroupFields { get; private set; }

        public DbSet<PlusHistory> PlusHistories { get; private set; }
        public DbSet<RelationshipInfo> Relationships { get; private set; }
        public DbSet<BeatmapPlus> BeatmapPlusCache { get; private set; }
        public DbSet<WebLog> WebLogs { get; private set; }
        public DbSet<DuplicateAuthentication> DuplicateAuthentication { get; private set; }

        public DbSet<BeatmapInfoCacheEntry> BeatmapInfoCache { get; private set; }

        #region User snapshots and play records.
        public DbSet<UserPlayRecord> UserPlayRecords { get; private set; }
        public DbSet<UserSnapshot> UserSnapshots { get; private set; }
        public DbSet<PlayRecordQueryTemp> PlayRecordQueryTemps { get; private set; }
        public DbSet<UpdateSchedule> UpdateSchedules { get; private set; }
        #endregion

        #region ML related
        public DbSet<MessageEntry> Messages { get; private set; }
        #endregion

        #region Recommendation related
        public DbSet<RecommendationEntry> Recommendations { get; private set; }
        #endregion

        public static NpgsqlDataSource GetDataSource(string connectionString)
        {
            // The cache is to try to fix the exception:
            // System.InvalidOperationException: An error was generated for warning 'Microsoft.EntityFrameworkCore.Infrastructure.ManyServiceProvidersCreatedWarning': More than twenty 'IServiceProvider' instances have been created for internal use by Entity Framework. This is commonly caused by injection of a new singleton service instance into every DbContext instance. For example, calling 'UseLoggerFactory' passing in a new instance each time--see https://go.microsoft.com/fwlink/?linkid=869049 for more details. This may lead to performance issues, consider reviewing calls on 'DbContextOptionsBuilder' that may require new service providers to be built. This exception can be suppressed or logged by passing event ID 'CoreEventId.ManyServiceProvidersCreatedWarning' to the 'ConfigureWarnings' method in 'DbContext.OnConfiguring' or 'AddDbContext'.
            // See: https://stackoverflow.com/questions/60047465/more-than-twenty-iserviceprovider-instances-have-been-created-for-internal-use
            // Not sure if this fixes it though. :(
            if (s_dataSourceCache.TryGetValue(connectionString, out var dataSource))
            {
                return dataSource;
            }
            lock (s_dataSourceCache)
            {
                if (s_dataSourceCache.TryGetValue(connectionString, out dataSource))
                {
                    return dataSource;
                }
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
                dataSourceBuilder.EnableDynamicJson();
                dataSource = dataSourceBuilder.Build();
                bool added = s_dataSourceCache.TryAdd(connectionString, dataSource);
                Debug.Assert(added);
                return dataSource;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = Environment.GetEnvironmentVariable("Xfs_ConnectionString_Postgres");
                var dataSource = GetDataSource(connectionString);
                optionsBuilder.UseNpgsql(dataSource,
                    options => options.EnableRetryOnFailure());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<Osu.ApiClient.UserEvent>();

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

            modelBuilder.Entity<UserPlayRecord>().OwnsOne(r => r.Record).HasIndex(r => r.Date);
            //modelBuilder.Entity<UserPlayRecord>().HasIndex(nameof(UserPlayRecord.UserId), nameof(UserPlayRecord.Mode), nameof(UserPlayRecord.Record) + "_" + nameof(UserRecent.Date));

            modelBuilder.Entity<UserSnapshot>().OwnsOne(r => r.UserInfo);
            modelBuilder.Entity<UserSnapshot>().HasIndex(r => new { r.UserId, r.Mode, r.Date });

            modelBuilder.Entity<PlayRecordQueryTemp>()
                .HasIndex(t => new { t.UserId, t.Mode })
                .IsUnique();

            modelBuilder.Entity<UpdateSchedule>()
                .HasKey(s => new { s.UserId, s.Mode });
            modelBuilder.Entity<UpdateSchedule>()
                .HasIndex(s => new { s.NextUpdate });

            modelBuilder.Entity<RecommendationEntry>()
                .Property(r => r.Left)
                .HasConversion(RecommendationBeatmapId.ValueConverter);
            modelBuilder.Entity<RecommendationEntry>()
                .Property(r => r.Recommendation)
                .HasConversion(RecommendationBeatmapId.ValueConverter);

            modelBuilder.Entity<RecommendationEntry>()
                .HasAlternateKey(r => new { r.Mode, r.Left, r.Recommendation });

            modelBuilder.Entity<RecommendationEntry>()
                .HasIndex(r => new { r.Mode, r.Left });

            modelBuilder.Entity<BotUserField>()
                .HasKey(f => new { f.UserId, f.FieldName });
            modelBuilder.Entity<BotUserField>()
                .HasIndex(f => f.FieldName);

            modelBuilder.Entity<BotGroupField>()
                .HasKey(f => new { f.GroupId, f.FieldName });
            modelBuilder.Entity<BotGroupField>()
                .HasIndex(f => f.FieldName);

            modelBuilder.Entity<BeatmapInfoCacheEntry>()
                .HasKey(c => new { c.BeatmapId, c.Mode });
        }
    }
}
