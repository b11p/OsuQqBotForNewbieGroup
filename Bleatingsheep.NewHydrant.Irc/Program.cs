using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.Osu.PerformancePlus;
using Meebey.SmartIrc4net;
using static System.Globalization.CultureInfo;

namespace Bleatingsheep.NewHydrant.Irc
{
    class Program
    {
        static void Main(string[] args)
        {
            string lastMessage = string.Empty;

            try
            {
                #region 区域设置
                // 保证输出和解析文本格式
                var cultureInfo = GetCultureInfo("zh-CN");
                DefaultThreadCurrentCulture = cultureInfo;
                DefaultThreadCurrentUICulture = cultureInfo;
                CurrentCulture = cultureInfo;
                CurrentUICulture = cultureInfo;
                #endregion

                var ircClient = new IrcClient();
                ircClient.Connect("irc.ppy.sh", 6667);
                string password = new Password();
                ircClient.Login("bleatingsheep", "bleatingsheep", 0, "bleatingsheep", password);

                var regex = new Regex(@"\[\S+?(\d+) .*]", RegexOptions.Compiled);
                var spider = new PerformancePlusSpider();

                ircClient.OnQueryAction += (sender, e) =>
                {
                    string username = null; // 用户名
                    string query = null; // 查询 id
                    long costTime = long.MinValue; // 查询时间
                    try
                    {
                        lastMessage = e.Data.RawMessage; // for logging unexpected exception

                        username = e.Data.Nick; // for logging
                        string message = e.Data.Message;
                        var match = regex.Match(message);
                        if (!match.Success)
                        {// 没获取到 bid
                            Log(message);
                            e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "请问您想查询什么呢？");
                            return;
                        }
                        query = match.Groups[1].Value; // for logging
                        int beatmapId = int.Parse(query);

                        // 统计 PP+ 的查询时间
                        var sw = Stopwatch.StartNew();
                        var beatmap = spider.GetCachedBeatmapPlusAsync(beatmapId).Result;
                        costTime = sw.ElapsedMilliseconds;

                        if (beatmap == null)
                        {// 没查到数据
                            e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "您查询的不是一张 *Ranked* 图。");
                            return;
                        }
                        string result = $"Stars: {beatmap.Stars} | Aim (Total/Jump/Flow): {beatmap.AimTotal}*/{beatmap.AimJump}*/{beatmap.AimFlow}* | Precision: {beatmap.Precision}* | Speed: {beatmap.Speed}* | Stamina: {beatmap.Stamina}* | Accuracy: {beatmap.Accuracy}*";
                        e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, result);
                    }
                    catch (AggregateException ex) when (ex.InnerException is ExceptionPlus)
                    {
                        e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "访问 PP+ 网站失败。");
                    }
                    catch (Exception ex)
                    {
                        string message = e?.Data?.Message;
                        Log(message);
                        LogException(ex);
                        e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "查询失败。");
                    }
                    finally
                    {
                        NormalLog(username, query, costTime);
                    }
                };
                ircClient.Listen();
            }
            catch (Exception e)
            {
                Log($"于 {Now} 崩溃。");
                Log(lastMessage);
                if (!(e is PlatformNotSupportedException))
                {
                    LogException(e);
                }
            }
        }

        private static readonly TimeSpan offset = new TimeSpan(8, 0, 0);
        private static DateTime Now => DateTimeOffset.Now.ToOffset(offset).DateTime;
        private static void NormalLog(string username, string beatmapId, long costMilliseconds) => Log($"{Now}|{username}|{beatmapId}|{costMilliseconds}");

        private static void LogException(Exception e) => Log(e);

        private static void Log<T>(T e)
        {
            var file = Assembly.GetExecutingAssembly().Location;
            var logFile = Path.Combine(new FileInfo(file).DirectoryName, "log.txt");
            File.AppendAllText(logFile, e?.ToString() + Environment.NewLine);
        }
    }
}
