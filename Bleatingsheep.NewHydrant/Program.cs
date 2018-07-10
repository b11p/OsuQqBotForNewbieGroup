using System;
using System.Globalization;
using System.IO;
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

            var httpApiClient = new HttpApiClient();
            httpApiClient.ApiAddress = configure.ApiAddress;
            do
            {
                try
                {
                    Console.WriteLine("访问..");
                    var li = httpApiClient.GetLoginInfoAsync().Result;
                    if (li?.UserId != default(long)) break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("无法连接...");
                }
                Console.WriteLine("等待...");
                Task.Delay(5000).Wait();
            } while (true);

            var apiPostListener = new ApiPostListener(configure.Listen);
            apiPostListener.ApiClient = httpApiClient;
            apiPostListener.StartListen();

            var hydrant = new Hydrant(new HardcodedConfigure(), httpApiClient, apiPostListener);

            Task.Delay(-1).Wait();
        }
    }
}
