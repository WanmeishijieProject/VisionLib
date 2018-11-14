using GalaSoft.MvvmLight;
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
    public class PairTool : ToolBase
    {
        #region Private
        private string _defaultPath = FileHelper.GetCurFilePathString() + @"VisionData\ToolData\";
        private int _caliperNumber = 30;
        private int _expectedPairNum = 2;
        private int _pairType = 0;  //light/dark, dark/light, all
        private int _selectType = 0;  //first，last, all
        private int _contrast = 20;
        #endregion

        #region Public
        public override string ToString()
        {
            return $"{GetType().Name}|{CaliperNumber}&{ExpectedPairNum}&{Polarity}&{SelectType}&{Contrast}&{ModelName}";
        }
        #endregion

        #region Command
        public RelayCommand SavePairParaCommand
        {
            get
            {
                return new RelayCommand(() => {
                    try
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
                            DefaultPath = sfd.FileName;
                            string strPara = $"{ToString()}|{HalconVision.Instance.PairRoiData}";
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
        public RelayCommand UpdatePairResultCommand
        {
            get
            {
                return new RelayCommand(() => {
                    try
                    {
                        EnumPairType PairType = (EnumPairType)this.Polarity;
                        EnumSelectType SelectType = (EnumSelectType)this.SelectType;                  
                        if (!string.IsNullOrEmpty(HalconVision.Instance.PairRoiData))
                            HalconVision.Instance.Debug_FindPair(0, PairType, SelectType, this.ExpectedPairNum, this.Contrast, this.CaliperNumber);                                 
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
        public int ExpectedPairNum
        {
            get { return _expectedPairNum; }
            set
            {
                if (value != _expectedPairNum)
                {
                    _expectedPairNum = value;
                    RaisePropertyChanged();
                }
            }
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
            get { return _pairType; }
            set
            {
                if (value != _pairType)
                {
                    _pairType = value;
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
