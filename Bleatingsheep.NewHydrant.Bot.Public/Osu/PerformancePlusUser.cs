﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.NewHydrant.Data.Results;
using Bleatingsheep.Osu.PerformancePlus;
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Component("pp+")]
    internal class PerformancePlusUser : Service, IMessageCommand
    {
        private static readonly PerformancePlusSpider s_spider = new PerformancePlusSpider();
        private readonly IDbContextFactory<NewbieContext> _dbContextFactory;
        private readonly IOsuDataUpdator _osuDataUpdator;
        private readonly ILogger<PerformancePlusUser> _logger;

        public PerformancePlusUser(
            ILegacyDataProvider dataProvider,
            OsuApiClient osuApi,
            IDbContextFactory<NewbieContext> dbContextFactory,
            IOsuDataUpdator osuDataUpdator,
            ILogger<PerformancePlusUser> logger)
        {
            DataProvider = dataProvider;
            OsuApi = osuApi;
            _dbContextFactory = dbContextFactory;
            _osuDataUpdator = osuDataUpdator;
            _logger = logger;
        }

        private string queryUser;

        private ILegacyDataProvider DataProvider { get; }
        private OsuApiClient OsuApi { get; }

        public async Task ProcessAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api)
        {
            dynamic query;
            if (!string.IsNullOrWhiteSpace(queryUser))
            {
                bool success;
                (success, query) = await GetUserKey(queryUser);
                if (!success)
                {
                    await api.SendMessageAsync(message.Endpoint, "查询失败。");
                    return;
                }
                if (query is null)
                {
                    await api.SendMessageAsync(message.Endpoint, "查无此人。");
                    return;
                }
            }
            else
            {
                var (success, userId) = await DataProvider.GetBindingIdAsync(message.UserId);
                if (!success)
                {
                    await api.SendMessageAsync(message.Endpoint, "查询绑定账号失败。");
                    return;
                }
                if (userId == null)
                {
                    await api.SendMessageAsync(message.Endpoint, "未绑定。请发送“绑定”后跟你的用户名进行绑定。");
                    return;
                }
                query = userId.Value;
            }
            try
            {
                var userPlus = (UserPlus)await s_spider.GetUserPlusAsync(query);
                if (userPlus == null)
                {
                    await api.SendMessageAsync(message.Endpoint, string.IsNullOrWhiteSpace(queryUser) ? "被办了。" : "查无此人。");
                    return;
                }

                PlusHistory old = null;
                try
                {
                    await using var db = _dbContextFactory.CreateDbContext();
                    old = await db.PlusHistories
                        .AsNoTracking()
                        .Where(ph => ph.Id == userPlus.Id && ph.Date < DateTimeOffset.UtcNow.AddHours(-16))
                        .OrderByDescending(ph => ph.Date)
                        .FirstOrDefaultAsync();
                }
                catch (Exception e)
                {
                    await api.SendMessageAsync(message.Endpoint, "无法找到历史数据。");
                    _logger.LogError(e, "查找 PP+ 历史数据时数据库访问错误");
                }

                string responseMessage;
                if (old == null)
                {
                    responseMessage = $"""
                        {userPlus.Name} 的 PP+ 数据
                        Performance: {userPlus.Performance}
                        Aim (Jump): {userPlus.AimJump}
                        Aim (Flow): {userPlus.AimFlow}
                        Precision: {userPlus.Precision}
                        Speed: {userPlus.Speed}
                        Stamina: {userPlus.Stamina}
                        Accuracy: {userPlus.Accuracy}
                        """;
                }
                else
                {
                    var durationSinceOldData = DateTimeOffset.UtcNow - old.Date;
                    responseMessage = $"""
                        {userPlus.Name} 的 PP+ 数据
                        Performance: {userPlus.Performance}{userPlus.Performance - old.Performance: (+#); (-#); ;}
                        Aim (Jump): {userPlus.AimJump}{userPlus.AimJump - old.AimJump: (+#); (-#); ;}
                        Aim (Flow): {userPlus.AimFlow}{userPlus.AimFlow - old.AimFlow: (+#); (-#); ;}
                        Precision: {userPlus.Precision}{userPlus.Precision - old.Precision: (+#); (-#); ;}
                        Speed: {userPlus.Speed}{userPlus.Speed - old.Speed: (+#); (-#); ;}
                        Stamina: {userPlus.Stamina}{userPlus.Stamina - old.Stamina: (+#); (-#); ;}
                        Accuracy: {userPlus.Accuracy}{userPlus.Accuracy - old.Accuracy: (+#); (-#); ;}
                        对比于 {durationSinceOldData.Days} 天 {durationSinceOldData.Hours} 小时前
                        """;
                }

                if (message is GroupMessage g && g.GroupId == 758120648)
                {
                    //responseMessage += $"\r\n化学式没付钱指数：{C8Mod.CostOf(userPlus):0.0}";
                    responseMessage = C8Mod.ModPerformancePlus(responseMessage, old, userPlus);
                }
                await api.SendMessageAsync(message.Endpoint, responseMessage);

                var addResult = await _osuDataUpdator.AddPlusHistoryAsync(userPlus);
                if (addResult.TryGetError(out var error))
                    _logger.LogError(error.Exception, "储存最新 PP+ 结果时出现错误。");
            }
            catch (ExceptionPlus)
            {
                await api.SendMessageAsync(message.Endpoint, "查询 PP+ 失败。");
                return;
            }
        }

        private async Task<(bool success, dynamic key)> GetUserKey(string userName)
        {
            if (userName.All(c => char.IsDigit(c)))
            {
                var (success, user) = await OsuApi.GetUserInfoAsync(userName, OsuMixedApi.Mode.Standard);
                return (success, user?.Id);
            }
            else
            {
                return (true, userName);
            }
        }

        public bool ShouldResponse(Sisters.WudiLib.Posts.Message message)
        {
            if (!message.Content.IsPlaintext)
                return false;
            if (message is GroupMessage g && (g.GroupId == 595985887 || g.GroupId == 231094840))
                return false; // ignored in newbie group.
            string text = message.Content.Text.Trim();
            if (text.StartsWith("+", StringComparison.Ordinal))
            {
                queryUser = text.Substring("+".Length).Trim();
                return string.IsNullOrEmpty(queryUser) || OsuHelper.IsUsername(queryUser);
            }
            return false;
        }
    }
}
