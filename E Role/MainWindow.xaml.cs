using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SharedProject;

namespace E.Role
{
    public partial class MainWindow : EWindow
    {
        public List<string> Surnames { get; } = new List<string>()
        {"赵", "钱", "孙", "李", "周", "吴", "郑", "王", "冯", "陈", "楮", "卫", "蒋", "沈", "韩", "杨",
  "朱", "秦", "尤", "许", "何", "吕", "施", "张", "孔", "曹", "严", "华", "金", "魏", "陶", "姜",
  "戚", "谢", "邹", "喻", "柏", "水", "窦", "章", "云", "苏", "潘", "葛", "奚", "范", "彭", "郎",
  "鲁", "韦", "昌", "马", "苗", "凤", "花", "方", "俞", "任", "袁", "柳", "酆", "鲍", "史", "唐",
  "费", "廉", "岑", "薛", "雷", "贺", "倪", "汤", "滕", "殷", "罗", "毕", "郝", "邬", "安", "常",
  "乐", "于", "时", "傅", "皮", "卞", "齐", "康", "伍", "余", "元", "卜", "顾", "孟", "平", "黄",
  "和", "穆", "萧", "尹", "姚", "邵", "湛", "汪", "祁", "毛", "禹", "狄", "米", "贝", "明", "臧",
  "计", "伏", "成", "戴", "谈", "宋", "茅", "庞", "熊", "纪", "舒", "屈", "项", "祝", "董", "梁",
  "杜", "阮", "蓝", "闽", "席", "季", "麻", "强", "贾", "路", "娄", "危", "江", "童", "颜", "郭",
  "梅", "盛", "林", "刁", "锺", "徐", "丘", "骆", "高", "夏", "蔡", "田", "樊", "胡", "凌", "霍",
  "虞", "万", "支", "柯", "昝", "管", "卢", "莫", "经", "房", "裘", "缪", "干", "解", "应", "宗",
  "丁", "宣", "贲", "邓", "郁", "单", "杭", "洪", "包", "诸", "左", "石", "崔", "吉", "钮", "龚",
  "程", "嵇", "邢", "滑", "裴", "陆", "荣", "翁", "荀", "羊", "於", "惠", "甄", "麹", "家", "封",
  "芮", "羿", "储", "靳", "汲", "邴", "糜", "松", "井", "段", "富", "巫", "乌", "焦", "巴", "弓",
  "牧", "隗", "山", "谷", "车", "侯", "宓", "蓬", "全", "郗", "班", "仰", "秋", "仲", "伊", "宫",
  "宁", "仇", "栾", "暴", "甘", "斜", "厉", "戎", "祖", "武", "符", "刘", "景", "詹", "束", "龙",
  "叶", "幸", "司", "韶", "郜", "黎", "蓟", "薄", "印", "宿", "白", "怀", "蒲", "邰", "从", "鄂",
  "索", "咸", "籍", "赖", "卓", "蔺", "屠", "蒙", "池", "乔", "阴", "郁", "胥", "能", "苍", "双",
  "闻", "莘", "党", "翟", "谭", "贡", "劳", "逄", "姬", "申", "扶", "堵", "冉", "宰", "郦", "雍",
  "郤", "璩", "桑", "桂", "濮", "牛", "寿", "通", "边", "扈", "燕", "冀", "郏", "浦", "尚", "农",
  "温", "别", "庄", "晏", "柴", "瞿", "阎", "充", "慕", "连", "茹", "习", "宦", "艾", "鱼", "容",
  "向", "古", "易", "慎", "戈", "廖", "庾", "终", "暨", "居", "衡", "步", "都", "耿", "满", "弘",
  "匡", "国", "文", "寇", "广", "禄", "阙", "东", "欧", "殳", "沃", "利", "蔚", "越", "夔", "隆",
  "师", "巩", "厍", "聂", "晁", "勾", "敖", "融", "冷", "訾", "辛", "阚", "那", "简", "饶", "空",
  "曾", "毋", "沙", "乜", "养", "鞠", "须", "丰", "巢", "关", "蒯", "相", "查", "后", "荆", "红",
  "游", "竺", "权", "逑", "盖", "益", "桓", "公", "仉", "督", "晋", "楚", "阎", "法", "汝", "鄢",
  "涂", "钦", "岳", "帅", "缑", "亢", "况", "后", "有", "琴", "归", "海", "墨", "哈", "谯", "笪",
  "年", "爱", "阳", "佟", "商", "牟", "佘", "佴", "伯", "赏",
  "万俟", "司马", "上官", "欧阳", "夏侯", "诸葛", "闻人", "东方", "赫连", "皇甫", "尉迟", "公羊",
  "澹台", "公冶", "宗政", "濮阳", "淳于", "单于", "太叔", "申屠", "公孙", "仲孙", "轩辕", "令狐",
  "锺离", "宇文", "长孙", "慕容", "鲜于", "闾丘", "司徒", "司空", "丌官", "司寇", "子车", "微生",
  "颛孙", "端木", "巫马", "公西", "漆雕", "乐正", "壤驷", "公良", "拓拔", "夹谷", "宰父", "谷梁",
  "段干", "百里", "东郭", "南门", "呼延", "羊舌", "梁丘", "左丘", "东门", "西门", "南宫"};
        public List<string> Genders { get; } = new List<string>()
        {
            "男","女", "无", "双"
        };

        //构造
        public MainWindow()
        {
            InitializeComponent();
        }

        //载入
        private void LoadRoleItems()
        {
            string str = Settings.Default.Record.Replace("}", "");
            string[] strs = str.Split('{');
            if (strs.Length > 0)
            {
                List<Role> roles = new List<Role>();
                foreach (string item in strs)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        string[] temps = item.Split(',');
                        Role role = new Role(temps);
                        roles.Add(role);
                    }
                }

                if (roles.Count > 0)
                {
                    foreach (Role item in roles)
                    {
                        AddRoleItem(item, false);
                    }

                    LtbRecord.SelectedIndex = 0;
                    ShowCurrentRoleItem();
                }
            }
        }

        //保存
        protected override void SaveSettings()
        {
            if (!CheckIsCorrectRange())
            {
                return;
            }
            if (LtbRecord.Items != null)
            {
                List<Role> items = new List<Role>();
                foreach (var item in LtbRecord.Items)
                {
                    ListBoxItem ib = (ListBoxItem)item;
                    Role role = (Role)ib.Tag;
                    if (role != null)
                    {
                        items.Add(role);
                    }
                }

                string str = "";
                foreach (Role item in items)
                {
                    str += item.ShortInfo;
                }
                Settings.Default.Record = str;
            }

            if (Settings.Default.Theme == 2)
            {
                string content = GetPanColors(PanColors);
                Settings.Default.ThemeCustomize = content;
            }

            Settings.Default.Save();
            ShowMessage(FindResource("已保存").ToString());
        }

        //创建
        private void CreateRole()
        {
            if (!CheckIsCorrectRange())
            {
                return;
            }

            string surname = "";
            if (Settings.Default.IsCreateSurame)
            {
                surname = GetRandomSurname((SurnameType)Settings.Default.SurameType);
            }
            string name = "";
            if (Settings.Default.IsCreateName)
            {
                name = GetRandomName(Settings.Default.NameLength);
            }
            string birthday = "";
            if (Settings.Default.IsCreateBirthday)
            {
                birthday = GetRandomBirthday(Settings.Default.BirthdayMin, Settings.Default.BirthdayMax);
            }
            string gender = "";
            if (Settings.Default.IsCreateGender)
            {
                gender = GetRandomGender((GenderType)Settings.Default.GenderType);
            }
            string height = "";
            if (Settings.Default.IsCreateHeight)
            {
                height = GetRandomHeight(Settings.Default.HeightMin, Settings.Default.HeightMax);
            }
            string weight = "";
            if (Settings.Default.IsCreateWeight)
            {
                weight = GetRandomWeight(Settings.Default.WeightMin, Settings.Default.WeightMax);
            }

            Role role = new Role(surname, name, birthday, gender, height, weight);
            AddRoleItem(role, true);
        }

        //添加
        private void AddRoleItem(Role role, bool isShow)
        {
            if (role != null)
            {
                ListBoxItem item = new ListBoxItem
                {
                    Name = "RoleItem",
                    Tag = role,
                    ToolTip = role.LongInfo,
                    Content = role.DisplayInfo,
                    Style = (Style)FindResource("列表子项样式")
                };
                //添加事件
                item.AddHandler(MouseDownEvent, new MouseButtonEventHandler(LtbRoles_MouseDown), true);
                LtbRecord.Items.Add(item);

                if (isShow)
                {
                    ShowRole(item);
                }
            }
        }


        //移除
        private void RemoveCurrentRoleItem()
        {
            if (LtbRecord.SelectedItem != null)
            {
                LtbRecord.Items.Remove(LtbRecord.SelectedItem);
            }
        }

        //获取
        /// <summary>
        /// 获取随机姓氏
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetRandomSurname(SurnameType type)
        {
            Random random = new Random();
            int index = random.Next(0, Surnames.Count());
            string value = Surnames[index];
            switch (type)
            {
                case SurnameType.Both:
                    break;
                case SurnameType.Single:
                    while (value.Length == 2)
                    {
                        index = random.Next(0, Surnames.Count());
                        value = Surnames[index];
                    }
                    break;
                case SurnameType.Complex:
                    while (value.Length == 1)
                    {
                        index = random.Next(0, Surnames.Count());
                        value = Surnames[index];
                    }
                    break;
                default:
                    break;
            }
            return value;
        }
        /// <summary>
        /// 获取随机名字
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private string GetRandomName(int count)
        {
            List<string> chineseWords = GetChineseWords(count);
            string name = "";
            foreach (string item in chineseWords)
            {
                name += item;
            }
            return name;
        }
        /// <summary>
        /// 随机产生常用汉字
        /// </summary>
        /// <param name="count">要产生汉字的个数</param>
        /// <returns>常用汉字</returns>
        private List<string> GetChineseWords(int count)
        {
            List<string> chineseWords = new List<string>();
            Random rm = new Random();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding gb = Encoding.GetEncoding("gb2312");

            for (int i = 0; i < count; i++)
            {
                // 获取区码(常用汉字的区码范围为16-55)
                int regionCode = rm.Next(16, 56);
                // 获取位码(位码范围为1-94 由于55区的90,91,92,93,94为空,故将其排除)
                int positionCode;
                if (regionCode == 55)
                {
                    // 55区排除90,91,92,93,94
                    positionCode = rm.Next(1, 90);
                }
                else
                {
                    positionCode = rm.Next(1, 95);
                }

                // 转换区位码为机内码
                int regionCode_Machine = regionCode + 160;// 160即为十六进制的20H+80H=A0H
                int positionCode_Machine = positionCode + 160;// 160即为十六进制的20H+80H=A0H

                // 转换为汉字
                byte[] bytes = new byte[] { (byte)regionCode_Machine, (byte)positionCode_Machine };
                chineseWords.Add(gb.GetString(bytes));
            }

            return chineseWords;
        }
        /// <summary>
        /// 获取随机日期
        /// </summary>
        /// <returns></returns>
        private string GetRandomBirthday(DateTime min, DateTime max)
        {
            if (min < max)
            {
                TimeSpan ts = max - min;
                int count = (int)ts.TotalDays;

                Random random = new Random();
                int add = random.Next(0, count);
                TimeSpan addts = TimeSpan.FromDays(add);
                DateTime dt = min + addts;

                string str = dt.ToShortDateString();
                return str;
            }
            return "";
        }
        /// <summary>
        /// 获取随机性别
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetRandomGender(GenderType type)
        {
            Random random = new Random();
            string value;
            int index = 0;
            switch (type)
            {
                case GenderType.Two:
                    index = random.Next(0, 2);
                    break;
                case GenderType.Four:
                    index = random.Next(0, 4);
                    break;
                default:
                    break;
            }
            value = Genders[index];
            return value;
        }
        /// <summary>
        /// 获取随机身高
        /// </summary>
        /// <returns></returns>
        private string GetRandomHeight(int min, int max)
        {
            if (min < max)
            {
                Random random = new Random();
                string value = random.Next(min, max).ToString();
                return value;
            }
            return "";
        }
        /// <summary>
        /// 获取随机体重
        /// </summary>
        /// <returns></returns>
        private string GetRandomWeight(int min, int max)
        {
            if (min < max)
            {
                Random random = new Random();
                string value = random.Next(min, max).ToString();
                return value;
            }
            return "";
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
                    BtnFile.Background = BrushBG01;
                    BtnSetting.Background = BrushBG01;
                    BtnAbout.Background = BrushBG01;
                    break;
                case MenuTool.文件:
                    PanFile.Visibility = Visibility.Visible;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.Background = BrushBG02;
                    BtnSetting.Background = BrushBG01;
                    BtnAbout.Background = BrushBG01;
                    break;
                case MenuTool.编辑:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Visible;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    break;
                case MenuTool.设置:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Visible;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.Background = BrushBG01;
                    BtnSetting.Background = BrushBG02;
                    BtnAbout.Background = BrushBG01;
                    break;
                case MenuTool.关于:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Visible;
                    BtnFile.Background = BrushBG01;
                    BtnSetting.Background = BrushBG01;
                    BtnAbout.Background = BrushBG02;
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

        //检查
        private bool CheckIsCorrectRange()
        {
            bool isRight = true;
            if (Settings.Default.BirthdayMin >= Settings.Default.BirthdayMax)
            {
                ShowMessage(FindResource("生日范围错误").ToString());
                isRight = false;
            }
            if (Settings.Default.HeightMin >= Settings.Default.HeightMax)
            {
                ShowMessage(FindResource("身高范围错误").ToString());
                isRight = false;
            }
            if (Settings.Default.WeightMin >= Settings.Default.WeightMax)
            {
                ShowMessage(FindResource("体重范围错误").ToString());
                isRight = false;
            }
            return isRight;
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
            TxtUpdateNote.Text = AppInfo.ReleaseNote;
        }

        //显示
        protected override void ShowMessage(string message, bool newBox = false)
        {
            ShowMessage(LblMessage, message, newBox);
        }
        private void ShowRole(Role role)
        {
            TxtSurname.Text = role.Surname;
            TxtName.Text = role.Name;
            TxtBirthday.Text = role.Birthday;
            TxtGender.Text = role.Gender;
            TxtHeight.Text = role.Height;
            TxtWeight.Text = role.Weight;
        }
        private void ShowRole(ListBoxItem item)
        {
            if (item != null)
            {
                LtbRecord.SelectedItem = item;
                Role role = (Role)item.Tag;
                ShowRole(role);
            }
        }
        private void ShowCurrentRoleItem()
        {
            if (LtbRecord.SelectedItem != null)
            {
                ListBoxItem lb = (ListBoxItem)LtbRecord.SelectedItem;
                ShowRole(lb);
            }
        }

        //主窗口
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            //载入
            LoadLanguageItems(CbbLanguages);
            LoadThemeItems(CbbThemes);
            LoadRoleItems();

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
        protected override void EWindow_KeyUp(object sender, KeyEventArgs e)
        {
            base.EWindow_KeyUp(sender, e);

            //Ctrl+T 切换下个主题
            if (e.Key == Key.T && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            { SetNextTheme(CbbThemes, Settings.Default.Theme); }
        }

        //工具栏
        /// 文件
        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            CreateRole();
        }
        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveCurrentRoleItem();
        }
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            LtbRecord.Items.Clear();
        }
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (LtbRecord.SelectedItem != null)
            {
                ListBoxItem lb = (ListBoxItem)LtbRecord.SelectedItem;
                Role role = (Role)lb.Tag;
                Clipboard.SetDataObject(role.LongInfo, true);
                ShowMessage(FindResource("已复制").ToString());
            }
        }
        private void BtnExportAll_Click(object sender, RoutedEventArgs e)
        {
            if (LtbRecord.Items != null && LtbRecord.Items.Count > 0)
            {
                string str = "";
                foreach (var item in LtbRecord.Items)
                {
                    ListBoxItem lb = (ListBoxItem)item;
                    Role role = (Role)lb.Tag;
                    str += role.LongInfo + "\n\n";
                }
                str = str.TrimEnd('\n').TrimEnd('\n');
                Clipboard.SetDataObject(str, true);
                ShowMessage(FindResource("已复制").ToString());
            }
        }
        private void LtbRecord_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowCurrentRoleItem();
            }
            else if (e.Key == Key.Delete)
            {
                RemoveCurrentRoleItem();
            }
        }
        private void LtbRoles_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                ShowRole((ListBoxItem)sender);
            }
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
                string content = cbi.Tag.ToString();
                if (content == "自定义")
                {
                    if (string.IsNullOrEmpty(Settings.Default.ThemeCustomize))
                    {
                        Settings.Default.ThemeCustomize = ThemeItems[0].Value;
                    }
                    content = Settings.Default.ThemeCustomize;
                    PanColors.Visibility = Visibility.Visible;
                }
                else
                {
                    PanColors.Visibility = Visibility.Collapsed;
                }

                if (string.IsNullOrEmpty(content))
                {
                    CbbThemes.Items.Remove(cbi);
                    //设为默认主题
                    Settings.Default.Theme = 0;
                }
                else
                {
                    SetPanColors(PanColors, content);
                    ColorHelper.SetColors(Resources, content);
                }
                //立即刷新按钮样式
                SetMenuTool(CurrentMenuTool);
            }
        }
    }

    public class Role
    {
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Birthday { get; set; }
        public string Gender { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string LongInfo
        {
            get
            {
                string info = "";

                if (!string.IsNullOrEmpty(Surname) || !string.IsNullOrEmpty(Name))
                {
                    info += "姓名：" + Surname + Name + "\n";
                }
                if (!string.IsNullOrEmpty(Birthday))
                {
                    info += "生日：" + Birthday + "\n";
                }
                if (!string.IsNullOrEmpty(Gender))
                {
                    info += "性别：" + Gender + "\n";
                }
                if (!string.IsNullOrEmpty(Height))
                {
                    info += "身高：" + Height + "cm" + "\n";
                }
                if (!string.IsNullOrEmpty(Weight))
                {
                    info += "体重：" + Weight + "kg" + "\n";
                }

                info = info.TrimEnd('\n');
                return info;
            }
        }
        public string ShortInfo
        {
            get
            {
                string info = "{";
                info += Surname + ",";
                info += Name + ",";
                info += Birthday + ",";
                info += Gender + ",";
                info += Height + ",";
                info += Weight + "";
                info += "}";
                return info;
            }
        }
        public string DisplayInfo
        {
            get
            {
                string info = "";
                info += Surname;
                info += Name + " ";
                info += Gender;
                return info;
            }
        }

        public Role(string[] values)
        {
            Surname = values[0];
            Name = values[1];
            Birthday = values[2];
            Gender = values[3];
            Height = values[4];
            Weight = values[5];
        }
        public Role(string surname, string name, string birthday, string gender, string height, string weight)
        {
            Surname = surname;
            Name = name;
            Birthday = birthday;
            Gender = gender;
            Height = height;
            Weight = weight;
        }
    }

    public enum SurnameType
    {
        Both,
        Single,
        Complex,
    }
    public enum GenderType
    {
        Two,
        Four
    }
}
