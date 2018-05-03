using Bleatingsheep.OsuMixedApi;
using OsuQqBot.Api;
using System;
using System.Collections.Generic;
using System.Threading;
using mixed = Bleatingsheep.OsuMixedApi;

namespace OsuQqBot
{
    sealed class Querying
    {
        private string key = null;
        private static Querying instance = null;

        private Querying(string key) => this.key = key;

        public IReadOnlyCollection<UserInfo> CheckUsername(IEnumerable<string> possibleUsernames, bool requireAllSucceeded = true)
        {
            var userInfos = new System.Collections.Concurrent.ConcurrentBag<UserInfo>();
            var api = mixed::OsuApiClient.ClientUsingKey(key);

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
