using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bleatingsheep.OsuMixedApi
{
    internal static class HttpMethods
    {
        /// <summary>
        /// Get array with specified URL and arguments.
        /// </summary>
        /// <typeparam name="T">Type of elements in the array.</typeparam>
        /// <param name="url">Request URL.</param>
        /// <param name="ps">Arguments.</param>
        /// <returns>Required array. <c>null</c> if network failed. Empty if no result.</returns>
        internal static async Task<T[]> GetJsonArrayDeserializeAsync<T>(string url, params (string key, string value)[] ps)
        {
            var (success, result) = await GetJsonDeserializeAsync<T[]>(url, ps);
            if (!success) return null;
            return result;
        }

        internal static async Task<(bool success, T result)> GetJsonDeserializeAsync<T>(string url, params (string key, string value)[] ps)
        {
            string json = await GetAsync(url, ps);
            if (json == null) return (false, default(T));
            T result = JsonConvert.DeserializeObject<T>(json);
            return (true, result);
        }

        private static async Task<string> GetAsync(string url, params (string key, string value)[] ps)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            char needed = '?';
            foreach (var (key, value) in ps)
            {
                url += needed + key + "=" + value;
                needed = '&';
            }
            using (var client = new HttpClient())
            {
                string result = null;
                try
                {
                    result = await client.GetStringAsync(url);
                }
                catch (HttpRequestException)
                {
                }
                catch (TaskCanceledException)
                {
                }
                return result;
            }
        }
    }
}
