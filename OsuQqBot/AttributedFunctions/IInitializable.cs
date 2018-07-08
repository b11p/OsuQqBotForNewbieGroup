using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.AttributedFunctions
{
    interface IInitializable
    {
        string Name { get; }

        bool Initialize();
    }
}
