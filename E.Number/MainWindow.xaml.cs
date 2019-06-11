using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.MessageBox;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;

namespace E.Number
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 获取软件信息
        /// <summary>
        /// 名称
        /// </summary>
        public static string ThisName { get; } = Application.ProductName;
        /// <summary>
        /// 描述
        /// </summary>
        public static string ThisDescription { get; } = "随机数生成器";
        /// <summary>
        /// 开发者
        /// </summary>
        public static string ThisCompany { get; } = Application.CompanyName;
        /// <summary>
        /// 作者
        /// </summary>
        public static string ThisDeveloper { get; } = "E Star";
        /// <summary>
        /// 当前版本
        /// </summary>
        public Version ThisVer { get; } = new Version(Application.ProductVersion);
        /// <summary>
        /// 新版本
        /// </summary>
        public Version NewVer { get; private set; }
        /// <summary>
        /// 新版信息获取连接
        /// </summary>
        public static string NewVerInfoLink { get; } = "http://estar.zone/wp-content/E_Updater_info.txt";
        /// <summary>
        /// 新版下载连接
        /// </summary>
        public static string NewVerDownloadLink { get; } = "http://estar.zone/introduction/e-number/";
        #endregion 

        /// <summary>
        /// 随机数范围
        /// </summary>
        private int min, max;

        #region 上下文菜单
        ContextMenu CM;
        MenuItem MenuHelp;
        MenuItem MenuPreference, MenuAbout, MenuLink;
        #endregion

        /// <summary>
        /// 构造器
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            ResetRange();


            //初始化右键菜单
            CreteContextMenu();
        }

        #region 
        //事件
        /// <summary>
        /// 窗口尺寸改变时：自适应窗体全屏
        /// </summary>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TxtValue.FontSize = GrdMain.ActualHeight - 70;
        }
        /// <summary>
        /// 修改随机范围最小值时：激活按钮
        /// </summary>
        private void TxtMinValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            BtnSetRange.IsEnabled = true;
            ShowMessage("修改随机范围");
        }
        /// <summary>
        /// 修改随机范围最大值时：激活按钮
        /// </summary>
        private void TxtMaxValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            BtnSetRange.IsEnabled = true;
            ShowMessage("修改随机范围");
        }
        /// <summary>
        /// 点击按钮时：生成随机数字
        /// </summary>
        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            //CheckLength();
            Random rd = new Random();
            TxtValue.Text = rd.Next(min, max).ToString();
        }
        /// <summary>
        /// 保存随机范围
        /// </summary>
        private void BtnSetRange_Click(object sender, RoutedEventArgs e)
        {
            SetRange();
        }
        /// <summary>
        /// 按键抬起时：按下回车键保存随机范围最大值
        /// </summary>
        private void TxtMaxValue_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetRange();
            }
        }
        /// <summary>
        /// 按键抬起时：按下回车键随机范围保存最小值
        /// </summary>
        private void TxtMinValue_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetRange();
            }
        }

        //帮助 事件
        /// <summary>
        /// 点击 打开帮助
        /// </summary>
        private void MenuHelp_Click(object sender, RoutedEventArgs e)
        {
            OpenHelp(1);
        }
        /// <summary>
        /// 点击 偏好设置
        /// </summary>
        private void MenuPreference_Click(object sender, RoutedEventArgs e)
        {
            OpenHelp(1);
        }
        /// <summary>
        /// 点击 软件信息
        /// </summary>
        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            OpenHelp(2);
        }
        /// <summary>
        /// 点击 相关链接
        /// </summary>
        private void MenuLink_Click(object sender, RoutedEventArgs e)
        {
            OpenHelp(3);
        }
        //实现
        /// <summary>
        /// 实例化帮助窗口，载入设置信息
        /// </summary>
        /// <param name="tab">选择打开的标签</param>
        public void OpenHelp(int tab)
        {
            HelpWindow Help = new HelpWindow(this);
            Help.Show();
            Help.MainTab.SelectedIndex = tab - 1;
        }
        #endregion

        #region 
        //方法
        /// <summary>
        /// 设置随机范围
        /// </summary>
        private void SetRange()
        {
            try
            {
                min = int.Parse(TxtMinValue.Text);
                max = int.Parse(TxtMaxValue.Text);
                if (min < max)
                {
                    BtnSetRange.IsEnabled = false;
                    BtnCreate.IsEnabled = true;
                    ShowMessage("已修改随机范围");
                }
                else
                {
                    BtnSetRange.IsEnabled = false;
                    BtnCreate.IsEnabled = false;
                    ShowMessage("随机范围错误");
                }
            }
            catch
            {
                BtnSetRange.IsEnabled = false;
                BtnCreate.IsEnabled = false;
                ShowMessage("请输入数字字符");
            }
        }
        /// <summary>
        /// 重置随机范围
        /// </summary>
        public void ResetRange()
        {
            min = 0;
            max = 100;
            TxtMinValue.Text = min.ToString();
            TxtMaxValue.Text = max.ToString();
        }
        /// <summary>
        /// 初始化右键菜单
        /// </summary>
        public void CreteContextMenu()
        {
            //实例化
            CM = new ContextMenu();

            MenuHelp = new MenuItem
            {
                Header = "帮助",
                InputGestureText = "Ctrl+H"
            };
            MenuPreference = new MenuItem
            {
                Header = "偏好设置",
                InputGestureText = "Ctrl+1"
            };
            MenuPreference.Click += new RoutedEventHandler(MenuPreference_Click);
            MenuAbout = new MenuItem
            {
                Header = "软件信息",
                InputGestureText = "Ctrl+2"
            };
            MenuAbout.Click += new RoutedEventHandler(MenuAbout_Click);
            MenuLink = new MenuItem
            {
                Header = "相关链接",
                InputGestureText = "Ctrl+3"
            };
            MenuLink.Click += new RoutedEventHandler(MenuLink_Click);

            CM.Items.Add(MenuHelp);
            //二级菜单绑定 MenuHelp
            MenuHelp.Items.Add(MenuPreference);
            MenuHelp.Items.Add(MenuAbout);
            MenuHelp.Items.Add(MenuLink);
            //绑定右键菜单
            mainWindow.ContextMenu = CM;

            //CM.Width = 600;
            CM.Placement = System.Windows.Controls.Primitives.PlacementMode.Left;
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="message">消息内容</param>
        public void ShowMessage(string message)
        {
            BtnSetRange.Content = message;
        }
        #endregion
    }
}
