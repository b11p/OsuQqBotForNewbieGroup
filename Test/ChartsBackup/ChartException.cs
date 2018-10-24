using System;

namespace OsuQqBot.Charts
{
    [Serializable]
    internal class ChartException : Exception
    {
        public ChartException() { }
        public ChartException(string message) : base(message) { }
        public ChartException(string message, Exception inner) : base(message, inner) { }
        protected ChartException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
