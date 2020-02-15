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
using System.ComponentModel;

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

        private List<AppInfo> appInfos = new List<AppInfo>();
        private string downloadingFile = "";
        private string downloadingMessage = "";
        private string installFolder = "";
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
        private void LoadAppsInfo()
        {
            appInfos.Clear();
            appInfos.Add(AppInfo);
            appInfos.Add(new AppInfo("E Writer"));
            appInfos.Add(new AppInfo("E Player"));
            appInfos.Add(new AppInfo("E Number"));
            appInfos.Add(new AppInfo("E Role"));

            PanApps.Children.Clear();
            foreach (AppInfo item in appInfos)
            {
                AppInfoItem aii = new AppInfoItem();
                aii.AppInfo = item;
                PanApps.Children.Add(aii);
            }

            //读取一个字符串，并加入播放列表
            if (!string.IsNullOrEmpty(Settings.Default.Paths))
            {
                string[] paths = Regex.Split(Settings.Default.Paths, ",");
                foreach (string item in paths)
                {
                    if (!File.Exists(item)) continue;

                    string name = Path.GetFileNameWithoutExtension(item);
                    AppInfo ai = appInfos.Find(x => x.Name == name);
                    ai.SetFilePath(item);
                }
            }
        }

        //打开

        ///关闭

        //保存
        /// <summary>
        /// 保存应用设置
        /// </summary>
        private void SaveSettings()
        {
            Settings.Default.Save();
            ShowMessage(FindResource("已保存").ToString());
        }
        /// <summary>
        /// 保存应用信息
        /// </summary>
        private void SaveAppInfos()
        {
            Settings.Default.Paths = "";
            string paths = "";
            foreach (AppInfo item in appInfos)
            {
                paths += item.FilePath + ",";
            }
            paths = paths.TrimEnd(',');
            Settings.Default.Paths = paths;
        }

        ///创建

        ///添加

        ///移除

        ///清空

        ///删除

        //获取
        /// <summary>
        /// 获取下载链接
        /// </summary>
        /// <returns></returns>
        private void GetDownloadLinks()
        {
            if (NetHelper.IsOnLine())
            {
                ShowMessage(FindResource("网络连接成功").ToString());

                using (WebClient wc = new WebClient())
                {
                    //获取或设置用于向Internet资源的请求进行身份验证的网络凭据
                    wc.Credentials = CredentialCache.DefaultCredentials;
                    wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler(DownloadDataCompleted);
                    wc.DownloadDataAsync(new Uri(AppInfo.WikiPage));
                    ShowMessage("正在获取最新版本信息");
                }
            }
            else
            {
                ShowMessage("未连接网络");
                ShowMessage(FindResource("网络连接失败").ToString());
                return;
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
            string str = AppInfo.Name + " " + AppInfo.VersionShort + downloadingMessage;
            Main.Title = str;
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshAppInfoItems()
        {
            for (int i = 0; i < PanApps.Children.Count; i++)
            {
                AppInfoItem aii = (AppInfoItem)PanApps.Children[i];
                if (aii != null)
                {
                    aii.Refresh();
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

        //其它
        /// <summary>
        /// 卸载
        /// </summary>
        /// <param name="name"></param>
        /// <param name="folder"></param>
        public bool UnInstall(AppInfo appInfo)
        {
            if (File.Exists(appInfo.FilePath))
            {
                if (!appInfo.IsRunning)
                {
                    string tip = string.Format("确定要卸载{0}吗？\n这将删除整个文件夹{1}，且无法恢复。", appInfo.Name, appInfo.FileFolder);
                    MessageBoxResult mbr = MessageBox.Show(tip, "", MessageBoxButton.YesNo);
                    switch (mbr)
                    {
                        case MessageBoxResult.Yes:
                            Directory.Delete(appInfo.FileFolder, true);
                            ShowMessage("已卸载 " + appInfo.Name);
                            return true;
                        default:
                            break;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 安装
        /// </summary>
        /// <param name="appName"></param>
        public bool Install(AppInfo appInfo)
        {
            if (string.IsNullOrEmpty(appInfo.DownloadLink))
            {
                return false;
            }

            if (appInfo.DownloadVersion == null)
            {
                return false;
            }

            //指定安装文件夹
            string folder = FolderHelper.ChooseFolder();
            if (!Directory.Exists(folder))
            {
                return false;
            }
            installFolder = folder + "\\" + appInfo.Name;
            foreach (AppInfo item in appInfos)
            {
                if (item == appInfo)
                {
                    item.SetFilePath(installFolder + "\\" + appInfo.Name + ".exe");
                }
            }

            //下载文件
            string fileName = appInfo.Name + " " + appInfo.DownloadVersion + ".zip";
            string filePath = AppInfo.DownloadFolder + "\\" + fileName;
            Download(appInfo.DownloadLink, filePath);
            return true;
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="appname"></param>
        public void Update(AppInfo appInfo)
        {
            Install(appInfo);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="downloadLink">下载路径</param>
        /// <param name="filePath">保存名称</param>
        public void Download(string downloadLink, string filePath)
        {
            //确保下载文件夹存在
            if (!Directory.Exists(AppInfo.DownloadFolder))
            { Directory.CreateDirectory(AppInfo.DownloadFolder); }

            if (File.Exists(filePath))
            {
                MessageBoxResult mbr = MessageBox.Show("下载文件夹内含有同名文件，替换此文件？", "", MessageBoxButton.YesNo);
                switch (mbr)
                {
                    case MessageBoxResult.Yes:
                        break;
                    case MessageBoxResult.No:
                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        filePath = filePath.Replace(fileName, fileName + " (1)");
                        break;
                    default:
                        break;
                }
            }

            using (WebClient wc = new WebClient())
            {
                downloadingFile = filePath;
                wc.DownloadFileAsync(new Uri(downloadLink), filePath);
                wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);

                downloadingMessage = " 正在下载安装包 0%";
                ShowMessage(downloadingMessage);
                RefreshTitle();
            }
        }
        /// <summary>  
        /// 解压zip格式的文件。  
        /// </summary>  
        /// <param name="zipFile">压缩文件路径</param>  
        /// <param name="targetDir">解压文件存放路径,为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹</param>  
        /// <param name="err">出错信息</param>  
        /// <returns>解压是否成功</returns>  
        private void UnZip(string zipFile, string targetDir)
        {
            //检查错误
            if (zipFile == string.Empty)
            {
                return;
            }
            if (!File.Exists(zipFile))
            {
                return;
            }

            //解压文件夹为空时默认在同级目录创建压缩文件同名文件夹  
            if (targetDir == string.Empty)
            { targetDir = zipFile.Replace(Path.GetFileName(zipFile), Path.GetFileNameWithoutExtension(zipFile)); }

            if (!targetDir.EndsWith("\\"))
            { targetDir += "\\"; }

            if (!Directory.Exists(targetDir))
            { Directory.CreateDirectory(targetDir); }

            using (ZipInputStream zis = new ZipInputStream(File.OpenRead(zipFile)))
            {
                ZipEntry ze;
                while ((ze = zis.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(ze.Name);
                    string fileName = Path.GetFileName(ze.Name);
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(targetDir + directoryName);
                    }
                    if (!directoryName.EndsWith("\\"))
                    {
                    }
                    if (fileName != string.Empty)
                    {
                        FileStream streamWriter = File.Create(targetDir + ze.Name);
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = zis.Read(data, 0, data.Length);
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
        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    ShowMessage("网络错误，无法下载文件");
                    return;
                }
                if (e.Cancelled == true)
                {
                    ShowMessage("操作已取消");
                    return;
                }
                if (!File.Exists(downloadingFile))
                {
                    ShowMessage("下载文件不存在");
                    return;
                }

                string name = Path.GetFileNameWithoutExtension(downloadingFile);
                ShowMessage("已下载文件，正在安装 " + name);
                //解压文件
                if (name == "E Updater")
                {
                    UnZip(downloadingFile, AppInfo.TempFolder);

                    string _bat = "Update.bat";
                    //获取解压后所有文件的路径
                    string _file1 = AppInfo.TempFolder + "\\E Updater.exe";
                    //string _file2 = AppInfo.TempFolder + "\\ICSharpCode.SharpZipLib.dll";
                    //创建批处理命令
                    FileStream fs = new FileStream(_bat, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine("@echo off");
                    sw.WriteLine("move \"" + _file1 + "\" \"\"");
                    //sw.WriteLine("move \"" + _file2 + "\" \"\"");
                    //sw.WriteLine("echo \"Press any key to run E Updater\"");
                    //sw.WriteLine("pause");
                    sw.WriteLine("start \"\" \"E Updater.exe\"");
                    sw.Flush();
                    sw.Close();
                    //运行批处理命令
                    Process.Start(_bat);
                    //退出EU
                    Close();
                }
                else
                {
                    if (string.IsNullOrEmpty(installFolder))
                    {
                        ShowMessage("未指定安装文件夹");
                    }
                    else
                    {
                        if (!Directory.Exists(installFolder))
                        {
                            Directory.CreateDirectory(installFolder);
                        }
                        UnZip(downloadingFile, installFolder);
                        ShowMessage("已安装 " + name);
                    }
                }
                RefreshAppInfoItems();

                downloadingFile = "";
                installFolder = "";
                downloadingMessage = "";
                RefreshTitle();
            }
            catch
            {
                ShowMessage("未知错误，无法下载文件");
            }
        }
        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            downloadingMessage = " 正在下载安装包 " + e.ProgressPercentage.ToString() + "%";
            ShowMessage(downloadingMessage);
            RefreshTitle();
        }
        private void DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    ShowMessage("网络错误，无法获取最新版本信息");
                    return;
                }
                if (e.Cancelled == true)
                {
                    ShowMessage("操作已取消");
                    return;
                }
                if (e.Result == null)
                {
                    ShowMessage("未获取最新版本信息");
                    return;
                }

                byte[] pageData = e.Result;
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
                    AppInfo ai = appInfos.Find(x => x.Name == name);
                    ai.DownloadLink = m.Value;
                }
                ShowMessage("已获取最新版本信息");
                RefreshAppInfoItems();
            }
            catch
            {
                ShowMessage("未知错误，无法获取最新版本信息");
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
            LoadAppsInfo();

            //刷新
            RefreshAppInfo();
            RefreshTitle();
            RefreshAppInfoItems();

            //检查用户协议
            CheckUserAgreement();

            GetDownloadLinks();
        }
        private void Main_Closing(object sender, CancelEventArgs e)
        {
            SaveAppInfos();
            SaveSettings();
        }
        private void Main_GotFocus(object sender, RoutedEventArgs e)
        {
            GetDownloadLinks();
            RefreshAppInfoItems();
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
