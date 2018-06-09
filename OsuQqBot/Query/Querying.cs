using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuMixedApi.MotherShip;
using Bleatingsheep.OsuQqBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OsuQqBot.Query
{
    /// <summary>
    /// 利用各种方法拿到想要的数据，并且负责一定的同步的方法。
    /// </summary>
    sealed class Querying
    {
        private readonly string _key = null;
        private readonly OsuApiClient _api = null;
        private static Querying instance = null;

        private Querying(string key)
        {
            _key = key;
            _api = OsuApiClient.ClientUsingKey(key);
        }

        public IReadOnlyCollection<UserInfo> CheckUsername(IEnumerable<string> possibleUsernames, bool requireAllSucceeded = true)
        {
            var userInfos = new System.Collections.Concurrent.ConcurrentBag<UserInfo>();
            var api = _api;

            var result = Parallel.ForEach(possibleUsernames, (name, state) =>
            {
                if (state.IsStopped) return;
                var (networkSuccess, userInfo) = api.GetUserInfoAsync(name, Mode.Standard).Result;
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
        /// <exception cref="ArgumentException">API Key 不正确。</exception>
        /// <returns></returns>
        public async Task<Beatmap> GetBeatmapAsync(int bid, Mode mode, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("API Key 不正确。", nameof(apiKey));
            }

            var map = await NewbieDatabase.GetBeatmapAsync(bid, mode);
            if (map != null) return map;
            map = (await _api.GetBeatmapsAsync(bid))?.SingleOrDefault();
            if (map == null) return null;
            if (!map.IsInfoFixed()) return map;
            return await NewbieDatabase.CacheBeatmapAsync(map) ?? map;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="qqId"></param>
        /// <exception cref="OsuApiFailedException"></exception>
        /// <returns></returns>
        public async Task<int?> GetUserBind(long qqId)
        {
            var result = OpenApi.Instance.Bindings.UserIdOf(qqId);
            if (result is int u) return u;
            var response = await OpenApi.Instance.MotherShipApiClient.GetUserInfoAsync(qqId);
            if (response.Data is MotherShipUserInfo info)
            {
                u = info.OsuId;
                OpenApi.Instance.Bindings.Bind(qqId, info.OsuId, info.Name, "Mother Ship (while running)", 0, null);
                return u;
            }
            return null;
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

            byLine[0] = userInfo.Name + "的个人信息" + (userInfo.Mode == Mode.Standard && !showMode ? "" : "—" + userInfo.Mode.GetModeString());
            byLine[1] = string.Empty;
            byLine[2] = userInfo.Performance + "pp 表现";
            byLine[3] = "#" + userInfo.Rank;
            byLine[4] = userInfo.Country + " #" + userInfo.CountryRank;
            byLine[5] = (userInfo.RankedScore).ToString("#,###") + " Ranked谱面总分";
            byLine[6] = displayAcc + " 准确率";
            byLine[7] = userInfo.PlayCount + " 游玩次数";
            byLine[8] = (userInfo.TotalHits).ToString("#,###") + " 总命中次数";

            return string.Join(Environment.NewLine, byLine);
        }
    }

    static class ModeExtends
    {
        public static string GetModeString(this Mode mode)
        {
            switch (mode)
            {
                case Mode.Standard:
                    return "osu!";
                case Mode.Taiko:
                    return "taiko";
                case Mode.Ctb:
                    return "catch";
                case Mode.Mania:
                    return "mania";
                default:
                    return null;
            }
        }
    }
}
