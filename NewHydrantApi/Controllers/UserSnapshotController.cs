using System;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.Osu;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NewHydrantApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserSnapshotController : ControllerBase
{
    private readonly NewbieContext _dbContext;
    private readonly ILogger<BindingController> _logger;

    public UserSnapshotController(NewbieContext dbContext, ILogger<BindingController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    private static TimeSpan GetError(DateTimeOffset wanted, DateTimeOffset actual)
    {
        var error = wanted - actual;
        if (error < TimeSpan.Zero)
            error = -error;
        return error;
    }

    /// <summary>
    /// Get the snapshot of user info 24 hours ago.
    /// </summary>
    /// <param name="userId">User osu! ID.</param>
    /// <param name="mode">Game mode.</param>
    /// <returns>NotFound if the user has no snapshot data in the past 36 hours. If found, the snapshot taken closest to the moment 24 hours ago.</returns>
    [HttpGet("{userId}", Name = "GetUserInfo")]
    public async Task<ActionResult<UserSnapshot>> Get(int userId, Mode mode)
    {
        // TODO: The code is temporarily copied from QueryHelper.cs. Should refactor later.
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var comparedDate = now.AddHours(-36);
        try
        {
            var snapshots = await _dbContext.UserSnapshots.AsNoTracking()
                .Where(s => s.UserId == userId && s.Mode == mode && s.Date > comparedDate)
                .ToListAsync();
            var history = snapshots
                .MinBy(s => GetError(now - TimeSpan.FromHours(24), s.Date));
            if (history == null)
            {
                return NotFound();
            }
            return history;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "查询用户最新快照时失败。");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
