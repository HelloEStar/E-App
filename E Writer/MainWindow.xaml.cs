using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using MessageBox = System.Windows.MessageBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using SharedProject;

namespace E.Writer
{
    public partial class MainWindow : EWindow
    {
        private FileOrFolderInfo CurrentBook { get; set; }
        private FileOrFolderInfo CurrentChapter { get; set; }
        private FileOrFolderInfo CurrentEssay { get; set; }
        private bool IsSaved { get; set; } = true;
        private string CurrentFindText { get; set; }
        private string ReplaceText { get; set; }
        private int NextStartIndex { get; set; }
        private TreeViewItemNode SelectedNode { get; set; }

        private DispatcherTimer AutoSaveTimer { get; set; }
        private DispatcherTimer AutoBackupTimer { get; set; }

        #region 方法
        //构造
        public MainWindow()
        {
            InitializeComponent();
        }

        //载入
        /// <summary>
        /// 获取字体选项
        /// </summary>
        private void LoadFontItems()
        {
            string ff = Settings.Default.FontFamily;
            CbbFonts.Items.Clear();
            foreach (FontFamily item in Fonts.SystemFontFamilies)
            {
                ComboBoxItem cbi = new ComboBoxItem
                {
                    Content = item.Source,
                    FontFamily = item
                };
                CbbFonts.Items.Add(cbi);
            }

            foreach (ComboBoxItem item in CbbFonts.Items)
            {
                if (ff == item.Content.ToString())
                {
                    CbbFonts.SelectedItem = item;
                    break;
                }
            }
        }
        /// <summary>
        /// 载入书籍选项
        /// </summary>
        private void LoadBookItems()
        {
            //验证上次关闭的书籍路径是否存在，若不存在，重置为根目录
            if (!Directory.Exists(Settings.Default.LastBookPath))
            {
                Settings.Default.LastBookPath = Settings.Default.BooksDir;
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
                CbbBooks.ToolTip = FindResource("未打开任何书籍");
            }
        }
        /// <summary>
        /// 重新导入书籍目录
        /// </summary>
        private void ReloadTvwBook()
        {
            TvwBook.Items.Clear();
            ScanBookPath(CurrentBook.Path);
            //提示消息
            ShowMessage(FindResource("目录已重新导入").ToString());
        }

        //打开
        /// <summary>
        /// 打开新书籍
        /// </summary>
        private void OpenNewBook()
        {
            string path;
            if (!Directory.Exists(Settings.Default.LastBookPath))
            {
                path = Settings.Default.BooksDir;
            }
            else
            {
                path = Settings.Default.LastBookPath;
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
                Settings.Default.LastBookPath = CurrentBook.Path;
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
                TxtFileName.IsEnabled = false;
                TxtFileContent.IsEnabled = false;
                TxtCreatePath.Text = book.Path;
                Settings.Default.BooksDir = Path.GetDirectoryName(book.Path);
                //加入书籍列表
                AddBookItem(true, book);
                SetBookItem(CurrentBook.Path);
                GetBookInfo();
                TvwBook.ToolTip = FindResource("当前书籍") + "：" + book.Name + Environment.NewLine + FindResource("书籍位置") + "：" + book.Path;
                RefreshTitle();
                RefreshBtnsState();
                ReloadTvwBook();

                ShowMessage(FindResource("已打开书籍").ToString() + "：" + book.Name);
            }
            else
            {
                ShowMessage(FindResource("此书籍不存在").ToString(), true);
            }
        }
        /// <summary>
        /// 打开卷册
        /// </summary>
        /// <param name="path">书籍文件夹路径</param>
        private void OpenChapter(string path)
        {
            if (Directory.Exists(path))
            {
                CurrentChapter = new FileOrFolderInfo(path);
                CurrentEssay = null;
                TxtFileName.IsEnabled = true;
                TxtFileContent.IsEnabled = false;
                TxtCreatePath.Text = path;
                GetChapterInfo();
                RefreshTitle();
                RefreshBtnsState();

                ShowMessage(FindResource("已打开卷册").ToString() + "：" + CurrentChapter.Name);
            }
            else
            {
                ShowMessage(FindResource("此卷册不存在").ToString(), true);
            }
        }
        /// <summary>
        /// 打开文章
        /// </summary>
        /// <param name="path">文章路径</param>
        private void OpenEssay(string path)
        {
            if (File.Exists(path))
            {
                //刷新选择信息
                CurrentEssay = new FileOrFolderInfo(path);
                CurrentChapter = new FileOrFolderInfo(Path.GetDirectoryName(CurrentEssay.Path));
                //如果上级文件夹是书籍文件夹
                if (CurrentChapter.Path == CurrentBook.Path)
                {
                    CurrentChapter = null;
                }

                ShowMessage(FindResource("正在读取文本").ToString());
                RefreshWindow();

                //创建读取文件
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader sw = new StreamReader(fs);
                TxtFileContent.Text = sw.ReadToEnd();
                sw.Close();
                fs.Close();
                TxtFileName.Text = Path.GetFileNameWithoutExtension(CurrentEssay.Path);
                TxtFileName.IsEnabled = true;
                TxtFileContent.IsEnabled = true;
                //光标到最后
                TxtFileContent.Focus();
                TxtFileContent.Select(TxtFileContent.Text.Length, 0);
                TxtFileContent.ScrollToEnd();
                IsSaved = true;

                RefreshBtnsState();
                RefreshTitle();

                ShowMessage(FindResource("已打开文章").ToString() + "：" + CurrentEssay.Name);
            }
            else
            {
                ShowMessage(FindResource("此文章不存在").ToString(), true);
            }
        }
        /// <summary>
        /// 打开上次关闭的书籍
        /// </summary>
        private void OpenLastBook()
        {
            foreach (ComboBoxItem item in CbbBooks.Items)
            {
                if (item.Tag.ToString() == Settings.Default.LastBookPath)
                {
                    CbbBooks.SelectedItem = item;
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
            if (Directory.Exists(Settings.Default.LastBookPath))
            {
                //获取当前书籍（文件夹）的名字、路径、根目录
                CurrentBook = new FileOrFolderInfo(Settings.Default.LastBookPath);
                //同步书籍列表选项
                SetBookItem(CurrentBook.Path);
                //显示书籍信息
                GetBookInfo();
                TvwBook.ToolTip = FindResource("当前书籍") + "：" + CurrentBook.Name + Environment.NewLine + FindResource("书籍位置") + "：" + CurrentBook.Path;
                //改变标题
                RefreshTitle();
                //改变控件状态
                RefreshBtnsState();
                //重新导入目录
                ReloadTvwBook();

                //显示消息
                ShowMessage(FindResource("已自动打开书籍").ToString() + "：" + CurrentBook.Name);
            }
            else
            {
                ShowMessage(FindResource("自动打开书籍失败").ToString(), true);
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

                TxtFileName.Text = FindResource("欢迎使用") + "：" + AppInfo.Name;
                TxtFileName.ToolTip = null;
                TxtFileContent.Text = FindResource("创建或打开以开始").ToString();
                TxtWordCount.ToolTip = null;
                TxtWordCount.Text = FindResource("字数") + "：0";
                TvwBook.ToolTip = FindResource("创建或打开以开始");

                ShowMessage(FindResource("已关闭书籍").ToString() + "：" + CurrentBook.Name);
                CurrentBook = null;
                RefreshBtnsState();
                RefreshTitle();
                TvwBook.Items.Clear();
                //刷新目录
                //TvwBook.Items.Refresh();
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
                if (Settings.Default.IsAutoSaveWhenSwitch == true)
                { SaveFile(); }

                TxtFileName.Text = null;
                TxtFileName.ToolTip = null;
                TxtFileContent.Text = null;
                TxtFileContent.IsEnabled = false;

                TxtWordCount.Text = FindResource("字数") + "：0";
                TxtWordCount.ToolTip = null;

                ShowMessage(FindResource("已关闭文章").ToString() + "：" + CurrentEssay.Name);
                CurrentEssay = null;
                if (CurrentChapter != null)
                {
                    GetChapterInfo();
                    TxtFileName.IsEnabled = true;
                }
                else
                {
                    TxtFileName.IsEnabled = false;
                }

                SelectedNode = null;

                RefreshBtnsState();
                RefreshTitle();
            }
        }

        //保存
        /// <summary>
        /// 保存应用设置
        /// </summary>
        protected override void SaveSettings()
        {
            if (Settings.Default.RunCount > 0)
            {
                Settings.Default._books = "";
                List<string> strs = new List<string>();
                foreach (ComboBoxItem item in CbbBooks.Items)
                {
                    strs.Add(item.Tag.ToString());
                }
                Settings.Default._books = string.Join("///", strs);
            }

            Settings.Default.Save();
            ShowMessage(FindResource("已保存").ToString());
        }
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
                    sw.Write(TxtFileContent.Text);
                    sw.Close();
                    fs.Close();
                    IsSaved = true;
                    //显示消息
                    ShowMessage(FindResource("保存成功").ToString());
                }
                else
                {
                    ShowMessage(FindResource("保存失败").ToString());
                }
                RefreshTitle();
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
                            sw.Write(TxtFileContent.Text);
                            sw.Close();
                        };
                        st.Close();
                        //显示消息
                        ShowMessage(FindResource("另存为成功").ToString());
                    }
                }
                RefreshTitle();
            }
        }

        //创建
        /// <summary>
        /// 创建计时器
        /// </summary>
        private void CreateTimer()
        {
            AutoSaveTimer = new DispatcherTimer
            { Interval = TimeSpan.FromMinutes(Settings.Default.AutoSaveMinute) };
            AutoSaveTimer.Tick += new EventHandler(TimerAutoSave_Tick);
            AutoSaveTimer.Start();

            AutoBackupTimer = new DispatcherTimer
            { Interval = TimeSpan.FromMinutes(Settings.Default.AutoBackupMinute) };
            AutoBackupTimer.Tick += new EventHandler(TimerAutoBackup_Tick);
            AutoBackupTimer.Start();
        }
        /// <summary>
        /// 创建
        /// </summary>
        private void Create()
        {
            if (PanCreate.Visibility == Visibility.Visible)
            {
                PanCreate.Visibility = Visibility.Collapsed;
            }
            else
            {
                PanCreate.Visibility = Visibility.Visible;
            }

            //获取上级目录
            if (Settings.Default.LastBookPath == Settings.Default.BooksDir)
            {
                TxtCreatePath.Text = Settings.Default.BooksDir;
            }
            else
            {
                if (!string.IsNullOrEmpty(Settings.Default.LastBookPath))
                {
                    TxtCreatePath.Text = Path.GetDirectoryName(Settings.Default.LastBookPath);
                }
            }

            if (CurrentBook != null)
            {
                TxtCreatePath.Text = CurrentBook.Path;
            }

            if (TxtCreatePath.Text == null || TxtCreatePath.Text == "")
            {
                BtnCreateBook.ToolTip = FindResource("请设置存放位置").ToString();
                BtnCreateChapter.ToolTip = FindResource("请设置存放位置").ToString();
                BtnCreateEssay.ToolTip = FindResource("请设置存放位置").ToString();
            }
            else
            {
                BtnCreateBook.ToolTip = TxtCreatePath.Text + @"\" + TxtCreateName.Text;
                BtnCreateChapter.ToolTip = TxtCreatePath.Text + @"\" + TxtCreateName.Text;
                BtnCreateEssay.ToolTip = TxtCreatePath.Text + @"\" + TxtCreateName.Text + ".txt";
            }

            TxtCreateName.Text = "未命名";
        }
        /// <summary>
        /// 创建书籍
        /// </summary>
        private void CreateBook()
        {
            if (TxtCreatePath.Text == null || TxtCreatePath.Text == "")
            {
                ShowMessage(FindResource("请设置存放位置").ToString(), true);
                return;
            }
            if (TxtCreateName.Text == null || TxtCreateName.Text == "")
            {
                ShowMessage(FindResource("请输入书籍名").ToString(), true);
                return;
            }
            if (!CheckIsRightName(TxtCreateName.Text))
            {
                ShowMessage(FindResource("名称中不能含有以下字符").ToString() + " \\ | / < > \" ? * : ", true);
                return;
            }

            string path = BtnCreateBook.ToolTip.ToString();
            if (CurrentBook != null)
            {
                if (path.Contains(CurrentBook.Path) && Path.GetDirectoryName(path) != Path.GetDirectoryName(CurrentBook.Path))
                {
                    ShowMessage(FindResource("请勿把书籍创建在书籍内").ToString(), true);
                    return;
                }
            }
            if (Directory.Exists(path))
            {
                ShowMessage(FindResource("同名书籍已存在").ToString(), true);
                return;
            }

            //保存当前文件再创建书籍
            if (CurrentEssay != null && Settings.Default.IsAutoSaveWhenSwitch == true)
            {
                SaveFile();
            }
            CurrentBook = new FileOrFolderInfo(path);
            //创建书籍文件夹
            Directory.CreateDirectory(CurrentBook.Path);
            Settings.Default.LastBookPath = CurrentBook.Path;
            //打开
            OpenBook(CurrentBook);
        }
        /// <summary>
        /// 创建卷册
        /// </summary>
        private void CreateChapter()
        {
            if (TxtCreatePath.Text == null || TxtCreatePath.Text == "")
            {
                ShowMessage(FindResource("请设置存放位置").ToString(), true);
                return;
            }
            if (TxtCreateName.Text == null || TxtCreateName.Text == "")
            {
                ShowMessage(FindResource("请输入卷册名").ToString(), true);
                return;
            }
            if (!CheckIsRightName(TxtCreateName.Text))
            {
                ShowMessage(FindResource("名称中不能含有以下字符").ToString() + " \\ | / < > \" ? * : ", true);
                return;
            }

            string path = BtnCreateChapter.ToolTip.ToString();
            if (!path.Contains(CurrentBook.Path) || path == CurrentBook.Path)
            {
                ShowMessage(FindResource("请勿把卷册创建在书籍外").ToString(), true);
                return;
            }
            if (Directory.Exists(path))
            {
                ShowMessage(FindResource("同名卷册已存在").ToString(), true);
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
                TvwBook.Items.Add(newfolderNode);
            }
            else
            {
                fatherNote.IsExpanded = true;
                fatherNote.Items.Add(newfolderNode);
            }
            SelectedNode = newfolderNode;
            TvwBook.Items.Refresh();
        }
        /// <summary>
        /// 创建文章
        /// </summary>
        private void CreateEssay()
        {
            if (TxtCreatePath.Text == null || TxtCreatePath.Text == "")
            {
                ShowMessage(FindResource("请设置存放位置").ToString(), true);
                return;
            }
            if (TxtCreateName.Text == null || TxtCreateName.Text == "")
            {
                ShowMessage(FindResource("请输入文章名").ToString(), true);
                return;
            }
            if (!CheckIsRightName(TxtCreateName.Text))
            {
                ShowMessage(FindResource("名称中不能含有以下字符").ToString() + " \\ | / < > \" ? * : ", true);
                return;
            }

            string path = BtnCreateEssay.ToolTip.ToString();
            if (!path.Contains(CurrentBook.Path) || path.Replace(".txt", "") == CurrentBook.Path)
            {
                ShowMessage(FindResource("请勿把文章创建在书籍外").ToString(), true);
                return;
            }
            if (File.Exists(path))
            {
                ShowMessage(FindResource("同名文章已存在").ToString(), true);
                return;
            }

            //创建前保存文章
            if (CurrentEssay != null)
            {
                SaveFile();
            }
            File.Create(path).Close();
            CurrentEssay = new FileOrFolderInfo(path);
            LblMessage.Content = "已创建文章 " + CurrentEssay.Name;
            OpenEssay(path);
            RefreshBtnsState();
            RefreshTitle();

            //创建节点
            TreeViewItemNode newFileNode = new TreeViewItemNode(CurrentEssay.Name, CurrentEssay.Path, true, false, null);
            TreeViewItemNode fatherNote = FindNote(Path.GetDirectoryName(CurrentEssay.Path));
            if (fatherNote == null)
            {
                TvwBook.Items.Add(newFileNode);
            }
            else
            {
                fatherNote.IsExpanded = true;
                fatherNote.Items.Add(newFileNode);
            }
            //选择节点
            SelectedNode = newFileNode;
            //刷新目录
            TvwBook.Items.Refresh();
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
            CbbBooks.ToolTip = FindResource("最近打开过的书籍列表");
            bool hasBook = false;
            //检测列表是否有此书
            if (CbbBooks.Items.Count > 0)
            {
                foreach (ComboBoxItem item in CbbBooks.Items)
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

                CbbBooks.Items.Add(item);

                if (isAdd)
                {
                    //记录的书籍数+1
                    Settings.Default.BookCount += 1;
                }
            }
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
                    TvwBook.Items.Remove(node);
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
                    TvwBook.Items.Remove(node);
                }
                //刷新视图
                TvwBook.Items.Refresh();
            }
        }
        /// <summary>
        /// 移除书籍打开记录
        /// </summary>
        /// <param name="book"></param>
        private void RemoveBookHistory(FileOrFolderInfo book)
        {
            foreach (ComboBoxItem item in CbbBooks.Items)
            {
                if (item.Tag.ToString() == book.Path)
                {
                    CloseBook();
                    CbbBooks.Items.Remove(item);
                    Settings.Default._books = Settings.Default._books.Replace(book.Path, "");
                    break;
                }
            }
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
                    ShowMessage(FindResource("请选择目标").ToString());
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
                    for (int i = 0; i < CbbBooks.Items.Count; i++)
                    {
                        if (((ComboBoxItem)CbbBooks.Items[i]).Tag.ToString() == CurrentBook.Path)
                        {
                            CbbBooks.Items.RemoveAt(i);
                            break;
                        }
                    }

                    TxtFileName.Text = FindResource("欢迎使用") + "：" + AppInfo.Name;
                    TxtFileName.ToolTip = null;
                    TxtFileContent.Text = FindResource("创建或打开以开始").ToString();
                    TxtWordCount.ToolTip = null;
                    TxtWordCount.Text = FindResource("字数") + "：0";
                    TvwBook.ToolTip = FindResource("创建或打开以开始");

                    ShowMessage(FindResource("已删除书籍").ToString() + CurrentBook.Name);
                    CurrentBook = null;
                    CurrentChapter = null;
                    CurrentEssay = null;
                    RefreshBtnsState();
                    RefreshTitle();
                    TvwBook.Items.Clear();
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
            MessageBoxResult result = MessageBox.Show("是否删除卷册：" + name + " ？", "删除项目", MessageBoxButton.YesNo);
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
                        TxtFileName.Text = FindResource("未选中任何文章").ToString();
                        TxtFileName.ToolTip = null;
                        TxtFileContent.Text = null;
                        TxtFileName.IsEnabled = false;
                        TxtFileContent.IsEnabled = false;
                        RefreshBtnsState();
                        RefreshTitle();
                    }
                }
                ShowMessage(FindResource("已删除卷册").ToString() + "：" + name);
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
            MessageBoxResult result = MessageBox.Show("是否删除文章：" + name + " ？", "删除项目", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                RemoveFolderNode(SelectedNode);
                File.Delete(_path);
                //如果选择的是正在编辑的
                if (CurrentEssay != null)
                {
                    if (_path == CurrentEssay.Path)
                    {
                        TxtFileName.Text = FindResource("未选中任何文章").ToString();
                        TxtFileName.ToolTip = null;
                        TxtFileContent.Text = null;
                        TxtFileName.IsEnabled = false;
                        TxtFileContent.IsEnabled = true;
                        CurrentEssay = null;
                        if (CurrentChapter != null)
                        {
                            GetChapterInfo();
                            TxtFileName.IsEnabled = true;
                        }
                        else
                        {
                            TxtFileName.IsEnabled = false;
                        }
                        RefreshBtnsState();
                        RefreshTitle();
                    }
                }
                ShowMessage(FindResource("已删除文章").ToString() + "：" + name);
            }
        }

        //清空
        /// <summary>
        /// 清空文本
        /// </summary>
        private void ClearNameAndContent()
        {
            TxtFileName.Text = "";
            TxtFileContent.Text = "";
            TxtFileName.IsEnabled = false;
            TxtFileContent.IsEnabled = false;
        }

        //获取
        /// <summary>
        /// 书籍信息
        /// </summary>
        private void GetBookInfo()
        {
            if (CurrentBook != null)
            {
                TxtWordCount.Text = FindResource("字数") + "：0";
                TxtFileName.Text = CurrentBook.Name;
                TxtFileContent.Text = FindResource("创建时间") + "：" + Directory.GetCreationTime(CurrentBook.Path) + Environment.NewLine +
                                   FindResource("子卷册数") + "：" + GetDirCounts(CurrentBook.Path) + Environment.NewLine +
                                   FindResource("总文章数") + "：" + GetFileCounts(CurrentBook.Path) + Environment.NewLine +
                                   FindResource("全书字数") + "：" + GetBookWords(CurrentBook.Path, "");
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
                TxtWordCount.Text = FindResource("字数") + "：0";
                TxtFileName.Text = CurrentChapter.Name;
                TxtFileContent.Text = FindResource("创建时间") + "：" + Directory.GetCreationTime(CurrentChapter.Path) + Environment.NewLine +
                                   FindResource("子卷册数") + "：" + GetDirCounts(CurrentChapter.Path) + Environment.NewLine +
                                   FindResource("总文章数") + "：" + GetFileCounts(CurrentChapter.Path) + Environment.NewLine +
                                   FindResource("全书字数") + "：" + GetBookWords(CurrentChapter.Path, "");
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
            string[] _files = Directory.GetFiles(_path);
            int n = _files.Count();
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
                    MatchCollection space = Regex.Matches(TxtFileContent.Text, @"\s");
                    int w = TxtFileContent.Text.Length - space.Count;
                    n += TxtFileContent.Text.Length - space.Count;
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
                int line = TxtFileContent.GetLineIndexFromCharacterIndex(TxtFileContent.SelectionStart) + 1;
                //得到当前行第一个字符的索引
                int index = TxtFileContent.GetCharacterIndexFromLineIndex(line - 1);
                //.SelectionStart得到光标所在位置的索引 减去 当前行第一个字符的索引 = 光标所在的列数（从0开始)
                int column = TxtFileContent.SelectionStart - index + 1;
                string rac = FindResource("行").ToString() + "：" + line + "    " + FindResource("列").ToString() + "：" + column;
                TxtRowAndColumn.Text = rac;
            }
            else
            {
                TxtRowAndColumn.Text = FindResource("行").ToString() + "：" + 0 + "    " + FindResource("列").ToString() + "：" + 0; ;
            }
        }
        /// <summary>
        /// 简转繁
        /// </summary>
        /// <param name="simplifiedChinese">文字内容</param>
        /// <returns>繁体文字内容</returns>
        private static string GetTraditional(string simplifiedChinese)
        {
            return ChineseConverter.Convert(simplifiedChinese, ChineseConversionDirection.SimplifiedToTraditional);

        }
        /// <summary>
        /// 繁转简
        /// </summary>
        /// <param name="traditionalChinese">文字内容</param>
        /// <returns>简体文字内容</returns>
        private static string GetSimplified(string traditionalChinese)
        {
            return ChineseConverter.Convert(traditionalChinese, ChineseConversionDirection.TraditionalToSimplified);
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
            foreach (TreeViewItemNode item in TvwBook.Items)
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
        protected override void SetMenuTool(MenuTool menu)
        {
            switch (menu)
            {
                case MenuTool.无:
                    PanFile.Visibility = Visibility.Collapsed;
                    PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.Background = BrushBG01;
                    BtnEdit.Background = BrushBG01;
                    BtnSetting.Background = BrushBG01;
                    BtnAbout.Background = BrushBG01;
                    break;
                case MenuTool.文件:
                    PanFile.Visibility = Visibility.Visible;
                    PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.Background = BrushBG02;
                    BtnEdit.Background = BrushBG01;
                    BtnSetting.Background = BrushBG01;
                    BtnAbout.Background = BrushBG01;
                    break;
                case MenuTool.编辑:
                    PanFile.Visibility = Visibility.Collapsed;
                    PanEdit.Visibility = Visibility.Visible;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.Background = BrushBG01;
                    BtnEdit.Background = BrushBG02;
                    BtnSetting.Background = BrushBG01;
                    BtnAbout.Background = BrushBG01;
                    break;
                case MenuTool.设置:
                    PanFile.Visibility = Visibility.Collapsed;
                    PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Visible;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.Background = BrushBG01;
                    BtnEdit.Background = BrushBG01;
                    BtnSetting.Background = BrushBG02;
                    BtnAbout.Background = BrushBG01;
                    break;
                case MenuTool.关于:
                    PanFile.Visibility = Visibility.Collapsed;
                    PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Visible;
                    BtnFile.Background = BrushBG01;
                    BtnEdit.Background = BrushBG01;
                    BtnSetting.Background = BrushBG01;
                    BtnAbout.Background = BrushBG02;
                    break;
                default:
                    break;
            }
            CurrentMenuTool = menu;
        }

        private void SetBookItem(string path)
        {
            foreach (ComboBoxItem item in CbbBooks.Items)
            {
                if (item.Tag.ToString() == path)
                {
                    CbbBooks.SelectedItem = item;
                    Settings.Default.LastBookPath = path;
                    break;
                }
            }
        }
        private static void SetEncodeType(string path, Encoding encoding)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] flieByte = new byte[fs.Length];
            fs.Read(flieByte, 0, flieByte.Length);
            fs.Close();

            Encoding ec = Encoding.GetEncoding("UTF-8");
            StreamWriter sw = new StreamWriter(path, false, ec);
            sw.Write(encoding.GetString(flieByte));
            sw.Close();
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

        //选择
        /// <summary>
        /// 点击一个节点
        /// </summary>
        /// <param name="clickTimes">点击次数</param>
        private void SelectNode(int clickTimes)
        {
            //记录选择的节点
            SelectedNode = (TreeViewItemNode)TvwBook.SelectedItem;
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
                        if (CurrentEssay != null && Settings.Default.IsAutoSaveWhenSwitch == true)
                        { SaveFile(); }
                        //打开
                        OpenEssay(SelectedNode.ToolTip.ToString());
                    //SaveFile();
                    finish:;
                    }
                    else
                    { ShowMessage(FindResource("双击或按回车键打开该文章").ToString()); }
                }
                //如果选中的节点是文件夹
                else if (SelectedNode.IsFile == false)
                {
                    //如果选中的节点是文件夹，且双击
                    if (clickTimes == 2)
                    {
                        //保存当前文件再打开新文件
                        if (CurrentEssay != null && Settings.Default.IsAutoSaveWhenSwitch == true)
                        { SaveFile(); }
                        //打开卷册
                        OpenChapter(SelectedNode.ToolTip.ToString());
                    }
                    else
                    { ShowMessage(FindResource("双击或按回车键打开该卷册").ToString()); }
                }
            }
        }

        //检查
        /// <summary>
        /// 用户是否同意
        /// </summary>
        /// <returns></returns>
        private bool IsUserAgree()
        {
            string str = AppInfo.UserAgreement + "\n\n你需要同意此协议才能使用本软件，是否同意？";
            MessageBoxResult result = MessageBox.Show(str, FindResource("用户协议").ToString(), MessageBoxButton.YesNo);
            return (result == MessageBoxResult.Yes);
        }
        /// <summary>
        /// 检查用户协议
        /// </summary>
        private void CheckUserAgreement()
        {
            Settings.Default.RunCount += 1;
            if (Settings.Default.RunCount == 1)
            {
                if (!IsUserAgree())
                {
                    Settings.Default.RunCount = 0;
                    Close();
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
                SelectedPath = Path.GetDirectoryName(Settings.Default.LastBookPath),
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
                        ShowMessage(FindResource("已取消导出").ToString());
                    }
                }
                else
                {
                    Export(output);
                }
            }
        }
        /// <summary>
        /// 检查自动打开书籍
        /// </summary>
        private void CheckAutoOpenBook()
        {
            if (Settings.Default.IsAutoOpenBook)
            {
                OpenLastBook();
            }
            else
            {
                TvwBook.ToolTip = FindResource("创建或打开以开始");
                LblMessage.Content = FindResource("创建或打开以开始").ToString();
            }
        }
        /// <summary>
        /// 检查显示运行信息
        /// </summary>
        private void CheckShowRunInfo()
        {
            if (Settings.Default.IsShowRunInfo)
            {
                RefreshRunInfo();
            }
        }
        /// <summary>
        /// 检查文章是否保存
        /// </summary>
        /// <returns>是否关闭取消关闭</returns>
        private bool CheckEssayWasSavedAndDoClose()
        {
            bool close = false;
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
                        close = true;
                    }
                }
                else
                {
                    CloseBook();
                }
            }
            return close;
        }

        //刷新
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
        /// 显示软件信息
        /// </summary>
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
        /// <summary>
        /// 刷新按钮状态
        /// </summary>
        protected void RefreshBtnsState()
        {
            BtnOpenBook.IsEnabled = true;
            BtnCreateBook.IsEnabled = true;
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
                    TxtFind.IsEnabled = true;
                    TxtReplace.IsEnabled = true;
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
                    TxtFind.IsEnabled = false;
                    TxtReplace.IsEnabled = false;
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
                TxtFind.IsEnabled = false;
                TxtReplace.IsEnabled = false;
            }
        }
        /// <summary>
        /// 刷新主窗口标题
        /// </summary>
        protected override void RefreshTitle()
        {
            string str = AppInfo.Name + " " + AppInfo.VersionShort;
            Main.Title = str;
            if (CurrentBook == null)
            {
            }
            else
            {
                if (CurrentChapter == null)
                {
                    if (CurrentEssay == null)
                    {
                        Main.Title += " - " + CurrentBook.Name;
                    }
                    else
                    {
                        Main.Title += " - " + CurrentBook.Name + @"\" + CurrentEssay.Name;
                        if (!IsSaved)
                        {
                            Main.Title += "*";
                        }
                    }
                }
                else
                {
                    if (CurrentEssay == null)
                    {
                        Main.Title += " - " + CurrentBook.Name + @"\" + CurrentChapter.Path.Replace(CurrentBook.Path + @"\", "");
                    }
                    else
                    {
                        Main.Title += " - " + CurrentBook.Name + @"\" + CurrentEssay.Path.Replace(CurrentBook.Path + @"\", "");
                        if (!IsSaved)
                        {
                            Main.Title += "*";
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 刷新字体大小
        /// </summary>
        private void RefreshFontSize()
        {
            TxtFontSize.Text = Settings.Default.FontSize.ToString();
            if (TxtFileContent != null)
            {
                TxtFileContent.FontSize = Settings.Default.FontSize;
            }
        }
        /// <summary>
        /// 刷新运行信息
        /// </summary>
        private void RefreshRunInfo()
        {
            TxtFileName.Text = FindResource("欢迎使用") + " " + AppInfo.Name;
            TxtFileContent.Text = FindResource("启动次数") + "：" + Settings.Default.RunCount;
            TxtFileName.IsEnabled = false;
            TxtFileContent.IsEnabled = false;
        }
        /// <summary>
        /// 刷新滑块自动备份时间
        /// </summary>
        private void RefreshSldAutoBackupMinute()
        {
            if (SldAutoBackupMinute != null)
            {
                SldAutoBackupMinute.IsEnabled = CcbAutoBackup.IsChecked == true;
            }
        }
        /// <summary>
        /// 刷新滑块自动缩进数量
        /// </summary>
        private void RefreshSldAutoIndentations()
        {
            if (SldAutoIndentations != null)
            {
                SldAutoIndentations.IsEnabled = CcbAutoIndentation.IsChecked == true;
            }
        }
        /// <summary>
        /// 刷新滑块自动保存时间
        /// </summary>
        private void RefreshSldAutoSaveMinute()
        {
            if (SldAutoSaveMinute != null)
            {
                SldAutoSaveMinute.IsEnabled = CcbAutoSaveEveryMinutes.IsChecked == true;
            }
        }

        //显示
        protected override void ShowMessage(string message, bool newBox = false)
        {
            ShowMessage(LblMessage, message, newBox);
        }
        private void ExpandTree()
        {
            foreach (TreeViewItemNode item in TvwBook.Items)
            {
                //DependencyObject dObject = TvwBook.ItemContainerGenerator.ContainerFromItem(item);
                //((FileNode)dObject).ExpandSubtree();
                item.ExpandSubtree();
            }
        }

        //隐藏
        /// <summary>
        /// 收起目录
        /// </summary>
        private void CollapseTree()
        {
            foreach (TreeViewItemNode item in TvwBook.Items)
            {
                //DependencyObject dObject = TvwBook.ItemContainerGenerator.ContainerFromItem(item);
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
                TvwBook.Items.Add(fileNode);
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
                    TvwBook.Items.Add(node);
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
            ShowMessage(FindResource("正在导出txt").ToString());
            RefreshWindow();

            File.CreateText(_output).Close();
            //书名
            //File.AppendAllText(output, selectedBook + Environment.NewLine);
            foreach (TreeViewItemNode txt in TvwBook.Items)
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
            ShowMessage(FindResource("导出成功").ToString() + "：" + l);
            Process.Start(Path.GetDirectoryName(_output));
        }
        /// <summary>
        /// 自动缩进
        /// </summary>
        private void Indentation()
        {
            int start = TxtFileContent.SelectionStart;
            string spaces = "";
            for (int i = 0; i < Settings.Default.AutoIndentations; i++)
            {
                spaces += " ";
            }
            TxtFileContent.Text = TxtFileContent.Text.Insert(start, spaces);
            TxtFileContent.Select(start + spaces.Length, 0);
        }
        /// <summary>
        /// 自动备份
        /// </summary>
        private void Backup()
        {
            if (Settings.Default.IsAutoBackup == true)
            {
                if (CurrentBook != null && Directory.Exists(CurrentBook.Path))
                {
                    ShowMessage(FindResource("书籍备份中").ToString());
                    RefreshWindow();
                    string _path = Settings.Default.BackupDir + @"\" + CurrentBook.Name;
                    //删除上个备份
                    if (Directory.Exists(_path))
                    { Directory.Delete(_path, true); }
                    //创建新的备份
                    Directory.CreateDirectory(_path);
                    CopyDirectory(CurrentBook.Path, _path);
                    //显示消息
                    ShowMessage(FindResource("已自动备份于").ToString() + DateTime.Now.ToLongTimeString().ToString());
                }
            }
        }
        #endregion 

        #region 事件
        //主窗口
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            //载入
            LoadLanguageItems(CbbLanguages);
            LoadThemeItems(CbbThemes);
            LoadFontItems();
            LoadBookItems();

            //初始化
            CheckAutoOpenBook();
            CheckShowRunInfo();
            CreateTimer();

            //刷新
            RefreshAppInfo();
            RefreshBtnsState();
            RefreshTitle();
            RefreshFontSize();
            RefreshSldAutoBackupMinute();
            RefreshSldAutoIndentations();
            RefreshSldAutoSaveMinute();

            //检查用户协议
            CheckUserAgreement();
        }
        private void Main_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = CheckEssayWasSavedAndDoClose();
            SaveSettings();
        }
        protected override void EWindow_KeyUp(object sender, KeyEventArgs e)
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
                    CbbBooks.SelectedItem = null;
                }
                //Ctrl+I 展开目录 
                if (e.Key == Key.I && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
                { ExpandTree(); }
                //Ctrl+U 收起目录
                if (e.Key == Key.U && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
                { CollapseTree(); }
                //Ctrl+R 刷新目录
                if (e.Key == Key.R && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
                { ReloadTvwBook(); }

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
                        BtnEdit_Click(null, null);
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
                        if (TxtFileContent.IsFocused && TxtFileContent.SelectedText != null && TxtFileContent.SelectedText != "")
                        {
                            TxtFileContent.SelectedText = GetSimplified(TxtFileContent.SelectedText);
                        }
                        else if (TxtFileName.IsFocused && TxtFileName.SelectedText != null && TxtFileName.SelectedText != "")
                        {
                            TxtFileName.SelectedText = GetSimplified(TxtFileName.SelectedText);
                        }
                    }
                    //Ctrl+Shift+K 
                    if (e.Key == Key.K && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
                                       && (e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift)))
                    {
                        if (TxtFileContent.IsFocused && TxtFileContent.SelectedText != null && TxtFileContent.SelectedText != "")
                        {
                            TxtFileContent.SelectedText = GetTraditional(TxtFileContent.SelectedText);
                        }
                        else if (TxtFileName.IsFocused && TxtFileName.SelectedText != null && TxtFileName.SelectedText != "")
                        {
                            TxtFileName.SelectedText = GetTraditional(TxtFileName.SelectedText);
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
            { SetNextTheme(CbbThemes, Settings.Default.Theme); }

            base.EWindow_KeyUp(sender, e);
        }
        private void TimerAutoSave_Tick(object sender, EventArgs e)
        {
            if (CurrentEssay.Name != null && Settings.Default.IsAutoSaveEveryMinutes == true)
            {
                SaveFile();
                //显示消息
                ShowMessage(FindResource("已自动保存于").ToString() + DateTime.Now.ToLongTimeString().ToString());
            }
        }
        private void TimerAutoBackup_Tick(object sender, EventArgs e)
        {
            //timer2.Stop();
            Backup();
        }

        //工具栏
        /// 文件
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
            CbbBooks.SelectedItem = null;
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
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete();
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
            ReloadTvwBook();
        }
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.GetFullPath(TxtCreatePath.Text);
            if (!Directory.Exists(path))
            {
                path = Path.GetFullPath(Settings.Default.BooksDir);
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
                TxtCreatePath.Text = fbd.SelectedPath;
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
        private void TxtCreatePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TxtCreatePath.Text == null || TxtCreatePath.Text == "")
            {
                BtnCreateBook.ToolTip = FindResource("请设置存放位置").ToString();
                BtnCreateChapter.ToolTip = FindResource("请设置存放位置").ToString();
                BtnCreateEssay.ToolTip = FindResource("请设置存放位置").ToString();
            }
            else
            {
                BtnCreateBook.ToolTip = TxtCreatePath.Text + @"\" + TxtCreateName.Text;
                BtnCreateChapter.ToolTip = TxtCreatePath.Text + @"\" + TxtCreateName.Text;
                BtnCreateEssay.ToolTip = TxtCreatePath.Text + @"\" + TxtCreateName.Text + ".txt";
            }
        }
        private void TxtCreateName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TxtCreateName.Text == null || TxtCreateName.Text == "")
            {
                BtnCreateBook.ToolTip = FindResource("请输入书籍名称").ToString();
                BtnCreateChapter.ToolTip = FindResource("请输入卷册名称").ToString();
                BtnCreateEssay.ToolTip = FindResource("请输入文章名称").ToString();
            }
            else
            {
                BtnCreateBook.ToolTip = TxtCreatePath.Text + @"\" + TxtCreateName.Text;
                BtnCreateChapter.ToolTip = TxtCreatePath.Text + @"\" + TxtCreateName.Text;
                BtnCreateEssay.ToolTip = TxtCreatePath.Text + @"\" + TxtCreateName.Text + ".txt";
            }
        }
        private void CbbBooks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbbBooks.SelectedItem is ComboBoxItem cbi)
            {
                if (CurrentBook != null)
                { CloseBook(); }

                string path = cbi.Tag.ToString();
                if (Directory.Exists(path))
                {
                    CurrentBook = new FileOrFolderInfo(path);
                    OpenBook(CurrentBook);
                }
                else
                {
                    CbbBooks.Items.Remove(cbi);
                    ShowMessage(FindResource("此书籍不存在").ToString(), true);
                }
            }
        }
        private void TvwBook_KeyUp(object sender, KeyEventArgs e)
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
        private void TvwBook_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectNode(1);
        }
        private void TvwBook_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectNode(2);
        }
        ///编辑
        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            TxtFileContent.Undo();
        }
        private void BtnRedo_Click(object sender, RoutedEventArgs e)
        {
            TxtFileContent.Redo();
        }
        private void BtnToTraditional_Click(object sender, RoutedEventArgs e)
        {
            if (TxtFileContent.IsFocused && TxtFileContent.SelectedText != null && TxtFileContent.SelectedText != "")
            {
                TxtFileContent.SelectedText = GetTraditional(TxtFileContent.SelectedText);
            }
            else if (TxtFileName.IsFocused && TxtFileName.SelectedText != null && TxtFileName.SelectedText != "")
            {
                TxtFileName.SelectedText = GetTraditional(TxtFileName.SelectedText);
            }
        }
        private void BtnToSimplified_Click(object sender, RoutedEventArgs e)
        {
            if (TxtFileContent.IsFocused && TxtFileContent.SelectedText != null && TxtFileContent.SelectedText != "")
            {
                TxtFileContent.SelectedText = GetSimplified(TxtFileContent.SelectedText);
            }
            else if (TxtFileName.IsFocused && TxtFileName.SelectedText != null && TxtFileName.SelectedText != "")
            {
                TxtFileName.SelectedText = GetSimplified(TxtFileName.SelectedText);
            }
        }
        private void BtnFindNext_Click(object sender, RoutedEventArgs e)
        {
            //检测是否输入
            if (TxtFind.Text != null && TxtFind.Text != "")
            {
                //检测是否存在
                if (TxtFileContent.Text.Contains(TxtFind.Text))
                {
                    //获取总个数
                    int i = Regex.Matches(TxtFileContent.Text, TxtFind.Text).Count;
                    TxtFindAmount.Text = string.Format(FindResource("共计？处").ToString(), i);
                    int j = TxtFind.Text.Length;
                    //检测是否同个内容第一次按下
                    if (CurrentFindText != TxtFind.Text)
                    {
                        CurrentFindText = TxtFind.Text;
                        //获取这个的索引
                        NextStartIndex = TxtFileContent.Text.IndexOf(CurrentFindText, 0);
                        //高亮
                        TxtFileContent.Focus();
                        TxtFileContent.Select(NextStartIndex, j);
                        //记录下一个开始寻找的地方
                        NextStartIndex += j;
                    }
                    else
                    {
                        //获取这个的索引
                        NextStartIndex = TxtFileContent.Text.IndexOf(CurrentFindText, NextStartIndex);
                        //检测是否存在
                        if (NextStartIndex >= 0)
                        {
                            //高亮
                            TxtFileContent.Focus();
                            TxtFileContent.Select(NextStartIndex, j);
                            //记录下一个开始寻找的地方
                            NextStartIndex += j;
                        }
                        else
                        {
                            ShowMessage(FindResource("所有位置已查找完毕，再按一次将从头查找").ToString(), true);
                            NextStartIndex = 0;
                        }
                    }
                }
                else
                {
                    ShowMessage(FindResource("没有找到该内容").ToString(), true);
                    TxtFindAmount.Text = string.Format(FindResource("共计？处").ToString(), 0);
                    NextStartIndex = 0;
                }
            }
            else
            {
                ShowMessage(FindResource("请输入要查找的内容").ToString(), true);
                TxtFindAmount.Text = string.Format(FindResource("共计？处").ToString(), 0);
                NextStartIndex = 0;
            }
        }
        private void BtnReplaceNext_Click(object sender, RoutedEventArgs e)
        {
            //检测是否输入查找项
            if (TxtFind.Text != null && TxtFind.Text != "")
            {
                //检测是否存在
                if (TxtFileContent.Text.Contains(TxtFind.Text))
                {
                    int j = TxtFind.Text.Length;
                    int k = TxtReplace.Text.Length;
                    ReplaceText = TxtReplace.Text;
                    //检测是否同个内容第一次按下
                    if (CurrentFindText != TxtFind.Text)
                    {
                        CurrentFindText = TxtFind.Text;
                        //获取这个的索引
                        NextStartIndex = TxtFileContent.Text.IndexOf(CurrentFindText, 0);
                        //移除、插入、高亮
                        TxtFileContent.Focus();
                        TxtFileContent.Text = TxtFileContent.Text.Remove(NextStartIndex, j);
                        TxtFileContent.Text = TxtFileContent.Text.Insert(NextStartIndex, ReplaceText);
                        TxtFileContent.Select(NextStartIndex, k);
                        //记录下一个开始寻找的地方
                        NextStartIndex += k;
                    }
                    else
                    {
                        //获取这个的索引
                        NextStartIndex = TxtFileContent.Text.IndexOf(CurrentFindText, NextStartIndex);
                        //检测是否存在
                        if (NextStartIndex >= 0)
                        {
                            //移除、插入、高亮
                            TxtFileContent.Focus();
                            TxtFileContent.Text = TxtFileContent.Text.Remove(NextStartIndex, j);
                            TxtFileContent.Text = TxtFileContent.Text.Insert(NextStartIndex, ReplaceText);
                            TxtFileContent.Select(NextStartIndex, k);
                            //记录下一个开始寻找的地方
                            NextStartIndex += k;
                        }
                        else
                        {
                            ShowMessage(FindResource("所有位置已替换完毕").ToString(), true);
                            NextStartIndex = 0;
                        }
                    }
                    //获取总个数
                    TxtFindAmount.Text = "还有" + Regex.Matches(TxtFileContent.Text, TxtFind.Text).Count.ToString() + "处";
                }
                else
                {
                    ShowMessage(FindResource("没有找到该内容").ToString(), true);
                    TxtFindAmount.Text = string.Format(FindResource("共计？处").ToString(), 0);
                    NextStartIndex = 0;
                }
            }
            else
            {
                ShowMessage(FindResource("请输入要查找的内容").ToString(), true);
                TxtFindAmount.Text = string.Format(FindResource("共计？处").ToString(), 0);
                NextStartIndex = 0;
            }
        }
        private void BtnReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            //检测是否输入查找项
            if (TxtFind.Text != null && TxtFind.Text != "")
            {
                //检测是否存在
                if (TxtFileContent.Text.Contains(TxtFind.Text))
                {
                    int i = Regex.Matches(TxtFileContent.Text, TxtFind.Text).Count;
                    ReplaceText = TxtReplace.Text;
                    CurrentFindText = TxtFind.Text;
                    //移除、插入、高亮
                    TxtFileContent.Focus();
                    TxtFileContent.Text = TxtFileContent.Text.Replace(CurrentFindText, ReplaceText);
                    ShowMessage(FindResource("全部内容已替换完毕").ToString() + string.Format(FindResource("共计？处").ToString(), i), true);
                    TxtFindAmount.Text = string.Format(FindResource("共计？处").ToString(), 0);
                }
                else
                {
                    ShowMessage(FindResource("没有找到该内容").ToString(), true);
                    TxtFindAmount.Text = string.Format(FindResource("共计？处").ToString(), 0);
                }
            }
            else
            {
                ShowMessage(FindResource("请输入要查找的内容").ToString(), true);
                TxtFindAmount.Text = string.Format(FindResource("共计？处").ToString(), 0);
            }
        }
        ///设置
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
                string content = FileHelper.GetContent(cbi.Tag.ToString());
                if (string.IsNullOrEmpty(content))
                {
                    CbbThemes.Items.Remove(cbi);
                    //设为默认主题
                    Settings.Default.Theme = 0;
                }
                else
                {
                    ColorHelper.SetColors(Resources, content);
                }
                //立即刷新按钮样式
                SetMenuTool(CurrentMenuTool);
            }
        }

        private void CbbFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbbFonts.SelectedItem is ComboBoxItem cbi)
            {
                TxtFileContent.FontFamily = cbi.FontFamily;
                Settings.Default.FontFamily = cbi.FontFamily.Source;
            }
        }
        private void SldFontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RefreshFontSize();
        }
        private void CcbAutoIndentation_Checked(object sender, RoutedEventArgs e)
        {
            RefreshSldAutoIndentations();
        }
        private void CcbAutoIndentation_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshSldAutoIndentations();
        }
        private void SldAutoIndentations_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TxtAutoIndentations.Text = Settings.Default.AutoIndentations.ToString();
        }
        private void CcbAutoSaveEveryMinutes_Checked(object sender, RoutedEventArgs e)
        {
            RefreshSldAutoSaveMinute();
        }
        private void CcbAutoSaveEveryMinutes_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshSldAutoSaveMinute();
        }
        private void SldAutoSaveMinute_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TxtAutoSaveMinute.Text = Settings.Default.AutoSaveMinute.ToString();
        }
        private void CcbAutoBackup_Checked(object sender, RoutedEventArgs e)
        {
            RefreshSldAutoBackupMinute();
        }
        private void CcbAutoBackup_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshSldAutoBackupMinute();
        }
        private void SldAutoBackupMinute_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TxtAutoBackupTime.Text = Settings.Default.AutoBackupMinute.ToString();
        }

        //工作区
        private void TxtFileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentEssay != null || CurrentChapter != null)
            {

            }
        }
        private void TxtFileName_GotFocus(object sender, RoutedEventArgs e)
        {
            ShowMessage(FindResource("在此处重命名").ToString());
        }
        private void TxtFileName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (TxtFileName.Text != null && TxtFileName.Text != "")
                {
                    //检测格式是否正确
                    if (CheckIsRightName(TxtFileName.Text))
                    {
                        if (CurrentEssay != null)
                        {
                            //删除原文件
                            File.Delete(CurrentEssay.Path);
                            //获取新名字
                            string name = TxtFileName.Text + ".txt";
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
                            ReloadTvwBook();

                            //显示消息
                            ShowMessage(FindResource("文章重命名成功").ToString());
                        }
                        else if (CurrentChapter != null)
                        {
                            //获取旧卷册路径
                            string _old = CurrentChapter.Path;
                            //更新卷册名称和路径
                            string name = TxtFileName.Text;
                            CurrentChapter = new FileOrFolderInfo(Path.GetDirectoryName(CurrentChapter.Path) + @"\" + name);
                            //ShowMessage(System.IO.Path.GetDirectoryName(_CurrentChapter.Name));
                            Directory.CreateDirectory(CurrentChapter.Path);
                            if (_old != CurrentChapter.Path)
                            {
                                //拷贝文件
                                CopyDirectory(_old, CurrentChapter.Path);
                                //删除旧目录
                                Directory.Delete(_old, true);

                                //重新导入目录
                                ReloadTvwBook();

                                //显示消息
                                ShowMessage(FindResource("卷册重命名成功").ToString());
                            }
                        }
                    }
                    else
                        ShowMessage(FindResource("重命名中不能含有以下字符").ToString() + " \\ | / < > \" ? * :");
                }
                else
                    ShowMessage(FindResource("重命名不能为空").ToString());
            }
        }
        private void TxtFileContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentEssay != null)
            {
                //过滤无效字符
                MatchCollection space = Regex.Matches(TxtFileContent.Text, @"\s");
                //MatchCollection newLine = Regex.Matches(FileContent.Text, @"\r");
                int w = TxtFileContent.Text.Length - space.Count;
                //刷新字数
                TxtWordCount.Text = FindResource("字数").ToString() + "：" + w;
                TxtWordCount.ToolTip = FindResource("全书字数").ToString() + "：" + GetBookWords(CurrentBook.Path, CurrentEssay.Path);
                //显示消息
                ShowMessage(FindResource("正在编辑").ToString());
                IsSaved = false;
                if (w > 100000)
                {
                    ShowMessage(FindResource("控制字数").ToString(), true);
                }
                RefreshTitle();
            }
        }
        private void TxtFileContent_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //NodePath.Text = e.Text;
            //自动补全
            if (Settings.Default.IsAutoCompletion)
            {
                //记录光标位置
                int position = TxtFileContent.SelectionStart;
                //记录滚动条位置
                double s = TxtFileContent.VerticalOffset;
                if (e.Text.Equals("("))
                { TxtFileContent.Text = TxtFileContent.Text.Insert(TxtFileContent.SelectionStart, ")"); TxtFileContent.Select(position, 0); }
                else if (e.Text.Equals("（"))
                { TxtFileContent.Text = TxtFileContent.Text.Insert(TxtFileContent.SelectionStart, "）"); TxtFileContent.Select(position, 0); }
                else if (e.Text.Equals("["))
                { TxtFileContent.Text = TxtFileContent.Text.Insert(TxtFileContent.SelectionStart, "]"); TxtFileContent.Select(position, 0); }
                else if (e.Text.Equals("【"))
                { TxtFileContent.Text = TxtFileContent.Text.Insert(TxtFileContent.SelectionStart, "】"); TxtFileContent.Select(position, 0); }
                else if (e.Text.Equals("{"))
                { TxtFileContent.Text = TxtFileContent.Text.Insert(TxtFileContent.SelectionStart, "}"); TxtFileContent.Select(position, 0); }
                else if (e.Text.Equals("'"))
                { TxtFileContent.Text = TxtFileContent.Text.Insert(TxtFileContent.SelectionStart, "'"); TxtFileContent.Select(position, 0); }
                else if (e.Text.Equals("‘"))
                { TxtFileContent.Text = TxtFileContent.Text.Insert(TxtFileContent.SelectionStart, "’"); TxtFileContent.Select(position, 0); }
                else if (e.Text.Equals("’"))
                { TxtFileContent.Text = TxtFileContent.Text.Insert(TxtFileContent.SelectionStart - 1, "‘"); TxtFileContent.Select(position, 0); }
                else if (e.Text.Equals("\""))
                { TxtFileContent.Text = TxtFileContent.Text.Insert(TxtFileContent.SelectionStart, "\""); TxtFileContent.Select(position, 0); }
                else if (e.Text.Equals("“"))
                { TxtFileContent.Text = TxtFileContent.Text.Insert(TxtFileContent.SelectionStart, "”"); TxtFileContent.Select(position, 0); }
                else if (e.Text.Equals("”"))
                { TxtFileContent.Text = TxtFileContent.Text.Insert(TxtFileContent.SelectionStart - 1, "“"); TxtFileContent.Select(position, 0); }
                else if (e.Text.Equals("<"))
                { TxtFileContent.Text = TxtFileContent.Text.Insert(TxtFileContent.SelectionStart, ">"); TxtFileContent.Select(position, 0); }
                else if (e.Text.Equals("《"))
                { TxtFileContent.Text = TxtFileContent.Text.Insert(TxtFileContent.SelectionStart, "》"); TxtFileContent.Select(position, 0); }
                //FileContent.Select(position, 0);
                TxtFileContent.ScrollToVerticalOffset(s);
            }
        }
        private void TxtFileContent_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Settings.Default.IsAutoIndentation)
                {
                    Indentation();
                }
            }
        }
        private void TxtFileContent_SelectionChanged(object sender, RoutedEventArgs e)
        {
            GetRowAndColumn();
            RefreshBtnsState();
        }
        private void TxtFileContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GetRowAndColumn();
        }
        private void TxtFileContent_LostFocus(object sender, RoutedEventArgs e)
        {
            TxtRowAndColumn.Text = FindResource("行").ToString() + "：" + 0 + "    " + FindResource("列").ToString() + "：" + 0; ;
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
