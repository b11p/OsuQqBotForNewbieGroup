﻿using System;
using Bleatingsheep.NewHydrant.Attributions;

namespace Bleatingsheep.NewHydrant.Core
{
#nullable enable
    internal class ScheduleInfo
    {
        private static readonly TimeSpan Close = new TimeSpan(0, 0, 1);

        public ScheduleType Type { get; }
        public TimeSpan Time { get; }
        public IRegularAsync Action { get; }
        public DateTime NextRun { get; set; }
        public TimeSpan WaitTime
        {
            get
            {
                var result = NextRun - DateTime.UtcNow;
                return result < TimeSpan.Zero ? TimeSpan.Zero : result;
            }
        }
        public ScheduleInfo(ScheduleType type, TimeSpan time, IRegularAsync action)
        {
            Type = type;
            Time = time;
            Action = action;
            switch (Type)
            {
                case ScheduleType.ByInterval:
                    NextRun = DateTime.UtcNow;
                    break;
                case ScheduleType.Daily:
                    NextRun = DateTime.UtcNow.Date + time;
                    if (ShouldRun())
                        NextRun = NextRun.AddDays(1);
                    break;
                default:
                    throw new ArgumentException("类型不对", nameof(type));
            }
        }

        public bool ShouldRun() => NextRun - DateTime.UtcNow < Close;

        public void Next()
        {
            switch (Type)
            {
                case ScheduleType.ByInterval:
                    NextRun += Time;
                    if (ShouldRun()) NextRun = DateTime.UtcNow;
                    break;
                case ScheduleType.Daily:
                    do
                    {
                        NextRun = NextRun.AddDays(1);
                    } while (ShouldRun());
                    break;
                default:
                    break;
            }
        }
    }

    internal enum ScheduleType
    {
        ByInterval = 0,
        Daily = 1,
    }
#nullable restore
}
