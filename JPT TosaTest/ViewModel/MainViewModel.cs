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
using GalaSoft.MvvmLight.Messaging;
using System.Data;
using AxisParaLib;
using System.Windows.Controls;
using JPT_TosaTest.Classes;
using JPT_TosaTest.MotionCards;
using AxisParaLib.UnitManager;
using JPT_TosaTest.Vision;
using System.IO;
using GalaSoft.MvvmLight.Ioc;

namespace JPT_TosaTest.ViewModel
{
    public enum EnumSystemState
    {
        Running,
        Pause,
        Idle,
    }

    public class MainViewModel : ViewModelBase
    {
        private int _viewIndex = 1;
        private int _errorCount = 0;
        private bool _boolShowInfoListBox = false;
        private AutoResetEvent OpenedEvent = new AutoResetEvent(true);
        private List<string> ErrList = null;
        private object[] stationLock = new object[10];
        private bool _showSnakeInfoBar = false;
        private string _snakeLastError = "";
        private LogExcel PrePointSetExcel;
        private string POINT_FILE = "Config/Point.xls";
        private object Hom_2D=null, ModelPos=null;
        private EnumSystemState _systemState = EnumSystemState.Idle;
        private MonitorViewModel MonitorVm = null;
        private const int HOME_PAGE = 1, SETTING_PAGE = 2, INFO_PAGE=3,
            CAMERA_PAGE = 4, MONITOR_PAGE = 5, USER_PAGE = 6;


        public MainViewModel(IDataService dataService)
        {
            //注册错误显示消息
            Messenger.Default.Register<string>(this, "Error", msg => {
                Application.Current.Dispatcher.Invoke(()=>ShowErrorinfo(msg));
            });

            SupportIsSelected = true;

            ResultCollection = new ObservableCollection<ResultItem>()
            {
              new ResultItem(){ Index=1, HSG_X=1, HSG_Y=2, HSG_R=3, PLC_X=5, PLC_Y=6, PLC_R=7 }
            };
            SystemErrorMessageCollection = new ObservableCollection<MessageItem>();
            SystemErrorMessageCollection.CollectionChanged += SystemErrorMessageCollection_CollectionChanged;

            //加载配置文件
            Config.ConfigMgr.Instance.LoadConfig(out ErrList);
            StationInfoCollection = new ObservableCollection<string>();
            foreach (var stationCfg in Config.ConfigMgr.Instance.SoftwareCfgMgr.WorkFlowConfigs)
            {
                if (stationCfg.Enable)
                {
                    StationInfoCollection.Add(stationCfg.Name);
                }
                else
                {
                    //
                }
            }
            foreach (var station in WorkFlow.WorkFlowMgr.Instance.stationDic)
            {
                station.Value.OnStationInfoChanged += Value_OnStationInfoChanged1; ;
            }
            for (int i = 0; i < 10; i++)
                stationLock[i] = new object();




            // 初始化示教点
            List<string> PrePointColumns = new List<string>();
            DataForPreSetPosition = new DataTable();
            DataForPreSetPosition.Columns.Add("PointName");
            PrePointColumns.Add("PointName");
            foreach (var it in Config.ConfigMgr.Instance.HardwareCfgMgr.AxisSettings)
            {
                DataForPreSetPosition.Columns.Add(it.AxisName);
                PrePointColumns.Add(it.AxisName);
            }


            //读取预设点文件
            PrePointSetExcel = new LogExcel();
            PrePointSetExcel.CreateExcelFile(PrePointColumns.ToArray(), POINT_FILE);
            DataTable dtPoint = new DataTable();
            PrePointSetExcel.ExcelToDataTable(ref dtPoint, "");
            DataForPreSetPosition = dtPoint;
            UpdateWorkFlowData(DataForPreSetPosition);

            //初始化产品状态
            SupportStateCollect = new ObservableCollection<ProductStateModel>();
            PLCStateCollect = new ObservableCollection<ProductStateModel>();
            for (int i = 0; i < 6; i++)
            {
                SupportStateCollect.Add(new ProductStateModel() { ProductIndex = i + 1, ProductName=$"Spt{i+1}" });
                PLCStateCollect.Add(new ProductStateModel() { ProductIndex = i + 1, ProductName = $"PLC{i + 1}" });
            }
        }

        ~MainViewModel()
        {
            Messenger.Default.Unregister("Error");
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
        private void ShowErrorinfo(string ErrorMsg)
        {
            if (!string.IsNullOrEmpty(ErrorMsg))
            {
                SnakeLastError = $"{DateTime.Now.GetDateTimeFormats()[35]}: {ErrorMsg}";
                ShowSnakeInfoBar = true;
                SystemErrorMessageCollection.Add(new MessageItem() { MsgType = EnumMessageType.Error, StrMsg = SnakeLastError });
            }
        }

        private void SavePrePointFile(LogExcel log,DataTable dt)
        {
            try
            {
                log.DataTableToExcel(dt, "", true, false);
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>(ex.Message, "Error");
            }
        }

        private void UpdateWorkFlowData(DataTable dt)
        {
            WorkFlow.WorkFlowMgr.Instance.ClearPt();
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    List<double> list = new List<double>();
                    for (int i=1;i<dt.Columns.Count; i++)
                    {
                        list.Add(double.Parse(row[i].ToString()));  
                    }
                    WorkFlow.WorkFlowMgr.Instance.AddPoint(row[0].ToString(), list);
                }
            }

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

        public ObservableCollection<ResultItem> ResultCollection
        {
            get;
            set;
        }
        public DataTable DataForPreSetPosition
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
        public bool ShowSnakeInfoBar
        {
            get { return _showSnakeInfoBar; }
            set
            {
                if (_showSnakeInfoBar != value)
                {
                    _showSnakeInfoBar = value;
                    RaisePropertyChanged();
                }
            }
        }
        public string SnakeLastError
        {
            get { return _snakeLastError; }
            set
            {
                if (_snakeLastError != value)
                {
                    _snakeLastError = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool SupportIsSelected { get; set; }
        public ObservableCollection<ProductStateModel> SupportStateCollect { get; set; }
        public ObservableCollection<ProductStateModel> PLCStateCollect { get; set; }
        /// <summary>
        /// Support高16位， PLC低16位
        /// </summary>
        public Int32 ProductStateFlag
        {
            get
            {
                Int16 H = 0, L = 0;
                Int32  State=0;
                for (int i = 0; i < 6; i++)
                {
                    if (SupportStateCollect[i].IsChecked)
                    {
                        H += (Int16)(1 << i);
                    
                    }
                    if (PLCStateCollect[i].IsChecked)
                    {
                        L += (Int16)(1 << i);
                    }
                    State = L + (H << 16);
                  
                }
                return State;
            }
            set { }
        }
        public EnumSystemState SystemState
        {
            set {
                if (_systemState != value)
                {
                    _systemState = value;
                    RaisePropertyChanged();
                }
            }
            get { return _systemState; }
        }
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
                    string langFileNew = null;
                    switch (strLang)
                    {
                        case "CH":
                            langFileNew = "Lang_CH";
                            break;
                        case "EN":
                            langFileNew = "Lang_EN";
                            break;
                        default:
                            break;
                    }
                    var MergedDic = Application.Current.Resources.MergedDictionaries;
                    if (!string.IsNullOrEmpty(langFileNew))
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
                    ShowErrorinfo(err);
                }
            }); }
        }

        /// <summary>
        /// 界面主菜单Home按钮
        /// </summary>
        public RelayCommand BtnHomeCommand
        {
            get { return new RelayCommand(() => ViewIndex = HOME_PAGE); }
        }

        /// <summary>
        /// 界面主菜单设置按钮
        /// </summary>
        public RelayCommand BtnSettingCommand
        {
            get { return new RelayCommand(() =>
            {
                ViewIndex = SETTING_PAGE; 
            }); }
        }

        /// <summary>
        /// 状态栏错误显示按钮
        /// </summary>
        public RelayCommand ShowInfoListCommand
        {
            get
            {
                return new RelayCommand(() =>ViewIndex = INFO_PAGE);
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
        public RelayCommand<ObservableCollection<AxisArgs>> AddPreSetCommand
        {
            get
            {
                return new RelayCommand<ObservableCollection<AxisArgs>>(AxisStatecollection =>
                {
                    if (AxisStatecollection != null && AxisStatecollection.Count == DataForPreSetPosition.Columns.Count - 1)
                    {
                        DataRow dr = DataForPreSetPosition.NewRow();
                        for (int i = 0; i < AxisStatecollection.Count; i++)
                        {
                            if(AxisStatecollection[i].Unit.Category==EnumUnitCategory.Length)
                                dr[i + 1] = UnitHelper.ConvertUnit(AxisStatecollection[i].Unit, new Millimeter(), AxisStatecollection[i].CurAbsPos);
                            else
                                dr[i+1]=UnitHelper.ConvertUnit(AxisStatecollection[i].Unit, new Degree(), AxisStatecollection[i].CurAbsPos);

                        }
                        DataForPreSetPosition.Rows.Add(dr);
                        UpdateWorkFlowData(DataForPreSetPosition);
                    }
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
                    {
                        DataForPreSetPosition.Rows.RemoveAt(nIndex);
                        UpdateWorkFlowData(DataForPreSetPosition);
                    }
                });
            }
        }


        public RelayCommand<DataGridCellInfo> MoveToPtCommand
        {
            get
            {
                return new RelayCommand<DataGridCellInfo>(drv =>
                {
                    if (drv != null)
                    {
                        DataRowView item = drv.Item as DataRowView;
                        DataGridTextColumn dgc = drv.Column as DataGridTextColumn;
                        int index = dgc.DisplayIndex;
             
                        if (index >= 0)
                        {
                            var data = item[index];
                            int AxisNo = Config.ConfigMgr.Instance.HardwareCfgMgr.AxisSettings[index - 1].AxisNo;
                            MotionMgr.Instance.GetAxisState(AxisNo, out AxisArgs args);
                            int Speed = (int)(args.MoveArgs.Speed * ((double)args.MaxSpeed / 100.0f));
                            MotionMgr.Instance.MoveAbs(AxisNo,500, Speed, double.Parse(data.ToString()));
                        }
                    }
                });
            }
        }
        public RelayCommand<DataGridCellInfo> UpdatePtCommand
        {
            get
            {
                return new RelayCommand<DataGridCellInfo>(drv =>
                {
                    try
                    {
                        if (drv != null)
                        {
                            DataRowView item = drv.Item as DataRowView;
                            string PointName = item[0].ToString();
                            foreach (DataRow row in DataForPreSetPosition.Rows)
                            {
                                if (row[0].ToString().Equals(PointName))
                                {
                                    //更新
                                    MonitorVm = SimpleIoc.Default.GetInstance<MonitorViewModel>();
                                    int i = 0;
                                    foreach (var state in MonitorVm.AxisStateCollection)
                                    {
                                        if (state.Unit.Category == EnumUnitCategory.Length)
                                            row[++i] = UnitHelper.ConvertUnit(state.Unit, new Millimeter(), state.CurAbsPos);
                                        else
                                            row[++i] = UnitHelper.ConvertUnit(state.Unit, new Degree(), state.CurAbsPos);

                                    }
                                }
                            }
                            UC_MessageBox.ShowMsgBox($"更新点{PointName}成功", "提示", MsgType.Info);
                        }
                        else
                        {
                            UC_MessageBox.ShowMsgBox($"更新点失败：请选择一行进行更新", "更新点失败", MsgType.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        UC_MessageBox.ShowMsgBox($"更新点失败：{ex.Message}", "更新点失败", MsgType.Error);
                    }
     
                });
            }
        }
        /// <summary>
        /// 主菜单弹出视觉设置界面
        /// </summary>
        public RelayCommand BtnCameraCommand
        {
            get { return new RelayCommand(() => {
                if (SystemState == EnumSystemState.Idle)
                    ViewIndex = CAMERA_PAGE;
                else
                    UC_MessageBox.ShowMsgBox("调试视觉时候，需要关闭自动运行");
            }); }
        }

        /// <summary>
        /// IO监控界面
        /// </summary>
        public RelayCommand BtnMonitorCommand
        {
            get { return new RelayCommand(() => ViewIndex = MONITOR_PAGE); }
        }
        
        /// <summary>
        /// 主界面运行按钮
        /// </summary>
        public RelayCommand StartStationCommand
        {
            get { return new RelayCommand(() => {

                if (WorkFlow.WorkFlowMgr.Instance.StartAllStation())
                    SystemState = EnumSystemState.Running;
                if (ViewIndex == 4)
                    ViewIndex = 1;
                
            }); }
        }
        public RelayCommand PauseStationCommand
        {
            get { return new RelayCommand(() => WorkFlow.WorkFlowMgr.Instance.PauseAllStation()); }
        }
        public RelayCommand StopStationCommand
        {
            get { return new RelayCommand(() => {
                WorkFlow.WorkFlowMgr.Instance.StopAllStation();
                MotionMgr.Instance.StopAll();
                SystemState = EnumSystemState.Idle;
            });}
        }
        /// <summary>
        /// 主界面登陆按钮
        /// </summary>
        public RelayCommand BtnUserCommand
        {
            get
            {
                return new RelayCommand(() => ViewIndex = USER_PAGE);
            }
        }

        /// <summary>
        /// 错误信息弹出框的响应按钮
        /// </summary>
        public RelayCommand SnackBarActionCommand
        {
            get
            {
                return new RelayCommand(() => ShowSnakeInfoBar = false);
            }
        }

        public RelayCommand SavePrePointCommand
        {
            get
            {
                return new RelayCommand(() => {
                    SavePrePointFile(PrePointSetExcel, DataForPreSetPosition);
                    UpdateWorkFlowData(DataForPreSetPosition);
                });
            }
        }

        public RelayCommand CommandFindLine
        {
            get { return new RelayCommand(()=> {
                var station = WorkFlow.WorkFlowMgr.Instance.FindStationByName("WorkService");
                if (station != null)
                {
                    object curCmd = station.GetCurCmd();
                    if (curCmd == null)                  
                        station.SetCmd(WorkFlow.STEP.CmdFindLine);
                    else
                    {
                        if ((WorkFlow.STEP)curCmd == WorkFlow.STEP.CmdFindLine)
                            station.ClearAllStep();
                    }
                }
            }); }
        }

      
        public RelayCommand CommandWorkPLC  //PLC Top
        {
            get
            {
                return new RelayCommand(() => {
                    var station = WorkFlow.WorkFlowMgr.Instance.FindStationByName("WorkService");
                    if (station != null)
                    {
                        object curCmd = station.GetCurCmd();
                        if (curCmd == null)
                        {
                            station.SetCmd(WorkFlow.STEP.CmdGetProductPLC, ProductStateFlag);
                        }
                        else
                        {
                            if ((WorkFlow.STEP)curCmd == WorkFlow.STEP.CmdGetProductPLC)
                                station.ClearAllStep();
                        }
                    }
                });
            }
        }

        public RelayCommand CommandWorkSupport   //Support Bottom
        {
            get
            {
                return new RelayCommand(() => {

                    var station = WorkFlow.WorkFlowMgr.Instance.FindStationByName("WorkService");
                    if (station != null)
                    {
                        object curCmd = station.GetCurCmd();
                        if (curCmd == null)
                        {
                            station.SetCmd(WorkFlow.STEP.CmdGetProductSupport, ProductStateFlag);
                        }
                        else
                        {
                            if ((WorkFlow.STEP)curCmd == WorkFlow.STEP.CmdGetProductSupport)
                                station.ClearAllStep();
                        }
                    }
                });
            }
        }

        public RelayCommand CommandHome
        {
            get
            {
                return new RelayCommand(() => {
                    WorkFlow.WorkFlowMgr.Instance.FindStationByName("WorkService").SetCmd(WorkFlow.STEP.Init);
                    ;
                });
            }
        }

       
        #endregion
    }
}