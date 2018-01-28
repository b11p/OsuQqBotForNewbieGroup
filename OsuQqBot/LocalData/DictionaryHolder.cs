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

        /// <summary>
        /// 如果key存在，就替换，否则就插入
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        /// <returns>旧值。如果不存在，则为默认。</returns>
        public TValue ReplaceOrInsert(TKey key, TValue newValue) =>
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

        /// <summary>
        /// 替换（或者插入），如果newValue为null，则删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        /// <returns>是否替换（或者插入）：true为替换，false为删除</returns>
        public bool ReplaceOrDelete(TKey key, TValue newValue) =>
            Write<bool>(dictionary =>
            {
                if (newValue == null)
                {
                    this.Delete(key);
                    return false;
                }
                else
                {
                    ReplaceOrInsert(key, newValue);
                    return true;
                }
            });

        public bool Delete(TKey key) =>
            Write(dictionary => dictionary.Remove(key));
    }
}
