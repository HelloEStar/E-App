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
    /// CreateNewEssay.xaml 的交互逻辑
    /// </summary>
    public partial class CreateNewEssay : Window
    {
        //获取父窗口
        public MainWindow Ow { get; set; }

        public CreateNewEssay()
        {
            InitializeComponent();
        }

        //创建窗口时
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //获取上级目录
            if (Ow.SelectedChapterPath == null)
            {
                FatherPath.Text = Ow.SelectedBookPath;
            }
            else
            {
                FatherPath.Text = Ow.SelectedChapterPath;
            }
            //检测
            if (FatherPath.Text == null || FatherPath.Text == "")
            {
                EssayPath.Text = "请设置上级目录。";
            }
            else
            {
                EssayName.Text = "未命名文章";
                EssayName.Select(5, 0);
                EssayPath.Text = FatherPath.Text + @"\" + EssayName.Text + ".txt";
                EssayPath.ToolTip = EssayPath.Text;
            }
            //设置光标
            EssayName.Focus();
        }
        //当EssayName被改变时
        private void EssayName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EssayName.Text == "" || EssayName.Text == null)
            {
                EssayPath.Text = "请输入文章名";
            }
            else
            {
                EssayPath.Text = FatherPath.Text + @"\" + EssayName.Text + ".txt";
            }
            EssayPath.ToolTip = EssayPath.Text;
        }
        //点击浏览
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                SelectedPath = FatherPath.Text,
                Description = "请选择此文章的存放位置："
            };
            //按下确定选择的按钮，获取根目录文件夹路径
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (fbd.SelectedPath.Contains(Ow.SelectedBookPath))
                {
                    FatherPath.Text = fbd.SelectedPath;
                    //检测
                    if (EssayName.Text != null && EssayName.Text != "")
                    {
                        EssayPath.Text = FatherPath.Text + @"\" + EssayName.Text + ".txt";
                        EssayPath.ToolTip = EssayPath.Text;
                    }
                }
                else
                {
                    MessageBox.Show("不可以将文章创建在书籍外");
                }
            }
        }
        //键盘按键UP事件
        private void CreateNew_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CreateESSAY();
            }
        }
        //点击创建文章
        private void CreateEssay_Click(object sender, RoutedEventArgs e)
        {
            CreateESSAY();
        }

        //创建文章
        private void CreateESSAY()
        {
            //若输入为空
            if (EssayName.Text == "")
            {
                MessageBox.Show("请输入文章名");
            }
            //若输入中含有 \ | / < > " ? * :
            else if (!MainWindow.CheckIsRightName(EssayName.Text))
            {
                MessageBox.Show("名称中不能含有以下字符 \\ | / < > \" ? * : ");
            }
            else
            {
                //若文件存在
                if (File.Exists(EssayPath.Text))
                {
                    MessageBox.Show("同名文章已存在，请换个名字");
                }
                //若文件不存在
                else
                {
                    //创建前保存文章
                    if (Ow.SelectedEssay != null)
                    {
                        Ow.SaveFile();
                    }
                    //创建文章
                    File.Create(EssayPath.Text).Close();
                    //刷新当前文章
                    Ow.SelectedEssay = EssayName.Text + ".txt";
                    Ow.SelectedEssayPath = EssayPath.Text;
                    //提示信息
                    Ow.HelpMessage.Content = "已创建文章 " + Ow.SelectedEssay;
                    //打开文章
                    Ow.OpenEssay(EssayPath.Text);
                    //改变主窗口控件状态
                    Ow.RefreshBtnsState();
                    //改变标题
                    Ow.RefreshTitle();

                    //
                    //创建节点
                    FileNode newFileNode = new FileNode(EssayName.Text + ".txt", EssayPath.Text, true, false, null);
                    FileNode fatherNote = Ow.FindNote(FatherPath.Text);
                    if (fatherNote == null)
                    {
                        Ow.FilesTree.Items.Add(newFileNode);
                    }
                    else
                    {
                        fatherNote.IsExpanded = true;
                        fatherNote.Items.Add(newFileNode);
                    }
                    //选择节点
                    Ow.SelectedNode = newFileNode;
                    //打开节点
                    Ow.OpenedNode = newFileNode;
                    //刷新目录
                    Ow.FilesTree.Items.Refresh();

                    //提示信息
                    Ow.HelpMessage.Content = "已创建并打开文章 " + Ow.SelectedEssay;
                    //关闭窗口
                    Close();
                }
            }
        }
    }
}
