using Sisters.WudiLib.Posts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace OsuQqBot.Commands
{
    sealed class Console : CommandBase
    {
        private static readonly IDictionary<string, CommandApp> s_apps;
        private readonly Endpoint _endpoint;
        private readonly MessageSource _user;
        private readonly string _startCommand;

        static Console()
        {
            CommandApp TryCreateInstance(Type type)
            {
                try
                {
                    return Activator.CreateInstance(type) as CommandApp;
                }
                catch (Exception)
                {// TODO: Logging
                    return null;
                }
            }

            var appsDictionary = new Dictionary<string, CommandApp>();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var attributes = type.GetCustomAttributes<CommandAppAttribute>().SingleOrDefault();
                if (attributes is null) continue;
                CommandApp instance = TryCreateInstance(type);
                if (instance is null) continue;
                foreach (string command in attributes.Command)
                {
                    if (!appsDictionary.TryAdd(command, instance))
                    {// TODO: Logging
                    }
                }
            }
            Interlocked.CompareExchange(ref s_apps, appsDictionary, null);
        }

        public Console(Endpoint endpoint, MessageSource user, string command) : base()
        {
            _endpoint = endpoint;
            _user = user;
            _startCommand = command;
        }

        protected override Endpoint Endpoint => _endpoint;

        public override CommandBase Input(string message)
        {
            throw new NotImplementedException();
        }
    }
}
