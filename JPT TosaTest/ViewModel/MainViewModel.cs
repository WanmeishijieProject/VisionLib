using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JPT_TosaTest.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using JPT_TosaTest.UserCtrl;
using System.Threading;
using System.Collections.Generic;
using System.Windows;

namespace JPT_TosaTest.ViewModel
{
    public class MainViewModel : ViewModelBase
    {


        private int _viewIndex = 1;
        private int _errorCount = 0;
        private bool _boolShowInfoListBox = false;
        private AutoResetEvent OpenedEvent = new AutoResetEvent(true);
        private List<string> ErrList = null;
        private object[] stationLock = new object[10];

        public MainViewModel(IDataService dataService)
        {
            TestItemCollection = new ObservableCollection<TestItemModel>()
            {
                new TestItemModel(){ ItemName="Item1", PosX=19.876, PosY=22.987, ItemColor="Green"},
                new TestItemModel(){ ItemName="Item1", PosX=19.876, PosY=22.987, ItemColor="Green"},
                new TestItemModel(){ ItemName="Item1", PosX=19.876, PosY=22.987, ItemColor="Green"},
                new TestItemModel(){ ItemName="Item1", PosX=19.876, PosY=22.987, ItemColor="Green"},
            };
            ResultCollection = new ObservableCollection<ResultItem>()
            {
              new ResultItem(){ Index=1, HSG_X=1, HSG_Y=2, HSG_R=3, PLC_X=5, PLC_Y=6, PLC_R=7 }
            };
            SystemErrorMessageCollection = new ObservableCollection<MessageItem>();
            SystemErrorMessageCollection.CollectionChanged += SystemErrorMessageCollection_CollectionChanged;
            Config.ConfigMgr.Instance.LoadConfig(out ErrList);
            StationInfoCollection = new ObservableCollection<string>();
            foreach (var stationCfg in Config.ConfigMgr.Instance.SoftwareCfgMgr.WorkFlowConfigs)
            {
                StationInfoCollection.Add(stationCfg.Name);
            }
            foreach (var station in WorkFlow.WorkFlowMgr.Instance.stationDic)
            {
                station.Value.OnStationInfoChanged += Value_OnStationInfoChanged1; ;
            }
            for (int i = 0; i < 10; i++)
                stationLock[i] = new object();
        }

        private void Value_OnStationInfoChanged1(int Index, string StationName, string Msg)
        {
            lock (stationLock[Index])
            {
                StationInfoCollection[Index] = $"{StationName}:{Msg}";
            }
        }

   

        private void SystemErrorMessageCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var colls = from item in SystemErrorMessageCollection where item.MsgType == EnumMessageType.Error select item;
            if (colls != null)
                ErrorCount = colls.Count();
        }


        #region Property
        public int ViewIndex
        {
            get { return _viewIndex; }
            set
            {
                if (_viewIndex != value)
                {
                    _viewIndex = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ObservableCollection<TestItemModel> TestItemCollection
        {
            get;
            set;
        }
        public ObservableCollection<ResultItem> ResultCollection
        {
            get;
            set;
        }
        public bool BoolShowInfoListBox
        {
            get { return _boolShowInfoListBox; }
            set
            {
                if (_boolShowInfoListBox != value)
                {
                    _boolShowInfoListBox = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int ErrorCount
        {
            get { return _errorCount; }
            set
            {
                if (_errorCount != value)
                {
                    _errorCount = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ObservableCollection<MessageItem> SystemErrorMessageCollection
        {
            get;
            set;
        }
        public ObservableCollection<string> StationInfoCollection { get; set; }
        #endregion



        #region Command
        /// <summary>
        /// 切换中英文
        /// </summary>
        public RelayCommand<string> SwitchLangCommand
        {
            get
            {
                return new RelayCommand<string>(strLang => {
                    string langFileOld = null;
                    string langFileNew = null;
                    switch (strLang)
                    {
                        case "CH":
                            langFileNew = "Lang_CH";
                            langFileOld = "Lang_EN";
                            break;
                        case "EN":
                            langFileNew = "Lang_EN";
                            langFileOld = "Lang_CH";
                            break;
                        default:
                            break;
                    }
                    var MergedDic = Application.Current.Resources.MergedDictionaries;
                    if (!string.IsNullOrEmpty(langFileNew) && !string.IsNullOrEmpty(langFileOld))
                    {
                        foreach (ResourceDictionary dictionary in MergedDic)
                        {
                            if (dictionary.Source.OriginalString.Contains(langFileNew))
                            {
                                MergedDic.Remove(dictionary);
                                MergedDic.Add(dictionary);
                                break;
                            }
                        }
                    }
                });
            }
        }
        /// <summary>
        /// 窗口Load
        /// </summary>
        public RelayCommand WindowLoadedCommand
        {
            get { return new RelayCommand(() => {
               
                foreach (var err in ErrList)
                {
                    SystemErrorMessageCollection.Add(new MessageItem() { MsgType = EnumMessageType.Error, StrMsg = err });
                }
            }); }
        }

        /// <summary>
        /// 界面主菜单Home按钮
        /// </summary>
        public RelayCommand BtnHomeCommand
        {
            get { return new RelayCommand(() => ViewIndex = 1); }
        }

        /// <summary>
        /// 界面主菜单设置按钮
        /// </summary>
        public RelayCommand BtnSettingCommand
        {
            get { return new RelayCommand(() => ViewIndex = 2); }
        }

        /// <summary>
        /// 状态栏错误显示按钮
        /// </summary>
        public RelayCommand ShowInfoListCommand
        {
            get
            {
                return new RelayCommand(() =>
                      {
                          SystemErrorMessageCollection.Add(new MessageItem() { MsgType = EnumMessageType.Error, StrMsg = "Error message" });
                          ViewIndex = 3;
                      });
            }
        }

        /// <summary>
        /// 弹出菜单清除错误菜单
        /// </summary>
        public RelayCommand ClearMessageCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SystemErrorMessageCollection.Clear();
                });
            }
        }

        /// <summary>
        /// 增加Hsg示教位置
        /// </summary>
        public RelayCommand AddPreSetCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    TestItemCollection.Add(new TestItemModel() { ItemName = "Item1", PosX = 19.876, PosY = 22.987, ItemColor = "Green" });
                });
            }
        }

        /// <summary>
        /// 删除选择的示教位置
        /// </summary>
        public RelayCommand<int> RemoveSelectedItemCommand
        {
            get
            {
                return new RelayCommand<int>(nIndex =>
                {
                    if (nIndex >= 0)
                        TestItemCollection.RemoveAt(nIndex);
                });
            }
        }

        /// <summary>
        /// 主菜单弹出示教盒按钮
        /// </summary>
        public RelayCommand BtnTeachCommand
        {
            get
            {
                return new RelayCommand(() =>
               {

                   if (OpenedEvent.WaitOne(200))
                   {
                       Window_TeachBox dlg = new Window_TeachBox(ref OpenedEvent);
                       dlg.Show();
                   }
               });
            }
        }

        /// <summary>
        /// 主菜单弹出视觉设置界面
        /// </summary>
        public RelayCommand BtnCameraCommand
        {
            get { return new RelayCommand(() => ViewIndex = 4); }
        }

        /// <summary>
        /// 主界面运行按钮
        /// </summary>
        public RelayCommand StartStationCommand
        {
            get { return new RelayCommand(() => WorkFlow.WorkFlowMgr.Instance.StartAllStation()); }
        }
        public RelayCommand PauseStationCommand
        {
            get { return new RelayCommand(() => WorkFlow.WorkFlowMgr.Instance.PauseAllStation()); }
        }
        public RelayCommand StopStationCommand
        {
            get { return new RelayCommand(() => WorkFlow.WorkFlowMgr.Instance.StopAllStation());}
        }
        /// <summary>
        /// 主界面登陆按钮
        /// </summary>
        public RelayCommand BtnUserCommand
        {
            get
            {
                return new RelayCommand(() => ViewIndex = 5);
            }
        }
        #endregion
    }
}