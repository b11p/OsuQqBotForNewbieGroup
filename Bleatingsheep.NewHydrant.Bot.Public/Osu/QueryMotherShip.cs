using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Component("query_mother_ship")]
    public class QueryMotherShip : IMessageCommand
    {
        private readonly Lazy<NewbieContext> _newbieContext;

        public QueryMotherShip(Lazy<NewbieContext> newbieContext)
        {
            _newbieContext = newbieContext;
        }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            using var db = _newbieContext.Value;
            var bindingInfo = await db.Bindings.FirstOrDefaultAsync(b => b.UserId == context.UserId).ConfigureAwait(false);
            if (bindingInfo is null)
                return;
            var url = $"https://www.mothership.top/api/v1/stat/{bindingInfo.OsuId}";
            await api.SendMessageAsync(context.Endpoint, Message.NetImage(url, true)).ConfigureAwait(false);
        }

        public bool ShouldResponse(MessageContext context)
            => context.Content.TryGetPlainText(out string text)
            && text is "妈船？" or "妈船?";
    }
}
