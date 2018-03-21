using Newtonsoft.Json;
using System.Collections.Generic;

namespace OsuQqBot.LocalData
{
    internal class Table<TItem, TInfo> : NumberedCollection<TItem> where TItem : class
    {
        internal Table(string name, string description, long createBy)
        {
            TableName = name;
            Description = description;
            Creator = createBy;
        }

        [JsonProperty]
        public string TableName { get; private set; }
        [JsonProperty]
        public string Description { get; private set; }
        [JsonProperty]
        public TInfo Extra { get; set; }
        [JsonProperty]
        public long Creator { get; private set; }
        [JsonProperty]
        public ICollection<long> Administrators { get; private set; } = new List<long>();
    }
}
