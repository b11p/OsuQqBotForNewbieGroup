using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database;
using Bleatingsheep.OsuQqBot.Database.Models;
using OsuQqBot.QqBot;

namespace OsuQqBot.StatelessFunctions
{
    class ChartViewingAndCommiting : IStatelessFunction
    {
        public bool ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message)
        {
            if (!(endPoint is GroupEndPoint g)) return false;
            if (message.ToLowerInvariant() == " charts")
            {
                var charts = NewbieDatabase.ChartInGroup(g.GroupId);
                var infos = ChartInfo(charts);
                var result = string.Join(Environment.NewLine + Environment.NewLine, infos);
                if (string.IsNullOrEmpty(result))
                    result = "没有";
                var api = OsuQqBot.QqApi;
                api.SendMessageAsync(g, result, true);
                return true;
            }
            if (message.ToLowerInvariant() == " upx")
            {
                var api = OsuQqBot.QqApi;
                Task.Run(async () =>
                {
                    try
                    {
                        await CommitXAsync(g, messageSource);
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e);
                    }
                });
                return true;
            }
            if (message.ToLowerInvariant() == " my")
            {
                var api = OsuQqBot.QqApi;
                ListCommits(g, messageSource);
                return true;
            }
            if (message.ToLowerInvariant().StartsWith(" rank "))
            {
                return ChartRank(message.Substring(" rank ".Length), g.GroupId);
            }
            if (message.ToLowerInvariant().StartsWith(" csv "))
            {
                return ChartCsv(message.Substring(" csv ".Length), g.GroupId);
            }
            return false;
        }

        private static void ListCommits(GroupEndPoint endPoint, MessageSource messageSource)
        {
            var api = OsuQqBot.QqApi;

            var uid = LocalData.Database.Instance.GetUidFromQq(messageSource.FromQq);
            if (uid == null)
            {
                api.SendMessageAsync(endPoint, "未绑定");
                return;
            }
            var commits = NewbieDatabase.Commits(endPoint.GroupId, (int)uid);

            var infos = commits.Select(cc =>
                string.Join(
                    Environment.NewLine,
                    cc.commits
                        .Select(c =>
                            string.Join(
                                Environment.NewLine,
                                LinkOf(c),
                                $"{c.Rank}, {c.Accuracy:.##%}, {c.Combo}x, {c.Score} + {c.Mods}"
                            )
                        )
                        .Prepend(cc.chart.ChartId + "：" + cc.chart.ChartName)
                )
            );
            string info = string.Join(Environment.NewLine + Environment.NewLine, infos);
            if (string.IsNullOrWhiteSpace(info)) info = "没找到结果";
            api.SendMessageAsync(endPoint, info, true);
        }

        private static async Task CommitXAsync(GroupEndPoint endPoint, MessageSource messageSource)
        {
            var osuApi = Bleatingsheep.OsuMixedApi.OsuApiClient.ClientUsingKey(OsuQqBot.osuApiKey);
            var uid = LocalData.Database.Instance.GetUidFromQq(messageSource.FromQq);
            var qq = OsuQqBot.QqApi;
            if (!uid.HasValue)
            {
                qq.SendMessageAsync(endPoint, "没绑定");
                return;
            }
            var userTask = new Api.OsuApiClient(OsuQqBot.osuApiKey)
                .GetUserAsync(uid.Value.ToString(), Api.OsuApiClient.UsernameType.User_id, Api.Mode.Std);
            var recent = await osuApi.GetRecentlyAsync((int)uid.Value, Bleatingsheep.OsuMixedApi.Mode.Standard);
            var user = await userTask;
            if (recent == null || user == null)
            {
                qq.SendMessageAsync(endPoint, "网络错误！");
                return;
            }
            if (user.Length == 0)
            {
                qq.SendMessageAsync(endPoint, "被ban了");
                return;
            }
            if (recent.Length == 0)
            {
                qq.SendMessageAsync(endPoint, "没打图！");
                return;
            }
            var result = new CommitResult[recent.Length];
            var hintResult = new string[recent.Length];
            for (int i = 0; i < recent.Length; i++)
            {
                result[i] = NewbieDatabase.Commit(endPoint.GroupId, recent[i], ((Api.User)user[0]).PP, out var commited);
                hintResult[i] = (i + 1) + "：" + ResultHint(result[i], commited);
            }
            qq.SendMessageAsync(endPoint, string.Join(Environment.NewLine, hintResult), true);
        }

        private static string ResultHint(CommitResult result, params Chart[] charts)
        {
            switch (result)
            {
                case CommitResult.Success:
                    return
                        string.Join(
                            Environment.NewLine,
                            charts
                                .Select(c => c.ChartId + "：" + c.ChartName)
                                .Prepend("在下列 Chart 中提交成功")
                        );
                case CommitResult.CommitedException:
                    return "你已经提交过本成绩了，请再打一次";
                case CommitResult.Failed:
                    return "你失败了，但是当前 Chart 不允许失败";
                case CommitResult.ModNotAllowed:
                    return "你使用的 Mod 不符合要求";
                case CommitResult.ChartNotStarted:
                    return "Chart 还没有开始，不要着急";
                case CommitResult.NoBeatmapForYou:
                    return "你没有打符合 Chart 要求的地图";
                case CommitResult.PerformanceOverLimit:
                    return "你的 PP 超限，不允许参加 Chart";
                case CommitResult.DateOverLimit:
                    return "Chart 已经结束";
                case CommitResult.NoChart:
                    return "本群没有 Chart 活动";
                case CommitResult.UnknownException:
                default:
                    return "未知错误";
            }
        }

        private static bool ChartCsv(string id, long groupId)
        {
            bool isInt = int.TryParse(id, out int chartId);
            if (isInt)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var csv = await Charts.Statistics.CsvResultAsync(chartId);
                        if (csv == null)
                        {
                            await OsuQqBot.ApiV2.SendGroupMessageAsync(groupId, "没有找到 Chart");
                            return;
                        }
                        await OsuQqBot.ApiV2.SendGroupMessageAsync(groupId, csv);
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e);
                    }
                });
            }
            return isInt;
        }

        private static bool ChartRank(string id, long groupId)
        {
            bool isInt = int.TryParse(id, out int chartId);
            if (isInt)
                Task.Run(async () =>
                {
                    try
                    {
                        //var chart = await NewbieDatabase.GetChartWithCommitsAsync(chartId);
                        var chart = await Charts.Statistics.RankAsync(chartId);
                        if (chart.title == null)
                        {
                            await OsuQqBot.ApiV2.SendGroupMessageAsync(groupId, "没有找到 Chart");
                            return;
                        }
                        var message = ChartRankInfo(chart);
                        await OsuQqBot.ApiV2.SendGroupMessageAsync(groupId, message);
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e);
                    }
                });
            return isInt;
        }

        private static string ChartRankInfo((string title, IEnumerable<(int uid, double score)> rank) chart)
        {
            var rank = chart.rank.ToArray();
            string[] results = new string[rank.Length];
            var api = OsuApiClient.ClientUsingKey(OsuQqBot.osuApiKey);
            var parallelLoopResult = Parallel.For(0, rank.Length, i =>
            {
                var (success, user) = api.GetUserInfoAsync(rank[i].uid, Mode.Standard).Result;
                string userInfo;
                if (!success)
                {
                    userInfo = "(" + rank[i].uid.ToString() + ")";
                }
                else
                {
                    userInfo = user?.Name ?? $"被 ban 用户 ({rank[i].uid.ToString()})";
                }
                results[i] = $"{userInfo} / {rank[i].score}";
            });
            string message = string.Join(Environment.NewLine, results.Prepend($"{chart.title}的排名信息"));
            return message;
        }

        private static IEnumerable<string> ChartInfo((Chart chart, ChartBeatmap[] maps)[] ps)
        {
            foreach (var (chart, maps) in ps)
            {
                var info = new List<string>
                {
                    $"{chart.ChartId}：{chart.ChartName}",
                    chart.ChartDescription,
                };
                var now = DateTimeOffset.Now;
                if (!chart.IsRunning) info.Add("不会开始");
                else if (now < chart.StartTime) info.Add("即将开始");
                else if (chart.EndTime <= now) info.Add("已结束");
                else info.Add("进行中");
                info.Add("始于：" + chart.StartTime.ToString());
                if (chart.EndTime.HasValue) info.Add("结束于：" + chart.EndTime.Value.ToString());
                if (chart.RecommendPerformance != 0) info.Add("建议PP：" + chart.RecommendPerformance);
                if (chart.MaximumPerformance.HasValue) info.Add("上限：" + chart.MaximumPerformance.Value + " PP");
                foreach (var mapInfo in BeatmapInfo(maps))
                {
                    info.Add(mapInfo);
                }
                yield return string.Join(Environment.NewLine, info);
            }
        }

        private static IEnumerable<string> BeatmapInfo(ChartBeatmap[] maps)
        {
            string[] infos = new string[maps.Length];
            Parallel.For(0, infos.Length, i =>
            {
                ChartBeatmap map = maps[i];
                var info = new LinkedList<string>();
                info.AddLast(BloodcatApi.Client.GetBeatmapAsync(map.BeatmapId).Result?.ToString() ?? "标题获取失败");
                info.AddLast(LinkOf(map));
                if (!string.IsNullOrEmpty(map.ScoreCalculation))
                    info.AddLast(map.ScoreCalculation);
                if (map.RequiredMods != Mods.None)
                    info.AddLast($"必须开启：{map.RequiredMods.ToString()}");
                if (map.ForceMods != Mods.None)
                    info.AddLast($"FM：{map.ForceMods.ToString()}");
                if (map.BannedMods != Mods.None)
                    info.AddLast($"禁止：{map.BannedMods.ToString()}");
                info.AddLast($"允许失败：{(map.AllowsFail ? "是" : "否")}");
                infos[i] = string.Join(Environment.NewLine, info);
            });
            return infos;
        }

        private static string LinkOf(ChartBeatmap map)
            => $"https://osu.ppy.sh/b/{map.BeatmapId}&m={(int)map.Mode}";
        private static string LinkOf(ChartCommit commit)
            => $"https://osu.ppy.sh/b/{commit.BeatmapId}&m={(int)commit.Mode}";
    }
}
