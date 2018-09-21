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
    public enum EnumWindowType
    {
        ROI,
        MODEL
    }
    public partial class Window_AddRoiModel : Window
    {
        public Window_AddRoiModel()
        {
            InitializeComponent();
        }
        private static MessageBoxResult _msgresult = MessageBoxResult.No;
        public static string ProfileValue = null;
        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            _msgresult = MessageBoxResult.Yes;
            if (EditBoxName.Text == "")
                UC_MessageBox.ShowMsgBox("名称不能为空", "错误");
            else
            {
                ProfileValue = EditBoxName.Text;
                Close();
            }
        }
        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            _msgresult = MessageBoxResult.No;
            ProfileValue = "";
            Close();
        }
        public static MessageBoxResult ShowWindowNewRoiModel(EnumWindowType type)
        {
            if (type == EnumWindowType.ROI)
            {
                Window_AddRoiModel dlg = new Window_AddRoiModel();
                dlg.ShowDialog();
                return _msgresult;
            }
            else
            {
                Window_AddRoiModel dlg = new Window_AddRoiModel();
                dlg.ShowDialog();
                return _msgresult;
            }
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
