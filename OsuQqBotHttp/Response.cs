using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBotHttp
{
#pragma warning disable IDE1006
    class Response<T>
    {
        public T data { get; set; }
        public int retcode { get; set; }
        public string status { get; set; }
    }
#pragma warning restore IDE1006
}
