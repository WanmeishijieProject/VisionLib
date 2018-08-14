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
    /// UC_LogIn.xaml 的交互逻辑
    /// </summary>
    public partial class UC_LogIn : UserControl
    {
        public UC_LogIn()
        {
            InitializeComponent();
        }

        private void BtnLogIn_Click(object sender, RoutedEventArgs e)
        {
            (UsrTextBox.DataContext as LogInViewModel).LogInCommand.Execute(new Tuple<string,string>(UsrTextBox.Text,PsdTextBox.Password));
        }
    }
}
