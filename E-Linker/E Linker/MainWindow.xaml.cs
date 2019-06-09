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

namespace E_Linker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            Move.MouseLeftButtonDown += Node_MouseLeftButtonDown;
            Move.MouseLeftButtonUp += Node_MouseLeftButtonUp;
            Move.MouseMove += Node_MouseMove;
        }


        Point point = new Point();
        private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Move.ReleaseMouseCapture();
        }

        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            point = e.GetPosition(null);
            Move.CaptureMouse();
            Move.Cursor = Cursors.Hand;
        }

        private void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double dx = e.GetPosition(null).X - point.X + Node.Margin.Left;
                double dy = e.GetPosition(null).Y - point.Y + Node.Margin.Top;
                Node.Margin = new Thickness(dx, dy, 0, 0);
                point = e.GetPosition(null);
            }
        }
    }
}
