using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.Functions
{
    class ManageHome : IManagementContext
    {
        public (long qq, string name) Manager { get; private set; }

        public string Name => "Main";

        public string Description => "管理Bot的主页";

        private readonly LocalData.Database database;
        private readonly long qq;
        private readonly string name;

        /// <summary>
        /// 初始化管理主页的新实例
        /// </summary>
        /// <param name="qq">QQ号</param>
        /// <param name="name">用户名</param>
        public ManageHome(long qq, string name, LocalData.Database database)
        {
            this.qq = qq;
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public (string commond, string description)[] GetCommonds() => throw new NotImplementedException();

        public string GetHelp()
        {
            throw new NotImplementedException();
        }

        public IManagementContext Manage(string commond) => throw new NotImplementedException();
    }
}
