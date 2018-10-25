using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    [Function("no_repeat_plus")]
    internal class LikeSayo : 啥玩意儿啊Base, IMessageMonitor
    {
        public async Task OnMessageAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api, ExecutingInfo executingInfo)
        {
            if (message.UserId == 1394932996 && message is GroupMessage g && g.GroupId == 641236878)
            {
                if (message.Content.IsPlaintext)
                {
                    if (message.Content.Text == "人类的本质是？" || message.Content.Text.Contains("复读", StringComparison.Ordinal))
                    {
                        await RecallAndBan(api, g);
                    }
                }
            }
        }
    }
}
