using Newtonsoft.Json;
using System.Collections.Generic;

namespace OsuQqBot.LocalData
{
    /// <summary>
    /// 提供带编号的集合。不提供线程安全性。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class NumberedCollection<T> where T : class
    {
        [JsonProperty]
        protected int Current { get; set; } = 1;
        [JsonProperty]
        protected SortedDictionary<int, T> Items { get; set; } = new SortedDictionary<int, T>();

        [JsonIgnore]
        public IReadOnlyDictionary<int, T> Item => Items;

        public int Insert(T item)
        {
            //Items.AddLast((Current, item));
            bool success = false;
            int no = Current;
            while (!success)
            {
                no = Current;
                success = Items.TryAdd(Current, item);
                Current++;
            }
            return no;
        }
    }
}
