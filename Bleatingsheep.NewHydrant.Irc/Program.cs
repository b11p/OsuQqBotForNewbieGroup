using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Bleatingsheep.Osu.PerformancePlus;
using Meebey.SmartIrc4net;
using static System.Globalization.CultureInfo;

namespace Bleatingsheep.NewHydrant.Irc
{
    class Program
    {
        static void Main(string[] args)
        {
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
                    try
                    {
                        string message = e.Data.Message;
                        var match = regex.Match(message);
                        if (!match.Success)
                        {// 没获取到 bid
                            Log(message);
                            e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "请问您想查询什么呢？");
                            return;
                        }
                        int beatmapId = int.Parse(match.Groups[1].Value);
                        var beatmap = spider.GetBeatmapPlusAsync(beatmapId).Result;
                        if (beatmap == null)
                        {// 没查到数据
                            e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "您查询的不是一张 *Ranked* 图。");
                            return;
                        }
                        string result = $"Stars: {beatmap.Stars} | Aim (Total/Jump/Flow): {beatmap.AimTotal}*/{beatmap.AimJump}*/{beatmap.AimFlow}* | Precision: {beatmap.Precision}* | Speed: {beatmap.Speed}* | Stamina: {beatmap.Stamina}* | Accuracy: {beatmap.Accuracy}*";
                        e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, result);
                    }
                    catch (ExceptionPlus)
                    {
                        e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "访问 PP+ 网站失败。");
                    }
                    catch (Exception ex)
                    {
                        string message = e?.Data?.Message;
                        Log(message);
                        LogException(ex);
                    }
                };
                ircClient.Listen();
            }
            catch (Exception e)
            {
                LogException(e);
            }

        }

        private static void LogException(Exception e) => Log(e);

        private static void Log<T>(T e)
        {
            var file = Assembly.GetExecutingAssembly().Location;
            var logFile = Path.Combine(new FileInfo(file).DirectoryName, "log.txt");
            File.AppendAllText(logFile, e?.ToString());
        }
    }
}
