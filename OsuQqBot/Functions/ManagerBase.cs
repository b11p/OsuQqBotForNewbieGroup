using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.Functions
{
    class ManageHome : IManagementContext
    {
        public (long qq, string name) Manager { get; private set; }
        
        public (string commond, string description)[] GetCommonds() => throw new NotImplementedException();

        public string GetHelp()
        {
            throw new NotImplementedException();
        }

        public IManagementContext Manage(string commond) => throw new NotImplementedException();
    }
}
