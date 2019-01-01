using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Addon;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Logging;
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuMixedApi.MotherShip;
using Bleatingsheep.OsuQqBot.Database.Execution;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Core
{
    public sealed class Hydrant
    {
        private readonly HttpApiClient _qq;
        private readonly ApiPostListener _listener;
        private readonly ExecutingInfo _executingInfo;
        private readonly ILogger _logger;
        private readonly Assembly[] _assemblies;
        private int _isInitialized = 0;

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

            _executingInfo = new ExecutingInfo
            {
            };

            // 配置日志
            _logger = FileLogger.Default;
            listener.OnException += _logger.LogException;

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
                            await info.Action.RunAsync(_qq, _executingInfo);
                        }
                        catch (Exception e)
                        {
                            _logger.LogException(e);
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
                        await m.OnMessageAsync(message, api, _executingInfo);
                    }
                    catch (ExecutingException)
                    {
                        // ignored
                    }
                    catch (Exception e)
                    {
                        _logger.LogException(e);
                    }
                });
            };
            _listener.MessageEvent += async (api, message) =>
            {
                IMessageCommand hit = default;
                try
                {
                    try
                    {
                        hit = _messageCommandList
                        .Select(c => c.GetType().CreateInstance<IMessageCommand>())
                        .FirstOrDefault(c => c.ShouldResponse(message));
                    }
                    catch (Exception e)
                    {
                        _logger.LogException(e);
                        return;
                    }
                    var task = hit?.ProcessAsync(message, api, _executingInfo);
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
                        _logger.LogException(e.InnerException);
                    }
                }
                catch (Exception e) when (_isInitialized == 0) // 暂时不会执行。
                {
                    try
                    {
                        var attr = hit.GetType().GetCustomAttribute<FunctionAttribute>();
                        OnCommandException?.Invoke(attr.Name, e, api, message);
                    }
                    catch (Exception)
                    {
                        // ignore exception on handling
                    }
                }
                catch (DatabaseFailException e)
                {
                    await api.SendMessageAsync(
                        endpoint: message.Endpoint,
                        message: e.Message ?? (e.InnerException is DbUpdateConcurrencyException ? "数据库太忙。" : "无法访问数据库。")
                    );
                    _logger.LogException(e);
                }
                catch (MySqlException)
                {
                    await api.SendMessageAsync(message.Endpoint, "无法访问 MySQL 数据库。");
                }
                catch (ApiAccessException)
                {
                    // 酷 Q 失败。
                }
                catch (Exception e)
                {
                    await api.SendMessageAsync(message.Endpoint, "有一些不好的事发生了。");
                    _logger.LogException(e);
                }
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
                valueFactory: () => Assembly.GetAssembly(t).CreateInstance(t.FullName),
                mode: LazyThreadSafetyMode.None
            );
            Array.ForEach(interfaces, i => InitInterface(i, lazy));
        }

        internal void InitInterface(Type t, Lazy<object> lazy)
        {
            if (t == typeof(IInitializable))
            {
                var init = lazy.Value as IInitializable;

                var success = init.InitializeAsync(_executingInfo).Result;
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

        public event Action<string, Exception, HttpApiClient, Sisters.WudiLib.Posts.Message> OnCommandException;

        #endregion
    }
}
