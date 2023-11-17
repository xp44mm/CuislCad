using System.Windows;
using System.Windows.Controls;

namespace AutoCADWpf
{
    /// <summary>
    /// PartWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PartWindow 
    {
        public PartWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dgObjects.SelectedIndex = 0;
            cbViews.SelectedIndex = 0;
        }

        /// <summary>
        /// 被选中的数据对象
        /// </summary>
        public object Object
        {
            get
            {
                return dgObjects.SelectedItem;
            }
        }
        
        public string View
        {
            get
            {
                return (string) cbViews.SelectedItem;
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

    }
}
