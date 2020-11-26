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

        public static readonly DependencyProperty ColorTargetProperty = DependencyProperty.Register(
            "ColorTarget", 
            typeof(string), 
            typeof(ColorPicker), 
            new PropertyMetadata("Color Target", new PropertyChangedCallback(OnColorTargetChanged)));
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color",
            typeof(string),
            typeof(ColorPicker), 
            new PropertyMetadata("Color", new PropertyChangedCallback(OnColorChanged)));


        public string Color
        {
            get { return (string)GetValue(ColorProperty);  }
            set { SetValue(ColorProperty, value); }
        }

        public string ColorTarget
        {
            get { return (string)GetValue(ColorTargetProperty); }
            set { SetValue(ColorTargetProperty, value); }
        }

        private static void OnColorTargetChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            ColorPicker source = (ColorPicker)sender;
            source.TxtName.Content = (string)args.NewValue;
        }
        private static void OnColorChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            ColorPicker source = (ColorPicker)sender;
            string str = (string)args.NewValue;
            source.TxtColor.Content = str;
            Brush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(str));
            source.BtnColor.Background = brush;
        }

        private void PanColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
        }
        private void BtnColor_Click(object sender, RoutedEventArgs e)
        {
            GetColor getColor = new GetColor();
            getColor.Show();
        }
    }
}