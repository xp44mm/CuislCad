using System.Windows;
using System.Windows.Controls;

namespace AutoCADWpf
{
    /// <summary>
    /// SelectListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SelectListWindow 
    {
        public SelectListWindow()
        {
            InitializeComponent();
        }
        public ListBox ListBox
        {
            get
            {
                return lstStyles;
            }
        }

        private void drawButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
