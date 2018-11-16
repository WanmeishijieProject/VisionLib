using GalaSoft.MvvmLight.Command;
using JPT_TosaTest.Classes;
using JPT_TosaTest.Model.ToolData;
using JPT_TosaTest.Vision;
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
    /// UC_PairPanel.xaml 的交互逻辑
    /// </summary>
    /// 

    public partial class UC_PairPanel : UserControl, INotifyPropertyChanged
    {
        private PairToolData ToolData = new PairToolData();
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

        public RelayCommand<ToolDataBase> SaveParaCommand
        {
            get
            {
                return GetValue(SaveParaCommandProperty) as RelayCommand<ToolDataBase>;
            }
            set
            {
                SetValue(SaveParaCommandProperty, value);
            }

        }
       
        public RelayCommand<ToolDataBase> UpdateParaCommand
        {
            get
            {
                return GetValue(UpdateParaCommandProperty) as RelayCommand<ToolDataBase>;
            }
            set
            {
                SetValue(UpdateParaCommandProperty, value);
            }

        }

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

        public static readonly DependencyProperty SaveParaCommandProperty = DependencyProperty.Register("SaveParaCommand", typeof(RelayCommand<ToolDataBase>), typeof(UC_PairPanel));

        public static readonly DependencyProperty UpdateParaCommandProperty = DependencyProperty.Register("UpdateParaCommand", typeof(RelayCommand<ToolDataBase>), typeof(UC_PairPanel));
        
        public static readonly DependencyProperty SaveCommandParameterProperty = DependencyProperty.Register("SaveCommandParameter", typeof(object), typeof(UC_PairPanel));
   
        public static readonly DependencyProperty UpdateCommandParameterProperty = DependencyProperty.Register("UpdateCommandParameter", typeof(object), typeof(UC_PairPanel));


        public PairToolData Data
        {
            get { return ToolData; }
        }

        private void BtnSavePara_Click(object sender, RoutedEventArgs e)
        {
            UpdatePairToolData();
            if (SaveParaCommand!=null)
                SaveParaCommand.Execute(SaveCommandParameter);
        }
        private void ExcuteUpdateCommand()
        {
            if (UpdateParaCommand != null)
            {
                UpdatePairToolData();
                UpdateParaCommand.Execute(UpdateCommandParameter);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        private void UpdatePairToolData()
        {
            if (int.TryParse(TbCaliberNum.Text, out int CaliperNum))
                ToolData.CaliperNum = CaliperNum;
            if (Enum.TryParse(CbSelectType.Text, out EnumPairType Polarity))
                ToolData.Polarity = Polarity;
            if (Enum.TryParse(CbSelectType.Text, out EnumSelectType SelectType))
                ToolData.SelectType = SelectType;
            if (int.TryParse(TbPairNum.Text, out int ExpectPairNum))
                ToolData.ExpectPairNum = ExpectPairNum;
            ToolData.Contrast = (int)SliderContrast.Value;
            ToolData.ModelName = cbModelName.Text;
        }
    }
}
