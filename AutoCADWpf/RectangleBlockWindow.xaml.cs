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

namespace AutoCADWpf
{
    /// <summary>
    /// RectangleBlockWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RectangleBlockWindow 
    {
        public RectangleBlockWindow()
        {
            InitializeComponent();
        }
        public double Dx { get{ return double.Parse(txtDx.Text);} }
        public double Dy { get { return double.Parse(txtDy.Text); } }
        public bool Up { get { return chkUp.IsChecked.Value; } }
        public bool Down { get { return chkDown.IsChecked.Value; } }

        private void draw_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
