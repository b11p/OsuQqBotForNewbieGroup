using System;

namespace Bleatingsheep.OsuMixedApi
{
    public class OsuApiFailedException : Exception
    {
        public OsuApiFailedException() { }
        public OsuApiFailedException(string message) : base(message) { }
        public OsuApiFailedException(string message, Exception inner) : base(message, inner) { }
    }
}
