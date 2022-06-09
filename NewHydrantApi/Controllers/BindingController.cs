using System.Globalization;
using System.Threading.Tasks;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NewHydrantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BindingController : ControllerBase
    {
        // Note that this is a legacy class, which is different with NewbieContext.
        private readonly NewbieContext _dbContext;

        private readonly ILogger<BindingController> _logger;

        public BindingController(ILogger<BindingController> logger, NewbieContext newbieContext)
        {
            _logger = logger;
            _dbContext = newbieContext;
        }

        [HttpGet("{qq}", Name = "GetBinding")]
        public async Task<ActionResult<BindingInfo>> GetByQq(long qq)
        {
            BindingInfo result = null;
            try
            {
                result = await _dbContext.Bindings.SingleOrDefaultAsync(b => b.UserId == qq).ConfigureAwait(false);
                if (result != null)
                    return result;
                return NotFound();
            }
            finally
            {
                _logger.LogInformation("来自 [{0}] 请求查询 {1}，结果为 {2}，结果来源 {3}",
                    HttpContext.Connection.RemoteIpAddress,
                    qq.ToString(CultureInfo.InvariantCulture),
                    result?.OsuId.ToString(CultureInfo.InvariantCulture) ?? "null",
                    result?.Source);
            }
        }
    }
}