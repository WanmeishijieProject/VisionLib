using GalaSoft.MvvmLight.Command;
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
    /// UC_Aligment.xaml 的交互逻辑
    /// </summary>
    public partial class UC_Aligment : UserControl
    {
        public UC_Aligment()
        {
            InitializeComponent();
        }


        public const string StartCommandPropertyName = "StartCommand";
        public RelayCommand StartCommand
        {
            get
            {
                return (RelayCommand)GetValue(StartCommandProperty);
            }
            set
            {
                SetValue(StartCommandProperty, value);
            }
        }
        public static readonly DependencyProperty StartCommandProperty = DependencyProperty.Register(
            StartCommandPropertyName,
            typeof(RelayCommand),
            typeof(UC_Aligment));

        public const string StopCommandPropertyName = "StopCommand";
        public RelayCommand StopCommand
        {
            get
            {
                return (RelayCommand)GetValue(StopCommandProperty);
            }
            set
            {
                SetValue(StopCommandProperty, value);
            }
        }
        public static readonly DependencyProperty StopCommandProperty = DependencyProperty.Register(
            StopCommandPropertyName,
            typeof(RelayCommand),
            typeof(UC_Aligment));
    }
}
