using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.OsuQqBot.Database.Execution;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NewHydrantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BindingController : ControllerBase
    {
        private NewbieDatabase _database = new NewbieDatabase();

        [HttpGet("{qq}", Name = "GetBinding")]
        public ActionResult<BindingInfo> GetByQq(long qq)
        {
            BindingInfo result = _database.GetBindingInfoAsync(qq).Result.EnsureSuccess().Result;
            if (result == null) return NotFound();
            return result;
        }
    }
}