using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;   

using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Forms.Application;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;

using Settings = E.Updater.Properties.Settings;
using User = E.Updater.Properties.User;
using E.Utility;
using System.Windows.Input;

namespace E.Updater
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 属性
        /// <summary>
        /// 应用信息
        /// </summary>
        private AppInfo AppInfo { get; set; }
        /// <summary>
        /// 语言列表
        /// </summary>
        private List<ItemInfo> LanguageItems { get; set; } = new List<ItemInfo>();
        /// <summary>
        /// 主题集合
        /// </summary>
        private List<TextBlock> ThemeItems { get; set; } = new List<TextBlock>();

        //当前版本
        private Version EUThisVer { get; set; }
        private Version EWThisVer { get; set; }
        private Version EPThisVer { get; set; }
        private Version ENThisVer { get; set; }
        //最新版本
        private Version EUNewVer { get; set; }
        private Version EWNewVer { get; set; }
        private Version EPNewVer { get; set; }
        private Version ENNewVer { get; set; }
        //模块状态
        private InstallState EUState { get; set; }
        private InstallState EWState { get; set; }
        private InstallState EPState { get; set; }
        private InstallState ENState { get; set; }
        #endregion

        //构造
        /// <summary>
        /// 默认构造器
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        //载入
        /// <summary>
        /// 载入应用信息
        /// </summary>
        private void LoadAppInfo()
        {
            AssemblyProductAttribute product = (AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute));
            AssemblyDescriptionAttribute description = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyDescriptionAttribute));
            AssemblyCompanyAttribute company = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCompanyAttribute));
            AssemblyCopyrightAttribute copyright = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute));

            Uri uri0 = new Uri("/文档/用户协议.md", UriKind.Relative);
            Stream src0 = System.Windows.Application.GetResourceStream(uri0).Stream;
            string userAgreement = new StreamReader(src0, Encoding.UTF8).ReadToEnd();

            Uri uri = new Uri("/文档/更新日志.md", UriKind.Relative);
            Stream src = System.Windows.Application.GetResourceStream(uri).Stream;
            string updateNote = new StreamReader(src, Encoding.UTF8).ReadToEnd().Replace("### ", "");

            string homePage = "https://github.com/HelloEStar/E.App/wiki/" + product.Product.Replace(" ", "-");
            string gitHubPage = "https://github.com/HelloEStar/E.App";
            string qqGroupLink = "http://jq.qq.com/?_wv=1027&k=5TQxcvR";
            string qqGroupNumber = "279807070";
            string bitCoinAddress = "19LHHVQzWJo8DemsanJhSZ4VNRtknyzR1q";
            AppInfo = new AppInfo(product.Product, description.Description, company.Company, copyright.Copyright, userAgreement, new Version(Application.ProductVersion), updateNote,
                                  homePage, gitHubPage, qqGroupLink, qqGroupNumber, bitCoinAddress);
        }
        /// <summary>
        /// 载入偏好设置
        /// </summary>
        private void LoadSettings()
        {
            //刷新选中项
            SelectLanguageItem(User.Default.language);
            SelectThemeItem(User.Default.ThemePath);
        }
        /// <summary>
        /// 创建语言选项
        /// </summary>
        private void LoadLanguageItems()
        {
            LanguageItems.Clear();
            ItemInfo zh_CN = new ItemInfo("中文（默认）", "zh_CN");
            ItemInfo en_US = new ItemInfo("English", "en_US");
            LanguageItems.Add(zh_CN);
            LanguageItems.Add(en_US);

            //绑定数据，真正的赋值
            CbbLanguages.ItemsSource = LanguageItems;
            CbbLanguages.DisplayMemberPath = "Name";
            CbbLanguages.SelectedValuePath = "Value";
        }
        /// <summary>
        /// 载入所有可用主题
        /// </summary>
        private void LoadThemeItems()
        {
            //创建皮肤文件夹
            if (!Directory.Exists(User.Default.ThemesDir))
            { Directory.CreateDirectory(User.Default.ThemesDir); }

            string[] _mySkins = Directory.GetFiles(User.Default.ThemesDir);
            ThemeItems.Clear();
            foreach (string s in _mySkins)
            {
                string tmp = Path.GetExtension(s);
                if (tmp == ".ini" || tmp == ".INI")
                {
                    string tmp2 = INIOperator.ReadIniKeys("文件", "类型", s);
                    //若是主题配置文件
                    if (tmp2 == "主题")
                    {
                        string tmp3 = INIOperator.ReadIniKeys("文件", "版本", s);
                        if (tmp3 == AppInfo.Version.ToString())
                        {
                            TextBlock theme = new TextBlock
                            {
                                Text = Path.GetFileNameWithoutExtension(s),
                                ToolTip = s
                            };
                            ThemeItems.Add(theme);
                        }
                    }
                }
            }

            CbbThemes.Items.Clear();
            foreach (TextBlock item in ThemeItems)
            {
                TextBlock theme = new TextBlock()
                {
                    Text = item.Text,
                    ToolTip = item.ToolTip
                };
                CbbThemes.Items.Add(theme);
            }
        }

        //打开
        /// <summary>
        /// 打开
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="path"></param>
        private void Start(string path)
        {
            Process pr = new Process();
            if (File.Exists(path))
            {
                pr.StartInfo.FileName = path;
                pr.Start();
            }
            else
            {
                ShowMessage("可执行文件不存在");
            }
        }

        //关闭
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="path"></param>
        private void Stop(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            Process[] ps = Process.GetProcesses();
            foreach (Process p in ps)
            {
                if (p.ProcessName == name && p.MainModule.FileName == path)
                {
                    p.CloseMainWindow();
                    ShowMessage("已关闭" + name);
                    break;
                }
            }
        }

        //创建
        /// <summary>
        /// 创建颜色
        /// </summary>
        /// <param name="text">ARGB色值，以点号分隔，0-255</param>
        /// <returns></returns>
        private static Color CreateColor(string text)
        {
            //MessageBox.Show(text);
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

        //添加

        ///移除

        ///清空

        ///删除

        //获取
        /// <summary>
        /// 获取所有软件当前版本信息
        /// </summary>
        private void GetAllThisVersion()
        {
            EUThisVer = GetThisVersion(Settings.Default._EU, "\\E Updater.exe");
            EWThisVer = GetThisVersion(Settings.Default._EW, "\\E Writer.exe");
            EPThisVer = GetThisVersion(Settings.Default._EP, "\\E Player.exe");
            ENThisVer = GetThisVersion(Settings.Default._EN, "\\E Number.exe");
        }
        /// <summary>
        /// 检测指定目录是否有可执行文件，并获取其版本信息
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="name"></param>
        private Version GetThisVersion(string dir, string name)
        {
            if (Directory.Exists(dir))
            {
                string path = dir + name;
                if (File.Exists(path))
                {
                    FileVersionInfo file = FileVersionInfo.GetVersionInfo(path);
                    Version ver = new Version(file.FileVersion);
                    return ver;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 获取所有软件最新版本信息
        /// </summary>
        /// <returns>所有软件新版本信息</returns>
        private void GetAllNewVersion()
        {
            try
            {
                //从网络获取数据
                WebClient webClient = new WebClient();
                Stream st = webClient.OpenRead(Settings.Default._LatestVersionInfo);
                StreamReader stw = new StreamReader(st, Encoding.UTF8);
                string info = stw.ReadToEnd().Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
                stw.Close();
                st.Close();

                string[] infos = info.Split(';');
                foreach (string items in infos)
                {
                    string[] item = items.Split(':');
                    switch (item[0])
                    {
                        case "EU":
                            EUNewVer = new Version(item[1]);
                            break;
                        case "EW":
                            EWNewVer = new Version(item[1]);
                            break;
                        case "EP":
                            EPNewVer = new Version(item[1]);
                            break;
                        case "EN":
                            ENNewVer = new Version(item[1]);
                            break;
                    }
                }
                ShowMessage("已获取最新版本信息");
            }
            catch (Exception)
            {
                ShowMessage("获取最新版本信息时出现错误");
            }
        }
        /// <summary>
        /// 浏览
        /// </summary>
        private string GetFolderPath()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                //SelectedPath = FatherPath.Text,
                Description = "请选择软件安装位置："
            };
            //按下确定选择的按钮，获取根目录文件夹路径
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return fbd.SelectedPath;
            }
            else
            {
                return null;
            }
        }

        //设置
        /// <summary>
        /// 设置语言显示
        /// </summary>
        /// <param name="language">语言简拼</param>
        private void SetLanguage(string language)
        {
            try
            {
                ResourceDictionary langRd;
                langRd = System.Windows.Application.LoadComponent(new Uri(@"语言/" + language + ".xaml", UriKind.Relative)) as ResourceDictionary;
                if (langRd != null)
                {
                    //主窗口更改语言
                    if (Resources.MergedDictionaries.Count > 0)
                    {
                        Resources.MergedDictionaries.Clear();
                    }
                    Resources.MergedDictionaries.Add(langRd);
                    User.Default.language = language;
                    User.Default.Save();
                }
            }
            catch (Exception e2)
            {
                MessageBox.Show(e2.Message);
            }
        }
        /// <summary>
        /// 切换主题显示
        /// </summary>
        private void SetTheme(string themePath)
        {
            foreach (TextBlock theme in ThemeItems)
            {
                if (theme.ToolTip.ToString() == themePath)
                {
                    if (File.Exists(themePath))
                    {
                        SetSkin(themePath);
                        User.Default.ThemePath = themePath;
                    }
                    else
                    {
                        ThemeItems.Remove(theme);
                        //设为默认主题
                        User.Default.ThemePath = User.Default.ThemePath;
                        SetSkin(User.Default.ThemePath);
                        ShowMessage("偏好主题的不存在");
                    }
                    User.Default.Save();
                    break;
                }
            }
        }
        /// <summary>
        /// 切换下个主题显示
        /// </summary>
        private void SetNextTheme()
        {
            foreach (TextBlock theme in ThemeItems)
            {
                if (theme.ToolTip.ToString() == User.Default.ThemePath)
                {
                    int themeOrder = ThemeItems.IndexOf(theme);
                    int themeCounts = ThemeItems.Count;
                    if (themeOrder + 1 < themeCounts)
                    { themeOrder += 1; }
                    else
                    { themeOrder = 0; }
                    if (File.Exists(ThemeItems[themeOrder].ToolTip.ToString()))
                    {
                        //设为此主题
                        User.Default.ThemePath = ThemeItems[themeOrder].ToolTip.ToString();
                        User.Default.Save();
                        SetSkin(User.Default.ThemePath);
                    }
                    else
                    {
                        ShowMessage("下一个主题的配置文件不存在");
                        ThemeItems.Remove(ThemeItems[themeOrder]);
                    }
                    break;
                }
            }

        }
        /// <summary>
        /// 重置主题颜色
        /// </summary>
        /// <param name="themePath">主题文件路径</param>
        private void SetSkin(string themePath)
        {
            SetColor("一级字体颜色", CreateColor(INIOperator.ReadIniKeys("字体", "一级字体", themePath)));
            SetColor("二级字体颜色", CreateColor(INIOperator.ReadIniKeys("字体", "二级字体", themePath)));
            SetColor("三级字体颜色", CreateColor(INIOperator.ReadIniKeys("字体", "三级字体", themePath)));

            SetColor("一级背景颜色", CreateColor(INIOperator.ReadIniKeys("背景", "一级背景", themePath)));
            SetColor("二级背景颜色", CreateColor(INIOperator.ReadIniKeys("背景", "二级背景", themePath)));
            SetColor("三级背景颜色", CreateColor(INIOperator.ReadIniKeys("背景", "三级背景", themePath)));

            SetColor("一级边框颜色", CreateColor(INIOperator.ReadIniKeys("边框", "一级边框", themePath)));

            SetColor("有焦点选中颜色", CreateColor(INIOperator.ReadIniKeys("高亮", "有焦点选中", themePath)));
            SetColor("无焦点选中颜色", CreateColor(INIOperator.ReadIniKeys("高亮", "无焦点选中", themePath)));
        }
        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="colorName"></param>
        /// <param name="c"></param>
        private void SetColor(string colorName, Color c)
        {
            Resources.Remove(colorName);
            Resources.Add(colorName, new SolidColorBrush(c));
        }

        //重置
        /// <summary>
        /// 重置应用设置
        /// </summary>
        private void ResetAppSettings()
        {
            Settings.Default.Reset();
            ShowMessage("已清空运行信息", true);
        }
        /// <summary>
        /// 重置用户设置
        /// </summary>
        private void ResetUserSettings()
        {
            User.Default.Reset();
        }

        //选择
        ///选择
        /// <summary>
        /// 设置语言选项
        /// </summary>
        /// <param name="language">语言简拼</param>
        private void SelectLanguageItem(string language)
        {
            foreach (ItemInfo ci in LanguageItems)
            {
                if (ci.Value == language)
                {
                    CbbLanguages.SelectedItem = ci;
                    break;
                }
            }
        }
        /// <summary>
        /// 设置主题选项
        /// </summary>
        /// <param name="themePath">主题路径</param>
        private void SelectThemeItem(string themePath)
        {
            foreach (TextBlock item in CbbThemes.Items)
            {
                if (item.ToolTip.ToString() == themePath)
                {
                    CbbThemes.SelectedItem = item;
                    break;
                }
            }
        }

        //检查
        private void RefreshAllAppsIsRunning()
        {
            if (IsRunning(Settings.Default._EW + "\\E Writer.exe"))
            {
                Btn_EWRun.Content = FindResource("终止");
            }
            else
            {
                Btn_EWRun.Content = FindResource("启动");
            }
            if (IsRunning(Settings.Default._EP + "\\E Player.exe"))
            {
                Btn_EPRun.Content = FindResource("终止");
            }
            else
            {
                Btn_EPRun.Content = FindResource("启动");
            }
            if (IsRunning(Settings.Default._EN + "\\E Number.exe"))
            {
                Btn_ENRun.Content = FindResource("终止");
            }
            else
            {
                Btn_ENRun.Content = FindResource("启动");
            }
        }
        /// <summary>
        /// 检测指定软件是否运行
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsRunning(string path)
        {
            if (File.Exists(path))
            {
                string name = Path.GetFileNameWithoutExtension(path);
                Process[] ps = Process.GetProcesses();
                foreach (Process p in ps)
                {
                    //软件已打开
                    if (p.ProcessName == name && p.MainModule.FileName == path)
                    {
                        return true;
                    }
                }
                //软件未打开
                return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 对比所有软件当前版本与最新版本信息
        /// </summary>
        private void RefreshAllAppsState()
        {
            EUState = GetState(EUNewVer, EUThisVer, Settings.Default._EU) ;
            EWState = GetState(EWNewVer, EWThisVer, Settings.Default._EW);
            EPState = GetState(EPNewVer, EPThisVer, Settings.Default._EP);
            ENState = GetState(ENNewVer, ENThisVer, Settings.Default._EN);
        }
        /// <summary>
        /// 对比指定软件当前版本与最新版本信息
        /// </summary>
        /// <param name="newVer"></param>
        /// <param name="thisVer"></param>
        /// <param name="path"></param>
        private InstallState GetState(Version newVer, Version thisVer, string path)
        {
            InstallState state;
            if (!Directory.Exists(path))
            {
                state = InstallState.未指定安装目录;
            }
            else
            {
                if (thisVer == null)
                {
                    state = InstallState.未找到可执行文件;
                }
                else
                {
                    state = InstallState.已安装旧版本;
                    if (newVer != null)
                    {
                        int hasNewVersion = newVer.CompareTo(thisVer);
                        if (hasNewVersion == 1)
                        {
                            state = InstallState.已安装旧版本;
                        }
                        else
                        {
                            state = InstallState.已安装最新版本;
                        }
                    }
                    else
                    {
                        ShowMessage("未检测到新版本");
                    }
                }
            }
            return state;
        }

        //刷新
        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="isCheckNew"></param>
        private void Refresh(bool isCheckNew = false)
        {
            //载入目录
            ShowAllPath();

            //获取版本信息
            GetAllThisVersion();
            ShowAllThisVersion();
            if (isCheckNew)
            {
                if (NetHelper.IsOnLine())
                {
                    ShowMessage(FindResource("网络连接成功").ToString());
                    GetAllNewVersion();
                    ShowAllNewVersion();
                }
                else
                {
                    ShowMessage(FindResource("网络连接失败").ToString());
                }
            }

            //对比版本
            RefreshAllAppsState();
            //设置UI按钮
            RefreshAllBtnsState();
            RefreshAllAppsIsRunning();
        }
        /// <summary>
        /// 设置所有UI按钮状态
        /// </summary>
        private void RefreshAllBtnsState()
        {
            RefreshBtnState(EUState, Btn_EUBrowse, Btn_EURun, Btn_EUInstall, Btn_EUUninstall);
            RefreshBtnState(EWState, Btn_EWBrowse, Btn_EWRun, Btn_EWInstall, Btn_EWUninstall);
            RefreshBtnState(EPState, Btn_EPBrowse, Btn_EPRun, Btn_EPInstall, Btn_EPUninstall);
            RefreshBtnState(ENState, Btn_ENBrowse, Btn_ENRun, Btn_ENInstall, Btn_ENUninstall);
        }
        /// <summary>
        /// 设置UI按钮状态
        /// </summary>
        /// <param name="state"></param>
        /// <param name="bro"></param>
        /// <param name="run"></param>
        /// <param name="ins"></param>
        /// <param name="uni"></param>
        private void RefreshBtnState(InstallState state, Button bro, Button run, Button ins, Button uni)
        {
            switch (state)
            {
                case InstallState.未指定安装目录:
                    bro.IsEnabled = true;
                    ins.IsEnabled = false;
                    ins.Content = FindResource("安装");
                    uni.IsEnabled = false;
                    run.IsEnabled = false;
                    break;
                case InstallState.未找到可执行文件:
                    bro.IsEnabled = true;
                    ins.IsEnabled = true;
                    ins.Content = FindResource("安装");
                    uni.IsEnabled = false;
                    run.IsEnabled = false;
                    break;
                case InstallState.已安装旧版本:
                    bro.IsEnabled = true;
                    ins.IsEnabled = true;
                    ins.Content = FindResource("更新");
                    uni.IsEnabled = true;
                    run.IsEnabled = true;
                    break;
                case InstallState.已安装最新版本:
                    bro.IsEnabled = true;
                    ins.IsEnabled = false;
                    ins.Content = FindResource("更新");
                    uni.IsEnabled = true;
                    run.IsEnabled = true;
                    break;
            }
        }

        //显示
        /// <summary>
        /// 显示软件信息
        /// </summary>
        private void ShowAppInfo()
        {
            ThisName.Text = AppInfo.Name;
            Description.Text = AppInfo.Description;
            Developer.Text = AppInfo.Company;
            Version.Text = AppInfo.Version.ToString();
            BitCoinAddress.Text = AppInfo.BitCoinAddress;
            UpdateNote.Text = AppInfo.UpdateNote;
        }
        /// <summary>
        /// 检测所有目录是否存在
        /// </summary>
        private void ShowAllPath()
        {
            TBx_EUPath.Text = Settings.Default._EU;
            TBx_EWPath.Text = Settings.Default._EW;
            TBx_EPPath.Text = Settings.Default._EP;
            TBx_ENPath.Text = Settings.Default._EN;
        }
        /// <summary>
        /// 显示最新版本信息
        /// </summary>
        private void ShowAllNewVersion()
        {
            if (EUNewVer != null)
            {
                Lbl_EUNewVer.Text = "最新版本：" + EUNewVer.ToString();
            }
            else
            {
                Lbl_EUNewVer.Text = "无最新版本";
            }
            if (EWNewVer != null)
            {
                Lbl_EWNewVer.Text = "最新版本：" + EWNewVer.ToString();
            }
            else
            {
                Lbl_EWNewVer.Text = "无最新版本";
            }
            if (EPNewVer != null)
            {
                Lbl_EPNewVer.Text = "最新版本：" + EPNewVer.ToString();
            }
            else
            {
                Lbl_EPNewVer.Text = "无最新版本";
            }
            if (ENNewVer != null)
            {
                Lbl_ENNewVer.Text = "最新版本：" + ENNewVer.ToString();
            }
            else
            {
                Lbl_ENNewVer.Text = "无最新版本";
            }
        }
        /// <summary>
        /// 显示当前版本信息
        /// </summary>
        private void ShowAllThisVersion()
        {
            if (EUThisVer != null)
            {
                Lbl_EUThisVer.Text = "当前版本：" + EUThisVer.ToString();
            }
            else
            {
                Lbl_EUThisVer.Text = "未安装任何版本";
            }
            if (EWThisVer != null)
            {
                Lbl_EWThisVer.Text = "当前版本：" + EWThisVer.ToString();
            }
            else
            {
                Lbl_EWThisVer.Text = "未安装任何版本";
            }
            if (EPThisVer != null)
            {
                Lbl_EPThisVer.Text = "当前版本：" + EPThisVer.ToString();
            }
            else
            {
                Lbl_EPThisVer.Text = "未安装任何版本";
            }
            if (ENThisVer != null)
            {
                Lbl_ENThisVer.Text = "当前版本：" + ENThisVer.ToString();
            }
            else
            {
                Lbl_ENThisVer.Text = "未安装任何版本";
            }
        }
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="resourceName">资源名</param>
        /// <param name="newBox">是否弹出对话框</param>
        private void ShowMessage(string message, bool newBox = false)
        {
            if (newBox)
            {
                MessageBox.Show(message);
            }
            else
            {
                Lbl_Message.Text = message;
            }
        }

        //其它
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">文件路径</param>
        /// <param name="name">保存的文件名</param>
        private void Download(string url, string name)
        {
            //尝试下载
            try
            {
                WebClient webClient = new WebClient();
                //确保下载文件夹存在
                if (!Directory.Exists(Settings.Default._download))
                { Directory.CreateDirectory(Settings.Default._download); }
                //设置存储路径
                string address = Settings.Default._download + @"\" + name;
                if (!File.Exists(address))
                {
                    //开始下载
                    webClient.DownloadFile(new Uri(url), address);
                    ShowMessage(name + "下载成功");
                    if (MessageBox.Show(name + " 已完成下载，是否打开下载文件夹", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Process.Start("explorer.exe", Settings.Default._download);
                    }
                }
                else
                {
                    if (MessageBox.Show("下载文件夹内含有同名文件，替换此文件？", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        //开始下载
                        webClient.DownloadFile(new Uri(url), address);
                        ShowMessage(name + "下载成功");
                        if (MessageBox.Show(name + " 已完成下载，是否打开下载文件夹", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            Process.Start("explorer.exe", Settings.Default._download);
                        }
                    }
                }
            }
            catch (Exception)
            {
                ShowMessage(name + "下载失败");
            }
        }
        /// <summary>  
        /// 功能：解压zip格式的文件。  
        /// </summary>  
        /// <param name="zipFilePath">压缩文件路径</param>  
        /// <param name="unZipDir">解压文件存放路径,为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹</param>  
        /// <param name="err">出错信息</param>  
        /// <returns>解压是否成功</returns>  
        private void UnZip(string zipFilePath, string unZipDir)
        {
            //检查错误
            if (zipFilePath == string.Empty)
            {
                //throw new Exception("压缩文件不能为空！");
                return;
            }
            if (!File.Exists(zipFilePath))
            {
                //throw new System.IO.FileNotFoundException("压缩文件不存在！");
                return;
            }
            //文件夹里建一个带有时间戳的文件夹
            //unZipDir += "weather" + DateTime.Now.ToString("yyyyMMddHHmmsss");
            //解压文件夹为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹  
            if (unZipDir == string.Empty)
            { unZipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath)); }
            if (!unZipDir.EndsWith("\\"))
            { unZipDir += "\\"; }
            if (!Directory.Exists(unZipDir))
            { Directory.CreateDirectory(unZipDir); }
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFilePath)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(unZipDir + directoryName);
                        //将解压后的文件放到带时间戳的文件夹里
                    }
                    if (!directoryName.EndsWith("\\"))
                    { directoryName += "\\"; }
                    if (fileName != String.Empty)
                    {
                        using (FileStream streamWriter = File.Create(unZipDir + theEntry.Name))
                        {
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

            }
        }
        /// <summary>
        /// 安装
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dir"></param>
        /// <param name="download"></param>
        private void Install(string name, string dir, string download)
        {
            if (Directory.Exists(dir))
            {
                Download(download, name);
                UnZip(Settings.Default._download + "\\" + name, dir);

                ShowMessage(Path.GetFileNameWithoutExtension(name) + "已安装", false);
                Refresh();
            }
            else
            {
                ShowMessage("请选择一个有效的安装路径");
            }
        }
        /// <summary>
        /// 卸载
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dir"></param>
        private void Uninstall(string name, string dir)
        {
            string path = dir + "\\" + name;
            if (File.Exists(path))
            {
                if (!IsRunning(path))
                {
                    //删除所有相关文件
                    File.Delete(path);
                    ShowMessage(name + "已卸载");
                    Refresh();
                }
                else
                {
                    ShowMessage("请先关闭" + name, true);
                }
            }

        }


        #region 事件
        //窗口激活
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //载入并显示应用信息
            LoadAppInfo();
            ShowAppInfo();

            //载入下拉菜单项
            LoadLanguageItems();
            LoadThemeItems();

            //载入设置
            LoadSettings();

            //初始化
            SetLanguage(User.Default.language);
            SetTheme(User.Default.ThemePath);

            Settings.Default._EU = Application.StartupPath;
            Settings.Default.Save();
            //提示消息
            ShowMessage("已载入");

            //Thread th = new Thread(new ThreadStart(Refresh));
            //th.Start(true);
            Refresh(true);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void BtnFold_Click(object sender, RoutedEventArgs e)
        {
            if (PanCenter.Visibility == Visibility.Visible)
            {
                PanCenter.Visibility = Visibility.Collapsed;
                BtnFold.BorderThickness = new Thickness(0, 0, 0, 0);
            }
            else
            {
                PanCenter.Visibility = Visibility.Visible;
                BtnFold.BorderThickness = new Thickness(4, 0, 0, 0);
            }
        }
        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            PanSetting.Visibility = Visibility.Visible;
            PanAbout.Visibility = Visibility.Collapsed;

            BtnsSetting.Visibility = Visibility.Visible;
            BtnsAbout.Visibility = Visibility.Collapsed;

            BtnSetting.BorderThickness = new Thickness(4, 0, 0, 0);
            BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
        }
        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            PanSetting.Visibility = Visibility.Collapsed;
            PanAbout.Visibility = Visibility.Visible;

            BtnsSetting.Visibility = Visibility.Collapsed;
            BtnsAbout.Visibility = Visibility.Visible;

            BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnAbout.BorderThickness = new Thickness(4, 0, 0, 0);
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetUserSettings();
            LoadSettings();
            ShowMessage("已重置");
        }
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh(true);
        }

        private void BitCoinAddress_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetDataObject(BitCoinAddress.Text, true);
            ShowMessage("已复制");
        }
        private void BtnHomePage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.HomePage);
        }
        private void BtnGitHubPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.GitHubPage);
        }
        private void BtnQQGroup_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.QQGroupLink);
        }

        //设置更改事件
        private void CbbLanguages_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (CbbLanguages.SelectedValue != null)
            {
                string langName = CbbLanguages.SelectedValue.ToString();
                SetLanguage(langName);
            }
        }
        private void CbbThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbbThemes.SelectedItem != null)
            {
                TextBlock item = CbbThemes.SelectedItem as TextBlock;
                string tmp = item.ToolTip.ToString();
                if (File.Exists(tmp))
                {
                    SetTheme(tmp);
                    ShowMessage("已更改");
                }
                else
                {
                    CbbThemes.Items.Remove(CbbThemes.SelectedItem);
                    ShowMessage("该主题的配置文件不存在");
                }
            }
        }

        //浏览
        private void Btn_EUBrowse_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Btn_EWBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = GetFolderPath();
            if (path != null)
            {
                Settings.Default._EW = path;
                Settings.Default.Save();
                //更新UI
                Refresh();
            }
        }
        private void Btn_EPBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = GetFolderPath();
            if (path != null)
            {
                Settings.Default._EP = path;
                Settings.Default.Save();
                //更新UI
                Refresh();
            }
        }
        private void Btn_ENBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = GetFolderPath();
            if (path != null)
            {
                Settings.Default._EN = path;
                Settings.Default.Save();
                //更新UI
                Refresh();
            }
        }
        private void Btn_EUInstall_Click(object sender, RoutedEventArgs e)
        {
            //创建的压缩文件名称
            string name = "E Updater " + EUNewVer + ".zip";
            string _bat = "Update.bat";
            //下载压缩包
            Download(Settings.Default._EUDownload, name);
            //解压
            UnZip(Settings.Default._download + name, Settings.Default._temp);
            //获取解压后所有文件的路径
            string _file1 = Settings.Default._temp + "\\E Updater.exe";
            string _file2 = Settings.Default._temp + "\\ICSharpCode.SharpZipLib.dll";
            //创建批处理命令
            FileStream fs = new FileStream(_bat, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine("@echo off");
            sw.WriteLine("move \"" + _file1 + "\" \"\"");
            sw.WriteLine("move \"" + _file2 + "\" \"\"");
            //sw.WriteLine("echo \"Press any key to run E Updater\"");
            //sw.WriteLine("pause");
            sw.WriteLine("start \"\" \"E Updater.exe\"");
            sw.Flush();
            sw.Close();
            //运行批处理命令
            Process process = new Process();
            process.StartInfo.FileName = _bat;
            process.Start();
            //退出EU
            Process.GetCurrentProcess().CloseMainWindow();
        }
        private void Btn_EWInstall_Click(object sender, RoutedEventArgs e)
        {
            if (EWNewVer != null)
            {
                Install("E Writer " + EWNewVer + ".zip", Settings.Default._EW, Settings.Default._EWDownload);
            }
            else
            {
                ShowMessage("版本检测失败，请刷新后再试");
            }
        }
        private void Btn_EPInstall_Click(object sender, RoutedEventArgs e)
        {
            if (EPNewVer != null)
            {
                Install("E Player " + EPNewVer + ".zip", Settings.Default._EP, Settings.Default._EPDownload);
            }
            else
            {
                ShowMessage("版本检测失败，请刷新后再试");
            }
        }
        private void Btn_ENInstall_Click(object sender, RoutedEventArgs e)
        {
            if (ENNewVer != null)
            {
                Install("E Number " + ENNewVer + ".zip", Settings.Default._EN, Settings.Default._ENDownload);
}
            else
            {
                ShowMessage("版本检测失败，请刷新后再试");
            }
        }
        private void Btn_EUUninstall_Click(object sender, RoutedEventArgs e)
        {
            //Uninstall("EU", Settings.Default._EU);
        }
        private void Btn_EWUninstall_Click(object sender, RoutedEventArgs e)
        {
            Uninstall("E Writer.exe", Settings.Default._EW);
        }
        private void Btn_EPUninstall_Click(object sender, RoutedEventArgs e)
        {
            Uninstall("E Player.exe", Settings.Default._EP);
        }
        private void Btn_ENUninstall_Click(object sender, RoutedEventArgs e)
        {
            Uninstall("E Number.exe", Settings.Default._EN);
        }
        private void Btn_EURun_Click(object sender, RoutedEventArgs e)
        {
            Process.GetCurrentProcess().CloseMainWindow();
        }
        private void Btn_EWRun_Click(object sender, RoutedEventArgs e)
        {
            string path = Settings.Default._EW + "\\E Writer.exe";
            if (IsRunning(path))
            {
                Stop(path);
            }
            else
            {
                Start(path);
            }
            RefreshAllAppsIsRunning();
        }
        private void Btn_EPRun_Click(object sender, RoutedEventArgs e)
        {
            string path = Settings.Default._EP + "\\E Player.exe";
            if (IsRunning(path))
            {
                Stop(path);
            }
            else
            {
                Start(path);
            }
            RefreshAllAppsIsRunning();
        }
        private void Btn_ENRun_Click(object sender, RoutedEventArgs e)
        {
            string path = Settings.Default._EN + "\\E Number.exe";
            if (IsRunning(path))
            {
                Stop(path);
            }
            else
            {
                Start(path);
            }
            RefreshAllAppsIsRunning();
        }
        private void Lbl_EU_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", "http://estar.zone/introduction/e-updater/");
        }
        private void Lbl_EW_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", "http://estar.zone/introduction/e-writer/");
        }
        private void Lbl_EP_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", "http://estar.zone/introduction/e-player/");
        }
        private void Lbl_EN_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", "http://estar.zone/introduction/e-number/");
        }
        #endregion

        private enum InstallState
        {
            未指定安装目录,
            未找到可执行文件,
            已安装旧版本,
            已安装最新版本,
        }
    }
}
