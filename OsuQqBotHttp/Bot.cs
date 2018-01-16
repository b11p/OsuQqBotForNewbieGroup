using Newtonsoft.Json;
using OsuQqBot.QqBot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Linq;

namespace OsuQqBotHttp
{
    class QqBot : IQqBot
    {
        static readonly string server = "http://127.0.0.1:5700";
        static readonly string privatePath = "/send_private_msg_async";
        static readonly string groupPath = "/send_group_msg_async";
        static readonly string groupMemberInfoPath = "/get_group_member_info";
        static readonly string groupMemberListPath = "/get_group_member_list";
        static readonly string loginInfoPath = "/get_login_info";

        static string PrivateUrl => server + privatePath;
        static string GroupUrl => server + groupPath;
        static string GroupMemberInfoUrl => server + groupMemberInfoPath;
        static string GroupMemberListUrl => server + groupMemberListPath;
        static string LoginInfoUrl => server + loginInfoPath;

        public OsuQqBot.QqBot.GroupMemberInfo GetGroupMemberInfo(long group, long qq)
        {
            string json = JsonConvert.SerializeObject(new
            {
                group_id = group,
                user_id = qq,
                no_cache = true
            });
            var resultStr = Post(GroupMemberInfoUrl, json).Result;
            if (resultStr == null) return null;
            var response = JsonConvert.DeserializeObject<GroupMemberInfoResponse>(resultStr);
            return response.data;
        }

        public IEnumerable<OsuQqBot.QqBot.GroupMemberInfo> GetGroupMemberList(long group)
        {
            string json = JsonConvert.SerializeObject(new
            {
                group_id = group
            });
            string resultStr = Post(GroupMemberListUrl, json).Result;
            if (resultStr == null) return null;
            var response = JsonConvert.DeserializeObject<GroupMemberListResponse>(resultStr);
            if (response == null) return null;
            var result = new LinkedList<OsuQqBot.QqBot.GroupMemberInfo>();
            foreach (var info in response.data)
            {
                result.AddLast(info);
            }
            return result;
        }
        
        public string GetLoginName() => throw new NotImplementedException();
        public long GetLoginQq()
        {
            string json = JsonConvert.SerializeObject(new { });
            var resultStr = Post(LoginInfoUrl, json).Result;
            dynamic response = JsonConvert.DeserializeObject(resultStr);
            return response.data.user_id;
        }

        public async void SendGroupMessageAsync(long group_id, string message)
        {
            string json = JsonConvert.SerializeObject(new
            {
                group_id = group_id,
                message = message
            });
            await Post(GroupUrl, json);
        }

        public async void SendGroupMessageAsync(long group_id, string message, bool auto_escape)
        {
            string json = JsonConvert.SerializeObject(new
            {
                group_id = group_id,
                message = message,
                auto_escape = auto_escape
            });
            await Post(GroupUrl, json);
        }

        public async void SendPrivateMessageAsync(long user_id, string message)
        {
            string json = JsonConvert.SerializeObject(new
            {
                user_id = user_id,
                message = message
            });
            await Post(PrivateUrl, json);
        }

        public async void SendPrivateMessageAsync(long user_id, string message, bool auto_escape)
        {
            string json = JsonConvert.SerializeObject(new
            {
                user_id = user_id,
                message = message,
                auto_escape = auto_escape
            });
            await Post(PrivateUrl, json);
        }

        private static async System.Threading.Tasks.Task<string> Post(string url, string json)
        {
            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                string message;
                try
                {
                    var t = await client.PostAsync(url, content);
                    //if (t.StatusCode != 0)
                    //{
                    //    StringBuilder sb = new StringBuilder();
                    //    sb.AppendLine(url);
                    //    sb.AppendLine(json);
                    //    if ((int)t.StatusCode == 1)
                    //    {
                    //        sb.AppendLine("Async!");
                    //        sb.AppendLine();
                    //    }
                    //    else
                    //    {
                    //        sb.AppendFormat("StatusCode is {0}", t.StatusCode);
                    //        sb.AppendLine();
                    //        sb.AppendLine();
                    //    }
                    //    Logger.Log(sb.ToString());
                    //}
                    message = await t.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException e)
                {
                    Logger.Log(json);
                    Logger.LogException(e);
                    message = null;
                }
                return message;
            }
        }

        public void SendMessageAsync(EndPoint endPoint, string message)
        {
            if(endPoint is PrivateEndPoint privateEP)
            {
                SendPrivateMessageAsync(privateEP.UserId, message);
            }
            else if(endPoint is GroupEndPoint groupEP)
            {
                SendGroupMessageAsync(groupEP.GroupId, message);
            }
            else if(endPoint is DiscussEndPoint discussEP)
            {

            }
        }

        public void SendMessageAsync(EndPoint endPoint, string message, bool isPlainText)
        {
            if (endPoint is PrivateEndPoint privateEP)
            {
                SendPrivateMessageAsync(privateEP.UserId, message, isPlainText);
            }
            else if (endPoint is GroupEndPoint groupEP)
            {
                SendGroupMessageAsync(groupEP.GroupId, message, isPlainText);
            }
            else if (endPoint is DiscussEndPoint discussEP)
            {

            }
        }

        public string BeforeSend(string message) => message.Replace("&", "&amp;").Replace("[", "&#91;").Replace("]", "&#93;");
    }
}
