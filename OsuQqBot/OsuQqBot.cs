using Newtonsoft.Json;
using OsuQqBot.Api;
using OsuQqBot.AttributedFunctions;
using OsuQqBot.QqBot;
using Sisters.WudiLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using wudipost = Sisters.WudiLib.Posts;

namespace OsuQqBot
{
    /// <summary>
    /// 只能用于CQ平台
    /// </summary>
    public partial class OsuQqBot
    {
        IQqBot qq;
        public static IQqBot QqApi { get; private set; }
        readonly LocalData.Database database = new LocalData.Database(Paths.DataPath);
        HttpApiClient v2;
        private static HttpApiClient s_apiV2;
        public static HttpApiClient ApiV2 { get => s_apiV2; private set => s_apiV2 = value; }
        wudipost::ApiPostListener _listener;
        LinkedList<IMessageCommandable> _messageCommands = new LinkedList<IMessageCommandable>();
        private readonly IList<ScheduleInfo> _byIntervalTasks = new List<ScheduleInfo>();
        private readonly IList<ScheduleInfo> _everyDayTasks = new List<ScheduleInfo>();
        private readonly Task _plan;
        private static long daloubot;
        public static long Daloubot => daloubot;

        public OsuQqBot(IQqBot qqBot, HttpApiClient apiClientV2, wudipost::ApiPostListener listener)
        {
            // 旧版初始化代码
            qq = qqBot;
            QqApi = qq;
            CurrentQq = qq.GetLoginQq();
            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Paths.JsonConfigPath));
            id_Kamisama = config.Kamisama;
            IdWhoLovesInt100Best = id_Kamisama;
            GroupId = config.MainGroup;
            foreach (var item in config.ValidGroups)
            {
                ValidGroups.Add(item);
            }

            try
            {
                var ignoreLines = File.ReadAllLines(Paths.IgnoreListPath);
                if (ignoreLines.Length == 2)
                {
                    ignoreList = JsonConvert.DeserializeObject<HashSet<long>>(ignoreLines[0]);
                    ignorePPList = JsonConvert.DeserializeObject<HashSet<long>>(ignoreLines[1]);
                }
            }
            catch (FileNotFoundException)
            { }
            if (ignoreList == null) ignoreList = new HashSet<long>();
            if (ignorePPList == null) ignorePPList = new HashSet<long>();

            osuApiKey = config.ApiKey;
            apiClient = new OsuApiClient(config.ApiKey);

            qq.GroupAdminChange += OnGroupAdminChanged;

            qq.GroupMemberIncrease += OnGroupMemberIncreased;

            // 初始化
            _plan = new Task(() =>
            {
                async void Run(ScheduleInfo info)
                {
                    Logger.Log(info.Action.GetType().FullName);
                    info.Next();
                    await Task.Run(() =>
                    {
                        try
                        {
                            info.Action.Run();
                        }
                        catch (Exception e)
                        {
                            Logger.LogException(e);
                        }
                    });
                }
                TimeSpan Clear()
                {
                    TimeSpan min = TimeSpan.MaxValue;
                    foreach (var info in _byIntervalTasks.Concat(_everyDayTasks))
                    {
                        if (info.ShouldRun())
                        {
                            Run(info);
                        }
                        TimeSpan wait = info.WaitTime;
                        if (wait < min) min = wait;
                    }
                    return min;
                }
                while (true)
                {
                    var interval = Clear();
                    Logger.Log($"Wait for {interval}");
                    Task.Delay(interval).Wait();
                }
            }, TaskCreationOptions.LongRunning);
            Querying.SetKey(config.ApiKey);
            Interlocked.CompareExchange(ref daloubot, config.Daloubot, 0);

            v2 = apiClientV2;
            Interlocked.CompareExchange(ref s_apiV2, v2, null);
            _listener = listener;
            _listener.FriendRequestEvent += wudipost.ApiPostListener.ApproveAllFriendRequests;

            Init();
        }

        private void Init()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttributes<FunctionAttribute>().Any());
            foreach (var t in types)
            {
                InitType(t);
            }
            _listener.MessageEvent += MessageEvent;
            if (_byIntervalTasks.Count + _everyDayTasks.Count > 0)
            {
                _plan.Start();
            }
        }

        private void InitType(Type t)
        {
            var interfaces = t.GetInterfaces();
            var lazy = new Lazy<object>(() =>
                Assembly.GetExecutingAssembly().CreateInstance(t.FullName),
                LazyThreadSafetyMode.None);
            foreach (var i in interfaces)
            {
                if (i == typeof(IGroupInvitation))
                {
                    _listener.GroupInviteEvent += ((IGroupInvitation)lazy.Value).GroupInvitation;
                }
                if (i == typeof(IMessageCommandable))
                {
                    _messageCommands.AddLast((IMessageCommandable)lazy.Value);
                }
                if (i == typeof(IRegularly))
                {
                    InitTask(lazy.Value as IRegularly);
                }
            }
        }

        private void InitTask(IRegularly task)
        {
            if (task.Every is TimeSpan every)
            {
                var info = new ScheduleInfo(ScheduleType.ByInterval, every, task);
                _byIntervalTasks.Add(info);
            }
            if (task.OnUtc is TimeSpan onUtc)
            {
                var info = new ScheduleInfo(ScheduleType.Daily, onUtc, task);
                _everyDayTasks.Add(info);
            }
        }

        private void MessageEvent(HttpApiClient api, wudipost.Message message) => MessageFunctions(message, _messageCommands, api);

        private static void MessageFunctions(wudipost::Message message, IEnumerable<IMessageCommandable> commandables, HttpApiClient api)
        {
            try
            {
                foreach (var function in commandables.Select(f => f.Create()))
                {
                    if (function.ShouldResponse(message))
                    {
                        function.Process(message, api);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }

        private void OnGroupMemberIncreased(IQqBot sender, GroupMemberIncreaseEventArgs e)
        {
            // debug新成员事件
            //string debug = $"gr:{e.GroupId}, us:{e.UserId}, op:{e.OperatorId}\r\n{e.Time}";
            //sender.SendGroupMessageAsync(72318078, debug, true);

            // 欢迎（在新人群）
            if (e.GroupId == GroupId)
            {
                long newUser = e.UserId;
                long uid = FindUid(newUser).Result ?? 0;
                string username = null;
                string welcome;
                if (uid != 0)
                    username = FindUsername(uid).Result;
                welcome = username != null ? (username == "" ? "被ban的朋友，" : (username + "，")) : "";
                welcome += "你好，欢迎来到新人群";
                sender.SendGroupMessageAsync(e.GroupId, welcome, true);
            }
        }

        private static void OnGroupAdminChanged(IQqBot sender, GroupAdminChangeEventArgs e)
        {
            string message;
            switch (e.Type)
            {
                case GroupAdminChangeEventArgs.GroupAdminChangeType.Set:
                    message = "新的狗管理诞生了";
                    break;
                case GroupAdminChangeEventArgs.GroupAdminChangeType.Unset:
                    message = "从现在起，你就是狗群员了，给我老实点";
                    break;
                default:
                    return;
            }
            message = sender.At(e.UserId) + " " + message;
            sender.SendGroupMessageAsync(e.GroupId, message);
            e.Handled = true;
        }

        /// <summary>
        /// 被At时升级用户数据
        /// </summary>
        /// <param name="context"></param>
        internal async Task<bool> UpdateUserBandingAsync(long group, long qq, string message)
        {
            if (group == GroupId && !ignoreList.Contains(qq))
                if (message.Contains($"[CQ:at,qq={CurrentQq}]"))
                {
                    var uid = await FindUid(qq);
                    if (!uid.HasValue)
                    {
                        this.qq.SendGroupMessageAsync(group, "网络错误，请再试一次");
                        return false;
                    }
                    else if (uid.Value == 0)
                    {
                        this.qq.SendGroupMessageAsync(group, "爷爷并没有绑定，无法更新");
                        return false;
                    }
                    var username = await FindUsername(uid.Value, true);
                    if (username == null)
                    {
                        this.qq.SendGroupMessageAsync(group, "网络错误，请再试一次");
                        return true;
                    }
                    if (string.IsNullOrEmpty(username)) this.qq.SendGroupMessageAsync(group, "咦？爷爷被办了？");
                    else this.qq.SendGroupMessageAsync(group, $"用户名更新为{username}");

                    //this.qq.SendPrivateMessageAsync(id_Kamisama, group.ToString() + " " + qq.ToString());
                    return true;
                }
            return false;
        }



        // 暂不支持
        private void SendQueryMessage(EndPoint endPoint, string username)
        {
            throw new NotImplementedException();
        }

        private async Task SendQueryMessage(EndPoint endPoint, long uid, string para = "")
        {
            switch (endPoint)
            {
                case GroupEndPoint g:
                    await SendQueryMessage(g.GroupId, uid, para);
                    break;
                default:
                    //string message;
                    (bool success, string message) = await ProcessQuery(uid, para);
                    qq.SendMessageAsync(endPoint, message, true);
                    break;
            }
        }

        /// <summary>
        /// （执行）查询玩家信息，返回字符串
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="m">模式，可选0，1，2，3</param>
        /// <returns></returns>
        private async Task<(bool success, string info)> ProcessQuery(long uid, string para = "")
        {
            // 和重载 ProcessQuery(string) 有重复代码，必须择日重构
            bool success;
            string message;
            var mode = Mode.Unspecified;
            var paras = para.Split();
            if (paras.Any(p => p == "0" || p.ToLowerInvariant() == "std")) mode = Mode.Std;
            else if (paras.Any(p => p == "1" || p.ToLowerInvariant() == "taiko")) mode = Mode.Taiko;
            else if (paras.Any(p => p == "2" || p.ToLowerInvariant() == "ctb" || p.ToLowerInvariant() == "catch")) mode = Mode.Ctb;
            else if (paras.Any(p => p == "3" || p.ToLowerInvariant() == "mania")) mode = Mode.Mania;

            var users = await apiClient.GetUserAsync(uid.ToString(), OsuApiClient.UsernameType.User_id, mode);
            if (users == null) { success = false; message = "网络错误"; }
            else if (!users.Any()) { success = false; message = "没这个人！"; }
            else
            {
                try
                {
                    User user = users[0];
                    var history = await MotherShipApi.GetUserNearest(uid, mode);

                    message = BuildQueryMessage(mode, user, history);
                    success = true;
                }
                catch (ArgumentNullException e)
                {
                    Logger.Log("这是非常重要的异常记录！");
                    Logger.LogException(e);
                    Logger.Log(Newtonsoft.Json.JsonConvert.SerializeObject(users[0]));
                    return (false, "未知异常，真的是未知的，咩咩找了好久都没找出来，请联系咩咩并且告诉他最近做了什么操作");
                }
            }
            return (success, message);
        }

        /// <summary>
        /// 通过用户名查询信息，返回字符串（太傻逼了，必须重构）
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private async Task<(bool success, string info)> ProcessQuery(string username)
        {
            // 和重载 ProcessQuery(long uid, string para = "") 有重复代码，必须择日重构
            var users = await apiClient.GetUserAsync(username, OsuApiClient.UsernameType.Username);
            if (users == null) return (false, "网络错误");
            else if (users.Length == 0) return (false, "没这个人！");
            else
            {
                try
                {
                    User user = users[0];
                    var history = await MotherShipApi.GetUserNearest(user.Id);

                    return (true, BuildQueryMessage(Mode.Unspecified, user, history));
                }
                catch (ArgumentNullException e)
                {
                    Logger.Log("这是非常重要的异常记录！");
                    Logger.LogException(e);
                    Logger.Log(Newtonsoft.Json.JsonConvert.SerializeObject(users[0]));
                    return (false, "未知异常，真的是未知的，咩咩找了好久都没找出来，请联系咩咩并且告诉他最近做了什么操作");
                }
            }
        }

        /// <summary>
        /// 根据查询结果和历史数据构造查询结果字符串
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="user"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        private static string BuildQueryMessage(Mode mode, User user, MotherShipUserData history = null)
        {
            //string message;
            //StringBuilder sb = new StringBuilder();
            string[] byLine = new string[9];

            //sb.Append(user.Name + "的个人信息")
            //    .Append(mode == Mode.Unspecified ? "" : "—" + mode.GetModeString()).AppendLine();
            //sb.AppendLine();
            //sb.AppendLine(user.PP + "pp 表现");
            //sb.Append("#" + user.Rank)
            //    .AppendLine(" (" + user.Country + " #" + user.CountryRank + ")");
            string displayAcc;
            try
            {
                displayAcc = user.Accuracy.ToString("#.##");
            }
            catch (FormatException)
            {
                displayAcc = user.Accuracy.ToString();
            }
            //sb.AppendLine(user.RankedScore + " Ranked谱面总分");
            //sb.AppendLine(displayAcc + " 准确率");
            //sb.AppendLine(user.PlayCount + " 游玩次数");
            //sb.Append(user.Tth + " 总命中次数");
            //message = sb.ToString();
            //return message;

            byLine[0] = user.Name + "的个人信息" + (mode == Mode.Unspecified ? "" : "—" + mode.GetModeString());
            byLine[1] = string.Empty;
            byLine[2] = user.PP + "pp 表现";
            byLine[3] = "#" + user.Rank;
            byLine[4] = user.Country + " #" + user.CountryRank;
            byLine[5] = (user.RankedScore).ToString("#,###") + " Ranked谱面总分";
            byLine[6] = displayAcc + "% 准确率";
            byLine[7] = user.PlayCount + " 游玩次数";
            byLine[8] = (user.Tth).ToString("#,###") + " 总命中次数";

            if (history != null)
            {
                if (history.PP < user.PP) byLine[2] += " (+" + (user.PP - history.PP).ToString(".##") + ")";
                else if (history.PP > user.PP) byLine[2] += " (-" + (history.PP - user.PP).ToString(".##") + ")";
                if (history.Rank > user.Rank) byLine[3] += " (↑" + (history.Rank - user.Rank) + ")";
                else if (history.Rank < user.Rank) byLine[3] += " (↓" + (user.Rank - history.Rank) + ")";
                //if(history.RankedScore)
                // 98.96934509277344 98.969345
                // 99.02718353271484 99.02718
                if (Math.Abs(user.Accuracy - history.Accuracy) > 0.000_005)
                {
                    string displayAccChange = (user.Accuracy > history.Accuracy ? "+" : "") + (user.Accuracy - history.Accuracy).ToString(".##");
                    if (displayAccChange == "") displayAccChange = "-";
                    if (char.IsDigit(displayAccChange.Last())) displayAccChange += "%";
                    byLine[6] += " (" + displayAccChange + ")";
                }
                if (history.PlayCount < user.PlayCount) byLine[7] += " (+" + (user.PlayCount - history.PlayCount) + ")";
                if (history.Tth < user.Tth) byLine[8] += " (+" + (user.Tth - history.Tth).ToString("#,###") + ")";
            }

            return string.Join(Environment.NewLine, byLine);
        }

        /// <summary>
        /// QQ群专用查询。给Query传入GroupEndPoint会调用此方法
        /// </summary>
        /// <param name="group"></param>
        /// <param name="uid"></param>
        /// <param name="para"></param>
        private async Task SendQueryMessage(long group, long uid, string para = "")
        {
            //var memberList = qq.GetGroupMemberList(group);
            #region BanchoBot
            //if (memberList.Any(m => m.Qq == 2478057279))
            //{// inter
            //    this.qq.SendGroupMessageAsync(group, $"!stats {uid}" +
            //    (
            //        string.IsNullOrEmpty(para) ? string.Empty :
            //            (char.IsLetterOrDigit(para[0]) ? "," : string.Empty)
            //            + $"{para}"
            //    ), true);
            //}
            //else 
            #endregion
            #region 白菜
            //if ((memberList?.Any(m => m.Qq == 1335734629) ?? false) &&
            //    !(para.Length > 0 && !para.StartsWith('#')))
            //{// 白菜
            //    if (para.Length > 0) para = " " + para;
            //    this.qq.SendGroupMessageAsync(group, $"!statu {uid}{para}", true);
            //}
            //else
            #endregion
            {
                (bool success, string message) = await ProcessQuery(uid, para);
                if (success)
                {
                    string tip = GetTip(group);
                    if (!string.IsNullOrWhiteSpace(tip))
                        message += Environment.NewLine + Environment.NewLine + tip;
                }
                this.qq.SendGroupMessageAsync(group, message);
            }
        }

        private string GetTip(long group)
        // =>
        //group == 514661057 || group == 614892339 ?
        //this.Tips[this.random.Next(this.Tips.Count)] :
        //string.Empty;
        {
            if (group != 514661057 && group != 614892339) return string.Empty;
            var tips = database.ListTips();
            return tips[this.random.Next(tips.Length)];
        }

        Random random = new Random();

        private async Task BindAsync(EndPoint sendBack, long qq, string username)
        {
            IList<string> uname = UsernameUtils.ParseUsername(username);
            if (!(uname.Count == 1 && uname[0] == username)) return;

            long? find = await FindUid(qq);
            UserRaw[] users = await apiClient.GetUserAsync(username, OsuApiClient.UsernameType.Username);

            // 判断网络、判断已绑定。
            if (!find.HasValue || users == null) this.qq.SendMessageAsync(sendBack, "网络错误");
            else if (find.Value != 0)
            {
                this.qq.SendMessageAsync(sendBack, "在已绑定的情况下不允许修改，如需修改请联系 bleatingsheep。");
            }
            else if (users.Length == 0)
            {
                this.qq.SendMessageAsync(sendBack, "找不到用户，未更改绑定。");
            }
            else
            {
                User u = users[0];

                database.Bind(qq, u.Id, "群员手动执行命令");
                this.qq.SendMessageAsync(sendBack, $"绑定为{u.Name}", true);
            }
        }

        /// <summary>
        /// 显示帮助
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="commonds">要显示哪个命令的帮助</param>
        private void ShowHelp(EndPoint endPoint, params string[] commonds)
        {
            if (commonds.Length == 0) TellInstructions(endPoint);
            else //if (commonds[0] == "帮助")
            {
                string help = "";
                switch (commonds[0].ToLowerInvariant())
                {
                    case "帮助":
                        help = @"查看帮助
用法：帮助 [<命令>]
示例：帮助
选项：
命令 要显示帮助的命令；如果没有填写，则显示命令列表

用法说明：
带有尖括号的部分，请根据其中的描述用适当内容取代，不要保留尖括号
带有方括号的部分为可选，参考选项说明";
                        break;
                    case "~":
                    case "～":
                        help = @"查询个人信息
用法：~ [<模式>]
示例：~ taiko
选项：
模式 要查询的模式，允许以下值
0/std 查询osu!模式信息
1/taiko 查询osu!taiko模式信息
2/ctb/catch 查询osu!catch模式信息
3/mania 查询osu!mania模式信息";
                        break;
                    case "绑定":
                        help = @"绑定osu!账号
用法：绑定 <用户名>
示例：绑定 bleatingsheep
选项：
用户名 要绑定的账号";
                        break;
                    case "where":
                        help = @"查询某个玩家的信息（暂只支持std）
用法：where <用户名>
示例：where daloubot
选项：
用户名 要查询的玩家名称";
                        break;
                    case "chart":
                        help = @"chart有关命令列表
*请注意所有命令均以空格（ ）开头*
 charts 查看本群chart
 commit 提交chart
 my 查看提交记录
 rank x 查看编号为x的chart排名";
                        break;
                    default:
                        break;
                }
                this.qq.SendMessageAsync(endPoint, help);
            }
        }

        /// <summary>
        /// 向目标发送指令列表
        /// </summary>
        /// <param name="endPoint"></param>
        private void TellInstructions(EndPoint endPoint)
        {
            string help = @"命令列表
帮助 显示帮助
~ 查询个人信息
绑定 绑定osu!账号
where 查询某个osu!玩家

使用命令“帮助 <命令>”查看特定命令的帮助
此外，请使用“帮助 chart”查看chart有关命令的帮助";
            this.qq.SendMessageAsync(endPoint, help);
        }

        /// <summary>
        /// 根据某人的昵称，或者At某人快速查询数据
        /// </summary>
        /// <param name="group"></param>
        /// <param name="qq"></param>
        /// <param name="message"></param>
        /// <returns>是否已处理（无论是否查询成功）</returns>
        internal async Task<bool> WhirIsBestAsync(long group, long qq, string message)
        {
            string queryAtPatten = @"^\s*查\s*\[CQ:at,qq=(\d+)\]\s*(.*)$";
            var queryAtRegex = new System.Text.RegularExpressions.Regex(queryAtPatten);
            var atMatch = queryAtRegex.Match(message);
            if (atMatch.Success)
            {
                if (group == GroupId)
                {
                    // 判断一下群名片
                }
                await Task.Run(async () =>
                {
                    long? aimUid = await FindUid(long.Parse(atMatch.Groups[1].Value));
                    if (aimUid.HasValue)
                        if (aimUid.Value != 0)
                            await SendQueryMessage(group, aimUid.Value, atMatch.Groups[2].Value);
                        else this.qq.SendGroupMessageAsync(group, "此人未绑定 id");
                });
                return true;
            }
            //if (message.HasCQFunction()) return false;
            //message = message.Replace("&#91;", "[").Replace("&#93;", "]").Replace("&amp;", "&");

            //string queryPatten = @"^\s*查\s*(\S+)\s*$";
            //System.Text.RegularExpressions.Regex queryRegex = new System.Text.RegularExpressions.Regex(queryPatten);
            //var queryMatch = queryRegex.Match(message);
            //if (queryMatch.Success)
            //{
            //    string wantedNickname = queryMatch.Groups[1].Value.ToLowerInvariant();
            //    long? uid = database.GetUidFromNickname(wantedNickname);
            //    if (!uid.HasValue) this.qq.SendGroupMessageAsync(group, $"我不知道{queryMatch.Groups[1].Value}是谁。", true);
            //    else await SendQueryMessage(group, uid.Value);
            //    return true;
            //}

            //string nickPatten = @"^\s*(.+?)\s*叫\s*(\S+)\s*$";
            //var nickRegex = new System.Text.RegularExpressions.Regex(nickPatten);
            //var nickMatch = nickRegex.Match(message);
            //if (nickMatch.Success)
            //{
            //    await Task.Run(async () =>
            //    {
            //        string matchUsername = nickMatch.Groups[1].Value;

            //        // 检查是否是合法的用户名（减少误触发）
            //        var uMatch = regexMatchingUsername.Match(matchUsername);
            //        if (!uMatch.Success || uMatch.Groups[1].Value != matchUsername) return;

            //        string newNick = nickMatch.Groups[2].Value;
            //        if (newNick.EndsWith("，记住了。"))
            //        {
            //            //this.qq.SendGroupMessageAsync(group, "调戏咱很好玩吗？");
            //            return;
            //        }

            //        var users = await apiClient.GetUserAsync(matchUsername, OsuApiClient.UsernameType.Username);
            //        if (users == null)
            //        {
            //            this.qq.SendGroupMessageAsync(group, "再试一次吧~");
            //            return;
            //        }
            //        if (!users.Any())
            //        {
            //            this.qq.SendGroupMessageAsync(group, "没这个人！叫什么叫！");
            //            return;
            //        }
            //        if (!long.TryParse(users[0].user_id, out long uid))
            //        {
            //            this.qq.SendGroupMessageAsync(group, "不知道为什么出错了。");
            //            return;
            //        }
            //        var previous = database.SaveNickname(newNick.ToLower(System.Globalization.CultureInfo.InvariantCulture), uid);
            //        if (!previous.HasValue)
            //        {
            //            this.qq.SendGroupMessageAsync(group, $"{users[0].username}叫{newNick}，记住了。", true);
            //        }
            //        else
            //        {
            //            string previousUsername = await FindUsername(previous.Value);
            //            if (string.IsNullOrEmpty(previousUsername))
            //                this.qq.SendGroupMessageAsync(group, $"有时会突然忘了，{newNick}是谁，但是从现在起，{newNick}就是{users[0].username}，记住了！", true);
            //            else if (
            //                previousUsername.ToLowerInvariant() == users[0].username.ToLowerInvariant()
            //            ) this.qq.SendGroupMessageAsync(group, $"我知道{newNick}是{previousUsername}。", true);
            //            else this.qq.SendGroupMessageAsync(group, $"我还以为{newNick}是{previousUsername}，原来是{users[0].username}，记住了！", true);
            //        }
            //    });
            //    return true;
            //}

            return false;
        }

        private async Task<string> QueryFromQq(long qq, string param = "")
        {
            long? aimUid = await FindUid(qq);
            if (aimUid.HasValue)
                if (aimUid.Value != 0)
                    return (await ProcessQuery(aimUid.Value, param)).info;
                else return "此人未绑定id";
            return "网络错误";
        }

        private async Task<bool> Where(EndPoint endPoint, string where)
        {
            string[] coms = where.Split(',', '，');
            if (coms.Length == 1)
            {
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// 上次检查群名片的时间
        /// </summary>
        private System.Collections.Concurrent.ConcurrentDictionary<long, DateTime> lastCheckTime = new System.Collections.Concurrent.ConcurrentDictionary<long, DateTime>();

        /// <summary>
        /// 自动绑定，并检测群名片是否包含ID，并且检测PP是否超限
        /// </summary>
        /// <param name="context"></param>
        internal async Task TestInGroupNameAsync(long fromGroup, long fromQq, string message)
        {
            if (!checkSwitch) return;
            if (fromGroup != GroupId) return;
            if (DateTime.UtcNow -
                lastCheckTime.GetValueOrDefault(fromQq, new DateTime(2018, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))
                <= new TimeSpan(8, 0, 0)) return;

            try
            {
                if (ignoreList.Contains(fromQq)) return;

                long? uid = await FindUid(fromQq);
                if (!uid.HasValue) return;

                if (uid.Value == 0)
                {// 未绑定，开始自动绑定
                    (bool? success, string username, long _uid) = await AutoBind(fromGroup, fromQq);
                    if (!success.HasValue) return;
                    if (success.Value)
                    {
                        this.qq.SendGroupMessageAsync(fromGroup, $"{At(fromQq)} " + Environment.NewLine +
                            $"{username}，你好！" + Environment.NewLine +
                            $"欢迎来到osu!新人群。请发送“~”查询信息。如果你不是{username}，请联系bleatingsheep。"
                        );
                        (bool queryOK, string query) = await ProcessQuery(_uid);
                        if (queryOK) query += Environment.NewLine + Environment.NewLine + "此号数据卓越，同分段中的王者！——interBot";
                        this.qq.SendGroupMessageAsync(fromGroup, query, true);
                    }
                    else
                    {
                        string inGroupName = this.qq.GetGroupMemberInfo(fromGroup, fromQq)?.InGroupName;
                        if (inGroupName?.StartsWith("【无ID】") ?? false)
                        {
                            lastCheckTime[fromQq] = DateTime.UtcNow;
                            this.qq.SendGroupMessageAsync(fromGroup, $"{At(fromQq)} 你好，请尽快注册，并修改群名片。");
                        }
                        else
                            this.qq.SendGroupMessageAsync(fromGroup, $"{At(fromQq)} 你好，请将群名片改为osu!中的名字，以便互相认识。如果暂时没有 ID，请保证群名片以“【无ID】”开头。");
                    }
                }
                else
                {// 已绑定，开始检查
                    lastCheckTime[fromQq] = DateTime.UtcNow;

                    string inGroupName;
                    inGroupName = GetInGroupName(fromGroup, fromQq);
                    if (inGroupName == null) return;

                    var (result, hisUsername) = await CheckInGroupName(inGroupName, uid.Value);

                    string hint = string.Empty;
                    switch (result)
                    {
                        case InGroupNameCheckResult.NeverBind:
                            break;
                        case InGroupNameCheckResult.Error:
                            break;
                        case InGroupNameCheckResult.NotContains:
                            hint = $"为了方便其他人认出您，请修改群名片，必须包括osu!用户名。";
                            break;
                        case InGroupNameCheckResult.NotOwner:
                            hint = $"为了方便其他人认出您，请修改群名片，必须包括正确的osu!用户名。";
                            break;
                        case InGroupNameCheckResult.IsSubstring:
                            hint = "建议修改群名片，不要在用户名前后添加可以被用做用户名的字符，以免混淆。";
                            hint += Environment.NewLine + "建议群名片：" + RecommendCard(inGroupName, hisUsername);
                            break;
                        case InGroupNameCheckResult.Qualified:
                            break;
                        default:
                            break;
                    }
                    if (!string.IsNullOrEmpty(hint))
                    {
                        this.qq.SendGroupMessageAsync(fromGroup, At(fromQq) + Environment.NewLine +
                            hisUsername + "，您好。" + this.qq.BeforeSend(hint));
                    }

                    if (!ignorePPList.Contains(fromQq))
                    {
                        await CheckIfPPOverLimit(fromGroup, fromQq, uid.Value);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                Logger.Log("throw");
                throw;
            }
        }

        /// <summary>
        /// 根据群名片和用户名推荐群名片
        /// </summary>
        /// <param name="card"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        private static string RecommendCard(string card, string username)
        {
            int firstIndex = card.IndexOf(username, StringComparison.InvariantCultureIgnoreCase);
            if (firstIndex != -1)
            {
                string recommendCard = card.Substring(0, firstIndex);
                if (firstIndex != 0)
                    recommendCard += "|";
                recommendCard += username;
                if (firstIndex + username.Length < card.Length)
                {
                    recommendCard += "|" + card.Substring(firstIndex + username.Length);
                }
                return recommendCard;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("无法给出推荐名片");
                sb.AppendLine("群名：" + card);
                sb.AppendLine("用户名：" + username);
                Logger.Log(sb.ToString());
                return null;
            }
        }

        /// <summary>
        /// 不检查群名片和PP的列表
        /// </summary>
        private readonly HashSet<long> ignoreList = null;

        /// <summary>
        /// 不检查PP的列表
        /// </summary>
        private readonly HashSet<long> ignorePPList = null;

        private void SaveIgnoreList()
        {
            var ignoreArray = ignoreList.ToArray();
            var ignoreString = Newtonsoft.Json.JsonConvert.SerializeObject(ignoreArray, Newtonsoft.Json.Formatting.None);
            var ignorePPArray = ignorePPList.ToArray();
            var ignorePPString = Newtonsoft.Json.JsonConvert.SerializeObject(ignorePPArray, Newtonsoft.Json.Formatting.None);
            File.WriteAllLines(Paths.IgnoreListPath, new string[]{
                ignoreString,
                ignorePPString
            });
        }

        /// <summary>
        /// 检查群名片是否包含osu的名字
        /// </summary>
        /// <param name="fromGroup">来自群组</param>
        /// <param name="fromQq">来自QQ</param>
        /// <param name="uid">osu! uid，如果未绑定，则为0</param>
        /// <returns></returns>
        private async Task<(InGroupNameCheckResult, string)> CheckInGroupName(string inGroupName, long uid)
        {
            var possibleUsernames = UsernameUtils.ParseUsername(inGroupName);
            if (possibleUsernames.Count == 0)
            {
                return (InGroupNameCheckResult.NotContains, null);
            }

            if (uid != 0)
            {
                string foundUsername = await FindUsername(uid); //找到的用户名
                if (foundUsername == null) return (InGroupNameCheckResult.Error, null); //查找失败
                if (possibleUsernames.Any(psb =>
                    psb.ToLowerInvariant() == foundUsername.ToLowerInvariant()
                )) return (InGroupNameCheckResult.Qualified, foundUsername); //OK
                return inGroupName.IndexOf(foundUsername, StringComparison.InvariantCultureIgnoreCase) != -1 ?
                    (InGroupNameCheckResult.IsSubstring, foundUsername) :
                    (InGroupNameCheckResult.NotOwner, foundUsername);
            }
            else
            {
                return (InGroupNameCheckResult.NeverBind, null);
            }
        }

        /// <summary>
        /// 通过群名片自动绑定
        /// </summary>
        /// <param name="qq"></param>
        /// <param name="inGroupName"></param>
        /// <returns>是否成功；如果因为群名片不好返回<c>false</c>；网络问题返回<c>null</c></returns>
        private async Task<(bool? success, string username, long uid)> AutoBind(long group, long qq)
        {
            string inGroupName;
            var memberInfo = this.qq.GetGroupMemberInfo(group, qq);
            if (memberInfo == null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("找不到群员信息");
                sb.AppendLine(group.ToString() + ", " + qq.ToString());
                Logger.Log(sb.ToString());
                return (null, null, 0);
            }
            inGroupName = memberInfo.InGroupName;

            var possibleUsernames = UsernameUtils.ParseUsername(inGroupName);

            (string username, long findUid) = await CheckUsername(possibleUsernames);
            if (username == null) return (null, null, 0);
            if (username == string.Empty)
            {
                return (false, username, 0);
            }
            else
            {
                database.CacheUsername(findUid, username);
                database.Bind(qq, findUid, "Auto");
                //var success = await Int100ApiClient.BindQqAndOsuUid(qq, findUid);
                Logger.Log("自动绑定" + qq + username);
                return (true, username, findUid);
            }
        }

        /// <summary>
        /// 群名片检查结果
        /// </summary>
        private enum InGroupNameCheckResult
        {
            /// <summary>
            /// 此人从未绑定过osu!ID
            /// </summary>
            NeverBind = 5,
            /// <summary>
            /// 检查过程中出现错误
            /// </summary>
            Error = 0,
            /// <summary>
            /// 不包含任何合法群名片
            /// </summary>
            NotContains = 1,
            /// <summary>
            /// 包含至少一个合法的群名片，但是没有他正在使用的
            /// </summary>
            NotOwner = 2,
            /// <summary>
            /// 包含群名片，但是前后有其他字符
            /// </summary>
            IsSubstring = 3,
            /// <summary>
            /// 合格，或者自动绑定后合格
            /// </summary>
            Qualified = 4
        }

        /// <summary>
        /// （执行）检查PP
        /// </summary>
        /// <param name="fromGroup"></param>
        /// <param name="fromQq"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        private async Task CheckIfPPOverLimit(long fromGroup, long fromQq, long uid)
        {
            var users = await apiClient.GetUserAsync(uid.ToString(), OsuApiClient.UsernameType.User_id);
            if (users == null || !users.Any()) return;
            if (double.TryParse(users[0].pp_raw, out double pp))
            {
                const int ppLimit = 2500;
                if (pp >= ppLimit)
                    qq.SendGroupMessageAsync(fromGroup, $"[CQ:at,qq={fromQq}] 您的PP超限，即将离开本群");
                //qq.SendGroupMessageAsync(fromGroup, $"[CQ:at,qq={fromQq}] 您的PP已经超过2600，如果超过3000，将离开本群");
                else
                {
                    var bp = await apiClient.GetBestPerformanceAsync(uid, 1);
                    double bpLimit = 185;
                    if (bp?.Length > 0 && bp[0]?.PP >= bpLimit)
                    {
                        this.qq.SendGroupMessageAsync(fromGroup, $"{At(fromQq)} 您的BP超限，即将离开本群");
                    }
                }
            }
        }

        private readonly OsuApiClient apiClient;
        public static string osuApiKey { get; private set; }

        private readonly long CurrentQq;
    }
}
