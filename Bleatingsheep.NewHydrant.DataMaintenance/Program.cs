using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.NewHydrant.DataMaintenance;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NLog.Extensions.Hosting;
using WebApiClient;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddHostedService<SyncScheduleService>();
        services.AddHostedService<UpdateSnapshotsService>();

        string? connectionString = ctx.Configuration.GetConnectionString("NewbieDatabase_Postgres");
        var dataSource = NewbieContext.GetDataSource(connectionString);
        services.AddDbContext<NewbieContext>(optionsBuilder =>
            optionsBuilder.UseNpgsql(
                dataSource,
                options => options.EnableRetryOnFailure())
                .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug),
                    (RelationalEventId.CommandExecuted, LogLevel.Debug))),
            ServiceLifetime.Transient);
        services.AddDbContextFactory<NewbieContext>(optionsBuilder =>
            optionsBuilder.UseNpgsql(
                dataSource,
                options => options.EnableRetryOnFailure())
                .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug),
                    (RelationalEventId.CommandExecuted, LogLevel.Debug))));
        services.AddTransient<DataMaintainer>();

        var factory = OsuApiClientFactory.CreateFactory(ctx.Configuration.GetSection("Hydrant")["ApiKey"]);
        services.AddSingleton<IHttpApiFactory<IOsuApiClient>>(factory);
        services.AddScoped(c => c.GetRequiredService<IHttpApiFactory<IOsuApiClient>>().CreateHttpApi());
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.SetMinimumLevel(LogLevel.Trace);
    })
    .UseNLog()
    .Build();

await host.RunAsync().ConfigureAwait(false);
