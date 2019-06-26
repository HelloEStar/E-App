using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Net;
using System.Diagnostics;
using System.Reflection;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.MessageBox;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;

using E.Utility;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace E.Writer
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
        public AppInfo AppInfo { get; private set; }
        /// <summary>
        /// 当前书籍的名字
        /// </summary>
        public string SelectedBook { get; set; }
        /// <summary>
        /// 当前书籍的路径
        /// </summary>
        public string SelectedBookPath { get; set; }
        /// <summary>
        /// 当前卷册的名字
        /// </summary>
        public string SelectedChapter { get; set; }
        /// <summary>
        /// 当前卷册的路径
        /// </summary>
        public string SelectedChapterPath { get; set; }
        /// <summary>
        /// 当前文章的名字
        /// </summary>
        public string SelectedEssay { get; set; }
        /// <summary>
        /// 当前文章的路径
        /// </summary>
        public string SelectedEssayPath { get; set; }
        /// <summary>
        /// 打开的文件是否已保存
        /// </summary>
        public bool IsSaved { get; private set; } = true;
        /// <summary>
        /// 主题集合
        /// </summary>
        public List<TextBlock> Themes { get; set; } = new List<TextBlock>();     
        /// <summary>
        /// 自动保存计时器
        /// </summary>
        public DispatcherTimer AutoSaveTimer { get; set; }
        /// <summary>
        /// 自动备份计时器
        /// </summary>
        public DispatcherTimer AutoBackupTimer { get; set; }
        /// <summary>
        /// 选中的节点
        /// </summary>
        public FileNode SelectedNode { get; set; }
        /// <summary>
        /// 打开的节点
        /// </summary>
        public FileNode OpenedNode { get; set; }
        /// <summary>
        /// 语言列表
        /// </summary>
        private List<CategoryInfo> LanguageCategorys { get; set; } = new List<CategoryInfo>();
        #endregion 

        #region 方法
        //构造
        /// <summary>
        /// 默认构造器
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
            string homePage = Properties.Settings.Default.HomePage;
            string infoPage = Properties.Settings.Default.InfoPage;
            string downloadPage = Properties.Settings.Default.DownloadPage;
            string gitHubPage = Properties.Settings.Default.GitHubPage;
            string qqGroup = Properties.Settings.Default.QQGroup;
            string bitCoinAddress = Properties.Settings.Default.BitCoinAddress;
            AppInfo = new AppInfo(product.Product, description.Description, company.Company, copyright.Copyright, new Version(Application.ProductVersion), updateNote,
                                  homePage, infoPage, downloadPage, gitHubPage, qqGroup, bitCoinAddress);
        }
        /// <summary>
        /// 载入应用设置
        /// </summary>
        private void LoadBookHistory()
        {
            //验证上次关闭的书籍路径是否存在，若不存在，重置为根目录
            if (!Directory.Exists(Properties.Settings.Default._lastBook))
            {
                Properties.Settings.Default._lastBook = Properties.User.Default.BooksDir;
                SaveAppSettings();
            }
            //读取集合所有打开过的书籍路径的单字符串，并加入书籍列表
            if (Properties.Settings.Default._books != null && Properties.Settings.Default._books != "")
            {
                string[] _myB = Regex.Split(Properties.Settings.Default._books, "///");
                foreach (var b in _myB)
                {
                    if (Directory.Exists(b))
                    {
                        string temp1 = Path.GetDirectoryName(b);
                        string temp2 = (b.Replace(temp1, "")).Replace(@"\", "");
                        AddBooks(false, temp2, b);
                    }
                }
            }
            else
            {
                Books.ToolTip = FindResource("未打开任何书籍");
            }
        }
        /// <summary>
        /// 加载用户偏好设置
        /// </summary>
        private void LoadPreferences()
        {
            //载入界面语言
            SetLanguage(Properties.User.Default.language);
            //设置偏好主题
            SetTheme(Properties.User.Default.ThemePath);
            //初始化字体
            SetFont(Properties.User.Default.fontName);
            //初始化字号
            FileContent.FontSize = Properties.User.Default.fontSize;
            //初始化提示消息
            FilesTree.ToolTip = FindResource("创建或打开以开始");
            HelpMessage.Content = FindResource("创建或打开以开始");

            //检测是否自动打开书
            if (Properties.User.Default.isAutoOpenBook)
            {
                //选择上次关闭的书籍
                foreach (var item in Books.Items)
                {
                    Grid grid = item as Grid;
                    System.Windows.Controls.Label tb = grid.Children[0] as System.Windows.Controls.Label;
                    System.Windows.Controls.Button btn = grid.Children[1] as System.Windows.Controls.Button;
                    if (tb.Tag.ToString() == Properties.Settings.Default._lastBook)
                    {
                        Books.SelectedItem = item;
                        //打开
                        AutoOpenBook();
                        break;
                    }
                }
            }
            //运行信息
            if (Properties.User.Default.isShowRunInfo)
            {
                ShowRunInfo();
            }
        }
        /// <summary>
        /// 载入所有可用主题
        /// </summary>
        private void LoadThemes()
        {
            string[] _mySkins = Directory.GetFiles(Properties.User.Default.ThemesDir);
            foreach (string s in _mySkins)
            {
                string tmp = Path.GetExtension(s);
                if (tmp == ".ini" || tmp == ".INI")
                {
                    string tmp2 = INIOperator.ReadIniKeys("info", "type", s);
                    //若是主题配置文件
                    if (tmp2 == "theme")
                    {
                        string tmp3 = INIOperator.ReadIniKeys("info", "version", s);
                        //若与当前版本匹配
                        if (tmp3 == AppInfo.Version.ToString())
                        {
                            //添加选项
                            TextBlock theme = new TextBlock
                            {
                                Text = Path.GetFileNameWithoutExtension(s),
                                ToolTip = s
                            };
                            Themes.Add(theme);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 获取主题选项
        /// </summary>
        private void LoadThemeItem()
        {
            Skins.Items.Clear();
            foreach (TextBlock item in Themes)
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

        //保存
        /// <summary>
        /// 储存应用设置
        /// </summary>
        public void SaveAppSettings()
        {
            Properties.Settings.Default.Save();
        }
        /// <summary>
        /// 保存用户偏好设置
        /// </summary>
        public void SavePreferences()
        {
            Properties.User.Default.Save();
        }

        //检查
        /// <summary>
        /// 检查文件夹
        /// </summary>
        private void CheckFolders()
        {
            //创建存档文件夹
            if (!Directory.Exists(Properties.User.Default.BooksDir))
            { Directory.CreateDirectory(Properties.User.Default.BooksDir); }
            //创建备份文件夹
            if (!Directory.Exists(Properties.User.Default.BackupDir))
            { Directory.CreateDirectory(Properties.User.Default.BackupDir); }
            //创建皮肤文件夹
            if (!Directory.Exists(Properties.User.Default.ThemesDir))
            { Directory.CreateDirectory(Properties.User.Default.ThemesDir); }
        }
        /// <summary>
        /// 检测名字是否合法字符
        /// </summary>
        /// <param name="name">名字</param>
        /// <returns>是否合法字符</returns>
        public static bool CheckIsRightName(string name)
        {
            String str1 = "/";
            String str2 = "|";
            String str3 = "\\";
            String str4 = "<";
            String str5 = ">";
            String str6 = ":";
            String str7 = "*";
            String str8 = "?";
            String str9 = "\"";

            if (name.Contains(str1) || name.Contains(str2) || name.Contains(str3) || name.Contains(str4) ||
                name.Contains(str5) || name.Contains(str6) || name.Contains(str7) || name.Contains(str8) || name.Contains(str9))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //设置
        /// <summary>
        /// 设置应用设置
        /// </summary>
        private void SetAppSettingsOnStart()
        {
            //运行次数+1
            Properties.Settings.Default.runTimes += 1;
            //记录启动时间
            Properties.Settings.Default.thisStartTime = DateTime.Now;
            SaveAppSettings();
        }
        /// <summary>
        /// 设置应用设置
        /// </summary>
        private void SetAppSettingsOnQuit()
        {
            if (Books.Items.Count > 0)
            {
                //记录最近打开过的书籍，将所有路径集合到一个字符串
                Properties.Settings.Default._books = "";
                foreach (var item in Books.Items)
                {
                    Grid grid = item as Grid;
                    System.Windows.Controls.Label tb = grid.Children[0] as System.Windows.Controls.Label;
                    Properties.Settings.Default._books += tb.Tag.ToString() + "///";
                }
                Properties.Settings.Default._books = Properties.Settings.Default._books.Substring(0, Properties.Settings.Default._books.Length - 3);
            }
            Properties.Settings.Default.thisEndTime = DateTime.Now;
            Properties.Settings.Default.lastStartTime = Properties.Settings.Default.thisStartTime;
            Properties.Settings.Default.lastEndTime = Properties.Settings.Default.thisEndTime;
            Properties.Settings.Default.thisTotalTime = Properties.Settings.Default.thisEndTime - Properties.Settings.Default.thisStartTime;
            Properties.Settings.Default.totalTime = Properties.Settings.Default.thisTotalTime + Properties.Settings.Default.totalTime;
            SaveAppSettings();
        }
        /// <summary>
        /// 载入语言
        /// </summary>
        /// <param name="lang">语言简拼</param>
        private void SetLanguage(string lang)
        {
            try
            {
                //根据名字载入语言文件
                ResourceDictionary langRd = System.Windows.Application.LoadComponent(new Uri(@"语言\" + lang + ".xaml", UriKind.Relative)) as ResourceDictionary;
                //主窗口更改语言
                if (Resources.MergedDictionaries.Count > 0)
                {
                    Resources.MergedDictionaries.Clear();
                }
                Resources.MergedDictionaries.Add(langRd);
            }
            catch (Exception e2)
            {
                MessageBox.Show(e2.Message);
            }
        }
        /// <summary>
        /// 切换主题
        /// </summary>
        public void SetTheme(string _theme)
        {
            foreach (TextBlock theme in Themes)
            {
                if (theme.ToolTip.ToString() == _theme)
                {
                    if (File.Exists(_theme))
                    {
                        //设为此主题
                        SetSkin(_theme);
                    }
                    else
                    {
                        Themes.Remove(theme);
                        //设为默认主题
                        Properties.User.Default.ThemePath = Properties.User.Default.ThemePath;
                        SavePreferences();
                        SetSkin(Properties.User.Default.ThemePath);
                        //显示消息
                        ShowMessage("偏好主题的不存在", false);
                    }
                    break;
                }
            }
        }
        /// <summary>
        /// 切换下个主题
        /// </summary>
        private void SetNextTheme()
        {
            foreach (TextBlock theme in Themes)
            {
                if (theme.ToolTip.ToString() == Properties.User.Default.ThemePath)
                {
                    int themeOrder = Themes.IndexOf(theme);
                    int themeCounts = Themes.Count;
                    if (themeOrder + 1 < themeCounts)
                    { themeOrder += 1; }
                    else
                    { themeOrder = 0; }
                    if (File.Exists(Themes[themeOrder].ToolTip.ToString()))
                    {
                        //设为此主题
                        Properties.User.Default.ThemePath = Themes[themeOrder].ToolTip.ToString();
                        SavePreferences();
                        SetSkin(Properties.User.Default.ThemePath);
                    }
                    else
                    {
                        ShowMessage("下一个主题的配置文件不存在", false);
                        Themes.Remove(Themes[themeOrder]);
                    }
                    break;
                }
            }

        }
        /// <summary>
        /// 重置主题颜色
        /// </summary>
        /// <param name="_skin">主题文件路径</param>
        private void SetSkin(string _skin)
        {
            //左侧区域配色
            //LeftArea.Background = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "leftBackColor", _skin)));
            ////书籍列表
            //Books.Background = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "booksBackColor", _skin)));
            //Books.Foreground = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "booksForeColor", _skin)));
            ////按钮
            //BtnOpenBook.Background = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "btnBackColor", _skin)));
            //BtnOpenBook.Foreground = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "btnForeColor", _skin)));
            //BtnCreateBook.Background = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "btnBackColor", _skin)));
            //BtnCreateBook.Foreground = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "btnForeColor", _skin)));
            //BtnCreateChapter.Background = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "btnBackColor", _skin)));
            //BtnCreateChapter.Foreground = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "btnForeColor", _skin)));
            //BtnCreateEssay.Background = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "btnBackColor", _skin)));
            //BtnCreateEssay.Foreground = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "btnForeColor", _skin)));
            ////文件树
            //FilesTree.Background = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "treeBackColor", _skin)));
            //FilesTree.Foreground = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "treeForeColor", _skin)));

            ////中侧区域配色
            //CenterArea.Background = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "midBackColor", _skin)));

            ////右侧区域配色
            //RightArea.Background = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "rightBackColor", _skin)));
            ////文章名与文章内容
            //EssayName.Background = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "nameBackColor", _skin)));
            //EssayName.Foreground = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "nameForeColor", _skin)));
            //EssayName.CaretBrush = EssayName.Foreground;
            //FileContent.Background = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "textBackColor", _skin)));
            //FileContent.Foreground = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "textForeColor", _skin)));
            //FileContent.CaretBrush = FileContent.Foreground;
            ////信息显示
            //RowAndColumn.Foreground = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "infoForeColor", _skin)));
            //Words.Foreground = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "infoForeColor", _skin)));
            //HelpMessage.Foreground = new SolidColorBrush(CreateColor(INIOperator.ReadIniKeys("mainWindow", "infoForeColor", _skin)));
        }
        /// <summary>
        /// 更改主窗口右键菜单和窗口控件状态
        /// </summary>
        /// <param name="state">0未打开，1书打开，2卷册打开，3文章打开</param>
        public void SetElementState(int state)
        {
            ////未打开书籍，未打开卷册，未打开文章
            //if (state == 0)
            //{
            //    //按钮可用性
            //    MenuCreateChapter.IsEnabled = false;
            //    MenuCreateEssay.IsEnabled = false;
            //    MenuSave.IsEnabled = false;
            //    MenuSaveAs.IsEnabled = false;
            //    MenuCloseBook.IsEnabled = false;
            //    MenuCloseChapter.IsEnabled = false;
            //    MenuCloseEssay.IsEnabled = false;
            //    MenuBookInfo.IsEnabled = false;
            //    MenuChapterInfo.IsEnabled = false;
            //    MenuExport.IsEnabled = false;
            //    MenuExpand.IsEnabled = false;
            //    MenuCollapse.IsEnabled = false;
            //    MenuDelete.IsEnabled = false;
            //    MenuRefresh.IsEnabled = false;
            //    MenuFindAndReplace.IsEnabled = false;
            //    BtnCreateChapter.IsEnabled = false;
            //    BtnCreateEssay.IsEnabled = false;
            //    //文本编辑可用性
            //    FileContent.IsEnabled = false;
            //    EssayName.IsEnabled = false;

            //}
            ////打开了书籍，未打开卷册，未打开文章
            //else if (state == 1)
            //{
            //    //按钮可用性
            //    MenuCreateChapter.IsEnabled = true;
            //    MenuCreateEssay.IsEnabled = true;
            //    MenuSave.IsEnabled = false;
            //    MenuSaveAs.IsEnabled = false;
            //    MenuCloseBook.IsEnabled = true;
            //    MenuCloseChapter.IsEnabled = false;
            //    MenuCloseEssay.IsEnabled = false;
            //    MenuBookInfo.IsEnabled = true;
            //    MenuChapterInfo.IsEnabled = false;
            //    MenuExport.IsEnabled = true;
            //    MenuExpand.IsEnabled = true;
            //    MenuCollapse.IsEnabled = true;
            //    MenuDelete.IsEnabled = true;
            //    MenuRefresh.IsEnabled = true;
            //    MenuFindAndReplace.IsEnabled = false;
            //    BtnCreateChapter.IsEnabled = true;
            //    BtnCreateEssay.IsEnabled = true;
            //    //文本编辑可用性
            //    FileContent.IsEnabled = false;
            //    EssayName.IsEnabled = false;
            //}
            ////打开了书籍，打开了卷册，未打开文章
            //else if (state == 2)
            //{
            //    //按钮可用性
            //    MenuCreateChapter.IsEnabled = true;
            //    MenuCreateEssay.IsEnabled = true;
            //    MenuSave.IsEnabled = false;
            //    MenuSaveAs.IsEnabled = false;
            //    MenuCloseBook.IsEnabled = true;
            //    MenuCloseChapter.IsEnabled = true;
            //    MenuCloseEssay.IsEnabled = false;
            //    MenuBookInfo.IsEnabled = true;
            //    MenuChapterInfo.IsEnabled = true;
            //    MenuExport.IsEnabled = true;
            //    MenuExpand.IsEnabled = true;
            //    MenuCollapse.IsEnabled = true;
            //    MenuDelete.IsEnabled = true;
            //    MenuRefresh.IsEnabled = true;
            //    MenuFindAndReplace.IsEnabled = false;
            //    BtnCreateChapter.IsEnabled = true;
            //    BtnCreateEssay.IsEnabled = true;
            //    //文本编辑可用性
            //    FileContent.IsEnabled = false;
            //    EssayName.IsEnabled = true;
            //}
            ////打开了书籍，打开了卷册，打开了文章
            //else if (state == 3)
            //{
            //    //按钮可用性
            //    MenuCreateChapter.IsEnabled = true;
            //    MenuCreateEssay.IsEnabled = true;
            //    MenuSave.IsEnabled = true;
            //    MenuSaveAs.IsEnabled = true;
            //    MenuCloseBook.IsEnabled = true;
            //    MenuCloseChapter.IsEnabled = true;
            //    MenuCloseEssay.IsEnabled = true;
            //    MenuBookInfo.IsEnabled = true;
            //    MenuChapterInfo.IsEnabled = true;
            //    MenuExport.IsEnabled = true;
            //    MenuExpand.IsEnabled = true;
            //    MenuCollapse.IsEnabled = true;
            //    MenuDelete.IsEnabled = true;
            //    MenuRefresh.IsEnabled = true;
            //    MenuFindAndReplace.IsEnabled = true;
            //    BtnCreateChapter.IsEnabled = true;
            //    BtnCreateEssay.IsEnabled = true;
            //    //文本编辑可用性
            //    FileContent.IsEnabled = true;
            //    EssayName.IsEnabled = true;
            //}
        }
        /// <summary>
        /// 设置内部展开状态
        /// </summary>
        /// <param name="node">节点</param>
        public void SetExpandedState(FileNode node)
        {
            if (node.IsFile == false)
            {
                //如果记录的展开状态是 展开
                if (node.IsExpanded)
                {
                    //将此节点展开
                    DependencyObject DPObj = FilesTree.ItemContainerGenerator.ContainerFromItem(node);
                    if (DPObj != null)
                    {
                        ((TreeViewItem)DPObj).IsExpanded = true;
                    }
                }
                //如果记录的展开状态是 收起
                else
                {
                    //将此节点收起
                    DependencyObject DPObj = FilesTree.ItemContainerGenerator.ContainerFromItem(node);
                    if (DPObj != null)
                    {
                        ((TreeViewItem)DPObj).IsExpanded = false;
                    }
                }
                //if (node.Nodes.Count != 0 && !node.IsFile)
                //{
                //    foreach (var no in node.Nodes)
                //    {
                //        GetExpandedState(no);
                //    }
                //}

            }
        }
        /// <summary>
        /// 点击一个节点
        /// </summary>
        /// <param name="clickTimes">点击次数</param>
        public void SetSelectedNode(int clickTimes)
        {
            //记录选择的节点
            SelectedNode = (FileNode)FilesTree.SelectedItem;
            if (SelectedNode != null)
            {
                //如果选中的节点是文件，且双击
                if (SelectedNode.IsFile)
                {
                    if (clickTimes == 2)
                    {
                        //确认是否打开非txt文件
                        if (Path.GetExtension(SelectedNode.Path) != ".txt")
                        {
                            MessageBoxResult dr = MessageBox.Show("打开非txt文件可能导致其损坏，确认打开吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.None);
                            if (dr == MessageBoxResult.Yes)
                            { goto open; }
                            else if (dr == MessageBoxResult.No || dr == MessageBoxResult.None)
                            { goto finish; }
                        }
                    open:
                        //保存当前文件再打开新文件
                        if (SelectedEssay != null && Properties.User.Default.isAutoSaveWhenSwitch == true)
                        { SaveFile(); }
                        //打开
                        OpenEssay(SelectedNode.Path);
                        //记录打开的节点
                        OpenedNode = (FileNode)FilesTree.SelectedItem;
                    //SaveFile();
                    finish:;
                    }
                    else
                    { ShowMessage("双击或按回车键打开该文章", false); }
                }
                //如果选中的节点是文件夹
                else if (SelectedNode.IsFile == false)
                {
                    //如果选中的节点是文件夹，且双击
                    if (clickTimes == 2)
                    {
                        //保存当前文件再打开新文件
                        if (SelectedEssay != null && Properties.User.Default.isAutoSaveWhenSwitch == true)
                        { SaveFile(); }
                        //打开卷册
                        OpenChapter(SelectedNode.Path);
                        //记录打开的节点
                        OpenedNode = (FileNode)FilesTree.SelectedItem;
                    }
                    else
                    { ShowMessage("双击或按回车键打开该卷册", false); }
                }
            }
        }
        /// <summary>
        /// 编码格式更改为utf-8
        /// </summary>
        /// <param name="_file">文件路径</param>
        /// <param name="encoding">编码格式</param>
        private static void SetEncodeType(string _file, Encoding encoding)
        {
            FileStream fs = new FileStream(_file, FileMode.Open, FileAccess.Read);
            byte[] flieByte = new byte[fs.Length];
            fs.Read(flieByte, 0, flieByte.Length);
            fs.Close();

            Encoding ec = Encoding.GetEncoding("UTF-8");
            StreamWriter sw = new StreamWriter(_file, false, ec);
            sw.Write(encoding.GetString(flieByte));
            sw.Close();
        }
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
        private void SelectLanguage(string language)
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

        //重设
        /// <summary>
        /// 重置偏好设置
        /// </summary>
        public void ResetPreferences()
        {
            Properties.User.Default.Reset();
        }

        //获取
        /// <summary>
        /// 统计书籍信息，获取所有txt文件数量
        /// </summary>
        /// <param name="_path">书籍路径</param>
        /// <returns>所有txt文件数量</returns>
        public static int GetFileCounts(string _path)
        {
            int n = 0;
            string[] _files = Directory.GetFiles(_path);
            n = _files.Count();
            string[] _dirs = Directory.GetDirectories(_path);
            foreach (var _dir in _dirs)
            {
                n += GetFileCounts(_dir);
            }
            return n;
        }
        /// <summary>
        /// 统计书籍信息，获取一级子目录数量
        /// </summary>
        /// <param name="_path">书籍路径</param>
        /// <returns>一级子目录数量</returns>
        public static int GetDirCounts(string _path)
        {
            string[] _dirs = Directory.GetDirectories(_path);
            return _dirs.Count();
        }
        /// <summary>
        /// 统计书籍信息，获取全书字数
        /// </summary>
        /// <param name="_path">书籍路径</param>
        /// <param name="_thisEssay">当前文章字数</param>
        /// <returns>全书字数</returns>
        public int GetBookWords(string _path, string _thisEssay)
        {
            int n = 0;
            string[] _files = Directory.GetFiles(_path);
            foreach (var _file in _files)
            {
                if (_file == _thisEssay)
                {
                    //获取当前文章字符数
                    MatchCollection space = Regex.Matches(FileContent.Text, @"\s");
                    int w = FileContent.Text.Length - space.Count;
                    n += FileContent.Text.Length - space.Count;
                }
                else
                {
                    //创建读取文件
                    FileStream fs = new FileStream(_file, FileMode.Open, FileAccess.Read);
                    StreamReader sw = new StreamReader(fs);
                    //过滤无效字符
                    string allWords = sw.ReadToEnd();
                    MatchCollection space = Regex.Matches(allWords, @"\s");
                    //获取此文章字符数
                    n += allWords.Length - space.Count;
                    sw.Close();
                    fs.Close();
                }
            }
            string[] _dirs = Directory.GetDirectories(_path);
            foreach (var _dir in _dirs)
            {
                n += GetBookWords(_dir, _thisEssay);
            }
            return n;
        }
        /// <summary>
        /// 检测文件编码格式
        /// </summary>
        /// <param name="_file">文件路径</param>
        /// <returns>编码格式</returns>
        public static Encoding GetFileEncodeType(string _file)
        {
            FileStream fs = new FileStream(_file, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            Byte[] buffer = br.ReadBytes(2);
            if (buffer[0] >= 0xEF)
            {
                if (buffer[0] == 0xEF && buffer[1] == 0xBB)
                {
                    return Encoding.UTF8;
                }
                else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                {
                    return Encoding.BigEndianUnicode;
                }
                else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                {
                    return Encoding.Unicode;
                }
                else
                {
                    return Encoding.Default;
                }
            }
            else
            {
                return Encoding.Default;
            }
        }

        //创建
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
        /// <summary>
        /// 创建右键菜单
        /// </summary>
        private void CreateContextMenu()
        {
        //    MenuSave = new MenuItem
        //    {
        //        Header = FindResource("保存文章"),
        //        InputGestureText = "Ctrl+S",
        //        IsEnabled = true
        //    };
        //    MenuSave.Click += new RoutedEventHandler(MenuSave_Click);
        //    MenuSaveAs = new MenuItem
        //    {
        //        Header = FindResource("另存为文章"),
        //        InputGestureText = "Ctrl+Shift+S",
        //        IsEnabled = true
        //    };
        //    MenuSaveAs.Click += new RoutedEventHandler(MenuSaveAs_Click);
        //    MenuExport = new MenuItem
        //    {
        //        Header = FindResource("导出全书"),
        //        InputGestureText = "Ctrl+E",
        //        IsEnabled = true
        //    };
        //    MenuExport.Click += new RoutedEventHandler(MenuExport_Click);
        //    MenuCloseBook = new MenuItem
        //    {
        //        Header = FindResource("关闭书籍"),
        //        InputGestureText = "Ctrl+Q",
        //        IsEnabled = true
        //    };
        //    MenuCloseBook.Click += new RoutedEventHandler(MenuCloseBook_Click);
        //    MenuCloseChapter = new MenuItem
        //    {
        //        Header = FindResource("关闭卷册"),
        //        InputGestureText = "Ctrl+Alt+Q",
        //        IsEnabled = true
        //    };
        //    MenuCloseChapter.Click += new RoutedEventHandler(MenuCloseChapter_Click);
        //    MenuCloseEssay = new MenuItem
        //    {
        //        Header = FindResource("关闭文章"),
        //        InputGestureText = "Ctrl+Shift+Q",
        //        IsEnabled = true
        //    };
        //    MenuCloseEssay.Click += new RoutedEventHandler(MenuCloseEssay_Click);
        //    MenuBookInfo = new MenuItem
        //    {
        //        Header = FindResource("书籍信息"),
        //        InputGestureText = "Ctrl+M",
        //        IsEnabled = true
        //    };
        //    MenuBookInfo.Click += new RoutedEventHandler(MenuBookInfo_Click);
        //    MenuChapterInfo = new MenuItem
        //    {
        //        Header = FindResource("卷册信息"),
        //        InputGestureText = "Ctrl+Alt+M",
        //        IsEnabled = true
        //    };
        //    MenuChapterInfo.Click += new RoutedEventHandler(MenuChapterInfo_Click);
        //    MenuCloseEW = new MenuItem
        //    {
        //        Header = FindResource("退出"),
        //        InputGestureText = "Alt+F4",
        //        IsEnabled = true
        //    };
        //    MenuCloseEW.Click += new RoutedEventHandler(MenuCloseEW_Click);

        //    //二级菜单实例化 MenuEdit
        //    MenuUndo = new MenuItem
        //    {
        //        Header = "撤销",
        //        InputGestureText = "Ctrl+Z"
        //    };
        //    MenuUndo.Click += new RoutedEventHandler(MenuUndo_Click);
        //    MenuRedo = new MenuItem
        //    {
        //        Header = "重做",
        //        InputGestureText = "Ctrl+Y"
        //    };
        //    MenuRedo.Click += new RoutedEventHandler(MenuRedo_Click);
        //    MenuCut = new MenuItem
        //    {
        //        Header = "剪切",
        //        InputGestureText = "Ctrl+X",
        //        IsEnabled = true
        //    };
        //    MenuCut.Click += new RoutedEventHandler(MenuCut_Click);
        //    MenuCopy = new MenuItem
        //    {
        //        Header = "复制",
        //        InputGestureText = "Ctrl+C",
        //        IsEnabled = true
        //    };
        //    MenuCopy.Click += new RoutedEventHandler(MenuCopy_Click);
        //    MenuPaste = new MenuItem
        //    {
        //        Header = "粘贴",
        //        InputGestureText = "Ctrl+V"
        //    };
        //    MenuPaste.Click += new RoutedEventHandler(MenuPaste_Click);
        //    MenuSelectAll = new MenuItem
        //    {
        //        Header = "全选",
        //        InputGestureText = "Ctrl+A"
        //    };
        //    MenuSelectAll.Click += new RoutedEventHandler(MenuSelectAll_Click);
        //    MenuFindAndReplace = new MenuItem
        //    {
        //        Header = FindResource("查找替换"),
        //        InputGestureText = "Ctrl+F",
        //        IsEnabled = true
        //    };
        //    MenuFindAndReplace.Click += new RoutedEventHandler(MenuFindAndReplace_Click);
        //    MenuToTraditional = new MenuItem
        //    {
        //        Header = "转换为繁体",
        //        IsEnabled = true
        //    };
        //    MenuToTraditional.Click += new RoutedEventHandler(MenuToTraditional_Click);
        //    MenuToSimplified = new MenuItem
        //    {
        //        Header = "转换为简体",
        //        IsEnabled = true
        //    };
        //    MenuToSimplified.Click += new RoutedEventHandler(MenuToSimplified_Click);
        //    MenuDelete = new MenuItem
        //    {
        //        Header = FindResource("删除选中"),
        //        InputGestureText = "Delete",
        //        IsEnabled = true
        //    };
        //    MenuDelete.Click += new RoutedEventHandler(MenuDelete_Click);

        //    //二级菜单实例化 MenuWindow
        //    MenuHideDir = new MenuItem
        //    {
        //        Header = FindResource("隐藏目录"),
        //        InputGestureText = "Shift+H",
        //        IsEnabled = true
        //    };
        //    MenuHideDir.Click += new RoutedEventHandler(MenuHideDir_Click);
        //    MenuRefresh = new MenuItem
        //    {
        //        Header = FindResource("刷新目录"),
        //        InputGestureText = "Ctrl+R",
        //        IsEnabled = true
        //    };
        //    MenuRefresh.Click += new RoutedEventHandler(MenuRefresh_Click);
        //    MenuExpand = new MenuItem
        //    {
        //        Header = FindResource("展开目录"),
        //        InputGestureText = "Ctrl+I",
        //        IsEnabled = true
        //    };
        //    MenuExpand.Click += new RoutedEventHandler(MenuExpand_Click);
        //    MenuCollapse = new MenuItem
        //    {
        //        Header = FindResource("收起目录"),
        //        InputGestureText = "Ctrl+U",
        //        IsEnabled = true
        //    };
        //    MenuCollapse.Click += new RoutedEventHandler(MenuCollapse_Click);
            #endregion

        }
        /// <summary>
        /// 创建计时器
        /// </summary>
        private void CreateTimer()
        {
            //设置计时器1，默认每1分钟触发一次
            AutoSaveTimer = new DispatcherTimer
            { Interval = TimeSpan.FromMinutes(Properties.User.Default.autoSaveMinute) };
            AutoSaveTimer.Tick += new EventHandler(TimerAutoSave_Tick);
            AutoSaveTimer.Start();

            //设置计时器2
            AutoBackupTimer = new DispatcherTimer
            { Interval = TimeSpan.FromMinutes(Properties.User.Default.autoBackupMinute) };
            AutoBackupTimer.Tick += new EventHandler(TimerAutoBackup_Tick);
            AutoBackupTimer.Start();
        }

        //清空
        /// <summary>
        /// 清空运行信息
        /// </summary>
        public void ClearRunInfo()
        {
            Properties.Settings.Default.Reset();
            ShowMessage("已清空运行信息", true);
        }

        //移除
        /// <summary>
        /// 深度优先移除节点
        /// </summary>
        /// <param name="Node">要删除的节点</param>
        public void RemoveFolderNode(FileNode node)
        {
            if (node.IsFile)
            {
                //父节点 
                FileNode fatherNote = FindNote(Directory.GetParent(node.Path).FullName);
                if (fatherNote != null)
                {
                    fatherNote.Items.Remove(node);
                }
                else
                {
                    FilesTree.Items.Remove(node);
                }
            }
            else
            {
                //父节点 
                FileNode fatherNote = FindNote(Directory.GetParent(node.Path).FullName);
                if (fatherNote != null)
                {
                    fatherNote.Items.Remove(node);
                }
                else
                {
                    FilesTree.Items.Remove(node);
                }
                //刷新视图
                FilesTree.Items.Refresh();
            }
        }

        //刷新
        /// <summary>
        /// 刷新右键菜单编辑菜单的控件状态
        /// </summary>
        private void RefreshElementState()
        {
            if (FileContent.SelectedText == "" || FileContent.SelectedText == null)
            {
                //MenuCut.IsEnabled = false;
                //MenuCopy.IsEnabled = false;
                //MenuToSimplified.IsEnabled = false;
                //MenuToTraditional.IsEnabled = false;
            }
            else
            {
                //MenuCut.IsEnabled = true;
                //MenuCopy.IsEnabled = true;
                //MenuToSimplified.IsEnabled = true;
                //MenuToTraditional.IsEnabled = true;
            }
        }
        /// <summary>
        /// 刷新主窗口标题
        /// </summary>
        public void RefreshTitle()
        {
            if (SelectedBook == null)
            {
                Main.Title = AppInfo.Name + " " + AppInfo.Version;
            }
            else
            {
                if (SelectedChapter == null)
                {
                    if (SelectedEssay == null)
                    {
                        Main.Title = AppInfo.Name + " - " + SelectedBook;
                    }
                    else
                    {
                        Main.Title = AppInfo.Name + " - " + SelectedBook + @"\" + SelectedEssay;
                    }
                }
                else
                {
                    if (SelectedEssay == null)
                    {
                        Main.Title = AppInfo.Name + " - " + SelectedBook + @"\" + SelectedChapterPath.Replace(SelectedBookPath + @"\", "");
                    }
                    else
                    {
                        Main.Title = AppInfo.Name + " - " + SelectedBook + @"\" + SelectedEssayPath.Replace(SelectedBookPath + @"\", "");
                    }
                }
            }
        }
        /// <summary>
        /// 强制刷新界面
        /// </summary>
        private void RefreshWindow()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(delegate (object f)
                {
                    ((DispatcherFrame)f).Continue = false;
                    return null;
                }
                    ), frame);
            Dispatcher.PushFrame(frame);
        }
        /// <summary>
        /// 刷新偏好设置
        /// </summary>
        private void RefreshPreferences()
        {
            //启动时显示运行信息
            ShowRunInfoCheckBox.IsChecked = Properties.User.Default.isShowRunInfo;
            //自动开书
            AutoOpenBookCheckBox.IsChecked = Properties.User.Default.isAutoOpenBook;
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

        //显示
        /// <summary>
        /// 显示软件信息
        /// </summary>
        private void ShowAppInfo()
        {
            ThisName.Content = AppInfo.Name;
            Description.Content = AppInfo.Description;
            Developer.Content = AppInfo.Company;
            Version.Content = AppInfo.Version;
            UpdateNote.Text = AppInfo.UpdateNote;
        }
        /// <summary>
        /// 显示运行信息
        /// </summary>
        private void ShowRunInfo()
        {
            //若第一次运行软件，提示消息
            if (Properties.Settings.Default.runTimes == 1)
            {
                EssayName.Text = FindResource("欢迎使用") + " " + AppInfo.Name;
                //显示运行记录
                FileContent.Text = FindResource("启动次数") + "：" + Properties.Settings.Default.runTimes + Environment.NewLine +
                                   FindResource("启动时间") + "：" + Properties.Settings.Default.thisStartTime + Environment.NewLine +
                                   Environment.NewLine +
                                   FindResource("单击右键获取命令");
            }
            else
            {
                EssayName.Text = FindResource("欢迎使用") + " " + AppInfo.Name;
                int d = Properties.Settings.Default.totalTime.Days;
                int h = Properties.Settings.Default.totalTime.Hours;
                int m = Properties.Settings.Default.totalTime.Minutes;
                int s = Properties.Settings.Default.totalTime.Seconds;
                string t = d + "天" + h + "时" + m + "分" + s + "秒";
                //显示运行记录
                FileContent.Text = FindResource("启动次数") + "：" + Properties.Settings.Default.runTimes + Environment.NewLine +
                                   FindResource("启动时间") + "：" + Properties.Settings.Default.thisStartTime + Environment.NewLine +
                                   Environment.NewLine +
                                   FindResource("上次启动时间") + "：" + Properties.Settings.Default.lastStartTime + Environment.NewLine +
                                   FindResource("上次关闭时间") + "：" + Properties.Settings.Default.lastEndTime + Environment.NewLine +
                                   FindResource("总运行时长") + "：" + t + Environment.NewLine +
                                   Environment.NewLine +
                                   FindResource("单击右键获取命令");
            }
        }
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="resourceName">资源名</param>
        /// <param name="newBox">是否弹出对话框</param>
        public void ShowMessage(string resourceName, bool newBox)
        {
            if (FindResource(resourceName) != null)
            {
                if (newBox)
                {
                    MessageBox.Show(FindResource(resourceName).ToString());
                }
                else
                {
                    HelpMessage.Content = FindResource(resourceName);
                }
            }
            else
            {
                if (newBox)
                {
                    MessageBox.Show(resourceName);
                }
                else
                {
                    HelpMessage.Content = resourceName;
                }
            }
        }
        /// <summary>
        /// 显示更多消息
        /// </summary>
        /// <param name="resourceName">资源名</param>
        /// <param name="moreText">附加信息</param>
        /// <param name="newBox">是否弹出对话框</param>
        public void ShowMessage(string resourceName, string moreText, bool newBox)
        {
            if (newBox)
            {
                MessageBox.Show(FindResource(resourceName) + moreText);
            }
            else
            {
                HelpMessage.Content = FindResource(resourceName) + moreText;
            }
        }

        //其它
        /// <summary>
        /// 复制文件夹
        /// </summary>
        /// <param name="_old">旧目录</param>
        /// <param name="_new">新路径</param>
        private static void CopyDirectory(string _old, string _new)
        {
            DirectoryInfo dir = new DirectoryInfo(_old);
            //获取目录下（不包含子目录）的文件和子目录
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
            foreach (FileSystemInfo i in fileinfo)
            {
                //判断是否文件夹
                if (i is DirectoryInfo)
                {
                    //目标目录下不存在此文件夹即创建子文件夹
                    if (!Directory.Exists(_new + @"\" + i.Name))
                    {
                        Directory.CreateDirectory(_new + @"\" + i.Name);
                    }
                    //递归调用复制子文件夹
                    CopyDirectory(i.FullName, _new + @"\" + i.Name);
                }
                //不是文件夹即复制文件，true表示可以覆盖同名文件
                else
                {
                    File.Copy(i.FullName, _new + @"\" + i.Name, true);
                }
            }
        }
        /// <summary>
        /// 创建文件集合
        /// </summary>
        /// <param name="bookName">书籍名称</param>
        /// <param name="_book">书籍路径</param>
        /// <returns>节点集合</returns>
        private void ScanBookPath(string _book)
        {
            //遍历当前文件夹中的文件
            foreach (FileNode fileNode in ScanFile(_book))
            {
                //BookList.Add(fileNode);
                FilesTree.Items.Add(fileNode);
            }
            //获取子文件夹信息
            DirectoryInfo DI = new DirectoryInfo(_book);
            //遍历目录里的子文件夹
            DirectoryInfo[] childFolders = DI.GetDirectories();
            //子文件夹的总数
            int j = childFolders.Length;
            //遍历当前文件夹中的子文件夹
            if (j != 0)
            {
                foreach (DirectoryInfo childFolder in childFolders)
                {
                    //遍历当前子文件夹中的子文件夹
                    FileNode node = ScanFolder(_book + @"\" + childFolder.ToString());
                    FilesTree.Items.Add(node);
                }
            }
        }
        /// <summary>
        /// 遍历书籍目录下的子文件夹，创建节点
        /// </summary>
        /// <param name="_folder">文件夹路径</param>
        private FileNode ScanFolder(string _folder)
        {
            //
            List<FileNode> thisNodes = new List<FileNode>();
            //遍历当前文件夹中的文件
            foreach (FileNode fileNode in ScanFile(_folder))
            {
                thisNodes.Add(fileNode);
            }
            //获取子文件夹信息
            DirectoryInfo DI = new DirectoryInfo(_folder);
            //遍历目录里的子文件夹
            DirectoryInfo[] childFolders = DI.GetDirectories();
            //子文件夹的总数
            int j = childFolders.Length;
            //遍历当前文件夹中的子文件夹
            if (j != 0)
            {
                foreach (DirectoryInfo childFolder in childFolders)
                {
                    //遍历当前子文件夹中的子文件夹
                    FileNode node = ScanFolder(_folder + @"\" + childFolder.ToString());
                    thisNodes.Add(node);
                }
            }
            FileNode thisNode = new FileNode(System.IO.Path.GetFileName(_folder), _folder, false, false, thisNodes);
            return thisNode;
        }
        /// <summary>
        /// 遍历此目录下的子文件，创建节点
        /// </summary>
        /// <param name="_folder">文件夹路径</param>
        private List<FileNode> ScanFile(string _folder)
        {
            //
            List<FileNode> thisNodes = new List<FileNode>();
            //
            DirectoryInfo DI = new DirectoryInfo(_folder);
            //遍历文件夹里的文件
            FileInfo[] theEssays = DI.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            //开始遍历
            foreach (FileInfo nextEssay in theEssays)
            {
                //添加文件节点
                FileNode thisNode = new FileNode(nextEssay.Name, nextEssay.FullName, true, false, null);
                thisNodes.Add(thisNode);
            }
            return thisNodes;
        }
        /// <summary>
        /// 在一级节点里寻找节点
        /// </summary>
        /// <param name="path">节点路径</param>
        /// <returns></returns>
        public FileNode FindNote(string path)
        {
            FileNode found = null;
            foreach (FileNode item in FilesTree.Items)
            {
                if (item.Path == path)
                {
                    found = item;
                    break;
                }

                List<FileNode> nos = new List<FileNode>();
                foreach (FileNode it in item.Items)
                {
                    nos.Add(it);
                }
                found = FindNote(nos, path);
            }
            return found;
        }
        /// <summary>
        /// 在节点集合里寻找节点
        /// </summary>
        /// <param name="nodes">节点集合</param>
        /// <param name="path">节点路径</param>
        /// <returns></returns>
        private FileNode FindNote(List<FileNode> nodes, string path)
        {
            FileNode found = null;
            if (nodes != null)
            {
                foreach (FileNode item in nodes)
                {
                    if (item.Path == path)
                    {
                        found = item;
                        break;
                    }

                    List<FileNode> nos = new List<FileNode>();
                    foreach (FileNode it in item.Items)
                    {
                        nos.Add(it);
                    }
                    found = FindNote(nos, path);
                }
                return found;
            }
            else
            {
                return null;
            }
        }

        //菜单：文件
        /// <summary>
        /// 新建文章
        /// </summary>
        private void CreateEssay()
        {
            if (SelectedBook == null)
            {
                ShowMessage("请选择一本书籍", true);
            }
            else
            {
                //实例化创建窗口
                CreateNewEssay Create = new CreateNewEssay()
                {
                    //获取当前MainWindow实例
                    Ow = this,
                };
                Create.ShowDialog();
            }
        }
        /// <summary>
        /// 新建卷册
        /// </summary>
        private void CreateChapter()
        {
            if (SelectedBook == null)
            {
                ShowMessage("请选择一本书籍", true);
            }
            else
            {
                //实例化创建窗口
                CreateNewChapter Create = new CreateNewChapter()
                {
                    //获取当前MainWindow实例
                    Ow = this,
                };
                Create.ShowDialog();
            }
        }
        /// <summary>
        /// 新建书籍
        /// </summary>
        private void CreateBook()
        {
            //实例化创建窗口
            CreateNewBook Create = new CreateNewBook()
            {
                //获取当前MainWindow实例
                Ow = this,
            };
            Create.ShowDialog();
        }
        /// <summary>
        /// 选择书籍
        /// </summary>
        private void SelectBook()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                SelectedPath = Properties.Settings.Default._lastBook,
                Description = "请选择书籍所在的路径：" + Environment.NewLine
                            + "注意：请确保该书籍内的txt文件都以UTF-8的格式编码，否则打开时会显示乱码。"
            };
            //弹出浏览文件夹对话框，获取文件夹路径
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (SelectedBook != null)
                {
                    CloseBook();
                }
                //获取当前书籍（文件夹）的名字、路径、根目录
                SelectedBookPath = folderBrowserDialog.SelectedPath;
                SelectedBook = Path.GetFileName(SelectedBookPath);
                Properties.Settings.Default._lastBook = SelectedBookPath;
                SaveAppSettings();
                //打开
                OpenBook(SelectedBookPath);
            }
        }
        /// <summary>
        /// 自动打开书籍文件夹
        /// </summary>
        private void AutoOpenBook()
        {
            if (Directory.Exists(Properties.Settings.Default._lastBook))
            {
                //获取当前书籍（文件夹）的名字、路径、根目录
                SelectedBook = System.IO.Path.GetFileName(Properties.Settings.Default._lastBook);
                SelectedBookPath = Properties.Settings.Default._lastBook;
                //同步书籍列表选项
                ChangeBook();
                //显示书籍信息
                GetBookInfo();
                FilesTree.ToolTip = FindResource("当前书籍") + "：" + SelectedBook + Environment.NewLine + FindResource("书籍位置") + "：" + SelectedBookPath;
                //改变标题
                RefreshTitle();
                //改变控件状态
                SetElementState(1);
                //重新导入目录
                ReloadFilesTree();

                //显示消息
                ShowMessage("已自动打开书籍", " " + SelectedBook, false);
            }
            else
            {
                ShowMessage("自动打开书籍失败", true);
            }
        }
        /// <summary>
        /// 打开书籍
        /// </summary>
        /// <param name="_path">书籍文件夹路径</param>
        public void OpenBook(string _path)
        {
            if (Directory.Exists(_path))
            {
                //刷新当前卷册
                SelectedChapter = null;
                SelectedChapterPath = null;
                SelectedEssay = null;
                SelectedEssayPath = null;
                //刷新根目录
                Properties.User.Default.BooksDir = Path.GetDirectoryName(SelectedBookPath);
                //加入书籍列表
                AddBooks(true, SelectedBook, SelectedBookPath);
                ChangeBook();
                //显示书籍信息
                GetBookInfo();
                FilesTree.ToolTip = FindResource("当前书籍") + "：" + SelectedBook + Environment.NewLine + FindResource("书籍位置") + "：" + SelectedBookPath;
                //刷新标题
                RefreshTitle();
                //改变控件状态
                SetElementState(1);
                //重新导入目录
                ReloadFilesTree();

                //显示消息
                ShowMessage("已打开书籍", " " + SelectedBook, false);
            }
            else
            {
                ShowMessage("此书籍不存在", true);
            }
        }
        /// <summary>
        /// 打开卷册
        /// </summary>
        /// <param name="_path">书籍文件夹路径</param>
        public void OpenChapter(string _path)
        {
            if (Directory.Exists(_path))
            {
                SelectedChapterPath = _path;
                SelectedChapter = System.IO.Path.GetFileName(SelectedChapterPath);
                SelectedEssayPath = null;
                SelectedEssay = null;
                //获取卷册信息
                GetChapterInfo();
                //刷新标题
                RefreshTitle();
                //更改主窗口控件状态
                SetElementState(2);

                //提示消息
                ShowMessage("已打开卷册", " " + SelectedChapter, false);
            }
            else
            {
                ShowMessage("此卷册不存在", true);
            }
        }
        /// <summary>
        /// 打开文章
        /// </summary>
        /// <param name="_path">文章路径</param>
        public void OpenEssay(string _path)
        {
            if (File.Exists(_path))
            {
                //刷新选择信息
                SelectedEssayPath = _path;
                SelectedEssay = System.IO.Path.GetFileName(SelectedEssayPath);
                SelectedChapterPath = System.IO.Path.GetDirectoryName(SelectedEssayPath);
                SelectedChapter = System.IO.Path.GetFileName(SelectedChapterPath);
                //如果上级文件夹是书籍文件夹
                if (SelectedChapterPath == SelectedBookPath)
                {
                    SelectedChapterPath = null;
                    SelectedChapter = null;
                }

                //提示消息
                ShowMessage("正在读取文本", false);
                RefreshWindow();

                //清空文本框
                FileContent.Text = "";
                //创建读取文件
                FileStream fs = new FileStream(_path, FileMode.Open, FileAccess.Read);
                StreamReader sw = new StreamReader(fs);
                //开始写入值  
                FileContent.Text = sw.ReadToEnd();
                sw.Close();
                fs.Close();
                //刷新文件名
                EssayName.Text = System.IO.Path.GetFileNameWithoutExtension(SelectedEssayPath);
                //改变控件状态
                SetElementState(3);
                RefreshTitle();
                //光标到最后
                FileContent.Focus();
                FileContent.Select(FileContent.Text.Length, 0);
                FileContent.ScrollToEnd();
                IsSaved = true;

                //提示消息
                ShowMessage("已打开文章", " " + SelectedEssay, false);

            }
            else
            {
                ShowMessage("此文章不存在", true);
            }
        }
        /// <summary>
        /// 书籍列表添加一个书籍选项项
        /// </summary>
        /// <param name="isAdd"></param>
        /// <param name="book">书名</param>
        /// <param name="_book">书籍路径</param>
        public void AddBooks(bool isAdd, string book, string _book)
        {
            Books.ToolTip = FindResource("最近打开过的书籍列表");
            bool hasBook = false;
            //检测列表是否有此书
            if (Books.Items.Count > 0)
            {
                foreach (var item in Books.Items)
                {
                    Grid grid = item as Grid;
                    System.Windows.Controls.Label tb = grid.Children[0] as System.Windows.Controls.Label;
                    System.Windows.Controls.Button btn = grid.Children[1] as System.Windows.Controls.Button;
                    if (tb.Tag.ToString() == SelectedBookPath)
                    {
                        hasBook = true;
                        break;
                    }
                }
            }
            if (!hasBook)
            {
                Grid grid = new Grid
                {
                    Height = 30,
                    Width = 261,
                    //Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0))
                };
                System.Windows.Controls.Label tb = new System.Windows.Controls.Label
                {
                    FontSize = 18,
                    Content = book,
                    Tag = _book,
                    ToolTip = _book,
                    Height = 30,
                    Width = grid.Width - 35,
                    //Margin = new Thickness(0,0,35,0),
                    Padding = new Thickness(0, 0, 0, 0),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
                    Background = new SolidColorBrush(Color.FromArgb(0, 210, 210, 210)),
                    //Cursor = System.Windows.Input.Cursors.Hand
                };
                //double d = Books.Width - 30;
                //Thickness tn1 = new Thickness(d, 0, 0, 0);
                System.Windows.Controls.Button btn = new System.Windows.Controls.Button
                {
                    FontSize = 18,
                    Content = "x",
                    Tag = _book,
                    ToolTip = FindResource("删除此书的打开纪录"),
                    Height = 30,
                    Width = 30,
                    //Margin = new Thickness(210, 0, 0, 0),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 100, 150)),
                    Background = new SolidColorBrush(Color.FromArgb(0, 210, 210, 220)),
                    BorderBrush = null,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                btn.Click += new RoutedEventHandler(BtnRemoveBookHistory_Click);
                grid.Children.Add(tb);
                grid.Children.Add(btn);
                Books.Items.Add(grid);

                if (isAdd)
                {
                    //记录的书籍数+1
                    Properties.Settings.Default.bookCounts += 1;
                    SaveAppSettings();
                }
            }
        }
        /// <summary>
        /// 书籍列表选择了其他书籍
        /// </summary>
        public void ChangeBook()
        {
            foreach (var item in Books.Items)
            {
                Grid grid = item as Grid;
                System.Windows.Controls.Label tb = grid.Children[0] as System.Windows.Controls.Label;
                System.Windows.Controls.Button btn = grid.Children[1] as System.Windows.Controls.Button;
                if (tb.Tag.ToString() == SelectedBookPath)
                {
                    Books.SelectedItem = item;
                    break;
                }
            }
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        public void SaveFile()
        {
            if (File.Exists(SelectedEssayPath))
            {
                //创建写入文件
                FileStream fs = new FileStream(SelectedEssayPath, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                //开始写入值  
                sw.Write(FileContent.Text);
                sw.Close();
                fs.Close();
                IsSaved = true;
                //显示消息
                ShowMessage("保存成功", false);
            }
            else
            {
                ShowMessage("保存失败", false);
            }
        }
        /// <summary>
        /// 另存为文件
        /// </summary>
        public void SaveFileAs()
        {
            Stream st;
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "txt文件（*.txt）|*.txt",
                FilterIndex = 2,
                RestoreDirectory = true
            };
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if ((st = sfd.OpenFile()) != null)
                {
                    using (StreamWriter sw = new StreamWriter(st, Encoding.UTF8))
                    {
                        sw.Write(FileContent.Text);
                        sw.Close();
                    };
                    st.Close();
                    //显示消息
                    ShowMessage("另存为成功", false);
                }
            }
        }
        /// <summary>
        /// 准备导出全书
        /// </summary>
        public void CheckExport()
        {
            //保存文章
            if (SelectedEssay != null)
            {
                SaveFile();
            }
            //导出路径
            string output;
            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                SelectedPath = System.IO.Path.GetDirectoryName(Properties.Settings.Default._lastBook),
                Description = FindResource("选择导出目录").ToString()
            };
            //按下确定选择的按钮，获取文件夹路径
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                output = fbd.SelectedPath + @"\" + SelectedBook + ".txt";
                if (File.Exists(output))
                {
                    MessageBoxResult dr = MessageBox.Show(FindResource("导出路径已存在").ToString(), FindResource("提示").ToString(), MessageBoxButton.OKCancel);
                    if (dr == MessageBoxResult.OK)
                    {
                        Export(output);
                    }
                    else
                    {
                        ShowMessage("已取消导出", false);
                    }
                }
                else
                {
                    Export(output);
                }
            }
        }
        /// <summary>
        /// 导出全书
        /// </summary>
        /// <param name="_output">导出的路径</param>
        public void Export(string _output)
        {
            //显示消息
            ShowMessage("正在导出txt", false);
            RefreshWindow();

            File.CreateText(_output).Close();
            //书名
            //File.AppendAllText(output, selectedBook + Environment.NewLine);
            foreach (FileNode txt in FilesTree.Items)
            {
                if (!txt.IsFile)
                {
                    //换行
                    File.AppendAllText(_output, Environment.NewLine);
                    //添加卷册名
                    File.AppendAllText(_output, txt.Name);
                    //换行
                    File.AppendAllText(_output, Environment.NewLine);
                    //换行
                    File.AppendAllText(_output, Environment.NewLine);
                }
                else
                {
                    //添加文章名
                    File.AppendAllText(_output, System.IO.Path.GetFileNameWithoutExtension(txt.Path));
                    //换行
                    File.AppendAllText(_output, Environment.NewLine);
                    //获取所有字符
                    string[] txtText = File.ReadAllLines(txt.Path, Encoding.UTF8);
                    File.AppendAllLines(_output, txtText);
                    //换行
                    File.AppendAllText(_output, Environment.NewLine);
                    //换行
                    File.AppendAllText(_output, Environment.NewLine);
                }
            }
            //创建读取文件
            FileStream fs = new FileStream(_output, FileMode.Open, FileAccess.Read);
            StreamReader sw = new StreamReader(fs);
            //过滤无效字符
            string allWords = sw.ReadToEnd();
            MatchCollection space = Regex.Matches(allWords, @"\s");
            //获取此文章字符数
            int l = allWords.Length - space.Count;
            sw.Close();
            fs.Close();

            //显示消息
            ShowMessage("导出成功", " " + l, false);
            Process.Start(System.IO.Path.GetDirectoryName(_output));
        }
        /// <summary>
        /// 关闭当前书籍
        /// </summary>
        public void CloseBook()
        {
            CloseChapter();
            //备份
            Backup();
            //刷新显示信息
            EssayName.Text = FindResource("欢迎使用") + " " + AppInfo.Name;
            EssayName.ToolTip = null;
            FileContent.Text = FindResource("创建或打开以开始") + Environment.NewLine +
                               FindResource("单击右键获取命令");
            Words.ToolTip = null;
            Words.Content = FindResource("字数") + "：0";
            FilesTree.ToolTip = FindResource("创建或打开以开始");
            //清空
            SelectedBook = null;
            SelectedBookPath = null;
            //改变控件状态
            SetElementState(0);
            //改变标题
            RefreshTitle();
            //文件树数据清空
            FilesTree.Items.Clear();
            //刷新目录
            //FilesTree.Items.Refresh();
            //显示消息
            ShowMessage("已关闭书籍", " " + SelectedBook, false);
        }
        /// <summary>
        /// 关闭当前卷册
        /// </summary>
        public void CloseChapter()
        {
            CloseEssay();
            if (SelectedChapter != null)
            {
                //清空
                SelectedChapter = null;
                SelectedChapterPath = null;
                //显示书籍信息
                GetBookInfo();
                //改变控件状态
                SetElementState(1);
                //改变标题
                RefreshTitle();
            }
        }
        /// <summary>
        /// 关闭当前文章
        /// </summary>
        public void CloseEssay()
        {
            if (SelectedEssay != null)
            {
                //保存当前文件
                if (Properties.User.Default.isAutoSaveWhenSwitch == true)
                { SaveFile(); }
                //刷新显示信息
                EssayName.Text = null;
                EssayName.ToolTip = null;
                FileContent.Text = null;
                Words.Content = FindResource("字数") + "：0";
                Words.ToolTip = null;
                //清空
                SelectedEssay = null;
                SelectedEssayPath = null;
                //改变控件状态
                if (SelectedChapter != null)
                {
                    GetChapterInfo();
                    SetElementState(2);
                }
                else
                { SetElementState(1); }
                //改变标题
                RefreshTitle();
                //显示消息
                ShowMessage("已关闭文章", " " + SelectedEssay, false);
            }
        }
        /// <summary>
        /// 删除文章
        /// </summary>
        public void DeleteEssay()
        {
            //系统自动提示窗  
            MessageBoxResult result = MessageBox.Show("是否删除该文章？", "删除项目", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                //获取对应文件路径
                string _path = SelectedNode.Path;
                string name = SelectedNode.Header.ToString();
                //删除节点
                RemoveFolderNode(SelectedNode);
                //删除文件
                File.Delete(_path);
                //如果选择的是正在编辑的
                if (_path == SelectedEssayPath)
                {
                    //清空当前打开的文章信息
                    SelectedEssay = null;
                    SelectedEssayPath = null;
                    //提示消息
                    EssayName.Text = FindResource("当前未选中任何文章").ToString();
                    EssayName.ToolTip = null;
                    FileContent.Text = null;
                    //改变控件状态
                    SetElementState(2);
                    //改变标题
                    RefreshTitle();
                }
                //显示消息
                ShowMessage("已删除文章", " " + name, false);
            }

        }
        /// <summary>
        /// 删除卷册
        /// </summary>
        public void DeleteChapter()
        {
            //系统自动提示窗  
            MessageBoxResult result = MessageBox.Show("是否删除该卷册？", "删除项目", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                //获取对应文件路径
                string _path = SelectedNode.Path;
                string name = SelectedNode.Header.ToString();
                //删除节点
                RemoveFolderNode(SelectedNode);
                //删除卷册
                Directory.Delete(_path, true);
                //如果选择的是正在编辑的卷册
                if (_path == SelectedChapterPath)
                {
                    //清空当前打开的卷册信息
                    SelectedEssay = null;
                    SelectedEssayPath = null;
                    SelectedChapter = null;
                    SelectedChapterPath = null;
                    //提示消息
                    EssayName.Text = FindResource("当前未选中任何文章").ToString();
                    EssayName.ToolTip = null;
                    FileContent.Text = null;
                    //改变控件状态
                    SetElementState(1);
                    //改变标题
                    RefreshTitle();
                }
                //显示消息
                ShowMessage("已删除卷册", " " + name, false);
            }

        }
        /// <summary>
        /// 书籍信息
        /// </summary>
        public void GetBookInfo()
        {
            if (SelectedBookPath != null)
            {
                Words.Content = FindResource("字数") + "：0";
                EssayName.Text = SelectedBook;
                FileContent.Text = FindResource("创建时间") + "：" + Directory.GetCreationTime(SelectedBookPath) + Environment.NewLine +
                                   FindResource("子卷册数") + "：" + GetDirCounts(SelectedBookPath) + Environment.NewLine +
                                   FindResource("总文章数") + "：" + GetFileCounts(SelectedBookPath) + Environment.NewLine +
                                   FindResource("总字数") + "：" + GetBookWords(SelectedBookPath, "");
            }
            else
            {

            }
        }
        /// <summary>
        /// 卷册信息
        /// </summary>
        public void GetChapterInfo()
        {
            if (SelectedChapterPath != null)
            {
                Words.Content = FindResource("字数") + "：0";
                EssayName.Text = SelectedChapter;
                FileContent.Text = FindResource("创建时间") + "：" + Directory.GetCreationTime(SelectedChapterPath) + Environment.NewLine +
                                   FindResource("子卷册数") + "：" + GetDirCounts(SelectedChapterPath) + Environment.NewLine +
                                   FindResource("总文章数") + "：" + GetFileCounts(SelectedChapterPath) + Environment.NewLine +
                                   FindResource("总字数") + "：" + GetBookWords(SelectedChapterPath, "");
            }
            else
            {

            }
        }

        //菜单：编辑
        /// <summary>
        /// 获取行和列
        /// </summary>
        private void GetRowAndColumn()
        {
            if (SelectedEssay != null)
            {
                //得到总行数。该行数会随着文本框的大小改变而改变；若只认回车符为一行(不考虑排版变化)请用 总行数=textBox1.Lines.Length;(记事本2是这种方式)
                //int totalline = FileContent.GetLineFromCharIndex(FileContent.Text.Length) + 1;
                //得到当前行的行号,从0开始，习惯是从1开始，所以+1.
                int line = FileContent.GetLineIndexFromCharacterIndex(FileContent.SelectionStart) + 1;
                //得到当前行第一个字符的索引
                int index = FileContent.GetCharacterIndexFromLineIndex(line - 1);
                //.SelectionStart得到光标所在位置的索引 减去 当前行第一个字符的索引 = 光标所在的列数（从0开始)
                int column = FileContent.SelectionStart - index + 1;
                string rac = FindResource("行").ToString() + "：" + line + "    " + FindResource("列").ToString() + "：" + column;
                RowAndColumn.Content = rac;
            }
            else
            {
                RowAndColumn.Content = FindResource("行").ToString() + "：" + 0 + "    " + FindResource("列").ToString() + "：" + 0; ;
            }
        }
        /// <summary>
        /// 自动缩进
        /// </summary>
        private void Indentation()
        {
            int start = FileContent.SelectionStart;
            string spaces = "";
            for (int i = 0; i < Properties.User.Default.autoIndentations; i++)
            {
                spaces += " ";
            }
            FileContent.Text = FileContent.Text.Insert(start, spaces);
            FileContent.Select(start + spaces.Length, 0);

        }
        /// <summary>
        /// 自动备份
        /// </summary>
        private void Backup()
        {
            //await Task.Run(() =>
            //{
            if (Properties.User.Default.isAutoBackup == true)
            {
                if (SelectedBook != null && Directory.Exists(SelectedBookPath))
                {
                    ShowMessage("书籍备份中", false);
                    RefreshWindow();
                    string _path = Properties.User.Default.BackupDir + @"\" + SelectedBook;
                    //删除上个备份
                    if (Directory.Exists(_path))
                    { Directory.Delete(_path, true); }
                    //创建新的备份
                    Directory.CreateDirectory(_path);
                    CopyDirectory(SelectedBookPath, _path);
                    //显示消息
                    //Dispatcher.BeginInvoke(new Action(delegate { HelpMessage.Content = "书籍已自动备份于 " + DateTime.Now.ToLongTimeString().ToString(); }));
                    ShowMessage("已自动备份于", DateTime.Now.ToLongTimeString().ToString(), false);
                }
            }
            //});
        }
        /// <summary>
        /// 查找和替换
        /// </summary>
        private void FindAndReplace()
        {
            //实例化创建窗口
            FindAndReplace Create = new FindAndReplace()
            {
                //获取当前MainWindow实例
                Ow = this,
            };
            Create.Show();
        }
        /// <summary>
        /// 设置字体
        /// </summary>
        public void SetFont(string fontName)
        {
            foreach (FontFamily font in Fonts.SystemFontFamilies)
            {
                if (fontName == font.Source)
                {
                    FileContent.FontFamily = font;
                    //储存更改
                    Properties.User.Default.fontName = fontName;
                    SavePreferences();
                    //EssayName.FontFamily = font;
                    break;
                }
            }
        }
        /// <summary>
        /// 简转繁
        /// </summary>
        /// <param name="simplifiedChinese">文字内容</param>
        /// <returns>繁体文字内容</returns>
        private static string ToTraditional(string simplifiedChinese)
        {
            string traditionalChinese = string.Empty;
            System.Globalization.CultureInfo vCultureInfo = new System.Globalization.CultureInfo("zh-CN", false);
            traditionalChinese = Microsoft.VisualBasic.Strings.StrConv(simplifiedChinese, Microsoft.VisualBasic.VbStrConv.TraditionalChinese, vCultureInfo.LCID);
            return traditionalChinese;
        }
        /// <summary>
        /// 繁转简
        /// </summary>
        /// <param name="traditionalChinese">文字内容</param>
        /// <returns>简体文字内容</returns>
        private static string ToSimplified(string traditionalChinese)
        {
            string simplifiedChinese = string.Empty;
            System.Globalization.CultureInfo vCultureInfo = new System.Globalization.CultureInfo("zh-CN", false);
            simplifiedChinese = Microsoft.VisualBasic.Strings.StrConv(traditionalChinese, Microsoft.VisualBasic.VbStrConv.SimplifiedChinese, vCultureInfo.LCID);
            return simplifiedChinese;
        }

        //菜单：窗口
        /// <summary>
        /// 展开目录
        /// </summary>
        public void ExpandTree()
        {
            foreach (FileNode item in FilesTree.Items)
            {
                //DependencyObject dObject = FilesTree.ItemContainerGenerator.ContainerFromItem(item);
                //((FileNode)dObject).ExpandSubtree();
                item.ExpandSubtree();
            }
        }
        /// <summary>
        /// 收起目录
        /// </summary>
        public void CollapseTree()
        {
            foreach (FileNode item in FilesTree.Items)
            {
                //DependencyObject dObject = FilesTree.ItemContainerGenerator.ContainerFromItem(item);
                //FileNode tvi = (FileNode)dObject;
                //tvi.IsExpanded = false;
                item.IsExpanded = false;
            }
        }
        /// <summary>
        /// 重新导入书籍目录
        /// </summary>
        public void ReloadFilesTree()
        {
            FilesTree.Items.Clear();
            ScanBookPath(SelectedBookPath);
            //提示消息
            ShowMessage("目录已重新导入", false);
        }
        /// <summary>
        /// 隐藏目录区
        /// </summary>
        public void HidePanCenter()
        {
            if (PanCenter.Visibility == Visibility.Visible)
            {
                PanCenter.Visibility = Visibility.Collapsed;
            }
        }

        #region 事件
        //窗口事件
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAppInfo();

            SetAppSettingsOnStart();

            CheckFolders();

            CreateContextMenu();
            CreateTimer();

            LoadThemes();
            LoadBookHistory();
            LoadPreferences();

            RefreshTitle();

            SetElementState(0);


            LoadLanguage();
            LoadThemeItem();
            LoadFontsItem();
            ShowAppInfo();
            RefreshPreferences();
            ShowMessage("已载入", false);
        }
        private void Main_Closing(object sender, CancelEventArgs e)
        {
            if (SelectedBook != null)
            {
                //确认是否保存文件再关闭
                if (SelectedEssay != null && IsSaved == false)
                {
                    MessageBoxResult dr = MessageBox.Show(FindResource("文章还未保存").ToString(), FindResource("提示").ToString(), MessageBoxButton.YesNoCancel, MessageBoxImage.None);
                    if (dr == MessageBoxResult.Yes)
                    {
                        SaveFile();
                        CloseBook();
                    }
                    else if (dr == MessageBoxResult.No)
                    {
                        CloseBook();
                    }
                    else if (dr == MessageBoxResult.Cancel || dr == MessageBoxResult.None)
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    CloseBook();
                }
            }

            if (Properties.Settings.Default.runTimes > 0)
            {
                SetAppSettingsOnQuit();
            }
        }
        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            //若打开了书
            if (SelectedBook != null)
            {
                //Ctrl+Alt+B 创建卷册
                if (e.Key == Key.B && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                                   && (e.KeyboardDevice.IsKeyDown(Key.LeftAlt) || e.KeyboardDevice.IsKeyDown(Key.RightAlt)))
                {
                    CreateChapter();
                    //e.Handled = true;
                }
                //Ctrl+Shift+B 创建文章
                if (e.Key == Key.B && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                                   && (e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift)))
                { CreateEssay(); }
                //Ctrl+E 导出全文
                if (e.Key == Key.E && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
                { CheckExport(); }
                //Ctrl+M 书籍信息
                if (e.Key == Key.M && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                                   && !(e.KeyboardDevice.IsKeyDown(Key.LeftAlt) || e.KeyboardDevice.IsKeyDown(Key.RightAlt)))
                { GetBookInfo(); }
                //Ctrl+Q 关闭书籍
                if (e.Key == Key.Q && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                                   && !(e.KeyboardDevice.IsKeyDown(Key.LeftAlt) || e.KeyboardDevice.IsKeyDown(Key.RightAlt))
                                   && !(e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift)))
                {
                    CloseBook();
                    Books.SelectedItem = null;
                }
                //Ctrl+I 展开目录 
                if (e.Key == Key.I && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
                { ExpandTree(); }
                //Ctrl+U 收起目录
                if (e.Key == Key.U && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
                { CollapseTree(); }
                //Ctrl+R 刷新目录
                if (e.Key == Key.R && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
                { ReloadFilesTree(); }

                //若打开了章节
                if (SelectedChapter != null)
                {
                    //Ctrl+Alt+Q 关闭卷册
                    if (e.Key == Key.Q && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                                       && (e.KeyboardDevice.IsKeyDown(Key.LeftAlt) || e.KeyboardDevice.IsKeyDown(Key.RightAlt)))
                    {
                        CloseChapter();
                    }
                }

                //若打开了文章
                if (SelectedEssay != null)
                {
                    //Ctrl+Shift+S 另存为
                    if (e.Key == Key.S && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                                       && (e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift)))
                    { SaveFileAs(); }
                    //Ctrl+S 保存
                    if (e.Key == Key.S && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
                    { SaveFile(); }
                    //Ctrl+Alt+S 一键全部保存
                    //if (e.Key == Key.S && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
                    //{ SaveFile(); }
                    //Ctrl+F 查找替换
                    if (e.Key == Key.F && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
                    { FindAndReplace(); }
                    //Ctrl+Shift+Q 关闭文章
                    if (e.Key == Key.Q && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                                       && (e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift)))
                    {
                        CloseEssay();
                    }
                    //Ctrl+Alt+M 卷册信息
                    if (e.Key == Key.M && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                                       && (e.KeyboardDevice.IsKeyDown(Key.LeftAlt) || e.KeyboardDevice.IsKeyDown(Key.LeftAlt)))
                    {
                        CloseEssay();
                    }
                }
            }
            //Ctrl+O 打开书籍
            if (e.Key == Key.O && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            { SelectBook(); }
            //Ctrl+B 创建书籍
            if (e.Key == Key.B && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            { CreateBook(); }
            //FT 切换下个主题
            if (e.Key == Key.T && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            { SetNextTheme(); }
        }

        private void EssayName_GotFocus(object sender, RoutedEventArgs e)
        {
            ShowMessage("在此处重命名", false);
        }
        private void EssayName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (EssayName.Text != null && EssayName.Text != "")
                {
                    //检测格式是否正确
                    if (CheckIsRightName(EssayName.Text))
                    {
                        if (SelectedEssay != null)
                        {
                            //删除原文件
                            File.Delete(SelectedEssayPath);
                            //获取新名字
                            SelectedEssay = EssayName.Text + ".txt";
                            if (SelectedChapter == null)
                            {
                                SelectedEssayPath = SelectedBookPath + @"\" + SelectedEssay;
                            }
                            else
                            {
                                SelectedEssayPath = SelectedChapterPath + @"\" + SelectedEssay;
                            }
                            //创建新文件
                            File.Create(SelectedEssayPath).Close();
                            //创建写入文件
                            SaveFile();

                            //重新导入目录
                            ReloadFilesTree();

                            //显示消息
                            ShowMessage("文章重命名成功", false);
                        }
                        else if (SelectedChapter != null)
                        {
                            //获取旧卷册路径
                            string _old = SelectedChapterPath;
                            //更新卷册名称和路径
                            SelectedChapter = EssayName.Text;
                            SelectedChapterPath = System.IO.Path.GetDirectoryName(SelectedChapterPath) + @"\" + SelectedChapter;
                            //MessageBox.Show(System.IO.Path.GetDirectoryName(_selectedChapter));
                            Directory.CreateDirectory(SelectedChapterPath);
                            if (_old != SelectedChapterPath)
                            {
                                //拷贝文件
                                CopyDirectory(_old, SelectedChapterPath);
                                //删除旧目录
                                Directory.Delete(_old, true);

                                //重新导入目录
                                ReloadFilesTree();

                                //显示消息
                                ShowMessage("卷册重命名成功", false);
                            }
                        }
                    }
                    else
                        ShowMessage("重命名中不能含有以下字符", " \\ | / < > \" ? * :", false);
                }
                else
                    ShowMessage("重命名不能为空", false);
            }
        }

        private void FileContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedEssay != null)
            {
                //过滤无效字符
                MatchCollection space = Regex.Matches(FileContent.Text, @"\s");
                //MatchCollection newLine = Regex.Matches(FileContent.Text, @"\r");
                int w = FileContent.Text.Length - space.Count;
                //刷新字数
                Words.Content = FindResource("字数").ToString() + "：" + w;
                Words.ToolTip = FindResource("全书字数").ToString() + "：" + GetBookWords(SelectedBookPath, SelectedEssayPath);
                //显示消息
                ShowMessage("正在编辑", false);
                IsSaved = false;
                if (w > 100000)
                {
                    ShowMessage("控制字数", true);
                }
            }
        }
        private void FileContent_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //NodePath.Text = e.Text;
            //自动补全
            if (Properties.User.Default.isAutoCompletion)
            {
                //记录光标位置
                int position = FileContent.SelectionStart;
                //记录滚动条位置
                double s = FileContent.VerticalOffset;
                if (e.Text.Equals("("))
                { FileContent.Text = FileContent.Text.Insert(FileContent.SelectionStart, ")"); FileContent.Select(position, 0); }
                else if (e.Text.Equals("（"))
                { FileContent.Text = FileContent.Text.Insert(FileContent.SelectionStart, "）"); FileContent.Select(position, 0); }
                else if (e.Text.Equals("["))
                { FileContent.Text = FileContent.Text.Insert(FileContent.SelectionStart, "]"); FileContent.Select(position, 0); }
                else if (e.Text.Equals("【"))
                { FileContent.Text = FileContent.Text.Insert(FileContent.SelectionStart, "】"); FileContent.Select(position, 0); }
                else if (e.Text.Equals("{"))
                { FileContent.Text = FileContent.Text.Insert(FileContent.SelectionStart, "}"); FileContent.Select(position, 0); }
                else if (e.Text.Equals("'"))
                { FileContent.Text = FileContent.Text.Insert(FileContent.SelectionStart, "'"); FileContent.Select(position, 0); }
                else if (e.Text.Equals("‘"))
                { FileContent.Text = FileContent.Text.Insert(FileContent.SelectionStart, "’"); FileContent.Select(position, 0); }
                else if (e.Text.Equals("’"))
                { FileContent.Text = FileContent.Text.Insert(FileContent.SelectionStart - 1, "‘"); FileContent.Select(position, 0); }
                else if (e.Text.Equals("\""))
                { FileContent.Text = FileContent.Text.Insert(FileContent.SelectionStart, "\""); FileContent.Select(position, 0); }
                else if (e.Text.Equals("“"))
                { FileContent.Text = FileContent.Text.Insert(FileContent.SelectionStart, "”"); FileContent.Select(position, 0); }
                else if (e.Text.Equals("”"))
                { FileContent.Text = FileContent.Text.Insert(FileContent.SelectionStart - 1, "“"); FileContent.Select(position, 0); }
                else if (e.Text.Equals("<"))
                { FileContent.Text = FileContent.Text.Insert(FileContent.SelectionStart, ">"); FileContent.Select(position, 0); }
                else if (e.Text.Equals("《"))
                { FileContent.Text = FileContent.Text.Insert(FileContent.SelectionStart, "》"); FileContent.Select(position, 0); }
                //FileContent.Select(position, 0);
                FileContent.ScrollToVerticalOffset(s);
            }
        }
        private void FileContent_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Properties.User.Default.isAutoIndentation)
                {
                    Indentation();
                }
            }
        }
        private void FileContent_SelectionChanged(object sender, RoutedEventArgs e)
        {
            GetRowAndColumn();
            RefreshElementState();
        }
        private void FileContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GetRowAndColumn();
        }
        private void FileContent_LostFocus(object sender, RoutedEventArgs e)
        {
            RowAndColumn.Content = FindResource("行").ToString() + "：" + 0 + "    " + FindResource("列").ToString() + "：" + 0; ;
        }

        private void FilesTree_KeyUp(object sender, KeyEventArgs e)
        {
            //Enter选择节点
            if (e.Key == Key.Enter)
            {
                SetSelectedNode(2);
            }
            //Delete 删除节点
            if (e.Key == Key.Delete && SelectedNode != null)
            {
                if (SelectedNode.IsFile == true && SelectedNode.Name != null)
                {
                    DeleteEssay();
                }
                else if (SelectedNode.IsFile == false && SelectedNode.Name != null)
                {
                    DeleteChapter();
                }
                else
                {
                    //显示消息
                    ShowMessage("请选择一个项目再删除", false);
                }
                e.Handled = true;
            }
        }
        private void FilesTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetSelectedNode(1);
        }
        private void FilesTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SetSelectedNode(2);
        }

        private void Books_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var item in Books.Items)
            {
                Grid grid = item as Grid;
                grid.Width = Books.ActualWidth + 18;
                System.Windows.Controls.Label tb = grid.Children[0] as System.Windows.Controls.Label;
                //System.Windows.Controls.Button btn = grid.Children[1] as System.Windows.Controls.Button;
                if (grid.Width - 35 > 0)
                {
                    tb.Width = grid.Width - 35;
                }
            }
        }
        private void Books_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Books.SelectedItem != null)
            {
                //关闭书籍
                if (SelectedBook != null)
                { CloseBook(); }

                Grid grid = Books.SelectedItem as Grid;
                System.Windows.Controls.Label tb = grid.Children[0] as System.Windows.Controls.Label;
                System.Windows.Controls.Button btn = grid.Children[1] as System.Windows.Controls.Button;
                string tmp1 = tb.Content.ToString();
                string tmp2 = tb.Tag.ToString();
                //检测书籍文件夹是否存在
                if (Directory.Exists(tmp2))
                {
                    //获取当前书籍（文件夹）的名字、路径，并记录
                    SelectedBook = tmp1;
                    SelectedBookPath = tmp2;
                    Properties.Settings.Default._lastBook = SelectedBookPath;
                    SaveAppSettings();
                    //打开
                    OpenBook(SelectedBookPath);
                }
                else
                {
                    Books.Items.Remove(Books.SelectedItem);
                    ShowMessage("此书籍不存在", true);
                }
            }
        }


        //按钮点击事件
        private void BtnFile_Click(object sender, RoutedEventArgs e)
        {
            CenterDirectoryPage.Visibility = Visibility.Visible;
            CenterSettingPage.Visibility = Visibility.Collapsed;
            CenterAboutPage.Visibility = Visibility.Collapsed;

            BtnsFile.Visibility = Visibility.Visible;
            BtnsEdit.Visibility = Visibility.Collapsed;
            BtnsView.Visibility = Visibility.Collapsed;
            BtnsSetting.Visibility = Visibility.Collapsed;
            BtnsAbout.Visibility = Visibility.Collapsed;
        }
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            CenterDirectoryPage.Visibility = Visibility.Visible;
            CenterSettingPage.Visibility = Visibility.Collapsed;
            CenterAboutPage.Visibility = Visibility.Collapsed;

            BtnsFile.Visibility = Visibility.Collapsed;
            BtnsEdit.Visibility = Visibility.Visible;
            BtnsView.Visibility = Visibility.Collapsed;
            BtnsSetting.Visibility = Visibility.Collapsed;
            BtnsAbout.Visibility = Visibility.Collapsed;
        }
        private void BtnView_Click(object sender, RoutedEventArgs e)
        {
            CenterDirectoryPage.Visibility = Visibility.Visible;
            CenterSettingPage.Visibility = Visibility.Collapsed;
            CenterAboutPage.Visibility = Visibility.Collapsed;

            BtnsFile.Visibility = Visibility.Collapsed;
            BtnsEdit.Visibility = Visibility.Collapsed;
            BtnsView.Visibility = Visibility.Visible;
            BtnsSetting.Visibility = Visibility.Collapsed;
            BtnsAbout.Visibility = Visibility.Collapsed;
        }
        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            CenterDirectoryPage.Visibility = Visibility.Collapsed;
            CenterSettingPage.Visibility = Visibility.Visible;
            CenterAboutPage.Visibility = Visibility.Collapsed;

            BtnsFile.Visibility = Visibility.Collapsed;
            BtnsEdit.Visibility = Visibility.Collapsed;
            BtnsView.Visibility = Visibility.Collapsed;
            BtnsSetting.Visibility = Visibility.Visible;
            BtnsAbout.Visibility = Visibility.Collapsed;
        }
        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            CenterDirectoryPage.Visibility = Visibility.Collapsed;
            CenterSettingPage.Visibility = Visibility.Collapsed;
            CenterAboutPage.Visibility = Visibility.Visible;

            BtnsFile.Visibility = Visibility.Collapsed;
            BtnsEdit.Visibility = Visibility.Collapsed;
            BtnsView.Visibility = Visibility.Collapsed;
            BtnsSetting.Visibility = Visibility.Collapsed;
            BtnsAbout.Visibility = Visibility.Visible;
        }

        private void BtnCreateEssay_Click(object sender, RoutedEventArgs e)
        {
            CreateEssay();
        }
        private void BtnCreateChapter_Click(object sender, RoutedEventArgs e)
        {
            CreateChapter();
        }
        private void BtnCreateBook_Click(object sender, RoutedEventArgs e)
        {
            CreateBook();
        }
        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            SelectBook();
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }
        private void BtnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileAs();
        }
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            CheckExport();
        }
        private void BtnCloseBook_Click(object sender, RoutedEventArgs e)
        {
            CloseBook();
            Books.SelectedItem = null;
        }
        private void BtnCloseChapter_Click(object sender, RoutedEventArgs e)
        {
            CloseChapter();
        }
        private void BtnCloseEssay_Click(object sender, RoutedEventArgs e)
        {
            CloseEssay();
        }
        private void BtnBookInfo_Click(object sender, RoutedEventArgs e)
        {
            GetBookInfo();
        }
        private void BtnChapterInfo_Click(object sender, RoutedEventArgs e)
        {
            GetChapterInfo();
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedNode != null)
            {
                if (SelectedNode.IsFile == true && SelectedNode.Name != null)
                {
                    DeleteEssay();
                }
                else if (SelectedNode.IsFile == false && SelectedNode.Name != null)
                {
                    DeleteChapter();
                }
                else
                {
                    //显示消息
                    ShowMessage("请选择一个项目再删除", false);
                }
            }
            else
            {
                //显示消息
                ShowMessage("请选择一个项目再删除", false);
            }
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            FileContent.Undo();
        }
        private void BtnRedo_Click(object sender, RoutedEventArgs e)
        {
            FileContent.Redo();
        }
        private void BtnCut_Click(object sender, RoutedEventArgs e)
        {
            FileContent.Cut();
        }
        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            FileContent.Copy();
        }
        private void BtnPaste_Click(object sender, RoutedEventArgs e)
        {
            FileContent.Paste();
        }
        private void BtnToTraditional_Click(object sender, RoutedEventArgs e)
        {
            if (FileContent.IsFocused)
            {
                FileContent.SelectedText = ToTraditional(FileContent.SelectedText);
            }
        }
        private void BtnToSimplified_Click(object sender, RoutedEventArgs e)
        {
            if (FileContent.IsFocused)
            {
                FileContent.SelectedText = ToSimplified(FileContent.SelectedText);
            }
        }
        private void BtnFindAndReplace_Click(object sender, RoutedEventArgs e)
        {
            FindAndReplace();
        }

        private void BtnFold_Click(object sender, RoutedEventArgs e)
        {
            if (PanCenter.Visibility == Visibility.Visible)
            {
                PanCenter.Visibility = Visibility.Collapsed;
            }
            else
            {
                PanCenter.Visibility = Visibility.Visible;
            }
        }
        private void BtnExpand_Click(object sender, RoutedEventArgs e)
        {
            ExpandTree();
        }
        private void BtnCollapse_Click(object sender, RoutedEventArgs e)
        {
            CollapseTree();
        }
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            //重新导入目录
            ReloadFilesTree();
        }

        private void BtnCheckNew_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.InfoPage);
        }
        private void BtnClearRunInfo_Click(object sender, RoutedEventArgs e)
        {
            ClearRunInfo();
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
        private void BtnGitHubPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.GitHubPage);
        }
        private void BtnQQGroup_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.QQGroup);
        }
        private void BtnBitCoinAddress_Click(object sender, RoutedEventArgs e)
        {
            //Process.Start("explorer.exe", AppInfo.BitCoinAddress);
        }

        //设置更改事件
        private void ShowRunInfo_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isShowRunInfo = true;
            SavePreferences();
            //显示消息
            ShowMessage("已更改", false);
        }
        private void ShowRunInfo_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isShowRunInfo = false;
            SavePreferences();
            //显示消息
            ShowMessage("已更改", false);
        }
        private void AutoOpenBook_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoOpenBook = true;
            SavePreferences();
            //显示消息
            ShowMessage("已更改", false);
        }
        private void AutoOpenBook_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoOpenBook = false;
            SavePreferences();
            //显示消息
            ShowMessage("已更改", false);
        }
        private void AutoBrackets_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoBrackets = true;
            SavePreferences();
            //显示消息
            ShowMessage("已更改", false);
        }
        private void AutoBrackets_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoBrackets = false;
            SavePreferences();
            //显示消息
            ShowMessage("已更改", false);
        }
        private void AutoCompletion_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoCompletion = true;
            SavePreferences();
            //显示消息
            ShowMessage("已更改", false);
        }
        private void AutoCompletion_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoCompletion = false;
            SavePreferences();
            //显示消息
            ShowMessage("已更改", false);
        }
        private void AutoIndentation_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoIndentation = true;
            SavePreferences();
            AutoIndentations.IsEnabled = true;
            //显示消息
            ShowMessage("已更改", false);
        }
        private void AutoIndentation_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoIndentation = false;
            SavePreferences();
            AutoIndentations.IsEnabled = false;
            //显示消息
            ShowMessage("已更改", false);
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
                        SavePreferences();
                        //显示消息
                        ShowMessage("已更改", false);
                    }
                    else
                    {
                        AutoIndentations.Text = Properties.User.Default.autoIndentations.ToString();
                        ShowMessage("输入1~999整数", false);
                    }
                }
                catch (Exception)
                {
                    AutoIndentations.Text = Properties.User.Default.autoIndentations.ToString();
                    ShowMessage("输入整数", false);
                }
            }
        }
        private void AutoSaveWhenSwitch_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoSaveWhenSwitch = true;
            SavePreferences();
            //显示消息
            ShowMessage("已更改", false);
        }
        private void AutoSaveWhenSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoSaveWhenSwitch = false;
            SavePreferences();
            //显示消息
            ShowMessage("已更改", false);
        }
        private void AutoSaveEvery_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoSaveEvery = true;
            SavePreferences();
            AutoSaveTime.IsEnabled = true;
            //显示消息
            ShowMessage("已更改", false);
        }
        private void AutoSaveEvery_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoSaveEvery = false;
            SavePreferences();
            AutoSaveTime.IsEnabled = false;
            //显示消息
            ShowMessage("已更改", false);
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
                        SavePreferences();
                        AutoSaveTimer.Interval = ts;
                        //显示消息
                        ShowMessage("已更改", false);
                    }
                    else
                    {
                        AutoSaveTime.Text = Properties.User.Default.autoSaveMinute.ToString();
                        ShowMessage("输入1~999整数", false);
                    }
                }
                catch (Exception)
                {
                    AutoSaveTime.Text = Properties.User.Default.autoSaveMinute.ToString();
                    ShowMessage("输入整数", false);
                }
            }
        }
        private void AutoBackup_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoBackup = true;
            SavePreferences();
            AutoBackupTime.IsEnabled = true;
            //显示消息
            ShowMessage("已更改", false);
        }
        private void AutoBackup_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isAutoBackup = false;
            SavePreferences();
            AutoBackupTime.IsEnabled = false;
            //显示消息
            ShowMessage("已更改", false);
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
                        SavePreferences();
                        AutoBackupTimer.Interval = ts;
                        //显示消息
                        ShowMessage("已更改", false);
                    }
                    else
                    {
                        AutoBackupTime.Text = Properties.User.Default.autoBackupMinute.ToString();
                        ShowMessage("输入1~999整数", false);
                    }
                }
                catch (Exception)
                {
                    AutoBackupTime.Text = Properties.User.Default.autoBackupMinute.ToString();
                    ShowMessage("输入整数", false);
                }
            }
        }
        private void ShowScrollBar_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isShowScrollBar = true;
            SavePreferences();
            FileContent.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //显示消息
            ShowMessage("已更改", false);
        }
        private void ShowScrollBar_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isShowScrollBar = false;
            SavePreferences();
            FileContent.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //显示消息
            ShowMessage("已更改", false);
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
                    SetTheme(tmp);
                    //储存更改
                    Properties.User.Default.ThemePath = tmp;
                    SavePreferences();
                    //显示消息
                    ShowMessage("已更改", false);
                }
                else
                {
                    Skins.Items.Remove(Skins.SelectedItem);
                    ShowMessage("该主题的配置文件不存在", false);
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
                        langRd = System.Windows.Application.LoadComponent(new Uri(@"语言/zh_CN.xaml", UriKind.Relative)) as ResourceDictionary;
                    }
                    else
                    {
                        langRd = System.Windows.Application.LoadComponent(new Uri(@"语言/" + langName + ".xaml", UriKind.Relative)) as ResourceDictionary;
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
                    if (Resources.MergedDictionaries.Count > 0)
                    { Resources.MergedDictionaries.Clear(); }
                    Resources.MergedDictionaries.Add(langRd);
                    //保存设置
                    Properties.User.Default.language = langName;
                    SavePreferences();
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
                        SavePreferences();
                        FileContent.FontSize = i;
                        ShowMessage("已更改", false);
                    }
                    else
                    {
                        TextSize.Text = Properties.User.Default.fontSize.ToString();
                        ShowMessage("输入1~999整数", false);
                    }
                }
                catch (Exception)
                {
                    TextSize.Text = Properties.User.Default.fontSize.ToString();
                    ShowMessage("输入整数", false);
                }
            }
        }
        private void CBFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBlock item = CBFonts.SelectedItem as TextBlock;
            //调用方法
            SetFont(item.Text);
            //储存更改
            Properties.User.Default.fontName = item.Text;
            SavePreferences();
            //显示消息
            ShowMessage("已更改", false);
        }
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetPreferences();
            RefreshPreferences();
            ShowMessage("已重置", false);
        }
        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {

        }

        //其它事件
        private void TimerAutoSave_Tick(object sender, EventArgs e)
        {
            if (SelectedEssay != null && Properties.User.Default.isAutoSaveEvery == true)
            {
                SaveFile();
                //显示消息
                ShowMessage("已自动保存于", DateTime.Now.ToLongTimeString().ToString(), false);
            }
        }
        private void TimerAutoBackup_Tick(object sender, EventArgs e)
        {
            //timer2.Stop();
            Backup();
        }
        private void BtnRemoveBookHistory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button closeBtn = sender as System.Windows.Controls.Button;
            foreach (var item in Books.Items)
            {
                Grid grid = item as Grid;
                System.Windows.Controls.Button btn = grid.Children[1] as System.Windows.Controls.Button;
                if (closeBtn.Tag.ToString() == btn.Tag.ToString())
                {
                    if (btn.Tag.ToString() == SelectedBookPath)
                    {
                        CloseBook();
                    }
                    Books.Items.Remove(grid);
                    Properties.Settings.Default._books = Properties.Settings.Default._books.Replace(btn.Tag.ToString(), "");
                    break;
                }
            }
        }
        #endregion
    }


    /// <summary>
    /// 书籍的节点
    /// </summary>
    public class FileNode : TreeViewItem
    {
        /// <summary>
        /// 节点类型
        /// </summary>
        public bool IsFile { get; set; }
        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 书籍节点构造器
        /// </summary>
        /// <param name="nodes">子节点集</param>
        /// <param name="id">ID</param>
        /// <param name="pid">父ID</param>
        /// <param name="DisplayName">显示名</param>
        /// <param name="_fileOrFolder">路径</param>
        /// <param name="isFile">是否文件</param>
        /// <param name="isExpanded">是否展开</param>
        public FileNode(string DisplayName, string _fileOrFolder, bool isFile, bool isExpanded, List<FileNode> nodes)
        {
            this.Header = DisplayName;
            this.Path = _fileOrFolder;
            this.IsFile = isFile;
            this.IsExpanded = isExpanded;
            if (nodes != null)
            {
                foreach (FileNode item in nodes)
                {
                    this.Items.Add(item);
                }
            }
        }
    }
    /// <summary>
    /// 书的信息，用于书籍列表打开记录
    /// </summary>
    public struct MyBook
    {
        public string name;
        public string path;
        //TextBlock Book;
    }
}
