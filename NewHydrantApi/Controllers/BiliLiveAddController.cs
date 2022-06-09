using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NewHydrantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BiliLiveAddController : ControllerBase
    {
        [HttpGet("{roomId}")]
        public async Task<ActionResult<string>> GetStreamAddress(int roomId)
        {
            // 检测指定参数错误
            if (roomId <= 0)
            {
                return BadRequest();
            }

            using (var httpClient = new HttpClient())
            {
                var json = await httpClient.GetStringAsync($"https://api.live.bilibili.com/room/v1/Room/playUrl?cid={roomId}&otype=json&quality=0&platform=web");
                var obj = JsonConvert.DeserializeObject<JObject>(json);
                return obj?["data"]?["durl"]?[0]?["url"]?.ToObject<string>();
            }
        }
    }
}