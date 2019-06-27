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
    /// CreateNewChapter.xaml 的交互逻辑
    /// </summary>
    public partial class CreateNewChapter : Window
    {
        //获取父窗口
        public MainWindow Ow { get; set; }

        public CreateNewChapter()
        {
            InitializeComponent();
        }

        //创建窗口时
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //获取上级目录，默认创建一级卷册
            if (Ow.SelectedChapterPath == null)
            { FatherPath.Text = Ow.SelectedBookPath; }
            else
            {FatherPath.Text = Ow.SelectedChapterPath; }
            //检测
            if (FatherPath.Text == null || FatherPath.Text == "")
            {
                ChapterPath.Text = "请设置上级目录。";
            }
            else
            {
                ChapterName.Text = "未命名卷册";
                ChapterName.Select(5, 0);
                ChapterPath.Text = FatherPath.Text + @"\" + ChapterName.Text;
                ChapterPath.ToolTip = ChapterPath.Text;
            }
            //设置光标
            ChapterName.Focus();
        }
        //点击浏览
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                SelectedPath = FatherPath.Text,
                Description = "请选择此卷册的存放位置："
            };
            //按下确定选择的按钮，获取根目录文件夹路径
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (fbd.SelectedPath.Contains(Ow.SelectedBookPath))
                {
                    FatherPath.Text = fbd.SelectedPath;
                    //检测
                    if (ChapterName.Text != null && ChapterName.Text != "")
                    {
                        ChapterPath.Text = FatherPath.Text + @"\" + ChapterName.Text;
                        ChapterPath.ToolTip = ChapterPath.Text;
                    }
                }
                else
                {
                    MessageBox.Show("不可以将卷册创建在书籍外");
                }
            }
        } 
        //当ChapterName被改变时
        private void ChapterName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ChapterName.Text == null || ChapterName.Text == "")
            {
                ChapterPath.Text = "请输入卷册名";
            }
            else
            {
                ChapterPath.Text = FatherPath.Text + @"\" + ChapterName.Text;
            }
            ChapterPath.ToolTip = ChapterPath.Text;
        }
        //键盘按键UP事件
        private void CreateNew_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CreateCHAPTER();
            }
        }
        //点击创建卷册
        private void CreateChapter_Click(object sender, RoutedEventArgs e)
        {
            CreateCHAPTER();
        }

        //创建卷册
        private void CreateCHAPTER()
        {
            //若输入为空
            if (ChapterName.Text == "" || ChapterName.Text == null)
            {
                MessageBox.Show("请输入卷册名");
            }
            //若输入中含有 \ | / < > " ? * :
            else if (!MainWindow.CheckIsRightName(ChapterName.Text))
            {
                MessageBox.Show("名称中不能含有以下字符 \\ | / < > \" ? * : ");
            }
            else
            {
                //若卷册存在
                if (Directory.Exists(ChapterPath.Text))
                {
                    MessageBox.Show("同名卷册已存在，请换个名字");
                }
                //若卷册不存在
                else
                {
                    //创建前保存文章
                    if (Ow.SelectedEssay != null)
                    {
                        Ow.SaveFile();
                    }
                    //创建卷册
                    Directory.CreateDirectory(ChapterPath.Text);
                    //打开
                    Ow.OpenChapter(ChapterPath.Text);
                    //改变主窗口控件状态
                    Ow.RefreshBtnsState();
                    //改变标题
                    Ow.RefreshTitle();

                    //
                    string tmp = ChapterPath.Text;
                    string name = tmp.Replace(FatherPath.Text + @"\", "");
                    //创建节点
                    FileNode newfolderNode = new FileNode(name, ChapterPath.Text, false, false, null);
                    FileNode fatherNote = Ow.FindNote(FatherPath.Text);
                    if (fatherNote == null)
                    {
                        Ow.FilesTree.Items.Add(newfolderNode);
                    }
                    else
                    {
                        fatherNote.IsExpanded = true;
                        fatherNote.Items.Add(newfolderNode);
                    }
                    //选择节点
                    Ow.SelectedNode = newfolderNode;
                    //打开节点
                    Ow.OpenedNode = newfolderNode;
                    //刷新目录
                    Ow.FilesTree.Items.Refresh();

                    //提示信息
                    Ow.HelpMessage.Content = "已创建并打开卷册 " + ChapterName.Text;
                    //关闭窗口
                    Close();

                }
            }
        }
        
    }
}
