using System;

namespace Bleatingsheep.OsuMixedApi
{
    [Serializable]
    public class OsuApiFailedException : Exception
    {
        public OsuApiFailedException() { }
        public OsuApiFailedException(string message) : base(message) { }
        public OsuApiFailedException(string message, Exception inner) : base(message, inner) { }
        protected OsuApiFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
