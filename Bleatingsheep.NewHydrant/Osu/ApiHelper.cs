using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bleatingsheep.NewHydrant.Osu
{
    public static class ApiHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">返回值类型。</typeparam>
        /// <param name="url"></param>
        /// <param name="onContentTypeNotJson"></param>
        /// <exception cref="HttpRequestException">网络错误，或者服务器未返回 200。</exception>
        /// <exception cref="OperationCanceledException">等待时间过长。</exception>
        /// <returns>API 返回结果。</returns>
        public static async Task<T> QueryJsonApiAsync<T>(string url, Func<object, Task> onContentTypeNotJson)
        {
            string content;
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.ContentType.MediaType != "application/json")
                {
                    if (onContentTypeNotJson != null)
                        await onContentTypeNotJson(response.Content);
                    return default;
                }
                content = await response.Content.ReadAsStringAsync();
            }
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
