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
using System.Text.RegularExpressions;
using System.ComponentModel;
using ICSharpCode.SharpZipLib.Zip;
using SharedProject;

using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Forms.Application;
using Button = System.Windows.Controls.Button;
using Label = System.Windows.Controls.Label;

namespace E.Updater
{
    public partial class MainWindow : EWindow
    {
        private List<AppInfo> AppInfos { get; set; } = new List<AppInfo>();

        private string currentApp = "";
        private string downloadingFile = "";
        private string downloadingMessage = "";
        private string installFolder = "";

        //构造
        public MainWindow()
        {
            InitializeComponent();
        }

        //载入
        private void LoadAppsInfo()
        {
            AppInfos.Clear();
            AppInfos.Add(AppInfo);
            AppInfos.Add(new AppInfo("E Writer"));
            AppInfos.Add(new AppInfo("E Player"));
            AppInfos.Add(new AppInfo("E Number"));
            AppInfos.Add(new AppInfo("E Role"));

            PanApps.Children.Clear();
            foreach (AppInfo item in AppInfos)
            {
                AppInfoItem aii = new AppInfoItem
                {
                    AppInfo = item
                };
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
                    AppInfo ai = AppInfos.Find(x => x.Name == name);
                    ai.FilePath = item;
                }
            }
        }

        //保存
        protected override void SaveSettings()
        {
            Settings.Default.Paths = "";
            string paths = "";
            foreach (AppInfo item in AppInfos)
            {
                paths += item.FilePath + ",";
            }
            paths = paths.TrimEnd(',');
            Settings.Default.Paths = paths;

            Settings.Default.Save();
            ShowMessage(FindResource("已保存").ToString());
        }

        //获取
        private void GetDownloadLinks()
        {
            if (NetHelper.IsOnLine())
            {
                ShowMessage(FindResource("网络连接成功").ToString());

                using WebClient wc = new WebClient
                {
                    //获取或设置用于向Internet资源的请求进行身份验证的网络凭据
                    Credentials = CredentialCache.DefaultCredentials
                };
                wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler(DownloadDataCompleted);
                wc.DownloadDataAsync(new Uri(AppInfo.WikiPage));
                ShowMessage("正在获取最新版本信息");
            }
            else
            {
                ShowMessage("未连接网络");
                ShowMessage(FindResource("网络连接失败").ToString());
                return;
            }
        }

        //设置
        protected override void SetMenuTool(MenuTool menu)
        {
            switch (menu)
            {
                case MenuTool.无:
                    BtnFile.Background = PanFile.Visibility == Visibility.Collapsed ? BrushBG01 : BrushBG02;
                    BtnSetting.Background = PanSetting.Visibility == Visibility.Collapsed ? BrushBG01 : BrushBG02;
                    BtnAbout.Background = PanAbout.Visibility == Visibility.Collapsed ? BrushBG01 : BrushBG02;
                    break;
                case MenuTool.文件:
                    PanFile.Visibility = Visibility.Visible;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.Background = BrushBG02;
                    BtnSetting.Background = BrushBG01;
                    BtnAbout.Background = BrushBG01;
                    break;
                case MenuTool.编辑:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Visible;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    break;
                case MenuTool.设置:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Visible;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.Background = BrushBG01;
                    BtnSetting.Background = BrushBG02;
                    BtnAbout.Background = BrushBG01;
                    break;
                case MenuTool.关于:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Visible;
                    BtnFile.Background = BrushBG01;
                    BtnSetting.Background = BrushBG01;
                    BtnAbout.Background = BrushBG02;
                    break;
                default:
                    break;
            }
            CurrentMenuTool = menu;
        }

        //重置
        protected override void ResetSettings()
        {
            Settings.Default.Reset();
            ShowMessage(FindResource("已重置").ToString());
        }

        //刷新
        protected override void RefreshAppInfo()
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
            TxtUpdateNote.Text = AppInfo.ReleaseNote;
        }
        protected override void RefreshTitle()
        {
            base.RefreshTitle();

            Title += downloadingMessage;
        }
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
        protected override void ShowMessage(string message, bool newBox = false)
        {
            ShowMessage(LblMessage, message, newBox);
        }

        //其它
        public bool UnInstall(AppInfo appInfo)
        {
            if (appInfo.IsExists)
            {
                if (appInfo.Name == "E Updater")
                {
                    string tip = string.Format("确定要卸载{0}吗？", appInfo.Name, appInfo.FileFolder);
                    MessageBoxResult mbr = MessageBox.Show(tip, "", MessageBoxButton.YesNo);
                    switch (mbr)
                    {
                        case MessageBoxResult.Yes:
                            string _bat = "UnInstall.bat";
                            //获取解压后所有文件的路径
                            string _file1 = "E Updater.exe";
                            string _file2 = AppInfo.DownloadFolder;
                            string _file3 = AppInfo.ThemeFolder;
                            string _file4 = AppInfo.TempFolder;
                            //创建批处理命令
                            FileStream fs = new FileStream(_bat, FileMode.Create, FileAccess.Write);
                            StreamWriter sw = new StreamWriter(fs);
                            sw.WriteLine("@echo off");
                            sw.WriteLine("del /f /s /q " + "\"" + _file1 + "\"");
                            sw.WriteLine("rd /s /q " + _file2);
                            sw.WriteLine("rd /s /q " + _file3);
                            sw.WriteLine("rd /s /q " + _file4);
                            sw.Flush();
                            sw.Close();
                            //退出EU
                            Close();
                            //运行批处理命令
                            Process.Start(_bat);
                            return true;
                        default:
                            break;
                    }
                }
                else
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
                    else
                    {
                        ShowMessage("请先关闭" + appInfo.Name + "再卸载");
                    }
                }
            }
            return false;
        }
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
            string folder = ChooseFolder("请选择安装目录");
            if (!Directory.Exists(folder))
            {
                return false;
            }
            currentApp = appInfo.Name;
            installFolder = folder + "\\" + currentApp;

            //下载文件
            string fileName = currentApp + " " + appInfo.DownloadVersion + ".zip";
            string filePath = AppInfo.DownloadFolder + "\\" + fileName;
            Download(appInfo.DownloadLink, filePath);
            return true;
        }
        public bool Update(AppInfo appInfo)
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
            installFolder = appInfo.FileFolder;

            //下载文件
            string fileName = appInfo.Name + " " + appInfo.DownloadVersion + ".zip";
            string filePath = AppInfo.DownloadFolder + "\\" + fileName;
            Download(appInfo.DownloadLink, filePath);
            return true;
        }
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

            using WebClient wc = new WebClient();
            downloadingFile = filePath;
            wc.DownloadFileAsync(new Uri(downloadLink), filePath);
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);

            downloadingMessage = " 正在下载安装包 0%";
            ShowMessage(downloadingMessage);
            RefreshTitle();
        }
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
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            int size = zis.Read(data, 0, data.Length);
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

        //主窗口
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            //载入
            LoadLanguageItems(CbbLanguages);
            LoadThemeItems(CbbThemes);
            LoadAppsInfo();

            //刷新
            RefreshAppInfo();
            RefreshTitle();
            RefreshAppInfoItems();

            //检查用户是否同意用户协议
            if (CheckUserAgreement(Settings.Default.RunCount))
            {
                Settings.Default.RunCount += 1;
            }

            GetDownloadLinks();
        }
        private void Main_Closing(object sender, CancelEventArgs e)
        {
            SaveSettings();
        }
        private void Main_GotFocus(object sender, RoutedEventArgs e)
        {
            RefreshAppInfoItems();
        }
        protected override void EWindow_KeyUp(object sender, KeyEventArgs e)
        {
            //Ctrl+T 切换下个主题
            if (e.Key == Key.T && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            { SetNextTheme(CbbThemes, Settings.Default.Theme); }

            base.EWindow_KeyUp(sender, e);
        }
        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            downloadingMessage = " 正在下载安装包 " + e.ProgressPercentage.ToString() + "%";
            ShowMessage(downloadingMessage);
            RefreshTitle();
        }
        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    currentApp = "";
                    downloadingFile = "";
                    installFolder = "";
                    downloadingMessage = "";
                    RefreshTitle();
                    ShowMessage("网络错误，无法下载文件");
                    return;
                }
                if (e.Cancelled == true)
                {
                    currentApp = "";
                    downloadingFile = "";
                    installFolder = "";
                    downloadingMessage = "";
                    RefreshTitle();
                    ShowMessage("操作已取消");
                    return;
                }
                if (!File.Exists(downloadingFile))
                {
                    currentApp = "";
                    downloadingFile = "";
                    installFolder = "";
                    downloadingMessage = "";
                    RefreshTitle();
                    ShowMessage("下载文件不存在");
                    return;
                }

                string name = Path.GetFileNameWithoutExtension(downloadingFile);
                ShowMessage("已下载文件，正在安装 " + name);
                //解压文件
                if (name.Contains("E Updater"))
                {
                    UnZip(downloadingFile, AppInfo.TempFolder);

                    string _bat = "Update.bat";
                    //获取解压后所有文件的路径
                    string _file1 = AppInfo.TempFolder + "\\E Updater.exe";
                    string _file2 = AppInfo.TempFolder + "\\" + AppInfo.ThemeFolder;
                    //创建批处理命令
                    FileStream fs = new FileStream(_bat, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine("@echo off");
                    sw.WriteLine("rd /s /q " + AppInfo.ThemeFolder);
                    sw.WriteLine("move \"" + _file1 + "\" \"\"");
                    sw.WriteLine("move \"" + _file2 + "\" \"\"");
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
                        AppInfo ai = AppInfos.Find(x => x.Name == currentApp);
                        if (ai != null)
                        {
                            ai.FilePath = installFolder + "\\" + ai.Name + ".exe";
                        }
                        ShowMessage("已安装 " + name);
                    }
                }
            }
            catch
            {
                ShowMessage("未知错误，无法下载文件");
            }

            currentApp = "";
            downloadingFile = "";
            installFolder = "";
            downloadingMessage = "";
            RefreshTitle();
            RefreshAppInfoItems();
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
                    AppInfo ai = AppInfos.Find(x => x.Name == name);
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

        //工具栏
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
                    Settings.Default.Language = 0;
                }
            }
        }
        private void CbbThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbbThemes.SelectedItem != null)
            {
                ComboBoxItem cbi = CbbThemes.SelectedItem as ComboBoxItem;
                string content = cbi.Tag.ToString();
                if (content == "自定义")
                {
                    if (string.IsNullOrEmpty(Settings.Default.ThemeCustomize))
                    {
                        Settings.Default.ThemeCustomize = ThemeItems[0].Value;
                        Settings.Default.Save();
                    }
                    content = Settings.Default.ThemeCustomize;
                }

                if (string.IsNullOrEmpty(content))
                {
                    CbbThemes.Items.Remove(cbi);
                    //设为默认主题
                    Settings.Default.Theme = 0;
                }
                else
                {
                    ColorHelper.SetColors(Resources, content);
                }
                //立即刷新按钮样式
                SetMenuTool(CurrentMenuTool);
            }
        }

        //工作区
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetDownloadLinks();
        }
    }
}
