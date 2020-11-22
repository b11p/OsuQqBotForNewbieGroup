using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Osu;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using WebApiClient;

namespace Bleatingsheep.NewHydrant
{
    internal class HydrantStartup : IHydrantStartup
    {
        public void Configure(ContainerBuilder builder)
        {
            builder.RegisterType<NewbieContext>().AsSelf();

            var hc = new HardcodedConfigure();
            var factory = OsuApiClientFactory.CreateFactory(hc.ApiKey);
            builder.RegisterInstance(factory).As(typeof(IHttpApiFactory<IOsuApiClient>)).SingleInstance();
            builder.Register(c => c.Resolve<IHttpApiFactory<IOsuApiClient>>().CreateHttpApi())
                .As<IOsuApiClient>().InstancePerLifetimeScope();

            builder.RegisterType<QueryHelper>().AsSelf();
        }
    }
}
