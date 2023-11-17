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
    /// InputWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InputWindow 
    {
        public InputWindow()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public string InputText
        {
            get
            {
                return this.txt.Text;
            }
        }

    }
}
