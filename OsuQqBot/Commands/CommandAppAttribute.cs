using System;
using System.Collections.Generic;

namespace OsuQqBot.Commands
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class CommandAppAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        public CommandAppAttribute(string appName, params string[] commands)
        {
            this.AppName = appName;
            Command = commands;
        }

        public string AppName { get; }

        public IEnumerable<string> Command { get; }

        public EndpointType ValidEndpointType { get; set; } = EndpointType.All;
    }

    [Flags]
    internal enum EndpointType
    {
        None = 0,
        Private = 1,
        Group = 2,
        Discuss = 4,
        All = Private | Group | Discuss,
    }
}
