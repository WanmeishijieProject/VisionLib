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
    /// UC_AxisPos.xaml 的交互逻辑
    /// </summary>
    public partial class UC_AxisPos : UserControl
    {
        public UC_AxisPos()
        {
            InitializeComponent();
        }


        public const string CtrlFontSizePropertyName = "CtrlFontSize";
        public int CtrlFontSize
        {
            get
            {
                return (int)GetValue(CtrlFontSizeProperty);
            }
            set
            {
                SetValue(CtrlFontSizeProperty, value);
            }
        }
        public static readonly DependencyProperty CtrlFontSizeProperty = DependencyProperty.Register(
            CtrlFontSizePropertyName,
            typeof(int),
            typeof(UC_AxisPos));

        public const string HeaderBrushPropertyName = "HeaderBrush";
        public Brush HeaderBrush
        {
            get
            {
                return (Brush)GetValue(HeaderBrushProperty);
            }
            set
            {
                SetValue(HeaderBrushProperty, value);
            }
        }
        public static readonly DependencyProperty HeaderBrushProperty = DependencyProperty.Register(
            HeaderBrushPropertyName,
            typeof(Brush),
            typeof(UC_AxisPos));


        public const string LabelPosBrushPropertyName = "LabelPosBrush";
        public Brush LabelPosBrush
        {
            get
            {
                return (Brush)GetValue(LabelPosBrushProperty);
            }
            set
            {
                SetValue(LabelPosBrushProperty, value);
            }
        }
        public static readonly DependencyProperty LabelPosBrushProperty = DependencyProperty.Register(
            LabelPosBrushPropertyName,
            typeof(Brush),
            typeof(UC_AxisPos));


        public const string CtrlFontColorPropertyName = "CtrlFontColor";
        public Brush CtrlFontColor
        {
            get
            {
                return (Brush)GetValue(CtrlFontColorProperty);
            }
            set
            {
                SetValue(CtrlFontColorProperty, value);
            }
        }
        public static readonly DependencyProperty CtrlFontColorProperty = DependencyProperty.Register(
            CtrlFontColorPropertyName,
            typeof(Brush),
            typeof(UC_AxisPos),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 0, 0))));


        public const string ItemHeightPropertyName = "ItemHeight";
        public int ItemHeight
        {
            get
            {
                return (int)GetValue(ItemHeightProperty);
            }
            set
            {
                SetValue(ItemHeightProperty, value);
            }
        }
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            ItemHeightPropertyName,
            typeof(int),
            typeof(UC_AxisPos),
            new UIPropertyMetadata(20));
    }
}
