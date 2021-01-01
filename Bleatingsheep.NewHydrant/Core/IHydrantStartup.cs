using Microsoft.Extensions.DependencyInjection;

namespace Bleatingsheep.NewHydrant.Core
{
#nullable enable
    public interface IHydrantStartup
    {
        void ConfigureServices(IServiceCollection services);
    }
#nullable restore
}
