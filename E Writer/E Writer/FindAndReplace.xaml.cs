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
using System.Windows.Shapes;

namespace E_Writer
{
    /// <summary>
    /// FindAndReplace.xaml 的交互逻辑
    /// </summary>
    public partial class FindAndReplace : Window
    {
        //获取父窗口
        public MainWindow Ow;
        string findText;
        string replaceText;
        int nextStart;

        public FindAndReplace()
        {
            InitializeComponent();
        }

        //载入窗口
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Left = Ow.Left + Ow.Width - Width;
            Top = Ow.Top + Ow.Height - Height;
            Topmost = true;
            TextFind.Focus();
        }

        //查找下一个
        private void BtnFindNext_Click(object sender, RoutedEventArgs e)
        {
            //检测是否输入
            if (TextFind.Text != null && TextFind.Text != "")
            {
                //检测是否存在
                if (Ow.FileContent.Text.Contains(TextFind.Text))
                {
                    //获取总个数
                    FindAmount.Text = "共计" + Regex.Matches(Ow.FileContent.Text, TextFind.Text).Count.ToString() + "处";
                    int j = TextFind.Text.Length;
                    //检测是否同个内容第一次按下
                    if (findText != TextFind.Text)
                    {
                        findText = TextFind.Text;
                        //获取这个的索引
                        nextStart = Ow.FileContent.Text.IndexOf(findText, 0);
                        //高亮
                        Ow.FileContent.Focus();
                        Ow.FileContent.Select(nextStart, j);
                        //记录下一个开始寻找的地方
                        nextStart = nextStart + j;
                    }
                    else
                    {
                        //获取这个的索引
                        nextStart = Ow.FileContent.Text.IndexOf(findText, nextStart);
                        //检测是否存在
                        if (nextStart >= 0)
                        {
                            //高亮
                            Ow.FileContent.Focus();
                            Ow.FileContent.Select(nextStart, j);
                            //记录下一个开始寻找的地方
                            nextStart = nextStart + j;
                        }
                        else
                        {
                            MessageBox.Show("所有位置已查找完毕，再按一次将从头查找");
                            nextStart = 0;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("没有找到该内容");
                    FindAmount.Text = "共计0处";
                    nextStart = 0;
                }
            }
            else
            {
                MessageBox.Show("请输入要查找的内容");
                FindAmount.Text = "共计0处";
                nextStart = 0;
            }
        }

        //替换下一个
        private void BtnReplaceNext_Click(object sender, RoutedEventArgs e)
        {
            //检测是否输入查找项
            if (TextFind.Text != null && TextFind.Text != "")
            { 
                //检测是否存在
                if (Ow.FileContent.Text.Contains(TextFind.Text))
                {
                    int j = TextFind.Text.Length;
                    int k = TextReplace.Text.Length;
                    replaceText = TextReplace.Text;
                    //检测是否同个内容第一次按下
                    if (findText != TextFind.Text)
                    {
                        findText = TextFind.Text;
                        //获取这个的索引
                        nextStart = Ow.FileContent.Text.IndexOf(findText, 0);
                        //移除、插入、高亮
                        Ow.FileContent.Focus();
                        Ow.FileContent.Text = Ow.FileContent.Text.Remove(nextStart, j);
                        Ow.FileContent.Text = Ow.FileContent.Text.Insert(nextStart, replaceText);
                        Ow.FileContent.Select(nextStart, k);
                        //记录下一个开始寻找的地方
                        nextStart = nextStart + k;
                    }
                    else
                    {
                        //获取这个的索引
                        nextStart = Ow.FileContent.Text.IndexOf(findText, nextStart);
                        //检测是否存在
                        if (nextStart >= 0)
                        {
                            //移除、插入、高亮
                            Ow.FileContent.Focus();
                            Ow.FileContent.Text = Ow.FileContent.Text.Remove(nextStart, j);
                            Ow.FileContent.Text = Ow.FileContent.Text.Insert(nextStart, replaceText);
                            Ow.FileContent.Select(nextStart, k);
                            //记录下一个开始寻找的地方
                            nextStart = nextStart + k;
                        }
                        else
                        {
                            MessageBox.Show("所有位置已替换完毕");
                            nextStart = 0;
                        }
                    }
                    //获取总个数
                    FindAmount.Text = "还有" + Regex.Matches(Ow.FileContent.Text, TextFind.Text).Count.ToString() + "处";
                }
                else
                {
                    MessageBox.Show("没有找到该内容");
                    FindAmount.Text = "共计0处";
                    nextStart = 0;
                }
            }
            else
            {
                MessageBox.Show("请输入要查找的内容");
                FindAmount.Text = "共计0处";
                nextStart = 0;
            }
        }

        //替换全部
        private void BtnReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            //检测是否输入查找项
            if (TextFind.Text != null && TextFind.Text != "")
            {
                //检测是否存在
                if (Ow.FileContent.Text.Contains(TextFind.Text))
                {
                    int i = Regex.Matches(Ow.FileContent.Text, TextFind.Text).Count;
                    int j = TextFind.Text.Length;
                    int k = TextReplace.Text.Length;
                    replaceText = TextReplace.Text;
                    findText = TextFind.Text;
                    //移除、插入、高亮
                    Ow.FileContent.Focus();
                    Ow.FileContent.Text = Ow.FileContent.Text.Replace(findText, replaceText);
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

    }
}
