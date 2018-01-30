using System;
using System.Collections.Generic;
using System.Text;
using OsuQqBot.QqBot;

namespace OsuQqBot.StatelessFunctions
{
    class ManageTips : IStatelessFunction
    {
        public bool ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message)
        {
            if (!HasRight(messageSource.FromQq)) return false;
            if (!(endPoint is PrivateEndPoint)) return false;

            string[] commonds = message.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (commonds.Length == 0 || commonds[0].ToLowerInvariant() != "tips") return false;

            DoCommond(endPoint, commonds);
            return true;
        }

        private static readonly IReadOnlyList<string> _defaultTips = new List<string>
        {
            //"你不合适屙屎——interBot",
            //"求求你别糊图了——interBot",
            //"再见了——interBot",
            //"pp刷子——interBot",
            //"打图经验充足，不飞升没理由——interBot",
            "再@我，我叫咩咩打你——我说的",
            //"whir爷爷——大家如是说",
            //"标准的正常玩家——interBot",
            //"你们还是去床上解决吧——interBot",
            //"相信你一定可以克服瓶颈",
            //At(1677323371)+"你快回来，生命因你而精彩",
            //At(1677323371)+"你快回来，把我的思念带回来",
            //"别让我的心空如大海——《你快回来》",
            //"新人赛火热进行中（化学式承办）",
            "广告位招租",
            "中国的whir，我被他打爆！",
            "早上好",
            "求求你别复读了",
            "ญ็้็้็้็้็้็้็้็้็้็้็้ŭ..",
            "。",
            "jump有用？",
        };

        public static IList<string> DefaultTips => new List<string>(_defaultTips);

        private void DoCommond(EndPoint endPoint, string[] commonds)
        {
            if (commonds.Length < 2) return;

            switch (commonds[1].ToLowerInvariant())
            {
                case "list":
                    ListTips(endPoint);
                    break;
                case "add":
                    if (commonds.Length < 3) return;
                    AddTip(endPoint, commonds[2]);
                    break;
                case "delete":
                    if (commonds.Length < 3) return;
                    DeleteTip(endPoint, commonds[2]);
                    break;
                default:
                    break;
            }
        }

        private void ListTips(EndPoint endPoint)
        {
            string[] tips = LocalData.Database.Instance.ListTips();
            string result = string.Join(Environment.NewLine, tips);
            OsuQqBot.QqApi.SendMessageAsync(endPoint, result);
        }

        private void AddTip(EndPoint endPoint, string newTip) =>
            OsuQqBot.QqApi.SendMessageAsync(
                endPoint,
                LocalData.Database.Instance.AddTip(newTip) ? "添加成功" : "添加失败，已有相同Tip",
                true
            );

        private void DeleteTip(EndPoint endPoint, string newTip) =>
            OsuQqBot.QqApi.SendMessageAsync(
                endPoint,
                LocalData.Database.Instance.DeleteTip(newTip) ? "删除成功" : "删除失败，没有此Tip",
                true
                );

        private bool HasRight(long qq) => LocalData.Database.Instance.IsAdministrator(qq);
    }
}
