using Autofac;

namespace Bleatingsheep.NewHydrant.Core
{
#nullable enable
    public interface IHydrantStartup
    {
        void Configure(ContainerBuilder builder);
    }
#nullable restore
}
