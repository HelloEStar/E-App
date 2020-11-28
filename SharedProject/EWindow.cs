using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SharedProject
{ 
    public class EWindow : Window
    {
        /// <summary>
        /// 应用信息
        /// </summary>
        protected AppInfo AppInfo { get; } = new AppInfo();

        /// <summary>
        /// 当前菜单
        /// </summary>
        protected MenuTool CurrentMenuTool { get; set; } = MenuTool.文件;

        protected Brush BrushFG01 => (Brush)FindResource("一级前景色");
        protected Brush BrushFG02 => (Brush)FindResource("二级前景色");
        protected Brush BrushFG03 => (Brush)FindResource("三级前景色");
        protected Brush BrushBG01 => (Brush)FindResource("一级背景色");
        protected Brush BrushBG02 => (Brush)FindResource("二级背景色");
        protected Brush BrushBG03 => (Brush)FindResource("三级背景色");
        protected Brush BrushHL01 => (Brush)FindResource("一级高亮色");
        protected Brush BrushHL02 => (Brush)FindResource("二级高亮色");
        protected Brush BrushHL03 => (Brush)FindResource("三级高亮色");

        protected List<RDItem> LanguageItems = new List<RDItem>()
        {
            new RDItem("中文（默认）", "zh_CN"),
            new RDItem("English", "en_US"),
        };
        protected List<ThemeItem> ThemeItems = new List<ThemeItem>()
        {
            new ThemeItem("亮色（默认）", "00 Light.ini"),
            new ThemeItem("暗色", "01 Dark.ini"),
            new ThemeItem("自定义", "自定义"),
        };

        /// <summary>
        /// 检查用户是否同意用户协议
        /// </summary>
        /// <returns></returns>
        protected bool CheckUserAgreement(int runCount)
        {
            if (runCount == 0)
            {
                string str = AppInfo.UserAgreement + "\n\n你需要同意此协议才能使用本软件，是否同意？";
                MessageBoxResult result = MessageBox.Show(str, FindResource("用户协议").ToString(), MessageBoxButton.YesNo);
                bool isAgree = result == MessageBoxResult.Yes;
                if (!isAgree)
                {
                    Close();
                }
                return isAgree;
            }
            return true;
        }

        /// <summary>
        /// 切换工具面板
        /// </summary>
        protected void SwitchMenuToolFile()
        {
            switch (CurrentMenuTool)
            {
                case MenuTool.文件:
                    SetMenuTool(MenuTool.无);
                    break;
                default:
                    SetMenuTool(MenuTool.文件);
                    break;
            }
        }
        /// <summary>
        /// 切换编辑面板
        /// </summary>
        protected void SwitchMenuToolEdit()
        {
            switch (CurrentMenuTool)
            {
                case MenuTool.编辑:
                    SetMenuTool(MenuTool.无);
                    break;
                default:
                    SetMenuTool(MenuTool.编辑);
                    break;
            }
        }
        /// <summary>
        /// 切换设置面板
        /// </summary>
        protected void SwitchMenuToolSetting()
        {
            switch (CurrentMenuTool)
            {
                case MenuTool.设置:
                    SetMenuTool(MenuTool.无);
                    break;
                default:
                    SetMenuTool(MenuTool.设置);
                    break;
            }
        }
        /// <summary>
        /// 切换关于面板
        /// </summary>
        protected void SwitchMenuToolAbout()
        {
            switch (CurrentMenuTool)
            {
                case MenuTool.关于:
                    SetMenuTool(MenuTool.无);
                    break;
                default:
                    SetMenuTool(MenuTool.关于);
                    break;
            }
        }

        /// <summary>
        /// 设置当前菜单
        /// </summary>
        /// <param name="menu"></param>
        protected virtual void SetMenuTool(MenuTool menu) { }

        protected void SetPanColors(StackPanel sp, string content)
        {
            string[] strs = content.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            foreach (string item in strs)
            {
                if (string.IsNullOrEmpty(item)) continue;

                string[] pair = item.Split('=');
                if (pair.Length != 2) continue;

                keyValuePairs.Add(pair[0], pair[1]);
            }

            for (int i = 0; i < sp.Children.Count; i++)
            {
                ColorPicker cp = (ColorPicker)sp.Children[i];
                Color cl = ColorHelper.Get(keyValuePairs[cp.ColorTarget]);
                cp.Color = cl.ToString();
                cp.Owner = this;
                cp.Resources = Resources;
            }
        }

        protected string GetPanColors(StackPanel sp)
        {
            List<string> strs = new List<string>();
            for (int i = 0; i < sp.Children.Count; i++)
            {
                ColorPicker cp = (ColorPicker)sp.Children[i];
                string name = cp.ColorTarget;
                string value = cp.Color;
                strs.Add(name + "=" + value);
            }

            string colors = "";
            foreach (string item in strs)
            {
                colors += item + "\n";
            }
            colors.TrimEnd('\n');
            return colors;
        }



        /// <summary>
        /// 重置设置
        /// </summary>
        protected virtual void ResetSettings() { }
        /// <summary>
        /// 保存设置
        /// </summary>
        protected virtual void SaveSettings() { }


        /// <summary>
        /// 刷新
        /// </summary>
        protected virtual void Refresh()
        {
            RefreshTitle();
            RefreshAppInfo();
        }
        /// <summary>
        /// 刷新窗口标题
        /// </summary>
        protected virtual void RefreshTitle()
        { 
            string str = AppInfo.Name + " " + AppInfo.VersionShort;
            Title = str;
        }
        /// <summary>
        /// 刷新应用信息
        /// </summary>
        protected virtual void RefreshAppInfo() { }

        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="resourceName">资源名</param>
        /// <param name="newBox">是否弹出对话框</param>
        protected virtual void ShowMessage(string message, bool newBox = false) { }


        /// <summary>
        /// 载入语言选项
        /// </summary>
        protected void LoadLanguageItems(ComboBox cbb)
        {
            cbb.Items.Clear();
            foreach (RDItem item in LanguageItems)
            {
                ComboBoxItem cbi = new ComboBoxItem
                {
                    Content = item.Name,
                    ToolTip = item.Value,
                    Tag = item.RD
                };
                cbb.Items.Add(cbi);
            }
        }
        /// <summary>
        /// 载入所有可用主题
        /// </summary>
        protected void LoadThemeItems(ComboBox cbb)
        {
            cbb.Items.Clear();
            for (int i = 0; i < ThemeItems.Count; i++)
            {
                ComboBoxItem cbi = new ComboBoxItem
                {
                    Content = ThemeItems[i].Name,
                    //ToolTip = values[i],
                    Tag = ThemeItems[i].Value
                };
                cbb.Items.Add(cbi);
            }
        }
        /// <summary>
        /// 设置为下一个主题
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected int SetNextTheme(ComboBox cbb, int index)
        {
            index++;
            if (index > cbb.Items.Count - 1)
            {
                index = 0;
            }
            return index;
        }


        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="resourceName">资源名</param>
        /// <param name="newBox">是否弹出对话框</param>
        protected static void ShowMessage(Label lbl, string message, bool newBox = false)
        {
            if (newBox)
            {
                MessageBox.Show(message);
            }
            else
            {
                if (lbl != null)
                {
                    //实例化一个DoubleAnimation类。
                    DoubleAnimation doubleAnimation = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = new Duration(TimeSpan.FromSeconds(3))
                    };
                    //为元素设置BeginAnimation方法。
                    lbl.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);

                    lbl.Content = message;
                }
            }
        }
        /// <summary>
        /// 选择文件夹
        /// </summary>
        protected static string ChooseFolder(string description)
        {
            //System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog
            //{
            //    ShowNewFolderButton = true,
            //    Description = description
            //};

            //if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    return fbd.SelectedPath;
            //}
            //else
            {
                return null;
            }
        }


        protected virtual void EWindow_GotFocus(object sender, RoutedEventArgs e)
        {
        }
        protected virtual void EWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }
        protected virtual void EWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        protected virtual void EWindow_KeyUp(object sender, KeyEventArgs e)
        {
            //关于菜单
            if (e.Key == Key.F1)
            { Process.Start("explorer.exe", AppInfo.HomePage); }
            else if (e.Key == Key.F2)
            { Process.Start("explorer.exe", AppInfo.GitHubPage); }
            else if (e.Key == Key.F3)
            { Process.Start("explorer.exe", AppInfo.QQGroupLink); }
        }

        protected void BtnFile_Click(object sender, RoutedEventArgs e)
        {
            SwitchMenuToolFile();
        }
        protected void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            SwitchMenuToolEdit();
        }
        protected void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            SwitchMenuToolSetting();
        }
        protected void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            SwitchMenuToolAbout();
        }

        protected void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
        protected void BtnResetSettings_Click(object sender, RoutedEventArgs e)
        {
            ResetSettings();
        }

        protected void BtnBitCoinAddress_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetDataObject(AppInfo.BitCoinAddress, true);
            ShowMessage(FindResource("已复制").ToString());
        }
        protected void BtnHomePage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.HomePage);
        }
        protected void BtnGitHubPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.GitHubPage);
        }
        protected void BtnQQGroup_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.QQGroupLink);
        }

        /// <summary>
        /// 菜单工具栏
        /// </summary>
        public enum MenuTool
        {
            无,
            文件,
            编辑,
            设置,
            关于
        }
    }
}