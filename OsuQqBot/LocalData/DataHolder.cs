using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Newtonsoft.Json.JsonConvert;

namespace OsuQqBot.LocalData
{
    class DataHolder<T> where T : class, new()
    {
        readonly T data;

        readonly string path;

        private static T Init(string path)
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
            return DeserializeObject<T>(data) ?? new T();
        }

        public DataHolder(string path)
        {
            this.path = path;
            data = Init(path);
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
                File.WriteAllText(path, SerializeObject(data, Newtonsoft.Json.Formatting.Indented));
                return result;
            }
            finally { readerWriterLock.ReleaseWriterLock(); }
        }
    }
}
