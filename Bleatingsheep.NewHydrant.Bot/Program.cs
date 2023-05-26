global using Message = Sisters.WudiLib.SendingMessage;
global using MessageContext = Sisters.WudiLib.Posts.Message;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Osu;
using Bleatingsheep.NewHydrant.Osu.Newbie;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using Npgsql;
using PuppeteerSharp;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Sisters.WudiLib.WebSocket.Reverse;

namespace Bleatingsheep.NewHydrant
{
    internal static class Program
    {
        private static IConfiguration s_configure;
        private static IConfiguration s_hydrantConfigure;

        private static int s_connectedClinetCount = 0;

        private static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            s_configure = host.Services.GetService<IConfiguration>();
            s_hydrantConfigure = s_configure.GetSection("Hydrant");

            var cultureInfo = CultureInfo.GetCultureInfo("zh-CN");
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            try
            {
                // 本应在此设置 HttpApiClient，并启动，但是使用反向 WebSocket 时，无需手动这么做。
#if DEBUG
                var rServer = new ReverseWebSocketServer("http://localhost:9191");
#else
                var rServer = new ReverseWebSocketServer(int.Parse(s_hydrantConfigure["ServerPort"]));
#endif
                rServer.SetListenerAuthenticationAndConfiguration(async r =>
                {
#if DEBUG
                    bool elevated = true;
#else
                    bool elevated = false;
#endif
                    Action<NegativeWebSocketEventListener, long> configuration = (l, selfId) =>
                    {
                        var logger = LogManager.LogFactory.GetLogger("Replica");
                        logger.Info($"客户端 {selfId} 成功连接。");
                        Hydrant hydrant = null;
                        try
                        {
                            if (elevated)
                            {
                                hydrant = ConfigureHost(l.ApiClient, l, elevated, typeof(Highlight).Assembly, typeof(Bind).Assembly);
                            }
                            else
                            {
                                hydrant = ConfigureHost(l.ApiClient, l, elevated, typeof(Highlight).Assembly);
                            }
                            hydrant.Start();
                            Console.WriteLine("Running...");
                            var count = Interlocked.Increment(ref s_connectedClinetCount);
                            logger.Info($"当前已有 {count} 个分身在连接。");
                            l.SocketDisconnected += () =>
                            {
                                Console.WriteLine("Disconnected.");
                                logger.Info("Disconnected");
                                Interlocked.Decrement(ref s_connectedClinetCount);
                                (hydrant as IDisposable)?.Dispose();
                            };
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            (hydrant as IDisposable)?.Dispose();
                        }
                    };

                    Logger logger = LogManager.LogFactory.GetLogger("Replica");
                    logger.Info($"{r.Headers["X-Forwarded-For"]} 尝试连接。");

                    // Check self id
                    if (!long.TryParse(r.Headers["X-Self-ID"], out long selfId))
                    {
                        logger.Info("连接未报告 'X-Self-ID'。");
                        return null;
                    }

                    await using var db = host.Services.GetService<NewbieContext>();
                    var auth = await db.DuplicateAuthentication.Where(a => a.SelfId == selfId).FirstOrDefaultAsync().ConfigureAwait(false);
                    if (auth != null)
                    {
                        // high privilege
                        var headValue = r.Headers["Authorization"];
                        if (string.IsNullOrWhiteSpace(headValue))
                        {
                            logger.Info("连接未报告 'Authorization'。");
                            return null;
                        }
                        int spaceIndex = headValue.IndexOf(' ');
                        if (headValue[(spaceIndex + 1)..] == auth?.AccessToken)
                        {
                            elevated = true;
                            logger.Info($"{selfId} 已提权。");
                            if ((await db.Database.GetPendingMigrationsAsync().ConfigureAwait(false)).Any())
                            {
                                logger.Warn("数据库还有未完成的迁移，请确认数据库迁移已完成。");
                                await db.Database.MigrateAsync().ConfigureAwait(false);
                                logger.Info("自动迁移完成。");
                            }
                            return configuration;
                        }
                        else
                        {
                            logger.Info($"{selfId} 提权失败。");
                            return null;
                        }
                    }

                    // No need to special auth
                    bool generalAuth = ReverseWebSocketServer.CreateAuthenticationFunction(s_hydrantConfigure["ServerAccessToken"], null)(r);
                    return generalAuth ? configuration : null;
                });
                rServer.Start();

                Task.Delay(-1).Wait();
            }
            catch (Exception e)
            {
                var logger = LogManager.LogFactory.GetCurrentClassLogger();
                logger.Fatal(e);

                Console.WriteLine(e);
            }
            finally
            {
                LogManager.Shutdown();
            }

            // 阻止自动重启
            Task.Delay(-1).Wait();
        }

        private static Hydrant ConfigureHost(HttpApiClient httpApiClient, ApiPostListener apiPostListener, bool elevated, params Assembly[] assemblies)
        {
            System.Console.WriteLine("开始配置消防栓。");
            var hydrant = new Hydrant(httpApiClient, apiPostListener, assemblies)
                .AddLogger(LogManager.LogFactory);

            hydrant.SetListenerExceptionHandler(handler => e =>
            {
                if (e is Newtonsoft.Json.JsonReaderException jre && jre.Path == "message_id")
                {
                    return;
                }
                handler(e);
            });

            System.Console.WriteLine("正在配置消防栓。(50%)");
            // 设置异常处理。
            hydrant.ExceptionCaught_Command += Hydrant_ExceptionCaught_Command;

            hydrant.Init<HydrantStartup>(new HydrantStartup(s_configure), s_configure);

            // 添加必要的事件处理。
            // Public
            apiPostListener.GroupRequestEvent += (api, e) => e.UserId == int.Parse(s_hydrantConfigure["SuperAdmin"]) ? new GroupRequestResponse { Approve = true } : null;
            apiPostListener.GroupInviteEvent += (api, e) => e.UserId == int.Parse(s_hydrantConfigure["SuperAdmin"]) ? new GroupRequestResponse { Approve = true } : null;

            // Private-only.
            if (elevated)
            {
                //apiPostListener.FriendRequestEvent += ApiPostListener.ApproveAllFriendRequests;
                //apiPostListener.GroupInviteEvent += (api, e) => new GroupRequestResponse { Approve = true };
                apiPostListener.GroupRequestEvent += hydrant.CreateServiceInstance<NotifyOnJoinRequest>().Monitor;
                apiPostListener.GroupBanEvent += (api, e) =>
                {
                    if (e.Type == GroupBanType.Ban && e.UserId == e.SelfId)
                    {
                        _ = api.CallAsync("set_group_leave", new
                        {
                            group_id = e.GroupId,
                        });
                    }
                };
            }

            Console.WriteLine("init complete.");
            return hydrant;
        }

        private static async Task Hydrant_ExceptionCaught_Command(string funcName, Exception exception, HttpApiClient api, Sisters.WudiLib.Posts.Message message)
        {
            if (exception is NpgsqlException)
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
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((c, s) =>
                {
                    s.AddDbContext<NewbieContext>(
                        optionsBuilder =>
                            optionsBuilder.UseNpgsql(
                                c.Configuration.GetConnectionString("NewbieDatabase_Postgres"),
                                options => options.EnableRetryOnFailure()),
                        ServiceLifetime.Transient);
                });
    }
}
