using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Diagnostics;
using E.Class;

namespace E.Writer
{
    /// <summary>
    /// HelpWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HelpWindow : Window
    {
        #region 属性
        //公有属性
        /// <summary>
        /// 上级窗口
        /// </summary>
        public MainWindow BaseWindow { get; private set; }

        //私有属性
        /// <summary>
        /// 语言列表
        /// </summary>
        private List<CategoryInfo> LanguageCategorys { get; set; } = new List<CategoryInfo>();
        #endregion

        #region 方法
        //构造器
        public HelpWindow(MainWindow ow)
        {
            BaseWindow = ow;
            InitializeComponent();
        }

        //刷新
        /// <summary>
        /// 刷新偏好设置
        /// </summary>
        private void RefreshPreferences()
        {
            //启动时显示运行信息
            ShowRunInfo.IsChecked = Properties.User.Default.isShowRunInfo;
            //自动开书
            AutoOpenBook.IsChecked = Properties.User.Default.isAutoOpenBook;
            //自动书名号
            AutoBrackets.IsChecked = Properties.User.Default.isAutoBrackets;
            //自动补全
            AutoCompletion.IsChecked = Properties.User.Default.isAutoCompletion;
            //自动缩进
            AutoIndentation.IsChecked = Properties.User.Default.isAutoIndentation;
            //缩进数
            AutoIndentations.Text = Properties.User.Default.autoIndentations.ToString();
            //缩进数可编辑性
            if ((bool)AutoIndentation.IsChecked) { AutoIndentations.IsEnabled = true; }
            else { AutoIndentations.IsEnabled = false; }
            //定时自动保存
            AutoSaveEvery.IsChecked = Properties.User.Default.isAutoSaveEvery;
            //定时自动保存时间间隔
            AutoSaveTime.Text = Properties.User.Default.autoSaveMinute.ToString();
            //时间间隔可编辑性
            if ((bool)AutoSaveEvery.IsChecked) { AutoSaveTime.IsEnabled = true; }
            else { AutoSaveTime.IsEnabled = false; }
            //切换自动保存
            AutoSaveWhenSwitch.IsChecked = Properties.User.Default.isAutoSaveWhenSwitch;
            //定时自动备份
            AutoBackup.IsChecked = Properties.User.Default.isAutoBackup;
            //自动备份时间间隔
            AutoBackupTime.Text = Properties.User.Default.autoBackupMinute.ToString();
            //自动备份可编辑性
            if ((bool)AutoBackup.IsChecked) { AutoBackupTime.IsEnabled = true; }
            else { AutoBackupTime.IsEnabled = false; }
            AutoBackup.ToolTip = "备份位置：" + System.Windows.Forms.Application.StartupPath + @"\" + Properties.User.Default.BackupDir;
            //滚动条
            ShowScrollBar.IsChecked = Properties.User.Default.isShowScrollBar;

            //创建语言选项
            SetLanguage(Properties.User.Default.language);
            //获取、设置主题选项
            SetThemeItem(Properties.User.Default.ThemePath);
            //获取文章字体信息
            TextSize.Text = Properties.User.Default.fontSize.ToString();
            SetFontsItem(Properties.User.Default.fontName);
        }

        //载入
        /// <summary>
        /// 获取主题选项
        /// </summary>
        private void LoadThemeItem()
        {
            Skins.Items.Clear();
            foreach (TextBlock item in BaseWindow.Themes)
            {
                TextBlock theme = new TextBlock()
                {
                    Text = item.Text,
                    ToolTip = item.ToolTip
                };
                Skins.Items.Add(theme);
            }
        }
        /// <summary>
        /// 创建语言选项
        /// </summary>
        private void LoadLanguage()
        {
            LanguageCategorys.Clear();
            CategoryInfo zh_CN = new CategoryInfo() { Name = "中文（默认）", Value = "zh_CN" };
            LanguageCategorys.Add(zh_CN);
            CategoryInfo en_US = new CategoryInfo() { Name = "English", Value = "en_US" };
            LanguageCategorys.Add(en_US);
            //绑定数据，真正的赋值
            CBSelectedLanguage.ItemsSource = LanguageCategorys;
            //指定显示的内容
            CBSelectedLanguage.DisplayMemberPath = "Name";
            //指定选中后的能够获取到的内容
            CBSelectedLanguage.SelectedValuePath = "Value";
        }
        /// <summary>
        /// 获取字体选项
        /// </summary>
        private void LoadFontsItem()
        {
            CBFonts.Items.Clear();
            foreach (FontFamily font in Fonts.SystemFontFamilies)
            {
                //动态的添加一个ListViewItem项
                TextBlock label = new TextBlock
                {
                    Text = font.Source,
                    FontFamily = font
                };
                CBFonts.Items.Add(label);
            }
        }

        //设置
        /// <summary>
        /// 设置主题选项
        /// </summary>
        /// <param name="_theme">主题路径</param>
        private void SetThemeItem(string _theme)
        {
            foreach (TextBlock item in Skins.Items)
            {
                if (item.ToolTip.ToString() == _theme)
                {
                    Skins.SelectedItem = item;
                    break;
                }
            }
        }
        /// <summary>
        /// 设置语言选项
        /// </summary>
        /// <param name="language">语言简拼</param>
        private void SetLanguage(string language)
        {
            foreach (CategoryInfo ci in LanguageCategorys)
            {
                if (ci.Value == language)
                {
                    CBSelectedLanguage.SelectedItem = ci;
                    break;
                }
            }
        }
        /// <summary>
        /// 设置字体选项
        /// </summary>
        private void SetFontsItem(string fontName)
        {
            foreach (TextBlock item in CBFonts.Items)
            {
                if (item.Text == fontName)
                {
                    CBFonts.SelectedItem = item;
                    break;
                }
            }
        }

        //创建
        /// <summary>
        /// 创建计时器
        /// </summary>
        private void CreateTimer()
        {
            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(0.01)
            };
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        //显示
        /// <summary>
        /// 显示软件信息
        /// </summary>
        private void ShowAppInfo()
        {
            ThisName.Content = BaseWindow.AppInfo.Name;
            Description.Content = BaseWindow.AppInfo.Description;
            Developer.Content = BaseWindow.AppInfo.Company;
            Version.Content = BaseWindow.AppInfo.Version;
            UpdateNote.Text = BaseWindow.AppInfo.UpdateNote;
            HomePage.Text = BaseWindow.AppInfo.HomePage;
            DownloadPage.Text = BaseWindow.AppInfo.DownloadPage;
            InfoPage.Text = BaseWindow.AppInfo.InfoPage;
            GitHubPage.Text = BaseWindow.AppInfo.GitHubPage;
            QQGroup.Text = BaseWindow.AppInfo.QQGroup;
            HomePage.Text = BaseWindow.AppInfo.HomePage;
            BitCoinAddress.Text = BaseWindow.AppInfo.BitCoinAddress;
        }
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="resourceName">资源名</param>
        private void ShowMessage(string resourceName)
        {
            Message.Opacity = 1;
            Message.Content = FindResource(resourceName);
        }
        #endregion

        #region 事件
        //窗口事件
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowAppInfo();
            LoadLanguage();
            LoadThemeItem();
            LoadFontsItem();
            RefreshPreferences();
            CreateTimer();
            ShowMessage("已载入");
        }

        //偏好设置页事件
        private void ShowRunInfo_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isShowRunInfo = true;
            BaseWindow.SavePreferences();
            //显示消息
            ShowMessage("已更改");
        }
        private void ShowRunInfo_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isShowRunInfo = false;
            BaseWindow.SavePreferences();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoOpenBook_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoOpenBook = true;
            BaseWindow.SavePreferences();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoOpenBook_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoOpenBook = false;
            BaseWindow.SavePreferences();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoBrackets_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoBrackets = true;
            BaseWindow.SavePreferences();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoBrackets_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoBrackets = false;
            BaseWindow.SavePreferences();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoCompletion_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoCompletion = true;
            BaseWindow.SavePreferences();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoCompletion_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoCompletion = false;
            BaseWindow.SavePreferences();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoIndentation_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoIndentation = true;
            BaseWindow.SavePreferences();
            AutoIndentations.IsEnabled = true;
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoIndentation_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoIndentation = false;
            BaseWindow.SavePreferences();
            AutoIndentations.IsEnabled = false;
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoIndentations_KeyUp(object sender, KeyEventArgs e)
        {
            if (AutoIndentations.Text != "")
            {
                try
                {
                    int t = int.Parse(AutoIndentations.Text);
                    if (t > 0 && t < 1000)
                    {
                        Properties.User.Default.autoIndentations = t;
                        BaseWindow.SavePreferences();
                        //显示消息
                        ShowMessage("已更改");
                    }
                    else
                    {
                        AutoIndentations.Text = Properties.User.Default.autoIndentations.ToString();
                        ShowMessage("输入1~999整数");
                    }
                }
                catch (Exception)
                {
                    AutoIndentations.Text = Properties.User.Default.autoIndentations.ToString();
                    ShowMessage("输入整数");
                }
            }
        }
        private void AutoSaveWhenSwitch_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoSaveWhenSwitch = true;
            BaseWindow.SavePreferences();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoSaveWhenSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoSaveWhenSwitch = false;
            BaseWindow.SavePreferences();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoSaveEvery_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoSaveEvery = true;
            BaseWindow.SavePreferences();
            AutoSaveTime.IsEnabled = true;
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoSaveEvery_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoSaveEvery = false;
            BaseWindow.SavePreferences();
            AutoSaveTime.IsEnabled = false;
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoSaveTime_KeyUp(object sender, KeyEventArgs e)
        {
            if (AutoSaveTime.Text != "")
            {
                try
                {
                    int t = int.Parse(AutoSaveTime.Text);
                    if (t > 0 && t < 1000)
                    {
                        TimeSpan ts = TimeSpan.FromMinutes(t);
                        Properties.User.Default.autoSaveMinute = t;
                        BaseWindow.SavePreferences();
                        BaseWindow.AutoSaveTimer.Interval = ts;
                        //显示消息
                        ShowMessage("已更改");
                    }
                    else
                    {
                        AutoSaveTime.Text = Properties.User.Default.autoSaveMinute.ToString();
                        ShowMessage("输入1~999整数");
                    }
                }
                catch (Exception)
                {
                    AutoSaveTime.Text = Properties.User.Default.autoSaveMinute.ToString();
                    ShowMessage("输入整数");
                }
            }
        }
        private void AutoBackup_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoBackup = true;
            BaseWindow.SavePreferences();
            AutoBackupTime.IsEnabled = true;
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoBackup_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoBackup = false;
            BaseWindow.SavePreferences();
            AutoBackupTime.IsEnabled = false;
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoBackupTime_KeyUp(object sender, KeyEventArgs e)
        {
            if (AutoBackupTime.Text != "")
            {
                try
                {
                    int t = int.Parse(AutoBackupTime.Text);
                    if (t > 0 && t < 1000)
                    {
                        TimeSpan ts = TimeSpan.FromMinutes(t);
                        Properties.User.Default.autoBackupMinute = t;
                        BaseWindow.SavePreferences();
                        BaseWindow.AutoBackupTimer.Interval = ts;
                        //显示消息
                        ShowMessage("已更改");
                    }
                    else
                    {
                        AutoBackupTime.Text = Properties.User.Default.autoBackupMinute.ToString();
                        ShowMessage("输入1~999整数");
                    }
                }
                catch (Exception)
                {
                    AutoBackupTime.Text = Properties.User.Default.autoBackupMinute.ToString();
                    ShowMessage("输入整数");
                }
            }
        }
        private void ShowScrollBar_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isShowScrollBar = true;
            BaseWindow.SavePreferences();
            BaseWindow.FileContent.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //显示消息
            ShowMessage("已更改");
        }
        private void ShowScrollBar_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isShowScrollBar = false;
            BaseWindow.SavePreferences();
            BaseWindow.FileContent.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //显示消息
            ShowMessage("已更改");
        }
        private void Skins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Skins.SelectedItem != null)
            {
                TextBlock item = Skins.SelectedItem as TextBlock;
                string tmp = item.ToolTip.ToString();
                if (File.Exists(tmp))
                {
                    //调用方法
                    BaseWindow.SetTheme(tmp);
                    //储存更改
                    Properties.User.Default.ThemePath = tmp;
                    BaseWindow.SavePreferences();
                    //显示消息
                    ShowMessage("已更改");
                }
                else
                {
                    Skins.Items.Remove(Skins.SelectedItem);
                    MessageBox.Show("该主题的配置文件不存在");
                }
            }
        }
        private void CBSelectedLanguage_SelectionChanged(object sender, RoutedEventArgs e)
        {
            object selectedName = CBSelectedLanguage.SelectedValue;
            if (selectedName != null)
            {
                string langName = selectedName.ToString();
                //根据本地语言来进行本地化,不过这里上不到
                //CultureInfo currentCultureInfo = CultureInfo.CurrentCulture;
                ResourceDictionary langRd = null;
                try
                {
                    //根据名字载入语言文件
                    if (langName == "zh_CN")
                    {
                        langRd = Application.LoadComponent(new Uri(@"语言/zh_CN.xaml", UriKind.Relative)) as ResourceDictionary;
                    }
                    else
                    {
                        langRd = Application.LoadComponent(new Uri(@"语言/" + langName + ".xaml", UriKind.Relative)) as ResourceDictionary;
                    }
                }
                catch (Exception e2)
                { MessageBox.Show(e2.Message); }

                if (langRd != null)
                {
                    //本窗口更改语言，如果已使用其他语言,先清空
                    if (Resources.MergedDictionaries.Count > 0)
                    { Resources.MergedDictionaries.Clear(); }
                    Resources.MergedDictionaries.Add(langRd);
                    //主窗口更改语言
                    if (BaseWindow.Resources.MergedDictionaries.Count > 0)
                    { BaseWindow.Resources.MergedDictionaries.Clear(); }
                    BaseWindow.Resources.MergedDictionaries.Add(langRd);
                    //保存设置
                    Properties.User.Default.language = langName;
                    BaseWindow.SavePreferences();
                }
            }
        }
        private void TextSize_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextSize.Text != "")
            {
                try
                {
                    int i = int.Parse(TextSize.Text);
                    if (i > 0 && i < 1000)
                    {
                        Properties.User.Default.fontSize = i;
                        BaseWindow.SavePreferences();
                        BaseWindow.FileContent.FontSize = i;
                        ShowMessage("已更改");
                    }
                    else
                    {
                        TextSize.Text = Properties.User.Default.fontSize.ToString();
                        ShowMessage("输入1~999整数");
                    }
                }
                catch (Exception)
                {
                    TextSize.Text = Properties.User.Default.fontSize.ToString();
                    ShowMessage("输入整数");
                }
            }
        }
        private void CBFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBlock item = CBFonts.SelectedItem as TextBlock;
            //调用方法
            BaseWindow.SetFont(item.Text);
            //储存更改
            Properties.User.Default.fontName = item.Text;
            BaseWindow.SavePreferences();
            //显示消息
            ShowMessage("已更改");
        }
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            BaseWindow.ResetPreferences();
            ShowMessage("已重置");
        }

        //软件信息页事件
        private void CheckNew_Click(object sender, RoutedEventArgs e)
        { Process.Start("explorer.exe", BaseWindow.AppInfo.InfoPage); }
        private void ClearRunInfo_Click(object sender, RoutedEventArgs e)
        {
            BaseWindow.ClearRunInfo();
        }

        //相关链接页事件
        private void HomePage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", BaseWindow.AppInfo.HomePage); }
        private void InfoPage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", BaseWindow.AppInfo.InfoPage); }
        private void DownloadPage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", BaseWindow.AppInfo.DownloadPage); }
        private void GitHubPage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", BaseWindow.AppInfo.GitHubPage); }
        private void QQGroup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", Properties.Settings.Default.QQGroupLink); }
        private void BitCoinAddress_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", BaseWindow.AppInfo.BitCoinAddress); }

        //其它事件
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Message.Opacity > 0)
            {
                Message.Opacity -= 0.01;
            }
        }
        #endregion
    }
}
