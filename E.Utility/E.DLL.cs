using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace E.Utility
{
    /// <summary>
    /// 应用信息
    /// </summary>
    public class AppInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// 组织
        /// </summary>
        public string Company { get; }
        /// <summary>
        /// 版权信息
        /// </summary>
        public string Copyright { get; }
        /// <summary>
        /// 用户协议
        /// </summary>
        public string UserAgreement { get; }
        /// <summary>
        /// 当前版本
        /// </summary>
        public Version Version { get; }
        /// <summary>
        /// 当前版本短写
        /// </summary>
        public string VersionShort { get { return Version.Major + "." + Version.Minor + "." + Version.Build; } }
        /// <summary>
        /// 更新日志
        /// </summary>
        public string UpdateNote { get; }

        /// <summary>
        /// 主页链接
        /// </summary>
        public string HomePage { get; }
        /// <summary>
        /// GitHub链接
        /// </summary>
        public string GitHubPage { get; }
        /// <summary>
        /// 官方Q群
        /// </summary>
        public string QQGroupLink { get; }
        /// <summary>
        /// 官方Q群
        /// </summary>
        public string QQGroupNumber { get; }
        /// <summary>
        /// 比特币地址
        /// </summary>
        public string BitCoinAddress { get; }

        public string ThemeFolder { get; } = "主题";

        public AppInfo(string name, string description, string company, string copyright, string userAgreement, Version version, string updateNote, 
                       string homePage, string gitHubPage, string qqGroupLink, string qqGroupNumber, string bitCoinAddress)
        {
            Name = name;
            Description = description;
            Company = company;
            Copyright = copyright;
            UserAgreement = userAgreement;
            Version = version;
            UpdateNote = updateNote;

            HomePage = homePage;
            GitHubPage = gitHubPage;
            QQGroupLink = qqGroupLink;
            QQGroupNumber = qqGroupNumber;
            BitCoinAddress = bitCoinAddress;
        }
    }

    /// <summary>
    /// ini文件操作类
    /// </summary>
    public class INIOperator
    {
        /// <summary>
        /// 写入或创建ini文件
        /// </summary>
        /// <param name="section">区域名</param>
        /// <param name="key">键名</param>
        /// <param name="value">值</param>
        /// <param name="iniPath">ini文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string iniPath);
        /// <summary>
        /// 获取ini文件内记录的设置
        /// </summary>
        /// <param name="lpAppName"></param>
        /// <param name="lpKeyName"></param>
        /// <param name="lpDefault"></param>
        /// <param name="lpReturnedString"></param>
        /// <param name="nSize"></param>
        /// <param name="lpFileName"></param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
        /// <summary>
        /// 获取ini文件内记录的区域
        /// </summary>
        /// <param name="lpAppName"></param>
        /// <param name="lpReturnedString"></param>
        /// <param name="nSize"></param>
        /// <param name="lpFileName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);
        /// <summary>
        /// 取值 
        /// </summary>
        /// <param name="section">区域名</param>
        /// <param name="key">键名</param>
        /// <param name="def"></param>
        /// <param name="filePath">ini文件路径</param>
        /// <returns></returns>
        private static string ReadString(string section, string key, string def, string filePath)
        {
            StringBuilder temp = new StringBuilder(1024);
            try
            {
                GetPrivateProfileString(section, key, def, temp, 1024, filePath);
            }
            catch
            { }
            return temp.ToString();
        }
        /// <summary>
        /// 获取ini值  
        /// </summary>
        /// <param name="section">区域名</param>
        /// <param name="keys">键值</param>
        /// <param name="filePath">ini文件路径</param>
        /// <returns>值</returns>
        public static string ReadIniKeys(string section, string keys, string filePath)
        {
            return ReadString(section, keys, "", filePath);
        }
        /// <summary>
        /// 根据section取所有key  
        /// </summary>
        /// <param name="section">区域名</param>
        /// <param name="filePath">ini文件路径</param>
        /// <returns>值集合</returns>
        public static string[] ReadIniAllKeys(string section, string filePath)
        {
            UInt32 MAX_BUFFER = 32767;
            string[] items = new string[0];
            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));
            UInt32 bytesReturned = GetPrivateProfileSection(section, pReturnedString, MAX_BUFFER, filePath);
            if (!(bytesReturned == MAX_BUFFER - 2) || (bytesReturned == 0))
            {
                string returnedString = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned);

                items = returnedString.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }
            Marshal.FreeCoTaskMem(pReturnedString);
            return items;
        }
        /// <summary>
        /// 写入ini文件
        /// </summary>
        /// <param name="section">区域名</param>
        /// <param name="keys">键值</param>
        /// <param name="value">值</param>
        /// <param name="filePath">ini文件路径</param>
        public static void WriteIniKeys(string section, string key, string value, string filePath)
        {
            WritePrivateProfileString(section, key, value, filePath);
        }
    }

    /// <summary>
    /// 网络帮助器
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

    /// <summary>
    /// 自定义ComboBox选项
    /// </summary>
    public struct ItemInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public ItemInfo(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    /// <summary>
    /// 文件或文件夹信息
    /// </summary>
    public class FileOrFolderInfo
    {
        public string Name { get; private set; }
        public string Path { get; private set; }

        public FileOrFolderInfo(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
            //Name = System.IO.Path.GetDirectoryName(path);
        }
    }

    /// <summary>
    /// 菜单工具栏
    /// </summary>
    public enum MenuTool
    {
        无,
        文件,
        编辑,
        设置,
        关于
    }
}