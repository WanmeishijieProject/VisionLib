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
    /// UC_CameraViewListItem.xaml 的交互逻辑
    /// </summary>
    public partial class UC_CameraViewListItem : UserControl
    {
        public UC_CameraViewListItem()
        {
            InitializeComponent();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        public int CurCamID
        {
            get
            {
                return Convert.ToInt16(GetValue(CurCamIDProperty));
            }
            set
            {
                SetValue(CurCamIDProperty, value);
            }
        }
        public static DependencyProperty CurCamIDProperty = DependencyProperty.Register("CurCamID", typeof(int), typeof(UC_CameraViewListItem));
    }
}
