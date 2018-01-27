using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Newtonsoft.Json.JsonConvert;

namespace OsuQqBot.LocalData
{
    class DataHolder<T> where T : class
    {
        readonly T data;

        readonly string path;

        private static T Init(string path, T instance)
        {
            string data = "";
            try
            {
                data = File.ReadAllText(path);
            }
            catch (FileNotFoundException)
            {
                data = "";
            }
            return DeserializeObject<T>(data) ?? instance ?? throw new ArgumentNullException(nameof(instance));
        }

        /// <summary>
        /// 从文件构造DataHolder类的新实例
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="instance">如果反序列化失败要使用的默认值</param>
        public DataHolder(string path, T instance)
        {
            this.path = path;
            if (!string.IsNullOrEmpty(path))
                data = Init(path, instance);
            else
            {
                path = null;
                data = instance ?? throw new ArgumentNullException(nameof(instance));
            }
        }

        readonly System.Threading.ReaderWriterLock readerWriterLock = new System.Threading.ReaderWriterLock();

        public TResult Read<TResult>(Func<T, TResult> readMethod)
        {
            readerWriterLock.AcquireReaderLock(int.MaxValue);
            try
            {
                return readMethod(data);
            }
            finally { readerWriterLock.ReleaseReaderLock(); }
        }

        public void Write(Action<T> writeMethod)
        {
            readerWriterLock.AcquireWriterLock(int.MaxValue);
            try
            {
                writeMethod(data);
                if (path != null)
                    File.WriteAllText(path, SerializeObject(data, Newtonsoft.Json.Formatting.Indented));
            }
            finally { readerWriterLock.ReleaseWriterLock(); }
        }

        public TResult Write<TResult>(Func<T, TResult> writeMethod)
        {
            readerWriterLock.AcquireWriterLock(int.MaxValue);
            try
            {
                var result = writeMethod(data);
                if (path != null)
                    File.WriteAllText(path, SerializeObject(data, Newtonsoft.Json.Formatting.Indented));
                return result;
            }
            finally { readerWriterLock.ReleaseWriterLock(); }
        }
    }
}
