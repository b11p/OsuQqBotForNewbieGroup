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
using Microsoft.EntityFrameworkCore;
using MotherShipDatabase;
using PuppeteerSharp;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;
using Mode = Bleatingsheep.Osu.Mode;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
    [Component("newbie_request_notify")]
    internal partial class NotifyOnJoinRequest : OsuFunction, IMessageCommand
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
            TrustedUserInfo userInfo = default)
        {
            long userId = r.UserId;
            string comment = r.Comment;
            var hints = new List<Message>();
            if (!string.IsNullOrEmpty(comment))
            {
                var userNames = OsuHelper.DiscoverUsernames(comment).Where(n => !string.Equals(n, "osu", StringComparison.OrdinalIgnoreCase));
                if (osuId != null)
                {
                    bool success = userInfo != null; // 由于当前未从调用方法处获得上级 API 是否调用成功，在信息不为 null 时默认成功。
                    if (userInfo == null)
                    {// 可能的重试。
                        (success, userInfo) = await OsuApi.GetUserInfoAsync(osuId.Value, Mode.Standard).ConfigureAwait(false);
                    }
                    _ = await ProcessApplicantReportAsync(hints, null, (success, userInfo)).ConfigureAwait(false);
                    if (userInfo != null && !userNames.Any(n => string.Equals(userInfo.Name, n, StringComparison.OrdinalIgnoreCase)))
                    {// 绑定不一致
                        hints.Add(new Message("警告：其绑定的账号与申请不符。"));
                    }
                }
                else
                {// 忽略已绑定的情况，因为可能绑定不一致或者查询失败。
                    foreach (var name in userNames)
                    {
                        var userTuple = await OsuApi.GetUserInfoAsync(name, Bleatingsheep.Osu.Mode.Standard);
                        //// 我想用 8.0 新语法
                        //hints.Add(new Message($"{info?.Name ?? name}: " +
                        //    $"{(success ? info == null ? "不存在此用户。" : $"PP: {info.Performance}, PC: {info.PlayCount}, TTH: {info.TotalHits}" : "查询失败。")}"));

                        var info = await ProcessApplicantReportAsync(hints, name, userTuple).ConfigureAwait(false);

                        if (info == null)
                        {// 属于没有查到的情况（因为网络问题或者用户不存在），并且之前已经给出错误信息。
                            continue;
                        }

                        // 自动绑定，在请求消息完全匹配 osu! 用户名的前提下。
                        if (userNames.Count() == 1
                            && comment.TrimEnd().EndsWith($"答案：{name}", StringComparison.Ordinal)
                            && info != null)
                        {
                            var bindingResult = await Database.AddNewBindAsync(
                                qq: r.UserId,
                                osuId: info.Id,
                                osuName: info.Name,
                                source: "Auto (Request)",
                                operatorId: r.UserId,
                                operatorName: info.Name).ConfigureAwait(false);
                            if (bindingResult.Success)
                            {
                                hints.Add(new Message($"自动绑定为 {info.Name}"));
                                goto binding_end;
                            }
                            else
                            {
                                hints.Add(new Message($"自动绑定失败。"));
                            }
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
                    binding_end:;
                    }
                }
            }
            if (hints.Count > 0)
            {
                var newLine = new Message("\r\n");
                await api.SendMessageAsync(sendBackEndpoint, hints.Aggregate((m1, m2) => m1 + newLine + m2));
            }
        }

        private async ValueTask<(double? performance, int? level)> HintBinding(HttpApiClient api, Endpoint endpoint, GroupRequest r)
        {
            long userId = r.UserId;
            string comment = r.Comment;
            var (success, osuId) = await DataProvider.GetBindingIdAsync(userId);
            var sb = new StringBuilder();
            sb.Append(comment).Append("\r\n");
            double? performance = default;
            TrustedUserInfo user = null;

            // API not implemented in go-cqhttp
            //var levelInfo = await api.GetLevelInfo(userId).ConfigureAwait(false);
            //var level = levelInfo?.Level;
            //if (levelInfo != null)
            //{
            //    sb.Append("QQ 等级为 ").Append(levelInfo.Level).Append("\r\n");
            //}

            if (!success)
            {
                sb.Append("查询失败");
                goto exit;
            }
            else if (osuId == null)
            {
                sb.Append("这个人没绑定。");
            }
            else
            {
                sb.Append("这个人绑定的 uid 是 ").Append(osuId).Append('，');
                bool osuApiGood;
                (osuApiGood, user) = await OsuApi.GetUserInfoAsync(osuId.Value, OsuMixedApi.Mode.Standard);
                _ = ((osuApiGood, user) switch
                {
                    (false, _) => sb.Append("查询失败。"),
                    (_, null) => sb.Append("被办了。"),
                    (_, TrustedUserInfo _) when user.Name != null => sb.Append("用户名是 ").Append(user.Name),
                    _ => sb.Append("未知错误"),
                });

                if (osuApiGood && user is null)
                {// user is banned, find user from mother ship
                    try
                    {
                        using var mother = new OsuContext();
                        var role = await mother.Userrole.Where(r => r.UserId == osuId).FirstOrDefaultAsync().ConfigureAwait(false);
                        var info = await mother.Userinfo.Where(i => i.UserId == osuId).OrderByDescending(i => i.QueryDate).FirstOrDefaultAsync().ConfigureAwait(false);
                        if (!(role is null || info is null))
                        {
                            user = TrustedUserInfo.FromMotherShip(info, role);
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception e)
                    {
                        Logger.Info(e);
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                }
                performance = user?.Performance;
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
                Logger.Warn(e);
                await api.SendMessageAsync(endpoint, e.Message).ConfigureAwait(false);
            }

        exit:
            await api.SendMessageAsync(endpoint, sb.ToString()).ConfigureAwait(false);
            // API not implemented in go-cqhttp
            //return (performance, level);
            return (performance, null);
        }

        public GroupRequestResponse Monitor(HttpApiClient httpApiClient, GroupRequest e)
        {
            if (ManagedGroups.TryGetValue(e.GroupId, out var limit))
            {
                Logger.Info($"{e.UserId}申请加入群{e.GroupId}。");
                var endpoint = new GroupEndpoint(NewbieManagementGroupId);
                var (performance, level) = HintBinding(httpApiClient, endpoint, e).ConfigureAwait(false).GetAwaiter().GetResult();
                if (performance >= limit)
                {
                    var reason = $"您的 PP 超限，不能加入本群。";
                    _ = httpApiClient.SendMessageAsync(endpoint, $"以“{reason}”拒绝。");
                    return new GroupRequestResponse(reason);
                    //_ = httpApiClient.SendMessageAsync(endpoint, $"建议拒绝。");
                    //return null;
                }
                if (limit != null && level == 1)
                {
                    const string reason = "";
                    _ = httpApiClient.SendMessageAsync(endpoint, $"已拒绝。");
                    return reason;
                }
            }
            return null;
        }

        private string _content;

        public bool ShouldResponse(MessageContext context)
            => context switch
            {
                GroupMessage g when g.GroupId == 695600319 && g.Content.TryGetPlainText(out _content) && _content.StartsWith("shotu", StringComparison.OrdinalIgnoreCase) => true,
                _ => false
            };

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            var uid = int.Parse(_content.Substring(5).Trim(), CultureInfo.InvariantCulture);
            var hints = new List<Message>();
            await ScreenShotsAsync(hints, uid).ConfigureAwait(false);
            var newLine = new Message("\r\n");
            Message message = hints.Count > 0
                ? hints.Aggregate((m1, m2) => m1 + newLine + m2)
                : new Message("没有结果。");
            await api.SendMessageAsync(context.Endpoint, message).ConfigureAwait(false);
        }
    }
}
