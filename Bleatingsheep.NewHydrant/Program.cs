using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Core;
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

            HttpApiClient.AccessToken = configure.AccessToken;
            var httpApiClient = new HttpApiClient();
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

                var hydrant = new Hydrant(new HardcodedConfigure(), httpApiClient, apiPostListener);
            }
            catch (Exception e)
            {
                var logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "fatal.log");
                new Logging.FileLogger(logPath).LogException(e);
            }

            Task.Delay(-1).Wait();
        }
    }
}
