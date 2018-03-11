using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OsuQqBot.Api
{
    class MotherShipApi
    {
        /// <summary>
        /// 获取QQ号绑定的osu!id
        /// </summary>
        /// <param name="qq">QQ号</param>
        /// <returns>osu!id；如果找不到，则为0；如果发生异常，则为null</returns>
        public static async Task<long?> GetOsuUidFromQqAsync(long qq)
        {
            using (HttpClient client = new HttpClient())
            {
                int tryTime = 5;
                string jsonResult = null;
                do
                    try
                    {
                        jsonResult = await client.GetStringAsync($"http://www.mothership.top:8080/api/v1/user/qq/{qq}");
                        tryTime = 0;
                    }
                    catch (HttpRequestException)
                    {
                        Task.Delay(100).Wait();
                        tryTime--;
                    }
                while (tryTime > 0);
                if (jsonResult == null) return null;
                var result = JsonConvert.DeserializeObject<MotherShipReturns>(jsonResult);
                if (result.code != 0) return 0;
                return result.data.userId;
            }
        }

        public static async Task<MotherShipUserData> GetUserNearest(long uid, Mode mode = Mode.Std)
        {
            if (mode == Mode.Unspecified) mode = Mode.Std;
            var Nope = await CallApi<MotherShipUserData>($"http://www.mothership.top:8080/api/v1/userinfo/nearest/{uid}?mode={(int)mode}");
            if (Nope == null)
            {   // 如果没找到记录，就访问妈船API让白菜开始记录
                // 实际应该调用不到了
                // 如果还是调用了，会有日志记录
                Logger.Log($"为什么妈船还是没有这个人的数据啊。（uid={uid}, mode={mode})");
                using (var httpClient = new HttpClient())
                    try
                    { using (await httpClient.GetStreamAsync(GetStatUrl(uid))) { } }
                    catch (HttpRequestException)
                    { }
            }
            return Nope;
        }

        public static string GetStatUrl(long uid, Mode mode = Mode.Std)
        {
            if (mode == Mode.Unspecified) mode = Mode.Std;
            return $"http://www.mothership.top:8080/api/v1/stat/{uid}?mode={(int)mode}";
        }

        private class MotherShipReturns
        {
            public int code { get; set; }
            public string status { get; set; }
            public Binding data { get; set; }
        }

        private static async Task<T> CallApi<T>(string url) where T : class
        {
            using (HttpClient client = new HttpClient())
            {
                int tryTime = 5;
                string jsonResult = null;
                do
                    try
                    {
                        jsonResult = await client.GetStringAsync(url);
                        break;
                    }
                    catch (HttpRequestException)
                    {
                        Task.Delay(100).Wait();
                        tryTime--;
                    }
                    catch (TaskCanceledException)
                    {
                        Logger.Log("抓到TaskCanceledException了");
                        Task.Delay(123).Wait();
                        tryTime--;
                    }
                while (tryTime > 0);
                if (jsonResult == null) return null;
                var response = JsonConvert.DeserializeObject<MotherShipResponse<T>>(jsonResult);
                if (response.code != 0) return null;
                return response.data;
            }
        }

        private class MotherShipResponse<T>
        {
            public int code { get; set; }
            public string status { get; set; }
            public T data { get; set; }
        }

        private class Binding
        {
            public long userId { get; set; }
            public string role { get; set; }
            public long qq { get; set; }
            public string legacyUname { get; set; }
            public string currentUname { get; set; }
            public bool banned { get; set; }
            public int repeatCount { get; set; }
            public int speakingCount { get; set; }
        }
    }

    //public class MotherShipUserHistoryResponse
    //{
    //    public int code { get; set; }
    //    public string status { get; set; }
    //    public MotherShipUserData[] data { get; set; }
    //}

    public class MotherShipUserData
    {
        [JsonProperty("userId")]
        public long Id { get; private set; }

        [JsonProperty("count300")]
        private int Count300 { get; set; }
        [JsonProperty("count100")]
        private int Count100 { get; set; }
        [JsonProperty("count50")]
        private int Count50 { get; set; }

        [JsonIgnore]
        public long Tth => Count300 + Count100 + Count50;

        [JsonProperty("playcount")]
        public int PlayCount { get; private set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; private set; }

        [JsonProperty("ppRaw")]
        public double PP { get; private set; }

        [JsonProperty("rankedScore")]
        public long RankedScore { get; private set; }

        [JsonProperty("totalScore")]
        public long TotalScore { get; private set; }

        [JsonProperty("level")]
        private double Level { get; set; }

        [JsonProperty("ppRank")]
        public int Rank { get; private set; }

        //public int countRankSs { get; private set; }
        //public int countRankSsh { get; private set; }
        //public int countRankS { get; private set; }
        //public int countRankSh { get; private set; }
        //public int countRankA { get; private set; }

        [JsonProperty("queryDate")]
        public Querydate QueryDate { get; private set; }
    }

    public class Querydate
    {
        [JsonProperty("year")]
        public int Year { get; private set; }

        [JsonProperty("month")]
        public int Month { get; private set; }

        [JsonProperty("day")]
        public int Day { get; private set; }
    }
}
