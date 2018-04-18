using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;

namespace qingjia_MVC
{
    /// <summary>
    /// 离线消息
    /// </summary>
    public class MessageInfo
    {
        public MessageInfo(DateTime _MsgTime, ArraySegment<byte> _MsgContent)
        {
            MsgTime = _MsgTime;
            MsgContent = _MsgContent;
        }
        public DateTime MsgTime { get; set; }
        public ArraySegment<byte> MsgContent { get; set; }
    }

    /// <summary>
    /// WSHandler 的摘要说明
    /// </summary>
    public class WSHandler : IHttpHandler
    {
        //用户连接数据
        private static Dictionary<string, WebSocket> CONNECT_POOL = new Dictionary<string, WebSocket>();
        //离线消息池
        private static Dictionary<string, List<MessageInfo>> MESSAGE_POOL = new Dictionary<string, List<MessageInfo>>();

        public void ProcessRequest(HttpContext context)
        {
            //context.Response.ContentType = "text/plain";
            //context.Response.Write("Hello World");
            if (context.IsWebSocketRequest)
            {
                context.AcceptWebSocketRequest(ProcessChat);
            }

            #region 模拟离线消息
            if (MESSAGE_POOL.Count == 0)
            {
                List<MessageInfo> messageList = new List<MessageInfo>();
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[2048]);
                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes("模拟消息1"));
                messageList.Add(new MessageInfo(DateTime.Now, buffer));
                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes("模拟消息2"));
                messageList.Add(new MessageInfo(DateTime.Now, buffer));
                MESSAGE_POOL.Add("123", messageList);
                System.Diagnostics.Debug.WriteLine("模拟离线消息！");
            }
            #endregion

            System.Diagnostics.Debug.WriteLine("后端websoceket创建完成！");
        }

        private async Task ProcessChat(AspNetWebSocketContext context)
        {
            WebSocket socket = context.WebSocket;
            string user = context.QueryString["user"].ToString();

            try
            {
                #region 用户添加连接池
                //第一次open时，添加到连接池中
                if (!CONNECT_POOL.ContainsKey(user))//不存在，添加当前用户websocket到连接池
                    CONNECT_POOL.Add(user, socket);
                else
                    if (socket != CONNECT_POOL[user])//当前对象不一致，更新
                    CONNECT_POOL[user] = socket;
                #endregion

                #region 离线消息处理
                if (MESSAGE_POOL.ContainsKey(user))
                {
                    List<MessageInfo> msgs = MESSAGE_POOL[user];
                    foreach (MessageInfo item in msgs)
                    {
                        await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(item.MsgTime + ":" + Encoding.UTF8.GetString
                            (item.MsgContent.Array))), WebSocketMessageType.Text, true, CancellationToken.None);
                        //此处存在格式转换问题  类->Json格式->byte[]->ArraySegment[]  需要解决以上记上格式切换问题
                    }
                    MESSAGE_POOL.Remove(user);//移除离线消息
                }
                #endregion

                string descUser = string.Empty;//目的用户
                while (true)
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[2048]);
                        WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None);

                        #region 消息处理（字符截取、消息转发）
                        try
                        {
                            #region 关闭Socket处理，删除连接池
                            if (socket.State != WebSocketState.Open)//连接关闭
                            {
                                if (CONNECT_POOL.ContainsKey(user)) CONNECT_POOL.Remove(user);//删除连接池
                                break;
                            }
                            #endregion

                            string userMsg = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);//发送过来的消息
                            string[] msgList = userMsg.Split('|');
                            if (msgList.Length == 2)
                            {
                                if (msgList[0].Trim().Length > 0)
                                    descUser = msgList[0].Trim();//记录消息目的用户
                                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msgList[1]));

                                if (CONNECT_POOL.ContainsKey(descUser))//判断客户端是否在线
                                {
                                    WebSocket destSocket = CONNECT_POOL[descUser];//目的客户端
                                    if (destSocket != null && destSocket.State == WebSocketState.Open)
                                        await destSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                                }
                                else
                                {
                                    await Task.Run(() =>
                                    {
                                        if (!MESSAGE_POOL.ContainsKey(descUser))//将用户添加至离线消息池中
                                            MESSAGE_POOL.Add(descUser, new List<MessageInfo>());
                                        MESSAGE_POOL[descUser].Add(new MessageInfo(DateTime.Now, buffer));//添加离线消息
                                    });
                                }
                            }
                            else
                            {
                                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(userMsg));

                                foreach (KeyValuePair<string, WebSocket> item in CONNECT_POOL)
                                {
                                    await item.Value.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            //消息转发异常处理，本次消息忽略 继续监听接下来的消息
                            System.Diagnostics.Debug.WriteLine("###################################");
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }
                        #endregion
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //整体异常处理
                if (CONNECT_POOL.ContainsKey(user)) CONNECT_POOL.Remove(user);
                System.Diagnostics.Debug.WriteLine("###################################");
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}