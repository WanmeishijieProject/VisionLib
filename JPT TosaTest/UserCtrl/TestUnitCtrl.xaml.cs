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
    /// TestUnitCtrl.xaml 的交互逻辑
    /// </summary>
    public partial class TestUnitCtrl : UserControl
    {
        public TestUnitCtrl()
        {
            InitializeComponent();
        }

        public const string HeaderNamePropertyName = "HeaderName";
        public string HeaderName
        {
            get
            {
                return (string)GetValue(HeaderNameProperty);
            }
            set
            {
                SetValue(HeaderNameProperty, value);
            }
        }
        public static readonly DependencyProperty HeaderNameProperty = DependencyProperty.Register(HeaderNamePropertyName,typeof(string),typeof(TestUnitCtrl));


        public const string HeaderBackgroundPropertyName = "HeaderBackground";
        public Brush HeaderBackground
        {
            get
            {
                return (Brush)GetValue(HeaderBackgroundProperty);
            }
            set
            {
                SetValue(HeaderBackgroundProperty, value);
            }
        }
        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register(HeaderBackgroundPropertyName,typeof(Brush),typeof(TestUnitCtrl));

    }
}
