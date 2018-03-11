using Newtonsoft.Json;
using System.Collections.Generic;

namespace OsuQqBot.LocalData
{
    internal class Table<T>
    {
        [JsonProperty]
        private int Current { get; set; } = 1;
        [JsonProperty]
        private LinkedList<(int no, T item)> Items { get; set; }

        internal Table(string name, string description, long createBy)
        {
            Items = new LinkedList<(int, T)>();
            TableName = name;
            Description = description;
            Creator = createBy;

            Items = new LinkedList<(int no, T item)>();
            Administrators = new List<long>();
        }

        [JsonProperty]
        public string TableName { get; private set; }
        [JsonProperty]
        public string Description { get; private set; }
        [JsonIgnore]
        public IReadOnlyCollection<(int, T)> Item => Items;
        [JsonProperty]
        public long Creator { get; private set; }
        [JsonProperty]
        public ICollection<long> Administrators { get; private set; }

        public void Insert(T item)
        {
            Items.AddLast((Current, item));
            Current++;
        }
    }
}
