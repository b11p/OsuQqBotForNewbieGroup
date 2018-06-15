using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OsuQqBot.Data
{
    class LegacyGroup : IUserGroupAsync
    {
        /// <summary>
        /// 不检查群名片和PP的列表
        /// </summary>
        private readonly HashSet<long> ignoreList = null;

        /// <summary>
        /// 不检查PP的列表
        /// </summary>
        private readonly HashSet<long> ignorePPList = null;

        private readonly object _thisLock = new object();

        public LegacyGroup()
        {
            var ignoreLines = File.ReadAllLines(Paths.IgnoreListPath);
            if (ignoreLines.Length == 2)
            {
                ignoreList = JsonConvert.DeserializeObject<HashSet<long>>(ignoreLines[0]);
                ignorePPList = JsonConvert.DeserializeObject<HashSet<long>>(ignoreLines[1]);
            }
        }

        private void SaveIgnoreList()
        {
            var ignoreArray = ignoreList.ToArray();
            var ignoreString = JsonConvert.SerializeObject(ignoreArray, Formatting.None);
            var ignorePPArray = ignorePPList.ToArray();
            var ignorePPString = JsonConvert.SerializeObject(ignorePPArray, Formatting.None);
            File.WriteAllLines(Paths.IgnoreListPath, new string[]{
                ignoreString,
                ignorePPString
            });
        }

        public async Task<bool> AddAsync(long qq, string group)
        {
            return await Task.Run(() =>
            {
                lock (_thisLock)
                {
                    if (Is(qq)) return false;
                    ignoreList.Add(qq);
                    ignorePPList.Add(qq);
                    SaveIgnoreList();
                    return true;
                }
            });
        }

        public async Task<bool> DeleteAsync(long qq, string group) => await Task.Run(() => false);

        public async Task<bool> IsAsync(long qq, string group)
        {
            return await Task.Run(() =>
            {
                lock (this._thisLock)
                {
                    return Is(qq);
                }
            });
        }

        private bool Is(long qq) => ignoreList.Contains(qq) || ignorePPList.Contains(qq);
    }
}
