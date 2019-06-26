using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace E.Utility
{
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
        /// 当前版本
        /// </summary>
        public Version Version { get; }
        /// <summary>
        /// 更新日志
        /// </summary>
        public string UpdateNote { get; }

        /// <summary>
        /// 主页链接
        /// </summary>
        public string HomePage { get; }
        /// <summary>
        /// 信息链接
        /// </summary>
        public string InfoPage { get; }
        /// <summary>
        /// 下载链接
        /// </summary>
        public string DownloadPage { get; }
        /// <summary>
        /// 反馈链接
        /// </summary>
        public string GitHubPage { get; }
        /// <summary>
        /// 官方Q群
        /// </summary>
        public string QQGroup { get; }
        /// <summary>
        /// 比特币地址
        /// </summary>
        public string BitCoinAddress { get; }

        public AppInfo(string name, string description, string company, string copyright, Version version, string updateNote, 
                       string homePage, string infoPage, string downloadPage, string gitHubPage, string qqGroup,string bitCoinAddress)
        {
            Name = name;
            Description = description;
            Company = company;
            Copyright = copyright;
            Version = version;
            UpdateNote = updateNote;

            HomePage = homePage;
            InfoPage = infoPage;
            DownloadPage = downloadPage;
            GitHubPage = gitHubPage;
            QQGroup = qqGroup;
            BitCoinAddress = bitCoinAddress;
        }

        //public struct UpdateNote
        //{
        //    /// <summary>
        //    /// 版本号
        //    /// </summary>
        //    public Version Version { get; set; }
        //    /// <summary>
        //    /// 发布日期
        //    /// </summary>
        //    public DateTime Time { get; set; }
        //    /// <summary>
        //    /// 新增功能
        //    /// </summary>
        //    public string[] Features { get; set; }
        //    /// <summary>
        //    /// 优化调整
        //    /// </summary>
        //    public string[] Optimizations { get; set; }
        //    /// <summary>
        //    /// 问题修复
        //    /// </summary>
        //    public string[] Fixs { get; set; }
        //}
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
    /// 自定义ComboBox选项
    /// </summary>
    public struct CategoryInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}