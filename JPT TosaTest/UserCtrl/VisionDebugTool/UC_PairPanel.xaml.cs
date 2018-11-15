using GalaSoft.MvvmLight.Command;
using JPT_TosaTest.Classes;
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
    /// UC_PairPanel.xaml 的交互逻辑
    /// </summary>
    /// 

    public partial class UC_PairPanel : UserControl
    {
        public UC_PairPanel()
        {
            InitializeComponent();
        }
    
        private void CbPolarity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExcuteUpdateCommand();
        }

        private void CbSelectType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExcuteUpdateCommand();
        }

        private void SliderContrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ExcuteUpdateCommand();
        }

        private void TbCaliberNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            ExcuteUpdateCommand();
        }

        private void TbPairNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            ExcuteUpdateCommand();
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
        public static readonly DependencyProperty ModelListProperty = DependencyProperty.Register("ModelList", typeof(ObservableCollection<string>), typeof(UC_PairPanel));

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
        public static readonly DependencyProperty SaveParaCommandProperty = DependencyProperty.Register("SaveParaCommand", typeof(RelayCommand<string>), typeof(UC_PairPanel));

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
        public static readonly DependencyProperty SaveCommandParameterProperty = DependencyProperty.Register("SaveCommandParameter", typeof(object), typeof(UC_PairPanel));

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
        public static readonly DependencyProperty UpdateParaCommandProperty = DependencyProperty.Register("UpdateParaCommand", typeof(RelayCommand<string>), typeof(UC_PairPanel));

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
        public static readonly DependencyProperty UpdateCommandParameterProperty = DependencyProperty.Register("UpdateCommandParameter", typeof(object), typeof(UC_PairPanel));


        public string Data
        {
            get { return $"PairTool|{TbCaliberNum.Text}&{TbPairNum.Text}&{CbPolarity.Text}&{CbSelectType.Text}&{(int)SliderContrast.Value}&{cbModelName.Text}"; }
        }

        private void BtnSavePara_Click(object sender, RoutedEventArgs e)
        {
            SaveParaCommand.Execute(SaveCommandParameter);
        }
        private void ExcuteUpdateCommand()
        {
            if (UpdateParaCommand != null)
                UpdateParaCommand.Execute(UpdateCommandParameter);
        }
    }
}
