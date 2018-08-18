using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Core;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Attributions
{
    interface IMessageCommand
    {
        bool ShouldResponse(Message message);
        Task ProcessAsync(Message message, Sisters.WudiLib.HttpApiClient api, ExecutingInfo executingInfo);
    }
}
