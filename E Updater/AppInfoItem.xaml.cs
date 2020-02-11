using System;
using System.Collections.Generic;
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

        public static readonly DependencyProperty AppNameProperty = DependencyProperty.Register("AppName", typeof(string), typeof(AppInfoItem), new PropertyMetadata("App Name", new PropertyChangedCallback(OnTextChanged)));
        public string AppName
        {
            get { return (string)GetValue(AppNameProperty); }
            set { SetValue(AppNameProperty, value); }
        }
        static void OnTextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            AppInfoItem source = (AppInfoItem)sender;
            source.LblName.Content = (string)args.NewValue;
        }
    }
}
