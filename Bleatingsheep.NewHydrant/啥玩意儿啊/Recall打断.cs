using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    [Function("recall_old_hydrant")]
    internal class Recall打断 : IMessageMonitor
    {
        public async Task OnMessageAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api)
        {
            if (message.UserId == 122866607 && message is GroupMessage g && g.GroupId == 641236878)
            {
                await api.RecallMessageAsync(message.MessageId);
            }
        }
    }
}
