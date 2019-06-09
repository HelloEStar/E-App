using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;   
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Forms.Application;

namespace E_Updater
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 获取软件信息
        //public static Assembly asm = Assembly.GetExecutingAssembly();
        //public static AssemblyTitleAttribute aTitle = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyTitleAttribute));
        //public static AssemblyDescriptionAttribute aDescription = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyDescriptionAttribute));
        //public static AssemblyCopyrightAttribute aCopyright = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyCopyrightAttribute));
        //public static AssemblyCompanyAttribute aCompany = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyCompanyAttribute));
        //public static AssemblyFileVersionAttribute aFileVersion = (AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyFileVersionAttribute));
        //public static string title = aTitle.Title;                     //软件名
        //public static string description = aDescription.Description;   //软件描述
        //public static string copyright = aCopyright.Copyright;         //软件版权
        //public static string company = aCompany.Company;               //软件开发公司
        #endregion
        public static string title = Application.ProductName;                     //软件名
        //public static string description = System.Windows.Forms.Application.;   //软件描述
        public static string copyright = Application.ProductVersion;         //软件版权
        public static string company = Application.CompanyName;               //软件开发公司
        
        //当前版本
        public Version EUThisVer;
        public Version EWThisVer;
        public Version EPThisVer;
        public Version ENThisVer;
        public Version ELThisVer;
        //最新版本
        public Version EUNewVer;
        public Version EWNewVer;
        public Version EPNewVer;
        public Version ENNewVer;
        public Version ELNewVer;
        
        //模块状态
        public State EUState;
        public State EWState;
        public State EPState;
        public State ENState;
        public State ELState;

        //窗口构造器
        public MainWindow()
        {
            InitializeComponent();
            Properties.Settings.Default._EU = Application.StartupPath;
            Properties.Settings.Default.Save();
            
            //Thread th = new Thread(new ThreadStart(Refresh));
            //th.Start(true);
            Refresh(true);
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
                //System.Windows.MessageBox.Show(FindResource(resourceName).ToString());
                MessageBox.Show(resourceName);
            }
            else
            {
                //Lbl_Message.Opacity = 1;
                //Lbl_Message.Content = FindResource(resourceName);
                Lbl_Message.Dispatcher.Invoke(new Action(() => { Lbl_Message.Content = resourceName; }));
                //Lbl_Message.Content = resourceName;
            }
        }
        
        /// <summary>  
        /// 功能：解压zip格式的文件。  
        /// </summary>  
        /// <param name="zipFilePath">压缩文件路径</param>  
        /// <param name="unZipDir">解压文件存放路径,为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹</param>  
        /// <param name="err">出错信息</param>  
        /// <returns>解压是否成功</returns>  
        public void UnZip(string zipFilePath, string unZipDir)
        {
            //检查错误
            if (zipFilePath == string.Empty)
            {
                //throw new Exception("压缩文件不能为空！");
                return;
            }
            if (!File.Exists(zipFilePath))
            {
                //throw new System.IO.FileNotFoundException("压缩文件不存在！");
                return;
            }
            //文件夹里建一个带有时间戳的文件夹
            //unZipDir += "weather" + DateTime.Now.ToString("yyyyMMddHHmmsss");
            //解压文件夹为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹  
            if (unZipDir == string.Empty)
            { unZipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath)); }
            if (!unZipDir.EndsWith("\\"))
            { unZipDir += "\\"; }
            if (!Directory.Exists(unZipDir))
            { Directory.CreateDirectory(unZipDir); }
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFilePath)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(unZipDir + directoryName);
                        //将解压后的文件放到带时间戳的文件夹里
                    }
                    if (!directoryName.EndsWith("\\"))
                    { directoryName += "\\"; }
                    if (fileName != String.Empty)
                    {
                        using (FileStream streamWriter = File.Create(unZipDir + theEntry.Name))
                        {
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

            }
        }

        /// <summary>
        /// 检测网络
        /// </summary>
        private const int INTERNET_CONNECTION_MODEM = 1;
        private const int INTERNET_CONNECTION_LAN = 2;
        private const int INTERNET_CONNECTION_PROXY = 4;
        private const int INTERNET_CONNECTION_MODEM_BUSY = 8;
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);
        /// <summary>
        /// 检测用户计算机是否已连接网络
        /// </summary>
        /// <returns>是否已连接网络</returns>
        public static bool IsOnLine()
        {
            try
            {
                var netstatus = string.Empty;
                var connection = 0;
                if (!InternetGetConnectedState(out connection, 0)) return false;
                if ((connection & INTERNET_CONNECTION_PROXY) != 0)
                    netstatus += " 采用代理上网  \n";
                if ((connection & INTERNET_CONNECTION_MODEM) != 0)
                    netstatus += " 采用调治解调器上网 \n";
                if ((connection & INTERNET_CONNECTION_LAN) != 0)
                    netstatus += " 采用网卡上网  \n";
                if ((connection & INTERNET_CONNECTION_MODEM_BUSY) != 0)
                    netstatus += " MODEM被其他非INTERNET连接占用  \n";
                Console.WriteLine(netstatus);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">文件路径</param>
        /// <param name="name">保存的文件名</param>
        public void Download(string url, string name)
        {
            //尝试下载
            try
            {
                WebClient webClient = new WebClient();
                //确保下载文件夹存在
                if (!Directory.Exists(Properties.Settings.Default._download))
                { Directory.CreateDirectory(Properties.Settings.Default._download); }
                //设置存储路径
                string address = Properties.Settings.Default._download + @"\" + name;
                if (!File.Exists(address))
                {
                    //开始下载
                    webClient.DownloadFile(new Uri(url), address);
                    ShowMessage(name + "下载成功", false);
                    if (MessageBox.Show(name + " 已完成下载，是否打开下载文件夹", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Process.Start("explorer.exe", Properties.Settings.Default._download);
                    }
                }
                else
                {
                    if (MessageBox.Show("下载文件夹内含有同名文件，替换此文件？", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        //开始下载
                        webClient.DownloadFile(new Uri(url), address);
                        ShowMessage(name + "下载成功", false);
                        if (MessageBox.Show(name + " 已完成下载，是否打开下载文件夹", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            Process.Start("explorer.exe", Properties.Settings.Default._download);
                        }
                    }
                }
            }
            catch (Exception)
            {
                ShowMessage(name + "下载失败", false);
            }
        }

        /// <summary>
        /// 检测所有目录是否存在
        /// </summary>
        public void LoadAllPath()
        {
            LoadPath(Properties.Settings.Default._EU, TBx_EUPath, ref EUState);
            LoadPath(Properties.Settings.Default._EW, TBx_EWPath, ref EWState);
            LoadPath(Properties.Settings.Default._EP, TBx_EPPath, ref EPState);
            LoadPath(Properties.Settings.Default._EN, TBx_ENPath, ref ENState);
            LoadPath(Properties.Settings.Default._EL, TBx_ELPath, ref ELState);
        }
        /// <summary>
        /// 检测指定目录是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tb"></param>
        public void LoadPath(string dir, System.Windows.Controls.TextBox tb, ref State state)
        {
            if (Directory.Exists(dir))
            {
                tb.Text = dir;
                state = State.HasNotEXE;
            }
            else
            {
                tb.Text = "未指定";
                state = State.HasNotSet;
            }
            tb.ToolTip = "没有可执行文件";
        }
        /// <summary>
        /// 检测所有目录是否有可执行文件，并获取其版本信息
        /// </summary>
        public void GetAllThisVersion()
        {
            GetThisVersion(Properties.Settings.Default._EU, "\\E Updater.exe", Lbl_EUThisVer, ref EUThisVer, ref EUState);
            GetThisVersion(Properties.Settings.Default._EW, "\\E Writer.exe", Lbl_EWThisVer, ref EWThisVer, ref EWState);
            GetThisVersion(Properties.Settings.Default._EP, "\\E Player.exe", Lbl_EPThisVer, ref EPThisVer, ref EPState);
            GetThisVersion(Properties.Settings.Default._EN, "\\E Number.exe", Lbl_ENThisVer, ref ENThisVer, ref ENState);
            GetThisVersion(Properties.Settings.Default._EL, "\\E Linker.exe", Lbl_ELThisVer, ref ELThisVer, ref ELState);
        }
        /// <summary>
        /// 检测指定目录是否有可执行文件，并获取其版本信息
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="name"></param>
        private void GetThisVersion(string dir, string name, System.Windows.Controls.Label lb, ref Version ver, ref State state)
        {
            if (Directory.Exists(dir))
            {
                string path = dir + name;
                if (File.Exists(path))
                {
                    FileVersionInfo file = FileVersionInfo.GetVersionInfo(path);
                    ver = new Version(file.FileVersion);
                    lb.Content = ver;
                    state = State.HasNewEXE;
                }
                else
                {
                    ver = null;
                    lb.Content = "未安装";
                    state = State.HasNotEXE;
                }
            }
            else
            {
                state = State.HasNotSet;
            }
        }
        /// <summary>
        /// 获取所有软件最新版本信息
        /// </summary>
        /// <returns>所有软件新版本信息</returns>
        public void GetAllNewVersion()
        {
            try
            {
                //从网络获取数据
                WebClient webClient = new WebClient();
                Stream st = webClient.OpenRead(Properties.Settings.Default._LatestVersionInfo);
                StreamReader stw = new StreamReader(st, Encoding.UTF8);
                string info = stw.ReadToEnd().Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
                //MessageBox.Show(info);
                stw.Close();
                st.Close();
                string[] infos = info.Split(';');
                //将数据记录到本地变量
                foreach (string items in infos)
                {
                    string[] item = items.Split(':');
                    switch (item[0])
                    {
                        case "EU":
                            ShowNewVersion(item[1], ref EUNewVer, Lbl_EUNewVer);
                            break;
                        case "EW":
                            ShowNewVersion(item[1], ref EWNewVer, Lbl_EWNewVer);
                            break;
                        case "EP":
                            ShowNewVersion(item[1], ref EPNewVer, Lbl_EPNewVer);
                            break;
                        case "EN":
                            ShowNewVersion(item[1], ref ENNewVer, Lbl_ENNewVer);
                            break;
                        case "EL":
                            ShowNewVersion(item[1], ref ELNewVer, Lbl_ELNewVer);
                            break;
                    }
                }
            }
            catch (Exception)
            {
                ShowMessage("获取软件版本信息时出现错误", false);
            }
        }
        /// <summary>
        /// 获取指定软件最新版本信息
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="lb"></param>
        public void ShowNewVersion(string str, ref Version ver, System.Windows.Controls.Label lb)
        {
            try
            {
                if (str =="0.0.0.0")
                {
                    ver = null;
                    lb.Content = "开发中";
                }
                else
                {
                    ver = new Version(str);
                    lb.Content = ver;
                }
            }
            catch (Exception)
            {
                ver = null;
                lb.Content = "检测失败";
            }
        }
        /// <summary>
        /// 对比所有软件当前版本与最新版本信息
        /// </summary>
        public void CompareAllVersion()
        {
            CompareVersion(EUNewVer, EUThisVer, ref EUState);
            CompareVersion(EWNewVer, EWThisVer, ref EWState);
            CompareVersion(EPNewVer, EPThisVer, ref EPState);
            CompareVersion(ENNewVer, ENThisVer, ref ENState);
            CompareVersion(ELNewVer, ELThisVer, ref ELState);
        }
        /// <summary>
        /// 对比指定软件当前版本与最新版本信息
        /// </summary>
        /// <param name="newLB"></param>
        /// <param name="thisLB"></param>
        /// <param name="btn"></param>
        public void CompareVersion(Version newVer, Version thisVer, ref State state)
        {
            if (thisVer != null)
            {
                if (newVer != null)
                {
                    //开始对比
                    int hasNewVersion = newVer.CompareTo(thisVer);
                    if (hasNewVersion == 1)
                    {
                        state = State.HasOldEXE;
                    }
                    else
                    {
                        state = State.HasNewEXE;
                    }
                }
                else
                {
                    state = State.HasNewEXE;
                }
            }
            else
            {
                if (state != State.HasNotSet)
                {
                    state = State.HasNotEXE;
                }
            }
        }

        /// <summary>
        /// 设置所有UI按钮状态
        /// </summary>
        public void GetAllState()
        {
            GetState(ref EUState, Btn_EUBrowse, Btn_EURun, Btn_EUInstall, Btn_EUUninstall);
            GetState(ref EWState, Btn_EWBrowse, Btn_EWRun, Btn_EWInstall, Btn_EWUninstall);
            GetState(ref EPState, Btn_EPBrowse, Btn_EPRun, Btn_EPInstall, Btn_EPUninstall);
            GetState(ref ENState, Btn_ENBrowse, Btn_ENRun, Btn_ENInstall, Btn_ENUninstall);
            GetState(ref ELState, Btn_ELBrowse, Btn_ELRun, Btn_ELInstall, Btn_ELUninstall);
        }
        /// <summary>
        /// 设置UI按钮状态
        /// </summary>
        /// <param name="state"></param>
        /// <param name="bro"></param>
        /// <param name="run"></param>
        /// <param name="ins"></param>
        /// <param name="uni"></param>
        public void GetState(ref State state, System.Windows.Controls.Button bro, System.Windows.Controls.Button run,
                                         System.Windows.Controls.Button ins, System.Windows.Controls.Button uni)
        {
            switch (state)
            {
                case State.HasNotSet:
                    bro.IsEnabled = true;
                    ins.IsEnabled = false;
                    ins.Content = "安装";
                    uni.IsEnabled = false;
                    run.IsEnabled = false;
                    break;
                case State.HasNotEXE:
                    bro.IsEnabled = true;
                    ins.IsEnabled = true;
                    ins.Content = "安装";
                    uni.IsEnabled = false;
                    run.IsEnabled = false;
                    break;
                case State.HasOldEXE:
                    bro.IsEnabled = true;
                    ins.IsEnabled = true;
                    ins.Content = "更新";
                    uni.IsEnabled = true;
                    run.IsEnabled = true;
                    break;
                case State.HasNewEXE:
                    bro.IsEnabled = true;
                    ins.IsEnabled = false;
                    ins.Content = "更新";
                    uni.IsEnabled = true;
                    run.IsEnabled = true;
                    break;
                case State.IsNotRunning:
                    bro.IsEnabled = true;
                    ins.IsEnabled = true;
                    uni.IsEnabled = true;
                    run.IsEnabled = true;
                    run.Content = "运行";
                    break;
                case State.IsRunning:
                    bro.IsEnabled = true;
                    ins.IsEnabled = true;
                    uni.IsEnabled = true;
                    run.IsEnabled = true;
                    run.Content = "结束";
                    break;
            }
        }
        
        //浏览
        private void Btn_EUBrowse_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Btn_EWBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = Browse();
            if (path != null)
            {
                Properties.Settings.Default._EW = path;
                Properties.Settings.Default.Save();
                //更新UI
                Refresh(false);
            }
        }
        private void Btn_EPBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = Browse();
            if (path != null)
            {
                Properties.Settings.Default._EP = path;
                Properties.Settings.Default.Save();
                //更新UI
                Refresh(false);
            }
        }
        private void Btn_ENBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = Browse();
            if (path != null)
            {
                Properties.Settings.Default._EN = path;
                Properties.Settings.Default.Save();
                //更新UI
                Refresh(false);
            }
        }
        private void Btn_ELBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = Browse();
            if (path != null)
            {
                Properties.Settings.Default._EL = path;
                Properties.Settings.Default.Save();
                //更新UI
                Refresh(false);
            }
        }
        //浏览
        private string Browse()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                //SelectedPath = FatherPath.Text,
                Description = "请选择软件安装位置："
            };
            //按下确定选择的按钮，获取根目录文件夹路径
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return fbd.SelectedPath;
            }
            else
            {
                return null;
            }
        }

        //安装、更新
        private void Btn_EUInstall_Click(object sender, RoutedEventArgs e)
        {
            //创建的压缩文件名称
            string name = "E Updater " + EUNewVer + ".zip";
            string _bat = "Update.bat";
            //下载压缩包
            //Download(Properties.Settings.Default._EUDownload, name);
            //解压
            //UnZip(Properties.Settings.Default._download + name, Properties.Settings.Default._temp);
            //获取解压后所有文件的路径
            string _file1 = Properties.Settings.Default._temp + "\\E Updater.exe";
            string _file2 = Properties.Settings.Default._temp + "\\ICSharpCode.SharpZipLib.dll";
            //创建批处理命令
            FileStream fs = new FileStream(_bat, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine("@echo off");
            sw.WriteLine("move \"" + _file1 + "\" \"\"");
            sw.WriteLine("move \"" + _file2 + "\" \"\"");
            //sw.WriteLine("echo \"Press any key to run E Updater\"");
            //sw.WriteLine("pause");
            sw.WriteLine("start \"\" \"E Updater.exe\"");
            sw.Flush();
            sw.Close();
            //运行批处理命令
            Process process = new Process();
            process.StartInfo.FileName = _bat;
            process.Start();
            //退出EU
            Process.GetCurrentProcess().CloseMainWindow();
        }
        private void Btn_EWInstall_Click(object sender, RoutedEventArgs e)
        {
            if (EWNewVer != null)
            {
                Install("E Writer " + EWNewVer + ".zip", Properties.Settings.Default._EW, Properties.Settings.Default._EWDownload);
            }
            else
            {
                ShowMessage("版本检测失败，请刷新后再试", false);
            }
        }
        private void Btn_EPInstall_Click(object sender, RoutedEventArgs e)
        {
            if (EPNewVer != null)
            {
                Install("E Player " + EPNewVer + ".zip", Properties.Settings.Default._EP, Properties.Settings.Default._EPDownload);
            }
            else
            {
                ShowMessage("版本检测失败，请刷新后再试", false);
            }
        }
        private void Btn_ENInstall_Click(object sender, RoutedEventArgs e)
        {
            if (ENNewVer != null)
            {
                Install("E Number " + ENNewVer + ".zip", Properties.Settings.Default._EN, Properties.Settings.Default._ENDownload);
}
            else
            {
                ShowMessage("版本检测失败，请刷新后再试", false);
            }
        }
        private void Btn_ELInstall_Click(object sender, RoutedEventArgs e)
        {
            if (ELNewVer != null)
            {
                Install("E Linker " + ELNewVer + ".zip", Properties.Settings.Default._EL, Properties.Settings.Default._ELDownload);
            }
            else
            {
                ShowMessage("软件开发中，敬请期待", false);
            }
        }
        //安装、更新
        private void Install(string name, string dir, string download)
        {
            if (Directory.Exists(dir))
            {
                //下载压缩包
                Download(download, name);
                //解压，覆盖
                UnZip(Properties.Settings.Default._download + "\\" + name, dir);
                //删除压缩包

                //
                ShowMessage(Path.GetFileNameWithoutExtension(name) + "已安装",false);
                //更新UI
                Refresh(false);
            }
            else
            {
                ShowMessage("请选择一个有效的安装路径", false);
            }
        }

        //卸载
        private void Btn_EUUninstall_Click(object sender, RoutedEventArgs e)
        {
            //Uninstall("EU", Properties.Settings.Default._EU);
        }
        private void Btn_EWUninstall_Click(object sender, RoutedEventArgs e)
        {
            Uninstall("E Writer.exe", Properties.Settings.Default._EW);
        }
        private void Btn_EPUninstall_Click(object sender, RoutedEventArgs e)
        {
            Uninstall("E Player.exe", Properties.Settings.Default._EP);
        }
        private void Btn_ENUninstall_Click(object sender, RoutedEventArgs e)
        {
            Uninstall("E Number.exe", Properties.Settings.Default._EN);
        }
        private void Btn_ELUninstall_Click(object sender, RoutedEventArgs e)
        {
            Uninstall("E Linker.exe", Properties.Settings.Default._EL);
        }
        //卸载
        private void Uninstall(string name, string dir)
        {
            string path = dir + "\\" + name;
            if (File.Exists(path))
            {
                if (!IsRunning(path))
                {
                    //删除所有相关文件
                    File.Delete(path);
                    ShowMessage(name + "已卸载", false);
                    Refresh(false);
                }
                else
                {
                    ShowMessage("请先关闭" + name, true);
                }
            }

        }

        //打开、结束
        private void Btn_EURun_Click(object sender, RoutedEventArgs e)
        {
            Process.GetCurrentProcess().CloseMainWindow();
        }
        private void Btn_EWRun_Click(object sender, RoutedEventArgs e)
        {
            if (IsRunning(TBx_EWPath.Text + "\\E Writer.exe"))
            {
               Stop(Btn_EWRun, TBx_EWPath.Text + "\\E Writer.exe");
                Btn_EWRun.Content = "打开";
            }
            else
            {
                Start(Btn_EWRun, TBx_EWPath.Text + "\\E Writer.exe");
                Btn_EWRun.Content = "关闭";
            }
        }
        private void Btn_EPRun_Click(object sender, RoutedEventArgs e)
        {
            if (IsRunning(TBx_EPPath.Text + "\\E Player.exe"))
            {
                Stop(Btn_EPRun, TBx_EPPath.Text + "\\E Player.exe");
                Btn_EPRun.Content = "打开";
            }
            else
            {
                Start(Btn_EPRun, TBx_EPPath.Text + "\\E Player.exe");
                Btn_EPRun.Content = "关闭";
            }
        }
        private void Btn_ENRun_Click(object sender, RoutedEventArgs e)
        {
            if (IsRunning(TBx_ENPath.Text + "\\E Number.exe"))
            {
                Stop(Btn_ENRun, TBx_ENPath.Text + "\\E Number.exe");
                Btn_ENRun.Content = "打开";
            }
            else
            {
                Start(Btn_ENRun, TBx_ENPath.Text + "\\E Number.exe");
                Btn_ENRun.Content = "关闭";
            }
        }
        private void Btn_ELRun_Click(object sender, RoutedEventArgs e)
        {
            if (IsRunning(TBx_ELPath.Text + "\\E Linker.exe"))
            {
                Stop(Btn_ELRun, TBx_ELPath.Text + "\\E Linker.exe");
                Btn_ELRun.Content = "打开";
            }
            else
            {
                Start(Btn_ELRun, TBx_ELPath.Text + "\\E Linker.exe");
                Btn_ELRun.Content = "关闭";
            }
        }
        /// <summary>
        /// 打开
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="path"></param>
        private void Start(System.Windows.Controls.Button btn, string path)
        {
            Process pr = new Process();
            if (btn != Btn_EUInstall)
            {
                pr.StartInfo.FileName = path;
                pr.Start();
            }
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="path"></param>
        private void Stop(System.Windows.Controls.Button btn, string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            Process[] ps = Process.GetProcesses();
            foreach (Process p in ps)
            {
                if (p.ProcessName == name && p.MainModule.FileName == path)
                {
                    p.CloseMainWindow();
                    ShowMessage("已关闭" + name, false);
                    break;
                }
            }
        }
        /// <summary>
        /// 检测指定软件是否运行
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsRunning(string path)
        {
            if (File.Exists(path))
            {
                string name = Path.GetFileNameWithoutExtension(path);
                Process[] ps = Process.GetProcesses();
                foreach (Process p in ps)
                {
                    //软件已打开
                    if (p.ProcessName == name && p.MainModule.FileName == path)
                    {
                        return true;
                    }
                }
                //软件未打开
                return false;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 检测所有软件运行状态
        /// </summary>
        private void CheckAllRunningState()
        {
            if (IsRunning(TBx_EUPath.Text + "\\E Updater.exe"))
            { Btn_EURun.Content = "关闭"; }
            else
            {Btn_EURun.Content = "打开";}

            if (IsRunning(TBx_EWPath.Text + "\\E Writer.exe"))
            { Btn_EWRun.Content = "关闭";}
            else
            { Btn_EWRun.Content = "打开"; }

            if (IsRunning(TBx_EPPath.Text + "\\E Player.exe"))
            {  Btn_EPRun.Content = "关闭"; }
            else
            { Btn_EPRun.Content = "打开"; }

            if (IsRunning(TBx_ENPath.Text + "\\E Number.exe"))
            {  Btn_ENRun.Content = "关闭"; }
            else
            { Btn_ENRun.Content = "打开"; }

            if (IsRunning(TBx_ELPath.Text + "\\E Linker.exe"))
            { Btn_ELRun.Content = "关闭"; }
            else
            { Btn_ELRun.Content = "打开"; }
        }        
        //浏览相关网页
        private void Lbl_EU_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", "http://estar.zone/introduction/e-updater/");
        }
        private void Lbl_EW_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", "http://estar.zone/introduction/e-writer/");
        }
        private void Lbl_EP_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", "http://estar.zone/introduction/e-player/");
        }
        private void Lbl_EN_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", "http://estar.zone/introduction/e-number/");
        }
        private void Lbl_EL_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", "http://estar.zone/introduction/e-linker/");
        }

        //窗口激活
        private void Window_Activated(object sender, EventArgs e)
        {
            Refresh(false);
        }
        //点击刷新
        private void Btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh(true);
        }

        //刷新
        private void Refresh(bool isOnlyUI)
        {
            //载入目录
            LoadAllPath();
            //获取当前版本
            GetAllThisVersion();
            if (isOnlyUI)
            {
                //检查网络，获取最新版本信息
                if (IsOnLine())
                {
                    GetAllNewVersion();
                    ShowMessage("网络连接成功", false);
                }
                else
                { ShowMessage("网络连接失败", false); }
            }
            //对比版本
            CompareAllVersion();
            //设置UI按钮
            GetAllState();
            //运行状态
            CheckAllRunningState();


            //初始化UI
            Btn_EUBrowse.IsEnabled = false;
            Btn_EURun.Content = "关闭";

        }
        //点击帮助
        private void Btn_Help_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "http://estar.zone/introduction/e-updater");
        }


        public enum State
        {
            HasNotSet,
            HasNotEXE,
            HasOldEXE,
            HasNewEXE,
            IsNotRunning,
            IsRunning
        }
    }
}
