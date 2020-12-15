using SharedProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace E_Talker
{
    public class Server
    {
        private Socket socket;
        private List<Socket> userList;//用户组

        public Server()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            userList = new List<Socket>();
        }

        /// <summary>
        /// 准备连接
        /// </summary>
        public void Start(int port)
        {
            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                socket.Listen(10);
                LogHelper.System(string.Format("服务器已启动，正在监听端口：{0}", port));

                //开启接受连接,用多线程
                Thread accThread = new Thread(Accept);
                accThread.IsBackground = true;
                accThread.Start();
            }
            catch (Exception e)
            {
                LogHelper.SystemError(e.Message);
            }
        }
        /// <summary>
        /// 接受连接
        /// </summary>
        private void Accept()
        {
            Socket clientSocket = socket.Accept();
            userList.Add(clientSocket);
            LogHelper.System(string.Format("客户端已连接：{0}", IPToAddress(clientSocket)));

            //
            Thread RecvThread = new Thread(ReceMessage);
            RecvThread.IsBackground = true;
            RecvThread.Start(clientSocket);

            //递归
            Accept();
        }

        /// <summary>
        /// 接收客户端信息
        /// </summary>
        /// <param name="obj"></param>
        private void ReceMessage(object obj)
        {
            Socket client = obj as Socket;
            byte[] strByte = new byte[1024 * 1024];//设定接受字符的长度
            string str = "";
            try
            {
                //接受用户发送的内容
                int len = client.Receive(strByte);
                str = Encoding.Default.GetString(strByte, 0, len);
                //广播给用户
                Broadcast(str, client);
                LogHelper.Message(IPToAddress(client), str);
            }
            catch (Exception e)
            {
                LogHelper.SystemError(e.Message);
                LogHelper.System(string.Format("客户端已退出：{0}", IPToAddress(client)));
                userList.Remove(client);
                //退出时掐死线程，不然递归反弹
                Thread.CurrentThread.Abort();
            }
            //使用递归
            ReceMessage(client);
        }

        /// <summary>
        /// 广播信息
        /// </summary>
        /// <param name="userStr">传入收到的传输的内容</param>
        /// <param name="obj">传送信息的客户</param>
        private void Broadcast(string userStr, object obj)
        {
            //当前发送信息的客户
            Socket clientSend = obj as Socket;
            foreach (Socket client in userList)
            {
                //将信息广播给其他用户
                if (client != clientSend)
                {
                    string str = string.Format("【{0}】{1}", IPToAddress(clientSend), userStr);
                    client.Send(Encoding.Default.GetBytes(str));
                }
            }
        }

        //转换出连来客户的IP地址
        private string IPToAddress(Socket soket)
        {
            return (soket.RemoteEndPoint as IPEndPoint).Address.ToString();
        }
    }
}
