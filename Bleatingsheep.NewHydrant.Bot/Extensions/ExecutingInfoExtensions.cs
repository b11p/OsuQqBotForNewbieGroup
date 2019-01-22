using System;
using System.Threading.Tasks;
using Bleatingsheep.OsuQqBot.Database.Execution;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Extentions
{
    internal static class ExecutingInfoExtensions
    {
        public static async Task<bool> SendIfFailAsync(this IExecutingResult executingResult, HttpApiClient httpApiClient, Endpoint endpoint)
        {
            if (executingResult.Success) return true;

            await httpApiClient.SendMessageAsync(endpoint, "访问数据库失败。");
            return false;
        }

        public static async Task<bool> SendIfFailOrNoBindAsync<T>(this IExecutingResult<T?> executingResult, HttpApiClient httpApiClient, Endpoint endpoint) where T : struct
        {
            if (!await SendIfFailAsync(executingResult, httpApiClient, endpoint)) return false;

            if (executingResult.Result.HasValue) return true;

            await httpApiClient.SendMessageAsync(endpoint, "没有绑定！");
            return false;
        }
    }
}
