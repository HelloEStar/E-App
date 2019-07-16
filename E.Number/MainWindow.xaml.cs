using System;
using System.Reflection;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.Diagnostics;

using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.MessageBox;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;

using Settings = E.Number.Properties.Settings;
using User = E.Number.Properties.User;
using E.Utility;
using System.Windows.Media;

namespace E.Number
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
        private AppInfo AppInfo { get; set; }
        /// <summary>
        /// 语言列表
        /// </summary>
        private List<ItemInfo> LanguageItems { get; set; } = new List<ItemInfo>();
        /// <summary>
        /// 主题集合
        /// </summary>
        private List<TextBlock> ThemeItems { get; set; } = new List<TextBlock>();

        /// <summary>
        /// 随机数范围
        /// </summary>
        private int min, max;

        #endregion 

        #region 方法
        //构造
        /// <summary>
        /// 构造器
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        //载入
        /// <summary>
        /// 载入应用信息
        /// </summary>
        private void LoadAppInfo()
        {
            AssemblyProductAttribute product = (AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute));
            AssemblyDescriptionAttribute description = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyDescriptionAttribute));
            AssemblyCompanyAttribute company = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCompanyAttribute));
            AssemblyCopyrightAttribute copyright = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute));
            Stream src = System.Windows.Application.GetResourceStream(new Uri("/文档/更新日志.txt", UriKind.Relative)).Stream;
            string updateNote = new StreamReader(src, Encoding.UTF8).ReadToEnd();
            string homePage = "http://estar.zone";
            string infoPage = "http://estar.zone/introduction/e-number/";
            string downloadPage = "http://estar.zone/introduction/e-number/";
            string gitHubPage = "https://github.com/HelloEStar/E.App";
            string qqGroupLink = "http://jq.qq.com/?_wv=1027&k=5TQxcvR";
            string qqGroupNumber = "279807070";
            string bitCoinAddress = "19LHHVQzWJo8DemsanJhSZ4VNRtknyzR1q";
            AppInfo = new AppInfo(product.Product, description.Description, company.Company, copyright.Copyright, new Version(Application.ProductVersion), updateNote,
                                  homePage, infoPage, downloadPage, gitHubPage, qqGroupLink, qqGroupNumber, bitCoinAddress);
        }
        /// <summary>
        /// 载入偏好设置
        /// </summary>
        private void LoadSettings()
        {

        }
        /// <summary>
        /// 创建语言选项
        /// </summary>
        private void LoadLanguageItems()
        {
            LanguageItems.Clear();
            ItemInfo zh_CN = new ItemInfo("中文（默认）", "zh_CN");
            ItemInfo en_US = new ItemInfo("English", "en_US");
            LanguageItems.Add(zh_CN);
            LanguageItems.Add(en_US);

            //绑定数据，真正的赋值
            CbbLanguages.ItemsSource = LanguageItems;
            CbbLanguages.DisplayMemberPath = "Name";
            CbbLanguages.SelectedValuePath = "Value";
        }
        /// <summary>
        /// 载入所有可用主题
        /// </summary>
        private void LoadThemeItems()
        {
            //创建皮肤文件夹
            if (!Directory.Exists(User.Default.ThemesDir))
            { Directory.CreateDirectory(User.Default.ThemesDir); }

            string[] _mySkins = Directory.GetFiles(User.Default.ThemesDir);
            ThemeItems.Clear();
            foreach (string s in _mySkins)
            {
                string tmp = Path.GetExtension(s);
                if (tmp == ".ini" || tmp == ".INI")
                {
                    string tmp2 = INIOperator.ReadIniKeys("文件", "类型", s);
                    //若是主题配置文件
                    if (tmp2 == "主题")
                    {
                        string tmp3 = INIOperator.ReadIniKeys("文件", "版本", s);
                        if (tmp3 == AppInfo.Version.ToString())
                        {
                            TextBlock theme = new TextBlock
                            {
                                Text = Path.GetFileNameWithoutExtension(s),
                                ToolTip = s
                            };
                            ThemeItems.Add(theme);
                        }
                    }
                }
            }

            CbbThemes.Items.Clear();
            foreach (TextBlock item in ThemeItems)
            {
                TextBlock theme = new TextBlock()
                {
                    Text = item.Text,
                    ToolTip = item.ToolTip
                };
                CbbThemes.Items.Add(theme);
            }
        }

        ///打开

        ///关闭

        ///保存
        /// <summary>
        /// 储存应用设置
        /// </summary>
        private void SaveAppSettings()
        {
            Settings.Default.Save();
        }
        /// <summary>
        /// 保存用户设置
        /// </summary>
        private void SaveUserSettings()
        {
            User.Default.Save();
        }

        ///创建
        /// <summary>
        /// 创建颜色
        /// </summary>
        /// <param name="text">ARGB色值，以点号分隔，0-255</param>
        /// <returns></returns>
        private static Color CreateColor(string text)
        {
            //MessageBox.Show(text);
            try
            {
                string[] colors = text.Split('.');
                byte red = byte.Parse(colors[0]);
                byte green = byte.Parse(colors[1]);
                byte blue = byte.Parse(colors[2]);
                byte alpha = byte.Parse(colors[3]);
                Color color = Color.FromArgb(alpha, red, green, blue);
                return color;
            }
            catch (Exception)
            {
                Color color = Color.FromArgb(255, 125, 125, 125);
                return color;
            }
        }

        ///添加

        ///移除

        ///清空

        ///删除

        ///获取


        //设置
        /// <summary>
        /// 设置语言显示
        /// </summary>
        /// <param name="language">语言简拼</param>
        private void SetLanguage(string language)
        {
            try
            {
                ResourceDictionary langRd;
                langRd = System.Windows.Application.LoadComponent(new Uri(@"语言/" + language + ".xaml", UriKind.Relative)) as ResourceDictionary;
                if (langRd != null)
                {
                    //主窗口更改语言
                    if (Resources.MergedDictionaries.Count > 0)
                    {
                        Resources.MergedDictionaries.Clear();
                    }
                    Resources.MergedDictionaries.Add(langRd);
                    User.Default.language = language;
                    SaveUserSettings();
                }
            }
            catch (Exception e2)
            {
                MessageBox.Show(e2.Message);
            }
        }
        /// <summary>
        /// 切换主题显示
        /// </summary>
        private void SetTheme(string themePath)
        {
            foreach (TextBlock theme in ThemeItems)
            {
                if (theme.ToolTip.ToString() == themePath)
                {
                    if (File.Exists(themePath))
                    {
                        SetSkin(themePath);
                        User.Default.ThemePath = themePath;
                    }
                    else
                    {
                        ThemeItems.Remove(theme);
                        //设为默认主题
                        User.Default.ThemePath = User.Default.ThemePath;
                        SetSkin(User.Default.ThemePath);
                        ShowMessage("偏好主题的不存在");
                    }
                    SaveUserSettings();
                    break;
                }
            }
        }
        /// <summary>
        /// 切换下个主题显示
        /// </summary>
        private void SetNextTheme()
        {
            foreach (TextBlock theme in ThemeItems)
            {
                if (theme.ToolTip.ToString() == User.Default.ThemePath)
                {
                    int themeOrder = ThemeItems.IndexOf(theme);
                    int themeCounts = ThemeItems.Count;
                    if (themeOrder + 1 < themeCounts)
                    { themeOrder += 1; }
                    else
                    { themeOrder = 0; }
                    if (File.Exists(ThemeItems[themeOrder].ToolTip.ToString()))
                    {
                        //设为此主题
                        User.Default.ThemePath = ThemeItems[themeOrder].ToolTip.ToString();
                        SaveUserSettings();
                        SetSkin(User.Default.ThemePath);
                    }
                    else
                    {
                        ShowMessage("下一个主题的配置文件不存在");
                        ThemeItems.Remove(ThemeItems[themeOrder]);
                    }
                    break;
                }
            }

        }
        /// <summary>
        /// 重置主题颜色
        /// </summary>
        /// <param name="themePath">主题文件路径</param>
        private void SetSkin(string themePath)
        {
            SetColor("一级字体颜色", CreateColor(INIOperator.ReadIniKeys("字体", "一级字体", themePath)));
            SetColor("二级字体颜色", CreateColor(INIOperator.ReadIniKeys("字体", "二级字体", themePath)));
            SetColor("三级字体颜色", CreateColor(INIOperator.ReadIniKeys("字体", "三级字体", themePath)));

            SetColor("一级背景颜色", CreateColor(INIOperator.ReadIniKeys("背景", "一级背景", themePath)));
            SetColor("二级背景颜色", CreateColor(INIOperator.ReadIniKeys("背景", "二级背景", themePath)));
            SetColor("三级背景颜色", CreateColor(INIOperator.ReadIniKeys("背景", "三级背景", themePath)));

            SetColor("一级边框颜色", CreateColor(INIOperator.ReadIniKeys("边框", "一级边框", themePath)));

            SetColor("有焦点选中颜色", CreateColor(INIOperator.ReadIniKeys("高亮", "有焦点选中", themePath)));
            SetColor("无焦点选中颜色", CreateColor(INIOperator.ReadIniKeys("高亮", "无焦点选中", themePath)));
        }
        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="colorName"></param>
        /// <param name="c"></param>
        public void SetColor(string colorName, Color c)
        {
            Resources.Remove(colorName);
            Resources.Add(colorName, new SolidColorBrush(c));
        }
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

        //重置
        /// <summary>
        /// 重置应用设置
        /// </summary>
        private void ResetAppSettings()
        {
            Settings.Default.Reset();
            ShowMessage("已清空运行信息", true);
        }
        /// <summary>
        /// 重置用户设置
        /// </summary>
        private void ResetUserSettings()
        {
            User.Default.Reset();
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

        ///选择
        /// <summary>
        /// 设置语言选项
        /// </summary>
        /// <param name="language">语言简拼</param>
        private void SelectLanguageItem(string language)
        {
            foreach (ItemInfo ci in LanguageItems)
            {
                if (ci.Value == language)
                {
                    CbbLanguages.SelectedItem = ci;
                    break;
                }
            }
        }
        /// <summary>
        /// 设置主题选项
        /// </summary>
        /// <param name="themePath">主题路径</param>
        private void SelectThemeItem(string themePath)
        {
            foreach (TextBlock item in CbbThemes.Items)
            {
                if (item.ToolTip.ToString() == themePath)
                {
                    CbbThemes.SelectedItem = item;
                    break;
                }
            }
        }

        ///检查

        ///刷新

        //显示
        /// <summary>
        /// 显示软件信息
        /// </summary>
        private void ShowAppInfo()
        {
            ThisName.Text = AppInfo.Name;
            Description.Text = AppInfo.Description;
            Developer.Text = AppInfo.Company;
            Version.Text = AppInfo.Version.ToString();
            BitCoinAddress.Text = AppInfo.BitCoinAddress;
            UpdateNote.Text = AppInfo.UpdateNote;
        }
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="resourceName">资源名</param>
        /// <param name="newBox">是否弹出对话框</param>
        private void ShowMessage(string message, bool newBox = false)
        {
            if (HelpMessage == null)
            {
                return;
            }

            if (newBox)
            {
                MessageBox.Show(message);
            }
            else
            {
                HelpMessage.Text = message;
            }
        }
        #endregion


        #region 事件
        //窗口事件
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //载入并显示应用信息
            LoadAppInfo();
            ShowAppInfo();

            //载入下拉菜单项
            LoadLanguageItems();
            LoadThemeItems();

            //载入设置
            LoadSettings();
            ResetRange();

            //初始化
            SetLanguage(User.Default.language);
            SetTheme(User.Default.ThemePath);

            //提示消息
            ShowMessage("已载入");
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TxtValue.FontSize = GrdMain.ActualHeight - 70;
        }

        //按钮点击事件
        private void BtnFold_Click(object sender, RoutedEventArgs e)
        {
            if (PanCenter.Visibility == Visibility.Visible)
            {
                PanCenter.Visibility = Visibility.Collapsed;
                BtnFold.BorderThickness = new Thickness(0, 0, 0, 0);
            }
            else
            {
                PanCenter.Visibility = Visibility.Visible;
                BtnFold.BorderThickness = new Thickness(4, 0, 0, 0);
            }
        }
        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            CenterSettingPage.Visibility = Visibility.Visible;
            CenterAboutPage.Visibility = Visibility.Collapsed;

            BtnsSetting.Visibility = Visibility.Visible;
            BtnsAbout.Visibility = Visibility.Collapsed;

            BtnSetting.BorderThickness = new Thickness(4, 0, 0, 0);
            BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
        }
        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            CenterSettingPage.Visibility = Visibility.Collapsed;
            CenterAboutPage.Visibility = Visibility.Visible;

            BtnsSetting.Visibility = Visibility.Collapsed;
            BtnsAbout.Visibility = Visibility.Visible;

            BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnAbout.BorderThickness = new Thickness(4, 0, 0, 0);
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            //CheckLength();
            Random rd = new Random();
            TxtValue.Text = rd.Next(min, max).ToString();
        }
        private void BtnSetRange_Click(object sender, RoutedEventArgs e)
        {
            SetRange();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetUserSettings();
            LoadSettings();
            ShowMessage("已重置");
        }
        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {

        }
        private void BtnClearRunInfo_Click(object sender, RoutedEventArgs e)
        {
            ResetAppSettings();
        }

        private void BitCoinAddress_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Clipboard.SetDataObject(BitCoinAddress.Text, true);
            ShowMessage("已复制");
        }
        private void BtnHomePage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.HomePage);
        }
        private void BtnInfoPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.InfoPage);
        }
        private void BtnDownloadPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.DownloadPage);
        }
        private void BtnFeedbackPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.FeedbackPage);
        }
        private void BtnGitHubPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.GitHubPage);
        }
        private void BtnQQGroup_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.QQGroupLink);
        }

        //设置更改事件
        private void CbbLanguages_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (CbbLanguages.SelectedValue != null)
            {
                string langName = CbbLanguages.SelectedValue.ToString();
                SetLanguage(langName);
            }
        }
        private void CbbThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbbThemes.SelectedItem != null)
            {
                TextBlock item = CbbThemes.SelectedItem as TextBlock;
                string tmp = item.ToolTip.ToString();
                if (File.Exists(tmp))
                {
                    SetTheme(tmp);
                    ShowMessage("已更改");
                }
                else
                {
                    CbbThemes.Items.Remove(CbbThemes.SelectedItem);
                    ShowMessage("该主题的配置文件不存在");
                }
            }
        }

        private void TxtMinValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            BtnSetRange.IsEnabled = true;
            ShowMessage("修改随机范围");
        }
        private void TxtMaxValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            BtnSetRange.IsEnabled = true;
            ShowMessage("修改随机范围");
        }
        private void TxtMaxValue_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetRange();
            }
        }
        private void TxtMinValue_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetRange();
            }
        }
        #endregion
    }
}
