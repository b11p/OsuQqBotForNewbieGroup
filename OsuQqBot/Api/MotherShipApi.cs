using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OsuQqBot.Api
{
    class MotherShipApi
    {
        public static async Task<MotherShipUserData> GetUserNearest(long uid, Mode mode = Mode.Std)
        {
            if (mode == Mode.Unspecified) mode = Mode.Std;
            var Nope = await CallApi<MotherShipUserData>($"http://www.mothership.top:8080/api/v1/userinfo/nearest/{uid}?mode={(int)mode}");
            return Nope;
        }

        private static async Task<T> CallApi<T>(string url) where T : class
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 15);
                int tryTime = 0;
                string jsonResult = null;
                do
                    try
                    {
                        jsonResult = await client.GetStringAsync(url);
                        break;
                    }
                    catch (HttpRequestException)
                    {
                        tryTime--;
                        if (tryTime > 0)
                            await Task.Delay(100);
                    }
                    catch (TaskCanceledException)
                    {
                        //Task.Delay(123).Wait();
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
    }

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
        public long Tth => (long)Count300 + Count100 + Count50;

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
