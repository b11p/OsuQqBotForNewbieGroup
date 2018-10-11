using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    [Function("burang")]
    class Huanghuacai : IMessageMonitor
    {
        private static readonly ConcurrentDictionary<long, DateTimeOffset> LastSpeakTime = new ConcurrentDictionary<long, DateTimeOffset>();
        private static readonly ISet<long> ValidGroups = new HashSet<long>
        {
            641236878,
        };
        public async Task OnMessageAsync(Message message, HttpApiClient api, ExecutingInfo executingInfo)
        {
            if (message is GroupMessage g && ValidGroups.Contains(g.GroupId))
            {
                // 获取此群上次发言时间。
                DateTimeOffset last = default;
                LastSpeakTime.AddOrUpdate(g.GroupId, g.Time, (group, time) =>
                {
                    last = time;
                    return g.Time;
                });

                if (last == default)
                {
                    return;
                }

                if (g.Time - last > TimeSpan.FromHours(1) && g.UserId == 2181697779)
                {
                    if (g.Content.Sections.Count == 1 && g.Content.Sections[0].Type == Section.ImageType)
                    {
                        await api.RecallMessageAsync(message.MessageId);
                    }
                }
            }
        }
    }
}
