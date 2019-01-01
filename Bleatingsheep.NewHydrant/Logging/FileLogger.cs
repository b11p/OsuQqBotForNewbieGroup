using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.Logging
{
    class FileLogger : ILogger
    {
        private readonly string _file;
        private readonly TimeSpan _offset = new TimeSpan(8, 0, 0);
        private readonly object _thisLock = new object();
        public FileLogger(string file) => _file = file;
        public FileLogger(string file, TimeSpan offset) : this(file) => _offset = offset;

        private static readonly Lazy<ILogger> s_instance = new Lazy<ILogger>(() =>
        {
            var executingFile = Assembly.GetExecutingAssembly().Location;
            var logFile = Path.Combine(Path.GetDirectoryName(executingFile), "log.txt");
            return new FileLogger(logFile);
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        public static ILogger Default => s_instance.Value;

        public void LogException(Exception exception) => LogLinesInBackground(DateTimeOffset.Now.ToOffset(_offset).DateTime, exception);

        public async void LogInBackground<T>(T data)
        {
            await Task.Run(() =>
            {
                lock (_thisLock)
                {
                    try
                    {
                        File.AppendAllText(_file, data?.ToString() + Environment.NewLine);
                    }
                    catch (Exception)
                    {
                    }
                }
            });
        }

        public async void LogLinesInBackground(params object[] logs)
        {
            await Task.Run(() =>
            {
                lock (_thisLock)
                {
                    try
                    {
                        File.AppendAllLines(_file, logs.Select(l => l?.ToString()));
                    }
                    catch (Exception)
                    {
                    }
                }
            });
        }
    }
}
