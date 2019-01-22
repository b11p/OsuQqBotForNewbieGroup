using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Core;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Attributions
{
    public interface IMessageCommand
    {
        bool ShouldResponse(MessageContext context);
        Task ProcessAsync(MessageContext context, Sisters.WudiLib.HttpApiClient api);
    }
}
