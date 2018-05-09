using System;
using System.Collections.Generic;
using System.Text;
using Sisters.WudiLib.Posts;

namespace OsuQqBot.Commands
{
    [CommandApp(AppName, StartCommand)]
    sealed class Chart : CommandApp
    {
        private readonly Endpoint _endpoint;
        private readonly MessageSource _user;

        internal const string AppName = "Chart";
        internal const string StartCommand = "chart";

        private Chart() { }
        private Chart(Endpoint endpoint, MessageSource user)
        {
            _endpoint = endpoint;
            _user = user;
        }

        protected override Endpoint Endpoint => throw new NotImplementedException();

        protected override MessageSource User => throw new NotImplementedException();

        public override string Introduce() => "创建、编辑、发布 chart。";
        public override string Help() => "Chart";
        public override string Help(string param) => Help();

        public override CommandBase Input(string message)
        {
            throw new NotImplementedException();
        }

        public override CommandApp Start(Endpoint endpoint, MessageSource user, string param)
        {
            if (string.IsNullOrWhiteSpace(param)) return new Chart(endpoint, user);

            string exeResult = Charts.ChartExecution.Execute(param);

            return null;
        }
    }
}
