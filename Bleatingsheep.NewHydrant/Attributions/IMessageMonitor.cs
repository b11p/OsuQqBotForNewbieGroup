using System.Threading.Tasks;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Attributions
{
    public interface IMessageMonitor
    {
        Task OnMessageAsync(Message message, Sisters.WudiLib.HttpApiClient api);
    }
}
