using Bleatingsheep.OsuMixedApi;
using OsuQqBot.QqBot;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OsuQqBot
{
    public partial class OsuQqBot
    {
        public void ProcessMessage(EndPoint endPoint, MessageSource source, string message)
        {
            if (new StatelessFunctions.ManageTips().ProcessMessage(endPoint, source, message))
                return;
            if (new StatelessFunctions.MaChuanA().ProcessMessage(endPoint, source, message))
                return;
            if (new StatelessFunctions.Konachan().ProcessMessage(endPoint, source, message))
                return;
            if (new StatelessFunctions.Yandere().ProcessMessage(endPoint, source, message))
                return;
            if (new StatelessFunctions.DalouRecommend().ProcessMessage(endPoint, source, message))
                return;
            if (message.Trim().StartsWith("~") || message.Trim().StartsWith("～") || message.Trim().StartsWith("∼"))
            {
                if (source.FromQq == 1677323371)
                {
                    qq.SendMessageAsync(endPoint, "不查，浪费资源");
                    return;
                }
                int? uid;
                try
                {
                    uid = Query.Querying.Instance.GetUserBind(source.FromQq).GetAwaiter().GetResult();
                }
                catch (OsuApiFailedException)
                {
                    qq.SendMessageAsync(endPoint, "API 故障，查询失败。");
                    throw;
                }
                if (uid == null)
                {
                    qq.SendMessageAsync(endPoint, "未绑定，请使用绑定<你的账号>命令绑定");
                }
                else
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            await SendQueryMessage(endPoint, uid.Value, message.Trim().Substring(1).Trim());
                        }
                        catch (Exception e)
                        {
                            Logger.LogException(e);
                        }
                    });
                }
            }
            else if (message.Trim().ToLowerInvariant().StartsWith("where"))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        string uNameToQuery = message.Trim().Substring("where".Length).Trim();
                        uNameToQuery = qq.AfterReceive(uNameToQuery);
                        if (!Bleatingsheep.Osu.Helper.UserNameHelper.IsUserName(uNameToQuery))
                            return;
                        if (string.IsNullOrEmpty(uNameToQuery))
                            return;
                        const string pattern = @"^qq\s*=\s*(\d+)$";
                        var match = Regex.Match(uNameToQuery, pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            long qq = long.Parse(match.Groups[1].Value);
                            this.qq.SendMessageAsync(endPoint, await QueryFromQq(qq));
                            return;
                        }
                        var (success, info) = await ProcessQuery(username: uNameToQuery);
                        this.qq.SendMessageAsync(endPoint, this.qq.BeforeSend(info));
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e);
                    }
                });
            }
            else if (!string.IsNullOrWhiteSpace(message) && message.Split()[0] == "帮助")
            {
                if (message.Split().Length > 1)
                    ShowHelp(endPoint, message.Split()[1]);
                else
                    ShowHelp(endPoint);
            }
            else if (message.Trim().StartsWith("bid"))
            {
                var match = Regex.Match(message.Trim(), "^bid (\\d+)$");
                if (!match.Success)
                    return;
                string bidString = match.Groups[1].Value;
                if (!int.TryParse(bidString, out int bid))
                    return;
                var api = Bleatingsheep.OsuMixedApi.OsuApiClient.ClientUsingKey(osuApiKey);
                var beatmaps = api.GetBeatmapsAsync(bid).Result;
                if (beatmaps?.Length != 1)
                    return;
                var beatmap = beatmaps[0];
                var result = $@"Beatmap {bid}
{beatmap.Artist} - {beatmap.Title}[{beatmap.DifficultyName}]
Beatmap by {beatmap.Creator}
Stars {beatmap.Stars.ToString(".##")}";
                qq.SendMessageAsync(endPoint, result);
            }
            else
            {
                switch (endPoint)
                {
                    case PrivateEndPoint p:
                        break;
                    case GroupEndPoint g:
                        ProcessGroupMessage(g, source, message);
                        break;
                    case DiscussEndPoint d:
                        break;
                }
            }
        }

        public void ProcessGroupMessage(GroupEndPoint endPoint, MessageSource source, string message)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (await WhirIsBestAsync(endPoint.GroupId, source.FromQq, message))
                        return;
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            });
        }

        /// <summary>
        /// 获取群内At代码
        /// </summary>
        /// <param name="qq"></param>
        /// <returns></returns>
        private static string At(long qq) => $"[CQ:at,qq={qq}]";

        /// <summary>
        /// 获取群名片
        /// </summary>
        /// <param name="fromGroup"></param>
        /// <param name="fromQq"></param>
        /// <returns>如果失败返回null</returns>
        private string GetInGroupName(long fromGroup, long fromQq)
        {
            string inGroupName;
            var memberInfo = qq.GetGroupMemberInfo(fromGroup, fromQq);
            if (memberInfo == null)
            {
                return null;
            }
            inGroupName = memberInfo.InGroupName;
            if (string.IsNullOrEmpty(inGroupName))
                inGroupName = memberInfo.QqNickname;
            return inGroupName;
        }
    }
}