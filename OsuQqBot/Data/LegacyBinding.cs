using Bleatingsheep.OsuQqBot.Database.Models;

namespace OsuQqBot.Data
{
    class LegacyBinding : IBindings
    {
        public int? Bind(long qq, int osuId, string osuName, string source, long operatorId, string operatorName) => (int?)LocalData.Database.Instance.Bind(qq, osuId, source);

        public BindingInfo GetBindingInfo(long qq)
        {
            var queried = LocalData.Database.Instance.GetUidFromQq(qq);
            if (queried is long osuId)
            {
                return new BindingInfo { UserId = qq, OsuId = (int)osuId };
            }
            return null;
        }

        public int? UserIdOf(long qq)
        {
            var queried = LocalData.Database.Instance.GetUidFromQq(qq);
            if (queried is long osuId)
            {
                return (int)osuId;
            }
            return null;
        }
    }
}
