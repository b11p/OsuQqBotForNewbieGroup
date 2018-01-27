using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.LocalData
{
    class DictionaryHolder<TKey, TValue> : DataHolder<Dictionary<TKey, TValue>>
    {
        public DictionaryHolder(string path) :
            base(path, new Dictionary<TKey, TValue>())
        { }

        public TValue GetValueOrDefault(TKey key, TValue defaultValue) =>
            Read<TValue>(dictionary => dictionary.TryGetValue(key, out var value) ? value : defaultValue);

        public TValue GetValueOrDefault(TKey key) => GetValueOrDefault(key, default(TValue));

        public TValue Replace(TKey key, TValue newValue) =>
            Write<TValue>(dictionary =>
            {
                TValue oldValue;
                try
                {
                    oldValue = dictionary[key];
                    dictionary[key] = newValue;
                }
                catch (KeyNotFoundException)
                {
                    oldValue = default(TValue);
                    dictionary.Add(key, newValue);
                }
                return oldValue;
            });

        public bool Delete(TKey key) =>
            Write(dictionary => dictionary.Remove(key));
    }
}
