using System.Windows;

namespace AutoCADWpf
{
    /// <summary>
    /// TextHeightWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TextHeightWindow 
    {
        public double Scale { get; set; }

        public double FontHeight
        {
            get
            {
                return double.Parse(txtHeight.Text);
            }
        }

        public TextHeightWindow(double scale)
        {
            InitializeComponent();
            this.Scale = scale;
        }


        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtScale.Text = Scale.ToString();
        }
    }
}
