using Bleatingsheep.OsuQqBot.Database.Models;

namespace OsuQqBot.Data
{
    interface IBindings
    {
        int? Bind(long qq, int osuId, string osuName, string source, long operatorId, string operatorName);

        int? UserIdOf(long qq);

        BindingInfo GetBindingInfo(long qq);
    }
}
