using System.Threading.Tasks;

namespace OsuQqBot
{
    public partial class OsuQqBot
    {
        /// <summary>
        /// 从uid查找用户名
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="forceUpdate">强制查询网络</param>
        /// <returns>找不到返回string.Empty，网络异常返回null</returns>
        private async Task<string> FindUsername(long uid, bool forceUpdate = false)
        {
            string foundUsername = null;
            if (!forceUpdate)
                foundUsername = database.GetUsername(uid);
            if (string.IsNullOrEmpty(foundUsername))
            {
                foundUsername = await apiClient.GetUsernameAsync(uid);
                if (!string.IsNullOrEmpty(foundUsername))
                    database.CacheUsername(uid, foundUsername);
            }
            return foundUsername;
        }
    }
}