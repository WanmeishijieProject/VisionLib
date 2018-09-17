using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AxisParaLib;
using JPT_TosaTest.Classes;
using JPT_TosaTest.Model;
using JPT_TosaTest.ViewModel;

namespace JPT_TosaTest.UserCtrl
{
    /// <summary> 
    /// Window_TeachBox.xaml 的交互逻辑
    /// </summary>
    public partial class Window_TeachBox : Window
    {
        private AutoResetEvent OpenedEvent = null;
        public Window_TeachBox(ref AutoResetEvent OpenedEvent)
        {
            this.OpenedEvent = OpenedEvent;
            this.OpenedEvent.Reset();
            InitializeComponent();
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
            Focus();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.OpenedEvent.Set();
            Close();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            (sender as TextBox).Text = e.Key.ToString();
            e.Handled = true;
        }

        private void TextBox_KeyDown_1(object sender, KeyEventArgs e)
        {
            (sender as TextBox).Text = e.Key.ToString();
            e.Handled = true;
        }

        private void Cb_Usehotkey_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                bool bChecked = (bool)(sender as CheckBox).IsChecked;
                (DataContext as TeachBoxViewModel).RegisterHotKeyCommand.Execute(new Tuple<Window, bool>(this, bChecked));
            }
        }

    }
}
