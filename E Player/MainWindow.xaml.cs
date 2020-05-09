using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Drawing.Imaging;
using Tags.ID3;
using MessageBox = System.Windows.MessageBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using DragEventArgs = System.Windows.DragEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Path = System.IO.Path;
using SharedProject;

namespace E.Player
{
    public partial class MainWindow : EWindow
    {
        private Uri CurrentMedia { get => MetMedia.Source; }
        private bool IsPlaying { get; set; } = false;
        private bool HasStartPosition { get; set; } = false;
        private bool HasEndPosition { get; set; } = false;
        private TimeSpan StartPosition { get; set; } = TimeSpan.Zero;
        private TimeSpan EndPosition { get; set; } = TimeSpan.Zero;
        private Rect LastRect;

        //计时器
        private DispatcherTimer timerSldLoop;
        private DispatcherTimer timerSldTime;

        //构造
        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(string[] args) : this()
        {
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    Uri uir = new Uri(args[i]);
                    if (i == 0)
                    {
                        OpenMedia(args[0]);
                    }
                    else
                    {
                        AddPlayListItem(uir, false);
                    }
                }
            }
        }

        //载入
        private void LoadVideoItems()
        {
            //读取一个字符串，并加入播放列表
            if (!string.IsNullOrEmpty(Settings.Default.FileList))
            {
                string[] _myB = Regex.Split(Settings.Default.FileList, "///");
                foreach (string b in _myB)
                {
                    try
                    {
                        Uri uir = new Uri(b);
                        AddPlayListItem(uir, false);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        //打开
        private void OpenMedia()
        {
            string path = GetFilePath();
            if (File.Exists(path))
            {
                Uri uir = new Uri(path);
                AddPlayListItem(uir, true);
            }
        }
        public void OpenMedia(string path)
        {
            if (File.Exists(path))
            {
                Uri uir = new Uri(path);
                AddPlayListItem(uir, true);
            }
        }

        //关闭
        private void CloseMedia()
        {
            MetMedia.Close();
            MetMedia.Source = null;
            IsPlaying = false;
            ImgCover.IsEnabled = false;
            LblAllTime.Content = "00:00:00";

            BtnPlay.Content = FindResource("播放");

            RefreshBtnsState();
        }

        //保存
        protected override void SaveSettings()
        {
            if (!Settings.Default.IsRecordFileList)
            {
                Settings.Default.FileList = "";
            }
            else
            {
                Settings.Default.FileList = "";
                if (LtbFile.Items.Count > 0)
                {
                    List<string> medias = new List<string>();
                    foreach (ListBoxItem item in LtbFile.Items)
                    {
                        medias.Add(item.Tag.ToString());
                    }
                    Settings.Default.FileList = string.Join("///", medias);
                }
            }
            if (!Settings.Default.IsRecordPlayMode)
            {
                Settings.Default.PlayMode = 0;
            }
            if (!Settings.Default.IsRecordVolume)
            {
                Settings.Default.Volume = 0.5;
            }
            if (!Settings.Default.IsRecordSpeed)
            {
                Settings.Default.Speed = 1;
            }
            if (!Settings.Default.IsRecordFlip)
            {
                Settings.Default.FlipLR = false;
                Settings.Default.FlipUD = false;
            }
            if (!Settings.Default.IsRecordRotation)
            {
                Settings.Default.Rotation = 0;
            }
            if (!Settings.Default.IsRecordJumpTime)
            {
                Settings.Default.JumpTime = 5;
            }

            Settings.Default.Save();
            ShowMessage(FindResource("已保存").ToString());
        }

        //创建
        private void CreateTimer()
        {
            Loaded += new RoutedEventHandler(TimertimerSldLoop_Tick);
            timerSldLoop = new DispatcherTimer
            { Interval = TimeSpan.FromSeconds(0.02) };
            timerSldLoop.Tick += new EventHandler(TimertimerSldLoop_Tick);
            timerSldLoop.Start();

            Loaded += new RoutedEventHandler(TimerSldTime_Tick);
            timerSldTime = new DispatcherTimer
            { Interval = TimeSpan.FromSeconds(0.05) };
            timerSldTime.Tick += new EventHandler(TimerSldTime_Tick);
            timerSldTime.Start();
        }

        //添加
        private void AddMedia()
        {
            string path = GetFilePath();
            if (File.Exists(path))
            {
                Uri uir = new Uri(path);
                AddPlayListItem(uir, false);
            }
        }
        private void AddPlayListItem(Uri uri, bool isPlay)
        {
            if (uri != null)
            {
                ListBoxItem PlayListBoxItem = new ListBoxItem
                {
                    Name = "VideoItem",
                    Tag = uri.LocalPath,
                    ToolTip = uri.LocalPath,
                    Content = GetMediaName(uri.LocalPath),
                    Style = (Style)FindResource("列表子项样式"),
                };
                //添加鼠标事件
                PlayListBoxItem.AddHandler(MouseDownEvent, new MouseButtonEventHandler(LtbFileItem_MouseDown), true);

                //添加Item时检测是否有重复视频
                bool isThere = false;
                if (LtbFile.Items.Count == 0)
                {
                    //增加第一个Item进PlayListBox
                    LtbFile.Items.Add(PlayListBoxItem);
                    LtbFile.ToolTip = FindResource("媒体总数") + "：" + LtbFile.Items.Count;
                    LtbFile.SelectedIndex = 0;
                    if (isPlay == true)
                    {
                        SetMediaAndPlay(uri);
                    }
                }
                else
                {
                    ListBoxItem tempItem = new ListBoxItem();
                    for (int i = 0; i < LtbFile.Items.Count; i++)
                    {
                        tempItem = LtbFile.Items.GetItemAt(i) as ListBoxItem;
                        if (uri.LocalPath == tempItem.Tag.ToString())
                        {
                            isThere = true;
                            break;
                        }
                        else
                        {
                            isThere = false;
                        }
                    }

                    if (isThere == false)
                    {
                        //增加第一个Item进PlayListBox
                        LtbFile.Items.Add(PlayListBoxItem);
                        LtbFile.ToolTip = FindResource("媒体总数") + "：" + LtbFile.Items.Count;
                        LtbFile.SelectedItem = PlayListBoxItem;
                        if (isPlay == true)
                        {
                            SetMediaAndPlay(uri);
                        }
                    }
                    else
                    {
                        ShowMessage(FindResource("视频重复").ToString(), true);
                    }
                }
            }
        }

        //移除
        private void RemoveMedia()
        {
            //获取当前选择的Item在PlayListBox的索引
            ListBoxItem tempItem;
            tempItem = LtbFile.SelectedItem as ListBoxItem;
            if (tempItem != null)
            {
                if (CurrentMedia != null)
                {
                    if (CurrentMedia.LocalPath == tempItem.Tag.ToString())
                    {
                        CloseMedia();
                        ShowMessage(FindResource("删除视频").ToString());
                    }
                }
                int s = LtbFile.SelectedIndex;
                LtbFile.Items.Remove(LtbFile.SelectedItem);
                LtbFile.SelectedIndex = s;
                if (s == LtbFile.Items.Count - 1)
                {
                    LtbFile.SelectedIndex = s - 1;
                }
                RefreshTitle();
                LtbFile.ToolTip = FindResource("媒体总数") + "：" + LtbFile.Items.Count;
            }
            else
            {
                ShowMessage(FindResource("没有媒体").ToString());
            }
        }
        private void RemoveLoop()
        {
            if (HasEndPosition == true || HasStartPosition == true)
            {
                StartPosition = TimeSpan.Zero;
                EndPosition = TimeSpan.Zero;
                HasStartPosition = false;
                HasEndPosition = false;

                //滑块高亮区域
                SldTime.SelectionStart = 0;
                SldTime.SelectionEnd = 0;
                //显示消息
                ShowMessage(FindResource("已清除循环").ToString());
            }
        }

        //清空
        private void ClearMediaList()
        {
            CloseMedia();
            LtbFile.Items.Clear();

            ShowMessage(FindResource("已清空媒体列表").ToString());
        }

        //获取
        private string GetMediaName(string path)
        {
            string name = Path.GetFileName(path);
            return name;
        }
        private string GetFilePath()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Video File(*.avi;*.mp4;*.mkv;*.wmv;*.wma;*.wav;*.mp3;*.aac;*.flac)|" +
                                    "*.avi;*.mp4;*.mkv;*.wmv;*.wma;*.wav;*.mp3;*.aac;*.flac|" +
                         "All File(*.*)|" +
                                  "*.*"
            };
            dialog.ShowDialog();
            return dialog.FileName;
        }
        private List<BitmapImage> GetMP3Cover(string MP3path)
        {
            try
            {
                List<BitmapImage> list_bitmapimg = new List<BitmapImage>();
                ID3Info File = new ID3Info(MP3path, true);

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
                MessageBox.Show(e.Message);
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
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.文件:
                    PanFile.Visibility = Visibility.Visible;
                    PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.BorderThickness = new Thickness(4, 0, 0, 0);
                    BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.编辑:
                    PanFile.Visibility = Visibility.Collapsed;
                    PanEdit.Visibility = Visibility.Visible;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnEdit.BorderThickness = new Thickness(4, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.设置:
                    PanFile.Visibility = Visibility.Collapsed;
                    PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Visible;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(4, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.关于:
                    PanFile.Visibility = Visibility.Collapsed;
                    PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Visible;
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(4, 0, 0, 0);
                    break;
                default:
                    break;
            }
            CurrentMenuTool = menu;
        }
        private void SetMenuToolControl(bool isShow)
        {
            PanControl.Opacity = isShow ? 1 : 0;
        }
        private void SetPlayMode(int index)
        {
            Settings.Default.PlayMode = index;
        }

        /// <summary>
        /// 设置媒体并播放
        /// </summary>
        /// <param name="path">媒体路径</param>
        private void SetMediaAndPlay(Uri uri)
        {
            //获取路径开始播放
            MetMedia.Source = uri;
            if (MetMedia.HasVideo)
            {
            }
            else
            {
                //若播放音频，显示封面
                ShowMP3Cover(uri.LocalPath);
            }
            MetMedia.Play();
            IsPlaying = true;

            MetMedia.ScrubbingEnabled = true;
            BtnPlay.Content = FindResource("暂停").ToString();

            RefreshBtnsState();
            ShowMessage(FindResource("开始播放").ToString() + "：" + GetMediaName(uri.LocalPath));
        }
        /// <summary>
        /// 设置媒体并播放
        /// </summary>
        /// <param name="index"></param>
        private void SetMediaAndPlay(int index)
        {
            ListBoxItem nextItem = LtbFile.Items.GetItemAt(index) as ListBoxItem;
            LtbFile.SelectedIndex = index;
            Uri uri = new Uri(nextItem.Tag.ToString());
            SetMediaAndPlay(uri);
        }
        /// <summary>
        /// 设置循环段开始
        /// </summary>
        private void SetLoopStart()
        {
            //获取当前位置
            TimeSpan tempPosition = MetMedia.Position;
            if (tempPosition < EndPosition || HasEndPosition == false)
            {
                StartPosition = tempPosition;
                string str = StartPosition.ToString();
                //滑块高亮区域
                SldTime.SelectionStart = StartPosition.TotalSeconds;
                HasStartPosition = true;

                //显示消息
                ShowMessage(FindResource("循环始于").ToString() + "：" + str.Substring(0, 8));
            }
            else
            {
                //显示消息
                ShowMessage(FindResource("开始点应靠前").ToString());
            }
        }
        /// <summary>
        /// 设置循环段结束
        /// </summary>
        private void SetLoopEnd()
        {
            //获取当前位置
            TimeSpan tempPosition = MetMedia.Position;
            if (StartPosition < tempPosition)
            {
                EndPosition = tempPosition;
                string str = EndPosition.ToString();
                //滑块高亮区域
                SldTime.SelectionEnd = EndPosition.TotalSeconds;
                HasEndPosition = true;

                //显示消息
                ShowMessage(FindResource("循环终于").ToString() + "：" + str.Substring(0, 8));
            }
            else
            {
                //显示消息
                ShowMessage(FindResource("结束点应靠后").ToString());
            }
        }
        /// <summary>
        /// 设置左右翻转
        /// </summary>
        private void SetLRFlip(bool value)
        {
            Settings.Default.FlipLR = value;
        }
        /// <summary>
        /// 设置上下翻转
        /// </summary>
        private void SetUDFlip(bool value)
        {
            Settings.Default.FlipUD = value;
        }
        /// <summary>
        /// 顺时针旋转90度
        /// </summary>
        private void SetRotationCW()
        {
            Settings.Default.Rotation++;
            if (Settings.Default.Rotation > 3)
            {
                Settings.Default.Rotation = 0;
            }
        }
        /// <summary>
        /// 逆时针旋转90度
        /// </summary>
        private void SetRotationCCW()
        {
            Settings.Default.Rotation--;
            if (Settings.Default.Rotation < 0)
            {
                Settings.Default.Rotation = 3;
            }
        }
        /// <summary>
        /// 设置屏幕模式
        /// </summary>
        private void SetScreenMode(bool value)
        {
            CcbScreenMode.IsChecked = value;
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

        //检查
        private bool IsPathRight(string path)
        {
            //检测路径是否存在
            if (File.Exists(path))
            {
                //获取文件扩展名
                string _videoExtension = Path.GetExtension(path);
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
                    ShowMessage(FindResource("无法播放").ToString(), true);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        //刷新
        protected override void RefreshTitle()
        {
            base.RefreshTitle();

            if (CurrentMedia != null)
            {
                Main.Title += " - " + GetMediaName(CurrentMedia.LocalPath);
            }
        }
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
        private void RefreshBtnsState()
        {
            if (CurrentMedia == null)
            {
                BtnLast.IsEnabled = false;
                BtnNext.IsEnabled = false;
                BtnStop.IsEnabled = false;
                BtnPlay.IsEnabled = false;
                BtnReplay.IsEnabled = false;
                BtnForward.IsEnabled = false;
                BtnBack.IsEnabled = false;

                BtnSetLoopStart.IsEnabled = false;
                BtnSetLoopEnd.IsEnabled = false;
                BtnClearLoop.IsEnabled = false;

                SldTime.IsEnabled = false;
            }
            else
            {
                BtnLast.IsEnabled = true;
                BtnNext.IsEnabled = true;
                BtnStop.IsEnabled = true;
                BtnPlay.IsEnabled = true;
                BtnReplay.IsEnabled = true;
                BtnForward.IsEnabled = true;
                BtnBack.IsEnabled = true;

                BtnSetLoopStart.IsEnabled = true;
                BtnSetLoopEnd.IsEnabled = true;
                BtnClearLoop.IsEnabled = true;

                SldTime.IsEnabled = true;
            }
        }
        /// <summary>
        /// 刷新翻转状态
        /// </summary>
        private void RefreshFlip()
        {
            int x = Settings.Default.FlipLR ? -1 : 1;
            int y = Settings.Default.FlipUD ? -1 : 1;

            ScaleTransform scaleTransform = new ScaleTransform
            {
                ScaleX = x,
                ScaleY = y
            };
            if (MetMedia != null)
            {
                MetMedia.LayoutTransform = scaleTransform;
            }
        }
        /// <summary>
        /// 刷新旋转状态
        /// </summary>
        private void RefreshRotation()
        {
            if (MetMedia != null)
            {
                MetMedia.LayoutTransform = new RotateTransform(90 * Settings.Default.Rotation);
            }
        }
        /// <summary>
        /// 刷新屏幕模式
        /// </summary>
        private void RefreshScreenMode()
        {
            if (CcbScreenMode.IsChecked == true)
            {
                LastRect.X = Left;
                LastRect.Y = Top;
                LastRect.Width = Width;
                LastRect.Height = Height;

                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Topmost = true;

                Left = 0;
                Top = 0;
                Width = SystemParameters.PrimaryScreenWidth;
                Height = SystemParameters.PrimaryScreenHeight;
            }
            else
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResize;
                Topmost = false;

                Left = LastRect.X;
                Top = LastRect.Y;
                Width = LastRect.Width;
                Height = LastRect.Height;
            }
        }
        /// <summary>
        /// 刷新菜单
        /// </summary>
        private void RefreshMenuVisibility()
        {
            PanMenu.Visibility = CcbMenuVisibility.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        //显示
        protected override void ShowMessage(string message, bool newBox = false)
        {
            ShowMessage(LblMessage, message, newBox);
        }
        private void ShowMP3Cover(string path)
        {
            if (Path.GetExtension(path) == ".mp3")
            {
                ImgCover.Source = null;
                ImgCover.IsEnabled = true;
                List<BitmapImage> images = GetMP3Cover(path);
                if (images != null)
                {
                    if (images.Count > 0)
                    {
                        ImgCover.Source = images[0];
                    }
                }
            }
            else
            {
                ImgCover.Source = null;
                ImgCover.IsEnabled = false;
            }
        }

        //切换
        private void SwichPlayAndPauseMedia()
        {
            if (!IsPlaying)
            {
                MetMedia.Play();
                BtnPlay.Content = FindResource("暂停");
                ShowMessage(FindResource("开始播放").ToString());
            }
            else
            {
                MetMedia.Pause();
                BtnPlay.Content = FindResource("播放");
                ShowMessage(FindResource("暂停播放").ToString());
            }
            IsPlaying = !IsPlaying;
        }

        //其它
        /// <summary>
        /// 重播
        /// </summary>
        private void ReplayMedia()
        {
            MetMedia.Stop();
            MetMedia.Play();
            IsPlaying = true;
            BtnPlay.Content = FindResource("暂停");
            //显示消息
            ShowMessage(FindResource("开始播放").ToString());
        }
        /// <summary>
        /// 上一个
        /// </summary>
        private void PlayLastMedia()
        {
            int count = LtbFile.Items.Count;
            if (count == 0)
            {
                ShowMessage(FindResource("没有媒体").ToString());
                return;
            }

            int targetIndex;
            if (CurrentMedia == null)
            {
                targetIndex = 0;
            }
            else
            {
                int currentIndex = 0;
                ListBoxItem tempItem;
                for (int i = 0; i < LtbFile.Items.Count; i++)
                {
                    tempItem = LtbFile.Items.GetItemAt(i) as ListBoxItem;
                    if (CurrentMedia.LocalPath == tempItem.Tag.ToString())
                    {
                        currentIndex = i;
                        break;
                    }
                }
                targetIndex = currentIndex - 1;
            }

            if (targetIndex < 0)
            {
                ShowMessage(FindResource("已经是第一个媒体").ToString());
            }
            else
            {
                SetMediaAndPlay(targetIndex);
            }
        }
        /// <summary>
        /// 下一个
        /// </summary>
        private void PlayNextMedia()
        {
            int count = LtbFile.Items.Count;
            if (count == 0)
            {
                ShowMessage(FindResource("没有媒体").ToString());
                return;
            }

            int targetIndex;
            if (CurrentMedia == null)
            {
                targetIndex = 0;
            }
            else
            {
                int currentIndex = 0;
                ListBoxItem tempItem;
                for (int i = 0; i < LtbFile.Items.Count; i++)
                {
                    tempItem = LtbFile.Items.GetItemAt(i) as ListBoxItem;
                    if (CurrentMedia.LocalPath == tempItem.Tag.ToString())
                    {
                        currentIndex = i;
                        break;
                    }
                }
                targetIndex = currentIndex + 1;
            }

            if (targetIndex >= LtbFile.Items.Count)
            {
                CloseMedia();
                ShowMessage(FindResource("全部播放完毕").ToString());
            }
            else
            {
                SetMediaAndPlay(targetIndex);
            }
        }
        /// <summary>
        /// 随机下一个
        /// </summary>
        private void PlayRamdomNextMedia()
        {
            int count = LtbFile.Items.Count;
            if (count == 0)
            {
                ShowMessage(FindResource("没有媒体").ToString());
                return;
            }

            Random ran = new Random();
            int targetIndex = ran.Next(0, LtbFile.Items.Count);
            SetMediaAndPlay(targetIndex);
        }
        /// <summary>
        /// 停止
        /// </summary>
        private void StopMedia()
        {
            MetMedia.Stop();
            IsPlaying = false;
            BtnPlay.Content = FindResource("播放");
            ShowMessage(FindResource("停止播放").ToString());
        }
        /// <summary>
        /// 快进
        /// </summary>
        private void Forward()
        {
            if (CurrentMedia == null)
            {
                return;
            }
            if (File.Exists(CurrentMedia.LocalPath))
            {
                MetMedia.Position += TimeSpan.FromSeconds(Settings.Default.JumpTime);
                //if (MetMedia.Position > MetMedia.NaturalDuration.TimeSpan)
                //{
                //    MetMedia.Position = MetMedia.NaturalDuration.TimeSpan;
                //}
                string str = MetMedia.Position.ToString();
                ShowMessage(FindResource("当前进度").ToString() + "：" + str.Substring(0, 8));
            }
        }
        /// <summary>
        /// 快退
        /// </summary>
        private void Back()
        {
            if (CurrentMedia == null)
            {
                return;
            }
            if (File.Exists(CurrentMedia.LocalPath))
            {
                MetMedia.Position -= TimeSpan.FromSeconds(Settings.Default.JumpTime);
                if (MetMedia.Position <= TimeSpan.Zero)
                {
                    MetMedia.Position = TimeSpan.Zero;
                }
                string str = MetMedia.Position.ToString();
                ShowMessage(FindResource("当前进度").ToString() + "：" + str.Substring(0, 8));
            }
        }
        /// <summary>
        /// 音量升高
        /// </summary>
        private void VolumeUp()
        {
            double temp = MetMedia.Volume + 0.1;
            if (temp > 1)
            {
                temp = 1;
            }
            temp = Math.Round(temp, 1);
            MetMedia.Volume = temp;
            SldVolume.Value = temp;
            ShowMessage(FindResource("当前音量").ToString() + "：" + temp);
        }
        /// <summary>
        /// 音量降低
        /// </summary>
        private void VolumeDown()
        {
            double temp = MetMedia.Volume - 0.1;
            if (temp < 0)
            {
                temp = 0;
            }
            temp = Math.Round(temp, 1);
            MetMedia.Volume = temp;
            SldVolume.Value = temp;
            ShowMessage(FindResource("当前音量").ToString() + "：" + temp);
        }
        /// <summary>
        /// 速度增加
        /// </summary>
        private void SpeedUp()
        {
            double temp = MetMedia.SpeedRatio + 0.1;
            if (temp > 2)
            {
                temp = 2;
            }
            temp = Math.Round(temp, 1);
            MetMedia.SpeedRatio = temp;
            SldSpeed.Value = temp;
            ShowMessage(FindResource("播放速度").ToString() + "：" + temp);
        }
        /// <summary>
        /// 速度减少
        /// </summary>
        private void SpeedDown()
        {
            double temp = MetMedia.SpeedRatio - 0.1;
            if (temp < 0.1)
            {
                temp = 0.1;
            }
            temp = Math.Round(temp, 1);
            MetMedia.SpeedRatio = temp;
            SldSpeed.Value = temp;
            ShowMessage(FindResource("播放速度").ToString() + "：" + temp);
        }
        /// <summary>
        /// 到达循环结束点时的动作
        /// </summary>
        private void Loop()
        {
            if (HasEndPosition && StartPosition < EndPosition)
            {
                if (MetMedia.Position > EndPosition)
                {
                    MetMedia.Position = StartPosition;
                }
                if (MetMedia.Position < StartPosition)
                {
                    MetMedia.Position = StartPosition;
                }
            }
        }

        //主窗口
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            //载入
            LoadLanguageItems(CbbLanguages);
            LoadThemeItems(CbbThemes);
            LoadVideoItems();

            //初始化
            CreateTimer();

            //刷新
            RefreshAppInfo();
            RefreshBtnsState();
            RefreshTitle();
            RefreshRotation();
            RefreshFlip();

            //检查用户是否同意用户协议
            if (CheckUserAgreement(Settings.Default.RunCount))
            {
                Settings.Default.RunCount += 1;
            }
        }
        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.FileList = "";
            SaveSettings();
        }
        protected override void EWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                //Ctrl +
                //1 
                if (e.Key == Key.D1)
                {
                    SetPlayMode(0);
                    SetPlayMode(Settings.Default.PlayMode);
                    ShowMessage(FindResource("播放模式切换").ToString() + "：" + FindResource("单次").ToString());
                }
                //2
                else if (e.Key == Key.D2)
                {
                    SetPlayMode(1);
                    SetPlayMode(Settings.Default.PlayMode);
                    ShowMessage(FindResource("播放模式切换").ToString() + "：" + FindResource("循环").ToString());
                }
                //3 
                else if (e.Key == Key.D3)
                {
                    SetPlayMode(2);
                    SetPlayMode(Settings.Default.PlayMode);
                    ShowMessage(FindResource("播放模式切换").ToString() + "：" + FindResource("顺序").ToString());
                }
                //4
                else if (e.Key == Key.D4)
                {
                    SetPlayMode(3);
                    SetPlayMode(Settings.Default.PlayMode);
                    ShowMessage(FindResource("播放模式切换").ToString() + "：" + FindResource("随机").ToString());
                }
            }
            else
            {
                //1 
                if (e.Key == Key.D1)
                {
                    SwitchMenuToolFile();
                }
                //2
                else if (e.Key == Key.D2)
                {
                    SwitchMenuToolEdit();
                }
                //3 
                else if (e.Key == Key.D3)
                {
                    SwitchMenuToolSetting();
                }
                //4
                else if (e.Key == Key.D4)
                {
                    SwitchMenuToolAbout();
                }
            }

            //O键打开媒体文件
            if (e.Key == Key.O)
            {
                OpenMedia();
            }
            //A键添加媒体文件
            else if (e.Key == Key.A)
            {
                AddMedia();
            }
            //按Esc键退出全屏
            else if (e.Key == Key.Escape)
            {
                SetScreenMode(false);
            }
            //Delete键删除视频
            else if (e.Key == Key.Delete)
            {
                RemoveMedia();
            }
            //上一个
            else if (e.Key == Key.PageUp)
            {
                PlayLastMedia();
            }
            //下一个
            else if (e.Key == Key.PageDown)
            {
                PlayNextMedia();
            }
            //M键左右翻转
            else if (e.Key == Key.M)
            {
                SetLRFlip(!Settings.Default.FlipLR);
            }
            //N键上下翻转
            else if (e.Key == Key.N)
            {
                SetUDFlip(!Settings.Default.FlipUD);
            }
            //Q键逆时针
            else if (e.Key == Key.Q)
            {
                SetRotationCCW();
            }
            //E键顺时针
            else if (e.Key == Key.E)
            {
                SetRotationCW();
            }
            //按上下键改变音量
            else if (!e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && !e.KeyboardDevice.IsKeyDown(Key.RightCtrl) && e.Key == Key.Up)
            {
                VolumeUp();
            }
            else if (!e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && !e.KeyboardDevice.IsKeyDown(Key.RightCtrl) && e.Key == Key.Down)
            {
                VolumeDown();
            }
            //按Ctrl+上下键改变播放速度
            else if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.Key == Key.Up)
            {
                SpeedUp();
            }
            else if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.Key == Key.Down)
            {
                SpeedDown();
            }

            if (CurrentMedia != null)
            {
                //按空格键播放和暂停
                if (e.Key == Key.Space)
                {
                    SwichPlayAndPauseMedia();
                }
                //R键重新播放
                else if (e.Key == Key.R)
                {
                    ReplayMedia();
                }
                //按←→键改变进度
                else if (e.Key == Key.Right && File.Exists(CurrentMedia.LocalPath))
                {
                    Forward();
                }
                else if (e.Key == Key.Left && File.Exists(CurrentMedia.LocalPath))
                {
                    Back();
                }
                //按[]键设置循环段开始与结束
                else if (e.Key == Key.OemOpenBrackets && File.Exists(CurrentMedia.LocalPath))
                {
                    SetLoopStart();
                }
                else if (e.Key == Key.OemCloseBrackets && File.Exists(CurrentMedia.LocalPath))
                {
                    SetLoopEnd();
                }
                //按\键清除循环段
                else if (e.Key == Key.OemPipe)
                {
                    RemoveLoop();
                }
            }

            //Ctrl+T 切换下个主题
            if (e.Key == Key.T && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            { SetNextTheme(CbbThemes, Settings.Default.Theme); }

            base.EWindow_KeyUp(sender, e);
        }
        private void Main_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                SetScreenMode(CcbScreenMode.IsChecked == false);
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                CcbMenuVisibility.IsChecked = !CcbMenuVisibility.IsChecked;
            }
        }
        private void TimertimerSldLoop_Tick(object sender, EventArgs e)
        {
            Loop();
        }
        private void TimerSldTime_Tick(object sender, EventArgs e)
        {
            SldTime.Value = MetMedia.Position.TotalSeconds;
            LblCurrentTime.Content = MetMedia.Position.ToString().Substring(0, 8);
        }

        //工具栏
        /// 文件
        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenMedia();
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddMedia();
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            RemoveMedia();
        }
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearMediaList();
        }
        private void LtbFile_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void LtbFile_PreviewMouseMove(object sender, MouseEventArgs e)
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
        private void LtbFile_PreviewDrop(object sender, DragEventArgs e)
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
        private void LtbFile_Drop(object sender, DragEventArgs e)
        {
            //如果有文件拖拽进入
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                //获取该文件路径
                Uri uri = new Uri(((Array)e.Data.GetData(System.Windows.DataFormats.FileDrop)).GetValue(0).ToString());
                //检测路径是否正确
                if (IsPathRight(uri.LocalPath))
                {
                    //在播放列表添加此媒体，不播放
                    AddPlayListItem(uri, false);
                }
            }
        }
        private void LtbFile_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetMediaAndPlay(LtbFile.SelectedIndex);
            }
        }
        private void LtbFileItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                SetMediaAndPlay(LtbFile.SelectedIndex);
            }
        }
        ///编辑
        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            SwichPlayAndPauseMedia();
        }
        private void BtnReplay_Click(object sender, RoutedEventArgs e)
        {
            ReplayMedia();
        }
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            StopMedia();
        }
        private void BtnLast_Click(object sender, RoutedEventArgs e)
        {
            PlayLastMedia();
        }
        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            PlayNextMedia();
        }
        private void BtnForward_Click(object sender, RoutedEventArgs e)
        {
            Forward();
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Back();
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
            RemoveLoop();
        }
        private void BtnCWRotation_Click(object sender, RoutedEventArgs e)
        {
            SetRotationCW();
        }
        private void BtnCCWRotation_Click(object sender, RoutedEventArgs e)
        {
            SetRotationCCW();
        }
        private void CcbUDFlip_Checked(object sender, RoutedEventArgs e)
        {
            RefreshFlip();
        }
        private void CcbUDFlip_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshFlip();
        }
        private void CcbLRFlip_Checked(object sender, RoutedEventArgs e)
        {
            RefreshFlip();
        }
        private void CcbLRFlip_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshFlip();
        }
        private void CcbScreenMode_Checked(object sender, RoutedEventArgs e)
        {
            RefreshScreenMode();
        }
        private void CcbScreenMode_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshScreenMode();
        }
        private void CcbMenuVisibility_Checked(object sender, RoutedEventArgs e)
        {
            RefreshMenuVisibility();
        }
        private void CcbMenuVisibility_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshMenuVisibility();
        }
        private void CbbRotation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshRotation();
        }
        private void SldJumpTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TxtJumpTime.Text = Settings.Default.JumpTime.ToString();
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
                string themePath = cbi.ToolTip.ToString();
                if (File.Exists(themePath))
                {
                    ColorHelper.SetColors(Resources, themePath);
                }
                else
                {
                    CbbThemes.Items.Remove(cbi);
                    //设为默认主题
                    Settings.Default.Theme = 0;
                }
            }
        }
        ///控制
        private void PanControl_MouseEnter(object sender, MouseEventArgs e)
        {
            SetMenuToolControl(true);
        }
        private void PanControl_MouseLeave(object sender, MouseEventArgs e)
        {
            SetMenuToolControl(false);
        }
        private void SldTime_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string str = MetMedia.Position.ToString();
            ShowMessage(FindResource("当前进度").ToString() + "：" + str.Substring(0, 8));
        }
        private void SldVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double temp = Math.Round(SldVolume.Value, 2);
            SldVolume.ToolTip = temp + " ↑/↓";
            ShowMessage(FindResource("当前音量").ToString() + "：" + temp);
        }
        private void SldSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double temp = Math.Round(SldSpeed.Value, 2);
            SldSpeed.ToolTip = temp + " Ctrl + ↑/Ctrl + ↓";
            ShowMessage(FindResource("播放速度").ToString() + "：" + temp);
        }

        //工作区
        private void PanWorkArea_Drop(object sender, DragEventArgs e)
        {
            //如果有文件拖拽进入
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                //获取该文件路径
                Uri uri = new Uri(((Array)e.Data.GetData(System.Windows.DataFormats.FileDrop)).GetValue(0).ToString());
                //检测路径是否正确
                if (IsPathRight(uri.LocalPath))
                {
                    //在播放列表添加此媒体，播放
                    AddPlayListItem(uri, true);
                }
            }
        }
        private void MetMedia_SourceUpdated(object sender, DataTransferEventArgs e)
        {
        }
        private void MetMedia_MediaOpened(object sender, RoutedEventArgs e)
        {
            SldTime.Maximum = MetMedia.NaturalDuration.TimeSpan.TotalSeconds;
            double d0 = SldTime.Maximum / 4;
            double d1 = SldTime.Maximum / 4 * 2;
            double d2 = SldTime.Maximum / 4 * 3;
            SldTime.Ticks = new DoubleCollection(new double[] { d0, d1, d2 });
            LblAllTime.Content = MetMedia.NaturalDuration;
            RefreshTitle();
        }
        private void MetMedia_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ShowMessage("媒体错误");
            RefreshTitle();
        }
        private void MetMedia_MediaEnded(object sender, RoutedEventArgs e)
        {
            //单次
            if (Settings.Default.PlayMode == 0)
            {
                StopMedia();
                ShowMessage(FindResource("播放结束").ToString());
            }
            //循环
            else if (Settings.Default.PlayMode == 1)
            {
                MetMedia.Position = TimeSpan.Zero;
            }
            //顺序
            else if (Settings.Default.PlayMode == 2)
            {
                PlayNextMedia();
            }
            //随机
            else if (Settings.Default.PlayMode == 3)
            {
                PlayRamdomNextMedia();
            }
            RefreshTitle();
        }
        private void MetMedia_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SwichPlayAndPauseMedia();
        }
    }
}
