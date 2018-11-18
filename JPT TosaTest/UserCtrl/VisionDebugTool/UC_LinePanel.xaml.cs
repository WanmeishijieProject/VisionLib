using GalaSoft.MvvmLight.Command;
using JPT_TosaTest.Classes;
using JPT_TosaTest.Model.ToolData;
using JPT_TosaTest.Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
    /// UC_LinePanel.xaml 的交互逻辑
    /// </summary>
    public partial class UC_LinePanel : UserControl, INotifyPropertyChanged
    {
        private LineToolData ToolData = new LineToolData();
        public UC_LinePanel()
        {
            InitializeComponent();

            PolarityCollect = new ObservableCollection<string>();
            SelectTypeCollect = new ObservableCollection<string>();
            var L1 =new List<string> { "First","Last","All"};
            var L2 = new List<string> { "LightToDark", "DarkToLight", "All" };
            foreach (var str in L1)
            {
                SelectTypeCollect.Add(str);
            }
            foreach (var str in L2)
            {
                PolarityCollect.Add(str);
            }

        }
        private void SliderContrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ExcuteUpdateCommand();
        }
        private void TbCaliberNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            ExcuteUpdateCommand();
        }
        private void CbSelectType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExcuteUpdateCommand();
        }
        private void CbPolarity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExcuteUpdateCommand();
        }

        public ObservableCollection<string> ModelList
        {
            get {
                return GetValue(ModelListProperty) as ObservableCollection<string>;
            }
            set
            {
                SetValue(ModelListProperty, value);
            }

        }
        public static readonly DependencyProperty ModelListProperty = DependencyProperty.Register("ModelList", typeof(ObservableCollection<string>), typeof(UC_LinePanel));

       
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
        public static readonly DependencyProperty SaveParaCommandProperty = DependencyProperty.Register("SaveParaCommand", typeof(RelayCommand<ToolDataBase>), typeof(UC_LinePanel));

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
        public static readonly DependencyProperty SaveCommandParameterProperty = DependencyProperty.Register("SaveCommandParameter", typeof(object), typeof(UC_LinePanel));

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
        public static readonly DependencyProperty UpdateParaCommandProperty = DependencyProperty.Register("UpdateParaCommand", typeof(RelayCommand<ToolDataBase>), typeof(UC_LinePanel));

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
       


        //#region Command
        //public RelayCommand SaveLineParaCommand
        //{
        //    get
        //    {
        //        return new RelayCommand(() => {
        //            try
        //            {
        //                SaveFileDialog sfd = new SaveFileDialog();
        //                sfd.Filter = "文本文件(*.para)|*.para|所有文件|*.*";//设置文件类型
        //                sfd.FileName = "LinePara";//设置默认文件名
        //                sfd.DefaultExt = "para";//设置默认格式（可以不设）
        //                sfd.AddExtension = true;//设置自动在文件名中添加扩展名
        //                sfd.RestoreDirectory = true;
        //                sfd.InitialDirectory = DefaultPath;
        //                if (sfd.ShowDialog() == DialogResult.OK)
        //                {
        //                    DefaultPath = sfd.FileName;
        //                    string strPara = $"{Data}|{HalconVision.Instance.LineRoiData}";
        //                    File.WriteAllText(DefaultPath, strPara);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                UC_MessageBox.ShowMsgBox("Error", ex.Message, MsgType.Error);
        //            }
        //        });
        //    }
        //}
        //public RelayCommand UpdateLineDataResultCommand
        //{
        //    get
        //    {
        //        return new RelayCommand(() => {
        //            try
        //            {
        //                //找直线
        //                EnumEdgeType EdgeType = (EnumEdgeType)(CbPolarity.SelectedIndex);
        //                EnumSelectType SelectType = (EnumSelectType)(CbSelectType.SelectedIndex);
        //                int.TryParse(TbCaliberNum.Text,out int CaliberNum);
        //                if (EdgeType != null && SelectType != null)
        //                {
        //                    if (!string.IsNullOrEmpty(HalconVision.Instance.LineRoiData))
        //                        HalconVision.Instance.Debug_FindLine(0, EdgeType, SelectType, (int)SliderContrast.Value, CaliberNum);
        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                UC_MessageBox.ShowMsgBox("Error", ex.Message, MsgType.Error);
        //            }
        //        });
        //    }
        //}
        //#endregion

        public LineToolData Data
        {
            get { UpdateLineToolData(); return ToolData; }
        }

      

        private void BtnSavePara_Click(object sender, RoutedEventArgs e)
        {
            UpdateLineToolData();
            if (SaveParaCommand!=null)
                SaveParaCommand.Execute(SaveCommandParameter);
        }
        private void ExcuteUpdateCommand()
        {
            if (UpdateParaCommand != null)
            {
                UpdateLineToolData();
                UpdateParaCommand.Execute(UpdateCommandParameter);
            }
        }

        public static readonly DependencyProperty UpdateCommandParameterProperty = DependencyProperty.Register("UpdateCommandParameter", typeof(object), typeof(UC_LinePanel));

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string PropertyName = "")
        {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(PropertyName));
        }

        private void UpdateLineToolData()
        {
            if (int.TryParse(TbCaliberNum.Text, out int CaliperNum))
                ToolData.CaliperNum = CaliperNum;
            if (Enum.TryParse(CbPolarity.Text, out EnumEdgeType Polarity))
                ToolData.Polarity = Polarity;
            if (Enum.TryParse(CbSelectType.Text, out EnumSelectType SelectType))
                ToolData.SelectType = SelectType;

            ToolData.Contrast = (int)SliderContrast.Value;
            ToolData.ModelName = cbModelName.Text;
            ToolData.HalconData = HalconVision.Instance.LineRoiData;
           
        }

        public ObservableCollection<string> PolarityCollect { get; set; }
        public ObservableCollection<string> SelectTypeCollect { get; set; }
    }
}
