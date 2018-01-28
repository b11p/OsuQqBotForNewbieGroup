using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using OsuQqBot.QqBot;

namespace OsuQqBotHttp
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // this is test
            new PostProcessor(8876).Listen();
#if DEBUG
            using (var client = new HttpClient())
            {

                client.GetStringAsync("http://127.0.0.1:5700/send_private_msg?user_id=962549599&message=hello%20HTTP/1.1").Wait();
                
                var json = JsonConvert.SerializeObject(new
                {
                    user_id = 962549599,
                    message = "hello"
                });
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var t = client.PostAsync("http://127.0.0.1:5700/send_private_msg/", content);
                t.Wait();
                
                var r = t.Result;
                Console.WriteLine(r.Content.ReadAsStringAsync().Result);

                json = JsonConvert.SerializeObject(new
                {
                    group_id = 614892339,
                    user_id = 2930081217
                });
                
                content = new StringContent(json, Encoding.UTF8, "application/json");
                
                t = client.PostAsync("http://127.0.0.1:5700/get_group_member_info", content);
                t.Wait();
                
                r = t.Result;
                Console.WriteLine(r.Content.ReadAsStringAsync().Result);
            }
#endif
            Console.Read();
        }
    }

    /// <summary>
    /// 处理上报消息的类
    /// </summary>
    internal class PostProcessor
    {
        public PostProcessor(int port)
        {
            if (port <= 0 || port >= 65536) throw new ArgumentException(nameof(port));
            Port = port;
        }

        private readonly OsuQqBot.OsuQqBot _osuBot = new OsuQqBot.OsuQqBot(new QqBot());

        private int Port { get; set; }

        private void ProcessPost(string json)
        {
            var p = JsonConvert.DeserializeObject<Post>(json);
            switch (p.post_type)
            {
                case "message":
                    ProcessMessage(json);
                    break;
                case "event":
                    break;
                case "request":
                    break;
                default:
                    break;
            }
        }

        private void ProcessMessage(string json)
        {
            var m = JsonConvert.DeserializeObject<Message>(json);
            switch (m.message_type)
            {
                case "private":
                    ProcessPrivateMessage(json);
                    break;
                case "group":
                    ProcessGroupMessage(json);
                    break;
                case "discuss":
                    break;
                default:
                    break;
            }
        }

        private void ProcessPrivateMessage(string json)
        {
            var privateMessage = JsonConvert.DeserializeObject<PrivateMessage>(json);
            _osuBot.ProcessMessage(
                new PrivateEndPoint { EndPointType = EndPointType.Private, UserId = privateMessage.user_id },
                new MessageSource { FromQq = privateMessage.user_id },
                privateMessage.message
                );
            //osuBot.ProcessPrivateMessage(privateMessage.user_id, privateMessage.message);
        }

        private void ProcessGroupMessage(string json)
        {
            var groupMessage = JsonConvert.DeserializeObject<GroupMessage>(json);
            switch (groupMessage.sub_type)
            {
                case "normal":
                    _osuBot.ProcessMessage(
                        new GroupEndPoint { EndPointType = EndPointType.Group, GroupId = groupMessage.group_id },
                        new MessageSource { FromQq = groupMessage.user_id },
                        groupMessage.message
                        );
                    break;
                case "anonymous":
                    break;
                case "notice":
                    break;
                default:
                    break;
            }
        }


        public void Listen()
        {
            try
            {
                using (var listener = new System.Net.HttpListener())
                {
                    listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
                    listener.Start();
                    while (true)
                    {
                        var context = listener.GetContext();
                        using (var inputStream = context.Request.InputStream)
                        using (var sr = new StreamReader(inputStream))
                        {
                            var message = sr.ReadToEnd();
                            Console.WriteLine(message);
                            ProcessPost(message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                throw;
            }
        }

    }
}
