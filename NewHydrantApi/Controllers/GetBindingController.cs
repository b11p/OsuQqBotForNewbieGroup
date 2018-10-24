using System.Threading.Tasks;
using Bleatingsheep.OsuMixedApi.MotherShip;
using Bleatingsheep.OsuQqBot.Database.Execution;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace NewHydrantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BindingController : ControllerBase
    {
        private NewbieDatabase _database = new NewbieDatabase();
        private readonly MotherShipApiClient _motherShipApi = new MotherShipApiClient(MotherShipApiClient.DefaultHost);

        [HttpGet("{qq}", Name = "GetBinding")]
        public async Task<ActionResult<BindingInfo>> GetByQq(long qq)
        {
            BindingInfo result = _database.GetBindingInfoAsync(qq).Result.EnsureSuccess().Result;
            if (result != null)
                return result;
            var response = await _motherShipApi.GetUserInfoAsync(qq);
            if (response.Data != null)
            {
                var info = response.Data;
                var u = info.OsuId;
                await Bleatingsheep.OsuQqBot.Database.NewbieDatabase.BindAsync(qq, info.OsuId, info.Name, "Mother Ship", null, null);
                return new BindingInfo() { UserId = qq, OsuId = u, Source = "Mother Ship" };
            }
            return NotFound();
        }
    }
}