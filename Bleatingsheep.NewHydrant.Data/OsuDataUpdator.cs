using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bleatingsheep.NewHydrant.Data;
public class OsuDataUpdator : IOsuDataUpdator
{
    private readonly IDbContextFactory<NewbieContext> _dbContextFactory;
    private readonly ILogger<OsuDataUpdator> _logger;

    public OsuDataUpdator(IDbContextFactory<NewbieContext> dbContextFactory, ILogger<OsuDataUpdator> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async ValueTask<(bool isChanged, int? oldOsuId, BindingInfo newBindingInfo)> AddOrUpdateBindingInfoAsync(
        long qq, int osuId, string osuName, string source, long? operatorId, string? operatorName, string reason = "", bool allowOverwrite = false)
    {
        _logger.LogInformation("Binding request. osu! name: {osuName} ({osuId}), QQ: {qq}", osuName, osuId, qq);
        await using var db = _dbContextFactory.CreateDbContext();
        var binding = await db.Bindings.Where(bi => bi.UserId == qq).FirstOrDefaultAsync().ConfigureAwait(false);
        var oldOsuId = binding?.OsuId;
        if (binding == null)
        {
            binding = new()
            {
                UserId = qq,
                OsuId = osuId,
                Source = source,
            };
            db.Bindings.Add(binding);
        }
        else if (binding.OsuId == osuId)
        {
            return (false, oldOsuId, binding);
        }
        else
        {
            if (allowOverwrite)
            {
                binding.OsuId = osuId;
                binding.Source = source;
            }
            else
            {
                // do not allow overwrite
                return (false, oldOsuId, binding);
            }
        }
        var historyEntry = await db.Histories.AddAsync(new()
        {
            Operation = Operation.Binding,
            UserId = qq,
            User = osuName,
            OperatorId = operatorId,
            Operator = operatorName,
            Remark = $"osu! ID: {osuId}; source: {source}; reason: {reason}",
        }).ConfigureAwait(false);
        await db.SaveChangesAsync().ConfigureAwait(false);
        return (true, oldOsuId, binding);
    }
}
