using System.Collections.Generic;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Core;

namespace Bleatingsheep.NewHydrant.Admin
{
    internal class AdminVerifier : IVerifier
    {
        private static readonly HashSet<long> AdminCollection = new HashSet<long>
        {
            962549599, //
            1208604740, // iron
            1239219529, // taolex
            1061566571, // dalou
            546748348, // 化学式
            431600414, // 844
            2482000231, // 杰克王
            2541721178, // heisiban
        };

        public AdminVerifier(ExecutingInfo executingInfo)
        {
        }

        public Task<bool> IsAdminAsync(long qq) => new Task<bool>(() => AdminCollection.Contains(qq));
    }
}
