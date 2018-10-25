using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Moebooru
{
    class Api
    {
        private const string SafeRating = "s";
        private const string QuestionableRating = "q";
        private const string ExplicitRating = "e";

        private const string PopularPath = "/post/popular_recent.json";
        private string Popular => domain + PopularPath;

        readonly string domain;

        public Api(string domain)
        {
            this.domain = domain.TrimEnd('/');
        }

        public bool EnableR18 { get; set; } = false;

        private static async Task<T> GetTAsync<T>(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var s = await client.GetStringAsync(url);
                    T result = JsonConvert.DeserializeObject<T>(s);
                    return result;
                }
            }
            catch (Exception) { return default(T); }
        }

        public async Task<IEnumerable<Post>> PopularRecentAsync()
        {
            IEnumerable<Post> result = await GetTAsync<Post[]>(Popular);

            if (!EnableR18) result = result?.Where(p => p.rating == SafeRating);
            return result;
        }

        internal async Task<(IEnumerable<Post> result, string info)> PopularRecentDebugAsync()
        {
            IEnumerable<Post> result = await GetTAsync<Post[]>(Popular);
            var groups = result.GroupBy(p => p.rating);
            var infos = new LinkedList<string>();
            foreach (var group in groups)
            {
                infos.AddLast($"{group.Key}: {group.Count()}");
            }
            string info = string.Join("\r\n", infos);
            if (!EnableR18) result = result.Where(p => p.rating == SafeRating);
            return (result, info);
        }
    }
}
