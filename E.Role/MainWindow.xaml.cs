using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using E.Utility;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.MessageBox;
using Settings = E.Role.Properties.Settings;
using Path = System.IO.Path;

namespace E.Role
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
        /// 当前菜单
        /// </summary>
        private MenuTool CurrentMenuTool { get; set; } = MenuTool.文件;
        /// <summary>
        /// 姓氏列表
        /// </summary>
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
        /// <summary>
        /// 性别列表
        /// </summary>
        public List<string> Genders { get; } = new List<string>()
        {
            "男","女", "无", "双"
        };
        #endregion


        #region 方法
        //构造
        /// <summary>
        /// 构造器
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

            Uri uri0 = new Uri("/文档/用户协议.md", UriKind.Relative);
            Stream src0 = System.Windows.Application.GetResourceStream(uri0).Stream;
            string userAgreement = new StreamReader(src0, Encoding.UTF8).ReadToEnd();

            Uri uri = new Uri("/文档/更新日志.md", UriKind.Relative);
            Stream src = System.Windows.Application.GetResourceStream(uri).Stream;
            string updateNote = new StreamReader(src, Encoding.UTF8).ReadToEnd().Replace("### ", "");

            string homePage = "https://github.com/HelloEStar/E.App/wiki/" + product.Product.Replace(" ", "-");
            string gitHubPage = "https://github.com/HelloEStar/E.App";
            string qqGroupLink = "http://jq.qq.com/?_wv=1027&k=5TQxcvR";
            string qqGroupNumber = "279807070";
            string bitCoinAddress = "19LHHVQzWJo8DemsanJhSZ4VNRtknyzR1q";
            AppInfo = new AppInfo(product.Product, description.Description, company.Company, copyright.Copyright, userAgreement, new Version(Application.ProductVersion), updateNote,
                                  homePage, gitHubPage, qqGroupLink, qqGroupNumber, bitCoinAddress);
        }
        /// <summary>
        /// 载入语言选项
        /// </summary>
        private void LoadLanguageItems()
        {
            List<LanguageItem> LanguageItems = new List<LanguageItem>()
            {
                new LanguageItem("中文（默认）", "zh_CN"),
                new LanguageItem("English", "en_US"),
            };

            CbbLanguages.Items.Clear();
            foreach (LanguageItem item in LanguageItems)
            {
                ComboBoxItem cbi = new ComboBoxItem
                {
                    Content = item.Name,
                    ToolTip = item.Value,
                    Tag = item.RD
                };
                CbbLanguages.Items.Add(cbi);
            }
        }
        /// <summary>
        /// 载入所有可用主题
        /// </summary>
        private void LoadThemeItems()
        {
            //创建皮肤文件夹
            if (!Directory.Exists(AppInfo.ThemeFolder))
            { Directory.CreateDirectory(AppInfo.ThemeFolder); }

            CbbThemes.Items.Clear();
            string[] _mySkins = Directory.GetFiles(AppInfo.ThemeFolder);
            foreach (string item in _mySkins)
            {
                string tmp = Path.GetExtension(item);
                if (tmp == ".ini" || tmp == ".INI")
                {
                    string tmp2 = INIOperator.ReadIniKeys("文件", "类型", item);
                    //若是主题配置文件
                    if (tmp2 == "主题")
                    {
                        ComboBoxItem cbi = new ComboBoxItem
                        {
                            Content = Path.GetFileNameWithoutExtension(item),
                            ToolTip = item
                        };
                        CbbThemes.Items.Add(cbi);
                    }
                }
            }
        }
        /// <summary>
        /// 载入角色生成记录
        /// </summary>
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

        ///打开

        ///关闭

        //保存
        /// <summary>
        /// 保存应用设置
        /// </summary>
        private void SaveSettings()
        {
            if (!CheckIsCorrectRange())
            {
                return;
            }

            Settings.Default.Save();
            ShowMessage(FindResource("已保存").ToString());
        }
        /// <summary>
        /// 保存角色
        /// </summary>
        private void SaveRoles()
        {
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
        }

        //创建
        /// <summary>
        /// 创建颜色
        /// </summary>
        /// <param name="text">ARGB色值，以点号分隔，0-255</param>
        /// <returns></returns>
        private static Color CreateColor(string text)
        {
            //MessageBox.Show(text);
            try
            {
                string[] colors = text.Split('.');
                byte red = byte.Parse(colors[0]);
                byte green = byte.Parse(colors[1]);
                byte blue = byte.Parse(colors[2]);
                byte alpha = byte.Parse(colors[3]);
                Color color = Color.FromArgb(alpha, red, green, blue);
                return color;
            }
            catch (Exception)
            {
                Color color = Color.FromArgb(255, 125, 125, 125);
                return color;
            }
        }
        /// <summary>
        /// 创建角色
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 创建列表元素
        /// </summary>
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
        /// <summary>
        /// 移除当前选中
        /// </summary>
        private void RemoveCurrentRoleItem()
        {
            if (LtbRecord.SelectedItem != null)
            {
                LtbRecord.Items.Remove(LtbRecord.SelectedItem);
            }
        }

        ///清空

        ///删除

        //获取
        /// <summary>
        /// 获取随机姓氏
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetRandomSurname(SurnameType type)
        {
            Random random = new Random();
            string value = "";
            int index = random.Next(0, Surnames.Count());
            value = Surnames[index];
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
        /// <summary>
        /// 设置菜单
        /// </summary>
        /// <param name="menu"></param>
        private void SetMenuTool(MenuTool menu)
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
        /// <summary>
        /// 设置语言选项
        /// </summary>
        /// <param name="language">语言简拼</param>
        private void SetLanguage(int index)
        {
            Settings.Default.Language = index;
        }
        /// <summary>
        /// 设置主题选项
        /// </summary>
        /// <param name="themePath">主题路径</param>
        private void SetTheme(int index)
        {
            Settings.Default.Theme = index;
        }
        /// <summary>
        /// 切换下个主题显示
        /// </summary>
        private void SetNextTheme()
        {
            int index = Settings.Default.Theme;
            index++;
            if (index > CbbThemes.Items.Count - 1)
            {
                index = 0;
            }
            SetTheme(index);
        }
        /// <summary>
        /// 重置主题颜色
        /// </summary>
        /// <param name="themePath">主题文件路径</param>
        private void SetSkin(string themePath)
        {
            SetColor("一级字体颜色", CreateColor(INIOperator.ReadIniKeys("字体", "一级字体", themePath)));
            SetColor("二级字体颜色", CreateColor(INIOperator.ReadIniKeys("字体", "二级字体", themePath)));
            SetColor("三级字体颜色", CreateColor(INIOperator.ReadIniKeys("字体", "三级字体", themePath)));

            SetColor("一级背景颜色", CreateColor(INIOperator.ReadIniKeys("背景", "一级背景", themePath)));
            SetColor("二级背景颜色", CreateColor(INIOperator.ReadIniKeys("背景", "二级背景", themePath)));
            SetColor("三级背景颜色", CreateColor(INIOperator.ReadIniKeys("背景", "三级背景", themePath)));

            SetColor("一级边框颜色", CreateColor(INIOperator.ReadIniKeys("边框", "一级边框", themePath)));

            SetColor("有焦点选中颜色", CreateColor(INIOperator.ReadIniKeys("高亮", "有焦点选中", themePath)));
            SetColor("无焦点选中颜色", CreateColor(INIOperator.ReadIniKeys("高亮", "无焦点选中", themePath)));
        }
        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="colorName"></param>
        /// <param name="c"></param>
        public void SetColor(string colorName, Color c)
        {
            Resources.Remove(colorName);
            Resources.Add(colorName, new SolidColorBrush(c));
        }

        //重置
        /// <summary>
        /// 重置应用设置
        /// </summary>
        private void ResetSettings()
        {
            Settings.Default.Reset();
            ShowMessage(FindResource("已重置").ToString());
        }

        ///选择

        //检查
        /// <summary>
        /// 检查范围是否正确
        /// </summary>
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
        /// <summary>
        /// 刷新软件信息
        /// </summary>
        private void RefreshAppInfo()
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
        /// <summary>
        /// 刷新主窗口标题
        /// </summary>
        public void RefreshTitle()
        {
            string str = AppInfo.Name + " " + AppInfo.VersionShort;
            Main.Title = str;
        }

        //显示
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="resourceName">资源名</param>
        /// <param name="newBox">是否弹出对话框</param>
        private void ShowMessage(string message, bool newBox = false)
        {

            if (newBox)
            {
                MessageBox.Show(message);
            }
            else
            {
                if (LblMessage != null)
                {
                    //实例化一个DoubleAnimation类。
                    DoubleAnimation doubleAnimation = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = new Duration(TimeSpan.FromSeconds(3))
                    };
                    //为元素设置BeginAnimation方法。
                    LblMessage.BeginAnimation(OpacityProperty, doubleAnimation);

                    LblMessage.Content = message;
                }
            }
        }
        /// <summary>
        /// 显示角色
        /// </summary>
        private void ShowRole(Role role)
        {
            TxtSurname.Text = role.Surname;
            TxtName.Text = role.Name;
            TxtBirthday.Text = role.Birthday;
            TxtGender.Text = role.Gender;
            TxtHeight.Text = role.Height;
            TxtWeight.Text = role.Weight;
        }
        /// <summary>
        /// 显示角色
        /// </summary>
        /// <param name="item"></param>
        private void ShowRole(ListBoxItem item)
        {
            if (item != null)
            {
                LtbRecord.SelectedItem = item;
                Role role = (Role)item.Tag;
                ShowRole(role);
            }
        }
        /// <summary>
        /// 显示当前选择角色
        /// </summary>
        private void ShowCurrentRoleItem()
        {
            if (LtbRecord.SelectedItem != null)
            {
                ListBoxItem lb = (ListBoxItem)LtbRecord.SelectedItem;
                ShowRole(lb);
            }
        }

        //切换
        /// <summary>
        /// 切换工具面板
        /// </summary>
        private void SwitchMenuToolFile()
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
        private void SwitchMenuToolEdit()
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
        private void SwitchMenuToolSetting()
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
        private void SwitchMenuToolAbout()
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

        #endregion


        #region 事件
        //主窗口
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            //载入
            LoadAppInfo();
            LoadLanguageItems();
            LoadThemeItems();
            LoadRoleItems();

            //刷新
            RefreshAppInfo();
            RefreshTitle();

            //提示消息
            ShowMessage(FindResource("已载入").ToString());
        }
        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveRoles();
            SaveSettings();
        }
        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            //Ctrl+T 切换下个主题
            if (e.Key == Key.T && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            { SetNextTheme(); }

            //关于菜单
            if (e.Key == Key.F1)
            { Process.Start("explorer.exe", AppInfo.HomePage); }
            else if (e.Key == Key.F2)
            { Process.Start("explorer.exe", AppInfo.GitHubPage); }
            else if (e.Key == Key.F3)
            { Process.Start("explorer.exe", AppInfo.QQGroupLink); }
        }

        //菜单栏
        private void BtnFile_Click(object sender, RoutedEventArgs e)
        {
            SwitchMenuToolFile();
        }
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            SwitchMenuToolEdit();
        }
        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            SwitchMenuToolSetting();
        }
        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            SwitchMenuToolAbout();
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
            if (e.ClickCount == 2)
            {
                ShowRole((ListBoxItem)sender);
            }
        }
        ///编辑
        ///设置
        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
        private void BtnResetSettings_Click(object sender, RoutedEventArgs e)
        {
            ResetSettings();
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
                    SetLanguage(0);
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
                    SetSkin(themePath);
                }
                else
                {
                    CbbThemes.Items.Remove(cbi);
                    //设为默认主题
                    SetTheme(0);
                }
            }
        }


        ///关于
        private void BtnBitCoinAddress_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetDataObject(TxtBitCoinAddress.Text, true);
            ShowMessage(FindResource("已复制").ToString());
        }
        private void BtnHomePage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.HomePage);
        }
        private void BtnGitHubPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.GitHubPage);
        }
        private void BtnQQGroup_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.QQGroupLink);
        }

        //工作区
        #endregion
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

    public class TimeSpanDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((TimeSpan)value).TotalSeconds;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TimeSpan.FromSeconds((double)value);
        }
    }
    public class VisibilityBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Visibility)value) == Visibility.Visible;
        }
    }

    public class LanguageItem : ResourceDictionary
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public ResourceDictionary RD { get; set; }

        public LanguageItem(string name, string value)
        {
            Name = name;
            Value = value;
            Uri uri = new Uri(@"语言\" + value + ".xaml", UriKind.Relative);
            ResourceDictionary rd = System.Windows.Application.LoadComponent(uri) as ResourceDictionary;
            RD = rd;
        }
    }
}
