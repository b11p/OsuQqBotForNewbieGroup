using Newtonsoft.Json;
using OsuQqBot.LocalData.DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Newtonsoft.Json.JsonConvert;

namespace OsuQqBot.LocalData
{
    /// <summary>
    /// 给机器人提供数据存储服务的类
    /// </summary>
    class Database
    {
        /**
         * 务必保证线程安全
         */

        string basePath;

        private static Database single;
        public static Database Single => single;

        /// <summary>
        /// 指定路径，创建Database的新实例
        /// </summary>
        /// <param name="basePath"></param>
        public Database(string basePath)
        {
            try
            {
                this.basePath = basePath;
                Directory.CreateDirectory(BindPath);
                Directory.CreateDirectory(NicknamePath);
                string bindData = "";
                try
                {
                    bindData = File.ReadAllText(BindFilename);
                }
                catch (FileNotFoundException)
                {
                    bindData = "";
                }
                BindData = DeserializeObject<Dictionary<long, BindingData>>(bindData) ?? new Dictionary<long, BindingData>();
                string cachedData = "";
                try
                {
                    cachedData = File.ReadAllText(CachedFilename);
                }
                catch (FileNotFoundException)
                {
                    cachedData = "";
                }
                CachedData = DeserializeObject<Dictionary<long, CachedData>>(cachedData) ?? new Dictionary<long, CachedData>();
                string nickData = string.Empty;
                try
                {
                    nickData = File.ReadAllText(NicknameFilename);
                }
                catch (FileNotFoundException)
                {
                    nickData = string.Empty;
                }
                NicknameData = DeserializeObject<Dictionary<string, long>>(nickData) ?? new Dictionary<string, long>();
                if (single == null) single = this;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                throw;
            }
        }

        /// <summary>
        /// 存储 QQ 与 osu!ID 的绑定数据
        /// </summary>
        string BindPath => Path.Combine(basePath, "Binding Data");
        string BindFilename => Path.Combine(BindPath, "Bind.json");

        /// <summary>
        /// 存储缓存数据的文件名
        /// </summary>
        string CachedFilename => Path.Combine(BindPath, "Cached.json");

        /// <summary>
        /// 存储昵称的目录
        /// </summary>
        string NicknamePath => Path.Combine(basePath, "nicknames");

        string NicknameFilename => Path.Combine(NicknamePath, "Nick.json");

        string TipsFilename => Path.Combine(basePath, "tips.json");

        IDictionary<long, BindingData> BindData { get; set; }
        IDictionary<long, CachedData> CachedData { get; set; }
        IDictionary<string, long> NicknameData { get; set; }
        DataHolder<ISet<long>> Administrators { get; set; }

        public long? Bind(long QQ, long uid, string source)
        {
            long? previous;
            lock (BindData)
            {
                try
                {
                    previous = BindData[QQ].Uid;
                    BindData[QQ] = new BindingData(uid, source);
                }
                catch (KeyNotFoundException)
                {
                    previous = null;
                    BindData.Add(QQ, new BindingData(uid, source));
                }
            }
            CommitBind();
            return previous;
        }

        public long? GetUidFromQq(long QQ)
        {
            lock (BindData)
            {
                return BindData.TryGetValue(QQ, out var bindingData) ? (long?)bindingData.Uid : null;
                //try
                //{
                //    return BindData[QQ].Uid;
                //}
                //catch (KeyNotFoundException) { return null; }
            }
        }

        public string GetUsername(long uid)
        {
            lock (CachedData)
            {
                if (CachedData.TryGetValue(uid, out var cachedData))
                {
                    if (DateTime.UtcNow - cachedData.LastUpdate >
                        new TimeSpan(3, 0, 0, 0)) return null;
                    return cachedData.Username;
                }
                else return null;

                //try
                //{
                //    return CachedData[uid].Username;
                //    CachedData.TryGetValue
                //}
                //catch (KeyNotFoundException)
                //{
                //    return null;
                //}
            }
        }

        public string CacheUsername(long uid, string username)
        {
            string previous;
            lock (CachedData)
            {
                try
                {
                    previous = CachedData[uid].Username;
                    CachedData[uid] = new CachedData(username);
                }
                catch (KeyNotFoundException)
                {
                    previous = null;
                    CachedData.Add(uid, new CachedData(username));
                }
            }
            CommitCache();
            return previous;
        }

        ///// <summary>
        ///// 保存昵称
        ///// </summary>
        ///// <param name="nickname">昵称</param>
        ///// <param name="osuUid">osu!数字ID</param>
        ///// <returns>绑定的用户名和覆盖的用户名</returns>
        //public (string username, string coveredUsername) SaveNickname(string nickname, long osuUid)
        //{
        //    throw new NotImplementedException();
        //    //if (Path.GetInvalidFileNameChars().Any(c => nickname.Contains(c)))
        //    //    throw new ArgumentException(nameof(nickname));
        //    //string saveAt = Path.Combine(NicknamePath, nickname);
        //    //StringBuilder sb = new StringBuilder();
        //    //if (File.Exists(saveAt)) sb.AppendLine($"此昵称已经绑定ID：{File.ReadAllText(saveAt)}，将替换为新ID");
        //    //File.WriteAllText(saveAt, osuUid.ToString());
        //    //sb.AppendLine($"{nickname}就是{osuUid}，记住了！");
        //    //return sb.ToString();
        //}

        public long? SaveNickname(string nickname, long osuUid)
        {
            long? previous;
            lock (NicknameData)
            {
                try
                {
                    previous = NicknameData[nickname];
                    NicknameData[nickname] = osuUid;
                }
                catch (KeyNotFoundException)
                {
                    previous = null;
                    NicknameData.Add(nickname, osuUid);
                }
            }
            CommitNick();
            return previous;
        }

        public long? GetUidFromNickname(string nickname)
        {
            lock (NicknameData)
            {
                return NicknameData.TryGetValue(nickname, out long uid) ? (long?)uid : null;
            }
        }

        public void Commit()
        {
            CommitBind();
            CommitCache();
            CommitNick();
        }

        private void CommitBind()
        {
            lock (BindData)
            {
                File.WriteAllText(BindFilename, SerializeObject(BindData, Formatting.Indented));
            }
        }

        private void CommitCache()
        {
            lock (CachedData)
            {
                File.WriteAllText(CachedFilename, SerializeObject(CachedData, Formatting.Indented));
            }
        }

        private void CommitNick()
        {
            lock (NicknameData)
            {
                File.WriteAllText(NicknameFilename, SerializeObject(NicknameData, Formatting.Indented));
            }
        }

        //public void CommitManually()
        //{
        //    lock (BindData)
        //    {
        //        File.WriteAllText(Path.Combine(BindPath, "save manually.json"), SerializeObject(BindData, Formatting.Indented));
        //    }
        //}
    }
}
