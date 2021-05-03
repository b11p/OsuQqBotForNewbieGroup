using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
    internal interface INewbieInfoProvider
    {
        Task<bool> ShouldIgnoreAsync(long qq);
        Task<bool> ShouldIgnorePerformanceAsync(long group, long qq);
        IEnumerable<long> MonitoredGroups { get; }
        double? PerformanceLimit(long group);
    }
}
