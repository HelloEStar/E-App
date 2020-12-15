using SharedProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace E_Talker
{
    public class Client
    {
        private Socket socket;

        //初始化服务器
        public Client()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public bool Connected(string ip, int port)
        {
            try
            {
                socket.Connect(ip, port);
                LogHelper.System(string.Format("已连接到服务器：{0}:{1}", ip, port));
                // ClientSocket.Bind(new IPEndPoint());

                Thread RecvThread = new Thread(RecvMessage);
                RecvThread.IsBackground = true;
                RecvThread.Start();
                return true;
            }
            catch (Exception e)
            {
                LogHelper.SystemError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="str">消息内容</param>
        public void SendMessage(string str)
        {
            try
            {
                socket.Send(Encoding.Default.GetBytes(str));
            }
            catch (Exception e)
            {
                LogHelper.SystemError(e.Message);
            }
        }

        /// <summary>
        /// 接受信息
        /// </summary>
        private void RecvMessage()
        {
            try
            {
                byte[] strByte = new byte[500 * 1024];
                int len = socket.Receive(strByte);
                LogHelper.Message(Encoding.Default.GetString(strByte, 0, len));
            }
            //服务器关闭
            catch (Exception e)
            {
                LogHelper.SystemError(e.Message);
                Thread.CurrentThread.Abort();//关闭时切断进程
            }
            RecvMessage();
        }
    }
}
