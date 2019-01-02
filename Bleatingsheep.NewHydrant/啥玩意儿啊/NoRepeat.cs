using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    [Function("no_repeat")]
    internal class NoRepeat : 啥玩意儿啊Base, IMessageMonitor
    {
        private readonly object _thisLock = new object();
        private string _currentMessage;
        private int _count;
        private readonly IDictionary<string, long> _messages = new Dictionary<string, long>();
        private const long GroupId = 641236878;
        private static readonly ISet<long> Bots = new HashSet<long>
        {
            2839098896,
            1394932996,
            3082577334,
        };

        public async Task OnMessageAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api)
        {
            if (!(message is GroupMessage g && g.GroupId == GroupId)) return;

            bool isBotRepeat;
            isBotRepeat = IsBotRepeat(g);
            if (isBotRepeat)
            {
                await RecallAndBan(api, g);
            }
        }

        private bool IsBotRepeat(GroupMessage g)
        {
            bool isBotRepeat;
            lock (_thisLock)
            {
                // 更新消息和连击条数。
                if (g.RawMessage != _currentMessage)
                {
                    _currentMessage = g.RawMessage;
                    _count = 0;
                }
                _count++;

                // 如果大于等于3条，记录到复读消息列表。
                if (_count >= 3)
                {
                    _messages[g.RawMessage] = DateTimeOffset.Now.ToUnixTimeSeconds();
                }

                // 是bot复读
                isBotRepeat = Bots.Contains(g.UserId) && _messages.ContainsKey(g.RawMessage);

                // 清除过旧的消息，防止内存泄露。
                _messages.Where(p => p.Value < DateTimeOffset.Now.ToUnixTimeSeconds() - 800).Select(p => p.Key).ToList()
                    .ForEach(k => _messages.Remove(k));
            }

            return isBotRepeat;
        }
    }
}
