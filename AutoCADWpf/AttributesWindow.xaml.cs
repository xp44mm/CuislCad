using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
    /// AttributesWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AttributesWindow
    {
        private Block[] blocks;
        private ObservableCollection<Modify> modifies;

        public AttributesWindow(Block[] blocks)
        {
            InitializeComponent();
            this.blocks = blocks;
            lstBlocks.ItemsSource = blocks;
            if (lstBlocks.Items.Count > 0 && lstBlocks.SelectedIndex < 0) { lstBlocks.SelectedIndex = 0; }

            modifies = new ObservableCollection<Modify>();
            lstModifies.ItemsSource = modifies;
        }

        
        private void lstBlocks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstAttributes.Items.Count > 0 && lstAttributes.SelectedIndex < 0) { lstAttributes.SelectedIndex = 0; }

        }


        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var textString = (string)this.lstTextStrings.SelectedItem;
            var newTextString = this.txtNewTextString.Text;
            this.modifies.Add(new Modify(){TextString=textString,NewTextString=newTextString});
            this.blocks[this.lstBlocks.SelectedIndex].Attributes[this.lstAttributes.SelectedIndex].TextStrings.RemoveAt(this.lstTextStrings.SelectedIndex);
            this.txtNewTextString.Clear();

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var modify = (Modify)((Button)sender).Tag;
            this.blocks[this.lstBlocks.SelectedIndex].Attributes[this.lstAttributes.SelectedIndex].TextStrings.Add(modify.TextString);
            this.modifies.Remove(modify);
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public string BlockName
        {
            get
            {
                return ((Block)lstBlocks.SelectedItem).Name;
            }
        }

        public string AttTag
        {
            get
            {
                return ((Att)lstAttributes.SelectedItem).Tag;
            }
        }

        public Modify[] Modifies
        {
            get
            {
                return modifies.ToArray();
            }
        }

    }//class

    public class Block {
        public string Name { get; set; }
        public Att[] Attributes { get; set; }   
    
    }

    public class Att { 
        public string Tag { get; set; }
        public ObservableCollection<string> TextStrings { get; set; }
    }

    public class Modify{
        public string TextString { get; set; }
        public string NewTextString { get; set; }

    
    }

}
