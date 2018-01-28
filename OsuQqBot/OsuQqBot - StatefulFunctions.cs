using OsuQqBot.Functions;
using OsuQqBot.QqBot;
using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot
{
    public partial class OsuQqBot
    {
        /// <summary>
        /// 存储用户状态（for有状态功能）
        /// </summary>
        private LocalData.DictionaryHolder<long, Functions.IFunction> states = new LocalData.DictionaryHolder<long, Functions.IFunction>(null);

        /// <summary>
        /// 用户状态锁（for有状态功能，保证有状态功能的同步执行）
        /// </summary>
        private object stateLock = new object();

        private bool PrivateStatefulFunctions(PrivateEndPoint endPoint, MessageSource source, string message)
        {
            lock (stateLock)
            {
                var state = states.GetValueOrDefault(source.FromQq, null);
                if (state != null)
                {
                    (bool handled, IFunction newState) = state.ProcessMessage(endPoint, source, message);
                    if (handled)
                    {
                        //if (newState != null)
                        //    states.ReplaceOrInsert(source.FromQq, newState);
                        //else states.Delete(source.FromQq);
                        states.ReplaceOrDelete(source.FromQq, newState);
                        return true;
                    }
                }
                else
                {// state = null;
                    bool handled;
                    IFunction newState;
                    (handled, newState) = new AdminAddRemove().ProcessMessage(endPoint, source, message);
                    if (handled)
                    {
                        states.ReplaceOrDelete(source.FromQq, newState);
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
