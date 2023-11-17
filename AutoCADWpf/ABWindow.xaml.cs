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
    /// ABWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ABWindow
    {
        public ABWindow()
        {
            InitializeComponent();
        }
        public double A
        {
            get
            {
                return double.Parse(this.txtA.Text);
            }
        }

        public double B
        {
            get
            {
                return double.Parse(this.txtB.Text);
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
