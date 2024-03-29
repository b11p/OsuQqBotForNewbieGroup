﻿using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace Bleatingsheep.OsuMixedApi
{
    internal static class HttpMethods
    {
        private static HttpClient s_httpClient = new HttpClient();
        private static long s_httpClientCreateDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

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
            T result = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return (true, result);
        }

        private static async Task<string> GetAsync(string url, params (string key, string value)[] ps)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            char needed = '?';
            foreach (var (key, value) in ps)
            {
                url += needed + key + "=" + HttpUtility.UrlEncode(value);
                needed = '&';
            }

            UpdateClientInstanceIfNecessary();
            var client = s_httpClient;
            string result = null;
            var stopwatch = Stopwatch.StartNew();
            Exception exception = null;
            try
            {
                result = await client.GetStringAsync(url);
            }
            catch (Exception e)
            {
                exception = e;
            }
            Diagnostics.FinishRequest(url, stopwatch.ElapsedMilliseconds, exception);
            return result;
        }

        private static void UpdateClientInstanceIfNecessary()
        {
            int outdated = 1800;
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now - s_httpClientCreateDate > outdated)
            {
                s_httpClient = new HttpClient();
                s_httpClientCreateDate = now;
            }
        }
    }
}
