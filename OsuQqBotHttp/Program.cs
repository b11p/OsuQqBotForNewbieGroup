using Newtonsoft.Json;
using OsuQqBot.QqBot;
using System;
using System.IO;
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
            System.Net.WebSockets.WebSocket webSocket = new System.Net.WebSockets.ClientWebSocket();
            //new MyWebServer().StartListenAsync().Wait();
            //new MyWebServer().Listener();
            // this is test
            new PostProcessor(8876).Listen();
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

    #region 没用的东西
    class MyWebServer
    {

        //private TcpListener myListener;
        private Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        private int port = 8876;

        public MyWebServer()
        {
            //myListener = new TcpListener(System.Net.IPAddress.IPv6Any, port);
            //myListener.Start();
            /* var iPEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.IPv6Any, port);
             socket.Bind(iPEndPoint);
             Console.WriteLine(socket.LocalEndPoint);
             socket.Listen(1000);*/
            //Thread th = new Thread(new ThreadStart(startlisten));
            //th.Start();
        }

        public void Listener()
        {
            System.Net.HttpListener listener = new System.Net.HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:8876/");
            listener.Start();
            while (true)
            {
                var context = listener.GetContext();
                var inputStream = context.Request.InputStream;
                StreamReader sr = new StreamReader(inputStream);
                var message = sr.ReadToEnd();
                Console.WriteLine(message);
            }
        }

        public async Task StartListenAsync()
        {
            Byte[] bytes = new Byte[10240];
            string inputdata = null;
            string outputdata = null;
            int i;

            while (true)
            {

                Socket client = await socket.AcceptAsync();
                Console.WriteLine("Accept");
                //NetworkStream networkStream = new NetworkStream(client);
                //StreamReader sr = new StreamReader(networkStream);
                //StringBuilder sb = new StringBuilder();
                //string rec = sr.ReadLine();
                //Console.WriteLine(rec);
                //if (rec != "POST / HTTP/1.1") continue;
                //client.Shutdown(SocketShutdown.Both);
                //client.Close();
                //continue;
                inputdata = "";
                outputdata = "HTTP/1.1 200 OK\r\n\r\n";
                int receiveLength;

                byte[] msg = Encoding.UTF8.GetBytes(outputdata);

                await Task.Delay(1000);
                //下面是用socket类读取方式，结果inputdata中只能有信息头

                if ((i = client.Available) > 0)
                {
                    receiveLength = client.Receive(bytes, client.Available, SocketFlags.None);
                }
                else
                {
                    Console.WriteLine("???");
                    continue;
                }
                inputdata = Encoding.UTF8.GetString(bytes, 0, receiveLength);
                Console.WriteLine(inputdata);

                client.Shutdown(SocketShutdown.Both);
                client.Close();

            }
        }


    }
    #endregion

    /// <summary>
    /// 处理上报消息的类
    /// </summary>
    class PostProcessor
    {
        public PostProcessor(int port)
        {
            if (port <= 0 || port >= 65536) throw new ArgumentException(nameof(port));
            Port = port;
        }

        OsuQqBot.OsuQqBot osuBot = new OsuQqBot.OsuQqBot(new QqBot());

        public int Port { get; private set; }

        string ProcessPost(string json)
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
            return null;
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
            osuBot.ProcessPrivateMessage(privateMessage.user_id, privateMessage.message);
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

                    Task.Run(() =>
                    {
                        try
                        {
                            if (osuBot.UpdateUserBandingAsync(groupMessage.group_id, groupMessage.user_id, groupMessage.message).Result) return;
                            if (osuBot.WhirIsBest(groupMessage.group_id, groupMessage.user_id, groupMessage.message)) return;
                            osuBot.TestInGroupNameAsync(groupMessage.group_id, groupMessage.user_id, groupMessage.message).Wait();
                        }
                        catch (Exception e)
                        {
                            Logger.LogException(e);
                        }
                    });
                    break;
                case "anonymous":
                    break;
                case "notice":
                    break;
                default:
                    break;
            }
        }

        /*
        /// <summary>
        /// 这东西没用了
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static Post FromSocket(Socket socket)
        {
            System.Net.HttpListener listener = new System.Net.HttpListener();
            StringBuilder sb = new StringBuilder();
            using (socket)
            {
                Post post = null;
                using (NetworkStream networkStream = new NetworkStream(socket))
                {
                    using (StreamReader reader = new StreamReader(networkStream))
                    {
                        try
                        {
                            string line;
                            string[] head = {
                                "POST / HTTP/1.1",
                                "Connection: Keep-Alive",
                                "User-Agent: CoolQHttpApi/3.3.3",
                                "Content-Type: application/json; charset=UTF-8" };
                            foreach (var headLine in head)
                            {
                                line = reader.ReadLine();
                                sb.AppendLine(line);
                                if (line != headLine) goto end;
                            }
                            line = reader.ReadLine();
                            sb.AppendLine(line);
                            if (!line.StartsWith("Content-Length:")) goto end;
                            var sp = line.Split(':');
                            if (sp.Length != 2) goto end;
                            if (!int.TryParse(sp[1], out int length)) goto end;
                            line = reader.ReadLine();
                            sb.AppendLine(line);
                            line = reader.ReadLine();
                            sb.AppendLine(line);
                            if (line != string.Empty) goto end;
                            line = reader.ReadLine();
                            sb.AppendLine(line);
                            Console.WriteLine(line);
                            ProcessPost(line);
                        }
                        catch (Exception e)
                        {
                            Logger.LogException(e);
                            throw;
                        }
                    }
                }
                return post;

                end:
                Task.Delay(250).Wait();
                if (socket.Available > 0)
                {
                    Byte[] bytes = new Byte[10240];
                    int l = socket.Receive(bytes, socket.Available, SocketFlags.None);
                    string data = Encoding.UTF8.GetString(bytes, 0, l);
                    sb.AppendLine(data);
                }
                Logger.Log(sb.ToString());
                return null;
            }
        }
        */

        public void Listen()
        {
            try
            {
                using (System.Net.HttpListener listener = new System.Net.HttpListener())
                {
                    listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
                    listener.Start();
                    while (true)
                    {
                        var context = listener.GetContext();
                        using (var inputStream = context.Request.InputStream)
                        using (StreamReader sr = new StreamReader(inputStream))
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
