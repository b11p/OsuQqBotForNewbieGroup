using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bleatingsheep.OsuMixedApi
{
    internal static class Execute
    {
        internal static T Do<T>(Func<T> func, string messageOnFail)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                throw new OsuApiFailedException(messageOnFail, e);
            }
        }

        internal static void Do(Action action, string messageOnFail)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                throw new OsuApiFailedException(messageOnFail, e);
            }
        }

        internal static async Task<T> DoAsync<T>(Func<Task<T>> func, string messageOnFail)
        {
            try
            {
                return await func();
            }
            catch (Exception e)
            {
                throw new OsuApiFailedException(messageOnFail, e);
            }
        }

        internal static async Task DoAsync(Func<Task> func, string messageOnFail)
        {
            try
            {
                await func();
            }
            catch (Exception e)
            {
                throw new OsuApiFailedException(messageOnFail, e);
            }
        }
    }
}
