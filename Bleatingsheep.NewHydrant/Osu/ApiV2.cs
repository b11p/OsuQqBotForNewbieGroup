using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.Osu.ApiV2;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Function("api2_provider")]
    internal class ApiV2 : IInitializable
    {
        public static OsuApiV2Client Client { get; private set; }

        public string Name { get; } = "apiv2";

        public async Task<bool> InitializeAsync(ExecutingInfo executingInfo)
        {
            var authPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "config", "authv2.txt");
            var lines = await File.ReadAllLinesAsync(authPath);
            if (lines.Length != 2)
                return false;
            string username = lines[0];
            string password = lines[1];
            Client = new OsuApiV2Client(username, password);
            return true;
        }
    }
}
