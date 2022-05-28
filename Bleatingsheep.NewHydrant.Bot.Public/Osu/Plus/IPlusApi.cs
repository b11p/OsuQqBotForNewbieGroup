using WebApiClient;
using WebApiClient.Attributes;

namespace Bleatingsheep.NewHydrant.Osu.Plus
{
    [HttpHost("https://syrin.me/pp+/api/")]
    public interface IPlusApi : IHttpApi
    {
        [HttpGet("user/{userId}")]
        ITask<PlusUserReturn> GetUserAsync(int userId);
    }
}
