using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.Functions
{
    interface IManagementContext
    {
        IManagementContext Manage(string commond);
        string GetHelp();
        (string commond, string description)[] GetCommonds();
    }
}
