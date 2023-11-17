using System;
using System.Windows;
using System.Windows.Controls;

namespace AutoCADWpf
{
    /// <summary>
    /// ladderWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ladderWindow 
    {
        public ladderWindow()
        {
            InitializeComponent();
        }

        public double LadderHeight
        {
            get
            {
                return Double.Parse(this.txtHeight.Text);
            }
        }

        public String LadderType
        {
            get
            {
                return (string)((ListBoxItem)this.lstTypes.SelectedItem).Content;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
