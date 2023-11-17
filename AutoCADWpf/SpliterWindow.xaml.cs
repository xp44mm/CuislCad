using System.Windows;

namespace AutoCADWpf
{
    /// <summary>
    /// SpliterWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SpliterWindow 
    {
        public SpliterWindow()
        {
            InitializeComponent();
        }

        public double InputWidth
        {
            get
            {
                var res = double.Parse(this.txtWidth.Text);
                return res;
            }
        }

        public double Radius
        {
            get
            {
                var res = double.Parse(this.txtRadius.Text);
                return res;
            }
        }

        public double OutputWidth
        {
            get
            {
                var res = double.Parse(this.txtOutput.Text);
                return res;
            }
        }

        public double Angle
        {
            get
            {
                var res = double.Parse(this.txtAngle.Text);
                return res;
            }
        }

        public int Number
        {
            get
            {
                var res = int.Parse(this.txtNumber.Text);
                return res;
            }
        }

        public Spliter[] Spliters;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }

    public class Spliter
    {
        public double Radius { get; set; }

        public double OptRadius { get; set; }
    }
}
