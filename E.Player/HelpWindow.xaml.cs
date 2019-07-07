using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace E.Player
{
    /// <summary>
    /// HelpWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HelpWindow : Window
    {
        //获取父窗口
        public MainWindow Ow;
        //计时器
        public DispatcherTimer timer;
        //语言列表
        List<CategoryInfo> categoryList = new List<CategoryInfo>();
        //构造函数
        public HelpWindow(MainWindow ow)
        {
            Ow = ow;
            InitializeComponent();
        }

        //窗口载入时
        private void Help_Loaded(object sender, RoutedEventArgs e)
        {
            //创建计时器
            CreateTimer();

            //获取软件信息
            Name.Content = Ow.AppInfo.Name;
            Description.Content = Ow.AppInfo.Description;
            Developer.Content = Ow.AppInfo.Company;
            Version.Content = Ow.AppInfo.Version;
            //Developer.Content = MainWindow.company;
            //载入更新日志
            Stream src = Application.GetResourceStream(new Uri("/文档/更新日志.txt", UriKind.Relative)).Stream;
            string str = new StreamReader(src, Encoding.UTF8).ReadToEnd();
            UpdateLog.Text = str;

            //是否在切换视频时保持变换
            KeepTrans.IsChecked = Properties.User.Default.isKeepTrans;
            //是否在退出时保留播放列表
            SavePlaylist.IsChecked = Properties.User.Default.isSavePlaylist;
            //快进快退时间
            JumpTime.Text = Properties.User.Default.jumpTime.ToString();

            //创建语言选项
            GetLanguage();
            SetLanguage(Properties.User.Default.language);

            //显示消息
            ShowMessage("已载入");
        }
        
        //切换视频时是否保留变换
        private void KeepTrans_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isKeepTrans = true;
            Properties.User.Default.Save();
            //显示消息
            ShowMessage("已更改");
        }
        private void KeepTrans_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isKeepTrans = false;
            Properties.User.Default.Save();
            //显示消息
            ShowMessage("已更改");
        }
        //是否在退出时保留播放列表
        private void SavePlaylist_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isSavePlaylist = true;
            Properties.User.Default.Save();
            //显示消息
            ShowMessage("已更改");
        }
        private void SavePlaylist_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isSavePlaylist = false;
            Properties.User.Default.Save();
            //显示消息
            ShowMessage("已更改");
        }
        //更改快进快退时间
        private void JumpTime_KeyUp(object sender, KeyEventArgs e)
        {
            if (JumpTime.Text != "")
            {
                try
                {
                    int t = int.Parse(JumpTime.Text);
                    if (t > 0 && t < 1000)
                    {
                        TimeSpan ts = TimeSpan.FromMinutes(t);
                        Properties.User.Default.jumpTime = t;
                        Properties.User.Default.Save();
                        Ow.timer1.Interval = ts;
                        //显示消息
                        ShowMessage("已更改");
                    }
                    else
                    {
                        JumpTime.Text = Properties.User.Default.jumpTime.ToString();
                        ShowMessage("输入1~999整数");
                    }
                }
                catch (Exception)
                {
                    JumpTime.Text = Properties.User.Default.jumpTime.ToString();
                    ShowMessage("输入整数");
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
                        langRd = Application.LoadComponent(new Uri(@"语言/zh_CN.xaml", UriKind.Relative)) as ResourceDictionary;
                    }
                    else
                    {
                        //根据名字载入语言文件
                        langRd = Application.LoadComponent(new Uri(@"语言/" + langName + ".xaml", UriKind.Relative)) as ResourceDictionary;
                    }
                }
                catch (Exception e2)
                { MessageBox.Show(e2.Message); }

                if (langRd != null)
                {
                    //本窗口更改语言，如果已使用其他语言,先清空
                    if (this.Resources.MergedDictionaries.Count > 0)
                    { this.Resources.MergedDictionaries.Clear(); }
                    this.Resources.MergedDictionaries.Add(langRd);
                    //主窗口更改语言
                    if (Ow.Resources.MergedDictionaries.Count > 0)
                    { Ow.Resources.MergedDictionaries.Clear(); }
                    Ow.Resources.MergedDictionaries.Add(langRd);
                    //手动刷新一些未自动更改语言的UI文字
                    Ow.RefreshUILanguage();
                    //保存设置
                    Properties.User.Default.language = langName;
                    Properties.User.Default.Save();
                    //显示消息
                    ShowMessage("已更改");
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
        /// 重置
        /// </summary>
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.Reset();

            //是否在切换视频时保持变换
            KeepTrans.IsChecked = Properties.User.Default.isKeepTrans;
            //是否在退出时保留播放列表
            SavePlaylist.IsChecked = Properties.User.Default.isSavePlaylist;
            //快进快退时间
            JumpTime.Text = Properties.User.Default.jumpTime.ToString();

            //语言
            SetLanguage(Properties.User.Default.language);

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
        void Timer_Tick(object sender, EventArgs e)
        {
            if (Message.Opacity > 0)
            {
                Message.Opacity -= 0.01;
            }
        }
        /// <summary>
        /// 检测更新
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        { System.Diagnostics.Process.Start("explorer.exe", "http://estar.zone/introduction/e-player"); }
        ///打开网页
        private void HomeSite_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { System.Diagnostics.Process.Start("explorer.exe", "http://estar.zone"); }
        private void SoftwareSite_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { System.Diagnostics.Process.Start("explorer.exe", "http://estar.zone/introduction/e-player"); }
        private void GitHubSite_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { System.Diagnostics.Process.Start("explorer.exe", "https://github.com/HelloEStar/E-Player"); }
        private void GitHubBugSite_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { System.Diagnostics.Process.Start("explorer.exe", "https://github.com/HelloEStar/E-Player/issues"); }
        private void QQGroup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { System.Diagnostics.Process.Start("explorer.exe", "http://jq.qq.com/?_wv=1027&k=5TQxcvR"); }
        
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
        public string Name
        {
            get;
            set;
        }
        public string Value
        {
            get;
            set;
        }
    }
}
