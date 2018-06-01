using System;
using System.Globalization;

namespace OsuQqBot.Charts
{
    internal abstract class ChartInfoField
    {
        public abstract bool IsSet { get; }

        public abstract bool CanCancel { get; }

        public abstract bool Cancel(string arg);

        public abstract bool Update(string arg);
    }

    /// <summary>
    /// 表示处理 Chart 信息时的一个字段。
    /// </summary>
    internal abstract class ChartInfoField<T> : ChartInfoField
    {
        private T _value;

        private bool _isSet;

        public sealed override bool IsSet => _isSet;

        public override bool CanCancel => default(T) == null;

        public T Value
        {
            get => IsSet ? _value : default(T);
            protected set
            {
                _value = value;
                _isSet = true;
            }
        }

        public override bool Cancel(string arg)
        {
            if (!CanCancel || !string.IsNullOrWhiteSpace(arg)) return false;
            Value = default(T);
            return true;
        }

        public override abstract bool Update(string arg);

        protected static bool ParseDouble(string arg, Action<double> set)
        {
            if (double.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
            {
                set(result);
                return true;
            }
            return false;
        }

        protected static bool ParseDateTime(string arg, Action<DateTime> set)
        {
            if (DateTime.TryParse(arg, CultureInfo.GetCultureInfo("zh-CN"), DateTimeStyles.None, out DateTime result))
            {
                set(result);
                return true;
            }
            return false;
        }
    }

    internal class StringInfo : ChartInfoField<string>
    {
        public override bool Update(string arg)
        {
            if (string.IsNullOrEmpty(arg)) return false;
            Value = arg;
            return true;
        }
    }

    internal class DoubleInfo : ChartInfoField<double>
    {
        public override bool Update(string arg) => ParseDouble(arg, value => Value = value);
    }

    internal class NullableDoubleInfo : ChartInfoField<double?>
    {
        public override bool Update(string arg) => ParseDouble(arg, value => Value = value);
    }

    internal class DateTimeInfo : ChartInfoField<DateTime>
    {
        public override bool Update(string arg) => ParseDateTime(arg, value => Value = value);
    }

    internal class NullableDateTimeInfo : ChartInfoField<DateTime?>
    {
        public override bool Update(string arg) => ParseDateTime(arg, value => Value = value);
    }

    internal class RangeInfo<T> : ChartInfoField where T : IComparable
    {
        private readonly ChartInfoField<T> _min;
        private readonly ChartInfoField<T> _max;

        public RangeInfo(ChartInfoField<T> min, ChartInfoField<T> max)
        {
            _min = min;
            _max = max;
        }

        public override bool IsSet => _min.IsSet || _max.IsSet;

        public override bool CanCancel => _min.CanCancel && _max.CanCancel;

        public override bool Cancel(string arg)
        {
            throw new NotImplementedException();
        }

        public override bool Update(string arg) => throw new NotImplementedException();
    }
}
