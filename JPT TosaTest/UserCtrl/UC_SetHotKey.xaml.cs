using JPT_TosaTest.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JPT_TosaTest.UserCtrl
{
    /// <summary>
    /// UC_SetHotKey.xaml 的交互逻辑
    /// </summary>
    public partial class UC_SetHotKey : UserControl
    {
        public UC_SetHotKey()
        {
            InitializeComponent();
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
                (DataContext as SettingViewModel).RegisterHotKeyCommand.Execute(new Tuple<Window, bool>(Application.Current.MainWindow, bChecked));
            }
        }
    }
}
