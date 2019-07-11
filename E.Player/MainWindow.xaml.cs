using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms.Integration;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Net;
using System.Diagnostics;
using System.Reflection;
using System.Drawing.Imaging;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.MessageBox;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using DragEventArgs = System.Windows.DragEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using E.Utility;

namespace E.Player
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
        /// 选中的视频的路径
        /// </summary>
        private string SelectedVideoPath { get; set; }
        /// <summary>
        /// 正在播放的视频的路径
        /// </summary>
        private string PlayingVideoPath { get; set; }
        /// <summary>
        /// 是否正在播放
        /// </summary>
        private bool IsPlay { get; set; } = false;
        /// <summary>
        /// 是否全屏
        /// </summary>
        private bool IsFullScreen { get; set; } = false; 
        /// <summary>
        /// 列表中视频数量
        /// </summary>
        private int VideoCount { get; set; } = 0;
        /// <summary>
        /// 循环的开始
        /// </summary>
        private TimeSpan StartPosition { get; set; } = TimeSpan.Zero;
        /// <summary>
        /// 循环的结束
        /// </summary>
        private TimeSpan EndPosition { get; set; } = TimeSpan.Zero;
        /// <summary>
        /// 是否有循环开始点
        /// </summary>
        private bool HasStartPosition { get; set; } = false;
        /// <summary>
        /// 是否有循环结束点
        /// </summary>
        private bool HasEndPosition { get; set; } = false;

        private List<ItemInfo> LanguageItems { get; set; } = new List<ItemInfo>();

        //计时器
        private DispatcherTimer timer1;
        private DispatcherTimer timer2;
        private DispatcherTimer timer3;
        #endregion

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
            string infoPage = "http://estar.zone/introduction/e-player/";
            string downloadPage = "http://estar.zone/introduction/e-player/";
            string gitHubPage = "https://github.com/HelloEStar/E.App";
            string qqGroupLink = "http://jq.qq.com/?_wv=1027&k=5TQxcvR";
            string qqGroupNumber = "279807070";
            string bitCoinAddress = "19LHHVQzWJo8DemsanJhSZ4VNRtknyzR1q";
            AppInfo = new AppInfo(product.Product, description.Description, company.Company, copyright.Copyright, new Version(Application.ProductVersion), updateNote,
                                  homePage, infoPage, downloadPage, gitHubPage, qqGroupLink, qqGroupNumber, bitCoinAddress);
        }
        /// <summary>
        /// 载入语言
        /// </summary>
        /// <param name="lang">语言简拼</param>
        private void LoadLanguage(string lang)
        {
            ResourceDictionary langRd = null;
            try
            {
                //根据名字载入语言文件
                langRd = System.Windows.Application.LoadComponent(new Uri(@"语言\" + lang + ".xaml", UriKind.Relative)) as ResourceDictionary;
            }
            catch (Exception e2)
            { System.Windows.MessageBox.Show(e2.Message); }
            if (langRd != null)
            {
                //主窗口更改语言
                if (Resources.MergedDictionaries.Count > 0)
                { Resources.MergedDictionaries.Clear(); }
                Resources.MergedDictionaries.Add(langRd);
            }
        }
        /// <summary>
        /// 载入媒体记录
        /// </summary>
        private void LoadVideos()
        {
            //读取一个字符串，并加入播放列表
            if (Properties.User.Default._medias != null && Properties.User.Default._medias != "")
            {
                string[] _myB = Regex.Split(Properties.User.Default._medias, "///");
                foreach (var b in _myB)
                {
                    CreateListBoxItem(b, false);
                }
            }
        }
        /// <summary>
        /// 载入偏好设置
        /// </summary>
        private void LoadSettings()
        {
            //是否在切换视频时保持变换
            KeepTrans.IsChecked = Properties.User.Default.isKeepTrans;
            //是否在退出时保留播放列表
            SavePlaylist.IsChecked = Properties.User.Default.isSavePlaylist;
            //快进快退时间
            JumpTime.Text = Properties.User.Default.jumpTime.ToString();

            //创建语言选项
            GetLanguage();
            SetLanguage(Properties.User.Default.language);
        }

        //打开

        //创建
        /// <summary>
        /// 创建播放列表媒体元素
        /// </summary>
        /// <param name="path">媒体路径</param>
        /// <param name="isPlay">是否直接播放</param>
        private void CreateListBoxItem(string path, bool isPlay)
        {
            if (path != "" && path != null)
            {
                double tmpWidth;
                if (PlayListBox.ActualWidth - 4 <= 0)
                {
                    tmpWidth = 100;
                }
                else
                {
                    tmpWidth = PlayListBox.ActualWidth - 4;
                }
                //
                ListBoxItem PlayListBoxItem = new ListBoxItem
                {
                    Name = "VideoItem",
                    Tag = path,
                    ToolTip = path,
                    Content = GetVideoName(path),
                    Width = tmpWidth,
                    Height = 25,
                    Margin = new Thickness(0, 0, 0, 0),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    Background = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)),
                };
                //添加鼠标事件
                PlayListBoxItem.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Item_Play_MouseDown), true);
                PlayListBoxItem.AddHandler(MouseRightButtonUpEvent, new MouseButtonEventHandler(Item_Delete_MouseDown), true);
                //PlayListBoxItem.AddHandler(KeyUpEvent, new System.Windows.Input.KeyEventHandler(Item_Delete_KeyUp), true);

                //添加Item时检测是否有重复视频
                bool isThere = false;
                //如果播放列表是空的
                if (VideoCount == 0)
                {
                    //增加第一个Item进PlayListBox
                    PlayListBox.Items.Add(PlayListBoxItem);
                    VideoCount = 1;
                    PlayListBox.ToolTip = FindResource("媒体总数") + "：" + VideoCount;
                    //选中此Item
                    PlayListBox.SelectedIndex = 0;
                    if (isPlay == true)
                    {
                        //播放此视频
                        PlayNewVideo(path);
                    }
                }
                //如果播放列表不是空的
                else
                {
                    ListBoxItem tempItem = new ListBoxItem();
                    //获取正在添加的Item视频路径
                    for (int i = 0; i < VideoCount; i++)
                    {
                        //获得所有item的视频路径
                        tempItem = PlayListBox.Items.GetItemAt(i) as ListBoxItem;
                        //对比,若有重复
                        if (path == tempItem.Tag.ToString())
                        {
                            isThere = true;
                            break;
                        }
                        //对比,若无重复
                        else
                        {
                            isThere = false;
                        }
                    }
                    //如果没有重复
                    if (isThere == false)
                    {
                        //增加第一个Item进PlayListBox
                        PlayListBox.Items.Add(PlayListBoxItem);
                        VideoCount = PlayListBox.Items.Count;
                        PlayListBox.ToolTip = FindResource("媒体总数") + "：" + VideoCount;
                        PlayListBox.SelectedItem = PlayListBoxItem;
                        //是否直接播放新增加的视频
                        if (isPlay == true)
                        {
                            //播放此视频
                            PlayNewVideo(path);
                        }
                    }
                    //如果有重复
                    else
                    {
                        ShowMessage("视频重复", true);
                    }
                }
            }
        }
        /// <summary>
        /// 创建计时器
        /// </summary>
        private void CreateTimer()
        {
            Loaded += new RoutedEventHandler(Timer1_Tick);
            Loaded += new RoutedEventHandler(Timer2_Tick);
            Loaded += new RoutedEventHandler(Timer3_Tick);
            //设置计时器1,每秒触发一次
            timer1 = new DispatcherTimer
            { Interval = TimeSpan.FromSeconds(1) };
            timer1.Tick += new EventHandler(Timer1_Tick);
            timer1.Start();
            //设置计时器2,每0.01秒触发一次
            timer2 = new DispatcherTimer
            { Interval = TimeSpan.FromSeconds(0.02) };
            timer2.Tick += new EventHandler(Timer2_Tick);
            timer2.Start();
            //设置计时器3,每0.01秒触发一次
            timer3 = new DispatcherTimer
            { Interval = TimeSpan.FromSeconds(0.01) };
            timer3.Tick += new EventHandler(Timer3_Tick);
            //timer3.Start();
        }
        /// <summary>
        /// 创建mp3封面
        /// </summary>
        private void CreateMP3Cover(string path)
        {
            if (System.IO.Path.GetExtension(path) == ".mp3")
            {
                Cover.Source = null;
                Cover.IsEnabled = true;
                //Cover.Visibility = Visibility.Visible;
                //调用方法示例
                if (CoverofMP3(path) != null)
                {
                    if (CoverofMP3(path).Count > 0)
                    {
                        Cover.Source = CoverofMP3(path)[0];//在控件中显示出来
                                                           //if (Imag.Source.Height>Imag.Source.Width)
                                                           //    Imag.Height = 113;
                                                           //else
                                                           //    Imag.Width = 107;

                    }
                }
            }
            else
            {
                Cover.Source = null;
                Cover.IsEnabled = false;
                //Cover.Visibility = Visibility.Hidden;
            }
        }

        //删除
        /// <summary>
        /// 删除媒体
        /// </summary>
        private void Delete()
        {
            //获取当前选择的Item在PlayListBox的索引
            ListBoxItem tempItem;
            tempItem = PlayListBox.SelectedItem as ListBoxItem;
            if (tempItem != null)
            {
                if (PlayingVideoPath == tempItem.Tag.ToString())
                {
                    Media.Stop();
                    Media.Source = null;
                    PlayingVideoPath = "";
                    Cover.Source = null;
                    Cover.IsEnabled = false;
                    IsPlay = false;
                    //不激活按钮
                    ChangeElementState(0);
                    // = FindResource("媒体路径");
                    //VideoName.ToolTip = FindResource("媒体路径");
                    TimeAll.Content = "00:00:00";
                    BtnPlay.Content = FindResource("播放");

                    //显示消息
                    ShowMessage("删除视频", false);
                }
                int s = PlayListBox.SelectedIndex;
                PlayListBox.Items.Remove(PlayListBox.SelectedItem);
                PlayListBox.SelectedIndex = s;
                if (s == VideoCount - 1)
                {
                    PlayListBox.SelectedIndex = s - 1;
                }
                VideoCount -= 1;
                PlayListBox.ToolTip = FindResource("媒体总数") + "：" + VideoCount;
            }
            else
            {
                //显示消息
                ShowMessage("未选择视频", false);
            }
        }

        //清除
        /// <summary>
        /// 清除循环段
        /// </summary>
        private void ClearLoop()
        {
            if (HasEndPosition == true || HasStartPosition == true)
            {
                StartPosition = TimeSpan.Zero;
                EndPosition = TimeSpan.Zero;
                HasStartPosition = false;
                HasEndPosition = false;

                //滑块高亮区域
                TimeSlider.SelectionStart = 0;
                TimeSlider.SelectionEnd = 0;
                //显示消息
                ShowMessage("已清除循环", false);
            }
        }

        //获取
        /// <summary>
        /// 创建语言选项
        /// </summary>
        private void GetLanguage()
        {
            ItemInfo zh_CN = new ItemInfo() { Name = "中文（默认）", Value = "zh_CN" };
            LanguageItems.Add(zh_CN);
            ItemInfo en_US = new ItemInfo() { Name = "English", Value = "en_US" };
            LanguageItems.Add(en_US);
            //绑定数据，真正的赋值
            CbbLanguages.ItemsSource = LanguageItems;
            CbbLanguages.DisplayMemberPath = "Name";
            CbbLanguages.SelectedValuePath = "Value";
        }
        /// <summary>
        /// 获取媒体名字
        /// </summary>
        /// <param name="path">媒体路径</param>
        /// <returns>媒体名称</returns>
        private string GetVideoName(string path)
        {
            string _videoName = System.IO.Path.GetFileName(path);
            return _videoName;
        }
        /// <summary>
        /// 由地址返回图片，返回类型为System.Drawing.Image
        /// </summary>
        /// <param name="MP3path">MP3全地址</param>
        /// <returns></returns>
        private List<BitmapImage> CoverofMP3(string MP3path)
        {
            try
            {
                List<BitmapImage> list_bitmapimg = new List<BitmapImage>();
                Tags.ID3.ID3Info File = new Tags.ID3.ID3Info(MP3path, true);

                foreach (Tags.ID3.ID3v2Frames.BinaryFrames.AttachedPictureFrame F in File.ID3v2Info.AttachedPictureFrames)
                {
                    System.Drawing.Image imgWinForms = F.Picture; // 此段代码用于将获得的System.Drawing.Image类型转化为
                    BitmapImage bi = new BitmapImage();                     //WPF中Image控件可以接受的内容
                    float scale = imgWinForms.Height / imgWinForms.Width;
                    bi.BeginInit();
                    MemoryStream ms = new MemoryStream();
                    imgWinForms.Save(ms, ImageFormat.Bmp);
                    ms.Seek(0, SeekOrigin.Begin);
                    bi.StreamSource = ms;
                    bi.EndInit();
                    list_bitmapimg.Add(bi);
                }
                return list_bitmapimg;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
                return null;
            }
        }
        /// <summary>
        /// 浏览文件获取路径
        /// </summary>
        /// <returns>媒体路径</returns>
        private string GetFilePath()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Video File(*.avi;*.mp4;*.rmvb;*.mkv;*.wmv;*.wma;*.wav;*.mp3;*.aac;*.flac)|" +
                                    "*.avi;*.mp4;*.rmvb;*.mkv;*.wmv;*.wma;*.wav;*.mp3;*.aac;*.flac|" +
                         "All File(*.*)|" +
                                  "*.*"
            };
            dialog.ShowDialog();
            //获取视频文件路径
            return dialog.FileName;
        }

        //设置
        /// <summary>
        /// 翻转归位
        /// </summary>
        private void ResetFlip()
        {
            Properties.User.Default.isLRFlip = false;
            Properties.User.Default.isUDFlip = false;
            Properties.User.Default.Save();
            ScaleTransform scaleTransform = new ScaleTransform
            {
                ScaleX = 1,
                ScaleY = 1
            };
            Media.LayoutTransform = scaleTransform;
        }
        /// <summary>
        /// 旋转归位
        /// </summary>
        private void ResetRotation()
        {
            Properties.User.Default.rotateTo = 0;
            Properties.User.Default.Save();
            Media.LayoutTransform = new RotateTransform(0);
        }
        /// <summary>
        /// 设置循环段开始
        /// </summary>
        private void SetLoopStart()
        {
            //获取当前位置
            TimeSpan tempPosition = Media.Position;
            if (tempPosition < EndPosition || HasEndPosition == false)
            {
                StartPosition = tempPosition;
                string str = StartPosition.ToString();
                //滑块高亮区域
                TimeSlider.SelectionStart = StartPosition.TotalSeconds;
                HasStartPosition = true;

                //显示消息
                ShowMessage("循环始于", "：" + str.Substring(0, 8), false);
            }
            else
            {
                //显示消息
                ShowMessage("开始点应靠前", false);
            }
        }
        /// <summary>
        /// 设置循环段结束
        /// </summary>
        private void SetLoopEnd()
        {
            //获取当前位置
            TimeSpan tempPosition = Media.Position;
            if (StartPosition < tempPosition)
            {
                EndPosition = tempPosition;
                string str = EndPosition.ToString();
                //滑块高亮区域
                TimeSlider.SelectionEnd = EndPosition.TotalSeconds;
                HasEndPosition = true;

                //显示消息
                ShowMessage("循环终于", "：" + str.Substring(0, 8), false);
            }
            else
            {
                //显示消息
                ShowMessage("结束点应靠后", false);
            }
        }
        /// <summary>
        /// 设置播放模式
        /// </summary>
        /// <param name="mode">模式</param>
        /// <param name="showMessage">是否显示消息</param>
        private void SetPlayMode(int mode)
        {
            Properties.User.Default.playMode = mode;
            Properties.User.Default.Save();
            ShowMessage("" + mode);
        }
        /// <summary>
        /// 设置语言选项
        /// </summary>
        /// <param name="language">语言简拼</param>
        private void SetLanguage(string language)
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

        //执行
        /// <summary>
        /// 播放新媒体
        /// </summary>
        /// <param name="path">媒体路径</param>
        private void PlayNewVideo(string path)
        {
            //若在切换视频时不保留镜像状态
            if (Properties.User.Default.isKeepTrans == false)
            {
                ResetFlip();
                ResetRotation();
            }
            //清空媒体源
            Media.Source = null;
            //若播放音频，显示封面
            CreateMP3Cover(path);
            //获取路径开始播放
            Media.Source = new Uri(path);
            Media.Play();
            Media.ScrubbingEnabled = true;
            IsPlay = true;
            //激活按钮
            ChangeElementState(1);
            BtnPlay.Content = FindResource("暂停");
            PlayingVideoPath = path;

            //帮助消息
            //VideoName.ToolTip = path;
            //VideoName.Content = GetVideoName(path);
            //显示消息
            ShowMessage("开始播放", "：" + GetVideoName(path), false);
        }
        /// <summary>
        /// 播放与暂停操作
        /// </summary>
        private void Play()
        {
            if (IsPlay == false)
            {
                Media.Play();
                IsPlay = true;
                BtnPlay.Content = FindResource("暂停");
                //显示消息
                ShowMessage("开始播放", false);
            }
            else if (IsPlay == true)
            {
                Media.Pause();
                IsPlay = false;
                BtnPlay.Content = FindResource("播放");
                //显示消息
                ShowMessage("暂停播放", false);
            }
        }
        /// <summary>
        /// 重播
        /// </summary>
        private void Replay()
        {
            Media.Position = TimeSpan.Zero;
            Media.Play();
            IsPlay = true;
            BtnPlay.Content = FindResource("暂停");
            //显示消息
            ShowMessage("开始播放", false);
        }
        /// <summary>
        /// 快进
        /// </summary>
        private void Forward()
        {
            if (File.Exists(PlayingVideoPath))
            {
                Media.Position += TimeSpan.FromSeconds(Properties.User.Default.jumpTime);
                if (Media.Position > Media.NaturalDuration.TimeSpan)
                {
                    Media.Position = Media.NaturalDuration.TimeSpan;
                }
                string str = Media.Position.ToString();
                //显示消息
                ShowMessage("当前进度", "：" + str.Substring(0, 8), false);
            }
        }
        /// <summary>
        /// 快退
        /// </summary>
        private void Back()
        {
            if (File.Exists(PlayingVideoPath))
            {
                Media.Position -= TimeSpan.FromSeconds(Properties.User.Default.jumpTime);
                if (Media.Position <= TimeSpan.Zero)
                {
                    Media.Position = TimeSpan.Zero;
                }
                string str = Media.Position.ToString();
                //显示消息
                ShowMessage("当前进度", "：" + str.Substring(0, 8), false);
            }
        }
        /// <summary>
        /// 音量升高
        /// </summary>
        private void VolumeUp()
        {
            Media.Volume = Media.Volume + 0.1;
            double temp = Math.Round(Media.Volume, 2);
            SldVolume.Value = temp;
            SldVolume.ToolTip = temp;
            //显示消息
            ShowMessage("当前音量", "：" + temp, false);
        }
        /// <summary>
        /// 音量降低
        /// </summary>
        private void VolumeDown()
        {
            Media.Volume = Media.Volume - 0.1;
            double temp = Math.Round(Media.Volume, 2);
            SldVolume.Value = temp;
            SldVolume.ToolTip = temp;
            //显示消息
            ShowMessage("当前音量", "：" + temp, false);
        }
        /// <summary>
        /// 速度增加
        /// </summary>
        private void SpeedUp()
        {
            Media.SpeedRatio = Media.SpeedRatio + 0.1;
            //显示消息
            ShowMessage("播放速度", "：" + Media.SpeedRatio, false);
        }
        /// <summary>
        /// 速度减少
        /// </summary>
        private void SpeedDown()
        {
            Media.SpeedRatio = Media.SpeedRatio - 0.1;
            //显示消息
            ShowMessage("播放速度", "：" + Media.SpeedRatio, false);
        }
        /// <summary>
        /// 左右翻转
        /// </summary>
        private void LRFlip()
        {
            if (Properties.User.Default.isLRFlip == false)
            {
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleX = -1;
                if (Properties.User.Default.isUDFlip == true)
                {
                    scaleTransform.ScaleY = -1;
                }
                else
                {
                    scaleTransform.ScaleY = 1;
                }
                Media.LayoutTransform = scaleTransform;
                Properties.User.Default.isLRFlip = true;
                //显示消息
                ShowMessage("已左右翻转", false);
            }
            else if (Properties.User.Default.isLRFlip == true)
            {
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleX = 1;
                if (Properties.User.Default.isUDFlip == true)
                {
                    scaleTransform.ScaleY = -1;
                }
                else
                {
                    scaleTransform.ScaleY = 1;
                }
                Media.LayoutTransform = scaleTransform;
                Properties.User.Default.isLRFlip = false;
                //显示消息
                ShowMessage("已取消左右翻转", false);
            }
        }
        /// <summary>
        /// 上下翻转
        /// </summary>
        private void UDFlip()
        {
            if (Properties.User.Default.isUDFlip == false)
            {
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleY = -1;
                if (Properties.User.Default.isLRFlip == true)
                {
                    scaleTransform.ScaleX = -1;
                }
                else
                {
                    scaleTransform.ScaleX = 1;
                }
                Media.LayoutTransform = scaleTransform;
                Properties.User.Default.isUDFlip = true;
                //显示消息
                ShowMessage("已上下翻转", false);
            }
            else if (Properties.User.Default.isUDFlip == true)
            {
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleY = 1;
                if (Properties.User.Default.isLRFlip == true)
                {
                    scaleTransform.ScaleX = -1;
                }
                else
                {
                    scaleTransform.ScaleX = 1;
                }
                Media.LayoutTransform = scaleTransform;
                Properties.User.Default.isUDFlip = false;
                //显示消息
                ShowMessage("已取消上下翻转", false);
            }
        }
        /// <summary>
        /// 顺时针旋转90度
        /// </summary>
        private void ClockwiseRotation()
        {
            Properties.User.Default.rotateTo += 1;
            if (Properties.User.Default.rotateTo > 3)
            {
                Properties.User.Default.rotateTo = 0;
            }
            Properties.User.Default.Save();
            Media.LayoutTransform = new RotateTransform(90 * Properties.User.Default.rotateTo);
            //显示消息
            ShowMessage("已顺时针旋转90度", false);
        }
        /// <summary>
        /// 逆时针旋转90度
        /// </summary>
        private void CounterClockwiseRotation()
        {
            Properties.User.Default.rotateTo -= 1;
            if (Properties.User.Default.rotateTo < 0)
            {
                Properties.User.Default.rotateTo = 3;
            }
            Properties.User.Default.Save();
            Media.LayoutTransform = new RotateTransform(90 * Properties.User.Default.rotateTo);
            //显示消息
            ShowMessage("已逆时针旋转90度", false);
        }
        /// <summary>
        /// 到达循环结束点时的动作
        /// </summary>
        private void Loop()
        {
            if (HasEndPosition == true && StartPosition < EndPosition)
            {
                if (Media.Position > EndPosition)
                {
                    Media.Position = StartPosition;
                }
                if (Media.Position < StartPosition)
                {
                    Media.Position = StartPosition;
                }
            }
        }

        //检查
        /// <summary>
        /// 检测路径是否正确
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>路径是否正确</returns>
        private bool IsPathRight(string path)
        {
            //检测路径是否存在
            if (File.Exists(path))
            {
                //获取文件扩展名
                string _videoExtension = System.IO.Path.GetExtension(path);
                //检测文件扩展名是否正确
                if (_videoExtension == ".avi" || _videoExtension == ".mp4" || _videoExtension == ".rmvb" || _videoExtension == ".mkv" || _videoExtension == ".wmv" || _videoExtension == ".wma" ||
                    _videoExtension == ".AVI" || _videoExtension == ".MP4" || _videoExtension == ".RMVB" || _videoExtension == ".MKV" || _videoExtension == ".WMV" || _videoExtension == ".WMA" ||
                    _videoExtension == ".wav" || _videoExtension == ".mp3" || _videoExtension == ".aac" || _videoExtension == ".flac" ||
                    _videoExtension == ".WAV" || _videoExtension == ".MP3" || _videoExtension == ".AAC" || _videoExtension == ".FLAC")
                {
                    return true;
                }
                else
                {
                    ShowMessage("无法播放", true);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        //刷新
        /// <summary>
        /// 手动刷新一些未自动更改语言的UI文字
        /// </summary>
        private void RefreshUILanguage()
        {
            PlayListBox.ToolTip = FindResource("媒体总数") + "：" + VideoCount;
            SetPlayMode(Properties.User.Default.playMode);

            if (Media.Source == null)
            {
                //VideoName.Content = FindResource("媒体路径");
                //VideoName.ToolTip = FindResource("媒体路径");
            }
        }
        /// <summary>
        /// 更改主窗口右键菜单和窗口控件状态
        /// </summary>
        /// <param name="state"></param>
        private void ChangeElementState(int state)
        {
            if (state == 0)
            {
                //按钮可用性
                BtnPlay.IsEnabled = false;
                BtnReplay.IsEnabled = false;
                BtnForward.IsEnabled = false;
                BtnBack.IsEnabled = false;
                BtnSetLoopStart.IsEnabled = false;
                BtnSetLoopEnd.IsEnabled = false;
                BtnClearLoop.IsEnabled = false;
            }
            else
            {
                //按钮可用性
                BtnPlay.IsEnabled = true;
                BtnReplay.IsEnabled = true;
                BtnForward.IsEnabled = true;
                BtnBack.IsEnabled = true;
                BtnSetLoopStart.IsEnabled = true;
                BtnSetLoopEnd.IsEnabled = true;
                BtnClearLoop.IsEnabled = true;
            }
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
        /// 显示/隐藏控制区域
        /// </summary>
        private void ActiveControlArea()
        {
        }
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="tip">资源名</param>
        /// <param name="newBox">是否弹出对话框</param>
        private void ShowMessage(string tip, bool newBox = false)
        {
            if (newBox)
            {
                MessageBox.Show(tip.ToString());
            }
            else
            {
                Message.Opacity = 1;
                Message.Content = tip;
            }
        }
        /// <summary>
        /// 显示更多消息
        /// </summary>
        /// <param name="tip">资源名</param>
        /// <param name="moreText">附加信息</param>
        /// <param name="newBox">是否弹出对话框</param>
        private void ShowMessage(string tip, string moreText, bool newBox = false)
        {
            if (newBox)
            {
                System.Windows.MessageBox.Show(FindResource(tip) + moreText);
            }
            else
            {
                Message.Opacity = 1;
                Message.Content = FindResource(tip) + moreText;
            }
        }

        //切换
        /// <summary>
        /// 显示/隐藏播放列表
        /// </summary>
        private void SwichMenuVisibility()
        {
            Properties.User.Default.isShowMenu = !Properties.User.Default.isShowMenu;
            Properties.User.Default.Save();
            if (Properties.User.Default.isShowMenu)
            {
                PanLeft.Visibility = Visibility.Visible;
            }
            else
            {
                PanLeft.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// 切换全屏
        /// </summary>
        private void SwichScreenState()
        {
            if (IsFullScreen == false)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Topmost = true;

                Left = 0.0;
                Top = 0.0;
                Width = SystemParameters.PrimaryScreenWidth;
                Height = SystemParameters.PrimaryScreenHeight;
            }
            else
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResize;
                Topmost = false;
            }

            IsFullScreen = !IsFullScreen;
        }
        /// <summary>
        /// 切换播放模式
        /// </summary>
        private void SwichPlayMode()
        {
            Properties.User.Default.playMode += 1;
            if (Properties.User.Default.playMode == 4)
            { Properties.User.Default.playMode = 0; }
            Properties.User.Default.Save();
        }

        //窗口
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAppInfo();
            ShowAppInfo();
            LoadSettings();

            CreateTimer();

            Message.Opacity = 0;
            Main.Title = AppInfo.Name;

            LoadVideos();
            LoadLanguage(Properties.User.Default.language);

            SetPlayMode(Properties.User.Default.playMode);

            ChangeElementState(0);

        }
        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Properties.User.Default.isSavePlaylist)
            {
                //记录在播放列表的媒体路径，将所有路径集合到一个字符串
                Properties.User.Default._medias = "";
                if (PlayListBox.Items.Count > 0)
                {
                    foreach (ListBoxItem item in PlayListBox.Items)
                    {
                        Properties.User.Default._medias += item.Tag.ToString() + "///";
                    }
                    Properties.User.Default._medias = Properties.User.Default._medias.Substring(0, Properties.User.Default._medias.Length - 3);
                    Properties.User.Default.Save();
                }
                else
                {
                    Properties.User.Default.Save();
                }
            }
            else
            {
                Properties.User.Default._medias = "";
                Properties.User.Default.Save();
            }

            //重置翻转与旋转
            ResetFlip();
            ResetRotation();
        }
        private void Main_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            SwichMenuVisibility();
        }
        private void Main_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                SwichScreenState();
            }
        }
        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            //Ctrl+1 
            if (e.Key == Key.D1 && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                SetPlayMode(0);
            }
            //Ctrl+2
            else if (e.Key == Key.D2 && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                SetPlayMode(1);
            }
            //Ctrl+3 
            else if (e.Key == Key.D3 && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                SetPlayMode(2);
            }
            //Ctrl+4
            else if (e.Key == Key.D4 && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                SetPlayMode(3);
            }

            //O键打开媒体文件
            if (e.KeyStates == Keyboard.GetKeyStates(Key.O))
            {
                //浏览文件
                SelectedVideoPath = GetFilePath();
                //在播放列表添加此媒体，播放
                CreateListBoxItem(SelectedVideoPath, true);
            }
            //A键添加媒体文件
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.A))
            {
                //浏览文件
                SelectedVideoPath = GetFilePath();
                //在播放列表添加此媒体，不播放
                CreateListBoxItem(SelectedVideoPath, false);
            }
            //M键左右翻转
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.M))
            {
                LRFlip();
            }
            //N键上下翻转
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.N))
            {
                UDFlip();
            }
            //Q键逆时针
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.Q))
            {
                CounterClockwiseRotation();
            }
            //E键顺时针
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.E))
            {
                ClockwiseRotation();
            }
            //按+-键改变音量
            else if ((e.KeyStates == Keyboard.GetKeyStates(Key.OemPlus) || e.KeyStates == Keyboard.GetKeyStates(Key.Add)) && Media.Volume < 0.9)
            {
                VolumeUp();
            }
            else if ((e.KeyStates == Keyboard.GetKeyStates(Key.OemMinus) || e.KeyStates == Keyboard.GetKeyStates(Key.Subtract)) && Media.Volume > 0.1)
            {
                VolumeDown();
            }
            //按，。键改变播放速度
            else if ((e.KeyStates == Keyboard.GetKeyStates(Key.OemPeriod)) && Media.SpeedRatio < 2)
            {
                SpeedUp();
            }
            else if ((e.KeyStates == Keyboard.GetKeyStates(Key.OemComma)) && Media.SpeedRatio > 0.15)
            {
                SpeedDown();
            }
            //按Esc键退出全屏
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.Escape))
            {
                if (IsFullScreen == true)
                {
                    WindowState = WindowState.Maximized;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    ResizeMode = ResizeMode.CanResize;
                    Topmost = false;

                    IsFullScreen = false;
                }
            }
            //H键隐藏控制区
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.H) && !(e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                ActiveControlArea();
            }
            //L键显示/隐藏播放列表
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.L))
            {
                SwichMenuVisibility();
            }
            //Delete键删除视频
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.Delete))
            {
                Delete();
            }
            //Enter键播放选定视频
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter))
            {
                //获取当前选择的Item在PlayListBox的索引
                ListBoxItem tempItem;
                tempItem = PlayListBox.SelectedItem as ListBoxItem;
                if (tempItem != null)
                {
                    //获取路径
                    SelectedVideoPath = tempItem.Tag.ToString();
                    //播放
                    PlayNewVideo(SelectedVideoPath);
                }
            }
            //
            if (PlayingVideoPath != "" && PlayingVideoPath != null)
            {
                //按空格键播放和暂停
                if (e.KeyStates == Keyboard.GetKeyStates(Key.Space))
                {
                    Play();
                }
                //R键重新播放
                else if (e.KeyStates == Keyboard.GetKeyStates(Key.R))
                {
                    Replay();
                }
                //按←→键改变进度
                else if (e.KeyStates == Keyboard.GetKeyStates(Key.Right) && File.Exists(PlayingVideoPath))
                {
                    Forward();
                }
                else if (e.KeyStates == Keyboard.GetKeyStates(Key.Left) && File.Exists(PlayingVideoPath))
                {
                    Back();
                }
                //按[]键设置循环段开始与结束
                else if (e.KeyStates == Keyboard.GetKeyStates(Key.OemOpenBrackets) && File.Exists(PlayingVideoPath))
                {
                    SetLoopStart();
                }
                else if (e.KeyStates == Keyboard.GetKeyStates(Key.OemCloseBrackets) && File.Exists(PlayingVideoPath))
                {
                    SetLoopEnd();
                }
                //按\键清除循环段
                else if (e.KeyStates == Keyboard.GetKeyStates(Key.OemPipe))
                {
                    ClearLoop();
                }
            }

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

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            TimeSlider.Maximum = Media.NaturalDuration.TimeSpan.TotalSeconds;
            TimeAll.Content = Media.NaturalDuration;
        }
        private void ConsoleGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            ConsoleGrid.Opacity = 1;
            timer3.Stop();
        }
        private void ConsoleGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            timer3.Start();
        }
        private void TimeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            TimeSlider.AddHandler(MouseLeftButtonUpEvent,
                new MouseButtonEventHandler(TimeSlider_MouseLeftButtonUp), true);
            SldVolume.AddHandler(MouseLeftButtonUpEvent,
                new MouseButtonEventHandler(VolumeSlider_MouseLeftButtonUp), true);
        }
        private void TimeSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Media.Position = TimeSpan.FromSeconds(TimeSlider.Value);
            string str = Media.Position.ToString();
            //显示消息
            Message.Opacity = 1;
            Message.Content = "当前进度：" + str.Substring(0, 8);
        }
        private void VolumeSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            double temp = Math.Round(SldVolume.Value, 2);
            Media.Volume = temp;
            SldVolume.ToolTip = temp;
            //显示消息
            Message.Opacity = 1;
            Message.Content = "当前音量：" + temp;

        }
        private void Media_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Play();
        }
        private void Item_Play_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                //获取当前选择的Item在PlayListBox的索引
                ListBoxItem tempItem;
                tempItem = PlayListBox.SelectedItem as ListBoxItem;
                //获取路径
                SelectedVideoPath = tempItem.Tag.ToString();
                //播放
                PlayNewVideo(SelectedVideoPath);
            }
        }
        private void Item_Delete_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Delete();
        }
        private void PlayListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void PlayListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //if (e.LeftButton == MouseButtonState.Pressed)
            //{
            //    var pos = e.GetPosition(PlayListBox);
            //    HitTestResult result = VisualTreeHelper.HitTest(PlayListBox, pos);
            //    if (result == null)
            //    {
            //        return;
            //    }
            //    var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
            //    if (listBoxItem == null || listBoxItem.Content != PlayListBox.SelectedItem)
            //    {
            //        return;
            //    }
            //    System.Windows.DataObject dataObj = new System.Windows.DataObject(listBoxItem.Content as TextBlock);
            //    DragDrop.DoDragDrop(PlayListBox, dataObj, System.Windows.DragDropEffects.Move);
            //}
        }
        private void PlayListBox_PreviewDrop(object sender, DragEventArgs e)
        {
            ////拖拽排序
            //var pos = e.GetPosition(PlayListBox);
            //var result = VisualTreeHelper.HitTest(PlayListBox, pos);
            //if (result == null)
            //{
            //    return;
            //}
            ////查找元数据  
            //var sourcePerson = e.Data.GetData(typeof(TextBlock)) as TextBlock;
            //if (sourcePerson == null)
            //{
            //    return;
            //}
            ////查找目标数据  
            //var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
            //if (listBoxItem == null)
            //{
            //    return;
            //}
            //var targetPerson = listBoxItem.Content as TextBlock;

            //if (ReferenceEquals(targetPerson, sourcePerson))
            //{
            //    return;
            //}
            //PlayListBox.Items.Remove(sourcePerson);
            //PlayListBox.Items.Insert(PlayListBox.Items.IndexOf(targetPerson), sourcePerson);
        }

        private void SldVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void SldSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void Player_Drop(object sender, DragEventArgs e)
        {
            //如果有文件拖拽进入
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                //获取该文件路径
                SelectedVideoPath = ((Array)e.Data.GetData(System.Windows.DataFormats.FileDrop)).GetValue(0).ToString();
                //检测路径是否正确
                if (IsPathRight(SelectedVideoPath))
                {
                    //在播放列表添加此媒体，播放
                    CreateListBoxItem(SelectedVideoPath, true);
                }
            }
        }
        private void PlayListBox_Drop(object sender, DragEventArgs e)
        {
            //如果有文件拖拽进入
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                //获取该文件路径
                SelectedVideoPath = ((Array)e.Data.GetData(System.Windows.DataFormats.FileDrop)).GetValue(0).ToString();
                //检测路径是否正确
                if (IsPathRight(SelectedVideoPath))
                {
                    //在播放列表添加此媒体，不播放
                    CreateListBoxItem(SelectedVideoPath, false);
                }
            }
        }
        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            //单次
            if (Properties.User.Default.playMode == 0)
            {
                Media.Stop();
                IsPlay = false;
                BtnPlay.Content = FindResource("播放");

                //显示消息
                ShowMessage("播放结束", false);
            }
            //循环
            else if (Properties.User.Default.playMode == 1)
            {
                Media.Position = TimeSpan.Zero;
            }
            //顺序
            else if (Properties.User.Default.playMode == 2)
            {
                //获取当前正在播放的Item在PlayListBox的索引
                int n = 0;
                string[] path = new string[VideoCount];
                ListBoxItem tempItem;
                for (int i = 0; i < VideoCount; i++)
                {
                    tempItem = PlayListBox.Items.GetItemAt(i) as ListBoxItem;
                    path[i] = tempItem.Tag.ToString();

                    if (PlayingVideoPath == path[i])
                    {
                        n = PlayListBox.Items.IndexOf(tempItem);
                        break;
                    }
                }
                //如果是最后一个Item
                if (n == VideoCount - 1)
                {
                    //获取第一个Item
                    //ListBoxItem nextItem = PlayListBox.Items.GetItemAt(0) as ListBoxItem;
                    //获取nextItem储存的路径信息
                    //_selectedVideo = nextItem.Tag.ToString();
                    //停止播放
                    //Media.Source = new Uri(_selectedVideo);
                    Media.Stop();
                    Media.Source = null;
                    IsPlay = false;
                    //VideoName.Content = FindResource("媒体路径");
                    TimeAll.Content = "00:00:00";

                    //显示消息
                    ShowMessage("全部播放完毕", false);
                }
                else
                {
                    //获取它的下一个Item
                    ListBoxItem nextItem = PlayListBox.Items.GetItemAt(n + 1) as ListBoxItem;
                    //获取nextItem储存的路径信息
                    SelectedVideoPath = nextItem.Tag.ToString();

                    PlayListBox.SelectedIndex = n + 1;
                    //播放下一个视频
                    PlayNewVideo(SelectedVideoPath);
                    IsPlay = true;
                }
            }
            //随机
            else if (Properties.User.Default.playMode == 3)
            {
                Random ran = new Random();
                int n = ran.Next(0, VideoCount);
                //获取Item
                ListBoxItem nextItem = PlayListBox.ItemContainerGenerator.ContainerFromIndex(n) as ListBoxItem;
                //获取nextItem储存的路径信息
                SelectedVideoPath = nextItem.Tag.ToString();

                PlayListBox.SelectedIndex = n;
                //播放下一个视频
                PlayNewVideo(SelectedVideoPath);
                IsPlay = true;
            }

        }
       
        private void Timer1_Tick(object sender, EventArgs e)
        {
            TimePosition.Content = Media.Position;
            TimeSlider.Value = Media.Position.TotalSeconds;
        }
        private void Timer2_Tick(object sender, EventArgs e)
        {
            Message.Opacity -= 0.01;
            Loop();
        }
        private void Timer3_Tick(object sender, EventArgs e)
        {
            ConsoleGrid.Opacity -= 0.01;
        }

        private void CBSelectedLanguage_SelectionChanged(object sender, RoutedEventArgs e)
        {
            object selectedName = CbbLanguages.SelectedValue;
            if (selectedName != null)
            {
                string langName = selectedName.ToString();
                //根据本地语言来进行本地化,不过这里上不到
                //CultureInfo currentCultureInfo = CultureInfo.CurrentCulture;
                ResourceDictionary langRd = null;
                try
                {
                    if (langName == "zh_CN")
                    {
                        //根据名字载入语言文件
                        langRd = System.Windows.Application.LoadComponent(new Uri(@"语言/zh_CN.xaml", UriKind.Relative)) as ResourceDictionary;
                    }
                    else
                    {
                        //根据名字载入语言文件
                        langRd = System.Windows.Application.LoadComponent(new Uri(@"语言/" + langName + ".xaml", UriKind.Relative)) as ResourceDictionary;
                    }
                }
                catch (Exception e2)
                { MessageBox.Show(e2.Message); }

                if (langRd != null)
                {
                    //本窗口更改语言，如果已使用其他语言,先清空
                    if (this.Resources.MergedDictionaries.Count > 0)
                    { this.Resources.MergedDictionaries.Clear(); }
                    this.Resources.MergedDictionaries.Add(langRd);
                    //主窗口更改语言
                    if (Resources.MergedDictionaries.Count > 0)
                    { Resources.MergedDictionaries.Clear(); }
                    Resources.MergedDictionaries.Add(langRd);
                    //手动刷新一些未自动更改语言的UI文字
                    RefreshUILanguage();
                    //保存设置
                    Properties.User.Default.language = langName;
                    Properties.User.Default.Save();
                    //显示消息
                    //ShowMessage("已更改");
                }
            }
        }
        private void CbbThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void KeepTrans_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isKeepTrans = true;
            Properties.User.Default.Save();
            //显示消息
            //ShowMessage("已更改");
        }
        private void KeepTrans_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isKeepTrans = false;
            Properties.User.Default.Save();
            //显示消息
            //ShowMessage("已更改");
        }
        private void SavePlaylist_Checked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isSavePlaylist = true;
            Properties.User.Default.Save();
            //显示消息
            //ShowMessage("已更改");
        }
        private void SavePlaylist_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.isSavePlaylist = false;
            Properties.User.Default.Save();
            //显示消息
            //ShowMessage("已更改");
        }
        private void JumpTime_KeyUp(object sender, KeyEventArgs e)
        {
            if (JumpTime.Text != "")
            {
                try
                {
                    int t = int.Parse(JumpTime.Text);
                    if (t > 0 && t < 1000)
                    {
                        TimeSpan ts = TimeSpan.FromMinutes(t);
                        Properties.User.Default.jumpTime = t;
                        Properties.User.Default.Save();
                        //显示消息
                        // ShowMessage("已更改");
                    }
                    else
                    {
                        JumpTime.Text = Properties.User.Default.jumpTime.ToString();
                        // ShowMessage("输入1~999整数");
                    }
                }
                catch (Exception)
                {
                    JumpTime.Text = Properties.User.Default.jumpTime.ToString();
                    // ShowMessage("输入整数");
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
            CenterFilePage.Visibility = Visibility.Visible;
            CenterSettingPage.Visibility = Visibility.Collapsed;
            CenterAboutPage.Visibility = Visibility.Collapsed;

            BtnsFile.Visibility = Visibility.Visible;
            BtnsSetting.Visibility = Visibility.Collapsed;
            BtnsAbout.Visibility = Visibility.Collapsed;

            BtnFile.BorderThickness = new Thickness(4, 0, 0, 0);
            BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
        }
        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            CenterFilePage.Visibility = Visibility.Collapsed;
            CenterSettingPage.Visibility = Visibility.Visible;
            CenterAboutPage.Visibility = Visibility.Collapsed;

            BtnsFile.Visibility = Visibility.Collapsed;
            BtnsSetting.Visibility = Visibility.Visible;
            BtnsAbout.Visibility = Visibility.Collapsed;

            BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnSetting.BorderThickness = new Thickness(4, 0, 0, 0);
            BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
        }
        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            CenterFilePage.Visibility = Visibility.Collapsed;
            CenterSettingPage.Visibility = Visibility.Collapsed;
            CenterAboutPage.Visibility = Visibility.Visible;

            BtnsFile.Visibility = Visibility.Collapsed;
            BtnsSetting.Visibility = Visibility.Collapsed;
            BtnsAbout.Visibility = Visibility.Visible;

            BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnAbout.BorderThickness = new Thickness(4, 0, 0, 0);
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            //浏览文件
            SelectedVideoPath = GetFilePath();
            //在播放列表添加此媒体，播放
            CreateListBoxItem(SelectedVideoPath, true);
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            //浏览文件
            SelectedVideoPath = GetFilePath();
            //在播放列表添加此媒体，不播放
            CreateListBoxItem(SelectedVideoPath, false);
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            Properties.User.Default.Reset();

            //是否在切换视频时保持变换
            KeepTrans.IsChecked = Properties.User.Default.isKeepTrans;
            //是否在退出时保留播放列表
            SavePlaylist.IsChecked = Properties.User.Default.isSavePlaylist;
            //快进快退时间
            JumpTime.Text = Properties.User.Default.jumpTime.ToString();

            //语言
            SetLanguage(Properties.User.Default.language);

            //显示消息
            //ShowMessage("已重置");
        }
        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {

        }
        private void BtnClearRunInfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BitCoinAddress_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Clipboard.SetDataObject(BitCoinAddress.Text, true);
            //ShowMessage("已复制");
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

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            Play();
        }
        private void BtnReplay_Click(object sender, RoutedEventArgs e)
        {
            Replay();
        }
        private void BtnForward_Click(object sender, RoutedEventArgs e)
        {
            Forward();
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Back();
        }
        private void BtnVolumeUp_Click(object sender, RoutedEventArgs e)
        {
            VolumeUp();
        }
        private void BtnVolumeDown_Click(object sender, RoutedEventArgs e)
        {
            VolumeDown();
        }
        private void BtnSpeedUp_Click(object sender, RoutedEventArgs e)
        {
            SpeedUp();
        }
        private void BtnSpeedDown_Click(object sender, RoutedEventArgs e)
        {
            SpeedDown();
        }
        private void BtnLoopStart_Click(object sender, RoutedEventArgs e)
        {
            SetLoopStart();
        }
        private void BtnLoopEnd_Click(object sender, RoutedEventArgs e)
        {
            SetLoopEnd();
        }
        private void BtnClearLoop_Click(object sender, RoutedEventArgs e)
        {
            ClearLoop();
        }
        private void BtnLRFlip_Click(object sender, RoutedEventArgs e)
        {
            LRFlip();
        }
        private void BtnUDFlip_Click(object sender, RoutedEventArgs e)
        {
            UDFlip();
        }
        private void BtnClockwise_Click(object sender, RoutedEventArgs e)
        {
            ClockwiseRotation();
        }
        private void BtnCClockwise_Click(object sender, RoutedEventArgs e)
        {
            CounterClockwiseRotation();
        }
        private void BtnHideControl_Click(object sender, RoutedEventArgs e)
        {
            ActiveControlArea();
        }

        private void BtnMenu_Click(object sender, RoutedEventArgs e)
        {
            SwichMenuVisibility();
        }
        private void BtnFullScreen_Click(object sender, RoutedEventArgs e)
        {
            SwichScreenState();
        }
        private void CbbPlayMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetPlayMode(CbbPlayMode.SelectedIndex);
        }
    }
}
