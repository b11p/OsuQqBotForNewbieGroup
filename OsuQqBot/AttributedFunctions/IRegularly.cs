using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.AttributedFunctions
{
    interface IRegularly
    {
        TimeSpan? OnUtc { get; }
        TimeSpan? Every { get; }
    }
}
