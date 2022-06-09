using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.Osu.PerformancePlus;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NewHydrantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlusController : ControllerBase
    {
        private static readonly PerformancePlusSpider s_spider = new PerformancePlusSpider();

        [HttpGet("{u}", Name = "ById")]
        public async Task<ActionResult<object>> GetByOsuId(int u) => await GetPlusInfo(u);

        [HttpGet("username/{username}", Name = "ByUsername")]
        public async Task<ActionResult<object>> GetByUsername(string username) => await GetPlusInfo(username);

        //[HttpGet("{query}", Name = "GetPlus")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<object>> GetPlusInfo(dynamic query)
        {
            var userPlus = (UserPlus)await s_spider.GetUserPlusAsync(query);
            if (userPlus == null)
            {
                return NotFound();
            }

            PlusHistory old = null;
            try
            {
                old = await GetRecentPlusHistory(userPlus.Id);
            }
            catch (Exception)
            {
                // 日志记录问题
            }

            var responseMessage = old == null
                ? $@"{userPlus.Name} 的 PP+ 数据
Performance: {userPlus.Performance}
Aim (Jump): {userPlus.AimJump}
Aim (Flow): {userPlus.AimFlow}
Precision: {userPlus.Precision}
Speed: {userPlus.Speed}
Stamina: {userPlus.Stamina}
Accuracy: {userPlus.Accuracy}"
                : $@"{userPlus.Name} 的 PP+ 数据
Performance: {userPlus.Performance}{userPlus.Performance - old.Performance: (+#); (-#); ;}
Aim (Jump): {userPlus.AimJump}{userPlus.AimJump - old.AimJump: (+#); (-#); ;}
Aim (Flow): {userPlus.AimFlow}{userPlus.AimFlow - old.AimFlow: (+#); (-#); ;}
Precision: {userPlus.Precision}{userPlus.Precision - old.Precision: (+#); (-#); ;}
Speed: {userPlus.Speed}{userPlus.Speed - old.Speed: (+#); (-#); ;}
Stamina: {userPlus.Stamina}{userPlus.Stamina - old.Stamina: (+#); (-#); ;}
Accuracy: {userPlus.Accuracy}{userPlus.Accuracy - old.Accuracy: (+#); (-#); ;}";

            var result = new Dictionary<string, object>
            {
                ["current"] = userPlus,
                ["old"] = old,
                ["message"] = responseMessage,
            };

            return result;
        }

        public static async Task<PlusHistory> GetRecentPlusHistory(int osuId)
        {
            using (var context = new NewbieContext())
            {
                return await context.PlusHistories
                    .Where(ph => ph.Id == osuId)
                    .OrderByDescending(ph => ph.Date)
                    .FirstOrDefaultAsync();
            }
        }
    }
}