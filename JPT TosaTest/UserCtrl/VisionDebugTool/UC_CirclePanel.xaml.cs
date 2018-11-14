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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JPT_TosaTest.UserCtrl.VisionDebugTool
{
    /// <summary>
    /// UC_CirclePanel.xaml 的交互逻辑
    /// </summary>
    public partial class UC_CirclePanel : UserControl
    {
        public UC_CirclePanel()
        {
            InitializeComponent();
        }
        public ObservableCollection<string> ModelList
        {
            get
            {
                return GetValue(ModelListProperty) as ObservableCollection<string>;
            }
            set
            {
                SetValue(ModelListProperty, value);
            }

        }
        public static readonly DependencyProperty ModelListProperty = DependencyProperty.Register("ModelList", typeof(ObservableCollection<string>), typeof(UC_CirclePanel));
    }
}
