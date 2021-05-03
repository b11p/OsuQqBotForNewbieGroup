using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Osu;
using Bleatingsheep.NewHydrant.Osu.Newbie;
using Bleatingsheep.OsuQqBot.Database.Execution;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using NLog;
using PuppeteerSharp;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Sisters.WudiLib.WebSocket;

namespace Bleatingsheep.NewHydrant
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var cultureInfo = CultureInfo.GetCultureInfo("zh-CN");
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            // config logger

            var configure = new HardcodedConfigure();

            var httpApiClient = new CqHttpWebSocketApiClient(configure.ApiWS, configure.AccessToken);
#if !DEBUG
            do
            {
                try
                {
                    Console.WriteLine("访问..");
                    var li = httpApiClient.GetLoginInfoAsync().Result;
                    if ((li?.UserId ?? 0) != default(long))
                        break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("无法连接...");
                }
                Console.WriteLine("等待...");
                Task.Delay(5000).Wait();
            } while (true);
#endif

            try
            {
                var apiPostListener = new CqHttpWebSocketEvent(configure.ListenWS, configure.AccessToken);
                apiPostListener.SetSecret(configure.Secret);
                apiPostListener.ApiClient = httpApiClient;
                apiPostListener.StartListen();

                // 配置 osu
                OsuFunction.SetApiKey(configure.ApiKey);

                var hydrant = new Hydrant(httpApiClient, apiPostListener, Assembly.GetExecutingAssembly(), typeof(Hydrant).Assembly)
                    .AddLogger(LogManager.LogFactory);

                // 设置异常处理。
                hydrant.ExceptionCaught_Command += Hydrant_ExceptionCaught_Command;

                hydrant.Init<HydrantStartup>(new HydrantStartup());

                // 添加必要的事件处理。
                apiPostListener.FriendRequestEvent += ApiPostListener.ApproveAllFriendRequests;
                apiPostListener.GroupRequestEvent += (api, e) => e.UserId == configure.SuperAdmin ? new GroupRequestResponse { Approve = true } : null;
                apiPostListener.GroupInviteEvent += (api, e) => new GroupRequestResponse { Approve = true };
                //apiPostListener.GroupAddedEvent += (api, e) => api.SetGroupCard(e.GroupId, e.SelfId, _configure.Name).Wait();
                apiPostListener.GroupRequestEvent += hydrant.CreateServiceInstance<NotifyOnJoinRequest>().Monitor;

                Console.WriteLine("init complete.");

                hydrant.Run();
                Console.WriteLine("Running...");

                Task.Delay(-1).Wait();
            }
            catch (Exception e)
            {
                var logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "fatal.log");
                new Logging.FileLogger(logPath).LogException(e);
                LogManager.Shutdown();

                Console.WriteLine(e);
            }

            // 阻止自动重启
            Task.Delay(-1).Wait();
        }

        private static async Task Hydrant_ExceptionCaught_Command(string funcName, Exception exception, HttpApiClient api, Sisters.WudiLib.Posts.Message message)
        {
            if (exception is DatabaseFailException e)
            {
                await api.SendMessageAsync(
                    endpoint: message.Endpoint,
                    message: e.Message ?? (e.InnerException is DbUpdateConcurrencyException ? "数据库太忙。" : "无法访问数据库。")
                );
                Logging.FileLogger.Default.LogException(e);
            }
            else if (exception is MySqlException)
            {
                await api.SendMessageAsync(message.Endpoint, "无法访问 MySQL 数据库。");
            }
            else if (exception is ApiAccessException)
            {
                // 酷 Q 失败。
            }
            else if (exception is NavigationException)
            {
                await api.SendMessageAsync(message.Endpoint, "无法访问对应网站。").ConfigureAwait(false);
            }
            else
            {
                await api.SendMessageAsync(message.Endpoint, "有一些不好的事发生了。");
                Logging.FileLogger.Default.LogException(exception);
            }
        }
    }
}
