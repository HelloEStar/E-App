using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharedProject;

namespace E_Talker
{
    public partial class MainWindow : EWindow
    {
        public string selfIP = "127.0.0.1";
        public int port = 50000;
        public string cmdQuit = "\\q";
        public string cmdHelp = "\\h";
        public string sj = "        ";

        
        public bool isStart;
        public Mode mode = Mode.客户端;
        public State state = State.未连接至任何服务器;

        public enum Mode
        {
            客户端,
            服务器,
            主机
        }
        public enum State
        {
            未连接到网络,
            客户端_未连接至任何服务器,
            客户端_已连接至本地服务器,
            客户端_已连接至其它服务器,
            服务器_等待其他客户端连接,
            服务器_已连接若干客户端,
            主机_未连接至任何服务器,
            主机_已连接至本地服务器,
            主机_已连接至其它服务器,
            
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void StartServer()
        {
            Server ss = new Server();
            ss.Start(port);
        }
        public void StartClient()
        {
            Client client = new Client();
            do
            {
                selfIP = GetServerIP();
            }
            while (!client.Connected(selfIP, port));

            string str = Console.ReadLine();
            while (true)
            {
                if (string.IsNullOrEmpty(str))
                {
                    AddMessageItem("请输入字符串");
                    str = Console.ReadLine();
                    continue;
                }
                if (str.Equals(cmdQuit))
                {
                    break;
                }
                if (str.Equals(cmdHelp))
                {
                    AddMessageItem("命令列表");
                    AddMessageItem(sj + cmdHelp + "  显示帮助");
                    AddMessageItem(sj + cmdQuit + "  退出聊天");
                    str = Console.ReadLine();
                    continue;
                }

                client.SendMessage(str);
                str = Console.ReadLine();
            }
            LogHelper.System("已退出聊天");
        }
        public void StartHost()
        {
            StartServer();
            StartClient();
        }
        private string GetServerIP()
        {
            // LogHelper.System("客户端准备连接，请输入正确格式的服务器IP地址，如 127.0.0.1");
            string ip = Console.ReadLine();
            while (true)
            {
                if (string.IsNullOrEmpty(ip))
                {
                    LogHelper.SystemError("请输入IP地址");
                    ip = Console.ReadLine();
                    continue;
                }
                break;
            }
            return ip;
        }

        //载入
        private void LoadRecordItems()
        {
            if (!string.IsNullOrEmpty(Settings.Default.Record))
            {
                string[] _myB = Regex.Split(Settings.Default.Record, "///");
                foreach (string b in _myB)
                {
                    //AddMessageItem(b);
                }
            }
        }

        //保存
        protected override void SaveSettings()
        {
            Settings.Default.Record = "";
            List<string> strs = new List<string>();
            foreach (ListBoxItem item in LtbRecord.Items)
            {
                strs.Add(item.Content.ToString());
            }
            Settings.Default.Record = string.Join("///", strs);

            if (Settings.Default.Theme == 2)
            {
                string content = GetPanColors(PanColors);
                Settings.Default.ThemeCustomize = content;
            }

            Settings.Default.Save();
            ShowMessage(FindResource("已保存").ToString());
        }

        //添加
        private void AddMessageItem(string content)
        {
            TextBlock item = new TextBlock
            {
                Text = content,
                //Style = (Style)FindResource("列表子项样式")
            };
            PanContent.Children.Add(item);
        }

        //设置
        protected override void SetMenuTool(MenuTool menu)
        {
            switch (menu)
            {
                case MenuTool.无:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.Background = BrushBG01;
                    BtnSetting.Background = BrushBG01;
                    BtnAbout.Background = BrushBG01;
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
            int rc = Settings.Default.RunCount;
            Settings.Default.Reset();
            Settings.Default.RunCount = rc;
            Settings.Default.Save();
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

        //显示
        protected override void ShowMessage(string message, bool newBox = false)
        {
            ShowMessage(LblMessage, message, newBox);
        }

        //主窗口
        protected override void EWindow_GotFocus(object sender, RoutedEventArgs e)
        {
        }
        protected override void EWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //载入
            LoadLanguageItems(CbbLanguages);
            LoadThemeItems(CbbThemes);
            LoadRecordItems();
            //刷新
            RefreshAppInfo();
            RefreshTitle();
            LblMessage.Opacity = 0;

            //检查用户是否同意用户协议
            if (CheckUserAgreement(Settings.Default.RunCount))
            {
                Settings.Default.RunCount += 1;
            }

            AddMessageItem("输入运行模式");
        }
        protected override void EWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }
        protected override void EWindow_KeyUp(object sender, KeyEventArgs e)
        {
            base.EWindow_KeyUp(sender, e);

            //Ctrl+T 切换下个主题
            if (e.Key == Key.T && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            { SetNextTheme(CbbThemes, Settings.Default.Theme); }
        }

        //工具栏
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            LtbRecord.Items.Clear();
        }
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (LtbRecord.Items != null && LtbRecord.Items.Count > 0)
            {
                string str = "";
                foreach (var item in LtbRecord.Items)
                {
                    ListBoxItem lb = (ListBoxItem)item;
                    str += lb.Content + "\n";
                }
                str = str.TrimEnd('\n').TrimEnd('\n');
                Clipboard.SetDataObject(str, true);
                ShowMessage(FindResource("已复制").ToString());
            }
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
                    }
                    content = Settings.Default.ThemeCustomize;
                    PanColors.Visibility = Visibility.Visible;
                }
                else
                {
                    PanColors.Visibility = Visibility.Collapsed;
                }

                if (string.IsNullOrEmpty(content))
                {
                    CbbThemes.Items.Remove(cbi);
                    //设为默认主题
                    Settings.Default.Theme = 0;
                }
                else
                {
                    SetPanColors(PanColors, content);
                    ColorHelper.SetColors(Resources, content);
                }
                //立即刷新按钮样式
                SetMenuTool(CurrentMenuTool);
            }
        }


        //工作区
        private void BtnInput_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtInput.Text))
            {
                ShowMessage("不能为空");
            }
            else
            {
                AddMessageItem(TxtInput.Text);
                TxtInput.Text = "";
            }
        }

        private void BtnHost_Click(object sender, RoutedEventArgs e)
        {
            StartHost();
        }
        private void BtnServer_Click(object sender, RoutedEventArgs e)
        {
            StartServer();
        }
        private void BtnClient_Click(object sender, RoutedEventArgs e)
        {
            StartClient();
        }
    }

}