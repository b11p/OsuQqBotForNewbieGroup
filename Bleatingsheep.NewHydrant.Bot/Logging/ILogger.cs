using System;

namespace Bleatingsheep.NewHydrant.Logging
{
    public interface ILogger
    {
        void LogInBackground<T>(T data);
        void LogException(Exception exception);
    }
}
