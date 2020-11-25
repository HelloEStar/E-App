using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SharedProject
{
    /// <summary>
    /// ColorPicker.xaml 的交互逻辑
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        public ColorPicker()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
            "Name", 
            typeof(string), 
            typeof(string), 
            new PropertyMetadata("Name", new PropertyChangedCallback(OnNameChanged)));
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color",
            typeof(string),
            typeof(Color), 
            new PropertyMetadata("Color", new PropertyChangedCallback(OnColorChanged)));


        public Color Color { get; set; }
        public string Name
        {
            get { return (string)GetValue(string); }
            set { SetValue(string, value); }
        }

        private static void OnNameChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            string source = (string)sender;
            source.LblName.Content = (string)args.NewValue;
        }
        private static void OnColorChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            string source = (string)sender;
            source.LblName.Content = (string)args.NewValue;
        }
    }
}
