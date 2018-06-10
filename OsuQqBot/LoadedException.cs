using System;

namespace OsuQqBot
{
    /// <summary>
    /// 已经初始化，不能再次初始化。
    /// </summary>
    [Serializable]
    public class LoadedException : Exception
    {
        public LoadedException() { }
        public LoadedException(string message) : base(message) { }
        public LoadedException(string message, Exception inner) : base(message, inner) { }
        protected LoadedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
