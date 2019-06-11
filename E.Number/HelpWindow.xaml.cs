using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

namespace E.Number
{
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
            ////创建语言选项
            //GetLanguage();
            //SetLanguage(Properties.Settings.Default.language);

            //显示消息
            ShowMessage("已载入");
        }

        /// <summary>
        /// 更改了语言选项
        /// </summary>
        //private void CBSelectedLanguage_SelectionChanged(object sender, RoutedEventArgs e)
        //{
        //    object selectedName = CBSelectedLanguage.SelectedValue;
        //    if (selectedName != null)
        //    {
        //        string langName = selectedName.ToString();
        //        //根据本地语言来进行本地化,不过这里上不到
        //        //CultureInfo currentCultureInfo = CultureInfo.CurrentCulture;
        //        ResourceDictionary langRd = null;
        //        try
        //        {
        //            if (langName == "zh_CN")
        //            {
        //                //根据名字载入语言文件
        //                langRd = Application.LoadComponent(new Uri(@"languages/zh_CN.xaml", UriKind.Relative)) as ResourceDictionary;
        //            }
        //            else
        //            {
        //                //根据名字载入语言文件
        //                langRd = Application.LoadComponent(new Uri(@"languages/" + langName + ".xaml", UriKind.Relative)) as ResourceDictionary;
        //            }
        //        }
        //        catch (Exception e2)
        //        { MessageBox.Show(e2.Message); }

        //        if (langRd != null)
        //        {
        //            //本窗口更改语言，如果已使用其他语言,先清空
        //            if (Resources.MergedDictionaries.Count > 0)
        //            { Resources.MergedDictionaries.Clear(); }
        //            Resources.MergedDictionaries.Add(langRd);
        //            //主窗口更改语言
        //            if (Ow.Resources.MergedDictionaries.Count > 0)
        //            { Ow.Resources.MergedDictionaries.Clear(); }
        //            Ow.Resources.MergedDictionaries.Add(langRd);
        //            //保存设置
        //            Properties.Settings.Default.language = langName;
        //            Properties.Settings.Default.Save();
        //        }
        //    }
        //}
        /// <summary>
        /// 创建语言选项
        /// </summary>
        //private void GetLanguage()
        //{
        //    CategoryInfo zh_CN = new CategoryInfo() { Name = "中文（默认）", Value = "zh_CN" };
        //    categoryList.Add(zh_CN);
        //    CategoryInfo en_US = new CategoryInfo() { Name = "English", Value = "en_US" };
        //    categoryList.Add(en_US);
        //    //绑定数据，真正的赋值
        //    CBSelectedLanguage.ItemsSource = categoryList;
        //    //指定显示的内容
        //    CBSelectedLanguage.DisplayMemberPath = "Name";
        //    //指定选中后的能够获取到的内容
        //    CBSelectedLanguage.SelectedValuePath = "Value";
        //}
        /// <summary>
        /// 设置语言选项
        /// </summary>
        /// <param name="language">语言简拼</param>
        //private void SetLanguage(string language)
        //{
        //    foreach (CategoryInfo ci in categoryList)
        //    {
        //        if (ci.Value == language)
        //        {
        //            CBSelectedLanguage.SelectedItem = ci;
        //            break;
        //        }
        //    }
        //}
        /// <summary>
        /// 重置设置
        /// </summary>
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();

            //语言
            //SetLanguage(Properties.Settings.Default.language);

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
        ///打开网页
        private void HomeSite_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", "http://estar.zone"); }
        private void DownloadSite_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", "http://estar.zone/introduction/e-number"); }
        private void FeedbackSite_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", "https://github.com/HelloEStar/E-Number"); }
        private void ForumSite_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", "https://github.com/HelloEStar/E-Number/issues"); }
        private void QQGroup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { Process.Start("explorer.exe", "http://jq.qq.com/?_wv=1027&k=5TQxcvR"); }

        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="resourceName">资源名</param>
        public void ShowMessage(string resourceName)
        {
            Message.Opacity = 1;
            Message.Content = resourceName;
        }
    }

    //定义ComboBox选项的类，存放Name和Value
    public struct CategoryInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
