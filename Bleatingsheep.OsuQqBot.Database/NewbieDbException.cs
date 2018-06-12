using System;
using System.Collections.Generic;
using System.Text;

namespace Bleatingsheep.OsuQqBot.Database
{

    [Serializable]
    public class NewbieDbException : Exception
    {
        public NewbieDbException() { }
        public NewbieDbException(string message) : base(message) { }
        public NewbieDbException(string message, Exception inner) : base(message, inner) { }
        protected NewbieDbException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
