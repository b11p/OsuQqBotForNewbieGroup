using System;
using System.Collections.Generic;
using System.Text;
using Bleatingsheep.Osu.PerformancePlus;
using Bleatingsheep.OsuQqBot.Database.Models;
using Newtonsoft.Json;
using WebApiClient;
using WebApiClient.Attributes;

namespace Bleatingsheep.NewHydrant.Osu
{
    [HttpHost("https://api.bleatingsheep.org/api/")]
    public interface IHydrantApiClient : IHttpApi
    {
        [HttpGet("plus/{u}")]
        ITask<PerformancePlusResponse> GetPlusMessage(int u);
    }

    public sealed class PerformancePlusResponse
    {
        [JsonProperty("current")]
        public UserPlus Current { get; private set; }

        [JsonProperty("old")]
        public PlusHistory Old { get; private set; }

        [JsonProperty("message")]
        public string Message { get; private set; }
    }
}
