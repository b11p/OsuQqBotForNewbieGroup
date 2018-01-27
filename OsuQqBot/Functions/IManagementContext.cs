using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.Functions
{
    interface IManagementContext
    {
        string Name { get; }
        string Description { get; }
        IManagementContext Manage(string commond);
        string GetHelp();
        (string commond, string description)[] GetCommonds();
    }
}
