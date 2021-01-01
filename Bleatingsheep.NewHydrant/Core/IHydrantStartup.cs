using Microsoft.Extensions.DependencyInjection;

namespace Bleatingsheep.NewHydrant.Core
{
#nullable enable
    public interface IHydrantStartup
    {
        void Configure(IServiceCollection services);
    }
#nullable restore
}
