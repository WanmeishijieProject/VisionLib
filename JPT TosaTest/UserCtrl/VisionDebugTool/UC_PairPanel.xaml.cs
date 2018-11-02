using GalaSoft.MvvmLight.Command;
using JPT_TosaTest.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
    public partial class UC_PairPanel : System.Windows.Controls.UserControl
    {
        private string DefaultPath = FileHelper.GetCurFilePathString() + @"VisionData\ToolData\PairToolData\";
        public UC_PairPanel()
        {
            InitializeComponent();
        }
        private void BtnSavePairPara_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "文本文件(*.para)|*.para|所有文件|*.*";//设置文件类型
            sfd.FileName = "PairPara";//设置默认文件名
            sfd.DefaultExt = "para";//设置默认格式（可以不设）
            sfd.AddExtension = true;//设置自动在文件名中添加扩展名
            sfd.RestoreDirectory = true;
            sfd.InitialDirectory = DefaultPath;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                SaveParaCommand.Execute(new Tuple<string, string>(sfd.FileName, PairPara));
            }
        }


        public const string SaveParaCommandPropertyName = "SaveParaCommand";
        public RelayCommand<Tuple<string, string>> SaveParaCommand
        {
            get
            {
                return (RelayCommand<Tuple<string, string>>)GetValue(SaveParaCommandProperty);
            }
            set
            {
                SetValue(SaveParaCommandProperty, value);
            }
        }
        public static readonly DependencyProperty SaveParaCommandProperty = DependencyProperty.Register(
            SaveParaCommandPropertyName,
            typeof(RelayCommand<Tuple<string, string>>),
            typeof(UC_PairPanel));



        public const string UpdateParaCommandPropertyName = "UpdateParaCommand";
        public RelayCommand<string> UpdateParaCommand
        {
            get
            {
                return (RelayCommand<string>)GetValue(UpdateParaCommandProperty);
            }
            set
            {
                SetValue(UpdateParaCommandProperty, value);
            }
        }
        public static readonly DependencyProperty UpdateParaCommandProperty = DependencyProperty.Register(
            UpdateParaCommandPropertyName,
            typeof(RelayCommand<string>),
            typeof(UC_PairPanel));




        public string PairPara
        {
            get { return $"{TbCaliberNum.Text}&{CbPolarity.Text}&{CbSelectType.Text}&{SliderContrast.Value}"; }
        }


        private void SliderContrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (UpdateParaCommand != null)
                UpdateParaCommand.Execute(PairPara);
        }

        private void CbPolarity_Selected(object sender, RoutedEventArgs e)
        {
            if (UpdateParaCommand != null)
                UpdateParaCommand.Execute(PairPara);
        }

        private void TbCaliberNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UpdateParaCommand != null)
                UpdateParaCommand.Execute(PairPara);
        }
    }
}
