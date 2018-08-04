using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Core;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Attributions
{
    interface IMessageMonitor
    {
        Task OnMessageAsync(Message message, Sisters.WudiLib.HttpApiClient api, ExecutingInfo executingInfo);
    }
}
