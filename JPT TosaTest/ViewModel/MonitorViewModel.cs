using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JPT_TosaTest.Config;
using JPT_TosaTest.Config.SoftwareManager;
using JPT_TosaTest.IOCards;
using JPT_TosaTest.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JPT_TosaTest.ViewModel
{
    public class MonitorViewModel : ViewModelBase
    {
        private Task IoScanTask = null;
        private CancellationTokenSource cts = null;
        //所有板卡的IO数组
        private List<ObservableCollection<IOModel>> IOCollectionListInput { get; set; }
        private List<ObservableCollection<IOModel>> IOCollectionListOutput { get; set; }

        //单个板卡的IO
        private ObservableCollection<IOModel> _currentIoCardCollectionInput = null;
        private ObservableCollection<IOModel> _currentIoCardCollectionOutput = null;

        //当前板卡的序号
        private int CurrentIoCardIndex_Input = 0;
        private int CurrentIoCardIndex_Output=0;

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
        #endregion
        public MonitorViewModel()
        {
 
            //界面显示
            IOCollectionListInput = new List<ObservableCollection<IOModel>>();
            IOCollectionListOutput = new List<ObservableCollection<IOModel>>();
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
                        for (int j=0;j<16;j++)
                        {
                            var piIoName = ionamecfgType.GetProperty($"GP_{j + 1}");
                            string strIoName = piIoName.GetValue(ConfigMgr.Instance.SoftwareCfgMgr.IONames[i], null).ToString();
                            
                            IOModel iomodel = new IOModel()
                            {
                                CardName = iocard.Value.ioCfg.Name,
                                Index =startIndex+ j,
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
                                Index = startIndex+j,
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

            //启动后台扫描IO线程
            if (IoScanTask == null || IoScanTask.IsCanceled || IoScanTask.IsCompleted)
            {
                cts = new CancellationTokenSource();
                int[] OlddataArrInput = new int[10];
                int[] OlddataArrOutput = new int[10];
                int[] fakedata = new int[] { 23451, 87, 90, 975, 345, 853 };
                IoScanTask = new Task(() =>
                {
                    while (!cts.IsCancellationRequested)
                    {
                        Thread.Sleep(30);
                        int i = 0;

                        //更新状态
                        foreach (var card in IOCardMgr.Instance.IOCardDic)
                        {
#if TEST_IO
                            int dataInput = fakedata[i];
                            int dataOutput = fakedata[i + 1];
#else
                            card.Value.ReadIoInWord(0,out int dataInput);
                            card.Value.ReadIoOutWord(0,out int dataOutput);
#endif
                            if (dataInput != OlddataArrInput[i])
                            {
                                OlddataArrInput[i] = dataInput;
                                for (int j = 0; j < 16; j++)
                                {
                                    IOCollectionListInput[i][j].IsChecked = ((dataInput >> j) & 0x01) == 1 ? true : false;
                                }
                                if ((dataInput & 0x01) == 1)
                                    Console.WriteLine("急停按钮被按下");
                                if (((dataInput >> 1) & 0x01) == 1)
                                    Console.WriteLine("复位按钮被按下");
                                if (((dataInput >> 2) & 0x01) == 1)
                                    Console.WriteLine("启动按钮被按下");
                            }
                            if (dataOutput != OlddataArrOutput[i])
                            {
                                OlddataArrOutput[i] = dataOutput;
                                for (int j = 0; j < 16; j++)
                                {
                                    IOCollectionListOutput[i][j].IsChecked = ((dataOutput >> j) & 0x01) == 1 ? true : false;
                                }
                            }
                            i++;
                        }
                    }
                }, cts.Token);
            }
            IoScanTask.Start();
        }


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
        #endregion
    }
}
