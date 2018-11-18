using GalaSoft.MvvmLight.Command;
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
    /// Interaction logic for UC_Flag.xaml
    /// </summary>
    public partial class UC_FlagPanel : UserControl , INotifyPropertyChanged
    {
        private FlagToolDaga ToolData = new FlagToolDaga();
        public UC_FlagPanel()
        {
            InitializeComponent();
            GeometryTypeCollect=new ObservableCollection<string>();
            string[] GtypeList = { "POINT", "LINE", "CIRCLE", "RECTANGLE1", "RECTANGLE2" };
            foreach(var type in GtypeList)
                GeometryTypeCollect.Add(type);
        }
        public ObservableCollection<string> LineList
        {
            get
            {
                return GetValue(LineListProperty) as ObservableCollection<string>;
            }
            set
            {
                SetValue(LineListProperty, value);
            }

        }
        public static readonly DependencyProperty LineListProperty = DependencyProperty.Register("LineList", typeof(ObservableCollection<string>), typeof(UC_FlagPanel));

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
        public static readonly DependencyProperty SaveParaCommandProperty = DependencyProperty.Register("SaveParaCommand", typeof(RelayCommand<ToolDataBase>), typeof(UC_FlagPanel));

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
        public static readonly DependencyProperty SaveCommandParameterProperty = DependencyProperty.Register("SaveCommandParameter", typeof(object), typeof(UC_FlagPanel));

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
        public static readonly DependencyProperty UpdateParaCommandProperty = DependencyProperty.Register("UpdateParaCommand", typeof(RelayCommand<ToolDataBase>), typeof(UC_FlagPanel));

        public RelayCommand<ToolDataBase> AddFlagCommand
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
        public static readonly DependencyProperty AddFlagCommandProperty = DependencyProperty.Register("AddFlagCommand", typeof(RelayCommand<ToolDataBase>), typeof(UC_FlagPanel));


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
        public static readonly DependencyProperty UpdateCommandParameterProperty = DependencyProperty.Register("UpdateCommandParameter", typeof(object), typeof(UC_FlagPanel));

        public object AddFlagCommandParameter
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
        public static readonly DependencyProperty AddFlagCommandParameterProperty = DependencyProperty.Register("AddFlagCommandParameter", typeof(object), typeof(UC_FlagPanel));



        public FlagToolDaga Data
        {
            get {
                UpdateTagToolData();
                return ToolData;
            }
            
        }

        private void BtnSavePara_Click(object sender, RoutedEventArgs e)
        {
            UpdateTagToolData();
            if (SaveParaCommand!=null)
                SaveParaCommand.Execute(SaveCommandParameter);
        }
        private void ExcuteUpdateCommand()
        {
            if (UpdateParaCommand != null)
            {
                UpdateTagToolData();
                UpdateParaCommand.Execute(UpdateCommandParameter);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        private void UpdateTagToolData()
        {
            if (Enum.TryParse(CbFlagType.Text, out EnumGeometryType GeometryType))
                ToolData.GeometryType = GeometryType;
            ToolData.L1Name = cbLine1.Text;
            ToolData.L2Name = cbLine2.Text;
            ToolData.HalconData = HalconVision.Instance.GeometryPosString;
        }

        public ObservableCollection<string> GeometryTypeCollect { get; set; }

        private void ButtonAddFlag_Click(object sender, RoutedEventArgs e)
        {
            UpdateTagToolData();
            if (AddFlagCommand != null)
                AddFlagCommand.Execute(AddFlagCommandParameter);
        }
    }
}
