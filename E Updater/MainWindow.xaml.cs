using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using ICSharpCode.SharpZipLib.Zip;
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Forms.Application;
using Button = System.Windows.Controls.Button;
using Label = System.Windows.Controls.Label;
using Settings = E.Updater.Properties.Settings;
using SharedProject;
using System.Text.RegularExpressions;

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
        private AppInfo AppInfo { get; } = new AppInfo();

        /// <summary>
        /// 当前菜单
        /// </summary>
        private MenuTool CurrentMenuTool { get; set; } = MenuTool.文件;

        private Dictionary<string, string> DownLoadLinks = new Dictionary<string, string>();
        private Dictionary<string, string> AppFilePaths = new Dictionary<string, string>();
        #endregion

        #region 方法
        //构造
        /// <summary>
        /// 默认构造器
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        ///载入

        //打开
        /// <summary>
        /// 打开
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="path"></param>
        private void Run(string appName)
        {
            if (appName == "E Updater")
            {
            }
            else
            {
                string file = AppFilePaths[appName];
                if (IsRunning(file))
                {
                }
                else
                {
                    Process pr = new Process();
                    if (File.Exists(file))
                    {
                        pr.StartInfo.FileName = file;
                        pr.Start();
                    }
                    else
                    {
                        ShowMessage("可执行文件不存在");
                    }
                }
            }
        }

        //关闭
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="path"></param>
        private void Kill(string appName)
        {
            if (appName =="E Updater")
            {
                Process.GetCurrentProcess().CloseMainWindow();
            }
            else
            {
                string file = AppFilePaths[appName];
                if (IsRunning(file))
                {
                    Process[] ps = Process.GetProcesses();
                    foreach (Process p in ps)
                    {
                        if (p.ProcessName == appName && p.MainModule.FileName == file)
                        {
                            p.CloseMainWindow();
                            ShowMessage("已关闭" + appName);
                            break;
                        }
                    }
                }
                else
                {
                    ShowMessage("未运行" + appName);
                }
            }
        }

        //
        /// <summary>
        /// 保存应用设置
        /// </summary>
        private void SaveSettings()
        {
            Settings.Default.Save();
            ShowMessage(FindResource("已保存").ToString());
        }


        ///创建

        ///添加

        ///移除

        ///清空

        ///删除

        //获取
        /// <summary>
        /// 检测指定目录是否有可执行文件，并获取其版本信息
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="name"></param>
        private Version GetFileVersion(string path)
        {
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
        private Version GetLinkVersion(string url)
        {
            string expr = @"[0,65536].[0,65536].[0,65536].[0,65536]";
            MatchCollection mc = Regex.Matches(url, expr);
            if (mc.Count > 0)
            {
                Version ver = new Version(mc[0].Value);
                return ver;
            }
            else return null;
        }

        private string GetAppFoleder(string appName)
        {
            string path = AppFilePaths[appName];
            path = Path.GetDirectoryName(path);
            return path;
        }

        /// <summary>
        /// 浏览
        /// </summary>
        private string CreateFolder()
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog
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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void GetDownloadLinks()
        {
            try
            {
                WebClient wc = new WebClient
                {
                    //获取或设置用于向Internet资源的请求进行身份验证的网络凭据
                    Credentials = CredentialCache.DefaultCredentials
                };
                byte[] pageData = wc.DownloadData("https://github.com/HelloEStar/E.App/wiki");
                //如果获取网站页面采用的是GB2312，则使用这句  
                //string pageHtml = Encoding.Default.GetString(pageData);
                //如果获取网站页面采用的是UTF-8，则使用这句
                string pageHtml = Encoding.UTF8.GetString(pageData);
                string expr = @"(https://github.com/HelloEStar/E.App/files).*(zip)";
                MatchCollection mc = Regex.Matches(pageHtml, expr);
                foreach (Match m in mc)
                {
                    string name = "";
                    if (m.Value.Contains("pdater"))
                    {
                        name = "E Updater";
                    }
                    else if (m.Value.Contains("riter"))
                    {
                        name = "E Writer";
                    }
                    else if (m.Value.Contains("layer"))
                    {
                        name = "E Player";
                    }
                    else if (m.Value.Contains("umber"))
                    {
                        name = "E Number";
                    }
                    else if (m.Value.Contains("ole"))
                    {
                        name = "E Role";
                    }
                    DownLoadLinks.Add(name, m.Value);
                }
            }
            catch (WebException)
            {
                ShowMessage("网络错误");
            }
        }

        //设置
        /// <summary>
        /// 设置菜单
        /// </summary>
        /// <param name="menu"></param>
        private void SetMenuTool(MenuTool menu)
        {
            switch (menu)
            {
                case MenuTool.无:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    //BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.文件:
                    PanFile.Visibility = Visibility.Visible;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.BorderThickness = new Thickness(4, 0, 0, 0);
                    //BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.编辑:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Visible;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    //BtnEdit.BorderThickness = new Thickness(4, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.设置:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Visible;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    //BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(4, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.关于:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Visible;
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    //BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(4, 0, 0, 0);
                    break;
                default:
                    break;
            }
            CurrentMenuTool = menu;
        }
        /// <summary>
        /// 设置语言选项
        /// </summary>
        /// <param name="language">语言简拼</param>
        private void SetLanguage(int index)
        {
            Settings.Default.Language = index;
        }
        /// <summary>
        /// 设置主题选项
        /// </summary>
        /// <param name="themePath">主题路径</param>
        private void SetTheme(int index)
        {
            Settings.Default.Theme = index;
        }
        /// <summary>
        /// 切换下个主题显示
        /// </summary>
        private void SetNextTheme()
        {
            int index = Settings.Default.Theme;
            index++;
            if (index > CbbThemes.Items.Count - 1)
            {
                index = 0;
            }
            SetTheme(index);
        }

        //重置
        /// <summary>
        /// 重置应用设置
        /// </summary>
        private void ResetSettings()
        {
            Settings.Default.Reset();
            ShowMessage(FindResource("已重置").ToString());
        }

        ///选择

        //检查
        /// <summary>
        /// 用户是否同意
        /// </summary>
        /// <returns></returns>
        private bool IsUserAgree()
        {
            string str = AppInfo.UserAgreement + "\n\n你需要同意此协议才能使用本软件，是否同意？";
            MessageBoxResult result = MessageBox.Show(str, FindResource("用户协议").ToString(), MessageBoxButton.YesNo);
            return (result == MessageBoxResult.Yes);
        }
        /// <summary>
        /// 检查用户协议
        /// </summary>
        private void CheckUserAgreement()
        {
            Settings.Default.RunCount += 1;
            if (Settings.Default.RunCount == 1)
            {
                if (!IsUserAgree())
                {
                    Settings.Default.RunCount = 0;
                    Close();
                }
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

        //刷新
        /// <summary>
        /// 刷新软件信息
        /// </summary>
        private void RefreshAppInfo()
        {
            TxtHomePage.Text = AppInfo.HomePage;
            TxtHomePage.ToolTip = AppInfo.HomePage;
            TxtGitHubPage.Text = AppInfo.GitHubPage;
            TxtGitHubPage.ToolTip = AppInfo.GitHubPage;
            TxtQQGroup.Text = AppInfo.QQGroupNumber;
            TxtQQGroup.ToolTip = AppInfo.QQGroupLink;
            TxtBitCoinAddress.Text = AppInfo.BitCoinAddress;
            TxtBitCoinAddress.ToolTip = AppInfo.BitCoinAddress;

            TxtThisName.Text = AppInfo.Name;
            TxtDescription.Text = AppInfo.Description;
            TxtDeveloper.Text = AppInfo.Company;
            TxtVersion.Text = AppInfo.Version.ToString();
            TxtUpdateNote.Text = AppInfo.UpdateNote;
        }
        /// <summary>
        /// 刷新主窗口标题
        /// </summary>
        public void RefreshTitle()
        {
            string str = AppInfo.Name + " " + AppInfo.VersionShort;
            Main.Title = str;
        }
        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="isCheckNew"></param>
        private void Refresh(bool isCheckNew = false)
        {
            if (isCheckNew)
            {
                if (NetHelper.IsOnLine())
                {
                    ShowMessage(FindResource("网络连接成功").ToString());
                }
                else
                {
                    ShowMessage(FindResource("网络连接失败").ToString());
                }
            }
        }
        //显示
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="resourceName">资源名</param>
        /// <param name="newBox">是否弹出对话框</param>
        private void ShowMessage(string message, bool newBox = false)
        {
            MessageHelper.ShowMessage(LblMessage, message, newBox);
        }

        //其它
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">文件路径</param>
        /// <param name="fileName">保存的文件名</param>
        private void Download(string url, string fileName)
        {
            //尝试下载
            try
            {
                WebClient webClient = new WebClient();
                //确保下载文件夹存在
                if (!Directory.Exists(AppInfo.DownloadFolder))
                { Directory.CreateDirectory(AppInfo.DownloadFolder); }
                //设置存储路径
                string address = AppInfo.DownloadFolder + @"\" + fileName;
                if (!File.Exists(address))
                {
                    //开始下载
                    webClient.DownloadFile(new Uri(url), address);
                    ShowMessage(fileName + "下载成功");
                    if (MessageBox.Show(fileName + " 已完成下载，是否打开下载文件夹", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Process.Start("explorer.exe", AppInfo.DownloadFolder);
                    }
                }
                else
                {
                    if (MessageBox.Show("下载文件夹内含有同名文件，替换此文件？", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        //开始下载
                        webClient.DownloadFile(new Uri(url), address);
                        ShowMessage(fileName + "下载成功");
                        if (MessageBox.Show(fileName + " 已完成下载，是否打开下载文件夹", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            Process.Start("explorer.exe", AppInfo.DownloadFolder);
                        }
                    }
                }
            }
            catch (Exception)
            {
                ShowMessage(fileName + "下载失败");
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
                    {
                    }
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
        /// 卸载
        /// </summary>
        /// <param name="name"></param>
        /// <param name="folder"></param>
        public void UnInstall(string appname)
        {
            switch (appname)
            {
                case "E Updater":
                    break;
                default:
                    string file = AppFilePaths[appname];
                    string folder = GetAppFoleder(appname);
                    if (Directory.Exists(folder))
                    {
                        if (IsRunning(file))
                        {
                            ShowMessage("正在运行，请关闭");
                        }
                        else
                        {
                            Directory.Delete(folder, true);
                            Refresh();
                            ShowMessage(appname + "已卸载");
                        }
                    }
                    break;
            }
        }
        public void Install(string appname)
        {
            string link = DownLoadLinks[appname];
            string version = GetLinkVersion(link).ToString();
            string zipFileName = appname + version + ".zip";
            //下载压缩包
            Download(link, zipFileName);
            //解压
            UnZip(AppInfo.DownloadFolder + "\\" + zipFileName, AppInfo.TempFolder);
            switch (appname)
            {
                case "E Updater":
                    //eu
                    //创建的压缩文件名称
                    string _bat = "Update.bat";
                    //获取解压后所有文件的路径
                    string _file1 = AppInfo.TempFolder + "\\E Updater.exe";
                    string _file2 = AppInfo.TempFolder + "\\ICSharpCode.SharpZipLib.dll";
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
                    break;
                default:
                    string folder = CreateFolder();
                    if (Directory.Exists(folder))
                    {
                        ShowMessage(Path.GetFileNameWithoutExtension(appname) + "已安装", false);
                        Refresh();
                    }
                    break;
            }
        }

        //切换
        /// <summary>
        /// 切换工具面板
        /// </summary>
        private void SwitchMenuToolFile()
        {
            switch (CurrentMenuTool)
            {
                case MenuTool.文件:
                    //SetMenuTool(MenuTool.无);
                    break;
                default:
                    SetMenuTool(MenuTool.文件);
                    break;
            }
        }
        /// <summary>
        /// 切换编辑面板
        /// </summary>
        private void SwitchMenuToolEdit()
        {
            switch (CurrentMenuTool)
            {
                case MenuTool.编辑:
                    SetMenuTool(MenuTool.无);
                    break;
                default:
                    SetMenuTool(MenuTool.编辑);
                    break;
            }
        }
        /// <summary>
        /// 切换设置面板
        /// </summary>
        private void SwitchMenuToolSetting()
        {
            switch (CurrentMenuTool)
            {
                case MenuTool.设置:
                    //SetMenuTool(MenuTool.无);
                    break;
                default:
                    SetMenuTool(MenuTool.设置);
                    break;
            }
        }
        /// <summary>
        /// 切换关于面板
        /// </summary>
        private void SwitchMenuToolAbout()
        {
            switch (CurrentMenuTool)
            {
                case MenuTool.关于:
                    //SetMenuTool(MenuTool.无);
                    break;
                default:
                    SetMenuTool(MenuTool.关于);
                    break;
            }
        }
        #endregion 

        #region 事件
        //主窗口
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            //载入
            LanguageHelper.LoadLanguageItems(CbbLanguages);
            ThemeHelper.LoadThemeItems(CbbThemes);

            //刷新
            RefreshAppInfo();
            RefreshTitle();

            //检查用户协议
            CheckUserAgreement();

            Refresh(true);
        }
        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }
        private void Main_GotFocus(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            //Ctrl+T 切换下个主题
            if (e.Key == Key.T && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            { SetNextTheme(); }

            //关于菜单
            if (e.Key == Key.F1)
            { Process.Start("explorer.exe", AppInfo.HomePage); }
            else if (e.Key == Key.F2)
            { Process.Start("explorer.exe", AppInfo.GitHubPage); }
            else if (e.Key == Key.F3)
            { Process.Start("explorer.exe", AppInfo.QQGroupLink); }
        }

        //菜单栏
        private void BtnFile_Click(object sender, RoutedEventArgs e)
        {
            SwitchMenuToolFile();
        }
        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            SwitchMenuToolSetting();
        }
        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            SwitchMenuToolAbout();
        }

        //工具栏
        ///设置
        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
        private void BtnResetSettings_Click(object sender, RoutedEventArgs e)
        {
            ResetSettings();
        }
        private void CbbLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbbLanguages.SelectedItem != null)
            {
                ComboBoxItem cbi = CbbLanguages.SelectedItem as ComboBoxItem;
                if (cbi.Tag is ResourceDictionary rd)
                {
                    //主窗口更改语言
                    if (Resources.MergedDictionaries.Count > 0)
                    {
                        Resources.MergedDictionaries.Clear();
                    }
                    Resources.MergedDictionaries.Add(rd);
                }
                else
                {
                    CbbLanguages.Items.Remove(cbi);
                    //设为默认主题
                    SetLanguage(0);
                }
            }
        }
        private void CbbThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbbThemes.SelectedItem != null)
            {
                ComboBoxItem cbi = CbbThemes.SelectedItem as ComboBoxItem;
                string themePath = cbi.ToolTip.ToString();
                if (File.Exists(themePath))
                {
                    ColorHelper.SetColors(Resources, themePath);
                }
                else
                {
                    CbbThemes.Items.Remove(cbi);
                    //设为默认主题
                    SetTheme(0);
                }
            }
        }

        ///关于
        private void BtnBitCoinAddress_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetDataObject(TxtBitCoinAddress.Text, true);
            ShowMessage(FindResource("已复制").ToString());
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

        //工作区
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetDownloadLinks();
        }
        #endregion
    }
}
