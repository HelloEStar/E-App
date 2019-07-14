﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using System.Diagnostics;
using System.Reflection;

using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.MessageBox;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using Button = System.Windows.Controls.Button;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

using Settings = E.Writer.Properties.Settings;
using User = E.Writer.Properties.User;
using E.Utility;

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
        /// 
        /// </summary>
        private FileOrFolderInfo CurrentBook { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private FileOrFolderInfo CurrentChapter { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private FileOrFolderInfo CurrentEssay { get; set; }

        /// <summary>
        /// 打开的文件是否已保存
        /// </summary>
        private bool IsSaved { get; set; } = true;
        /// <summary>
        /// 当前匹配文字
        /// </summary>
        private string CurrentFindText { get; set; }
        /// <summary>
        /// 替换文字
        /// </summary>
        private string ReplaceText { get; set; }
        /// <summary>
        /// 下个匹配文字的索引
        /// </summary>
        private int NextStartIndex { get; set; }
        /// <summary>
        /// 选中的节点
        /// </summary>
        private TreeViewItemNode SelectedNode { get; set; }

        /// <summary>
        /// 自动保存计时器
        /// </summary>
        private DispatcherTimer AutoSaveTimer { get; set; }
        /// <summary>
        /// 自动备份计时器
        /// </summary>
        private DispatcherTimer AutoBackupTimer { get; set; }
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
            string homePage = "http://estar.zone";
            string infoPage = "http://estar.zone/introduction/e-writer/";
            string downloadPage = "http://estar.zone/introduction/e-writer/";
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
            //启动时显示运行信息
            ShowRunInfoCheckBox.IsChecked = User.Default.isShowRunInfo;
            //自动开书
            AutoOpenBookCheckBox.IsChecked = User.Default.isAutoOpenBook;
            //自动补全
            AutoCompletion.IsChecked = User.Default.isAutoCompletion;
            //自动缩进
            AutoIndentation.IsChecked = User.Default.isAutoIndentation;
            //缩进数
            AutoIndentations.Text = User.Default.autoIndentations.ToString();
            //缩进数可编辑性
            if ((bool)AutoIndentation.IsChecked) { AutoIndentations.IsEnabled = true; }
            else { AutoIndentations.IsEnabled = false; }
            //定时自动保存
            AutoSaveEvery.IsChecked = User.Default.isAutoSaveEvery;
            //定时自动保存时间间隔
            AutoSaveTime.Text = User.Default.autoSaveMinute.ToString();
            //时间间隔可编辑性
            if ((bool)AutoSaveEvery.IsChecked) { AutoSaveTime.IsEnabled = true; }
            else { AutoSaveTime.IsEnabled = false; }
            //切换自动保存
            AutoSaveWhenSwitch.IsChecked = User.Default.isAutoSaveWhenSwitch;
            //定时自动备份
            AutoBackup.IsChecked = User.Default.isAutoBackup;
            //自动备份时间间隔
            AutoBackupTime.Text = User.Default.autoBackupMinute.ToString();
            //自动备份可编辑性
            if ((bool)AutoBackup.IsChecked) { AutoBackupTime.IsEnabled = true; }
            else { AutoBackupTime.IsEnabled = false; }
            AutoBackup.ToolTip = "备份位置：" + Application.StartupPath + @"\" + User.Default.BackupDir;
            //字体尺寸
            TextSize.Text = User.Default.fontSize.ToString();

            //刷新选中项
            SelectLanguageItem(User.Default.language);
            SelectThemeItem(User.Default.ThemePath);
            SelectFontItem(User.Default.fontName);
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
        /// <summary>
        /// 获取字体选项
        /// </summary>
        private void LoadFontItems()
        {
            CbbFonts.Items.Clear();
            foreach (FontFamily font in Fonts.SystemFontFamilies)
            {
                //动态的添加一个ListViewItem项
                TextBlock label = new TextBlock
                {
                    Text = font.Source,
                    FontFamily = font
                };
                CbbFonts.Items.Add(label);
            }
        }
        /// <summary>
        /// 载入书籍选项
        /// </summary>
        private void LoadBookItems()
        {
            //验证上次关闭的书籍路径是否存在，若不存在，重置为根目录
            if (!Directory.Exists(Settings.Default._lastBook))
            {
                Settings.Default._lastBook = User.Default.BooksDir;
                SaveAppSettings();
            }
            //读取集合所有打开过的书籍路径的单字符串，并加入书籍列表
            if (Settings.Default._books != null && Settings.Default._books != "")
            {
                string[] _myB = Regex.Split(Settings.Default._books, "///");
                foreach (var b in _myB)
                {
                    if (Directory.Exists(b))
                    {
                        AddBookItem(false, new FileOrFolderInfo(b));
                    }
                }
            }
            else
            {
                Books.ToolTip = FindResource("未打开任何书籍");
            }
        }
        /// <summary>
        /// 重新导入书籍目录
        /// </summary>
        private void ReloadFilesTree()
        {
            FilesTree.Items.Clear();
            ScanBookPath(CurrentBook.Path);
            //提示消息
            ShowMessage("目录已重新导入");
        }

        //打开
        /// <summary>
        /// 打开新书籍
        /// </summary>
        private void OpenNewBook()
        {
            string path;
            if (!Directory.Exists(Settings.Default._lastBook))
            {
                path = User.Default.BooksDir;
            }
            else
            {
                path = Settings.Default._lastBook;
            }
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                SelectedPath = path,
                Description = "请选择书籍所在的路径：" + Environment.NewLine
                            + "注意：请确保该书籍内的txt文件都以UTF-8的格式编码，否则打开时会显示乱码。"
            };
            //弹出浏览文件夹对话框，获取文件夹路径
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (CurrentBook != null)
                {
                    CloseBook();
                }
                //获取当前书籍（文件夹）的名字、路径、根目录
                CurrentBook = new FileOrFolderInfo(folderBrowserDialog.SelectedPath);
                Settings.Default._lastBook = CurrentBook.Path;
                SaveAppSettings();
                //打开
                OpenBook(CurrentBook);
            }
        }
        /// <summary>
        /// 打开书籍
        /// </summary>
        /// <param name="_path">书籍文件夹路径</param>
        private void OpenBook(FileOrFolderInfo book)
        {
            if (Directory.Exists(book.Path))
            {
                CurrentChapter = null;
                CurrentEssay = null;
                TbxFileName.IsEnabled = false;
                TbxFileContent.IsEnabled = false;
                TbxCreatePath.Text = book.Path;
                User.Default.BooksDir = Path.GetDirectoryName(book.Path);
                //加入书籍列表
                AddBookItem(true, book);
                SelectBookItem(CurrentBook);
                GetBookInfo();
                FilesTree.ToolTip = FindResource("当前书籍") + "：" + book.Name + Environment.NewLine + FindResource("书籍位置") + "：" + book.Path;
                RefreshTitle();
                RefreshBtnsState();
                ReloadFilesTree();

                ShowMessage("已打开书籍", " " + book.Name);
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
        private void OpenChapter(string _path)
        {
            if (Directory.Exists(_path))
            {
                CurrentChapter = new FileOrFolderInfo(_path);
                CurrentEssay = null;
                TbxFileName.IsEnabled = true;
                TbxFileContent.IsEnabled = false;
                TbxCreatePath.Text = _path;
                GetChapterInfo();
                RefreshTitle();
                RefreshBtnsState();

                ShowMessage("已打开卷册", " " + CurrentChapter.Name);
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
        private void OpenEssay(string _path)
        {
            if (File.Exists(_path))
            {
                //刷新选择信息
                CurrentEssay = new FileOrFolderInfo(_path);
                CurrentChapter = new FileOrFolderInfo(Path.GetDirectoryName(CurrentEssay.Path));
                //如果上级文件夹是书籍文件夹
                if (CurrentChapter.Path == CurrentBook.Path)
                {
                    CurrentChapter = null;
                }

                ShowMessage("正在读取文本");
                RefreshWindow();

                //创建读取文件
                FileStream fs = new FileStream(_path, FileMode.Open, FileAccess.Read);
                StreamReader sw = new StreamReader(fs);
                TbxFileContent.Text = sw.ReadToEnd();
                sw.Close();
                fs.Close();
                TbxFileName.Text = Path.GetFileNameWithoutExtension(CurrentEssay.Path);
                TbxFileName.IsEnabled = true;
                TbxFileContent.IsEnabled = true;
                RefreshBtnsState();
                RefreshTitle();
                //光标到最后
                TbxFileContent.Focus();
                TbxFileContent.Select(TbxFileContent.Text.Length, 0);
                TbxFileContent.ScrollToEnd();
                IsSaved = true;

                ShowMessage("已打开文章", " " + CurrentEssay.Name);

            }
            else
            {
                ShowMessage("此文章不存在", true);
            }
        }
        /// <summary>
        /// 打开上次关闭的书籍
        /// </summary>
        private void OpenLastBook()
        {
            foreach (ComboBoxItem item in Books.Items)
            {
                if (item.Tag.ToString() == Settings.Default._lastBook)
                {
                    Books.SelectedItem = item;
                    AutoOpenBook();
                    break;
                }
            }
        }
        /// <summary>
        /// 自动打开书籍文件夹
        /// </summary>
        private void AutoOpenBook()
        {
            if (Directory.Exists(Settings.Default._lastBook))
            {
                //获取当前书籍（文件夹）的名字、路径、根目录
                CurrentBook = new FileOrFolderInfo(Settings.Default._lastBook);
                //同步书籍列表选项
                SelectBookItem(CurrentBook);
                //显示书籍信息
                GetBookInfo();
                FilesTree.ToolTip = FindResource("当前书籍") + "：" + CurrentBook.Name + Environment.NewLine + FindResource("书籍位置") + "：" + CurrentBook.Path;
                //改变标题
                RefreshTitle();
                //改变控件状态
                RefreshBtnsState();
                //重新导入目录
                ReloadFilesTree();

                //显示消息
                ShowMessage("已自动打开书籍", " " + CurrentBook.Name);
            }
            else
            {
                ShowMessage("自动打开书籍失败", true);
            }
        }

        //关闭
        /// <summary>
        /// 关闭当前书籍
        /// </summary>
        private void CloseBook()
        {
            if (CurrentBook != null)
            {
                CloseChapter();
                Backup();

                TbxFileName.Text = FindResource("欢迎使用") + " " + AppInfo.Name;
                TbxFileName.ToolTip = null;
                TbxFileContent.Text = FindResource("创建或打开以开始").ToString();
                Words.ToolTip = null;
                Words.Text = FindResource("字数") + "：0";
                FilesTree.ToolTip = FindResource("创建或打开以开始");

                ShowMessage("已关闭书籍", " " + CurrentBook.Name);
                CurrentBook = null;
                RefreshBtnsState();
                RefreshTitle();
                FilesTree.Items.Clear();
                //刷新目录
                //FilesTree.Items.Refresh();
            }
        }
        /// <summary>
        /// 关闭当前卷册
        /// </summary>
        private void CloseChapter()
        {
            if (CurrentChapter != null)
            {
                CloseEssay();
                CurrentChapter = null;
                GetBookInfo();
                RefreshBtnsState();
                RefreshTitle();
            }
        }
        /// <summary>
        /// 关闭当前文章
        /// </summary>
        private void CloseEssay()
        {
            if (CurrentEssay != null)
            {
                if (User.Default.isAutoSaveWhenSwitch == true)
                { SaveFile(); }

                TbxFileName.Text = null;
                TbxFileName.ToolTip = null;
                TbxFileContent.Text = null;
                TbxFileContent.IsEnabled = false;

                Words.Text = FindResource("字数") + "：0";
                Words.ToolTip = null;

                ShowMessage("已关闭文章", " " + CurrentEssay.Name);
                CurrentEssay = null;
                if (CurrentChapter != null)
                {
                    GetChapterInfo();
                    TbxFileName.IsEnabled = true;
                }
                else
                {
                    TbxFileName.IsEnabled = false;
                }

                SelectedNode = null;

                RefreshBtnsState();
                RefreshTitle();
            }
        }

        //保存
        /// <summary>
        /// 保存文件
        /// </summary>
        private void SaveFile()
        {
            if (CurrentEssay != null)
            {
                if (File.Exists(CurrentEssay.Path))
                {
                    //创建写入文件
                    FileStream fs = new FileStream(CurrentEssay.Path, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                    //开始写入值  
                    sw.Write(TbxFileContent.Text);
                    sw.Close();
                    fs.Close();
                    IsSaved = true;
                    //显示消息
                    ShowMessage("保存成功");
                }
                else
                {
                    ShowMessage("保存失败");
                }
            }
        }
        /// <summary>
        /// 另存为文件
        /// </summary>
        private void SaveFileAs()
        {
            if (CurrentEssay != null)
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
                            sw.Write(TbxFileContent.Text);
                            sw.Close();
                        };
                        st.Close();
                        //显示消息
                        ShowMessage("另存为成功");
                    }
                }
            }
        }
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
        /// <summary>
        /// 保存书籍历史
        /// </summary>
        private void SaveBookHistory()
        {
            if (Books.Items.Count > 0)
            {
                //记录最近打开过的书籍，将所有路径集合到一个字符串
                Settings.Default._books = "";
                foreach (ComboBoxItem item in Books.Items)
                {
                    Settings.Default._books += item.Tag.ToString() + "///";
                }
                Settings.Default._books = Settings.Default._books.Substring(0, Settings.Default._books.Length - 3);
            }
            Settings.Default.thisEndTime = DateTime.Now;
            Settings.Default.lastStartTime = Settings.Default.thisStartTime;
            Settings.Default.lastEndTime = Settings.Default.thisEndTime;
            Settings.Default.thisTotalTime = Settings.Default.thisEndTime - Settings.Default.thisStartTime;
            Settings.Default.totalTime = Settings.Default.thisTotalTime + Settings.Default.totalTime;
            SaveAppSettings();
        }

        //创建
        /// <summary>
        /// 创建
        /// </summary>
        private void Create()
        {
            if (PanCenter.Visibility == Visibility.Collapsed)
            {
                PanCenter.Visibility = Visibility.Visible;
                BtnFold.BorderThickness = new Thickness(4, 0, 0, 0);
            }

            if (PanCreate.Visibility == Visibility.Visible)
            {
                PanCreate.Visibility = Visibility.Collapsed;
                BtnCrete.BorderThickness = new Thickness(0, 0, 0, 0);
            }
            else
            {
                PanCreate.Visibility = Visibility.Visible;
                BtnCrete.BorderThickness = new Thickness(4, 0, 0, 0);
            }

            //获取上级目录
            if (Settings.Default._lastBook == User.Default.BooksDir)
            {
                TbxCreatePath.Text = User.Default.BooksDir;
            }
            else
            {
                TbxCreatePath.Text = Path.GetDirectoryName(Settings.Default._lastBook);
            }

            if (CurrentBook != null)
            {
                TbxCreatePath.Text = CurrentBook.Path;
            }

            if (TbxCreatePath.Text == null || TbxCreatePath.Text == "")
            {
                BtnCreateBook.ToolTip = "请设置存放位置";
                BtnCreateChapter.ToolTip = "请设置存放位置";
                BtnCreateEssay.ToolTip = "请设置存放位置";
            }
            else
            {
                BtnCreateBook.ToolTip = TbxCreatePath.Text + @"\" + TbxCreateName.Text;
                BtnCreateChapter.ToolTip = TbxCreatePath.Text + @"\" + TbxCreateName.Text;
                BtnCreateEssay.ToolTip = TbxCreatePath.Text + @"\" + TbxCreateName.Text + ".txt";
            }

            TbxCreateName.Text = "未命名";
        }
        /// <summary>
        /// 创建书籍
        /// </summary>
        private void CreateBook()
        {
            if (TbxCreatePath.Text == null || TbxCreatePath.Text == "")
            {
                MessageBox.Show("请设置存放位置");
                return;
            }
            if (TbxCreateName.Text == null || TbxCreateName.Text == "")
            {
                MessageBox.Show("请输入书籍名");
                return;
            }
            if (!CheckIsRightName(TbxCreateName.Text))
            {
                MessageBox.Show("名称中不能含有以下字符 \\ | / < > \" ? * : ");
                return;
            }

            string path = BtnCreateBook.ToolTip.ToString();
            if (CurrentBook != null)
            {
                if (path.Contains(CurrentBook.Path) && Path.GetDirectoryName(path) != Path.GetDirectoryName(CurrentBook.Path))
                {
                    MessageBox.Show("请勿把新书籍创建在另一个书籍里");
                    return;
                }
            }
            if (Directory.Exists(path))
            {
                MessageBox.Show("同名书籍已存在，请换个名字");
                return;
            }

            //保存当前文件再创建书籍
            if (CurrentEssay != null && User.Default.isAutoSaveWhenSwitch == true)
            {
                SaveFile();
            }
            CurrentBook = new FileOrFolderInfo(path);
            //创建书籍文件夹
            Directory.CreateDirectory(CurrentBook.Path);
            //记录
            Settings.Default._lastBook = CurrentBook.Path;
            SaveAppSettings();
            //打开
            OpenBook(CurrentBook);
        }
        /// <summary>
        /// 创建卷册
        /// </summary>
        private void CreateChapter()
        {
            if (TbxCreatePath.Text == null || TbxCreatePath.Text == "")
            {
                MessageBox.Show("请设置存放位置");
                return;
            }
            if (TbxCreateName.Text == null || TbxCreateName.Text == "")
            {
                MessageBox.Show("请输入卷册名");
                return;
            }
            if (!CheckIsRightName(TbxCreateName.Text))
            {
                MessageBox.Show("名称中不能含有以下字符 \\ | / < > \" ? * : ");
                return;
            }

            string path = BtnCreateChapter.ToolTip.ToString();
            if (!path.Contains(CurrentBook.Path) || path == CurrentBook.Path)
            {
                MessageBox.Show("请勿把卷册创建在书籍外");
                return;
            }
            if (Directory.Exists(path))
            {
                MessageBox.Show("同名卷册已存在，请换个名字");
                return;
            }

            //创建前保存文章
            if (CurrentEssay != null)
            {
                SaveFile();
            }
            //创建卷册
            Directory.CreateDirectory(path);
            OpenChapter(path);
            RefreshBtnsState();
            RefreshTitle();

            //创建节点
            TreeViewItemNode newfolderNode = new TreeViewItemNode(CurrentChapter.Name, CurrentChapter.Path, false, false, null);
            TreeViewItemNode fatherNote = FindNote(Path.GetDirectoryName(CurrentChapter.Path));
            if (fatherNote == null)
            {
                FilesTree.Items.Add(newfolderNode);
            }
            else
            {
                fatherNote.IsExpanded = true;
                fatherNote.Items.Add(newfolderNode);
            }
            SelectedNode = newfolderNode;
            FilesTree.Items.Refresh();
        }
        /// <summary>
        /// 创建文章
        /// </summary>
        private void CreateEssay()
        {
            if (TbxCreatePath.Text == null || TbxCreatePath.Text == "")
            {
                MessageBox.Show("请设置存放位置");
                return;
            }
            if (TbxCreateName.Text == null || TbxCreateName.Text == "")
            {
                MessageBox.Show("请输入文章名");
                return;
            }
            if (!CheckIsRightName(TbxCreateName.Text))
            {
                MessageBox.Show("名称中不能含有以下字符 \\ | / < > \" ? * : ");
                return;
            }

            string path = BtnCreateEssay.ToolTip.ToString();
            if (!path.Contains(CurrentBook.Path) || path.Replace(".txt", "") == CurrentBook.Path)
            {
                MessageBox.Show("请勿把文章创建在书籍外");
                return;
            }
            if (File.Exists(path))
            {
                MessageBox.Show("同名文章已存在，请换个名字");
                return;
            }

            //创建前保存文章
            if (CurrentEssay != null)
            {
                SaveFile();
            }
            File.Create(path).Close();
            CurrentEssay = new FileOrFolderInfo(path);
            HelpMessage.Text = "已创建文章 " + CurrentEssay.Name;
            OpenEssay(path);
            RefreshBtnsState();
            RefreshTitle();

            //创建节点
            TreeViewItemNode newFileNode = new TreeViewItemNode(CurrentEssay.Name, CurrentEssay.Path, true, false, null);
            TreeViewItemNode fatherNote = FindNote(Path.GetDirectoryName(CurrentEssay.Path));
            if (fatherNote == null)
            {
                FilesTree.Items.Add(newFileNode);
            }
            else
            {
                fatherNote.IsExpanded = true;
                fatherNote.Items.Add(newFileNode);
            }
            //选择节点
            SelectedNode = newFileNode;
            //刷新目录
            FilesTree.Items.Refresh();
        }
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
        /// 创建计时器
        /// </summary>
        private void CreateTimer()
        {
            //设置计时器1，默认每1分钟触发一次
            AutoSaveTimer = new DispatcherTimer
            { Interval = TimeSpan.FromMinutes(User.Default.autoSaveMinute) };
            AutoSaveTimer.Tick += new EventHandler(TimerAutoSave_Tick);
            AutoSaveTimer.Start();

            //设置计时器2
            AutoBackupTimer = new DispatcherTimer
            { Interval = TimeSpan.FromMinutes(User.Default.autoBackupMinute) };
            AutoBackupTimer.Tick += new EventHandler(TimerAutoBackup_Tick);
            AutoBackupTimer.Start();
        }

        //添加
        /// <summary>
        /// 书籍列表添加一个书籍选项项
        /// </summary>
        /// <param name="isAdd"></param>
        /// <param name="book">书名</param>
        /// <param name="_book">书籍路径</param>
        private void AddBookItem(bool isAdd, FileOrFolderInfo book)
        {
            Books.ToolTip = FindResource("最近打开过的书籍列表");
            bool hasBook = false;
            //检测列表是否有此书
            if (Books.Items.Count > 0)
            {
                foreach (ComboBoxItem item in Books.Items)
                {
                    if (CurrentBook != null)
                    {
                        if (item.Tag.ToString() == CurrentBook.Path)
                        {
                            hasBook = true;
                            break;
                        }
                    }
                }
            }
            if (!hasBook)
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Content = book.Name,
                    Tag = book.Path,
                    ToolTip = book.Path,
                };

                Books.Items.Add(item);

                if (isAdd)
                {
                    //记录的书籍数+1
                    Settings.Default.bookCounts += 1;
                    SaveAppSettings();
                }
            }
        }
        /// <summary>
        /// 增加运行次数记录
        /// </summary>
        private void AddRunTime()
        {
            //运行次数+1
            Settings.Default.runTimes += 1;
            //记录启动时间
            Settings.Default.thisStartTime = DateTime.Now;
            SaveAppSettings();
        }

        //移除
        /// <summary>
        /// 深度优先移除节点
        /// </summary>
        /// <param name="Node">要删除的节点</param>
        private void RemoveFolderNode(TreeViewItemNode node)
        {
            if (node.IsFile)
            {
                //父节点 
                TreeViewItemNode fatherNote = FindNote(Directory.GetParent(node.ToolTip.ToString()).FullName);
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
                TreeViewItemNode fatherNote = FindNote(Directory.GetParent(node.ToolTip.ToString()).FullName);
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
        /// <summary>
        /// 移除书籍打开记录
        /// </summary>
        /// <param name="book"></param>
        private void RemoveBookHistory(FileOrFolderInfo book)
        {
            foreach (ComboBoxItem item in Books.Items)
            {
                if (item.Tag.ToString() == book.Path)
                {
                    CloseBook();
                    Books.Items.Remove(item);
                    Settings.Default._books = Settings.Default._books.Replace(book.Path, "");
                    Settings.Default.Save();
                    break;
                }
            }
        }

        //清空
        /// <summary>
        /// 清空文本
        /// </summary>
        private void ClearNameAndContent()
        {
            TbxFileName.Text = "";
            TbxFileContent.Text = "";
            TbxFileName.IsEnabled = false;
            TbxFileContent.IsEnabled = false;
        }

        //删除
        /// <summary>
        /// 删除
        /// </summary>
        private void Delete()
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
                    ShowMessage("请选择一个项目再删除");
                }
            }
            else
            {
                DeleteBook();
            }
        }
        /// <summary>
        /// 删除书籍
        /// </summary>
        private void DeleteBook()
        {
            if (CurrentBook != null)
            {
                MessageBoxResult result = MessageBox.Show("是否删除当前书籍？\n是：删除当前书籍并删除打开记录\n否：仅删除打开记录\n取消：不做任何操作", "删除项目", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    Directory.Delete(CurrentBook.Path, true);
                    for (int i = 0; i < Books.Items.Count; i++)
                    {
                        if (((ComboBoxItem)Books.Items[i]).Tag.ToString() == CurrentBook.Path)
                        {
                            Books.Items.RemoveAt(i);
                            break;
                        }
                    }

                    TbxFileName.Text = FindResource("欢迎使用") + " " + AppInfo.Name;
                    TbxFileName.ToolTip = null;
                    TbxFileContent.Text = FindResource("创建或打开以开始").ToString();
                    Words.ToolTip = null;
                    Words.Text = FindResource("字数") + "：0";
                    FilesTree.ToolTip = FindResource("创建或打开以开始");

                    ShowMessage("已删除书籍" + CurrentBook.Name);
                    CurrentBook = null;
                    CurrentChapter = null;
                    CurrentEssay = null;
                    RefreshBtnsState();
                    RefreshTitle();
                    FilesTree.Items.Clear();
                }
                else if (result == MessageBoxResult.No)
                {
                    RemoveBookHistory(CurrentBook);
                }
            }
        }
        /// <summary>
        /// 删除卷册
        /// </summary>
        private void DeleteChapter()
        {
            //获取对应文件路径
            string _path = SelectedNode.ToolTip.ToString();
            string name = SelectedNode.Header.ToString();
            MessageBoxResult result = MessageBox.Show("是否删除卷册 " + name + " ？", "删除项目", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Directory.Delete(_path, true);
                RemoveFolderNode(SelectedNode);
                //如果选择的是正在编辑的卷册
                if (CurrentChapter != null)
                {
                    if (_path == CurrentChapter.Path)
                    {
                        CurrentEssay = null;
                        CurrentChapter = null;
                        TbxFileName.Text = FindResource("当前未选中任何文章").ToString();
                        TbxFileName.ToolTip = null;
                        TbxFileContent.Text = null;
                        TbxFileName.IsEnabled = false;
                        TbxFileContent.IsEnabled = false;
                        RefreshBtnsState();
                        RefreshTitle();
                    }
                }
                ShowMessage("已删除卷册", " " + name);
            }
        }
        /// <summary>
        /// 删除文章
        /// </summary>
        private void DeleteEssay()
        {
            //获取对应文件路径
            string _path = SelectedNode.ToolTip.ToString();
            string name = SelectedNode.Header.ToString();
            MessageBoxResult result = MessageBox.Show("是否删除文章 " + name +" ？", "删除项目", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                RemoveFolderNode(SelectedNode);
                File.Delete(_path);
                //如果选择的是正在编辑的
                if (CurrentEssay != null)
                {
                    if (_path == CurrentEssay.Path)
                    {
                        TbxFileName.Text = FindResource("当前未选中任何文章").ToString();
                        TbxFileName.ToolTip = null;
                        TbxFileContent.Text = null;
                        TbxFileName.IsEnabled = false;
                        TbxFileContent.IsEnabled = true;
                        CurrentEssay = null;
                        if (CurrentChapter != null)
                        {
                            GetChapterInfo();
                            TbxFileName.IsEnabled = true;
                        }
                        else
                        {
                            TbxFileName.IsEnabled = false;
                        }
                        RefreshBtnsState();
                        RefreshTitle();
                    }
                }
                ShowMessage("已删除文章", " " + name);
            }
        }

        //获取
        /// <summary>
        /// 书籍信息
        /// </summary>
        private void GetBookInfo()
        {
            if (CurrentBook != null)
            {
                Words.Text = FindResource("字数") + "：0";
                TbxFileName.Text = CurrentBook.Name;
                TbxFileContent.Text = FindResource("创建时间") + "：" + Directory.GetCreationTime(CurrentBook.Path) + Environment.NewLine +
                                   FindResource("子卷册数") + "：" + GetDirCounts(CurrentBook.Path) + Environment.NewLine +
                                   FindResource("总文章数") + "：" + GetFileCounts(CurrentBook.Path) + Environment.NewLine +
                                   FindResource("总字数") + "：" + GetBookWords(CurrentBook.Path, "");
                CloseChapter();
            }
        }
        /// <summary>
        /// 卷册信息
        /// </summary>
        private void GetChapterInfo()
        {
            if (CurrentChapter != null)
            {
                Words.Text = FindResource("字数") + "：0";
                TbxFileName.Text = CurrentChapter.Name;
                TbxFileContent.Text = FindResource("创建时间") + "：" + Directory.GetCreationTime(CurrentChapter.Path) + Environment.NewLine +
                                   FindResource("子卷册数") + "：" + GetDirCounts(CurrentChapter.Path) + Environment.NewLine +
                                   FindResource("总文章数") + "：" + GetFileCounts(CurrentChapter.Path) + Environment.NewLine +
                                   FindResource("总字数") + "：" + GetBookWords(CurrentChapter.Path, "");
                CloseEssay();
            }
        }
        /// <summary>
        /// 统计书籍信息，获取所有txt文件数量
        /// </summary>
        /// <param name="_path">书籍路径</param>
        /// <returns>所有txt文件数量</returns>
        private static int GetFileCounts(string _path)
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
        private static int GetDirCounts(string _path)
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
        private int GetBookWords(string _path, string _thisEssay)
        {
            int n = 0;
            string[] _files = Directory.GetFiles(_path);
            foreach (var _file in _files)
            {
                if (_file == _thisEssay)
                {
                    //获取当前文章字符数
                    MatchCollection space = Regex.Matches(TbxFileContent.Text, @"\s");
                    int w = TbxFileContent.Text.Length - space.Count;
                    n += TbxFileContent.Text.Length - space.Count;
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
        private static Encoding GetFileEncodeType(string _file)
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
        /// <summary>
        /// 获取行和列
        /// </summary>
        private void GetRowAndColumn()
        {
            if (CurrentEssay != null)
            {
                //得到总行数。该行数会随着文本框的大小改变而改变；若只认回车符为一行(不考虑排版变化)请用 总行数=textBox1.Lines.Length;(记事本2是这种方式)
                //int totalline = FileContent.GetLineFromCharIndex(FileContent.Text.Length) + 1;
                //得到当前行的行号,从0开始，习惯是从1开始，所以+1.
                int line = TbxFileContent.GetLineIndexFromCharacterIndex(TbxFileContent.SelectionStart) + 1;
                //得到当前行第一个字符的索引
                int index = TbxFileContent.GetCharacterIndexFromLineIndex(line - 1);
                //.SelectionStart得到光标所在位置的索引 减去 当前行第一个字符的索引 = 光标所在的列数（从0开始)
                int column = TbxFileContent.SelectionStart - index + 1;
                string rac = FindResource("行").ToString() + "：" + line + "    " + FindResource("列").ToString() + "：" + column;
                RowAndColumn.Text = rac;
            }
            else
            {
                RowAndColumn.Text = FindResource("行").ToString() + "：" + 0 + "    " + FindResource("列").ToString() + "：" + 0; ;
            }
        }
        /// <summary>
        /// 简转繁
        /// </summary>
        /// <param name="simplifiedChinese">文字内容</param>
        /// <returns>繁体文字内容</returns>
        private static string GetTraditional(string simplifiedChinese)
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
        private static string GetSimplified(string traditionalChinese)
        {
            string simplifiedChinese = string.Empty;
            System.Globalization.CultureInfo vCultureInfo = new System.Globalization.CultureInfo("zh-CN", false);
            simplifiedChinese = Microsoft.VisualBasic.Strings.StrConv(traditionalChinese, Microsoft.VisualBasic.VbStrConv.SimplifiedChinese, vCultureInfo.LCID);
            return simplifiedChinese;
        }
        /// <summary>
        /// 遍历书籍目录下的子文件夹，创建节点
        /// </summary>
        /// <param name="_folder">文件夹路径</param>
        private TreeViewItemNode ScanFolder(string _folder)
        {
            //
            List<TreeViewItemNode> thisNodes = new List<TreeViewItemNode>();
            //遍历当前文件夹中的文件
            foreach (TreeViewItemNode fileNode in ScanFile(_folder))
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
                    TreeViewItemNode node = ScanFolder(_folder + @"\" + childFolder.ToString());
                    thisNodes.Add(node);
                }
            }
            TreeViewItemNode thisNode = new TreeViewItemNode(System.IO.Path.GetFileName(_folder), _folder, false, false, thisNodes);
            return thisNode;
        }
        /// <summary>
        /// 遍历此目录下的子文件，创建节点
        /// </summary>
        /// <param name="_folder">文件夹路径</param>
        private List<TreeViewItemNode> ScanFile(string _folder)
        {
            //
            List<TreeViewItemNode> thisNodes = new List<TreeViewItemNode>();
            //
            DirectoryInfo DI = new DirectoryInfo(_folder);
            //遍历文件夹里的文件
            FileInfo[] theEssays = DI.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            //开始遍历
            foreach (FileInfo nextEssay in theEssays)
            {
                //添加文件节点
                TreeViewItemNode thisNode = new TreeViewItemNode(nextEssay.Name, nextEssay.FullName, true, false, null);
                thisNodes.Add(thisNode);
            }
            return thisNodes;
        }
        /// <summary>
        /// 在一级节点里寻找节点
        /// </summary>
        /// <param name="path">节点路径</param>
        /// <returns></returns>
        private TreeViewItemNode FindNote(string path)
        {
            TreeViewItemNode found = null;
            //path = path.Replace("\\","/");
            foreach (TreeViewItemNode item in FilesTree.Items)
            {
                string itemPath = item.ToolTip.ToString();
                if (itemPath == path)
                {
                    found = item;
                    break;
                }

                List<TreeViewItemNode> nos = new List<TreeViewItemNode>();
                foreach (TreeViewItemNode it in item.Items)
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
        private TreeViewItemNode FindNote(List<TreeViewItemNode> nodes, string path)
        {
            TreeViewItemNode found = null;
            if (nodes != null)
            {
                foreach (TreeViewItemNode item in nodes)
                {
                    if (item.ToolTip.ToString() == path)
                    {
                        found = item;
                        break;
                    }

                    List<TreeViewItemNode> nos = new List<TreeViewItemNode>();
                    foreach (TreeViewItemNode it in item.Items)
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
        /// 设置字体显示
        /// </summary>
        private void SetFont(string fontName)
        {
            foreach (FontFamily font in Fonts.SystemFontFamilies)
            {
                if (fontName == font.Source)
                {
                    TbxFileContent.FontFamily = font;
                    //储存更改
                    User.Default.fontName = fontName;
                    SaveUserSettings();
                    //EssayName.FontFamily = font;
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
        /// 设置内部展开状态
        /// </summary>
        /// <param name="node">节点</param>
        private void SetExpandedState(TreeViewItemNode node)
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

        //选择
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
        /// <summary>
        /// 设置字体选项
        /// </summary>
        private void SelectFontItem(string fontName)
        {
            foreach (TextBlock item in CbbFonts.Items)
            {
                if (item.Text == fontName)
                {
                    CbbFonts.SelectedItem = item;
                    break;
                }
            }
        }
        /// <summary>
        /// 设置书籍选项
        /// </summary>
        private void SelectBookItem(FileOrFolderInfo book)
        {
            foreach (ComboBoxItem item in Books.Items)
            {
                if (item.Tag.ToString() == book.Path)
                {
                    Books.SelectedItem = item;
                    break;
                }
            }
        }
        /// <summary>
        /// 点击一个节点
        /// </summary>
        /// <param name="clickTimes">点击次数</param>
        private void SelectNode(int clickTimes)
        {
            //记录选择的节点
            SelectedNode = (TreeViewItemNode)FilesTree.SelectedItem;
            if (SelectedNode != null)
            {
                //如果选中的节点是文件，且双击
                if (SelectedNode.IsFile)
                {
                    if (clickTimes == 2)
                    {
                        //确认是否打开非txt文件
                        if (Path.GetExtension(SelectedNode.ToolTip.ToString()) != ".txt")
                        {
                            MessageBoxResult dr = MessageBox.Show("打开非txt文件可能导致其损坏，确认打开吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.None);
                            if (dr == MessageBoxResult.Yes)
                            { goto open; }
                            else if (dr == MessageBoxResult.No || dr == MessageBoxResult.None)
                            { goto finish; }
                        }
                    open:
                        //保存当前文件再打开新文件
                        if (CurrentEssay != null && User.Default.isAutoSaveWhenSwitch == true)
                        { SaveFile(); }
                        //打开
                        OpenEssay(SelectedNode.ToolTip.ToString());
                    //SaveFile();
                    finish:;
                    }
                    else
                    { ShowMessage("双击或按回车键打开该文章"); }
                }
                //如果选中的节点是文件夹
                else if (SelectedNode.IsFile == false)
                {
                    //如果选中的节点是文件夹，且双击
                    if (clickTimes == 2)
                    {
                        //保存当前文件再打开新文件
                        if (CurrentEssay != null && User.Default.isAutoSaveWhenSwitch == true)
                        { SaveFile(); }
                        //打开卷册
                        OpenChapter(SelectedNode.ToolTip.ToString());
                    }
                    else
                    { ShowMessage("双击或按回车键打开该卷册"); }
                }
            }
        }

        //检查
        /// <summary>
        /// 检查文件夹
        /// </summary>
        private void CheckFolders()
        {
            //创建存档文件夹
            if (!Directory.Exists(User.Default.BooksDir))
            { Directory.CreateDirectory(User.Default.BooksDir); }
            //创建备份文件夹
            if (!Directory.Exists(User.Default.BackupDir))
            { Directory.CreateDirectory(User.Default.BackupDir); }
        }
        /// <summary>
        /// 准备导出全书
        /// </summary>
        private void CheckExport()
        {
            //保存文章
            if (CurrentEssay != null)
            {
                SaveFile();
            }
            //导出路径
            string output;
            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                SelectedPath = Path.GetDirectoryName(Settings.Default._lastBook),
                Description = FindResource("选择导出目录").ToString()
            };
            //按下确定选择的按钮，获取文件夹路径
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                output = fbd.SelectedPath + @"\" + CurrentBook.Name + ".txt";
                if (File.Exists(output))
                {
                    MessageBoxResult dr = MessageBox.Show(FindResource("导出路径已存在").ToString(), FindResource("提示").ToString(), MessageBoxButton.OKCancel);
                    if (dr == MessageBoxResult.OK)
                    {
                        Export(output);
                    }
                    else
                    {
                        ShowMessage("已取消导出");
                    }
                }
                else
                {
                    Export(output);
                }
            }
        }
        /// <summary>
        /// 检测名字是否合法字符
        /// </summary>
        /// <param name="name">名字</param>
        /// <returns>是否合法字符</returns>
        private static bool CheckIsRightName(string name)
        {
            string str1 = "/";
            string str2 = "|";
            string str3 = "\\";
            string str4 = "<";
            string str5 = ">";
            string str6 = ":";
            string str7 = "*";
            string str8 = "?";
            string str9 = "\"";

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

        //刷新
        /// <summary>
        /// 刷新按钮状态
        /// </summary>
        public void RefreshBtnsState()
        {
            BtnOpenBook.IsEnabled = true;
            BtnCreateBook.IsEnabled = true;
            BtnFold.IsEnabled = true;
            BtnToSimplified.IsEnabled = false;
            BtnToTraditional.IsEnabled = false;

            if (CurrentBook != null)
            {
                BtnCloseBook.IsEnabled = true;
                BtnCreateChapter.IsEnabled = true;
                BtnCreateEssay.IsEnabled = true;
                BtnExport.IsEnabled = true;
                BtnBookInfo.IsEnabled = true;

                BtnExpand.IsEnabled = true;
                BtnCollapse.IsEnabled = true;
                BtnRefresh.IsEnabled = true;
                BtnDelete.IsEnabled = true;

                if (CurrentChapter != null)
                {
                    BtnToSimplified.IsEnabled = true;
                    BtnToTraditional.IsEnabled = true;
                }

                if (CurrentEssay != null)
                {
                    BtnSave.IsEnabled = true;
                    BtnSaveAs.IsEnabled = true;

                    BtnUndo.IsEnabled = true;
                    BtnRedo.IsEnabled = true;
                    BtnToSimplified.IsEnabled = true;
                    BtnToTraditional.IsEnabled = true;

                    BtnFindNext.IsEnabled = true;
                    BtnReplaceNext.IsEnabled = true;
                    BtnReplaceAll.IsEnabled = true;
                    TextFind.IsEnabled = true;
                    TextReplace.IsEnabled = true;
                }
                else
                {
                    BtnSave.IsEnabled = false;
                    BtnSaveAs.IsEnabled = false;

                    BtnUndo.IsEnabled = false;
                    BtnRedo.IsEnabled = false;
                    BtnFindNext.IsEnabled = false;
                    BtnReplaceNext.IsEnabled = false;
                    BtnReplaceAll.IsEnabled = false;
                    TextFind.IsEnabled = false;
                    TextReplace.IsEnabled = false;
                }
            }
            else
            {
                BtnCloseBook.IsEnabled = false;
                BtnCreateChapter.IsEnabled = false;
                BtnCreateEssay.IsEnabled = false;
                BtnExport.IsEnabled = false;
                BtnBookInfo.IsEnabled = false;
                BtnSave.IsEnabled = false;
                BtnSaveAs.IsEnabled = false;

                BtnExpand.IsEnabled = false;
                BtnCollapse.IsEnabled = false;
                BtnRefresh.IsEnabled = false;
                BtnDelete.IsEnabled = false;

                BtnUndo.IsEnabled = false;
                BtnRedo.IsEnabled = false;
                BtnToSimplified.IsEnabled = false;
                BtnToTraditional.IsEnabled = false;
                BtnFindNext.IsEnabled = false;
                BtnReplaceNext.IsEnabled = false;
                BtnReplaceAll.IsEnabled = false;
                TextFind.IsEnabled = false;
                TextReplace.IsEnabled = false;
            }

        }
        /// <summary>
        /// 刷新主窗口标题
        /// </summary>
        public void RefreshTitle()
        {
            if (CurrentBook == null)
            {
                Main.Title = AppInfo.Name + " " + AppInfo.Version;
            }
            else
            {
                if (CurrentChapter == null)
                {
                    if (CurrentEssay == null)
                    {
                        Main.Title = AppInfo.Name + " - " + CurrentBook.Name;
                    }
                    else
                    {
                        Main.Title = AppInfo.Name + " - " + CurrentBook.Name + @"\" + CurrentEssay.Name;
                    }
                }
                else
                {
                    if (CurrentEssay == null)
                    {
                        Main.Title = AppInfo.Name + " - " + CurrentBook.Name + @"\" + CurrentChapter.Path.Replace(CurrentBook.Path + @"\", "");
                    }
                    else
                    {
                        Main.Title = AppInfo.Name + " - " + CurrentBook.Name + @"\" + CurrentEssay.Path.Replace(CurrentBook.Path + @"\", "");
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
        /// 显示运行信息
        /// </summary>
        private void ShowRunInfo()
        {
            //若第一次运行软件，提示消息
            if (Settings.Default.runTimes == 1)
            {
                TbxFileName.Text = FindResource("欢迎使用") + " " + AppInfo.Name;
                //显示运行记录
                TbxFileContent.Text = FindResource("启动次数") + "：" + Settings.Default.runTimes + Environment.NewLine +
                                   FindResource("启动时间") + "：" + Settings.Default.thisStartTime;
            }
            else
            {
                TbxFileName.Text = FindResource("欢迎使用") + " " + AppInfo.Name;
                int d = Settings.Default.totalTime.Days;
                int h = Settings.Default.totalTime.Hours;
                int m = Settings.Default.totalTime.Minutes;
                int s = Settings.Default.totalTime.Seconds;
                string t = d + "天" + h + "时" + m + "分" + s + "秒";
                //显示运行记录
                TbxFileContent.Text = FindResource("启动次数") + "：" + Settings.Default.runTimes + Environment.NewLine +
                                   FindResource("启动时间") + "：" + Settings.Default.thisStartTime + Environment.NewLine +
                                   Environment.NewLine +
                                   FindResource("上次启动时间") + "：" + Settings.Default.lastStartTime + Environment.NewLine +
                                   FindResource("上次关闭时间") + "：" + Settings.Default.lastEndTime + Environment.NewLine +
                                   FindResource("总运行时长") + "：" + t;
            }
            TbxFileName.IsEnabled = false;
            TbxFileContent.IsEnabled = false;
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
        /// <summary>
        /// 显示更多消息
        /// </summary>
        /// <param name="resourceName">资源名</param>
        /// <param name="moreText">附加信息</param>
        /// <param name="newBox">是否弹出对话框</param>
        private void ShowMessage(string resourceName, string moreText, bool newBox = false)
        {
            if (newBox)
            {
                MessageBox.Show(FindResource(resourceName) + moreText);
            }
            else
            {
                HelpMessage.Text = FindResource(resourceName) + moreText;
            }
        }
        /// <summary>
        /// 展开目录
        /// </summary>
        private void ExpandTree()
        {
            foreach (TreeViewItemNode item in FilesTree.Items)
            {
                //DependencyObject dObject = FilesTree.ItemContainerGenerator.ContainerFromItem(item);
                //((FileNode)dObject).ExpandSubtree();
                item.ExpandSubtree();
            }
        }

        //隐藏
        /// <summary>
        /// 隐藏目录区
        /// </summary>
        private void HidePanCenter()
        {
            if (PanCenter.Visibility == Visibility.Visible)
            {
                PanCenter.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// 收起目录
        /// </summary>
        private void CollapseTree()
        {
            foreach (TreeViewItemNode item in FilesTree.Items)
            {
                //DependencyObject dObject = FilesTree.ItemContainerGenerator.ContainerFromItem(item);
                //FileNode tvi = (FileNode)dObject;
                //tvi.IsExpanded = false;
                item.IsExpanded = false;
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
            foreach (TreeViewItemNode fileNode in ScanFile(_book))
            {
                FilesTree.Items.Add(fileNode);
            }
            //获取子文件夹信息
            DirectoryInfo DI = new DirectoryInfo(_book);
            DirectoryInfo[] childFolders = DI.GetDirectories();
            int j = childFolders.Length;
            //遍历当前文件夹中的子文件夹
            if (j != 0)
            {
                foreach (DirectoryInfo childFolder in childFolders)
                {
                    //遍历当前子文件夹中的子文件夹
                    TreeViewItemNode node = ScanFolder(_book + @"\" + childFolder.ToString());
                    FilesTree.Items.Add(node);
                }
            }
        }
        /// <summary>
        /// 导出全书
        /// </summary>
        /// <param name="_output">导出的路径</param>
        private void Export(string _output)
        {
            //显示消息
            ShowMessage("正在导出txt");
            RefreshWindow();

            File.CreateText(_output).Close();
            //书名
            //File.AppendAllText(output, selectedBook + Environment.NewLine);
            foreach (TreeViewItemNode txt in FilesTree.Items)
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
                    File.AppendAllText(_output, System.IO.Path.GetFileNameWithoutExtension(txt.ToolTip.ToString()));
                    //换行
                    File.AppendAllText(_output, Environment.NewLine);
                    //获取所有字符
                    string[] txtText = File.ReadAllLines(txt.ToolTip.ToString(), Encoding.UTF8);
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
            ShowMessage("导出成功", " " + l);
            Process.Start(Path.GetDirectoryName(_output));
        }
        /// <summary>
        /// 自动缩进
        /// </summary>
        private void Indentation()
        {
            int start = TbxFileContent.SelectionStart;
            string spaces = "";
            for (int i = 0; i < User.Default.autoIndentations; i++)
            {
                spaces += " ";
            }
            TbxFileContent.Text = TbxFileContent.Text.Insert(start, spaces);
            TbxFileContent.Select(start + spaces.Length, 0);
        }
        /// <summary>
        /// 自动备份
        /// </summary>
        private void Backup()
        {
            //await Task.Run(() =>
            //{
            if (User.Default.isAutoBackup == true)
            {
                if (CurrentBook != null && Directory.Exists(CurrentBook.Path))
                {
                    ShowMessage("书籍备份中");
                    RefreshWindow();
                    string _path = User.Default.BackupDir + @"\" + CurrentBook.Name;
                    //删除上个备份
                    if (Directory.Exists(_path))
                    { Directory.Delete(_path, true); }
                    //创建新的备份
                    Directory.CreateDirectory(_path);
                    CopyDirectory(CurrentBook.Path, _path);
                    //显示消息
                    //Dispatcher.BeginInvoke(new Action(delegate { HelpMessage.Content = "书籍已自动备份于 " + DateTime.Now.ToLongTimeString().ToString(); }));
                    ShowMessage("已自动备份于", DateTime.Now.ToLongTimeString().ToString());
                }
            }
            //});
        }
        #endregion 

        #region 事件
        //窗口事件
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            CheckFolders();
            CreateTimer();

            //载入并显示应用信息
            LoadAppInfo();
            ShowAppInfo();

            //载入下拉菜单项
            LoadLanguageItems();
            LoadThemeItems();
            LoadFontItems();
            LoadBookItems();

            //载入设置
            LoadSettings();
            AddRunTime();

            //初始化
            SetLanguage(User.Default.language);
            SetTheme(User.Default.ThemePath);
            SetFont(User.Default.fontName);
            if (User.Default.isAutoOpenBook)
            {
                OpenLastBook();
            }
            else
            {
                FilesTree.ToolTip = FindResource("创建或打开以开始");
                HelpMessage.Text = FindResource("创建或打开以开始").ToString();
            }

            if (User.Default.isShowRunInfo)
            {
                ShowRunInfo();
            }
            else
            {
                ClearNameAndContent();
                if (User.Default.isAutoOpenBook)
                {
                    OpenLastBook();
                }
            }

            //刷新
            RefreshBtnsState();
            RefreshTitle();

            //提示消息
            ShowMessage("已载入");
        }
        private void Main_Closing(object sender, CancelEventArgs e)
        {
            if (CurrentBook != null)
            {
                //确认是否保存文件再关闭
                if (CurrentEssay != null && IsSaved == false)
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

            if (Settings.Default.runTimes > 0)
            {
                SaveBookHistory();
            }
        }
        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            //若打开了书
            if (CurrentBook != null)
            {
                //Ctrl+E 导出全文
                if (e.Key == Key.E && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
                { CheckExport(); }
                //Ctrl+M 书籍信息
                if (e.Key == Key.M && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                                   && !(e.KeyboardDevice.IsKeyDown(Key.LeftAlt) || e.KeyboardDevice.IsKeyDown(Key.RightAlt)))
                {
                    CloseChapter();
                    CloseEssay();
                    GetBookInfo();
                }
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
                if (CurrentChapter != null)
                {
                    //Ctrl+Alt+Q 关闭卷册
                    if (e.Key == Key.Q && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                                       && (e.KeyboardDevice.IsKeyDown(Key.LeftAlt) || e.KeyboardDevice.IsKeyDown(Key.RightAlt)))
                    {
                        CloseChapter();
                    }
                }

                //若打开了文章
                if (CurrentEssay != null)
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
                    {
                        BtnEdit_Click(null,null);
                    }
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
                        GetChapterInfo();
                    }
                    //Ctrl+Shift+J
                    if (e.Key == Key.J && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                                       && (e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift)))
                    {
                        if (TbxFileContent.IsFocused && TbxFileContent.SelectedText != null && TbxFileContent.SelectedText != "")
                        {
                            TbxFileContent.SelectedText = GetSimplified(TbxFileContent.SelectedText);
                        }
                        else if (TbxFileName.IsFocused && TbxFileName.SelectedText != null && TbxFileName.SelectedText != "")
                        {
                            TbxFileName.SelectedText = GetSimplified(TbxFileName.SelectedText);
                        }
                    }
                    //Ctrl+Shift+K 
                    if (e.Key == Key.K && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                                       && (e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift)))
                    {
                        if (TbxFileContent.IsFocused && TbxFileContent.SelectedText != null && TbxFileContent.SelectedText != "")
                        {
                            TbxFileContent.SelectedText = GetTraditional(TbxFileContent.SelectedText);
                        }
                        else if (TbxFileName.IsFocused && TbxFileName.SelectedText != null && TbxFileName.SelectedText != "")
                        {
                            TbxFileName.SelectedText = GetTraditional(TbxFileName.SelectedText);
                        }
                    }
                }
            }
            //Ctrl+O 打开书籍
            if (e.Key == Key.O && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            { OpenNewBook(); }
            //Ctrl+B 创建
            if (e.Key == Key.B && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            { Create(); }
            //Ctrl+T 切换下个主题
            if (e.Key == Key.T && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            { SetNextTheme(); }

            //关于菜单
            if (e.Key == Key.F1)
            { Process.Start("explorer.exe", AppInfo.HomePage); }
            else if (e.Key == Key.F2)
            { Process.Start("explorer.exe", AppInfo.InfoPage); }
            else if (e.Key == Key.F3)
            { Process.Start("explorer.exe", AppInfo.DownloadPage); }
            else if (e.Key == Key.F4)
            { Process.Start("explorer.exe", AppInfo.FeedbackPage); }
            else if (e.Key == Key.F8)
            { Process.Start("explorer.exe", AppInfo.GitHubPage); }
            else if (e.Key == Key.F6)
            { Process.Start("explorer.exe", AppInfo.QQGroupLink); }
        }

        private void TbxCreatePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TbxCreatePath.Text == null || TbxCreatePath.Text == "")
            {
                BtnCreateBook.ToolTip = "请设置存放位置";
                BtnCreateChapter.ToolTip = "请设置存放位置";
                BtnCreateEssay.ToolTip = "请设置存放位置";
            }
            else
            {
                BtnCreateBook.ToolTip = TbxCreatePath.Text + @"\" + TbxCreateName.Text;
                BtnCreateChapter.ToolTip = TbxCreatePath.Text + @"\" + TbxCreateName.Text;
                BtnCreateEssay.ToolTip = TbxCreatePath.Text + @"\" + TbxCreateName.Text + ".txt";
            }
        }
        private void TbxCreateName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TbxCreateName.Text == null || TbxCreateName.Text == "")
            {
                BtnCreateBook.ToolTip = "请输入书籍名称";
                BtnCreateChapter.ToolTip = "请输入卷册名称";
                BtnCreateEssay.ToolTip = "请输入文章名称";
            }
            else
            {
                BtnCreateBook.ToolTip = TbxCreatePath.Text + @"\" + TbxCreateName.Text;
                BtnCreateChapter.ToolTip = TbxCreatePath.Text + @"\" + TbxCreateName.Text;
                BtnCreateEssay.ToolTip = TbxCreatePath.Text + @"\" + TbxCreateName.Text + ".txt";
            }
        }

        private void TbxFileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentEssay != null || CurrentChapter != null)
            {

            }
        }
        private void TbxFileName_GotFocus(object sender, RoutedEventArgs e)
        {
            ShowMessage("在此处重命名");
        }
        private void TbxFileName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (TbxFileName.Text != null && TbxFileName.Text != "")
                {
                    //检测格式是否正确
                    if (CheckIsRightName(TbxFileName.Text))
                    {
                        if (CurrentEssay != null)
                        {
                            //删除原文件
                            File.Delete(CurrentEssay.Path);
                            //获取新名字
                            string name = TbxFileName.Text + ".txt";
                            string path;
                            if (CurrentChapter == null)
                            {
                                path = CurrentBook.Path + @"\" + name;
                            }
                            else
                            {
                                path = CurrentChapter.Path + @"\" + name;
                            }
                            CurrentEssay = new FileOrFolderInfo(path);
                            //创建新文件
                            File.Create(CurrentEssay.Path).Close();
                            //创建写入文件
                            SaveFile();

                            //重新导入目录
                            ReloadFilesTree();

                            //显示消息
                            ShowMessage("文章重命名成功");
                        }
                        else if (CurrentChapter != null)
                        {
                            //获取旧卷册路径
                            string _old = CurrentChapter.Path;
                            //更新卷册名称和路径
                            string name = TbxFileName.Text;
                            CurrentChapter = new FileOrFolderInfo(Path.GetDirectoryName(CurrentChapter.Path) + @"\" + name);
                            //MessageBox.Show(System.IO.Path.GetDirectoryName(_CurrentChapter.Name));
                            Directory.CreateDirectory(CurrentChapter.Path);
                            if (_old != CurrentChapter.Path)
                            {
                                //拷贝文件
                                CopyDirectory(_old, CurrentChapter.Path);
                                //删除旧目录
                                Directory.Delete(_old, true);

                                //重新导入目录
                                ReloadFilesTree();

                                //显示消息
                                ShowMessage("卷册重命名成功");
                            }
                        }
                    }
                    else
                        ShowMessage("重命名中不能含有以下字符", " \\ | / < > \" ? * :");
                }
                else
                    ShowMessage("重命名不能为空");
            }
        }

        private void TbxFileContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentEssay != null)
            {
                //过滤无效字符
                MatchCollection space = Regex.Matches(TbxFileContent.Text, @"\s");
                //MatchCollection newLine = Regex.Matches(FileContent.Text, @"\r");
                int w = TbxFileContent.Text.Length - space.Count;
                //刷新字数
                Words.Text = FindResource("字数").ToString() + "：" + w;
                Words.ToolTip = FindResource("全书字数").ToString() + "：" + GetBookWords(CurrentBook.Path, CurrentEssay.Path);
                //显示消息
                ShowMessage("正在编辑");
                IsSaved = false;
                if (w > 100000)
                {
                    ShowMessage("控制字数", true);
                }
            }
        }
        private void TbxFileContent_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //NodePath.Text = e.Text;
            //自动补全
            if (User.Default.isAutoCompletion)
            {
                //记录光标位置
                int position = TbxFileContent.SelectionStart;
                //记录滚动条位置
                double s = TbxFileContent.VerticalOffset;
                if (e.Text.Equals("("))
                { TbxFileContent.Text = TbxFileContent.Text.Insert(TbxFileContent.SelectionStart, ")"); TbxFileContent.Select(position, 0); }
                else if (e.Text.Equals("（"))
                { TbxFileContent.Text = TbxFileContent.Text.Insert(TbxFileContent.SelectionStart, "）"); TbxFileContent.Select(position, 0); }
                else if (e.Text.Equals("["))
                { TbxFileContent.Text = TbxFileContent.Text.Insert(TbxFileContent.SelectionStart, "]"); TbxFileContent.Select(position, 0); }
                else if (e.Text.Equals("【"))
                { TbxFileContent.Text = TbxFileContent.Text.Insert(TbxFileContent.SelectionStart, "】"); TbxFileContent.Select(position, 0); }
                else if (e.Text.Equals("{"))
                { TbxFileContent.Text = TbxFileContent.Text.Insert(TbxFileContent.SelectionStart, "}"); TbxFileContent.Select(position, 0); }
                else if (e.Text.Equals("'"))
                { TbxFileContent.Text = TbxFileContent.Text.Insert(TbxFileContent.SelectionStart, "'"); TbxFileContent.Select(position, 0); }
                else if (e.Text.Equals("‘"))
                { TbxFileContent.Text = TbxFileContent.Text.Insert(TbxFileContent.SelectionStart, "’"); TbxFileContent.Select(position, 0); }
                else if (e.Text.Equals("’"))
                { TbxFileContent.Text = TbxFileContent.Text.Insert(TbxFileContent.SelectionStart - 1, "‘"); TbxFileContent.Select(position, 0); }
                else if (e.Text.Equals("\""))
                { TbxFileContent.Text = TbxFileContent.Text.Insert(TbxFileContent.SelectionStart, "\""); TbxFileContent.Select(position, 0); }
                else if (e.Text.Equals("“"))
                { TbxFileContent.Text = TbxFileContent.Text.Insert(TbxFileContent.SelectionStart, "”"); TbxFileContent.Select(position, 0); }
                else if (e.Text.Equals("”"))
                { TbxFileContent.Text = TbxFileContent.Text.Insert(TbxFileContent.SelectionStart - 1, "“"); TbxFileContent.Select(position, 0); }
                else if (e.Text.Equals("<"))
                { TbxFileContent.Text = TbxFileContent.Text.Insert(TbxFileContent.SelectionStart, ">"); TbxFileContent.Select(position, 0); }
                else if (e.Text.Equals("《"))
                { TbxFileContent.Text = TbxFileContent.Text.Insert(TbxFileContent.SelectionStart, "》"); TbxFileContent.Select(position, 0); }
                //FileContent.Select(position, 0);
                TbxFileContent.ScrollToVerticalOffset(s);
            }
        }
        private void TbxFileContent_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (User.Default.isAutoIndentation)
                {
                    Indentation();
                }
            }
        }
        private void TbxFileContent_SelectionChanged(object sender, RoutedEventArgs e)
        {
            GetRowAndColumn();
            RefreshBtnsState();
        }
        private void TbxFileContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GetRowAndColumn();
        }
        private void TbxFileContent_LostFocus(object sender, RoutedEventArgs e)
        {
            RowAndColumn.Text = FindResource("行").ToString() + "：" + 0 + "    " + FindResource("列").ToString() + "：" + 0; ;
        }

        private void FilesTree_KeyUp(object sender, KeyEventArgs e)
        {
            //Enter选择节点
            if (e.Key == Key.Enter)
            {
                SelectNode(2);
            }
            //Delete 删除节点
            if (e.Key == Key.Delete)
            {
                Delete();
                e.Handled = true;
            }
        }
        private void FilesTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectNode(1);
        }
        private void FilesTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectNode(2);
        }

        private void Books_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Books.SelectedItem != null)
            {
                if (CurrentBook != null)
                { CloseBook(); }

                ComboBoxItem item = Books.SelectedItem as ComboBoxItem;
                string path = item.Tag.ToString();
                if (Directory.Exists(path))
                {
                    CurrentBook = new FileOrFolderInfo(path);
                    Settings.Default._lastBook = CurrentBook.Path;
                    SaveAppSettings();
                    OpenBook(CurrentBook);
                }
                else
                {
                    Books.Items.Remove(Books.SelectedItem);
                    ShowMessage("此书籍不存在", true);
                }
            }
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
        private void BtnFile_Click(object sender, RoutedEventArgs e)
        {
            CenterBookPage.Visibility = Visibility.Visible;
            CenterEditPage.Visibility = Visibility.Collapsed;
            CenterSettingPage.Visibility = Visibility.Collapsed;
            CenterAboutPage.Visibility = Visibility.Collapsed;

            BtnsFile.Visibility = Visibility.Visible;
            BtnsEdit.Visibility = Visibility.Collapsed;
            BtnsSetting.Visibility = Visibility.Collapsed;
            BtnsAbout.Visibility = Visibility.Collapsed;

            BtnFile.BorderThickness = new Thickness(4, 0, 0, 0);
            BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
        }
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            CenterBookPage.Visibility = Visibility.Collapsed;
            CenterEditPage.Visibility = Visibility.Visible;
            CenterSettingPage.Visibility = Visibility.Collapsed;
            CenterAboutPage.Visibility = Visibility.Collapsed;

            BtnsFile.Visibility = Visibility.Collapsed;
            BtnsEdit.Visibility = Visibility.Visible;
            BtnsSetting.Visibility = Visibility.Collapsed;
            BtnsAbout.Visibility = Visibility.Collapsed;

            BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnEdit.BorderThickness = new Thickness(4, 0, 0, 0);
            BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
        }
        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            CenterBookPage.Visibility = Visibility.Collapsed;
            CenterEditPage.Visibility = Visibility.Collapsed;
            CenterSettingPage.Visibility = Visibility.Visible;
            CenterAboutPage.Visibility = Visibility.Collapsed;

            BtnsFile.Visibility = Visibility.Collapsed;
            BtnsEdit.Visibility = Visibility.Collapsed;
            BtnsSetting.Visibility = Visibility.Visible;
            BtnsAbout.Visibility = Visibility.Collapsed;

            BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnSetting.BorderThickness = new Thickness(4, 0, 0, 0);
            BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
        }
        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            CenterBookPage.Visibility = Visibility.Collapsed;
            CenterEditPage.Visibility = Visibility.Collapsed;
            CenterSettingPage.Visibility = Visibility.Collapsed;
            CenterAboutPage.Visibility = Visibility.Visible;

            BtnsFile.Visibility = Visibility.Collapsed;
            BtnsEdit.Visibility = Visibility.Collapsed;
            BtnsSetting.Visibility = Visibility.Collapsed;
            BtnsAbout.Visibility = Visibility.Visible;

            BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnAbout.BorderThickness = new Thickness(4, 0, 0, 0);
        }

        private void BtnOpenBook_Click(object sender, RoutedEventArgs e)
        {
            OpenNewBook();
        }
        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            Create();
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
        private void BtnBookInfo_Click(object sender, RoutedEventArgs e)
        {
            CloseChapter();
            CloseEssay();
            GetBookInfo();
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }
        private void BtnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileAs();
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

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete();
        }
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.GetFullPath(TbxCreatePath.Text);
            if (!Directory.Exists(path))
            {
                path = Path.GetFullPath(User.Default.BooksDir);
            }
            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                SelectedPath = path,
                Description = "请选择目标存放位置："
            };
            //按下确定选择的按钮，获取存放路径
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TbxCreatePath.Text = fbd.SelectedPath;
            }
        }
        private void BtnCreateBook_Click(object sender, RoutedEventArgs e)
        {
            CreateBook();
        }
        private void BtnCreateChapter_Click(object sender, RoutedEventArgs e)
        {
            CreateChapter();
        }
        private void BtnCreateEssay_Click(object sender, RoutedEventArgs e)
        {
            CreateEssay();
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            TbxFileContent.Undo();
        }
        private void BtnRedo_Click(object sender, RoutedEventArgs e)
        {
            TbxFileContent.Redo();
        }
        private void BtnToTraditional_Click(object sender, RoutedEventArgs e)
        {
            if (TbxFileContent.IsFocused && TbxFileContent.SelectedText != null && TbxFileContent.SelectedText != "")
            {
                TbxFileContent.SelectedText = GetTraditional(TbxFileContent.SelectedText);
            }
            else if (TbxFileName.IsFocused && TbxFileName.SelectedText != null && TbxFileName.SelectedText != "")
            {
                TbxFileName.SelectedText = GetTraditional(TbxFileName.SelectedText);
            }
        }
        private void BtnToSimplified_Click(object sender, RoutedEventArgs e)
        {
            if (TbxFileContent.IsFocused && TbxFileContent.SelectedText != null && TbxFileContent.SelectedText != "")
            {
                TbxFileContent.SelectedText = GetSimplified(TbxFileContent.SelectedText);
            }
            else if (TbxFileName.IsFocused && TbxFileName.SelectedText != null && TbxFileName.SelectedText != "")
            {
                TbxFileName.SelectedText = GetSimplified(TbxFileName.SelectedText);
            }
        }
        private void BtnFindNext_Click(object sender, RoutedEventArgs e)
        {
            //检测是否输入
            if (TextFind.Text != null && TextFind.Text != "")
            {
                //检测是否存在
                if (TbxFileContent.Text.Contains(TextFind.Text))
                {
                    //获取总个数
                    FindAmount.Text = "共计" + Regex.Matches(TbxFileContent.Text, TextFind.Text).Count.ToString() + "处";
                    int j = TextFind.Text.Length;
                    //检测是否同个内容第一次按下
                    if (CurrentFindText != TextFind.Text)
                    {
                        CurrentFindText = TextFind.Text;
                        //获取这个的索引
                        NextStartIndex = TbxFileContent.Text.IndexOf(CurrentFindText, 0);
                        //高亮
                        TbxFileContent.Focus();
                        TbxFileContent.Select(NextStartIndex, j);
                        //记录下一个开始寻找的地方
                        NextStartIndex += j;
                    }
                    else
                    {
                        //获取这个的索引
                        NextStartIndex = TbxFileContent.Text.IndexOf(CurrentFindText, NextStartIndex);
                        //检测是否存在
                        if (NextStartIndex >= 0)
                        {
                            //高亮
                            TbxFileContent.Focus();
                            TbxFileContent.Select(NextStartIndex, j);
                            //记录下一个开始寻找的地方
                            NextStartIndex += j;
                        }
                        else
                        {
                            MessageBox.Show("所有位置已查找完毕，再按一次将从头查找");
                            NextStartIndex = 0;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("没有找到该内容");
                    FindAmount.Text = "共计0处";
                    NextStartIndex = 0;
                }
            }
            else
            {
                MessageBox.Show("请输入要查找的内容");
                FindAmount.Text = "共计0处";
                NextStartIndex = 0;
            }
        }
        private void BtnReplaceNext_Click(object sender, RoutedEventArgs e)
        {
            //检测是否输入查找项
            if (TextFind.Text != null && TextFind.Text != "")
            {
                //检测是否存在
                if (TbxFileContent.Text.Contains(TextFind.Text))
                {
                    int j = TextFind.Text.Length;
                    int k = TextReplace.Text.Length;
                    ReplaceText = TextReplace.Text;
                    //检测是否同个内容第一次按下
                    if (CurrentFindText != TextFind.Text)
                    {
                        CurrentFindText = TextFind.Text;
                        //获取这个的索引
                        NextStartIndex = TbxFileContent.Text.IndexOf(CurrentFindText, 0);
                        //移除、插入、高亮
                        TbxFileContent.Focus();
                        TbxFileContent.Text = TbxFileContent.Text.Remove(NextStartIndex, j);
                        TbxFileContent.Text = TbxFileContent.Text.Insert(NextStartIndex, ReplaceText);
                        TbxFileContent.Select(NextStartIndex, k);
                        //记录下一个开始寻找的地方
                        NextStartIndex += k;
                    }
                    else
                    {
                        //获取这个的索引
                        NextStartIndex = TbxFileContent.Text.IndexOf(CurrentFindText, NextStartIndex);
                        //检测是否存在
                        if (NextStartIndex >= 0)
                        {
                            //移除、插入、高亮
                            TbxFileContent.Focus();
                            TbxFileContent.Text = TbxFileContent.Text.Remove(NextStartIndex, j);
                            TbxFileContent.Text = TbxFileContent.Text.Insert(NextStartIndex, ReplaceText);
                            TbxFileContent.Select(NextStartIndex, k);
                            //记录下一个开始寻找的地方
                            NextStartIndex += k;
                        }
                        else
                        {
                            MessageBox.Show("所有位置已替换完毕");
                            NextStartIndex = 0;
                        }
                    }
                    //获取总个数
                    FindAmount.Text = "还有" + Regex.Matches(TbxFileContent.Text, TextFind.Text).Count.ToString() + "处";
                }
                else
                {
                    MessageBox.Show("没有找到该内容");
                    FindAmount.Text = "共计0处";
                    NextStartIndex = 0;
                }
            }
            else
            {
                MessageBox.Show("请输入要查找的内容");
                FindAmount.Text = "共计0处";
                NextStartIndex = 0;
            }
        }
        private void BtnReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            //检测是否输入查找项
            if (TextFind.Text != null && TextFind.Text != "")
            {
                //检测是否存在
                if (TbxFileContent.Text.Contains(TextFind.Text))
                {
                    int i = Regex.Matches(TbxFileContent.Text, TextFind.Text).Count;
                    int j = TextFind.Text.Length;
                    int k = TextReplace.Text.Length;
                    ReplaceText = TextReplace.Text;
                    CurrentFindText = TextFind.Text;
                    //移除、插入、高亮
                    TbxFileContent.Focus();
                    TbxFileContent.Text = TbxFileContent.Text.Replace(CurrentFindText, ReplaceText);
                    MessageBox.Show("全部内容已替换完毕，共计" + i + "处");
                    FindAmount.Text = "共计0处";
                }
                else
                {
                    MessageBox.Show("没有找到该内容");
                    FindAmount.Text = "共计0处";
                }
            }
            else
            {
                MessageBox.Show("请输入要查找的内容");
                FindAmount.Text = "共计0处";
            }
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
        private void CbbFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbbFonts.SelectedItem != null)
            {
                TextBlock item = CbbFonts.SelectedItem as TextBlock;
                SetFont(item.Text);
                ShowMessage("已更改");
            }
        }

        private void ShowRunInfo_Checked(object sender, RoutedEventArgs e)
        {
            User.Default.isShowRunInfo = true;
            SaveUserSettings();
            //显示消息
            ShowMessage("已更改");
        }
        private void ShowRunInfo_Unchecked(object sender, RoutedEventArgs e)
        {
            User.Default.isShowRunInfo = false;
            SaveUserSettings();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoOpenBook_Checked(object sender, RoutedEventArgs e)
        {
            User.Default.isAutoOpenBook = true;
            SaveUserSettings();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoOpenBook_Unchecked(object sender, RoutedEventArgs e)
        {
            User.Default.isAutoOpenBook = false;
            SaveUserSettings();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoCompletion_Checked(object sender, RoutedEventArgs e)
        {
            User.Default.isAutoCompletion = true;
            SaveUserSettings();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoCompletion_Unchecked(object sender, RoutedEventArgs e)
        {
            User.Default.isAutoCompletion = false;
            SaveUserSettings();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoIndentation_Checked(object sender, RoutedEventArgs e)
        {
            User.Default.isAutoIndentation = true;
            SaveUserSettings();
            AutoIndentations.IsEnabled = true;
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoIndentation_Unchecked(object sender, RoutedEventArgs e)
        {
            User.Default.isAutoIndentation = false;
            SaveUserSettings();
            AutoIndentations.IsEnabled = false;
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoIndentations_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AutoIndentations.Text != "" && AutoIndentations.Text != null)
            {
                try
                {
                    int t = int.Parse(AutoIndentations.Text);
                    if (t > 0 && t < 1000)
                    {
                        User.Default.autoIndentations = t;
                        SaveUserSettings();
                        ShowMessage("已更改");
                    }
                    else
                    {
                        AutoIndentations.Text = User.Default.autoIndentations.ToString();
                        ShowMessage("输入1~999整数");
                    }
                }
                catch (Exception)
                {
                    AutoIndentations.Text = User.Default.autoIndentations.ToString();
                    ShowMessage("输入整数");
                }
            }
        }
        private void AutoSaveWhenSwitch_Checked(object sender, RoutedEventArgs e)
        {
            User.Default.isAutoSaveWhenSwitch = true;
            SaveUserSettings();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoSaveWhenSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            User.Default.isAutoSaveWhenSwitch = false;
            SaveUserSettings();
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoSaveEvery_Checked(object sender, RoutedEventArgs e)
        {
            User.Default.isAutoSaveEvery = true;
            SaveUserSettings();
            AutoSaveTime.IsEnabled = true;
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoSaveEvery_Unchecked(object sender, RoutedEventArgs e)
        {
            User.Default.isAutoSaveEvery = false;
            SaveUserSettings();
            AutoSaveTime.IsEnabled = false;
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoSaveTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AutoSaveTime.Text != "" && AutoSaveTime.Text != null)
            {
                try
                {
                    int t = int.Parse(AutoSaveTime.Text);
                    if (t > 0 && t < 1000)
                    {
                        TimeSpan ts = TimeSpan.FromMinutes(t);
                        User.Default.autoSaveMinute = t;
                        SaveUserSettings();
                        AutoSaveTimer.Interval = ts;
                        ShowMessage("已更改");
                    }
                    else
                    {
                        AutoSaveTime.Text = User.Default.autoSaveMinute.ToString();
                        ShowMessage("输入1~999整数");
                    }
                }
                catch (Exception)
                {
                    AutoSaveTime.Text = User.Default.autoSaveMinute.ToString();
                    ShowMessage("输入整数");
                }
            }
        }
        private void AutoBackup_Checked(object sender, RoutedEventArgs e)
        {
            User.Default.isAutoBackup = true;
            SaveUserSettings();
            AutoBackupTime.IsEnabled = true;
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoBackup_Unchecked(object sender, RoutedEventArgs e)
        {
            User.Default.isAutoBackup = false;
            SaveUserSettings();
            AutoBackupTime.IsEnabled = false;
            //显示消息
            ShowMessage("已更改");
        }
        private void AutoBackupTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AutoBackupTime.Text != "" && AutoBackupTime.Text != null)
            {
                try
                {
                    int t = int.Parse(AutoBackupTime.Text);
                    if (t > 0 && t < 1000)
                    {
                        TimeSpan ts = TimeSpan.FromMinutes(t);
                        User.Default.autoBackupMinute = t;
                        SaveUserSettings();
                        AutoBackupTimer.Interval = ts;
                        //显示消息
                        ShowMessage("已更改");
                    }
                    else
                    {
                        AutoBackupTime.Text = User.Default.autoBackupMinute.ToString();
                        ShowMessage("输入1~999整数");
                    }
                }
                catch (Exception)
                {
                    AutoBackupTime.Text = User.Default.autoBackupMinute.ToString();
                    ShowMessage("输入整数");
                }
            }
        }
        private void TextSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextSize.Text != "" && TextSize.Text != null)
            {
                try
                {
                    int i = int.Parse(TextSize.Text);
                    if (i > 0 && i < 1000)
                    {
                        User.Default.fontSize = i;
                        SaveUserSettings();
                        TbxFileContent.FontSize = i;
                        ShowMessage("已更改");
                    }
                    else
                    {
                        TextSize.Text = User.Default.fontSize.ToString();
                        ShowMessage("输入1~999整数");
                    }
                }
                catch (Exception)
                {
                    TextSize.Text = User.Default.fontSize.ToString();
                    ShowMessage("输入整数");
                }
            }
        }

        //计时器事件
        private void TimerAutoSave_Tick(object sender, EventArgs e)
        {
            if (CurrentEssay.Name != null && User.Default.isAutoSaveEvery == true)
            {
                SaveFile();
                //显示消息
                ShowMessage("已自动保存于", DateTime.Now.ToLongTimeString().ToString());
            }
        }
        private void TimerAutoBackup_Tick(object sender, EventArgs e)
        {
            //timer2.Stop();
            Backup();
        }
        #endregion
    }


    /// <summary>
    /// 书籍的节点
    /// </summary>
    public class TreeViewItemNode : TreeViewItem
    {
        /// <summary>
        /// 节点类型
        /// </summary>
        public bool IsFile { get; set; }

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
        public TreeViewItemNode(string DisplayName, string _fileOrFolder, bool isFile, bool isExpanded, List<TreeViewItemNode> nodes)
        {
            Header = DisplayName;
            ToolTip = _fileOrFolder;
            IsFile = isFile;
            IsExpanded = isExpanded;
            if (nodes != null)
            {
                foreach (TreeViewItemNode item in nodes)
                {
                    Items.Add(item);
                }
            }
        }
    }
}
