using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.Osu;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NewHydrantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserPlays : ControllerBase
    {
        private readonly NewbieContext _context;

        public UserPlays(NewbieContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get user play records.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="mode">Mode to query. An integer from 0 to 3.</param>
        /// <param name="start">Query play records starting at this number.</param>
        /// <param name="limit">Maximum count in results.</param>
        /// <returns>Play records.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserPlayRecord>>> GetUserPlays(int userId, Mode mode, int start, int limit = 100)
        {
            return await _context.UserPlayRecords
                .Where(r => r.UserId == userId && r.Mode == mode && r.PlayNumber >= start && r.PlayNumber < start + 100)
                .ToListAsync().ConfigureAwait(false);
        }
    }
}
