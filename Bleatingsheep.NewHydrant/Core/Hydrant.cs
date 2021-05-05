using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Core
{
    public sealed class Hydrant :IDisposable
    {
#nullable enable
        private readonly HttpApiClient _qq;
        private readonly ApiPostListener _listener;
        private readonly Assembly[] _assemblies;
        private readonly CancellationTokenSource _disposeCancellationTokenSource = new CancellationTokenSource();
        private int _isInitialized = 0;
        private LogFactory? _logFactory;
        private IServiceProvider _serviceProvider = default!;

        /// <exception cref="ArgumentException">Some of elements in <c>assemblies</c> was <c>null</c>.</exception>
        public Hydrant(HttpApiClient httpApiClient, ApiPostListener listener, params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            _assemblies = (Assembly[])assemblies.Clone();

            if (_assemblies.Any(a => a is null))
            {
                throw new ArgumentException("Each of elements in assemblies must not be null.", nameof(assemblies));
            }

            _qq = httpApiClient;
            _listener = listener;

            // 配置日志
            //_logger = FileLogger.Default;
            listener.OnException += e => LogException(nameof(ApiPostListener), "Listener 的异常", e);

            // 配置定期任务
            _plan = new Task(() =>
            {
                async void Run(ScheduleInfo info)
                {
                    info.Next();
                    await Task.Run(async () =>
                    {
                        try
                        {
                            await info.Action.RunAsync(_qq);
                        }
                        catch (Exception e)
                        {
                            //_logger.LogException(e);
                            LogException(GetServiceName(info.Action), "定期任务的异常", e);
                        }
                    });
                }
                var limit = new TimeSpan(0, 1, 0);
                var minus = new TimeSpan(0, 0, 10);
                var delay = new TimeSpan(0, 0, 1);
                TimeSpan Clear()
                {
                    //TimeSpan min = TimeSpan.MaxValue;
                    //foreach (var info in _regularTasks)
                    //{
                    //    if (info.ShouldRun())
                    //    {
                    //        Run(info);
                    //    }
                    //    TimeSpan wait = info.WaitTime;
                    //    if (wait < min) min = wait;
                    //}
                    _regularTasks.Where(info => info.ShouldRun()).ForEach(Run);
                    var min = _regularTasks.Min(info => info.WaitTime);
                    min = min > limit ? min - minus : min + delay;
                    return min;
                }
                while (!_disposeCancellationTokenSource.IsCancellationRequested)
                {
                    var interval = Clear();
                    Task.Delay(interval, _disposeCancellationTokenSource.Token).Wait();
                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logFactory"></param>
        /// <exception cref="Exception">The logger is already added.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public Hydrant AddLogger(LogFactory logFactory)
        {
            if (logFactory == null)
            {
                throw new ArgumentNullException(nameof(logFactory));
            }

            var old = Interlocked.CompareExchange(ref _logFactory, logFactory, default);
            if (old != default) throw new InvalidOperationException("Logger can be added only once.");
            return this;
        }

        #region 执行期间各种事件处理器集合
        private readonly IList<IInitializable> _initializableList = new List<IInitializable>();
        private readonly IList<IMessageCommand> _messageCommandList = new List<IMessageCommand>();
        private readonly IList<IMessageMonitor> _messageMonitorList = new List<IMessageMonitor>();
        #endregion

        #region 定期任务
        private readonly IList<ScheduleInfo> _regularTasks = new List<ScheduleInfo>();
        private readonly Task _plan;
        #endregion

        #region init and run
        public void Init() => Init<IHydrantStartup>(default!);

        public void Init<T>(T startup) where T : IHydrantStartup
        {
            if (Interlocked.Exchange(ref _isInitialized, 1) == 0)
            {
                var services = new ServiceCollection();
                startup?.ConfigureServices(services);
                Init(_assemblies, services);
            }
        }

        public void Start()
        {
            var mc = new MemoryCache(new MemoryCacheOptions());
            var slidingExpiration = TimeSpan.FromDays(2);
            _listener.MessageEvent += async (api, message) =>
            {
                // Workaround for go-cqhttp repeatedly post private message.
                if (message is PrivateMessage p)
                {
                    var cached = mc.GetOrCreate(p.MessageId, e =>
                    {
                        e.SetSlidingExpiration(slidingExpiration);
                        return p;
                    });
                    if (cached != p)
                        return;
                }

                // Message monitors
                _ = _messageMonitorList.ForEachAsync(async m =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    try
                    {
                        var monitor = CreateServiceInstance<IMessageMonitor>(m.GetType(), scope);
                        await monitor.OnMessageAsync(message, api).ConfigureAwait(false);
                    }
                    catch (ExecutingException)
                    {
                        // ignored
                    }
                    catch (Exception e)
                    {
                        //_logger.LogException(e);
                        LogException(GetServiceName(m), "Monitor 的异常。", e);
                    }
                });

                // Commands
                using var scope = _serviceProvider.CreateScope();
                IMessageCommand? hit = default;
                try
                {
                    IMessageCommand? last = null;
                    try
                    {
                        hit = _messageCommandList
                        .Select(c => CreateServiceInstance<IMessageCommand>(c.GetType(), scope))
                        .FirstOrDefault(c => (last = c).ShouldResponse(message));
                    }
                    catch (Exception e)
                    {
                        //_logger.LogException(e);
                        LogException(GetServiceName(last), "ShouldResponse 方法引发了一个异常", e);
                        return;
                    }
                    var task = hit?.ProcessAsync(message, api);
                    if (task != null)
                        await task;
                }
                catch (ExecutingException e)
                {
                    if (!string.IsNullOrEmpty(e.Message))
                        await api.SendMessageAsync(
                            endpoint: message.Endpoint,
                            message: e.Message
                        );
                    if (e.InnerException != null)
                    {
                        //_logger.LogException(e.InnerException);
                        LogException(GetServiceName(hit), "包含内部异常的执行异常", e.InnerException);
                    }
                }
                catch (Exception e) //when (_isInitialized == 0) // 暂时不会执行。
                {
                    LogException(GetServiceName(hit), "有一些不好的事发生了。", e);
                    try
                    {
                        var name = GetServiceName(hit);
                        var hTask = ExceptionCaught_Command?.Invoke(name, e, api, message);
                        if (hTask != null)
                            await hTask;
                    }
                    catch (Exception he)
                    {
                        LogException("Exception handling", null, he);
                    }
                }
            };

            // 跑定期任务
            if (_regularTasks.Count > 0)
            {
                _plan.Start();
            }
        }
        #endregion

        internal static string GetServiceName(object? hit)
        {
            var type = hit?.GetType() ?? typeof(Hydrant);
            var attr = type.GetCustomAttribute<ComponentAttribute>();
            return attr?.Name ?? type.Name;
        }

        private void LogException(string name, string? message, Exception e)
        {
            var logger = _logFactory?.GetLogger(name) ?? LogManager.CreateNullLogger();
            logger.Warn(e, message);
        }

        #region Create Service Instance
        public T CreateServiceInstance<T>() where T : notnull, Service
        {
            using var scope = _serviceProvider.CreateScope();
            return CreateServiceInstance<T>(typeof(T), scope);
        }

        private object CreateServiceInstance(Type type, IServiceScope scope)
        {
            var result = type.CreateInstance(scope);
            ConfigureDefaultService(result);
            return result;
        }

        private T CreateServiceInstance<T>(Type type, IServiceScope scope) where T : notnull
        {
            var result = type.CreateInstance<T>(scope);
            ConfigureDefaultService(result);
            return result;
        }

        private void ConfigureDefaultService(object result)
        {
            if (result is Service s)
            {
                s.LogFactory = _logFactory;
            }
        }
        #endregion

        #region init private
        private void Init(IEnumerable<Assembly> assemblies, IServiceCollection services)
        {
            var types = assemblies.SelectMany(a => a.GetTypes()
                .Where(t => t.GetCustomAttributes<ComponentAttribute>().Any()))
                .ToList();

            types.ForEach(t => services.AddTransient(t));
            _serviceProvider = services.BuildServiceProvider();

            types.ForEach(InitType);
        }

        internal void InitType(Type t)
        {
            using var scope = _serviceProvider.CreateScope();
            var interfaces = t.GetInterfaces();
            var lazy = new Lazy<object>(
                valueFactory: () => CreateServiceInstance(t, scope),
                mode: LazyThreadSafetyMode.None
            );
            Array.ForEach(interfaces, i => InitInterface(i, lazy));
        }

        internal void InitInterface(Type t, Lazy<object> lazy)
        {
            if (t == typeof(IInitializable))
            {
                var init = lazy.Value as IInitializable ?? throw new InvalidCastException();

                var success = init.InitializeAsync().Result;
                if (!success)
                    throw new AggregateException();

                if (!string.IsNullOrEmpty(init.Name))
                    _initializableList.Add(init);
            }
            if (t == typeof(IMessageCommand))
            {
                _messageCommandList.Add(lazy.Value as IMessageCommand ?? throw new InvalidCastException());
            }
            if (t == typeof(IMessageMonitor))
            {
                _messageMonitorList.Add(lazy.Value as IMessageMonitor ?? throw new InvalidCastException());
            }
            if (t == typeof(IRegularAsync))
            {
                InitTask(lazy.Value as IRegularAsync ?? throw new InvalidCastException());
            }
        }

        private void InitTask(IRegularAsync task)
        {
            if (task.Every is TimeSpan every)
                _regularTasks.Add(new ScheduleInfo(ScheduleType.ByInterval, every, task));
            if (task.OnUtc is TimeSpan onUtc)
                _regularTasks.Add(new ScheduleInfo(ScheduleType.Daily, onUtc, task));
        }

        public void Dispose()
        {
            _disposeCancellationTokenSource.Cancel();
        }
        #endregion

        #region ExceptionEvent

        /// <summary>
        /// 在执行命令时被框架抓到了异常。
        /// </summary>
        public event Func<string, Exception, HttpApiClient, Sisters.WudiLib.Posts.Message, Task>? ExceptionCaught_Command;

        #endregion
#nullable restore
    }
}
