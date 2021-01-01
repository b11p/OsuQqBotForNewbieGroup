using System;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Osu;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.Extensions.DependencyInjection;
using WebApiClient;

namespace Bleatingsheep.NewHydrant
{
    internal class HydrantStartup : IHydrantStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<NewbieContext>();

            var hc = new HardcodedConfigure();
            var factory = OsuApiClientFactory.CreateFactory(hc.ApiKey);
            services.AddSingleton<IHttpApiFactory<IOsuApiClient>>(factory);
            services.AddScoped<IOsuApiClient>(c => c.GetService<IHttpApiFactory<IOsuApiClient>>().CreateHttpApi());

            services.AddTransient<QueryHelper>();

            services.AddTransient(typeof(Lazy<>), typeof(LazyService<>));
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
