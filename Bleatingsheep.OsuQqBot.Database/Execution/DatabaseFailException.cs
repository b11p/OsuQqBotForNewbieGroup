using System;

namespace Bleatingsheep.OsuQqBot.Database.Execution
{

    [Serializable]
    public class DatabaseFailException : Exception
    {
        public DatabaseFailException() { }
        public DatabaseFailException(string message) : base(message) { }
        public DatabaseFailException(string message, Exception inner) : base(message, inner) { }
        protected DatabaseFailException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
