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


namespace E_Player
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 获取软件信息
        /// <summary>
        /// 名称
        /// </summary>
        public static string ThisName { get; } = Application.ProductName;
        /// <summary>
        /// 描述
        /// </summary>
        public static string ThisDescription { get; } = "小说编辑器";
        /// <summary>
        /// 开发者
        /// </summary>
        public static string ThisCompany { get; } = Application.CompanyName;
        /// <summary>
        /// 作者
        /// </summary>
        public static string ThisDeveloper { get; } = "E Star";
        /// <summary>
        /// 当前版本
        /// </summary>
        public Version ThisVer { get; } = new Version(Application.ProductVersion);
        /// <summary>
        /// 新版本
        /// </summary>
        public Version NewVer { get; private set; }
        /// <summary>
        /// 新版信息获取连接
        /// </summary>
        public static string NewVerInfoLink { get; } = "http://estar.zone/wp-content/E_Updater_info.txt";
        /// <summary>
        /// 新版下载连接
        /// </summary>
        public static string NewVerDownloadLink { get; } = "http://estar.zone/introduction/e-player/";
        #endregion


        #region 运行中的信息
        //路径
        public static string _download = @"下载";           //下载文件夹
        //计时器
        public DispatcherTimer timer1;
        public DispatcherTimer timer2;
        public DispatcherTimer timer3;
        //运行中的信息
        public string _selectedVideo;                    //选中的视频的路径
        public string _playingVideo;                     //正在播放的视频的路径
        public bool isPlay = false;                      //是否正在播放
        public bool isFullScreen = false;                //是否全屏
        public int videoCount = 0;                       //列表中视频数量
        public double tmpWidthRight;                     //右侧区域大小
        public TimeSpan startPosition = TimeSpan.Zero;   //循环的开始
        public TimeSpan endPosition = TimeSpan.Zero;     //循环的结束
        public bool hasStartPosition = false;            //是否有循环开始点
        public bool hasEndPosition = false;              //是否有循环结束点
        #endregion

        //右键菜单
        ContextMenu CM = new ContextMenu();
        //一级菜单
        MenuItem MenuFile = new MenuItem();
        MenuItem MenuControl = new MenuItem();
        MenuItem MenuWindow = new MenuItem();
        MenuItem MenuHelp = new MenuItem();
        //二级菜单 MenuFile
        MenuItem MenuOpen = new MenuItem();
        MenuItem MenuAdd = new MenuItem();
        MenuItem MenuDelete = new MenuItem();
        Separator separator1 = new Separator();
        MenuItem MenuCloseEP = new MenuItem();
        //二级菜单 MenuControl
        MenuItem MenuPlay = new MenuItem();
        //MenuItem MenuPause = new MenuItem();
        //MenuItem MenuStop = new MenuItem();
        MenuItem MenuReplay = new MenuItem();
        Separator separator2 = new Separator();
        MenuItem MenuForward = new MenuItem();
        MenuItem MenuBack = new MenuItem();
        MenuItem MenuVolumeUp = new MenuItem();
        MenuItem MenuVolumeDown = new MenuItem();
        MenuItem MenuSpeedUp = new MenuItem();
        MenuItem MenuSpeedDown = new MenuItem();
        Separator separator3 = new Separator();
        MenuItem MenuLoopStart = new MenuItem();
        MenuItem MenuLoopEnd = new MenuItem();
        MenuItem MenuClearLoop = new MenuItem();
        Separator separator4 = new Separator();
        MenuItem MenuLRFlip = new MenuItem();
        MenuItem MenuUDFlip = new MenuItem();
        MenuItem MenuClockwise = new MenuItem();
        MenuItem MenuCClockwise = new MenuItem();
        Separator separator5 = new Separator();
        MenuItem MenuFullScreen = new MenuItem();
        Separator separator6 = new Separator();
        MenuItem MenuSingle = new MenuItem();
        MenuItem MenuLoop = new MenuItem();
        MenuItem MenuOrder = new MenuItem();
        MenuItem MenuRandom = new MenuItem();
        //二级菜单 MenuWindow
        MenuItem MenuHideList = new MenuItem();
        MenuItem MenuHideControl = new MenuItem();
        //二级菜单 MenuHelp
        MenuItem MenuPreference = new MenuItem();
        MenuItem MenuAbout = new MenuItem();
        MenuItem MenuLink = new MenuItem();


        /// <summary>
        /// 主窗口构造器
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        //窗口
        /// <summary>
        /// 窗口载入事件
        /// </summary>
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Message.Opacity = 0;
            //创建计时器
            CreateTimer();
            //VideoName.ToolTip = FindResource("媒体路径");
            //设置标题
            string ver = Application.ProductVersion.ToString();
            Main.Title = ThisName;
            //载入界面语言
            LoadLanguage(Properties.User.Default.language);
            //初始化右键菜单
            CreteContextMenu();
            //不激活按钮
            ChangeElementState(0);
            //设置播放列表状态
            if (!Properties.User.Default.isShowList)
            {
                tmpWidthRight = RightArea.ActualWidth;
                RightArea.Width = new GridLength(0);
                MidArea.Width = new GridLength(0);
            }
            //读取一个字符串，并加入播放列表
            if (Properties.User.Default._medias != null && Properties.User.Default._medias != "")
            {
                string[] _myB = Regex.Split(Properties.User.Default._medias, "///");
                foreach (var b in _myB)
                {
                    CreateListBoxItem(b, false);
                }
                //激活按钮
                //ChangeElementState(1);
            }
            //设置播放模式按钮信息
            SetPlayMode(Properties.User.Default.playMode, false);
        }
        /// <summary>
        /// 窗口关闭事件
        /// </summary>
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
        //快捷键
        /// <summary>
        /// 快捷键
        /// </summary>
        private void Main_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //Ctrl+1 
            if (e.Key == Key.D1 && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                SetPlayMode(0,true);
            }
            //Ctrl+2
            else if (e.Key == Key.D2 && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                SetPlayMode(1, true);
            }
            //Ctrl+3 
            else if (e.Key == Key.D3 && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                SetPlayMode(2, true);
            }
            //Ctrl+4
            else if (e.Key == Key.D4 && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                SetPlayMode(3, true);
            }
            else if (e.Key == Key.F1)
            {
                OpenHelp(1);
            }
            else if (e.Key == Key.F2)
            {
                OpenHelp(2);
            }
            else if (e.Key == Key.F3)
            {
                OpenHelp(3);
            }
            //O键打开媒体文件
            if (e.KeyStates == Keyboard.GetKeyStates(Key.O))
            {
                //浏览文件
                _selectedVideo = Open();
                //在播放列表添加此媒体，播放
                CreateListBoxItem(_selectedVideo, true);
            }
            //A键添加媒体文件
            else if (e.KeyStates == Keyboard.GetKeyStates(Key.A))
            {
                //浏览文件
                _selectedVideo = Open();
                //在播放列表添加此媒体，不播放
                CreateListBoxItem(_selectedVideo, false);
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
            else if ((e.KeyStates == Keyboard.GetKeyStates(Key.OemPlus) || e.KeyStates == Keyboard.GetKeyStates(Key.Add))  && Media.Volume < 0.9)
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
                if (isFullScreen == true)
                {
                    WindowState = WindowState.Maximized;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    ResizeMode = ResizeMode.CanResize;
                    Topmost = false;

                    isFullScreen = false;
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
                ShowList();
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
                    _selectedVideo = tempItem.Tag.ToString();
                    //播放
                    PlayNewVideo(_selectedVideo);
                }
            }
            //
            if (_playingVideo != "" && _playingVideo != null)
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
                else if (e.KeyStates == Keyboard.GetKeyStates(Key.Right) && File.Exists(_playingVideo))
                {
                    Forward();
                }
                else if (e.KeyStates == Keyboard.GetKeyStates(Key.Left) && File.Exists(_playingVideo))
                {
                    Back();
                }
                //按[]键设置循环段开始与结束
                else if (e.KeyStates == Keyboard.GetKeyStates(Key.OemOpenBrackets) && File.Exists(_playingVideo))
                {
                    SetLoopStart();
                }
                else if (e.KeyStates == Keyboard.GetKeyStates(Key.OemCloseBrackets) && File.Exists(_playingVideo))
                {
                    SetLoopEnd();
                }
                //按\键清除循环段
                else if (e.KeyStates == Keyboard.GetKeyStates(Key.OemPipe))
                {
                    ClearLoop();
                }
            }
        }
        //右键菜单点击
        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            //浏览文件
            _selectedVideo = Open();
            //在播放列表添加此媒体，播放
            CreateListBoxItem(_selectedVideo, true);
        }
        private void MenuAdd_Click(object sender, RoutedEventArgs e)
        {
            //浏览文件
            _selectedVideo = Open();
            //在播放列表添加此媒体，不播放
            CreateListBoxItem(_selectedVideo, false);
        }
        private void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete();
        }
        private void MenuCloseEP_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void MenuPlay_Click(object sender, RoutedEventArgs e)
        {
            Play();
        }
        private void MenuReplay_Click(object sender, RoutedEventArgs e)
        {
            Replay();
        }
        private void MenuForward_Click(object sender, RoutedEventArgs e)
        {
            Forward();
        }
        private void MenuBack_Click(object sender, RoutedEventArgs e)
        {
            Back();
        }
        private void MenuVolumeUp_Click(object sender, RoutedEventArgs e)
        {
            VolumeUp();
        }
        private void MenuVolumeDown_Click(object sender, RoutedEventArgs e)
        {
            VolumeDown();
        }
        private void MenuSpeedUp_Click(object sender, RoutedEventArgs e)
        {
            SpeedUp();
        }
        private void MenuSpeedDown_Click(object sender, RoutedEventArgs e)
        {
            SpeedDown();
        }
        private void MenuLoopStart_Click(object sender, RoutedEventArgs e)
        {
            SetLoopStart();
        }
        private void MenuLoopEnd_Click(object sender, RoutedEventArgs e)
        {
            SetLoopEnd();
        }
        private void MenuClearLoop_Click(object sender, RoutedEventArgs e)
        {
            ClearLoop();
        }
        private void MenuLRFlip_Click(object sender, RoutedEventArgs e)
        {
            LRFlip();
        }
        private void MenuUDFlip_Click(object sender, RoutedEventArgs e)
        {
            UDFlip();
        }
        private void MenuClockwise_Click(object sender, RoutedEventArgs e)
        {
            ClockwiseRotation();
        }
        private void MenuCClockwise_Click(object sender, RoutedEventArgs e)
        {
            CounterClockwiseRotation();
        }
        private void MenuFullScreen_Click(object sender, RoutedEventArgs e)
        {
            FullScreen();
        }
        private void MenuSingle_Click(object sender, RoutedEventArgs e)
        {
            SetPlayMode(0, true);
        }
        private void MenuLoop_Click(object sender, RoutedEventArgs e)
        {
            SetPlayMode(1, true);
        }
        private void MenuOrder_Click(object sender, RoutedEventArgs e)
        {
            SetPlayMode(2, true);
        }
        private void MenuRandom_Click(object sender, RoutedEventArgs e)
        {
            SetPlayMode(3, true);
        }
        private void MenuHideControl_Click(object sender, RoutedEventArgs e)
        {
            ActiveControlArea();
        }
        private void MenuHideList_Click(object sender, RoutedEventArgs e)
        {
            ShowList();
        }
        public void MenuHelp_Click(object sender, RoutedEventArgs e)
        {
            OpenHelp(1);
        }
        /// <summary>
        /// 切换播放模式按钮
        /// </summary>
        private void ButtonPlayMode_Click(object sender, RoutedEventArgs e)
        {
            ChangePlayMode();
        }
        
        /// <summary>
        /// 点击 打开帮助
        /// </summary>
        //private void MenuHelp_Click(object sender, RoutedEventArgs e)
        //{
        //    OpenHelp(1);
        //}
        /// <summary>
        /// 点击 偏好设置
        /// </summary>
        private void MenuPreference_Click(object sender, RoutedEventArgs e)
        {
            OpenHelp(1);
        }
        /// <summary>
        /// 点击 软件信息
        /// </summary>
        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            OpenHelp(2);
        }
        /// <summary>
        /// 点击 相关链接
        /// </summary>
        private void MenuLink_Click(object sender, RoutedEventArgs e)
        {
            OpenHelp(3);
        }
        
        /// <summary>
        /// 视频载入时获取视频时长
        /// </summary>
        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            TimeSlider.Maximum = Media.NaturalDuration.TimeSpan.TotalSeconds;
            TimeAll.Content = Media.NaturalDuration;
        }
        /// <summary>
        /// 同步播放列表媒体元素宽度
        /// </summary>
        private void PlayListBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (ListBoxItem item in PlayListBox.Items)
            {
                if (PlayListBox.ActualWidth - 4 <= 0)
                {
                    item.Width = 100;
                }
                else
                {
                    item.Width = PlayListBox.ActualWidth - 4;
                }
            }
        }
        /// <summary>
        /// 窗口尺寸变更
        /// </summary>
        private void Main_SizeChanged(object sender, SizeChangedEventArgs e)
        {


        }
        private void Main_StateChanged(object sender, EventArgs e)
        {
        }
        /// <summary>
        /// 控制区域显隐
        /// </summary>
        private void ConsoleGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ConsoleGrid.Opacity = 1;
            timer3.Stop();
        }
        private void ConsoleGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            timer3.Start();
        }
        /// <summary>
        /// 增加句柄，保证触发鼠标左键点击事件
        /// </summary>
        private void TimeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            TimeSlider.AddHandler(MouseLeftButtonUpEvent,
                new MouseButtonEventHandler(TimeSlider_MouseLeftButtonUp), true);
            VolumeSlider.AddHandler(MouseLeftButtonUpEvent,
                new MouseButtonEventHandler(VolumeSlider_MouseLeftButtonUp), true);
        }
        /// <summary>
        /// 按鼠标左键改变进度
        /// </summary>
        private void TimeSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Media.Position = TimeSpan.FromSeconds(TimeSlider.Value);
            string str = Media.Position.ToString();
            //显示消息
            Message.Opacity = 1;
            Message.Content = "当前进度：" + str.Substring(0, 8);
        }
        /// <summary>
        /// 按鼠标左键改变音量
        /// </summary>
        private void VolumeSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            double temp = Math.Round(VolumeSlider.Value, 2);
            Media.Volume = temp;
            VolumeSlider.ToolTip = temp;
            //显示消息
            Message.Opacity = 1;
            Message.Content = "当前音量：" + temp;

        }
        /// <summary>
        /// 鼠标左键播放与暂停
        /// </summary>
        private void Media_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Play();
        }
        /// <summary>
        /// 在播放区域，双击鼠标左键设置全屏
        /// </summary>
        private void Player_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                FullScreen();
            }
        }
        /// <summary>
        /// 在播放列表Item，双击鼠标左键播放视频
        /// </summary>
        private void Item_Play_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                //获取当前选择的Item在PlayListBox的索引
                ListBoxItem tempItem;
                tempItem = PlayListBox.SelectedItem as ListBoxItem;
                //获取路径
                _selectedVideo = tempItem.Tag.ToString();
                //播放
                PlayNewVideo(_selectedVideo);
            }
        }
        /// <summary>
        /// 在播放列表Item，单击鼠标右键
        /// </summary>
        private void Item_Delete_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Delete();
        }
        /// <summary>
        /// 
        /// </summary>
        private void PlayListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void PlayListBox_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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
        private void PlayListBox_PreviewDrop(object sender, System.Windows.DragEventArgs e)
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

        
        //具体的功能实现
        /// <summary>
        /// 浏览文件获取路径
        /// </summary>
        /// <returns>媒体路径</returns>
        private string Open()
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
                if (videoCount == 0)
                {
                    //增加第一个Item进PlayListBox
                    PlayListBox.Items.Add(PlayListBoxItem);
                    videoCount = 1;
                    PlayListBox.ToolTip = FindResource("媒体总数") + "：" + videoCount;
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
                    for (int i = 0; i < videoCount; i++)
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
                        videoCount = PlayListBox.Items.Count;
                        PlayListBox.ToolTip = FindResource("媒体总数") + "：" + videoCount;
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
            isPlay = true;
            //激活按钮
            ChangeElementState(1);
            ButtonPlay.Content =FindResource("暂停");
            _playingVideo = path;

            //帮助消息
            VideoName.ToolTip = path;
            VideoName.Content = GetVideoName(path);
            //显示消息
            ShowMessage("开始播放", "：" + GetVideoName(path), false);
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
        /// 播放与暂停操作
        /// </summary>
        private void Play()
        {
            if (isPlay == false)
            {
                Media.Play();
                isPlay = true;
                ButtonPlay.Content = FindResource("暂停");
                MenuPlay.Header = FindResource("暂停");
                //显示消息
                ShowMessage("开始播放", false);
            }
            else if (isPlay == true)
            {
                Media.Pause();
                isPlay = false;
                ButtonPlay.Content = FindResource("播放");
                MenuPlay.Header = FindResource("播放");
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
            isPlay = true;
            ButtonPlay.Content = FindResource("暂停");
            MenuPlay.Header = FindResource("暂停");
            //显示消息
            ShowMessage("开始播放", false);
        }
        /// <summary>
        /// 快进
        /// </summary>
        private void Forward()
        {
            if (File.Exists(_playingVideo))
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
            if (File.Exists(_playingVideo))
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
            VolumeSlider.Value = temp;
            VolumeSlider.ToolTip = temp;
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
            VolumeSlider.Value = temp;
            VolumeSlider.ToolTip = temp;
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
        /// 切换全屏
        /// </summary>
        private void FullScreen()
        {
            if (isFullScreen == false)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Topmost = true;

                Left = 0.0;
                Top = 0.0;
                Width = SystemParameters.PrimaryScreenWidth;
                Height = SystemParameters.PrimaryScreenHeight;

                isFullScreen = true;
                MenuFullScreen.Header = FindResource("退出全屏");
            }
            else
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResize;
                Topmost = false;

                isFullScreen = false;
                MenuFullScreen.Header = FindResource("全屏显示");
            }
        }
        /// <summary>
        /// 显示/隐藏播放列表
        /// </summary>
        private void ShowList()
        {
            //显示
            if (Properties.User.Default.isShowList == false)
            {
                //LeftArea.Width = new GridLength(tmpWidthLeft);
                RightArea.Width = new GridLength(tmpWidthRight);
                MidArea.Width = new GridLength(10);

                Properties.User.Default.isShowList = true;
                MenuHideList.Header = FindResource("隐藏播放列表");
            }
            //隐藏
            else if (Properties.User.Default.isShowList == true)
            {
                //tmpWidthLeft = LeftArea.ActualWidth;
                //LeftArea.Width = new GridLength(MainGrid.ActualWidth);
                tmpWidthRight = RightArea.ActualWidth;
                RightArea.Width = new GridLength(0);
                MidArea.Width = new GridLength(0);

                Properties.User.Default.isShowList = false;
                MenuHideList.Header = FindResource("显示播放列表");
            }
        }
        /// <summary>
        /// 显示/隐藏控制区域
        /// </summary>
        private void ActiveControlArea()
        {
            //显示
            if (ConsoleGrid.Opacity < 0.5)
            {
                ConsoleGrid.Opacity = 1;
                timer3.Stop();
                MenuHideControl.Header = FindResource("隐藏控制台");
            }
            //隐藏
            else if (ConsoleGrid.Opacity >= 0.5)
            {
                ConsoleGrid.Opacity = 0;
                timer3.Stop();
                MenuHideControl.Header = FindResource("显示控制台");
            }
        }
        /// <summary>
        /// 切换播放模式
        /// </summary>
        private void ChangePlayMode()
        {
            Properties.User.Default.playMode += 1;
            if (Properties.User.Default.playMode == 4)
            { Properties.User.Default.playMode = 0; }
            Properties.User.Default.Save();

            ShowPlayMode(Properties.User.Default.playMode, true);
        }
        /// <summary>
        /// 设置播放模式
        /// </summary>
        /// <param name="mode">模式</param>
        /// <param name="showMessage">是否显示消息</param>
        private void SetPlayMode(int mode, bool show)
        {
            Properties.User.Default.playMode = mode;
            Properties.User.Default.Save();
            ShowPlayMode(mode, show);
        }
        /// <summary>
        /// 拖拽文件进入Player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Player_Drop(object sender, System.Windows.DragEventArgs e)
        {
            //如果有文件拖拽进入
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                //获取该文件路径
                _selectedVideo = ((Array)e.Data.GetData(System.Windows.DataFormats.FileDrop)).GetValue(0).ToString();
                //检测路径是否正确
                if (IsPathRight(_selectedVideo))
                {
                    //在播放列表添加此媒体，播放
                    CreateListBoxItem(_selectedVideo, true);
                }
            }
        }
        /// <summary>
        /// 拖拽文件进入播放列表
        /// </summary>
        private void PlayListBox_Drop(object sender, System.Windows.DragEventArgs e)
        {
            //如果有文件拖拽进入
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                //获取该文件路径
                _selectedVideo = ((Array)e.Data.GetData(System.Windows.DataFormats.FileDrop)).GetValue(0).ToString();
                //检测路径是否正确
                if (IsPathRight(_selectedVideo))
                {
                    //在播放列表添加此媒体，不播放
                    CreateListBoxItem(_selectedVideo, false);
                }
            }
        }
        /// <summary>
        /// 媒体播放结束事件
        /// </summary>
        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            //单次
            if (Properties.User.Default.playMode == 0)
            {
                Media.Stop();
                isPlay = false;
                ButtonPlay.Content = FindResource("播放");

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
                int n=0;
                string[] path = new string[videoCount];
                ListBoxItem tempItem;
                for (int i = 0; i < videoCount; i++)
                {
                    tempItem = PlayListBox.Items.GetItemAt(i) as ListBoxItem;
                    path[i] = tempItem.Tag.ToString();

                    if (_playingVideo == path[i])
                    {
                        n = PlayListBox.Items.IndexOf(tempItem);
                        break;
                    }
                }
                //如果是最后一个Item
                if (n == videoCount - 1)
                {
                    //获取第一个Item
                    //ListBoxItem nextItem = PlayListBox.Items.GetItemAt(0) as ListBoxItem;
                    //获取nextItem储存的路径信息
                    //_selectedVideo = nextItem.Tag.ToString();
                    //停止播放
                    //Media.Source = new Uri(_selectedVideo);
                    Media.Stop();
                    Media.Source = null;
                    isPlay = false;
                    VideoName.Content = FindResource("媒体路径");
                    TimeAll.Content = "00:00:00";
                    
                    //显示消息
                    ShowMessage("全部播放完毕",false);
                }
                else
                {
                    //获取它的下一个Item
                    ListBoxItem nextItem = PlayListBox.Items.GetItemAt(n+1) as ListBoxItem;
                    //获取nextItem储存的路径信息
                    _selectedVideo = nextItem.Tag.ToString();

                    PlayListBox.SelectedIndex = n + 1;
                    //播放下一个视频
                    PlayNewVideo(_selectedVideo);
                    isPlay = true;
                }
            }
            //随机
            else if (Properties.User.Default.playMode == 3)
            {
                Random ran = new Random();
                int n = ran.Next(0, videoCount);
                //获取Item
                ListBoxItem nextItem = PlayListBox.ItemContainerGenerator.ContainerFromIndex(n) as ListBoxItem;
                //获取nextItem储存的路径信息
                _selectedVideo = nextItem.Tag.ToString();

                PlayListBox.SelectedIndex = n;
                //播放下一个视频
                PlayNewVideo(_selectedVideo);
                isPlay = true;
            }

        }
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
                if (_playingVideo == tempItem.Tag.ToString())
                {
                    Media.Stop();
                    Media.Source = null;
                    _playingVideo = "";
                    Cover.Source = null;
                    Cover.IsEnabled = false;
                    isPlay = false;
                    //不激活按钮
                    ChangeElementState(0);
                    VideoName.Content = FindResource("媒体路径");
                    VideoName.ToolTip = FindResource("媒体路径");
                    TimeAll.Content = "00:00:00";
                    ButtonPlay.Content = FindResource("播放");

                    //显示消息
                    ShowMessage("删除视频", false);
                }
                int s = PlayListBox.SelectedIndex;
                PlayListBox.Items.Remove(PlayListBox.SelectedItem);
                PlayListBox.SelectedIndex = s;
                if (s == videoCount - 1)
                {
                    PlayListBox.SelectedIndex = s - 1;
                }
                videoCount -= 1;
                PlayListBox.ToolTip = FindResource("媒体总数") + "：" + videoCount;
            }
            else
            {
                //显示消息
                ShowMessage("未选择视频", false);
            }
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
        /// 翻转归位
        /// </summary>
        private void ResetFlip()
        {
            Properties.User.Default.isLRFlip = false;
            Properties.User.Default.isUDFlip = false;
            Properties.User.Default.Save();
            ScaleTransform scaleTransform = new ScaleTransform();
            scaleTransform.ScaleX = 1;
            scaleTransform.ScaleY = 1;
            Media.LayoutTransform = scaleTransform;
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
            if (tempPosition < endPosition || hasEndPosition == false)
            {
                startPosition = tempPosition;
                string str = startPosition.ToString();
                //滑块高亮区域
                TimeSlider.SelectionStart = startPosition.TotalSeconds;
                hasStartPosition = true;

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
            if (startPosition < tempPosition)
            {
                endPosition = tempPosition;
                string str = endPosition.ToString();
                //滑块高亮区域
                TimeSlider.SelectionEnd = endPosition.TotalSeconds;
                hasEndPosition = true;

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
        /// 清除循环段
        /// </summary>
        private void ClearLoop()
        {
            if (hasEndPosition == true || hasStartPosition == true)
            {
                startPosition = TimeSpan.Zero;
                endPosition = TimeSpan.Zero;
                hasStartPosition = false;
                hasEndPosition = false;

                //滑块高亮区域
                TimeSlider.SelectionStart = 0;
                TimeSlider.SelectionEnd = 0;
                //显示消息
                ShowMessage("已清除循环", false);
            }
        }
        /// <summary>
        /// 到达循环结束点时的动作
        /// </summary>
        private void Loop()
        {
            if (hasEndPosition == true && startPosition < endPosition)
            {
                if (Media.Position > endPosition)
                {
                    Media.Position = startPosition;
                }
                if (Media.Position < startPosition)
                {
                    Media.Position = startPosition;
                }
            }
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
        /// <summary>
        /// 由地址返回图片，返回类型为System.Drawing.Image
        /// </summary>
        /// <param name="MP3path">MP3全地址</param>
        /// <returns></returns>
        public List<BitmapImage> CoverofMP3(string MP3path)
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
        /// 载入语言
        /// </summary>
        /// <param name="lang">语言简拼</param>
        public void LoadLanguage(string lang)
        {
            ResourceDictionary langRd = null;
            try
            {
                //根据名字载入语言文件
                langRd = System.Windows.Application.LoadComponent(new Uri(@"languages\" + lang + ".xaml", UriKind.Relative)) as ResourceDictionary;
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
        /// 手动刷新一些未自动更改语言的UI文字
        /// </summary>
        public void RefreshUILanguage()
        {
            PlayListBox.ToolTip = FindResource("媒体总数") + "：" + videoCount;
            SetPlayMode(Properties.User.Default.playMode ,false);

            if (Media.Source == null)
            {
                VideoName.Content = FindResource("媒体路径");
                VideoName.ToolTip = FindResource("媒体路径");
            }
        }
        /// <summary>
        /// 初始化右键菜单
        /// </summary>
        public void CreteContextMenu()
        {
            //CM.Width = 600;
            //CM.Placement = Primitives.PlacementMode.Left;

            //一级菜单
            MenuFile.Header = FindResource("文件");
            CM.Items.Add(MenuFile);
            MenuControl.Header = FindResource("控制");
            CM.Items.Add(MenuControl);
            MenuWindow.Header = FindResource("窗口");
            CM.Items.Add(MenuWindow);
            MenuHelp.Header = FindResource("帮助");
            MenuHelp.InputGestureText = "F1";
            CM.Items.Add(MenuHelp);

            //二级菜单 MenuFile
            MenuOpen.Header = FindResource("打开");
            MenuOpen.InputGestureText = "O";
            MenuOpen.Click += new RoutedEventHandler(MenuOpen_Click);
            MenuFile.Items.Add(MenuOpen);
            MenuAdd.Header = FindResource("添加");
            MenuAdd.InputGestureText = "A";
            MenuAdd.Click += new RoutedEventHandler(MenuAdd_Click);
            MenuFile.Items.Add(MenuAdd);
            MenuDelete.Header = FindResource("删除");
            MenuDelete.InputGestureText = "Delete";
            MenuDelete.Click += new RoutedEventHandler(MenuDelete_Click);
            MenuFile.Items.Add(MenuDelete);
            //分隔线1
            MenuFile.Items.Add(separator1);
            MenuCloseEP.Header = FindResource("退出");
            MenuCloseEP.InputGestureText = "Alt+F4";
            MenuCloseEP.Click += new RoutedEventHandler(MenuCloseEP_Click);
            MenuFile.Items.Add(MenuCloseEP);

            //二级菜单 MenuControl
            MenuPlay.Header = FindResource("播放");
            MenuPlay.InputGestureText = "Space";
            MenuPlay.IsEnabled = true;
            MenuPlay.Click += new RoutedEventHandler(MenuPlay_Click);
            MenuControl.Items.Add(MenuPlay);
            //MenuStop.Header = FindResource("停止");
            //MenuStop.InputGestureText = "";
            //MenuStop.IsEnabled = true;
            //MenuStop.Click += new RoutedEventHandler(MenuStop_Click);
            //MenuControl.Items.Add(MenuStop);
            MenuReplay.Header = FindResource("重播");
            MenuReplay.InputGestureText = "R";
            MenuReplay.IsEnabled = true;
            MenuReplay.Click += new RoutedEventHandler(MenuReplay_Click);
            MenuControl.Items.Add(MenuReplay);
            //分隔线
            MenuControl.Items.Add(separator2);
            MenuForward.Header = FindResource("快进");
            MenuForward.InputGestureText = "→";
            MenuForward.IsEnabled = true;
            MenuForward.Click += new RoutedEventHandler(MenuForward_Click);
            MenuControl.Items.Add(MenuForward);
            MenuBack.Header = FindResource("快退");
            MenuBack.InputGestureText = "←";
            MenuBack.IsEnabled = true;
            MenuBack.Click += new RoutedEventHandler(MenuBack_Click);
            MenuControl.Items.Add(MenuBack);
            MenuVolumeUp.Header = FindResource("音量升高");
            MenuVolumeUp.InputGestureText = "";
            MenuVolumeUp.Click += new RoutedEventHandler(MenuVolumeUp_Click);
            MenuControl.Items.Add(MenuVolumeUp);
            MenuVolumeDown.Header = FindResource("音量降低");
            MenuVolumeDown.InputGestureText = "";
            MenuVolumeDown.Click += new RoutedEventHandler(MenuVolumeDown_Click);
            MenuControl.Items.Add(MenuVolumeDown);
            MenuSpeedUp.Header = FindResource("速度增加");
            MenuSpeedUp.InputGestureText = "";
            MenuSpeedUp.Click += new RoutedEventHandler(MenuSpeedUp_Click);
            MenuControl.Items.Add(MenuSpeedUp);
            MenuSpeedDown.Header = FindResource("速度减少");
            MenuSpeedDown.InputGestureText = "";
            MenuSpeedDown.Click += new RoutedEventHandler(MenuSpeedDown_Click);
            MenuControl.Items.Add(MenuSpeedDown);
            //分隔线
            MenuControl.Items.Add(separator3);
            MenuLoopStart.Header = FindResource("循环起点");
            MenuLoopStart.InputGestureText = "";
            MenuLoopStart.IsEnabled = true;
            MenuLoopStart.Click += new RoutedEventHandler(MenuLoopStart_Click);
            MenuControl.Items.Add(MenuLoopStart);
            MenuLoopEnd.Header = FindResource("循环终点");
            MenuLoopEnd.InputGestureText = "";
            MenuLoopEnd.IsEnabled = true;
            MenuLoopEnd.Click += new RoutedEventHandler(MenuLoopEnd_Click);
            MenuControl.Items.Add(MenuLoopEnd);
            MenuClearLoop.Header = FindResource("清除循环");
            MenuClearLoop.InputGestureText = "";
            MenuClearLoop.IsEnabled = true;
            MenuClearLoop.Click += new RoutedEventHandler(MenuClearLoop_Click);
            MenuControl.Items.Add(MenuClearLoop);
            //分隔线
            MenuControl.Items.Add(separator4);
            MenuLRFlip.Header = FindResource("左右翻转");
            MenuLRFlip.InputGestureText = "M";
            MenuLRFlip.Click += new RoutedEventHandler(MenuLRFlip_Click);
            MenuControl.Items.Add(MenuLRFlip);
            MenuUDFlip.Header = FindResource("上下翻转");
            MenuUDFlip.InputGestureText = "N";
            MenuUDFlip.Click += new RoutedEventHandler(MenuUDFlip_Click);
            MenuControl.Items.Add(MenuUDFlip);
            MenuClockwise.Header = FindResource("顺时针旋转");
            MenuClockwise.InputGestureText = "E";
            MenuClockwise.Click += new RoutedEventHandler(MenuClockwise_Click);
            MenuControl.Items.Add(MenuClockwise);
            MenuCClockwise.Header = FindResource("逆时针旋转");
            MenuCClockwise.InputGestureText = "Q";
            MenuCClockwise.Click += new RoutedEventHandler(MenuCClockwise_Click);
            MenuControl.Items.Add(MenuCClockwise);
            //分隔线
            MenuControl.Items.Add(separator5);
            MenuFullScreen.Header = FindResource("全屏显示");
            MenuFullScreen.InputGestureText = FindResource("双击左键").ToString();
            MenuFullScreen.Click += new RoutedEventHandler(MenuFullScreen_Click);
            MenuControl.Items.Add(MenuFullScreen);
            //分隔线
            MenuControl.Items.Add(separator6);
            MenuSingle.Header = FindResource("单次");
            MenuSingle.InputGestureText = "Ctrl+1";
            MenuSingle.Click += new RoutedEventHandler(MenuSingle_Click);
            MenuControl.Items.Add(MenuSingle);
            MenuLoop.Header = FindResource("循环");
            MenuLoop.InputGestureText = "Ctrl+2";
            MenuLoop.Click += new RoutedEventHandler(MenuLoop_Click);
            MenuControl.Items.Add(MenuLoop);
            MenuOrder.Header = FindResource("顺序");
            MenuOrder.InputGestureText = "Ctrl+3";
            MenuOrder.Click += new RoutedEventHandler(MenuOrder_Click);
            MenuControl.Items.Add(MenuOrder);
            MenuRandom.Header = FindResource("随机");
            MenuRandom.InputGestureText = "Ctrl+4";
            MenuRandom.Click += new RoutedEventHandler(MenuRandom_Click);
            MenuControl.Items.Add(MenuRandom);

            //二级菜单 MenuWindow
            MenuHideControl.Header = FindResource("隐藏控制台");
            MenuHideControl.InputGestureText = "C";
            MenuHideControl.IsEnabled = true;
            MenuHideControl.Click += new RoutedEventHandler(MenuHideControl_Click);
            MenuWindow.Items.Add(MenuHideControl);
            MenuHideList.Header = FindResource("隐藏播放列表");
            MenuHideList.InputGestureText = "L";
            MenuHideList.IsEnabled = true;
            MenuHideList.Click += new RoutedEventHandler(MenuHideList_Click);
            MenuWindow.Items.Add(MenuHideList);

            //二级菜单 MenuHelp
            MenuPreference.Header = FindResource("偏好设置");
            MenuPreference.InputGestureText = "F1";
            MenuPreference.Click += new RoutedEventHandler(MenuPreference_Click);
            MenuHelp.Items.Add(MenuPreference);
            MenuAbout.Header = FindResource("软件信息");
            MenuAbout.InputGestureText = "F2";
            MenuAbout.Click += new RoutedEventHandler(MenuAbout_Click);
            MenuHelp.Items.Add(MenuAbout);
            MenuLink.Header = FindResource("相关链接");
            MenuLink.InputGestureText = "F3";
            MenuLink.Click += new RoutedEventHandler(MenuLink_Click);
            MenuHelp.Items.Add(MenuLink);

            //绑定右键菜单
            MainGrid.ContextMenu = CM;
        }
        /// <summary>
        /// 更改主窗口右键菜单和窗口控件状态
        /// </summary>
        /// <param name="state"></param>
        public void ChangeElementState(int state)
        {
            if (state == 0)
            {
                //右键按钮可用性
                MenuPlay.IsEnabled = false;
                //MenuStop.IsEnabled = false;
                MenuReplay.IsEnabled = false;
                MenuForward.IsEnabled = false;
                MenuBack.IsEnabled = false;
                MenuLoopStart.IsEnabled = false;
                MenuLoopEnd.IsEnabled = false;
                MenuClearLoop.IsEnabled = false;
                //按钮可用性
                ButtonPlay.IsEnabled = false;
                ButtonReplay.IsEnabled = false;
                ButtonForward.IsEnabled = false;
                ButtonBack.IsEnabled = false;
                ButtonSetLoopStart.IsEnabled = false;
                ButtonSetLoopEnd.IsEnabled = false;
                ButtonClearLoop.IsEnabled = false;
            }
            else
            {
                //右键按钮可用性
                MenuPlay.IsEnabled = true;
                //MenuStop.IsEnabled = true;
                MenuReplay.IsEnabled = true;
                MenuForward.IsEnabled = true;
                MenuBack.IsEnabled = true;
                MenuLoopStart.IsEnabled = true;
                MenuLoopEnd.IsEnabled = true;
                MenuClearLoop.IsEnabled = true;
                //按钮可用性
                ButtonPlay.IsEnabled = true;
                ButtonReplay.IsEnabled = true;
                ButtonForward.IsEnabled = true;
                ButtonBack.IsEnabled = true;
                ButtonSetLoopStart.IsEnabled = true;
                ButtonSetLoopEnd.IsEnabled = true;
                ButtonClearLoop.IsEnabled = true;
            }
        }
        public void ShowPlayMode(int mode, bool showMessage)
        {
            switch (mode)
            {
                case 0:
                    MenuSingle.IsChecked = true;
                    MenuLoop.IsChecked = false;
                    MenuOrder.IsChecked = false;
                    MenuRandom.IsChecked = false;
                    ButtonPlayMode.Content = FindResource("单次");
                    if (showMessage)
                    {
                        //显示消息
                        ShowMessage("播放模式切换", "：" + FindResource("单次").ToString(), false);
                     }
                    break;
                case 1:
                    MenuSingle.IsChecked = false;
                    MenuLoop.IsChecked = true;
                    MenuOrder.IsChecked = false;
                    MenuRandom.IsChecked = false;
                    ButtonPlayMode.Content = FindResource("循环");
                    if (showMessage)
                    {
                        //显示消息
                        ShowMessage("播放模式切换", "：" + FindResource("循环").ToString(), false);
                    }
                    break;
                case 2:
                    MenuSingle.IsChecked = false;
                    MenuLoop.IsChecked = false;
                    MenuOrder.IsChecked = true;
                    MenuRandom.IsChecked = false;
                    ButtonPlayMode.Content = FindResource("顺序");
                    if (showMessage)
                    {
                        //显示消息
                        ShowMessage("播放模式切换", "：" + FindResource("顺序").ToString(), false);
                    }
                    break;
                case 3:
                    MenuSingle.IsChecked = false;
                    MenuLoop.IsChecked = false;
                    MenuOrder.IsChecked = false;
                    MenuRandom.IsChecked = true;
                    ButtonPlayMode.Content = FindResource("随机");
                    if (showMessage)
                    {
                        //显示消息
                        ShowMessage("播放模式切换", "：" + FindResource("随机").ToString(), false);
                    }
                    break;
            }
        }
        /// <summary>
        /// 实例化帮助窗口，载入设置信息
        /// </summary>
        /// <param name="tab">选择打开的标签</param>
        public void OpenHelp(int tab)
        {
            HelpWindow Help = new HelpWindow(this);
            Help.Show();
            Help.MainTab.SelectedIndex = tab - 1;
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="resourceName">资源名</param>
        /// <param name="newBox">是否弹出对话框</param>
        public void ShowMessage(string resourceName, bool newBox)
        {
            if (newBox)
            {
                System.Windows.MessageBox.Show(FindResource(resourceName).ToString());
            }
            else
            {
                Message.Opacity = 1;
                Message.Content = FindResource(resourceName);
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
                System.Windows.MessageBox.Show(FindResource(resourceName) + moreText);
            }
            else
            {
                Message.Opacity = 1;
                Message.Content = FindResource(resourceName) + moreText;
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
        /// 计时器1，进度条绑定媒体播放进度
        /// </summary>
        void Timer1_Tick(object sender, EventArgs e)
        {
            TimePosition.Content = Media.Position;
            TimeSlider.Value = Media.Position.TotalSeconds;
        }
        /// <summary>
        /// 计时器2，消息透明度降低
        /// </summary>
        void Timer2_Tick(object sender, EventArgs e)
        {
            Message.Opacity -= 0.01;
            Loop();
        }
        /// <summary>
        /// 计时器3，控制区域透明度降低
        /// </summary>
        void Timer3_Tick(object sender, EventArgs e)
        {
            ConsoleGrid.Opacity -= 0.01;
        }
    }
}
