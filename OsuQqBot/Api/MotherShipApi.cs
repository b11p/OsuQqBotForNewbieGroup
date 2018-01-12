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

        private class MotherShipReturns
        {
            public int code { get; set; }
            public string status { get; set; }
            public Data data { get; set; }
        }

        private class Data
        {
            public long userId { get; set; }
            public string role { get; set; }
            public int qq { get; set; }
            public string legacyUname { get; set; }
            public string currentUname { get; set; }
            public bool banned { get; set; }
            public int repeatCount { get; set; }
            public int speakingCount { get; set; }
        }
    }
}
