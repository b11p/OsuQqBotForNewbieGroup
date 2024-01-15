using System.Collections.Generic;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Core;

namespace Bleatingsheep.NewHydrant.Admin
{
    internal class AdminVerifier : IVerifier
    {
        private static readonly HashSet<long> AdminCollection = new HashSet<long>
        {
            962549599, // 咩咩羊
            1208604740, // iron
            // 1239219529, // taolex
            1061566571, // dalou
            546748348, // 化学式
            // 431600414, // 844
            // 2482000231, // 杰克王
            // 2541721178, // heisiban
            447503971, // 白季
            944072537, // na-gi
            1340691940, // muzi
            178039743, // whir
            2429299722, // sayori
            // 1904603706, // 226
            2636027237, // morika
            3203995073, // happy
            2897010516, // pr1mary
            3228981717, // slyuyuko
            1172482284, // UselessPlayer
            1528769425, // m u s e
            2624161473, // guozi
            630060047, // CYCLC
            365246692, // -Spring Night-
            1120180945, // n0000000000o
            2199188467, // NatsuRin
            524986802, // Dragon-Fox
            411843675, // xxbg
            1584775323. // YRScarlet
        };

        public AdminVerifier()
        {
        }

        public Task<bool> IsAdminAsync(long qq) => Task.FromResult(AdminCollection.Contains(qq));
    }
}
