using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OsuQqBot.Charts
{
    internal abstract class ChartInfoField
    {
        public abstract bool IsSet { get; }

        public abstract bool CanCancel { get; }

        public abstract bool Cancel(string arg);

        public abstract bool Update(string arg);

        public virtual bool TrySet(string arg) => IsSet ? false : Update(arg);

        public virtual bool TryCancel(string arg) => IsSet ? false : Cancel(arg);
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

    internal class StringField : ChartInfoField<string>
    {
        public override bool Update(string arg)
        {
            if (string.IsNullOrEmpty(arg)) return false;
            Value = arg;
            return true;
        }
    }

    internal class DoubleField : ChartInfoField<double>
    {
        public override bool Update(string arg) => ParseDouble(arg, value => Value = value);
    }

    internal class NullableDoubleField : ChartInfoField<double?>
    {
        public override bool Update(string arg) => ParseDouble(arg, value => Value = value);
    }

    internal class DateTimeField : ChartInfoField<DateTime>
    {
        public override bool Update(string arg) => ParseDateTime(arg, value => Value = value);
    }

    internal class NullableDateTimeField : ChartInfoField<DateTime?>
    {
        public override bool Update(string arg) => ParseDateTime(arg, value => Value = value);
    }

    internal class PerformanceRangeField : ChartInfoField
    {
        private readonly ChartInfoField<double?> _min;
        private readonly ChartInfoField<double?> _max;

        public PerformanceRangeField(ChartInfoField<double?> min, ChartInfoField<double?> max)
        {
            _min = min ?? throw new ArgumentNullException(nameof(min));
            _max = max ?? throw new ArgumentNullException(nameof(max));
        }

        public override bool IsSet => _min.IsSet || _max.IsSet;

        public override bool CanCancel => _min.CanCancel && _max.CanCancel;

        public override bool Cancel(string arg)
        {
            if (!CanCancel) return false;
            _min.Cancel(arg);
            _max.Cancel(arg);
            return true;
        }

        public override bool Update(string arg)
        {
            var match = Regex.Match(arg, @"^\s*(.*?)\s*-\s*(.*?)\s*$");
            if (!match.Success) return false;
            string lower = match.Groups[1].Value;
            string higher = match.Groups[2].Value;
            return UpdateOrCancel(lower, _min) && UpdateOrCancel(higher, _max);
        }

        private static bool UpdateOrCancel(string arg, ChartInfoField<double?> field)
        {
            return string.IsNullOrEmpty(arg) ? field.Cancel(string.Empty) : field.Update(arg);
        }
    }
}
