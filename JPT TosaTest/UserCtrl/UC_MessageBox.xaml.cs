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

namespace JPT_TosaTest.UserCtrl
{
    /// <summary>
    /// UC_MessageBox.xaml 的交互逻辑
    /// </summary>
    public enum MsgType
    {
        Info,
        Error,
        Warning,
        Question,
        
    }
    public partial class UC_MessageBox : Window
    {
        private static MessageBoxResult result;
       
        private UC_MessageBox()
        {
            InitializeComponent();
            StrCaption = "Info";
            StrContent = "Message";
        }

        public static MessageBoxResult ShowMsgBox(string strContent, string strCaption = "Info",MsgType msgType=MsgType.Info)
        {
            //UC_MessageBox dlg = new UC_MessageBox();

            //dlg.StrCaption = strCaption;
            //dlg.StrContent = strContent;
            //dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //dlg.ShowDialog();
            //return result;
            MessageBoxImage image = MessageBoxImage.Error;
            switch (msgType)
            {
                case MsgType.Error:
                    image= MessageBoxImage.Error;
                    break;
                case MsgType.Info:
                    image = MessageBoxImage.Information;
                    break;
                case MsgType.Warning:
                    image = MessageBoxImage.Warning;
                    break;
                case MsgType.Question:
                    image = MessageBoxImage.Question;
                    break;
                default:
                    break;

            }
            result= MessageBox.Show(strContent, strCaption, MessageBoxButton.YesNo, image, MessageBoxResult.No);
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
    }
}
