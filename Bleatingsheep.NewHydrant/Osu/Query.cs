using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.Osu.ApiV2;
using Bleatingsheep.OsuMixedApi.MotherShip;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Function("query")]
    internal class Query : OsuFunction, IMessageCommand
    {
        private static Regex Parser { get; } = new Regex(
            pattern: @"^\s*[~～∼]\s*[,，]?\s*(\S*)\s*$",
            options: RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex WhereParser { get; } = new Regex(
            pattern: $@"^\s*WHERE\s+({OsuHelper.UsernamePattern})\s*[,，]?\s*(\S*)\s*$",
            options: RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private string _username;
        private string _mode;

        public async Task ProcessAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api, ExecutingInfo executingInfo)
        {
            var id = await GetId(message.UserId, executingInfo.OsuApi);
            var (status, userInfo) = await ApiV2.Client.GetUserAsync(id, Mode.Osu);
            ExecutingException.Ensure(status != ApiStatus.AuthorizationFail, "我感到一股危机。");
            ExecutingException.Ensure(status != ApiStatus.NotFound, "被办了吧");
            ExecutingException.Ensure(status == ApiStatus.Success, $"失败了，{status}！");
            ExecutingException.Ensure(userInfo != null, "被办了。");
            var stat = userInfo.Statistics;

            UserHistory history = null;
            try
            {
                history = (await executingInfo.MotherShipApi.GetYesterdayInfo(id))?.Data;
            }
            catch
            {
            }

            var response = history != null
                ? $@"{userInfo.Username} 的个人信息。

{stat.Performance:0.00}pp 表现
#{stat.Rank?.Global}
{stat.Accuracy:0.00%} 准确率
{stat.PlayCount} 游玩次数
{PlayTimeQuery.GetDisplayTime(stat.PlayTime)}游戏时间
{stat.TotalHits:#,###} 总命中次数"
                : $@"{userInfo.Username} 的个人信息。
我没钱氪酷Q Pro了，原来的代码跑不了，只能查这些（暗示）
{stat.Performance:0.00}pp 表现
#{stat.Rank?.Global}
{stat.Accuracy:0.00%} 准确率
{stat.PlayCount} 游玩次数
{PlayTimeQuery.GetDisplayTime(stat.PlayTime)}游戏时间
{stat.TotalHits:#,###} 总命中次数";

            await api.SendMessageAsync(message.Endpoint, response);
        }

        private async Task<int> GetId(long qq, OsuMixedApi.OsuApiClient osuApi)
        {
            if (string.IsNullOrEmpty(_username))
                return await EnsureGetBindingIdAsync(qq);

            var userInfo = await EnsureGetUserInfo(_username, OsuMixedApi.Mode.Standard);
            return userInfo.Id;
        }

        public bool ShouldResponse(Sisters.WudiLib.Posts.Message message)
        {
            if (!message.Content.IsPlaintext)
                return false;

            var match = Parser.Match(message.Content.Text);
            if (match.Success)
            {
                _mode = match.Groups[1].Value;
                return true;
            }

            match = WhereParser.Match(message.Content.Text);
            if (match.Success)
            {
                _mode = match.Groups[2].Value;
                _username = match.Groups[1].Value;
                return true;
            }

            return false;
        }
    }
}
