using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Addon;
using Bleatingsheep.NewHydrant.Attributions;
using NLog;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Core
{
    public sealed class Hydrant
    {
        private readonly HttpApiClient _qq;
        private readonly ApiPostListener _listener;
        private readonly Assembly[] _assemblies;
        private int _isInitialized = 0;
        private LogFactory _logFactory;

        /// <exception cref="ArgumentException">Some of elements in <c>assemblies</c> was <c>null</c>.</exception>
        public Hydrant(HttpApiClient httpApiClient, ApiPostListener listener, params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            _assemblies = assemblies.Clone() as Assembly[];

            if (_assemblies.Any(a => a is null))
            {
                throw new ArgumentException("No assembly avalible.", nameof(assemblies));
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
                while (true)
                {
                    var interval = Clear();
                    Task.Delay(interval).Wait();
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
            if (old != default) throw new Exception();
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

        public void Init()
        {
            if (Interlocked.Exchange(ref _isInitialized, 1) == 0)
            {
                Init(_assemblies);
            }
        }

        internal static string GetServiceName(object hit)
        {
            if (hit == null)
            {
                throw new ArgumentNullException(nameof(hit));
            }

            var attr = hit.GetType().GetCustomAttribute<FunctionAttribute>();
            var name = attr?.Name;
            return name;
        }

        private void LogException(string name, string message, Exception e)
        {
            var logger = _logFactory?.GetLogger(name) ?? LogManager.CreateNullLogger();
            logger.Warn(e, message);
        }

        private object CreateServiceInstance(Type type)
        {
            var result = type.CreateInstance();
            ConfigureDefaultService(result);
            return result;
        }

        private T CreateServiceInstance<T>(Type type)
        {
            var result = type.CreateInstance<T>();
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

        private void Init(IEnumerable<Assembly> assemblies)
        {
            var types = assemblies.SelectMany(a => a.GetTypes()
                .Where(t => t.GetCustomAttributes<FunctionAttribute>().Any()));

            types.ForEach(InitType);

            _listener.MessageEvent += async (api, message) =>
            {
                await _messageMonitorList.ForEachAsync(async m =>
                {
                    try
                    {
                        await m.OnMessageAsync(message, api);
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
            };
            _listener.MessageEvent += async (api, message) =>
            {
                IMessageCommand hit = default;
                try
                {
                    IMessageCommand last = null;
                    try
                    {
                        hit = _messageCommandList
                        .Select(c => CreateServiceInstance<IMessageCommand>(c.GetType()))
                        .FirstOrDefault(c => (last = c).ShouldResponse(message));
                    }
                    catch (Exception e)
                    {
                        //_logger.LogException(e);
                        LogException(last is null ? nameof(Hydrant) : GetServiceName(last), "ShouldResponse 方法引发了一个异常", e);
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
                    catch (Exception)
                    {
                        // ignore exception on handling
                    }
                }
                //catch (DatabaseFailException e)
                //{
                //    await api.SendMessageAsync(
                //        endpoint: message.Endpoint,
                //        message: e.Message ?? (e.InnerException is DbUpdateConcurrencyException ? "数据库太忙。" : "无法访问数据库。")
                //    );
                //    _logger.LogException(e);
                //}
                //catch (MySqlException)
                //{
                //    await api.SendMessageAsync(message.Endpoint, "无法访问 MySQL 数据库。");
                //}
                //catch (ApiAccessException)
                //{
                //    // 酷 Q 失败。
                //}
                //catch (Exception e)
                //{
                //    await api.SendMessageAsync(message.Endpoint, "有一些不好的事发生了。");
                //    _logger.LogException(e);
                //}
            };

            // 跑定期任务
            if (_regularTasks.Count > 0)
            {
                _plan.Start();
            }
        }

        internal void InitType(Type t)
        {
            var interfaces = t.GetInterfaces();
            var lazy = new Lazy<object>(
                valueFactory: () => CreateServiceInstance(t),
                mode: LazyThreadSafetyMode.None
            );
            Array.ForEach(interfaces, i => InitInterface(i, lazy));
        }

        internal void InitInterface(Type t, Lazy<object> lazy)
        {
            if (t == typeof(IInitializable))
            {
                var init = lazy.Value as IInitializable;

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
                InitTask(lazy.Value as IRegularAsync);
            }
        }

        private void InitTask(IRegularAsync task)
        {
            if (task.Every is TimeSpan every)
                _regularTasks.Add(new ScheduleInfo(ScheduleType.ByInterval, every, task));
            if (task.OnUtc is TimeSpan onUtc)
                _regularTasks.Add(new ScheduleInfo(ScheduleType.Daily, onUtc, task));
        }

        #region ExceptionEvent

        /// <summary>
        /// 在执行命令时被框架抓到了异常。
        /// </summary>
        public event Func<string, Exception, HttpApiClient, Sisters.WudiLib.Posts.Message, Task> ExceptionCaught_Command;

        #endregion
    }
}
