using System.Threading.Tasks;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    internal abstract class 啥玩意儿啊Base
    {
        protected static async Task RecallAndBan(HttpApiClient api, GroupMessage g)
        {
            await api.RecallMessageAsync(g.MessageId);
            await api.BanGroupMember(g.GroupId, g.UserId, 43200);
        }
    }
}
