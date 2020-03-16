using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;
using SharedProject;
using MessageBox = System.Windows.MessageBox;
using Settings = E.Coder.Properties.Settings;

namespace E.Coder
{
    public partial class MainWindow : EWindow
    {
        //构造
        public MainWindow()
        {
            InitializeComponent();
        }

        //载入
        private void LoadRecordItems()
        {
            if (!string.IsNullOrEmpty(Settings.Default.Record))
            {
                string[] _myB = Regex.Split(Settings.Default.Record, "///");
                foreach (string b in _myB)
                {
                    AddRecordItem(b);
                }
            }
        }

        //保存
        protected override void SaveSettings()
        {
            Settings.Default.Save();
            ShowMessage(FindResource("已保存").ToString());
        }

        //添加
        private void AddRecordItem(string content)
        {
            ListBoxItem item = new ListBoxItem
            {
                FontSize = 20,
                Content = content,
                Style = (Style)FindResource("列表子项样式")
            };
            LtbRecord.Items.Add(item);
        }

        //设置
        protected override void SetMenuTool(MenuTool menu)
        {
            switch (menu)
            {
                case MenuTool.无:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    //BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.文件:
                    PanFile.Visibility = Visibility.Visible;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.BorderThickness = new Thickness(4, 0, 0, 0);
                    //BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.编辑:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Visible;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    //BtnEdit.BorderThickness = new Thickness(4, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.设置:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Visible;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    //BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(4, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.关于:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Visible;
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    //BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(4, 0, 0, 0);
                    break;
                default:
                    break;
            }
            CurrentMenuTool = menu;
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

        //刷新
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
            TxtUpdateNote.Text = AppInfo.UpdateNote;
        }
        protected override void RefreshTitle()
        {
            string str = AppInfo.Name + " " + AppInfo.VersionShort;
            Main.Title = str;
        }

        //显示
        protected override void ShowMessage(string message, bool newBox = false)
        {
            ShowMessage(LblMessage, message, newBox);
        }

        //主窗口
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            //载入
            LoadLanguageItems(CbbLanguages);
            LoadThemeItems(CbbThemes);
            LoadRecordItems();

            //刷新
            RefreshAppInfo();
            RefreshTitle();
            LblMessage.Opacity = 0;

            //检查用户是否同意用户协议
            if (CheckUserAgreement(Settings.Default.RunCount))
            {
                Settings.Default.RunCount += 1;
            }
        }
        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }
        protected override void Main_KeyUp(object sender, KeyEventArgs e)
        {
            base.Main_KeyUp(sender, e);

            //Ctrl+T 切换下个主题
            if (e.Key == Key.T && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            { SetNextTheme(CbbThemes, Settings.Default.Theme); }
        }

        //工具栏
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            LtbRecord.Items.Clear();
        }
        private void TxtValue_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TxtValue.FontSize = TxtValue.ActualHeight / 1.25f;
        }
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
        private void TxtMinValue_Loaded(object sender, RoutedEventArgs e)
        {
            //TxtMinValue.Text = Settings.Default.Min.ToString();
        }
        private void TxtMaxValue_Loaded(object sender, RoutedEventArgs e)
        {
            //TxtMaxValue.Text = Settings.Default.Max.ToString();
        }
        private void TxtMinValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (int.TryParse(TxtMinValue.Text, out int min))
            //{
            //    Settings.Default.Min = min;
            //}
            //else
            //{
            //    TxtMinValue.Text = Settings.Default.Min.ToString();
            //}
        }
        private void TxtMaxValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (int.TryParse(TxtMaxValue.Text, out int max))
            //{
            //    Settings.Default.Max = max;
            //}
            //else
            //{
            //    TxtMaxValue.Text = Settings.Default.Max.ToString();
            //}
        }

        //工作区
        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            //if (Settings.Default.Min < Settings.Default.Max)
            //{
            //    Random rd = new Random();
            //    int value = rd.Next(Settings.Default.Min, Settings.Default.Max);
            //    TxtValue.Text = value.ToString();
            //    AddRecordItem(LtbRecord.Items.Count + 1 + ".    " + value);
            //}
            //else
            //{
            //    ShowMessage(FindResource("范围错误").ToString());
            //}
        }
    }
}
