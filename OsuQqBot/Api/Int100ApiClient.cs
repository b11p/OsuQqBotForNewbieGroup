using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OsuQqBot.Api
{
    static class Int100ApiClient
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
                int tryTime = 1;
                string jsonResult = null;
                do
                    try
                    {
                        jsonResult = await client.GetStringAsync($"http://www.int100.org/api/get_id.php?k={apiKey}&qq={qq}");
                        tryTime = 0;
                    }
                    catch (HttpRequestException)
                    {
                        Task.Delay(1000).Wait();
                        tryTime--;
                    }
                while (tryTime > 0);
                if (jsonResult == null) return null;
                return JsonConvert.DeserializeObject<UserUid>(jsonResult).uid;
            }
        }

        public static async Task<bool> BindQqAndOsuUid(long qq, long uid)
        {
            using (HttpClient client = new HttpClient())
            {
                int tryTime = 5;
                string jsonResult = null;
                do
                    try
                    {
                        jsonResult = await client.GetStringAsync($"http://www.int100.org/api/bound_qq.php?k={apiKey}&qq={qq}&u={uid}");
                        tryTime = 0;
                    }
                    catch (HttpRequestException)
                    {
                        Task.Delay(1000).Wait();
                        tryTime--;
                    }
                while (tryTime > 0);
                if (jsonResult == null) return false;
                if (JsonConvert.DeserializeObject<BindingResult>(jsonResult).code == 500) return true;
                else return false;
            }
        }

        private static string apiKey =
            System.IO.File.ReadAllText(System.IO.Path.Combine(Paths.DataPath, "Int100ApiKey.txt"));

        public class BindingResult
        {
            public int code { get; set; }
            public string msg { get; set; }
        }

        private class UserUid
        {
            public long uid { get; set; }
        }
    }
}
