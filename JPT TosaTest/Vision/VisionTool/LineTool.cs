using GalaSoft.MvvmLight.Command;
using JPT_TosaTest.Classes;
using JPT_TosaTest.UserCtrl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JPT_TosaTest.Vision.VisionTool
{
    public class LineTool : ToolBase, INotifyPropertyChanged
    {
        #region Private
        private string _defaultPath = FileHelper.GetCurFilePathString() + @"VisionData\ToolData\";
        private int _caliperNumber = 30;
        private int _polarity = 0;  //light/dark, dark/light, all
        private int _selectType = 0;  //first，last, all
        private int _contrast = 20;
        #endregion

        #region Public
        public override string ToString()
        {
            return $"{GetType().Name}|{CaliperNumber}&{Polarity}&{SelectType}&{Contrast}&{ModelName}";
        }

        #endregion

        #region Command
        public RelayCommand SaveLineParaCommand
        {
            get
            {
                return new RelayCommand(() => {
                    try
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Filter = "文本文件(*.para)|*.para|所有文件|*.*";//设置文件类型
                        sfd.FileName = "LinePara";//设置默认文件名
                        sfd.DefaultExt = "para";//设置默认格式（可以不设）
                        sfd.AddExtension = true;//设置自动在文件名中添加扩展名
                        sfd.RestoreDirectory = true;
                        sfd.InitialDirectory = DefaultPath;
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            DefaultPath = sfd.FileName;
                            string strPara = $"{ToString()}|{HalconVision.Instance.LineRoiData}";
                            File.WriteAllText(DefaultPath, strPara);
                        }
                    }
                    catch (Exception ex)
                    {
                        UC_MessageBox.ShowMsgBox("Error", ex.Message, MsgType.Error);
                    }
                });
            }
        }
        public RelayCommand UpdateLineResultCommand
        {
            get
            {
                return new RelayCommand(() => {
                    try
                    {
                        //找直线
                        EnumEdgeType EdgeType = (EnumEdgeType)this.Polarity;
                        EnumSelectType SelectType = (EnumSelectType)this.SelectType;
                        if (EdgeType != null && SelectType != null)
                        {
                            if (!string.IsNullOrEmpty(HalconVision.Instance.LineRoiData))
                                HalconVision.Instance.Debug_FindLine(0, EdgeType, SelectType, this.Contrast, this.CaliperNumber);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        UC_MessageBox.ShowMsgBox("Error", ex.Message, MsgType.Error);
                    }
                });
            }
        }
        #endregion

        #region  Property
        public override string DefaultPath
        {
            get { return _defaultPath; }
            set { _defaultPath = value; }
        }

        public int CaliperNumber
        {
            get { return _caliperNumber; }
            set
            {
                if (value != _caliperNumber)
                {
                    _caliperNumber = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int Polarity
        {
            get { return _polarity; }
            set
            {
                if (value != _polarity)
                {
                    _polarity = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int SelectType
        {
            get { return _selectType; }
            set
            {
                if (value != _selectType)
                {
                    _selectType = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int Contrast
        {

            get { return _contrast; }
            set
            {
                if (value != _contrast)
                {
                    _contrast = value;
                    RaisePropertyChanged();
                }
            }
        }
        public string ModelName
        {
            get;
            set;
        }
        #endregion

        
    }
}
