using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OsuQqBot
{
    static class Logger
    {
        static readonly string FilePath;// = @"C:\Users\yinmi\Desktop\新建文本文档.txt";

        static readonly object thisLock = new object();

        static Logger()
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Directory.CreateDirectory(Path.Combine(desktop, "Bot Log"));
            FilePath = Path.Combine(desktop, "Bot Log", "Bot Log.txt");
            Log("Logger OK!");
        }

        public static void Log(string s)
        {
            lock (thisLock)
                //File.AppendAllLines(FilePath, new string[] { s });
                File.AppendAllText(FilePath, s + Environment.NewLine);
        }

        public static string LogException(Exception e, bool inner = false)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(DateTime.Now.ToString());
            if (!inner && e is AggregateException) stringBuilder.AppendLine("------Start Aggregate------");
            stringBuilder.AppendLine(e.GetType().ToString());
            stringBuilder.AppendLine(e.Message);
            stringBuilder.AppendLine(e.Source);
            stringBuilder.AppendLine(e.StackTrace);
            stringBuilder.AppendLine();
            if (e is AggregateException ae)
            {
                foreach (var exception in ae.InnerExceptions)
                {
                    stringBuilder.AppendLine(LogException(exception, true));
                }
            }

            if (!inner)
            {
                lock (thisLock)
                    File.AppendAllText(FilePath, stringBuilder.ToString());
                return null;
            }
            else return stringBuilder.ToString();
        }
    }
}
