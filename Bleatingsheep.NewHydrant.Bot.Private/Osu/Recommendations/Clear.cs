using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.OsuQqBot.Database.Models;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu.Recommendations
{
#nullable enable
    [Component("Clear")]
    public class Clear : IMessageCommand
    {
        private readonly NewbieContext _newbieContext;

        public Clear(NewbieContext newbieContext)
        {
            _newbieContext = newbieContext;
        }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            _newbieContext.RemoveRange(_newbieContext.Recommendations);
            await _newbieContext.SaveChangesAsync().ConfigureAwait(false);
            await api.SendMessageAsync(context.Endpoint, "清除完成。");
        }

        public bool ShouldResponse(MessageContext context)
            => context.UserId == 962549599 && context.Content.Text == "清除数据";
    }
#nullable restore
}
