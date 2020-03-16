using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SharedProject
{
    /// <summary>
    /// 网络助手
    /// </summary>
    public class NetHelper
    {
        private const int INTERNET_CONNECTION_MODEM = 1;
        private const int INTERNET_CONNECTION_LAN = 2;
        private const int INTERNET_CONNECTION_PROXY = 4;
        private const int INTERNET_CONNECTION_MODEM_BUSY = 8;

        /// <summary>
        /// 获取联网状态
        /// </summary>
        /// <param name="connectionDescription"></param>
        /// <param name="reservedValue"></param>
        /// <returns></returns>
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);

        /// <summary>
        /// 检测用户计算机是否已连接网络
        /// </summary>
        /// <returns>是否已连接网络</returns>
        public static bool IsOnLine()
        {
            try
            {
                var netstatus = string.Empty;
                if (!InternetGetConnectedState(out int connection, 0)) return false;
                if ((connection & INTERNET_CONNECTION_PROXY) != 0)
                    netstatus += " 采用代理上网  \n";
                if ((connection & INTERNET_CONNECTION_MODEM) != 0)
                    netstatus += " 采用调治解调器上网 \n";
                if ((connection & INTERNET_CONNECTION_LAN) != 0)
                    netstatus += " 采用网卡上网  \n";
                if ((connection & INTERNET_CONNECTION_MODEM_BUSY) != 0)
                    netstatus += " MODEM被其他非INTERNET连接占用  \n";
                Console.WriteLine(netstatus);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
