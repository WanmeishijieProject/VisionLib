using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JPT_TosaTest.Config;
using JPT_TosaTest.Config.SoftwareManager;
using JPT_TosaTest.IOCards;
using JPT_TosaTest.Model;
using JPT_TosaTest.MotionCards;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AxisParaLib;
using GalaSoft.MvvmLight.Messaging;

namespace JPT_TosaTest.ViewModel
{
    public class MonitorViewModel : ViewModelBase
    {
        #region Field
        private static int MAX_IOCARD_NUM = 10;
        //所有板卡的IO数组
        private List<ObservableCollection<IOModel>> IOCollectionListInput { get; set; }
        private List<ObservableCollection<IOModel>> IOCollectionListOutput { get; set; }

        //单个板卡的IO
        private ObservableCollection<IOModel> _currentIoCardCollectionInput = null;
        private ObservableCollection<IOModel> _currentIoCardCollectionOutput = null;

        //当前板卡的序号
        private int CurrentIoCardIndex_Input = 0;
        private int CurrentIoCardIndex_Output = 0;

        int[] OlddataArrInput = new int[MAX_IOCARD_NUM];    //记录旧的IO状态//应该不会超过10组IO卡
        int[] OlddataArrOutput = new int[MAX_IOCARD_NUM];

        private bool _allHomeOk;
        #endregion

        public MonitorViewModel()
        {
            #region 界面显示
            IOCollectionListInput = new List<ObservableCollection<IOModel>>();
            IOCollectionListOutput = new List<ObservableCollection<IOModel>>();
            AxisStateCollection = new ObservableCollection<AxisArgs>();
            foreach (var it in ConfigMgr.Instance.HardwareCfgMgr.AxisSettings)
            {
                var MotionCard = MotionMgr.Instance.FindMotionCardByAxisIndex(it.AxisNo);
                if (MotionCard != null)
                    AxisStateCollection.Add(MotionCard.AxisArgsList[it.AxisNo - MotionCard.MIN_AXIS]);
                else
                    AxisStateCollection.Add(new AxisArgs());
            }

            foreach (var iocard in IOCardMgr.Instance.IOCardDic)
            {
                ObservableCollection<IOModel> collect_input = new ObservableCollection<IOModel>();
                ObservableCollection<IOModel> collect_output = new ObservableCollection<IOModel>();

                var ionamecfgType = typeof(IoNameCfg);
                int i = 0;
                foreach (var ioname in ConfigMgr.Instance.SoftwareCfgMgr.IONames)
                {
                    //起始编号
                    var piStartIndex = ionamecfgType.GetProperty("StartIndex");
                    int startIndex = Convert.ToInt16(piStartIndex.GetValue(ConfigMgr.Instance.SoftwareCfgMgr.IONames[i], null));

                    if (ioname.Name == iocard.Value.ioCfg.IOName_Input)
                    {
                        //遍历每一个IObit名称
                        for (int j = 0; j < 16; j++)
                        {
                            var piIoName = ionamecfgType.GetProperty($"GP_{j + 1}");
                            string strIoName = piIoName.GetValue(ConfigMgr.Instance.SoftwareCfgMgr.IONames[i], null).ToString();

                            IOModel iomodel = new IOModel()
                            {
                                CardName = iocard.Value.ioCfg.Name,
                                Index = startIndex + j,
                                IsChecked = false,
                                IOName = strIoName,
                                IOType = EnumIoType.InPut
                            };
                            collect_input.Add(iomodel);
                        }

                    }
                    if (ioname.Name == iocard.Value.ioCfg.IOName_Output)
                    {
                        for (int j = 0; j < 16; j++)
                        {
                            var pi = ionamecfgType.GetProperty($"GP_{j + 1}");
                            string strIoName = pi.GetValue(ConfigMgr.Instance.SoftwareCfgMgr.IONames[i], null).ToString();
                            IOModel iomodel = new IOModel()
                            {
                                CardName = iocard.Value.ioCfg.Name,
                                Index = startIndex + j,
                                IsChecked = false,
                                IOName = strIoName,
                                IOType = EnumIoType.Output
                            };
                            collect_output.Add(iomodel);
                        }
                    }
                    ++i;
                }
                IOCollectionListInput.Add(collect_input);
                IOCollectionListOutput.Add(collect_output);
            }

            //初始化当前显示的IO板卡
            CurrentIoCardCollectionInput = IOCollectionListInput.Count > 0 ? IOCollectionListInput.First() : null;
            CurrentIoCardCollectionOutput = IOCollectionListOutput.Count > 0 ? IOCollectionListOutput.First() : null;

            #endregion

            #region 订阅事件
            foreach (var motionDic in MotionMgr.Instance.MotionDic)
            {
                motionDic.Value.OnAxisStateChanged += Value_OnAxisStateChanged; ;
                motionDic.Value.OnErrorOccured += Value_OnErrorOccured; ;
            }
            foreach (var iocardDic in IOCardMgr.Instance.IOCardDic)
            {
                iocardDic.Value.OnIOStateChanged += Value_OnIOStateChanged;
            }
            #endregion

        }

        #region Property

        public ObservableCollection<IOModel> CurrentIoCardCollectionInput
        {
            get { return _currentIoCardCollectionInput; }
            set {
                if (value != _currentIoCardCollectionInput)
                {
                    _currentIoCardCollectionInput = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ObservableCollection<IOModel> CurrentIoCardCollectionOutput
        {
            get { return _currentIoCardCollectionOutput; }
            set
            {
                if (value != _currentIoCardCollectionOutput)
                {
                    _currentIoCardCollectionOutput = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ObservableCollection<AxisArgs> AxisStateCollection { get; set; }

        #endregion
        
        #region Command
        public RelayCommand PreIoCardInputCommand
        {
            get { return new RelayCommand(()=> {
                if (CurrentIoCardIndex_Input - 1 >= 0)
                    CurrentIoCardCollectionInput = IOCollectionListInput[--CurrentIoCardIndex_Input];
            }); }
        }
        public RelayCommand NextIoCardInputCommand
        {
            get
            {
                return new RelayCommand(() => {
                    if(CurrentIoCardIndex_Input+1< IOCollectionListInput.Count)
                        CurrentIoCardCollectionInput = IOCollectionListInput[++CurrentIoCardIndex_Input];
                });
            }
        }
        public RelayCommand PreIoCardOutputCommand
        {
            get
            {
                return new RelayCommand(() => {
                    if (CurrentIoCardIndex_Output - 1 >= 0)
                        CurrentIoCardCollectionOutput = IOCollectionListOutput[--CurrentIoCardIndex_Output];
                });
            }
        }
        public RelayCommand NextIoCardOutputCommand
        {
            get
            {
                return new RelayCommand(() => {
                    if (CurrentIoCardIndex_Output + 1 < IOCollectionListOutput.Count)
                        CurrentIoCardCollectionOutput = IOCollectionListOutput[++CurrentIoCardIndex_Output];
                });
            }
        }

        public RelayCommand<int> ClickOutputCommand
        {
            get
            {
                return new RelayCommand<int>(index => {
                    Console.WriteLine(index);
                    IIO io = IOCards.IOCardMgr.Instance.FindIOCardByCardNo(CurrentIoCardIndex_Output);
                    bool value = IOCollectionListOutput[CurrentIoCardIndex_Output][index-1].IsChecked;
                    io.WriteIoOutBit(index - 1, !value);
                });
            }
        }

        public bool AllHomeOk
        {
            get { return _allHomeOk; }
            set {
                if (_allHomeOk != value)
                {
                    _allHomeOk = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Private

        private void Value_OnErrorOccured(IMotion sender, int ErrorCode, string ErrorMsg)
        {
            Messenger.Default.Send<string>($"Motion{sender.motionCfg.Name} error occured:{ErrorMsg}", "Error");
        }

        private void Value_OnAxisStateChanged(IMotion sender, int AxisNo, AxisArgs args)
        {
            int AxisNoTotal = AxisNo + sender.MIN_AXIS;
            for (int j = 0; j < ConfigMgr.Instance.HardwareCfgMgr.AxisSettings.Count(); j++)
            {
                if (ConfigMgr.Instance.HardwareCfgMgr.AxisSettings[j].AxisNo == AxisNoTotal)
                {
                    AxisStateCollection[j].CurAbsPos = args.CurAbsPos* AxisStateCollection[j].Unit.Factor;
                    AxisStateCollection[j].IsHomed = args.IsHomed;
                    AxisStateCollection[j].IsBusy = args.IsBusy;
                    AxisStateCollection[j].ErrorCode = args.ErrorCode;
                    break;
                }
            }
        }

        private void Value_OnIOStateChanged(IIO sender, EnumIOType IOType, ushort OldValue, ushort NewValue)
        {

            if (IOType == EnumIOType.OUTPUT) //Output
            {
                int i = 0;
                foreach (var it in IOCardMgr.Instance.IOCardDic)
                {
                    if (it.Value.ioCfg.Name == sender.ioCfg.Name)
                    {
                        if (NewValue != OlddataArrOutput[i])
                        {
                            OlddataArrOutput[i] = NewValue;
                            for (int j = 0; j < 16; j++)
                            {
                                IOCollectionListOutput[i][j].IsChecked = ((NewValue >> j) & 0x01) == 1 ? true : false;
                            }
                        }
                        break;
                    }
                    i++;
                }
            }
            else  //Input
            {
                int i = 0;
                foreach (var it in IOCardMgr.Instance.IOCardDic)
                {
                    if (it.Value.ioCfg.Name == sender.ioCfg.Name)
                    {
                        if (NewValue != OlddataArrInput[i])
                        {
                            OlddataArrInput[i] = NewValue;
                            for (int j = 0; j < 16; j++)
                            {
                                IOCollectionListInput[i][j].IsChecked = ((NewValue >> j) & 0x01) == 1 ? true : false;
                            }
                        }
                        break;
                    }
                    i++;
                }
            }
          
        }
        #endregion
    }
}
