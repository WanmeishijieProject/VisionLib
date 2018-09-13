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
        HotKey hotkey_Left ;
        HotKey hotkey_Right;
        HotKey hotkey_Up ;
        HotKey hotkey_Down ;
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

        private void Window_Closed(object sender, EventArgs e)
        {
            //hotkey_Left.UnRegisterHotKey();
            //hotkey_Right.UnRegisterHotKey();
            //hotkey_Up.UnRegisterHotKey();
            //hotkey_Down.UnRegisterHotKey();

        }

        private void Hotkey_Down_OnHotKey()
        {
            (DataContext as TeachBoxViewModel).DownCommand.Execute(null);
        }

        private void Hotkey_Up_OnHotKey()
        {
            (DataContext as TeachBoxViewModel).UpCommand.Execute(null);
        }

        private void Hotkey_Right_OnHotKey()
        {
            (DataContext as TeachBoxViewModel).RightCommand.Execute(null);
        }

        private void Hotkey_Left_OnHotKey()
        {
            (DataContext as TeachBoxViewModel).LeftCommand.Execute(null);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //hotkey_Left = new HotKey(this, HotKey.KeyFlags.MOD_NOREPEAT, System.Windows.Forms.Keys.Left);
            //hotkey_Right = new HotKey(this, HotKey.KeyFlags.MOD_NOREPEAT, System.Windows.Forms.Keys.Right);
            //hotkey_Up = new HotKey(this, HotKey.KeyFlags.MOD_NOREPEAT, System.Windows.Forms.Keys.Up);
            //hotkey_Down = new HotKey(this, HotKey.KeyFlags.MOD_NOREPEAT, System.Windows.Forms.Keys.Down);
            //hotkey_Left.OnHotKey += Hotkey_Left_OnHotKey;
            //hotkey_Right.OnHotKey += Hotkey_Right_OnHotKey;
            //hotkey_Up.OnHotKey += Hotkey_Up_OnHotKey;
            //hotkey_Down.OnHotKey += Hotkey_Down_OnHotKey;
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
                if ((sender as CheckBox).IsChecked == true)
                {
                    //(DataContext as MonitorViewModel).
                }
            }
        }

    }
}
