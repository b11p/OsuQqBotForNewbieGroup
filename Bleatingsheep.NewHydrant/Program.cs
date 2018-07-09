using System.IO;
using System.Reflection;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant
{
    class Program
    {
        static FileInfo DllFileInfo;
        static void Main(string[] args)
        {
            var httpApiClient = new HttpApiClient();
            httpApiClient.ApiAddress = "http://cq:6700";
            var apiPostListener = new ApiPostListener(8876);
            apiPostListener.ApiClient = httpApiClient;
            apiPostListener.StartListen();
        }
    }
}
