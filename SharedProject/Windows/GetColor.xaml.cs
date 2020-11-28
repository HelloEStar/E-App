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
using System.Windows.Shapes;

namespace SharedProject
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class GetColor : Window
    {
        private ColorPicker ColorPicker { get; set; }
        private Color OldColor { get; set; }
        private Color NewColor
        {
            get 
            {
                Color cl = new Color
                {
                    A = (byte)A,
                    R = (byte)R,
                    G = (byte)G,
                    B = (byte)B
                };
                return cl;
            }
        }
        public int A
        {
            get 
            {
                return (int)SldA.Value;
            }
            set 
            {
                if (SldA.Value != value)
                {
                    SldA.Value = value;
                }
            }
        }
        public int R
        {
            get
            {
                return (int)SldR.Value;
            }
            set
            {
                if (SldR.Value != value)
                {
                    SldR.Value = value;
                }
            }
        }
        public int G
        {
            get
            {
                return (int)SldG.Value;
            }
            set
            {
                if (SldG.Value != value)
                {
                    SldG.Value = value;
                }
            }
        }
        public int B
        {
            get
            {
                return (int)SldB.Value;
            }
            set
            {
                if (SldB.Value != value)
                {
                    SldB.Value = value;
                }
            }
        }

        public GetColor(ColorPicker cp)
        {
            ColorPicker = cp;
            Owner = cp.Owner;
            InitializeComponent();
        }

        private void Refresh()
        {
            TxtColor.Content = NewColor;
            RctColor.Fill = new SolidColorBrush(NewColor);

            TxtA.Text = A.ToString();
            TxtR.Text = R.ToString();
            TxtG.Text = G.ToString();
            TxtB.Text = B.ToString();
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            OldColor = ColorHelper.Get(ColorPicker.Color);
            A = OldColor.A;
            R = OldColor.R;
            G = OldColor.G;
            B = OldColor.B;

            Refresh();
        }
        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            ColorPicker.SetColor(NewColor);
            Close();
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SldA_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;
            Refresh();
        }
        private void SldR_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;
            Refresh();
        }
        private void SldG_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;
            Refresh();
        }
        private void SldB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;
            Refresh();
        }
        private void TxtA_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (int.TryParse(TxtA.Text, out int result))
            {
                A = result;
                Refresh();
            }
        }
        private void TxtR_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (int.TryParse(TxtR.Text, out int result))
            {
                R = result;
                Refresh();
            }
        }
        private void TxtG_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (int.TryParse(TxtG.Text, out int result))
            {
                G = result;
                Refresh();
            }
        }
        private void TxtB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (int.TryParse(TxtB.Text, out int result))
            {
                B = result;
                Refresh();
            }
        }
    }
}
