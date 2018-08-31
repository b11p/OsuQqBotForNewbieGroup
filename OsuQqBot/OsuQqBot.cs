using Bleatingsheep.OsuMixedApi.MotherShip;
using Newtonsoft.Json;
using OsuQqBot.Api;
using OsuQqBot.AttributedFunctions;
using OsuQqBot.Data;
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
        private readonly LinkedList<IMessageCommandable> _messageCommands = new LinkedList<IMessageCommandable>();
        private readonly IList<IMessageMonitor> _monitors = new List<IMessageMonitor>();
        private readonly IList<ScheduleInfo> _byIntervalTasks = new List<ScheduleInfo>();
        private readonly IList<ScheduleInfo> _everyDayTasks = new List<ScheduleInfo>();
        private readonly Task _plan;
        private static long daloubot;
        public static long Daloubot => daloubot;

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="LoadedException"></exception>
        /// <param name="qqBot"></param>
        /// <param name="apiClientV2"></param>
        /// <param name="listener"></param>
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

            osuApiKey = config.ApiKey;
            apiClient = new OsuApiClient(config.ApiKey);

            qq.GroupAdminChange += OnGroupAdminChanged;

            qq.GroupMemberIncrease += OnGroupMemberIncreased;

            // 初始化
            OpenApi.Init(
                bindings: new EFData(),
                motherShipApiClient: new MotherShipApiClient(MotherShipApiClient.DefaultHost),
                osuApiClient: Bleatingsheep.OsuMixedApi.OsuApiClient.ClientUsingKey(config.ApiKey)
            );
            _plan = new Task(() =>
            {
                async void Run(ScheduleInfo info)
                {
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
                        if (wait < min)
                            min = wait;
                    }
                    return min;
                }
                while (true)
                {
                    var interval = Clear();
                    Task.Delay(interval).Wait();
                }
            }, TaskCreationOptions.LongRunning);
            Interlocked.CompareExchange(ref daloubot, config.Daloubot, 0);

            v2 = apiClientV2;
            Interlocked.CompareExchange(ref s_apiV2, v2, null);
            _listener = listener;
            _listener.FriendRequestEvent += wudipost::ApiPostListener.ApproveAllFriendRequests;

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
                if (i == typeof(IMessageMonitor))
                {
                    _monitors.Add((IMessageMonitor)lazy.Value);
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

        private void MessageEvent(HttpApiClient api, wudipost.Message message)
        {
            MessageMonitors(message, _monitors, api);
            MessageFunctions(message, _messageCommands, api);
        }

        private static void MessageMonitors(wudipost::Message message, IEnumerable<IMessageMonitor> monitors, HttpApiClient api)
        {
            Parallel.ForEach(monitors, monitor =>
            {
                try
                {
                    monitor.Create().OnMessage(message, api);
                }
                catch (Exception e)
                {
                    Logger.Log(e.ToString());
                }
            });
        }

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
                long uid = Query.Querying.Instance.GetUserBind(newUser).Result ?? 0;
                string username = null;
                string welcome;
                if (uid != 0)
                    username = apiClient.GetUsernameAsync(uid).Result;
                welcome = username != null ? (username.Length == 0 ? "被ban的朋友，" : (username + "，")) : "";
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
            if (paras.Any(p => p == "0" || p.ToLowerInvariant() == "std"))
                mode = Mode.Std;
            else if (paras.Any(p => p == "1" || p.ToLowerInvariant() == "taiko"))
                mode = Mode.Taiko;
            else if (paras.Any(p => p == "2" || p.ToLowerInvariant() == "ctb" || p.ToLowerInvariant() == "catch"))
                mode = Mode.Ctb;
            else if (paras.Any(p => p == "3" || p.ToLowerInvariant() == "mania"))
                mode = Mode.Mania;

            var users = await apiClient.GetUserAsync(uid.ToString(), OsuApiClient.UsernameType.User_id, mode);
            if (users == null)
            { success = false; message = "网络错误"; }
            else if (!users.Any())
            { success = false; message = "没这个人！"; }
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
            if (users == null)
                return (false, "网络错误");
            else if (users.Length == 0)
                return (false, "没这个人！");
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
                if (history.PP < user.PP)
                    byLine[2] += " (+" + (user.PP - history.PP).ToString(".##") + ")";
                else if (history.PP > user.PP)
                    byLine[2] += " (-" + (history.PP - user.PP).ToString(".##") + ")";
                if (history.Rank > user.Rank)
                    byLine[3] += " (↑" + (history.Rank - user.Rank) + ")";
                else if (history.Rank < user.Rank)
                    byLine[3] += " (↓" + (user.Rank - history.Rank) + ")";
                //if(history.RankedScore)
                // 98.96934509277344 98.969345
                // 99.02718353271484 99.02718
                if (Math.Abs(user.Accuracy - history.Accuracy) > 0.000_005)
                {
                    string displayAccChange = (user.Accuracy > history.Accuracy ? "+" : "") + (user.Accuracy - history.Accuracy).ToString(".##");
                    if (displayAccChange == "")
                        displayAccChange = "-";
                    if (char.IsDigit(displayAccChange.Last()))
                        displayAccChange += "%";
                    byLine[6] += " (" + displayAccChange + ")";
                }
                if (history.PlayCount < user.PlayCount)
                    byLine[7] += " (+" + (user.PlayCount - history.PlayCount) + ")";
                if (history.Tth < user.Tth)
                    byLine[8] += " (+" + (user.Tth - history.Tth).ToString("#,###") + ")";
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
            if (group != 514661057 && group != 614892339)
                return string.Empty;
            var tips = database.ListTips();
            lock (_randomLock)
            {
                return tips[this.random.Next(tips.Length)];
            }
        }

        private readonly object _randomLock = new object();
        Random random = new Random();

        /// <summary>
        /// 显示帮助
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="commonds">要显示哪个命令的帮助</param>
        private void ShowHelp(EndPoint endPoint, params string[] commonds)
        {
            if (commonds.Length == 0)
                TellInstructions(endPoint);
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
                    long? aimUid = await Query.Querying.Instance.GetUserBind(long.Parse(atMatch.Groups[1].Value));
                    if (aimUid.HasValue)
                        if (aimUid.Value != 0)
                            await SendQueryMessage(group, aimUid.Value, atMatch.Groups[2].Value);
                        else
                            this.qq.SendGroupMessageAsync(group, "此人未绑定 id");
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
            long? aimUid = await Query.Querying.Instance.GetUserBind(qq);
            if (aimUid.HasValue)
                if (aimUid.Value != 0)
                    return (await ProcessQuery(aimUid.Value, param)).info;
                else
                    return "此人未绑定id";
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

        private readonly OsuApiClient apiClient;
        public static string osuApiKey { get; private set; }

        private readonly long CurrentQq;
    }
}
