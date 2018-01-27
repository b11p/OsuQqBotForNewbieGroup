using System;
using System.Collections.Generic;
using System.Text;
using OsuQqBot.QqBot;

namespace OsuQqBot.Functions
{
    /// <summary>
    /// 添加或者删除管理员的功能
    /// </summary>
    class AdminAddRemove : IFunction
    {
        /// <summary>
        /// 指示是否正在处理中。
        /// </summary>
        bool inProcess;

        /// <summary>
        /// 根据是否处理返回应有的IFunction
        /// </summary>
        private IFunction StateFunction => inProcess ? this : null;

        public (bool handled, IFunction state) ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message)
        {

        }
    }
}
