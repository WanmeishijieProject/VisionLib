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
using System.Windows.Shapes;

namespace CameraDebugLib
{
    /// <summary>
    /// UC_MessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class UC_MessageBox : Window
    {
        private static MessageBoxResult result;

        private UC_MessageBox()
        {
            InitializeComponent();
            StrCaption = "GPAS";
            StrContent = "Message";
        }

        public static MessageBoxResult ShowMsgBox(string strContent, string strCaption = "Info")
        {
            UC_MessageBox dlg = new UC_MessageBox();
            dlg.StrCaption = strCaption;
            dlg.StrContent = strContent;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dlg.ShowDialog();
            return result;
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Yes;
            Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.No;
            Close();
        }
        public string StrCaption { get { return GetValue(StrCaptionProperty).ToString(); } set { SetValue(StrCaptionProperty, value); } }
        public string StrContent { get { return GetValue(StrContentProperty).ToString(); } set { SetValue(StrContentProperty, value); } }
        public static readonly DependencyProperty StrCaptionProperty = DependencyProperty.Register("StrCaption", typeof(string), typeof(UC_MessageBox));
        public static readonly DependencyProperty StrContentProperty = DependencyProperty.Register("StrContent", typeof(string), typeof(UC_MessageBox));

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
