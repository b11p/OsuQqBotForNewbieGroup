using Sisters.WudiLib.Posts;
using System;

namespace OsuQqBot.Commands
{
    sealed class Console : CommandBase
    {
        private Endpoint _endpoint;
        private MessageSource _user;
        private string _startCommand;

        public Console(Endpoint endpoint, MessageSource user, string command) : base()
        {
            _endpoint = endpoint;
            _user = user;
            _startCommand = command;
        }

        protected override Endpoint Endpoint => _endpoint;

        private MessageSource User => _user;

        public override CommandBase Input(string message)
        {
            throw new NotImplementedException();
        }
    }
}
