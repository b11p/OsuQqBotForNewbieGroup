using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bleatingsheep.OsuMixedApi
{
    public static class Diagnostics
    {
        public static event Action<string, long, Exception> OnRequestFinished;

        internal static async void FinishRequest(string url, long milliseconds, Exception exception)
        {
            await Task.Run(() =>
            {
                try
                {
                    OnRequestFinished?.Invoke(url, milliseconds, exception);
                }
                catch (Exception)
                {
                }
            });
        }
    }
}
