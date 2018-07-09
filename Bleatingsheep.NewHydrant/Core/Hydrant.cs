using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Core
{
    public sealed class Hydrant
    {
        private readonly HttpApiClient _qq;
        private readonly ApiPostListener _post;
        private readonly IConfigure _configure;

        public Hydrant(IConfigure configure, HttpApiClient apiClientV2, ApiPostListener listener)
        {
            _qq = apiClientV2;
            _post = listener;
            _configure = new HardcodedConfigure();
        }
    }
}
