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
                    return info.ProductName;
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
                    return info.FileDescription;
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
                    string userAgreement = new StreamReader(src0, Encoding.UTF8).ReadToEnd();
                    return userAgreement;
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
                    return new Version(info.ProductVersion);
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
        public string UpdateNote
        {
            get
            {
                if (IsExists)
                {
                    Uri uri = new Uri("Resources/ReleaseNotes.md", UriKind.Relative);
                    Stream src = Application.GetResourceStream(uri).Stream;
                    string updateNote = new StreamReader(src, Encoding.UTF8).ReadToEnd().Replace("### ", "");
                    return updateNote;
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
                return "https://github.com/HelloEStar/E.App/wiki/" + Name.Replace(" ", "-");
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
        public static string WikiPage { get; } = "https://github.com/HelloEStar/E.App/wiki";
        /// <summary>
        /// GitHub链接
        /// </summary>
        public static string GitHubPage { get; } = "https://github.com/HelloEStar/E.App";
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
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = Name + "可执行文件(" + Name + ".exe)|" + Name + ".exe"
            };
            dialog.ShowDialog();
            if (File.Exists(dialog.FileName))
            {
                FilePath = dialog.FileName;
            }
        }
    }

    /// <summary>
    /// ini文件助手
    /// </summary>
    public class INIHelper
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
    /// 颜色助手
    /// </summary>
    public class ColorHelper
    {
        /// <summary>
        /// 创建颜色
        /// </summary>
        /// <param name="text">ARGB色值，以点号分隔，0-255</param>
        /// <returns></returns>
        public static Color Create(string text)
        {
            try
            {
                string[] colors = text.Split('.');
                byte red = byte.Parse(colors[0]);
                byte green = byte.Parse(colors[1]);
                byte blue = byte.Parse(colors[2]);
                byte alpha = byte.Parse(colors[3]);
                Color color = Color.FromArgb(alpha, red, green, blue);
                return color;
            }
            catch (Exception)
            {
                Color color = Color.FromArgb(255, 125, 125, 125);
                return color;
            }
        }

        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="colorName"></param>
        /// <param name="c"></param>
        public static void SetColor(ResourceDictionary resource, string colorName, Color c)
        {
            resource.Remove(colorName);
            resource.Add(colorName, new SolidColorBrush(c));
        }

        /// <summary>
        /// 设置主题
        /// </summary>
        /// <param name="themePath">主题文件路径</param>
        public static void SetColors(ResourceDictionary resource, string themePath)
        {
            SetColor(resource, "一级字体颜色", Create(INIHelper.ReadIniKeys("Font", "Level_1", themePath)));
            SetColor(resource, "二级字体颜色", Create(INIHelper.ReadIniKeys("Font", "Level_2", themePath)));
            SetColor(resource, "三级字体颜色", Create(INIHelper.ReadIniKeys("Font", "Level_3", themePath)));

            SetColor(resource, "一级背景颜色", Create(INIHelper.ReadIniKeys("Background", "Level_1", themePath)));
            SetColor(resource, "二级背景颜色", Create(INIHelper.ReadIniKeys("Background", "Level_2", themePath)));
            SetColor(resource, "三级背景颜色", Create(INIHelper.ReadIniKeys("Background", "Level_3", themePath)));

            SetColor(resource, "一级边框颜色", Create(INIHelper.ReadIniKeys("Border", "Level_1", themePath)));

            SetColor(resource, "有焦点选中颜色", Create(INIHelper.ReadIniKeys("Highlight", "Focused", themePath)));
            SetColor(resource, "无焦点选中颜色", Create(INIHelper.ReadIniKeys("Highlight", "UnFocused", themePath)));
        }
    }

    /// <summary>
    /// 语言助手
    /// </summary>
    public class LanguageHelper
    {
        /// <summary>
        /// 载入语言选项
        /// </summary>
        public static void LoadLanguageItems(ComboBox cbb)
        {
            List<LanguageItem> LanguageItems = new List<LanguageItem>()
            {
                new LanguageItem("中文（默认）", "zh_CN"),
                new LanguageItem("English", "en_US"),
            };

            cbb.Items.Clear();
            foreach (LanguageItem item in LanguageItems)
            {
                ComboBoxItem cbi = new ComboBoxItem
                {
                    Content = item.Name,
                    ToolTip = item.Value,
                    Tag = item.RD
                };
                cbb.Items.Add(cbi);
            }
        }
    }

    /// <summary>
    /// 主题助手
    /// </summary>
    public class ThemeHelper
    {
        /// <summary>
        /// 载入所有可用主题
        /// </summary>
        public static void LoadThemeItems(ComboBox cbb)
        {
            //创建皮肤文件夹
            if (!Directory.Exists(AppInfo.ThemeFolder))
            { return; }

            cbb.Items.Clear();
            string[] themes = Directory.GetFiles(AppInfo.ThemeFolder);
            foreach (string item in themes)
            {
                string tmp = Path.GetExtension(item);
                if (tmp == ".ini" || tmp == ".INI")
                {
                    string tmp2 = INIHelper.ReadIniKeys("File", "Type", item);
                    //若是主题配置文件
                    if (tmp2 == "Theme")
                    {
                        ComboBoxItem cbi = new ComboBoxItem
                        {
                            Content = Path.GetFileNameWithoutExtension(item),
                            ToolTip = item
                        };
                        cbb.Items.Add(cbi);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 消息助手
    /// </summary>
    public class MessageHelper
    {
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="resourceName">资源名</param>
        /// <param name="newBox">是否弹出对话框</param>
        public static void ShowMessage(Label lbl, string message, bool newBox = false)
        {
            if (newBox)
            {
                MessageBox.Show(message);
            }
            else
            {
                if (lbl != null)
                {
                    //实例化一个DoubleAnimation类。
                    DoubleAnimation doubleAnimation = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = new Duration(TimeSpan.FromSeconds(3))
                    };
                    //为元素设置BeginAnimation方法。
                    lbl.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);

                    lbl.Content = message;
                }
            }
        }
    }

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

    public class FolderHelper
    {
        /// <summary>
        /// 选择文件夹
        /// </summary>
        public static string ChooseFolder()
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                Description = "请选择文件夹"
            };

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return fbd.SelectedPath;
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 时间间隔-浮点 转换器
    /// </summary>
    public class TimeSpanDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((TimeSpan)value).TotalSeconds;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TimeSpan.FromSeconds((double)value);
        }
    }
    /// <summary>
    /// 可见性-布尔 转换器
    /// </summary>
    public class VisibilityBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Visibility)value) == Visibility.Visible;
        }
    }

    /// <summary>
    /// 语言项
    /// </summary>
    public class LanguageItem : ResourceDictionary
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public ResourceDictionary RD { get; set; }

        public LanguageItem(string name, string value)
        {
            Name = name;
            Value = value;
            Uri uri = new Uri( value + ".xaml", UriKind.Relative);
            ResourceDictionary rd = Application.LoadComponent(uri) as ResourceDictionary;
            RD = rd;
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