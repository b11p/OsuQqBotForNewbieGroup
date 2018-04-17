using Newtonsoft.Json;
using OsuQqBot.QqBot;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OsuQqBotHttp
{
    class Program
    {
        static void Main(string[] args)
        {

            new PostProcessor(port: 8877, wudiPort: 8876).Listen();
            using (HttpClient client = new HttpClient())
            {

                client.GetStringAsync("http://127.0.0.1:5700/send_private_msg?user_id=962549599&message=hello%20HTTP/1.1").Wait();
                string json = JsonConvert.SerializeObject(new
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
            Console.Read();
        }
    }

    /// <summary>
    /// 处理上报消息的类
    /// </summary>
    class PostProcessor
    {
        public PostProcessor(int port, int wudiPort)
        {
            if (port <= 0 || port >= 65536) throw new ArgumentException(nameof(port));
            Port = port;

            apiClient = new Sisters.WudiLib.HttpApiClient();
            apiClient.ApiAddress = "http://127.0.0.1:5700";

            _listener = new Sisters.WudiLib.Posts.ApiPostListener();
            _listener.ApiClient = apiClient;
            _listener.PostAddress = $"http://127.0.0.1:{wudiPort}/";
            _listener.ForwardTo = $"http://127.0.0.1:{port}/";
            _listener.StartListen();

            osuBot = new OsuQqBot.OsuQqBot(_qq, apiClient, _listener);
        }

        QqBot _qq = new QqBot();

        Sisters.WudiLib.HttpApiClient apiClient;

        Sisters.WudiLib.Posts.ApiPostListener _listener;

        OsuQqBot.OsuQqBot osuBot;

        public int Port { get; private set; }

        void ProcessPost(string json)
        {
            Task.Run(() =>
            {
                try
                {
                    var p = JsonConvert.DeserializeObject<Post>(json);
                    switch (p.post_type)
                    {
                        case "message":
                            ProcessMessage(json);
                            break;
                        case "event":
                            ProcessEvent(json);
                            break;
                        case "request":
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            });
        }

        Message ProcessMessage(string json)
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
            return null;
        }

        void ProcessPrivateMessage(string json)
        {
            var privateMessage = JsonConvert.DeserializeObject<PrivateMessage>(json);
            osuBot.ProcessMessage(
                new PrivateEndPoint { EndPointType = EndPointType.Private, UserId = privateMessage.user_id },
                new MessageSource { FromQq = privateMessage.user_id },
                privateMessage.message
                );
            //osuBot.ProcessPrivateMessage(privateMessage.user_id, privateMessage.message);
        }

        void ProcessGroupMessage(string json)
        {
            var groupMessage = JsonConvert.DeserializeObject<GroupMessage>(json);
            switch (groupMessage.sub_type)
            {
                case "normal":
                    osuBot.ProcessMessage(
                        new GroupEndPoint { EndPointType = EndPointType.Group, GroupId = groupMessage.group_id },
                        new MessageSource { FromQq = groupMessage.user_id },
                        groupMessage.message
                        );

                    //Task.Run(() =>
                    //{
                    //    try
                    //    {
                    //        if (osuBot.UpdateUserBandingAsync(groupMessage.group_id, groupMessage.user_id, groupMessage.message).Result) return;
                    //        if (osuBot.WhirIsBest(groupMessage.group_id, groupMessage.user_id, groupMessage.message)) return;
                    //        osuBot.TestInGroupNameAsync(groupMessage.group_id, groupMessage.user_id, groupMessage.message).Wait();
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        Logger.LogException(e);
                    //    }
                    //});
                    break;
                case "anonymous":
                    break;
                case "notice":

                    break;
                default:
                    break;
            }
        }

        void ProcessEvent(string json)
        {
            var e = JsonConvert.DeserializeObject<Event>(json);
            switch (e.EventType)
            {
                case "group_admin":
                    _qq.GroupAdminChanging(JsonConvert.DeserializeObject<GroupAdminChanged>(json));
                    break;
                case "group_increase":
                    _qq.GroupMemberIncreased(JsonConvert.DeserializeObject<SomeoneComesToGroup>(json));
                    break;
                default:
                    break;
            }
        }

        public void Listen()
        {
            try
            {
                using (var listener = new HttpListener())
                {
                    listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
                    listener.Start();
                    while (true)
                    {
                        var context = listener.GetContext();
                        //if (!IPAddress.IsLoopback(context.Request.RemoteEndPoint.Address)) continue;
                        var sw = Stopwatch.StartNew();
                        using (var inputStream = context.Request.InputStream)
                        using (var sr = new StreamReader(inputStream))
                        {
                            string message;
                            try
                            {
                                message = sr.ReadToEnd();
                            }
                            catch (HttpListenerException e)
                            {
                                Logger.LogException(e);
                                continue;
                            }
                            Console.WriteLine(message);
                            ProcessPost(message);
                            using (context.Response) { }
                        }
                        Console.WriteLine(sw.Elapsed);
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
