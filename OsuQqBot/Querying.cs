using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database;
using OsuQqBot.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using mixed = Bleatingsheep.OsuMixedApi;

namespace OsuQqBot
{
    sealed class Querying
    {
        private readonly string _key = null;
        private readonly mixed::OsuApiClient _api = null;
        private static Querying instance = null;

        private Querying(string key)
        {
            _key = key;
            _api = mixed::OsuApiClient.ClientUsingKey(key);
        }

        public IReadOnlyCollection<UserInfo> CheckUsername(IEnumerable<string> possibleUsernames, bool requireAllSucceeded = true)
        {
            var userInfos = new System.Collections.Concurrent.ConcurrentBag<UserInfo>();
            var api = _api;

            var result = System.Threading.Tasks.Parallel.ForEach(possibleUsernames, (name, state) =>
            {
                if (state.IsStopped) return;
                var (networkSuccess, userInfo) = api.GetUserInfoAsync(name, mixed::Mode.Standard).Result;
                if (requireAllSucceeded && !networkSuccess)
                {
                    state.Stop();
                    return;
                }
                if (userInfo == null) return;
                userInfos.Add(userInfo);
            });

            if (!result.IsCompleted) return null;
            return userInfos;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bid"></param>
        /// <param name="mode"></param>
        /// <param name="apiKey"></param>
        /// <exception cref="System.ArgumentException">API Key 不正确。</exception>
        /// <returns></returns>
        public async Task<mixed::Beatmap> GetBeatmapAsync(int bid, mixed::Mode mode, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new System.ArgumentException("API Key 不正确。", nameof(apiKey));
            }

            var map = await NewbieDatabase.GetBeatmapAsync(bid, mode);
            if (map != null) return map;
            map = (await _api.GetBeatmapsAsync(bid))?.SingleOrDefault();
            if (map == null) return null;
            if (!map.IsInfoFixed()) return map;
            return await NewbieDatabase.CacheBeatmapAsync(map) ?? map;
        }

        public static Querying Instance => instance;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException">已经设置过 Key。</exception>
        /// <returns></returns>
        public static Querying SetKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var querying = new Querying(key);
            var oldValue = Interlocked.CompareExchange(ref instance, querying, null);
            if (oldValue != null) throw new InvalidOperationException("已经设置过 Key，不能再次设置。");
            return querying;
        }
    }

    static class UserInfoExtension
    {
        public static string TextInfo(this UserInfo userInfo, bool showMode = false)
        {
            string[] byLine = new string[9];

            string displayAcc;
            try
            {
                displayAcc = userInfo.Accuracy.ToString("#.##%");
            }
            catch (FormatException)
            {
                displayAcc = userInfo.Accuracy.ToString();
            }

            byLine[0] = userInfo.Name + "的个人信息" + (userInfo.Mode == mixed::Mode.Standard && !showMode ? "" : "—" + userInfo.Mode.GetModeString());
            byLine[1] = string.Empty;
            byLine[2] = userInfo.Performance + "pp 表现";
            byLine[3] = "#" + userInfo.Rank;
            byLine[4] = userInfo.Country() + " #" + userInfo.CountryRank;
            byLine[5] = (userInfo.RankedScore).ToString("#,###") + " Ranked谱面总分";
            byLine[6] = displayAcc + " 准确率";
            byLine[7] = userInfo.PlayCount + " 游玩次数";
            byLine[8] = (userInfo.TotalHits).ToString("#,###") + " 总命中次数";

            return string.Join(Environment.NewLine, byLine);
        }
    }

    static class ModeExtends
    {
        public static string GetModeString(this mixed::Mode mode)
        {
            switch (mode)
            {
                case mixed::Mode.Standard:
                    return "osu!";
                case mixed::Mode.Taiko:
                    return "taiko";
                case mixed::Mode.Ctb:
                    return "catch";
                case mixed::Mode.Mania:
                    return "mania";
                default:
                    return null;
            }
        }
    }
}
