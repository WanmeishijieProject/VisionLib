using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class UC_CirclePanel : UserControl , INotifyPropertyChanged
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


        public RelayCommand<string> SaveParaCommand
        {
            get
            {
                return GetValue(SaveParaCommandProperty) as RelayCommand<string>;
            }
            set
            {
                SetValue(SaveParaCommandProperty, value);
            }

        }
        public static readonly DependencyProperty SaveParaCommandProperty = DependencyProperty.Register("SaveParaCommand", typeof(RelayCommand<string>), typeof(UC_CirclePanel));

        public object SaveCommandParameter
        {
            get
            {
                return GetValue(SaveCommandParameterProperty);
            }
            set
            {
                SetValue(SaveCommandParameterProperty, value);
            }

        }
        public static readonly DependencyProperty SaveCommandParameterProperty = DependencyProperty.Register("SaveCommandParameter", typeof(object), typeof(UC_CirclePanel));

        public RelayCommand<string> UpdateParaCommand
        {
            get
            {
                return GetValue(UpdateParaCommandProperty) as RelayCommand<string>;
            }
            set
            {
                SetValue(UpdateParaCommandProperty, value);
            }

        }
        public static readonly DependencyProperty UpdateParaCommandProperty = DependencyProperty.Register("UpdateParaCommand", typeof(RelayCommand<string>), typeof(UC_CirclePanel));

        public object UpdateCommandParameter
        {
            get
            {
                return GetValue(UpdateCommandParameterProperty) as object;
            }
            set
            {
                SetValue(UpdateCommandParameterProperty, value);
            }

        }
        public static readonly DependencyProperty UpdateCommandParameterProperty = DependencyProperty.Register("UpdateCommandParameter", typeof(object), typeof(UC_CirclePanel));


        public string Data { get; set; }

        private void ExcuteUpdateCommand()
        {
            if (UpdateParaCommand != null)
            {
                RaisePropertyChanged("Data");
                UpdateParaCommand.Execute(UpdateCommandParameter);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
