using System;
using System.Security.Cryptography;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.NewHydrant.Osu;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using WebApiClient;

namespace Bleatingsheep.NewHydrant
{
    internal class HydrantStartup : IHydrantStartup
    {
        public HydrantStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<NewbieContext>(ServiceLifetime.Transient);
            services.AddDbContextFactory<NewbieContext>();
            services.AddLogging(b =>
            {
                b.ClearProviders();
                b.SetMinimumLevel(LogLevel.Trace);
                b.AddNLog(); // uses NLog.config?
            });

            var factory = OsuApiClientFactory.CreateFactory(Configuration.GetSection("Hydrant")["ApiKey"]);
            services.AddSingleton<IHttpApiFactory<IOsuApiClient>>(factory);
            services.AddScoped<IOsuApiClient>(c => c.GetService<IHttpApiFactory<IOsuApiClient>>().CreateHttpApi());

            services.AddTransient<QueryHelper>();

            services.AddTransient(typeof(Lazy<>), typeof(LazyService<>));

            services.AddSingleton(RandomNumberGenerator.Create());

            services.AddMemoryCache();

            services.AddTransient<IDataProvider, DataProvider>();
            services.AddTransient<DataMaintainer>();
        }

        private sealed class LazyService<T> : Lazy<T> where T : class
        {
            public LazyService(IServiceProvider provider)
                : base(() => provider.GetRequiredService<T>())
            {
            }
        }
    }
}
