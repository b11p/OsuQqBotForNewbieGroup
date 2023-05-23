using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Net.Http;
using System.Text;
using Bleatingsheep.OsuQqBot.Database.Execution;
using Sisters.WudiLib;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
#nullable enable
    [Component("CheckTheBP1Job")]
    public class CheckTheBP1Job : Service, IRegularAsync
    {
        private readonly ILogger<CheckTheBP1Job> _logger;

        /**
         * http get $"https://osu.ppy.sh/users/{uid}/scores/best?mode=osu&limit=1&offset=0"
         * 数据返回 [{...,"pp":280.01,...},...]
         */
        private static readonly Regex ppRegex = new Regex("\"pp\":([0-9]+(\\.[0-9]+)?)");

        private static readonly Regex nameRegex = new Regex("\"username\":\"([\\w _\\-\\[\\]]+?)\"");

        // 可以写入 本地存储? 
        private const string DestPath = "/outputs/bp1_{0}.csv";
        private const string ResourceUrl = "https://res.bleatingsheep.org/bp1_{0}.csv";
        private INewbieDatabase Database { get; }

        public TimeSpan? OnUtc => new TimeSpan(3, 0, 0);
        public TimeSpan? Every => TimeSpan.FromDays(1);

        private struct UserInfo
        {
            public long qq;
            public string? osuName;
            public float bp1;
            public string? errInfo;

            public UserInfo(long qq)
            {
                this.qq = qq;
                osuName = "";
                bp1 = 0;
                errInfo = "";
            }
            public override string ToString()
            {
                return $"{qq},{osuName},{bp1},{errInfo}\n";
            }
        }

        public CheckTheBP1Job(INewbieDatabase database, ILogger<CheckTheBP1Job> logger)
        {
            Database = database;
            _logger = logger;
        }

        private float getPP(string body)
        {
            Match match = ppRegex.Match(body);
            float pp = 0;
            if (match.Success)
            {
                pp = float.Parse(match.Groups[1].Value);
            }

            return pp;
        }

        [Parameter("group")] public string ProcessingGroupName { get; set; }

        public async Task RunAsync(HttpApiClient api)
        {
            await Task.Run(() =>
            {
                work(api, 928936255).Wait();
            });
        }

        public async Task work(HttpApiClient api, long groupId)
        {
            var infoList = await api.GetGroupMemberListAsync(groupId);
            Logger.Info($"开始检查进阶群,总计 {infoList.Length}");
            var resultList = new List<UserInfo>();
            var client = new HttpClient();
            foreach (var groupMember in infoList)
            {
                long qq = groupMember.UserId;
                var user = new UserInfo(qq);
                try
                {
                    var bindInfoResult = await Database.GetBindingInfoAsync(qq);
                    if (!bindInfoResult.Success)
                    {
                        user.errInfo = "获取绑定信息错误,可能是未绑定";
                        continue;
                    }

                    var bindInfo = bindInfoResult.Result;
                    // 此处是通过公开的 api 查询BP信息,但是仍然会占据当前ip的请求量 
                    var url = $"https://osu.ppy.sh/users/{bindInfo.OsuId}/scores/best?mode=osu&limit=1&offset=0";
                    var data = await client.GetStringAsync(url);
                    user.bp1 = getPP(data);
                    var nameMatch = nameRegex.Match(data);
                    if (nameMatch.Success)
                    {
                        user.osuName = nameMatch.Groups[1].Value;
                    }
                }
                catch (Exception e)
                {
                    user.errInfo = e.Message;
                    // todo error handler
                }

                resultList.Add(user);
                // 防止请求过多
                Thread.Sleep(20000);
            }

            client.Dispose();
            StringBuilder sb = new StringBuilder();
            foreach (var userInfo in resultList)
            {
                sb.Append(userInfo.ToString());
            }
            string result = sb.ToString();
            File.WriteAllText(string.Format(DestPath, groupId), result, new System.Text.UTF8Encoding(true));
            await api.SendMessageAsync( /* 不晓得怎么发送*/null, $"统计完成，前往 {string.Format(ResourceUrl, groupId)} 查看结果。");
        }
    }
}