using OsuQqBot.Api;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OsuQqBot
{
    public partial class OsuQqBot
    {

        /// <summary>
        /// 从 qq 号查找 uid
        /// </summary>
        /// <param name="qq">QQ号</param>
        /// <param name="forceUpdate">强制通过 int100 的API查找</param>
        /// <returns>找不到返回0，网络异常返回null</returns>
        private async Task<long?> FindUid(long qq, bool forceUpdate = false)
        {
            long? uid = null;
            bool isFromApi = false; // 成功从别人的 API 处找到绑定信息
            string fromApi = string.Empty;
            if (!forceUpdate)
                uid = database.GetUidFromQq(qq);
            if (uid == null)
            {
                uid = await MotherShipApi.GetOsuUidFromQqAsync(qq);
                if (uid == null)
                {
                    Logger.Log($"从妈船找QQ={qq}的绑定信息失败（网络问题）");
                }
                if (uid.Value != 0)
                {
                    isFromApi = true;
                    fromApi = "Mother Ship";
                }
            }

            if (uid == null)
            {// 数据库找不到UID，在int的数据库查找
                uid = await Int100ApiClient.GetOsuUidFromQqAsync(qq);
                if (uid == null)
                {
                    Logger.Log($"找QQ={qq}的绑定信息失败");
                    return null;
                }
                if (uid.Value != 0)
                {
                    isFromApi = true;
                    fromApi = "int100";
                }
            }
            // 此时 uid != null
            if (isFromApi) database.Bind(qq, uid.Value, fromApi + " (while running)");
            return uid;
        }

        /// <summary>
        /// 从uid查找用户名
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="forceUpdate">强制查询网络</param>
        /// <returns>找不到返回string.Empty，网络异常返回null</returns>
        private async Task<string> FindUsername(long uid, bool forceUpdate = false)
        {
            string foundUsername = null;
            if (!forceUpdate) foundUsername = database.GetUsername(uid);
            if (string.IsNullOrEmpty(foundUsername))
            {
                foundUsername = await apiClient.GetUsernameAsync(uid);
                if (!string.IsNullOrEmpty(foundUsername)) database.CacheUsername(uid, foundUsername);
            }
            return foundUsername;
        }

        /// <summary>
        /// 从可能的用户名里找到真正被注册的，返回用户名和uid
        /// </summary>
        /// <param name="possibleUsernames"></param>
        /// <returns>真正被注册的用户名；如果有多个有效或者查询失败，则返回null；如果没有一个有效，返回string.Empty</returns>
        private async Task<(string username, long uid)> CheckUsername(string[] possibleUsernames)
        {
            List<UserRaw> list = new List<UserRaw>();
            foreach (var uName in possibleUsernames)
            {
                var qResults = await apiClient.GetUserAsync(uName, OsuApiClient.UsernameType.Username);
                if (qResults == null) return (null, 0);
                if (!qResults.Any()) continue;
                if (qResults[0].username.ToLower(System.Globalization.CultureInfo.InvariantCulture) == uName.ToLower(System.Globalization.CultureInfo.InvariantCulture))
                    list.Add(qResults[0]);
                if (list.Count > 1) return (null, 0);
            }
            if (list.Any()) return (list[0].username, long.Parse(list[0].user_id));
            return (string.Empty, 0);
        }

    }
}