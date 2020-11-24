using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SharedProject
{
    /// <summary>
    /// 应用信息
    /// </summary>
    public class AppInfo
    {
        private string name;

        /// <summary>
        /// 全路径
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 获取文件目录
        /// </summary>
        /// <returns></returns>
        public string FileFolder
        {
            get
            {
                if (IsExists)
                {
                    return Path.GetDirectoryName(FilePath);
                }
                return "";
            }
        }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get
            {
                if (IsExists)
                {
                    FileVersionInfo info = FileVersionInfo.GetVersionInfo(FilePath);
                    return info.FileDescription;
                }
                return name;
            }
            set 
            {
                FilePath = "";
                name = value;
            }
        }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description 
        { 
            get
            {
                if (IsExists)
                {
                    FileVersionInfo info = FileVersionInfo.GetVersionInfo(FilePath);
                    return info.Comments;
                }
                return "";
            }
        }
        /// <summary>
        /// 组织
        /// </summary>
        public string Company
        {
            get
            {
                if (IsExists)
                {
                    FileVersionInfo info = FileVersionInfo.GetVersionInfo(FilePath);
                    return info.CompanyName;
                }
                return "";
            }
        }
        /// <summary>
        /// 版权信息
        /// </summary>
        public string Copyright
        {
            get
            {
                if (IsExists)
                {
                    FileVersionInfo info = FileVersionInfo.GetVersionInfo(FilePath);
                    return info.LegalCopyright;
                }
                return "";
            }
        }
        /// <summary>
        /// 用户协议
        /// </summary>
        public string UserAgreement
        {
            get
            {
                if (IsExists)
                {
                    Uri uri0 = new Uri("UserAgreement.md", UriKind.Relative);
                    Stream src0 = Application.GetResourceStream(uri0).Stream;
                    string str = new StreamReader(src0, Encoding.UTF8).ReadToEnd();
                    return str;
                }
                return "";
            }
        }
        /// <summary>
        /// 当前版本
        /// </summary>
        public Version Version
        {
            get
            {
                if (IsExists)
                {
                    FileVersionInfo info = FileVersionInfo.GetVersionInfo(FilePath);
                    return new Version(info.FileVersion);
                }
                return new Version();
            }
        }
        /// <summary>
        /// 当前版本短写
        /// </summary>
        public string VersionShort 
        { 
            get 
            {
                return Version.Major + "." + Version.Minor + "." + Version.Build; 
            }
        }
        /// <summary>
        /// 更新日志
        /// </summary>
        public string ReleaseNote
        {
            get
            {
                if (IsExists)
                {
                    Uri uri = new Uri("Resources/ReleaseNotes.md", UriKind.Relative);
                    //Uri uri = new Uri("pack://application:,,,/Resources/ReleaseNotes.md", UriKind.RelativeOrAbsolute);
                    Stream src = Application.GetResourceStream(uri).Stream;
                    string str = new StreamReader(src, Encoding.UTF8).ReadToEnd().Replace("### ", "");
                    return str;
                }
                return "";
            }
        }
        /// <summary>
        /// 主页链接
        /// </summary>
        public string HomePage 
        {
            get
            {
                return "https://github.com/HelloEStar/E-App/wiki/" + Name.Replace(" ", "-");
            }
        }
        /// <summary>
        /// 下载链接
        /// </summary>
        public string DownloadLink { get; set; }
        /// <summary>
        /// 下载文件版本
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Version DownloadVersion
        {
            get
            {
                if (!string.IsNullOrEmpty(DownloadLink))
                {
                    string expr = @"\d+\.\d+\.\d+\.\d+";
                    MatchCollection mc = Regex.Matches(DownloadLink, expr);
                    if (mc.Count > 0)
                    {
                        Version ver = new Version(mc[0].Value);
                        return ver;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// 维基链接
        /// </summary>
        public static string WikiPage { get; } = "https://github.com/HelloEStar/E-App/wiki";
        /// <summary>
        /// GitHub链接
        /// </summary>
        public static string GitHubPage { get; } = "https://github.com/HelloEStar/E-App";
        /// <summary>
        /// 官方Q群
        /// </summary>
        public static string QQGroupLink { get; } = "http://jq.qq.com/?_wv=1027&k=5TQxcvR";
        /// <summary>
        /// 官方Q群
        /// </summary>
        public static string QQGroupNumber { get; } = "279807070";
        /// <summary>
        /// 比特币地址
        /// </summary>
        public static string BitCoinAddress { get; } = "19LHHVQzWJo8DemsanJhSZ4VNRtknyzR1q";

        public static string ThemeFolder { get; } = "Themes";
        public static string BackupFolder { get; } = "Backups";
        public static string DownloadFolder { get; } = "Downloads";
        public static string TempFolder { get; } = "TempFiles";


        /// <summary>
        /// 应用文件是否存在
        /// </summary>
        /// <returns></returns>
        public bool IsExists
        {
            get
            {
                return File.Exists(FilePath);
            }
        }
        /// <summary>
        /// 检测指定软件是否运行
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsRunning
        {
            get
            {
                if (IsExists)
                {
                    try
                    {
                        Process[] ps = Process.GetProcesses();
                        foreach (Process p in ps)
                        {
                            if (p.ProcessName == Name && p.MainModule.FileName == FilePath)
                            {
                                return true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                return false;
            }
        }

        public AppInfo()
        {
            FilePath = Process.GetCurrentProcess().MainModule.FileName;
        }
        public AppInfo(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="path"></param>
        public void Run()
        {
            Process.Start(FilePath);
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="path"></param>
        public void Kill()
        {
            Process[] ps = Process.GetProcesses();
            foreach (Process p in ps)
            {
                if (p.ProcessName == Name && p.MainModule.FileName == FilePath)
                {
                    p.CloseMainWindow();
                    return;
                }
            }
        }
        /// <summary>
        /// 浏览
        /// </summary>
        public void Browse()
        {
            if (Name == "E Updater")
            {
                Process.Start("explorer.exe", @" /select, " + FilePath);
                return;
            }
            
            if (IsExists)
            {
                string tip = "是否手动指定软件位置？\n是：手动指定软件位置\n否：打开软件所在位置";
                MessageBoxResult mbr = MessageBox.Show(tip, "提示", MessageBoxButton.YesNoCancel);
                switch (mbr)
                {
                    case MessageBoxResult.Yes:
                        break;
                    case MessageBoxResult.No:
                        Process.Start("explorer.exe", @" /select, " + FilePath);
                        return;
                    default:
                        return;
                }
            }
            //System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog
            //{
            //    Filter = Name + "可执2行文件(" + Name + ".exe)|" + Name + ".exe"
            //};
            //dialog.ShowDialog();
            //if (File.Exists(dialog.FileName))
            //{
            //    FilePath = dialog.FileName;
            //}
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
    /// 语言项
    /// </summary>
    public class RDItem : ResourceDictionary
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public ResourceDictionary RD { get; set; }

        public RDItem(string name, string value)
        {
            Name = name;
            Value = value;
            Uri uri = new Uri("Languages/" + value + ".xaml", UriKind.Relative);
            ResourceDictionary rd = Application.LoadComponent(uri) as ResourceDictionary;
            RD = rd;
        }
    }
}