using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace OsuQqBot.Api
{
    static class BloodcatApiClient
    {
        private static readonly string BaseUrl = "https://bloodcat.com/osu/?mod=json";

        public static async System.Threading.Tasks.Task<(bool networkSuccess, string artist, string title)> GetTitleAndArtistAsync(long bid)
        {
            string url = BaseUrl + "&c=b";
            url += "&q=" + bid;
            using (HttpClient client = new HttpClient())
            {
                string json;
                try
                {
                    json = await client.GetStringAsync(url);
                }
                catch (HttpRequestException)
                {
                    return (false, null, null);
                }
                BloodcatSet[] result = Newtonsoft.Json.JsonConvert.DeserializeObject<BloodcatSet[]>(json);
                if (!(result?.Length >= 1)) return (true, null, null);
                return (true, result[0].artistU ?? result[0].artist, result[0].titleU ?? result[0].title);
            }
        }
    }

    class BloodcatSet
    {
        public string id { get; set; }
        public string artist { get; set; }
        public string artistU { get; set; }
        public string title { get; set; }
        public string titleU { get; set; }
        public string creatorId { get; set; }
        public string creator { get; set; }
        public string status { get; set; }
        public string synced { get; set; }
        public Difficulty[] beatmaps { get; set; }
    }

    class Difficulty
    {
        public string id { get; set; }
        public string name { get; set; }
        public string mode { get; set; }
        public string hp { get; set; }
        public string cs { get; set; }
        public string od { get; set; }
        public string ar { get; set; }
        public string bpm { get; set; }
        public string length { get; set; }
        public string star { get; set; }
    }
}
