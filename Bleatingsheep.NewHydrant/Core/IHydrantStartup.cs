using Microsoft.Extensions.DependencyInjection;

namespace Bleatingsheep.NewHydrant.Core
{
    public interface IHydrantStartup
    {
        void ConfigureServices(IServiceCollection services);
    }
}
