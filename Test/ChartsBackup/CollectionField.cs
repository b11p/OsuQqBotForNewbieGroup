using System.Collections.Generic;

namespace OsuQqBot.Charts
{
    internal abstract class CollectionField<T> : ChartInfoField
    {
        protected readonly ICollection<T> _adds = new List<T>();
        protected readonly ICollection<T> _removes = new List<T>();
        protected readonly ICollection<T> _sets = new List<T>();

        public override abstract bool IsSet { get; }

        public override abstract bool CanCancel { get; }

        public override abstract bool Cancel(string arg);
        public override abstract bool Update(string arg);
    }
}
