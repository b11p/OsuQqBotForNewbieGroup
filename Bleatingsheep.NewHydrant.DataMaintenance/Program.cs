using Bleatingsheep.NewHydrant.DataMaintenance;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddHostedService<SyncScheduleService>();
        services.AddHostedService<UpdateSnapshotsService>();

        services.AddDbContext<NewbieContext>(optionsBuilder =>
            optionsBuilder.UseNpgsql(
                ctx.Configuration.GetConnectionString("NewbieDatabase_Postgres"),
                options => options.EnableRetryOnFailure())
                .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug),
                    (RelationalEventId.CommandExecuted, LogLevel.Debug))),
            ServiceLifetime.Transient);
        services.AddDbContextFactory<NewbieContext>(optionsBuilder =>
            optionsBuilder.UseNpgsql(
                ctx.Configuration.GetConnectionString("NewbieDatabase_Postgres"),
                options => options.EnableRetryOnFailure())
                .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug),
                    (RelationalEventId.CommandExecuted, LogLevel.Debug))));
    })
    .Build();

await host.RunAsync().ConfigureAwait(false);
