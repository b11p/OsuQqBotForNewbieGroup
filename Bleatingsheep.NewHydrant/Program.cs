using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Osu;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant
{
    class Program
    {
        static void Main(string[] args)
        {
            var cultureInfo = CultureInfo.GetCultureInfo("zh-CN");
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            var configure = new HardcodedConfigure();

            var httpApiClient = new HttpApiClient();
            httpApiClient.AccessToken = configure.AccessToken;
            httpApiClient.ApiAddress = configure.ApiAddress;
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

            try
            {
                var apiPostListener = new ApiPostListener(configure.Listen);
                apiPostListener.SetSecret(configure.Secret);
                apiPostListener.ApiClient = httpApiClient;
                apiPostListener.ForwardTo = "http://oldbot:8876";
                apiPostListener.StartListen();

                // 添加必要的事件处理。
                apiPostListener.FriendRequestEvent += ApiPostListener.ApproveAllFriendRequests;
                apiPostListener.GroupRequestEvent += (api, e) => e.UserId == configure.SuperAdmin ? new GroupRequestResponse { Approve = true } : null;
                apiPostListener.GroupInviteEvent += (api, e) => new GroupRequestResponse { Approve = true };
                //apiPostListener.GroupAddedEvent += (api, e) => api.SetGroupCard(e.GroupId, e.SelfId, _configure.Name).Wait();

                // 配置 osu
                OsuFunction.SetApiKey(configure.ApiKey);

                var hydrant = new Hydrant(configure, httpApiClient, apiPostListener, Assembly.GetExecutingAssembly());

                // 添加异常处理
                hydrant.OnCommandException += Hydrant_OnCommandExceptionAsync;

                hydrant.Init();
            }
            catch (Exception e)
            {
                var logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "fatal.log");
                new Logging.FileLogger(logPath).LogException(e);
            }

            Task.Delay(-1).Wait();
        }

        private static async void Hydrant_OnCommandExceptionAsync(string funName, Exception exception, HttpApiClient api, Sisters.WudiLib.Posts.Message message)
        {
        }
    }
}
