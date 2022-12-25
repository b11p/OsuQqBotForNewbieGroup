using System.Threading.Tasks;
using Bleatingsheep.OsuQqBot.Database.Models;

namespace Bleatingsheep.NewHydrant.Data;
public interface IOsuDataUpdator
{
    public ValueTask<(bool isChanged, int? oldOsuId, BindingInfo newBindingInfo)> AddOrUpdateBindingInfoAsync(long qq, int osuId, string osuName, string source, long? operatorId, string operatorName, string reason = "", bool allowOverwrite = false);
}
