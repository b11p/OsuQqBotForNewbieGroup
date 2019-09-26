using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.啥玩意儿啊;
using PuppeteerSharp;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
    [Function("newbie_request_notify")]
    internal class NotifyOnJoinRequest : OsuFunction, IMessageCommand
    {
        //private const string Pattern = @"^收到新人群加群申请\r\n群号: (\d+)\r\n群类型: .*?\r\n申请者: (\d+)\r\n验证信息: (.*)$"; // 匹配上报申请的消息。
        private const int NewbieManagementGroupId = 695600319;
        private static readonly IReadOnlyDictionary<long, double?> ManagedGroups = new Dictionary<long, double?>
        {
            [885984366] = 2500,
            [758120648] = null,
            [514661057] = null,
        };

        private async Task ParseInfoAsync(
            HttpApiClient api,
            Endpoint sendBackEndpoint,
            GroupRequest r,
            int? osuId = null,
            OsuMixedApi.UserInfo userInfo = default)
        {
            long userId = r.UserId;
            string comment = r.Comment;
            var hints = new List<Message>();
            var levelInfo = await api.GetLevelInfo(userId);
            if (levelInfo != null)
            {
                hints.Add(new Message($"QQ 等级为 {levelInfo.Level}"));
            }
            if (!string.IsNullOrEmpty(comment))
            {
                var userNames = OsuHelper.DiscoverUsernames(comment).Where(n => !string.Equals(n, "osu", StringComparison.OrdinalIgnoreCase));
                if (userInfo != null && userNames.Any(n => string.Equals(userInfo.Name, n, StringComparison.OrdinalIgnoreCase)))
                {// 绑定一致

                }
                else if (osuId == null)
                {// 忽略已绑定的情况，因为可能绑定不一致或者查询失败。
                    foreach (var name in userNames)
                    {
                        var (success, info) = await OsuApi.GetUserInfoAsync(name, Bleatingsheep.Osu.Mode.Standard);
                        // 我想用 8.0 新语法
                        hints.Add(new Message($"{info?.Name ?? name}: " +
                            $"{(success ? info == null ? "不存在此用户。" : $"PP: {info.Performance}, PC: {info.PlayCount}, TTH: {info.TotalHits}" : "查询失败。")}"));

                        if (info == null)
                        {
                            continue;
                        }

                        // 提供绑定并放行的捷径。
                        if (info?.Performance < 2500)
                        {
                            var ms = new MemoryStream();
                            using (var bw = new BinaryWriter(ms, Encoding.UTF8, true))
                            {
                                bw.Write(userId);
                                bw.Write(info.Id);
                                bw.Write(r.Flag);
                            }
#pragma warning disable CA5351 // 不要使用损坏的加密算法
                            using var md5 = MD5.Create();
#pragma warning restore CA5351 // 不要使用损坏的加密算法
                            ms.Write(md5.ComputeHash(ms.ToArray()));
                            var bytes = ms.ToArray();
                            var base64 = Convert.ToBase64String(bytes);
                            await api.SendMessageAsync(sendBackEndpoint, $"（占位）绑定为 {info.Name} 并放行：#{base64}#");
                        }
                        var uid = info.Id;
                        await ScreenShotsAsync(hints, uid).ConfigureAwait(false);
                    }
                }
            }
            if (hints.Any())
            {
                var newLine = new Message("\r\n");
                await api.SendMessageAsync(sendBackEndpoint, hints.Aggregate((m1, m2) => m1 + newLine + m2));
            }
        }

        private static async Task ScreenShotsAsync(List<Message> hints, int uid)
        {
            async Task<byte[]> GetScreenshot(Page page, string selector)
            {
                ElementHandle detailElement = await page.WaitForSelectorAsync(selector);
                var data = await detailElement
                    .ScreenshotDataAsync(new ScreenshotOptions
                    {
                    });
                return data;
            }

            Message messageRankHistory, messageBest;
            using (var page = await Chrome.OpenNewPageAsync())
            {
                await page.SetViewportAsync(new ViewPortOptions
                {
                    DeviceScaleFactor = 2,
                    Width = 1440,
                    Height = 900,
                });
                await page.GoToAsync($"https://osu.ppy.sh/users/{uid}/osu");
                // draw history
                const string chartSelector = "body > div.osu-layout__section.osu-layout__section--full.js-content.community_profile > div > div > div > div.js-switchable-mode-page--scrollspy.js-switchable-mode-page--page > div.osu-page.osu-page--users > div > div.profile-detail > div:nth-child(2)";
                var data = await GetScreenshot(page, chartSelector);
                messageRankHistory = Message.ByteArrayImage(data);
                hints.Add(messageRankHistory);

                // draw ranks
                const string bestSelector = "body > div.osu-layout__section.osu-layout__section--full.js-content.community_profile > div > div > div > div.osu-layout__section.osu-layout__section--users-extra > div > div > div > div:nth-child(2) > div > div.play-detail-list";
                const string bestFallbackSelector = "body > div.osu-layout__section.osu-layout__section--full.js-content.community_profile > div > div > div > div.osu-layout__section.osu-layout__section--users-extra > div > div > div > div:nth-child(2) > p";
                ElementHandle bpElement = (await page.QuerySelectorAsync(bestSelector))
                    ?? await page.QuerySelectorAsync(bestFallbackSelector);
                if (bpElement != null)
                {
                    data = await bpElement?.ScreenshotDataAsync();
                    //data = await GetScreenshot(page, bestSelector).ConfigureAwait(false);
                    messageBest = Message.ByteArrayImage(data);
                    hints.Add(messageBest);
                }
                else
                {
                    hints.Add(new Message("查询 BP 失败。既没有找到 BP 数据，也没有找到未上传成绩的说明。"));
                }
            }
        }

        private async ValueTask<double?> HintBinding(HttpApiClient api, Endpoint endpoint, GroupRequest r)
        {
            long userId = r.UserId;
            string comment = r.Comment;
            var (success, osuId) = await DataProvider.GetBindingIdAsync(userId);
            string response = string.Empty;
            double? performance = default;
            OsuMixedApi.UserInfo user = null;
            if (!success)
            {
                response = "查询失败";
            }

            else if (osuId == null)
            {
                response = "这个人没绑定。";
            }
            else
            {
                bool osuApiGood;
                (osuApiGood, user) = await OsuApi.GetUserInfoAsync(osuId.Value, OsuMixedApi.Mode.Standard);
                performance = user?.Performance;

                response = $"这个人绑定的 uid 是 {osuId}，用户名是 {(osuApiGood ? user?.Name ?? "被办了" : "查询失败")}\r\n（正在施工）";
            }

            // 提供额外信息
            try
            {
                await ParseInfoAsync(api, endpoint, r, osuId, user);
            }
#pragma warning disable CA1031 // 不捕获常规异常类型
            catch (Exception e)
#pragma warning restore CA1031 // 不捕获常规异常类型
            {
                await api.SendMessageAsync(endpoint, e.ToString());
            }

            // 保留 comment
            if (!string.IsNullOrEmpty(comment))
            {
                response = comment + "\r\n" + response;
            }

            await api.SendMessageAsync(endpoint, response);
            return performance;
        }

        public GroupRequestResponse Monitor(HttpApiClient httpApiClient, GroupRequest e)
        {
            if (ManagedGroups.TryGetValue(e.GroupId, out var limit))
            {
                var endpoint = new GroupEndpoint(NewbieManagementGroupId);
                var performance = HintBinding(httpApiClient, endpoint, e).ConfigureAwait(false).GetAwaiter().GetResult();
                if (performance >= limit)
                {
                    var reason = $"您的 PP 超限，不能加入本群。";
                    _ = httpApiClient.SendMessageAsync(endpoint, $"以“{reason}”拒绝。");
                    return new GroupRequestResponse(reason);
                    //_ = httpApiClient.SendMessageAsync(endpoint, $"建议拒绝。");
                    //return null;
                }
            }
            return null;
        }

        private string _content;

        public bool ShouldResponse(MessageContext context)
            => context switch
            {
                GroupMessage g when g.GroupId == 695600319 && g.Content.TryGetPlainText(out _content) && _content.StartsWith("drawu", StringComparison.OrdinalIgnoreCase) => true,
                _ => false
            };

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            var uid = int.Parse(_content.Substring(5).Trim(), CultureInfo.InvariantCulture);
            var hints = new List<Message>();
            await ScreenShotsAsync(hints, uid).ConfigureAwait(false);
            _ = await api.SendMessageAsync(context.Endpoint, hints[0] + hints[1]).ConfigureAwait(false);
        }
    }
}
