using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static OsuQqBot.Api.OsuApiClient;

namespace OsuQqBot.Api
{
    class OsuApiClient
    {
        private static readonly string protocol = "https";
        private static readonly string site = "osu.ppy.sh";
        private static readonly IReadOnlyDictionary<InformationType, string> paths = new Dictionary<InformationType, string>
            {
                { InformationType.Beatmap, "/api/get_beatmaps" },
                { InformationType.User, "/api/get_user" },
                { InformationType.Scores, "/api/get_scores" },
                { InformationType.BestPerformance, "/api/get_user_best" },
                { InformationType.RecentlyPlayed, "/api/get_user_recent" },
                { InformationType.Multiplayer, "/api/get_match" },
                { InformationType.ReplayData, "/api/get_replay" }
            }; // 各查询类型对应的路径

        private readonly string apiKey;
        public OsuApiClient(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey)) throw new ArgumentNullException(nameof(apiKey));
            this.apiKey = apiKey;
            if (Client == null) Client = this;
        }

        public static OsuApiClient Client { get; private set; }

        /// <summary>
        /// 获取用户信息（修复了API问题）
        /// </summary>
        /// <param name="u">用户名或者 ID（必需）</param>
        /// <param name="type">指定u是用户名还是id</param>
        /// <param name="mode">模式（可选，-1为默认）</param>
        /// <param name="event_days"></param>
        /// <returns>查找到的用户数组；如果没找到，返回空数组；如果发生异常，返回null</returns>
        public async Task<UserRaw[]> GetUserAsync(string u, UsernameType type = UsernameType.Unspecified, Mode mode = Mode.Unspecified, int event_days = -1)
        {
            if (string.IsNullOrWhiteSpace(u)) throw new ArgumentNullException(nameof(u));
            int m = (int)mode;
            if (m < -1 || m > 3) throw new ArgumentOutOfRangeException(nameof(m));
            if (event_days == 0 || event_days < -1 || event_days > 31) throw new ArgumentOutOfRangeException(nameof(event_days));
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("k", apiKey),
                new KeyValuePair<string, string>("u", u)
            };
            if (m != -1) list.Add(new KeyValuePair<string, string>("m", m.ToString()));
            switch (type)
            {
                case UsernameType.Unspecified:
                    break;
                case UsernameType.Username:
                    list.Add(new KeyValuePair<string, string>("type", "string"));
                    break;
                case UsernameType.User_id:
                    if (!long.TryParse(u, out long uid)) throw new ArgumentException(u);
                    list.Add(new KeyValuePair<string, string>("type", "id"));
                    break;
                default:
                    throw new ArgumentException(nameof(type));
            }
            if (event_days != -1) list.Add(new KeyValuePair<string, string>("event_days", event_days.ToString()));
            string jsonResult;
            jsonResult = await GetHttpAsync($"{protocol}://{site}{paths[InformationType.User]}", list.ToArray());
            if (jsonResult == null) return null;
            var result = JsonConvert.DeserializeObject<IEnumerable<UserRaw>>(jsonResult);

            // 确保返回数据满足指定类型
            if (result.Any())
                switch (type)
                {
                    case UsernameType.Username:
                        result = result.TakeWhile(user =>
                            user.username.ToLower(System.Globalization.CultureInfo.InvariantCulture)
                            == u.ToLower(System.Globalization.CultureInfo.InvariantCulture)
                        );
                        break;
                    case UsernameType.User_id:
                        result = result.TakeWhile(user => user.user_id == u);
                        break;
                }
            return result.ToArray();
        }

        /// <summary>
        /// 获取用户信息（修复了API问题），可以指定是否允许本地缓存（未实现）
        /// </summary>
        /// <param name="forceUpdate">强制不使用缓存</param>
        /// <param name="u">用户名或者 ID（必需）</param>
        /// <param name="type">指定u是用户名还是id</param>
        /// <param name="mode">模式（可选，-1为默认）</param>
        /// <param name="event_days"></param>
        /// <returns>查找到的用户数组；如果没找到，返回空数组；如果发生异常，返回null</returns>
        public /*async*/ Task<UserRaw[]> GetUserAsync(bool forceUpdate, string u, UsernameType type = UsernameType.Unspecified, Mode mode = Mode.Unspecified, int event_days = -1)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 从 uid 查找用户名
        /// </summary>
        /// <param name="uid">uid</param>
        /// <returns>用户名；找不到返回string.Empty，出现异常返回null</returns>
        public async Task<string> GetUsernameAsync(long uid)
        {
            var users = await GetUserAsync(uid.ToString(), UsernameType.User_id);
            if (users == null) return null;
            if (!users.Any()) return string.Empty;
            if (users[0].user_id != uid.ToString()) return string.Empty;
            return users[0].username;
        }

        public async Task<BestPerformance[]> GetBestPerformanceAsync(long uid, int count)
        {
            string dnmlgb = await GetHttpAsync(InformationType.BestPerformance, ("k", apiKey), ("u", uid.ToString()), ("limit", count.ToString()));
            if (dnmlgb == null) return null;
            best_performance[] bpRaw = JsonConvert.DeserializeObject<best_performance[]>(dnmlgb);

            //BestPerformance[] result = new BestPerformance[bpRaw.Length];
            //for (int i = 0; i < bpRaw.Length; i++)
            //{
            //    result[i] = (BestPerformance)bpRaw[i];
            //}
            //return result;

            var previousIsShit = from bp in bpRaw
                                 select (BestPerformance)bp;
            return previousIsShit.ToArray();
        }

        public async Task<Beatmap> GetBeatmapAsync(long bid)
        {
            string json = await GetHttpAsync($"{protocol}://{site}{paths[InformationType.Beatmap]}", new KeyValuePair<string, string>("b", bid.ToString()));
            if (json == null) return null;
            var result = JsonConvert.DeserializeObject<beatmap[]>(json);
            if (result == null || result.Length == 0) return null;
            return (Beatmap)result[0];
        }

        //public Beatmap[] GetBeatmap(string k, string b)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// 传入参数并获取 http
        /// </summary>
        /// <param name="type"></param>
        /// <param name="paras">参数</param>
        /// <returns></returns>
        private static async Task<string> GetHttpAsync(InformationType type, params (string key, string value)[] paras)
        {
            return await GetHttpAsync($"{protocol}://{site}{paths[type]}", paras);
        }

        /// <summary>
        /// 传入参数并获取 http
        /// </summary>
        /// <param name="path">访问的路径（完整，包含协议名）</param>
        /// <param name="paras">参数</param>
        /// <returns>获取到的数据（JSON）；如果失败，则为null</returns>
        private static async Task<string> GetHttpAsync(string path, params KeyValuePair<string, string>[] paras)
        {
            using (HttpClient client = new HttpClient())
            {
                StringBuilder urlSB = new StringBuilder(path);
                char neededChar = '?';
                foreach (var param in paras)
                {
                    urlSB.Append($"{neededChar}{param.Key}={param.Value}");
                    neededChar = '&';
                }
                try
                {
                    string result = await client.GetStringAsync(urlSB.ToString());
                    return result;
                }
                catch (HttpRequestException)
                {
                    return null;
                }
                catch (TaskCanceledException)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 传入参数并获取 http
        /// </summary>
        /// <param name="path">访问的路径（完整，包含协议名）</param>
        /// <param name="paras">参数</param>
        /// <returns>获取到的数据（JSON）；如果失败，则为null</returns>
        private static async Task<string> GetHttpAsync(string path, params (string key, string value)[] paras)
        {
            LinkedList<KeyValuePair<string, string>> pair = new LinkedList<KeyValuePair<string, string>>();
            foreach (var (key, value) in paras)
            {
                pair.AddLast(new KeyValuePair<string, string>(key, value));
            }
            return await GetHttpAsync(path, pair.ToArray());
        }

        enum InformationType
        {
            Beatmap,
            User,
            Scores,
            BestPerformance,
            RecentlyPlayed,
            Multiplayer,
            ReplayData,
        }

        /// <summary>
        /// 用户名类型，指定字符串的内容是用户名还是用户ID
        /// </summary>
        public enum UsernameType
        {
            /// <summary>
            /// 未指定
            /// </summary>
            Unspecified,
            /// <summary>
            /// 字符串的内容是用户名
            /// </summary>
            Username,
            /// <summary>
            /// 字符串的内容是用户ID
            /// </summary>
            User_id
        }

        [Flags]
        public enum Mods
        {
            None = 0,
            NoFail = 1,
            Easy = 2,
            NoVideo = 4, // Not used anymore, but can be found on old plays like Mesita on b/78239
            Hidden = 8,
            HardRock = 16,
            SuddenDeath = 32,
            DoubleTime = 64,
            Relax = 128,
            HalfTime = 256,
            Nightcore = 512, // Only set along with DoubleTime. i.e: NC only gives 576
            Flashlight = 1024,
            Autoplay = 2048,
            SpunOut = 4096,
            Relax2 = 8192,  // Autopilot?
            Perfect = 16384, // Only set along with SuddenDeath. i.e: PF only gives 16416  
            Key4 = 32768,
            Key5 = 65536,
            Key6 = 131072,
            Key7 = 262144,
            Key8 = 524288,
            keyMod = Key4 | Key5 | Key6 | Key7 | Key8,
            FadeIn = 1048576,
            Random = 2097152,
            LastMod = 4194304,
            FreeModAllowed = NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn | Relax | Relax2 | SpunOut | keyMod,
            Key9 = 16777216,
            Key10 = 33554432,
            Key1 = 67108864,
            Key3 = 134217728,
            Key2 = 268435456
        }
    }

    public enum Mode
    {
        Unspecified = -1,
        Std = 0,
        Taiko = 1,
        Ctb = 2,
        Mania = 3,
    }

    static class OsuApiExtends
    {
        public static string GetModeString(this Mode mode)
        {
            switch (mode)
            {
                case Mode.Unspecified:
                    return string.Empty;
                case Mode.Std:
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
