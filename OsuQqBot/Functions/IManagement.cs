using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.Functions
{
    interface IManagement
    {
        (long qq, string name) Manager { get; }
        IManagement Manage(string commond);
    }
}
