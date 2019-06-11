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

namespace E.Writer
{
    /// <summary>
    /// SetWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HelpWindow : Window
    {
        //获取父窗口
        public MainWindow Ow { get; private set; }

        //计时器
        private DispatcherTimer timer;
        //语言列表
        private List<CategoryInfo> categoryList = new List<CategoryInfo>();

        //构造器
        public HelpWindow(MainWindow ow)
        {
            Ow = ow;
            InitializeComponent();
        }
        
        //窗口载入事件
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //创建计时器
            CreateTimer();
            //获取软件信息
            ThisName.Content = MainWindow.ThisName;
            Description.Content = MainWindow.ThisDescription;
            Developer.Content = MainWindow.ThisDeveloper + "@" + MainWindow.ThisCompany;
            Version.Content = Ow.ThisVer;
            //载入更新日志
            Stream src = Application.GetResourceStream(new Uri("/Documents/更新日志.txt", UriKind.Relative)).Stream;
            string str = new StreamReader(src, Encoding.UTF8).ReadToEnd();
            UpdateLog.Text = str;


            ///获取偏好设置
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
            GetLanguage();
            SetLanguage(Properties.User.Default.language);
            //获取、设置主题选项
            GetThemeItem();
            SetThemeItem(Properties.User.Default.ThemePath);
            //获取文章字体信息
            TextSize.Text = Properties.User.Default.fontSize.ToString();
            GetFontsItem();
            SetFontsItem(Properties.User.Default.fontName);

            //显示消息
            ShowMessage("已载入");
        }

        //是否 启动时显示运行信息
        private void ShowRunInfo_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isShowRunInfo = true;
            Properties.User.Default.Save();
            //显示消息
            ShowMessage("已更改");
        }
        private void ShowRunInfo_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isShowRunInfo = false;
            Properties.User.Default.Save();
            //显示消息
            ShowMessage("已更改");
        }
        //是否 启动时自动打开上次的书籍
        private void AutoOpenBook_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoOpenBook = true;
            Properties.User.Default.Save();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoOpenBook_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoOpenBook = false;
            Properties.User.Default.Save();
            //显示消息
            ShowMessage("已更改");
        }
        //是否 创建书籍时自动添加书名号
        private void AutoBrackets_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoBrackets = true;
            Properties.User.Default.Save();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoBrackets_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoBrackets = false;
            Properties.User.Default.Save();
            //显示消息
            ShowMessage("已更改");
        }
        //是否 自动补全
        private void AutoCompletion_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoCompletion = true;
            Properties.User.Default.Save();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoCompletion_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoCompletion = false;
            Properties.User.Default.Save();
            //显示消息
            ShowMessage("已更改");
        }
        //是否 换行自动缩进
        private void AutoIndentation_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoIndentation = true;
            Properties.User.Default.Save();
            AutoIndentations.IsEnabled = true;
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoIndentation_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoIndentation = false;
            Properties.User.Default.Save();
            AutoIndentations.IsEnabled = false;
            //显示消息
            ShowMessage("已更改");
        }
        //设置缩进数
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
                        Properties.User.Default.Save();
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
        //是否 切换时自动保存
        private void AutoSaveWhenSwitch_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoSaveWhenSwitch = true;
            Properties.User.Default.Save();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoSaveWhenSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoSaveWhenSwitch = false;
            Properties.User.Default.Save();
            //显示消息
            ShowMessage("已更改");
        }
        //是否 设置间隔自动保存
        private void AutoSaveEvery_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoSaveEvery = true;
            Properties.User.Default.Save();
            AutoSaveTime.IsEnabled = true;
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoSaveEvery_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoSaveEvery = false;
            Properties.User.Default.Save();
            AutoSaveTime.IsEnabled = false;
            //显示消息
            ShowMessage("已更改");
        }
        //设置间隔时间
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
                        Properties.User.Default.Save();
                        Ow.timer1.Interval = ts;
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
        //是否 设置自动备份
        private void AutoBackup_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoBackup = true;
            Properties.User.Default.Save();
            AutoBackupTime.IsEnabled = true;
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoBackup_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoBackup = false;
            Properties.User.Default.Save();
            AutoBackupTime.IsEnabled = false;
            //显示消息
            ShowMessage("已更改");
        }
        //设置自动备份间隔时间
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
                        Properties.User.Default.Save();
                        Ow.timer2.Interval = ts;
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
        //是否 显示滚动条
        private void ShowScrollBar_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isShowScrollBar = true;
            Properties.User.Default.Save();
            Ow.FileContent.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //显示消息
            ShowMessage("已更改");
        }
        private void ShowScrollBar_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isShowScrollBar = false;
            Properties.User.Default.Save();
            Ow.FileContent.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //显示消息
            ShowMessage("已更改");
        }

        /// <summary>
        /// 更改了主题选项
        /// </summary>
        private void Skins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Skins.SelectedItem != null)
            {
                TextBlock item = Skins.SelectedItem as TextBlock;
                string tmp = item.ToolTip.ToString();
                if (File.Exists(tmp))
                {
                    //调用方法
                    Ow.SetTheme(tmp);
                    //储存更改
                    Properties.User.Default.ThemePath = tmp;
                    Properties.User.Default.Save();
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
        /// <summary>
        /// 获取主题选项
        /// </summary>
        private void GetThemeItem()
        {
            foreach (TextBlock item in Ow.themes)
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
        /// 更改主题选项
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
        /// 更改了语言选项
        /// </summary>
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
                    if (langName == "zh_CN")
                    {
                        //根据名字载入语言文件
                        langRd = Application.LoadComponent(new Uri(@"languages/zh_CN.xaml", UriKind.Relative)) as ResourceDictionary;
                    }
                    else
                    {
                        //根据名字载入语言文件
                        langRd = Application.LoadComponent(new Uri(@"languages/" + langName + ".xaml", UriKind.Relative)) as ResourceDictionary;
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
                    if (Ow.Resources.MergedDictionaries.Count > 0)
                    { Ow.Resources.MergedDictionaries.Clear(); }
                    Ow.Resources.MergedDictionaries.Add(langRd);
                    //保存设置
                    Properties.User.Default.language = langName;
                    Properties.User.Default.Save();
                }
            }
        }
        /// <summary>
        /// 创建语言选项
        /// </summary>
        private void GetLanguage()
        {
            CategoryInfo zh_CN = new CategoryInfo() { Name = "中文（默认）", Value = "zh_CN" };
            categoryList.Add(zh_CN);
            CategoryInfo en_US = new CategoryInfo() { Name = "English", Value = "en_US" };
            categoryList.Add(en_US);
            //绑定数据，真正的赋值
            CBSelectedLanguage.ItemsSource = categoryList;
            //指定显示的内容
            CBSelectedLanguage.DisplayMemberPath = "Name";
            //指定选中后的能够获取到的内容
            CBSelectedLanguage.SelectedValuePath = "Value";
        }
        /// <summary>
        /// 设置语言选项
        /// </summary>
        /// <param name="language">语言简拼</param>
        private void SetLanguage(string language)
        {
            foreach (CategoryInfo ci in categoryList)
            {
                if (ci.Value == language)
                {
                    CBSelectedLanguage.SelectedItem = ci;
                    break;
                }
            }
        }
        /// <summary>
        /// 设置字号
        /// </summary>
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
                        Properties.User.Default.Save();
                        Ow.FileContent.FontSize = i;
                        //显示消息
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
        /// <summary>
        /// 更改了字体选项
        /// </summary>
        private void CBFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBlock item = CBFonts.SelectedItem as TextBlock;
            //调用方法
            Ow.SetFont(item.Text);
            //储存更改
            Properties.User.Default.fontName = item.Text;
            Properties.User.Default.Save();
            //显示消息
            ShowMessage("已更改");
        }
        /// <summary>
        /// 获取字体选项
        /// </summary>
        private void GetFontsItem()
        {
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

        /// <summary>
        /// 重置设置
        /// </summary>
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.Reset();
            
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
            //滚动条
            ShowScrollBar.IsChecked = Properties.User.Default.isShowScrollBar;

            //语言
            SetLanguage(Properties.User.Default.language);
            //设置主题选项
            SetThemeItem(Properties.User.Default.ThemePath);
            //获取文章字体信息
            TextSize.Text = Properties.User.Default.fontSize.ToString();
            SetFontsItem(Properties.User.Default.fontName);

            //显示消息
            ShowMessage("已重置");
        }
        
        /// <summary>
        /// 创建计时器
        /// </summary>
        private void CreateTimer()
        {
            timer = new DispatcherTimer
            { Interval = TimeSpan.FromSeconds(0.01) };
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }
        /// <summary>
        /// 计时器，消息渐隐
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Message.Opacity > 0)
            {
                Message.Opacity -= 0.01;
            }
        }
        /// <summary>
        /// 检查更新
        /// </summary>
        private void CheckNew_Click(object sender, RoutedEventArgs e)
        { Process.Start("explorer.exe", "http://estar.zone/introduction/e-writer/"); }
        /// <summary>
        /// 清除运行信息
        /// </summary>
        private void ClearRunInfo_Click(object sender, RoutedEventArgs e)
        {
            Ow.ClearRunInfo();
        }
        ///打开网页
        private void HomeSite_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", "http://estar.zone"); }
        private void DownloadSite_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", "http://estar.zone/introduction/e-writer/"); }
        private void FeedbackSite_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", "https://github.com/HelloEStar/E-Writer"); }
        private void ForumSite_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", "https://github.com/HelloEStar/E-Writer/issues"); }
        private void QQGroup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", "http://jq.qq.com/?_wv=1027&k=5TQxcvR"); }
        
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="resourceName">资源名</param>
        public void ShowMessage(string resourceName)
        {
            Message.Opacity = 1;
            Message.Content = FindResource(resourceName);
        }

    }

    //定义ComboBox选项的类，存放Name和Value
    public struct CategoryInfo
    {
        public string Name{ get; set; }
        public string Value{ get; set; }
    }
}
