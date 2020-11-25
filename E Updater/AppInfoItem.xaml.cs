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
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharedProject;

namespace E.Updater
{
    /// <summary>
    /// AppInfo.xaml 的交互逻辑
    /// </summary>
    public partial class AppInfoItem : UserControl
    {
        public AppInfoItem()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty AppNameProperty = DependencyProperty.Register(
            "AppName",
            typeof(string), 
            typeof(AppInfoItem),
            new PropertyMetadata("App Name", new PropertyChangedCallback(OnAppNameChanged)));
        public static readonly DependencyProperty AppDescriptionProperty = DependencyProperty.Register(
            "AppDescription", 
            typeof(string), 
            typeof(AppInfoItem), 
            new PropertyMetadata("App Description", new PropertyChangedCallback(OnAppDescriptionChanged)));
        public static readonly DependencyProperty AppVersionProperty = DependencyProperty.Register(
            "AppVersion",
            typeof(string),
            typeof(AppInfoItem), 
            new PropertyMetadata("App Version", new PropertyChangedCallback(OnAppVersionChanged)));
        public static readonly DependencyProperty AppStateProperty = DependencyProperty.Register(
            "AppState", 
            typeof(string), 
            typeof(AppInfoItem),
            new PropertyMetadata("App State", new PropertyChangedCallback(OnAppStateChanged)));
        public static readonly DependencyProperty AppIconProperty = DependencyProperty.Register(
            "AppIcon",
            typeof(string),
            typeof(AppInfoItem),
            new PropertyMetadata("App Icon", new PropertyChangedCallback(OnAppIconChanged)));
       
        public AppInfo AppInfo { get; set; }
        public string AppName
        {
            get { return (string)GetValue(AppNameProperty); }
            set { SetValue(AppNameProperty, value); }
        }
        public string AppDescription
        {
            get { return (string)GetValue(AppDescriptionProperty); }
            set { SetValue(AppDescriptionProperty, value); }
        }
        public string AppVersion
        {
            get { return (string)GetValue(AppVersionProperty); }
            set { SetValue(AppVersionProperty, value); }
        }
        public string AppState
        {
            get { return (string)GetValue(AppStateProperty); }
            set { SetValue(AppStateProperty, value); }
        }
        public string AppIcon
        {
            get { return (string)GetValue(AppIconProperty); }
            set { SetValue(AppIconProperty, value); }
        }

        private static void OnAppNameChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            AppInfoItem source = (AppInfoItem)sender;
            source.LblName.Content = (string)args.NewValue;
        }
        private static void OnAppDescriptionChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            AppInfoItem source = (AppInfoItem)sender;
            source.LblDescription.Content = (string)args.NewValue;
        }
        private static void OnAppVersionChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            AppInfoItem source = (AppInfoItem)sender;
            source.LblVersion.Content = (string)args.NewValue;
        }
        private static void OnAppStateChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            AppInfoItem source = (AppInfoItem)sender;
            source.LblState.Content = (string)args.NewValue;
        }
        private static void OnAppIconChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            AppInfoItem source = (AppInfoItem)sender;
            source.ImgIcon.Source = new BitmapImage(new Uri((string)args.NewValue, UriKind.Relative));
        }


        private void PanAppInfo_Loaded(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
        private void PanBtns_Loaded(object sender, RoutedEventArgs e)
        {
            PanBtns.Visibility = Visibility.Collapsed;
        }
        private void PanAppInfo_MouseEnter(object sender, MouseEventArgs e)
        {
            PanAppInfo.Background = (Brush)FindResource("三级背景色");
            PanBtns.Visibility = Visibility.Visible;
        }
        private void PanAppInfo_MouseLeave(object sender, MouseEventArgs e)
        {
            PanAppInfo.Background = (Brush)FindResource("二级背景色");
            PanBtns.Visibility = Visibility.Collapsed;
        }
        private void ImgIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Process.Start("explorer.exe", AppInfo.HomePage);
            }
        }

        private void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Install(AppInfo);
            Refresh();
        }
        private void BtnUnInstall_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).UnInstall(AppInfo);
            Refresh();
        }
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Update(AppInfo);
            Refresh();
        }
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            AppInfo.Browse();
            Refresh();
        }
        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            AppInfo.Run();
            Refresh();
        }
        private void BtnKill_Click(object sender, RoutedEventArgs e)
        {
            AppInfo.Kill();
            Refresh();
        }

        public void Refresh()
        {
            if (AppInfo != null)
            {
                BtnBrowse.IsEnabled = true;

                //已安装
                if (AppInfo.IsExists)
                {
                    BtnInstall.IsEnabled = false;
                    BtnUnInstall.IsEnabled = true;

                    //运行中
                    if (AppInfo.IsRunning)
                    {
                        BtnRun.IsEnabled = false;
                        BtnKill.IsEnabled = true;
                    }
                    //未运行
                    else
                    {
                        BtnRun.IsEnabled = true;
                        BtnKill.IsEnabled = false;
                    }

                    //有下载链接
                    if (AppInfo.DownloadVersion != null)
                    {
                        //有新版
                        if (AppInfo.DownloadVersion > AppInfo.Version)
                        {
                            BtnUpdate.IsEnabled = true;
                            AppState = "有新版本待更新";
                        }
                        //无新版
                        else
                        {
                            BtnUpdate.IsEnabled = false;
                            AppState = "已是最新版本";
                        }
                    }
                    //无下载链接
                    else
                    {
                        BtnUpdate.IsEnabled = false;
                        AppState = "已安装";
                    }

                    AppVersion = AppInfo.Version.ToString();
                }
                //未安装
                else
                {
                    BtnUpdate.IsEnabled = false;

                    BtnUnInstall.IsEnabled = false;
                    BtnRun.IsEnabled = false;
                    BtnKill.IsEnabled = false;

                    //有下载链接
                    if (AppInfo.DownloadVersion != null)
                    {
                        BtnInstall.IsEnabled = true;
                        AppState = "未安装";
                    }
                    //无下载链接
                    else
                    {
                        BtnInstall.IsEnabled = false;
                        AppState = "未取得下载链接";
                    }

                    AppVersion = "0.0.0.0";
                }

                AppName = AppInfo.Name;
                AppDescription = AppInfo.Description;
                AppIcon = "Resources/" + AppInfo.Name + ".ico";
                ImgIcon.ToolTip = AppInfo.HomePage;
            }
            else
            {
                BtnInstall.IsEnabled = false;
                BtnUnInstall.IsEnabled = false;
                BtnBrowse.IsEnabled = false;
                BtnUpdate.IsEnabled = false;
                BtnRun.IsEnabled = false;
                BtnKill.IsEnabled = false;

                AppName = "未知应用";
                AppDescription = "应用描述";
                AppVersion = "0.0.0.0";
                AppState = "应用未配置";
                AppIcon = "Resources/E Updater.ico";
                ImgIcon.ToolTip = AppInfo.HomePage;
            }
        }
    }
}
