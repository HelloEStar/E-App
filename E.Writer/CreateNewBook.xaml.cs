using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

namespace E.Writer
{
    /// <summary>
    /// CreateNewBook.xaml 的交互逻辑
    /// </summary>
    public partial class CreateNewBook : Window
    {
        //获取父窗口
        public MainWindow Ow { get; set; }
        //构造器
        public CreateNewBook()
        {
            InitializeComponent();
        }

        //创建窗口时
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //获取上级目录
            if (Properties.Settings.Default._lastBook == Properties.User.Default.BooksDir)
            {
                FatherPath.Text = Properties.User.Default.BooksDir;
            }
            else
            {
                FatherPath.Text = Path.GetDirectoryName(Properties.Settings.Default._lastBook);
            }
            //检测
            if (FatherPath.Text == null || FatherPath.Text == "")
            {
                BookPath.Text = "请设置上级目录";
            }
            else
            {
                BookName.Text = "未命名书籍";
                BookName.Select(5, 0);
                if ( Properties.User.Default.isAutoBrackets)
                {
                    BookPath.Text = FatherPath.Text + @"\" + "《" + BookName.Text + "》";
                }
                else
                {
                    BookPath.Text = FatherPath.Text + @"\" + BookName.Text;
                }
                BookPath.ToolTip = BookPath.Text;
            }
            //设置光标
            BookName.Focus();
        }
        //点击浏览
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                SelectedPath = FatherPath.Text,
                Description = "请选择此书籍的存放位置："
            };
            //按下确定选择的按钮，获取根目录文件夹路径
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FatherPath.Text = fbd.SelectedPath;
                //检测
                if (BookName.Text != null && BookName.Text != "")
                {
                    if (Properties.User.Default.isAutoBrackets)
                    {
                        BookPath.Text = FatherPath.Text + @"\" + "《" + BookName.Text + "》";
                    }
                    else
                    {
                        BookPath.Text = FatherPath.Text + @"\" + BookName.Text;
                    }
                    BookPath.ToolTip = BookPath.Text;
                }
            }
        }
        //当BookName被改变时
        private void BookName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BookName.Text == null || BookName.Text == "")
            {
                BookPath.Text = "请输入书籍名";
            }
            else
            {
                if (Properties.User.Default.isAutoBrackets)
                {
                    BookPath.Text = FatherPath.Text + @"\" + "《" + BookName.Text + "》";
                }
                else
                {
                    BookPath.Text = FatherPath.Text + @"\" + BookName.Text;
                }
            }
            BookPath.ToolTip = BookPath.Text;
        }
        //键盘按键UP事件
        private void CreateNew_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CreateBOOK();
            }
        }
        //点击创建书籍
        private void CreateBook_Click(object sender, RoutedEventArgs e)
        {
            CreateBOOK();
        }

        //创建书籍
        private void CreateBOOK()
        {
            //检测
            if (FatherPath.Text == null || FatherPath.Text == "")
            {
                MessageBox.Show("请设置上级目录");
            }
            else
            {
                //若输入为空
                if (BookName.Text == null || BookName.Text == "")
                {
                    MessageBox.Show("请输入书籍名称");
                }
                //若输入中含有 \ | / < > " ? * :
                else if (!MainWindow.IsRightName(BookName.Text))
                {
                    MessageBox.Show("名称中不能含有以下字符 \\ | / < > \" ? * : ");
                }
                else
                {
                    //若书籍存在
                    if (Directory.Exists(BookPath.Text))
                    {
                        MessageBox.Show("同名书籍已存在，请换个名字");
                    }
                    //若书籍不存在
                    else
                    {
                        //保存当前文件再创建书籍
                        if (Ow.SelectedEssay != null && Properties.User.Default.isAutoSaveWhenSwitch == true)
                        { Ow.SaveFile(); }
                        //创建书籍文件夹
                        Directory.CreateDirectory(BookPath.Text);
                        //是否自动书名号
                        if (Properties.User.Default.isAutoBrackets)
                        {
                            Ow.SelectedBook = "《" + BookName.Text + "》";
                        }
                        else
                        {
                            Ow.SelectedBook = BookName.Text;
                        }
                        //记录
                        Ow.SelectedBookPath = BookPath.Text;
                        Properties.Settings.Default._lastBook = BookPath.Text;
                        Properties.Settings.Default.Save();
                        //刷新根目录
                        //Properties.User.Default.BooksDir = FatherPath.Text;
                        //打开
                        Ow.OpenBook(Ow.SelectedBookPath);
                        //提示消息
                        Ow.HelpMessage.Content = "已创建并打开书籍 " + Ow.SelectedBook;
                        //关闭窗口
                        Close();
                    }
                }
            }
        }
    }
}
