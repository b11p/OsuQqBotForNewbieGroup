using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    /// <summary>
    /// 检测到 Daloubot 复读就 At 大楼。
    /// </summary>
    [Component("no_repeat")]
    internal class NoRepeat : 啥玩意儿啊Base, IMessageMonitor
    {
        private static readonly object _thisLock = new object();
        private string _currentMessage;
        private const long GroupId = 514661057;
        private const long BotId = 3082577334;
        private const long BotMaintainerId = 1061566571;

        public async Task OnMessageAsync(MessageContext message, HttpApiClient api)
        {
            if (!(message is GroupMessage g && g.GroupId == GroupId)) return;

            if (IsBotRepeat(g))
            {
                await api.SendMessageAsync(g.Endpoint, Message.At(BotMaintainerId) + new Message(" 求求你别复读了。")).ConfigureAwait(false);
            }
        }

        private bool IsBotRepeat(GroupMessage g)
        {
            bool isBotRepeat;
            lock (_thisLock)
            {
                // 是bot复读
                isBotRepeat = BotId == g.UserId && _currentMessage == g.RawMessage;

                // 更新消息。
                _currentMessage = g.RawMessage;
            }

            return isBotRepeat;
        }
    }
}
