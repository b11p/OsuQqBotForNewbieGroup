using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Microsoft.Extensions.Caching.Memory;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
#nullable enable
    [Component("称金币")]
    public partial class 称金币 : IMessageCommand
    {

        private static readonly IMemoryCache s_cache = new MemoryCache(new MemoryCacheOptions());

        private const int Max = ExecuteInfo.Max;

        private bool _isInit;

        private int _left;
        private int _right;

        private string GetInfo(IList<string> cp)
        {
            var l = cp.Last();
            var sb = new StringBuilder("最近一次结果是：").Append(l).Append("\r\n");
            sb.AppendJoin("\r\n", cp);
            return sb.ToString();
        }

        private async Task Init(MessageContext context, HttpApiClient api)
        {
            var sb = new StringBuilder();
            if (s_cache.TryGetValue(context.UserId, out ExecuteInfo exec))
            {
                sb.AppendLine("当前游戏正在进行。");
            }
            else
            {
                exec = s_cache.Set(context.UserId, new ExecuteInfo(), new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });
            }
            sb.Append("你有").Append(Max).Append("枚重量各不相同的金币，编号为1-").Append(Max).Append(@"，和一个天平。你可以用天平比较两枚金币的重量。
你的目标是：称最少的次数（9枚金币19次，不知道是否有解），将这些金币按从轻到重排序
你可以使用指令“称xy”来比较两枚金币的重量，其中x、y都是金币编号。
例如：称12
你无须提交排序结果，消防栓会自动检测你能否排出来。
------
");
            string info = GetInfo(exec.GetKnown(out _));
            sb.Append(info);
            string message = sb.ToString();
            await api.SendMessageAsync(context.Endpoint, message).ConfigureAwait(false);
        }

        private async Task Weigh(MessageContext context, HttpApiClient api)
        {
            if (!s_cache.TryGetValue<ExecuteInfo>(context.UserId, out var exec))
            {
                await api.SendMessageAsync(context.Endpoint, "你未开始游戏！").ConfigureAwait(false);
                return;
            }

            if (_left <= 0 || _left > Max || _right <= 0 || _right > Max)
            {
                await api.SendMessageAsync(context.Endpoint, "称量金币错误。").ConfigureAwait(false);
                return;
            }

            if (_left == _right)
            {
                await api.SendMessageAsync(context.Endpoint, "你怎么把一枚金币放到天平两边？").ConfigureAwait(false);
                return;
            }

            var sb = new StringBuilder();
            var cp = exec.Weigh(_left, _right, out bool isClear, out bool isKnown, out IList<int>? result);
            if (isKnown)
            {
                sb.AppendLine("你已经知道哪边更重了，不是吗？");
            }

            sb.Append(GetInfo(cp));
            if (isClear)
            {
                s_cache.Remove(context.UserId);
                sb.Append("\r\n恭喜排序完成，共称了").Append(exec.WeighingCount).Append("次。");
                if (exec.WeighingCount <= 19)
                {
                    await api.SendMessageAsync(context.Endpoint, "恭喜达到最优次数！").ConfigureAwait(false);
                    await api.SendPrivateMessageAsync(962549599, $"{context.UserId} 达到最优19次！").ConfigureAwait(false);
                    await api.SendPrivateMessageAsync(962549599, GetInfo(cp)).ConfigureAwait(false);

                }
                if (result != null)
                {
                    sb.AppendLine().AppendLine("从轻到重分别为：").AppendJoin(", ", result);
                }
            }
            string message = sb.ToString();
            await api.SendMessageAsync(context.Endpoint, message).ConfigureAwait(false);
        }

        public Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            return _isInit ? Init(context, api) : Weigh(context, api);
        }

        public bool ShouldResponse(MessageContext context)
        {
            if (context.Content.TryGetPlainText(out string text))
            {
                if (string.Equals(text, "称金币", StringComparison.Ordinal))
                {
                    _isInit = true;
                    return true;
                }
                else if (Regex.IsMatch(text.Trim(), @"^称\d\d$", RegexOptions.Compiled))
                {
                    _left = int.Parse(text.Trim().Substring(1, 1));
                    _right = int.Parse(text.Trim().Substring(2, 1));
                    return true;
                }
            }
            return false;
        }
    }
#nullable restore
}
