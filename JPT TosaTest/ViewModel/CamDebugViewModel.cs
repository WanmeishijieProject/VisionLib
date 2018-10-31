using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using JPT_TosaTest.Classes;
using JPT_TosaTest.Config;
using JPT_TosaTest.Models;
using JPT_TosaTest.UserCtrl;
using JPT_TosaTest.Vision;
using JPT_TosaTest.Vision.Light;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace JPT_TosaTest.ViewModel
{


    public class CamDebugViewModel : ViewModelBase
    {
        public enum EnumRoiModelType : int
        {
            ROI,
            MODEL
        }
        public CamDebugViewModel()
        {
            #region CameraInit
            CameraCollection = new ObservableCollection<CameraItem>();
            int i = 0;
            List<string> CamListSetting = new List<string>();
            foreach (var it in ConfigMgr.Instance.HardwareCfgMgr.Cameras)
                CamListSetting.Add(it.NameForVision);

            var CamListFind = HalconVision.Instance.FindCamera(EnumCamType.GigEVision, CamListSetting, out List<string> ErrorList);
            foreach (var it in CamListFind)
            {
                bool bOpen = HalconVision.Instance.OpenCam(i++);
                CameraCollection.Add(new CameraItem() { CameraName = it.Key, StrCameraState = bOpen ? "Connected" : "DisConnected" });
            }
            #endregion

            //ModelList
            RoiModelList = RoiCollection;

            Messenger.Default.Register<int>(this, "UpdateRoiFiles", nCamID => UpdateRoiCollect(nCamID));
            Messenger.Default.Register<int>(this, "UpdateModelFiles", nCamID => UpdateModelCollect(nCamID));
        }
        ~CamDebugViewModel()
        {
            HalconVision.Instance.CloseCamera();
            Messenger.Default.Unregister<string>("UpdateRoiFiles");
            Messenger.Default.Unregister<string>("UpdateModelFiles");

        }

        private ObservableCollection<CameraItem> _cameraCollection = new ObservableCollection<CameraItem>();
        private ObservableCollection<RoiItem> _roiCollection = new ObservableCollection<RoiItem>();
        private ObservableCollection<ModelItem> _modelCollection = new ObservableCollection<ModelItem>();
        private FileHelper ModelFileHelper = new FileHelper(FileHelper.GetCurFilePathString() + "VisionData\\Model");
        private FileHelper RoiFileHelper = new FileHelper(FileHelper.GetCurFilePathString() + "VisionData\\Roi");
        private int _maxThre = 0, _minThre = 0;
        private EnumRoiModelType _currentSelectRoiModel;
        private CancellationTokenSource cts = null;
        private int _currentSelectedCamera = -1;
        private bool _saveImageType = true;
        private IEnumerable<RoiModelBase> _roiModelList = null;
        private Task GrabTask = null;
        public EnumCamSnapState _camSnapState;
        private Storyboard RoiSb = null, TemplateSb=null;
        private string DefaultImagePath = @"C:\";
        private int _lightBrightness = 0;
        private bool _openLightSource = false;

        #region Private method
        private void UpdateRoiCollect(int nCamID)
        {
            RoiCollection.Clear();
            foreach (var it in Vision.VisionDataHelper.GetRoiListForSpecCamera(nCamID, RoiFileHelper.GetWorkDictoryProfileList(new string[] { "reg" })))
                RoiCollection.Add(new RoiItem() { StrName = it.Replace(string.Format("Cam{0}_", nCamID), ""), StrFullName = it });
        }
        private void UpdateModelCollect(int nCamID)
        {
            ModelCollection.Clear();
            foreach (var it in Vision.VisionDataHelper.GetTemplateListForSpecCamera(nCamID, ModelFileHelper.GetWorkDictoryProfileList(new string[] { "shm" })))
                ModelCollection.Add(new ModelItem() { StrName = it.Replace(string.Format("Cam{0}_", nCamID), ""), StrFullName = it });
        }
        private void ThreadFunc(int nCamID)
        {
            CamSnapState = EnumCamSnapState.BUSY;
            while (!cts.Token.IsCancellationRequested)
            {
                HalconVision.Instance.GrabImage(nCamID);
                Thread.Sleep(30);
            }
            CamSnapState = EnumCamSnapState.IDLE;
        }
        private void TemplateSb_Completed(object sender, EventArgs e)
        {
            RoiModelList = ModelCollection;
        }
        private void RoiSb_Completed(object sender, EventArgs e)
        {
            RoiModelList = RoiCollection;
        }
        #endregion


        #region Properties
        public ObservableCollection<RoiItem> RoiCollection
        {
            get { return _roiCollection; }
            set
            {
                if (_roiCollection != value)
                {
                    _roiCollection = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ObservableCollection<ModelItem> ModelCollection
        {
            get { return _modelCollection; }
            set
            {
                if (_modelCollection != value)
                {
                    _modelCollection = value;
                    RaisePropertyChanged();
                }
            }
        }


        public int MaxThre
        {
            set
            {
                if (_maxThre != value)
                {
                    _maxThre = value;
                    RaisePropertyChanged();
                }
            }
            get { return _maxThre; }
        }
        public int MinThre
        {
            set
            {
                if (_minThre != value)
                {
                    _minThre = value;
                    RaisePropertyChanged();
                }
            }
            get { return _minThre; }
        }
  
        /// <summary>
        /// 当前选择的是ROI还是Model模式
        /// </summary>
        public EnumRoiModelType RoiOrModelPanel
        {
            set
            {
                if (_currentSelectRoiModel != value)
                {
                    _currentSelectRoiModel = value;
                    RaisePropertyChanged();
                }
            }
            get { return _currentSelectRoiModel; }
        }

        /// <summary>
        /// 当前ROI的操作符
        /// </summary>
        public Enum_REGION_OPERATOR RegionOperator
        {
            get { return HalconVision.Instance.RegionOperator; }
            set { HalconVision.Instance.RegionOperator = value; }
        }

        /// <summary>
        /// ROI的类型
        /// </summary>
        public Enum_REGION_TYPE RegionType
        {
            get { return HalconVision.Instance.RegionType; }
            set { HalconVision.Instance.RegionType = value; }
        }

        /// <summary>
        /// 相机的状态
        /// </summary>
        public EnumCamSnapState CamSnapState
        {
            set
            {
                if (_camSnapState != value)
                {
                    _camSnapState = value;
                    RaisePropertyChanged();
                }
            }
            get { return _camSnapState; }
        }

        /// <summary>
        /// 保存图像的类型
        /// </summary>
        public bool SaveImageType
        {
            set
            {
                if (_saveImageType != value)
                {
                    _saveImageType = value;
                    RaisePropertyChanged();
                }
            }
            get { return _saveImageType; }
        }

        public ObservableCollection<CameraItem> CameraCollection
        {
            get;
            set;
        }

        /// <summary>
        /// 当前选择的是哪个相机
        /// </summary>
        public int CurrentSelectedCamera
        {
            set
            {
                if (_currentSelectedCamera != value)
                {
                    //更新对应相机的Roi和Model
                    if (RoiOrModelPanel == EnumRoiModelType.ROI)
                        RoiModelList = ModelCollection;
                    else
                        RoiModelList = RoiCollection;

                    //更新对应光源的亮度值
                    LightBrightness = ConfigMgr.Instance.HardwareCfgMgr.Cameras[value].LightValue;

                    _currentSelectedCamera = value;
                    RaisePropertyChanged();
                }
            }
            get { return _currentSelectedCamera; }
        }

        public IEnumerable<RoiModelBase> RoiModelList
        {
            set
            {
                if (_roiModelList != value)
                {
                    _roiModelList = value;
                    RaisePropertyChanged();
                }
            }
            get { return _roiModelList; }
        }

        public int LightBrightness
        {
            set
            {
                if (_lightBrightness != value)
                {
                    _lightBrightness = value;
                    RaisePropertyChanged();
                }
            }
            get { return _lightBrightness; }
        }
        public bool OpenLightSource
        {
            set
            {
                if (_openLightSource != value)
                {
                    _openLightSource = value;
                    RaisePropertyChanged();
                }
            }
            get { return _openLightSource; }
        }
        #endregion

        #region Command
        /// <summary>
        /// 更新Roi与Model的目录，传入相机的CamID
        /// </summary>
        public RelayCommand<int> UpdateRoiAndModel
        {
            get
            {
                return new RelayCommand<int>(nCamID =>
                {
                    UpdateModelCollect(nCamID);
                    UpdateRoiCollect(nCamID);
                });
            }
        }
        public RelayCommand<int> NewRoiCommand
        {
            get
            {
                return new RelayCommand<int>(nCamID =>
                {
                    if (nCamID >= 0)
                    {
                        if (MessageBoxResult.Yes == Window_AddRoiModel.ShowWindowNewRoiModel(EnumWindowType.ROI))
                        {
                            foreach (var it in RoiCollection)
                            {
                                if (it.StrName == Window_AddRoiModel.ProfileValue)
                                {
                                    UC_MessageBox.ShowMsgBox("该文件已经存在，请重新命名","警告", MsgType.Warning);
                                    return;
                                }
                            }
                            HalconVision.Instance.NewRoi(nCamID, $"VisionData\\Roi\\Cam{nCamID}_{Window_AddRoiModel.ProfileValue}");
                            //HalconVision.Instance.ShowRoi($"Cam{nCamID}_{Window_AddRoiModel.ProfileValue}");
                            UpdateRoiCollect(nCamID);   //只更新这一个相机的Roi文件
                        }
                    }
                });
            }
        }
        public RelayCommand<int> PreCreateModelCommand
        {
            get
            {
                return new RelayCommand<int>(nCamID =>
                {
                    if (nCamID >= 0)
                    {
                        if (MessageBoxResult.Yes == Window_AddRoiModel.ShowWindowNewRoiModel(EnumWindowType.ROI))
                        {
                            foreach (var it in ModelCollection)
                            {
                                if (it.StrName == Window_AddRoiModel.ProfileValue)
                                {
                                    UC_MessageBox.ShowMsgBox("该文件已经存在，请重新命名", "警告", MsgType.Warning);
                                    return;
                                }
                            }
                            string strRegionTemp = $"VisionData\\ModelTemp\\Cam{nCamID}_{Window_AddRoiModel.ProfileValue}.reg";
                            FileHelper.DeleteAllFileInDirectory($"VisionData\\ModelTemp");
                            File.OpenWrite(strRegionTemp);
                            HalconVision.Instance.PreCreateShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, strRegionTemp);

                        }

                    }
                });
            }
        }

        public RelayCommand<RoiModelBase> PreDrawModelRegionCommand     //调整Model的Region
        {
            get
            {
                return new RelayCommand<RoiModelBase>(item =>
                {
                    List<string> fileList = FileHelper.GetProfileList($"VisionData\\ModelTemp");
                    int nCamID = CurrentSelectedCamera;
                    if (fileList.Count == 0) //判断编辑现有的还是编辑新模板
                    {
                        if (item != null)      
                        {
                            if (nCamID >= 0)
                            {
                                string regionPath = $"VisionData\\Model\\{item.StrFullName}.reg";
                                HalconVision.Instance.DrawRoi(nCamID, EnumRoiType.ModelRegionReduce, out object region, regionPath);    //有模板的时候直接以名称存储
                                HalconVision.Instance.PreCreateShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, regionPath, region);
                            }
                        }
                    }
                    else
                    {
                        if (nCamID >= 0)
                        {
                            string regionPath = $"VisionData\\ModelTemp\\{fileList[0]}.reg";
                            HalconVision.Instance.DrawRoi(nCamID, EnumRoiType.ModelRegionReduce, out object region, regionPath);       //没有模板的时候就按照相机存储
                            HalconVision.Instance.PreCreateShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, regionPath, region);    //传入Region
                        }
                    }
                });
            }
        }
        public RelayCommand<ModelItem> PreViewModelRegionCommand     //只是动态显示，不绘图
        {
            get
            {
                return new RelayCommand<ModelItem>(item =>
                {
                    List<string> fileList = FileHelper.GetProfileList($"VisionData\\ModelTemp");
                    int nCamID = CurrentSelectedCamera;
                    if (fileList.Count == 0)
                    {
                        if (item != null)
                        {
                            if (nCamID >= 0)
                            {
                                string strRegionPath = $"VisionData\\Model\\{item.StrFullName}.reg";
                                object region = HalconVision.Instance.ReadRegion(strRegionPath);
                                HalconVision.Instance.PreCreateShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, strRegionPath, region);
                            }
                        }
                    }
                    else
                    {
                        if (nCamID >= 0)
                        {
                            string strRegionPath = $"VisionData\\ModelTemp\\{fileList[0]}.reg";
                            object region = HalconVision.Instance.ReadRegion(strRegionPath);
                            HalconVision.Instance.PreCreateShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, strRegionPath, region);    //传入Region
                        }
                    }
                });
            }
        }

        public RelayCommand<RoiModelBase> ShowRoiModelCommand       //菜单项显示此项
        {
            get
            {
                return new RelayCommand<RoiModelBase>(item =>
                {
                    if (item != null)
                    {
                        if (item.GetType() == typeof(RoiItem))
                            HalconVision.Instance.ShowRoi($"VisionData\\Roi\\{item.StrFullName}.reg");
                        else
                            HalconVision.Instance.ShowModel($"VisionData\\Model\\{item.StrFullName}.shm");
                    }
                });
            }
        }
        public RelayCommand<RoiModelBase> SelectUseRoiModelCommand    //菜单键选中此项,选择相机使用哪个模板和Roi
        {
            get
            {
                return new RelayCommand<RoiModelBase>(item =>
                {
                    if (item != null)
                    {
                        
                    }
                });
            }
        }
        public RelayCommand<ModelItem> SaveModelParaCommand
        {
            get
            {
                return new RelayCommand<ModelItem>(item =>
                {
                    List<string> fileList = FileHelper.GetProfileList($"VisionData\\ModelTemp");
                    int nCamID = CurrentSelectedCamera;
                    if (fileList.Count == 0)
                    {
                        if (item != null)
                        {
                            if (nCamID >= 0)
                            {
                                string strRegionPath = $"VisionData\\Model\\{item.StrFullName}.reg";
                                object region = HalconVision.Instance.ReadRegion(strRegionPath);
                                HalconVision.Instance.SaveShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, strRegionPath, region);
                                UpdateModelCollect(nCamID);   //只更新这一个相机的Roi文件
                            }
                        }
                    }
                    else
                    {
                        if (nCamID >= 0)
                        {
                            string strRegionPath = $"VisionData\\ModelTemp\\{fileList[0]}.reg";
                            object region = HalconVision.Instance.ReadRegion(strRegionPath);
                            FileHelper.DeleteAllFileInDirectory($"VisionData\\ModelTemp");  //删除Temp文件
                            HalconVision.Instance.SaveShapeModel(nCamID, MinThre, MaxThre, EnumShapeModelType.XLD, $"VisionData\\Model\\{fileList[0]}.reg", region);    //传入Region
                            UpdateModelCollect(nCamID);   //只更新这一个相机的Roi文件
                        }
                    }
                });
            }
        }
        public RelayCommand<ModelItem> TestModelParaCommand
        {
            get
            {

                return new RelayCommand<ModelItem>(item =>
                {
                    int nCamID = CurrentSelectedCamera;
                    if (item == null)
                    {
                        UC_MessageBox.ShowMsgBox("请选择一个模板进行操作", "请选择模板", MsgType.Error);
                        return;
                    }
                      
                    if (nCamID >= 0)
                    {
                        string strRecParaFileName = $"VisionData\\Roi\\Cam{item.StrName}.tup";  //Model Roi
                        string strModelFileName = $"VisionData\\Model\\Cam{item.StrName}.shm";    //Model
                        try
                        {
                            //查找模板并获取数据
                            bool bRet = HalconVision.Instance.ProcessImage(HalconVision.IMAGEPROCESS_STEP.T1, nCamID, $"{strRecParaFileName}&{strModelFileName}", out object result);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"测试Model发生错误{ex.Message}");
                        }
                    }
                });
            }
        }
        public RelayCommand<string> TestRoiCommand
        {
            get
            {
                return new RelayCommand<string>(str =>
                {
                    int nCamID = Convert.ToInt16(str);
                    if (nCamID < 0)
                        return;
                    try
                    {
                        HalconVision.Instance.ProcessImage(HalconVision.IMAGEPROCESS_STEP.T2, nCamID, null, out object result);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"测试Roi发生错误{ex.Message}");
                    }
                });
            }
        }
        public RelayCommand GrabContinusCommand
        {
            get { return new RelayCommand(()=> {
                if (CurrentSelectedCamera >= 0 && (GrabTask == null || GrabTask.IsCanceled || GrabTask.IsCompleted))
                {
                    cts = new CancellationTokenSource();
                    GrabTask = new Task(()=>ThreadFunc(CurrentSelectedCamera));
                    GrabTask.Start();
                }
            }); }
        }
        public RelayCommand GrabOnceCommand
        {
            get
            {
                return new RelayCommand (() => {
                    if (CurrentSelectedCamera >= 0)
                    {
                        HalconVision.Instance.GrabImage(CurrentSelectedCamera);
                    }
                    else
                    {
                        //doNothing
                    }
                });
            }
        }

        /// <summary>
        /// 停止采集
        /// </summary>
        public RelayCommand StopGrabCommand
        {
            get
            {
                return new RelayCommand(() => {
                    if(cts!=null)
                        cts.Cancel();
                    CamSnapState = EnumCamSnapState.IDLE;
                });
            }
        }

        /// <summary>
        /// 保存图片命令
        /// </summary>
        public RelayCommand<IntPtr> SaveImagerCommand
        {
            get
            {
                return new RelayCommand<IntPtr>(hWindow =>
                {
                    if (CurrentSelectedCamera >= 0)
                    {
                        DateTime now = DateTime.Now;
                        HalconVision.Instance.SaveImage(CurrentSelectedCamera, SaveImageType ? EnumImageType.Image : EnumImageType.Window, FileHelper.GetCurFilePathString() + "ImageSaved\\ImageSaved", $"{now.Month}月{now.Day}日 {now.Hour}时{now.Minute}分{now.Second}秒_Cam{CurrentSelectedCamera}.jpg", hWindow);
                    }
                });
            }
        }

        /// <summary>
        /// 打开图片命令
        /// </summary>
        public RelayCommand<IntPtr> OpenImageCommand
        {
            get
            {
                return new RelayCommand<IntPtr>(hWindow =>
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Title = "请选择要打开的文件";
                    ofd.Multiselect = false;
                    ofd.InitialDirectory = DefaultImagePath;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        DefaultImagePath = ofd.FileName;
                        HalconVision.Instance.OpenImageInWindow(CurrentSelectedCamera, DefaultImagePath, hWindow);
                    }

                });
            }
        }

        /// <summary>
        /// 调整相应相机的光源亮度
        /// 参数为传出的光源亮度设定值
        /// </summary>
        public RelayCommand BrightnessChangedCommand
        {
            get
            {
                return new RelayCommand(() => {
                    if (CurrentSelectedCamera >= 0)
                    {
                        //调整相应相机的光源
                        int LightChannel = ConfigMgr.Instance.HardwareCfgMgr.Cameras[CurrentSelectedCamera].LightPortChannel;
                        LightBase lightControl= LigtMgr.Instance.FindInstrumentByChannelIndex(LightChannel);
                        if (lightControl != null)
                        {
                            //lightControl.OpenLight(LightChannel);
                            lightControl.SetLightValue(LightChannel, LightBrightness);
                        }
                    }
                    else
                    {
                        //doNothing
                    }
                });
            }
        }

        /// <summary>
        /// Model与Roi切换动画
        /// </summary>
        public RelayCommand<FrameworkElement> SwitchRoiModelCommand
        {
            get
            {
                return new RelayCommand<FrameworkElement> (control =>
                {
                    if (RoiSb == null && TemplateSb == null)
                    {
                        RoiSb = control.TryFindResource("RoiSb") as Storyboard;
                        TemplateSb = control.TryFindResource("ModelSb") as Storyboard;
                        if (RoiSb != null)
                            RoiSb.Completed += RoiSb_Completed;
                        if (TemplateSb != null)
                            TemplateSb.Completed += TemplateSb_Completed;
                    }
                    RoiOrModelPanel = (Convert.ToInt32(RoiOrModelPanel) ^ 1)==0? EnumRoiModelType.ROI : EnumRoiModelType.MODEL;
                    if (RoiSb != null && TemplateSb != null)
                    {
                        if (RoiOrModelPanel == EnumRoiModelType.ROI)
                            TemplateSb.Begin();
                        else
                            RoiSb.Begin();
                    }
                    
                });
            }
        }
        public RelayCommand SwitchLightPowerCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (CurrentSelectedCamera < 0)
                        return;
                    OpenLightSource = !OpenLightSource;
                    int LightChannel = ConfigMgr.Instance.HardwareCfgMgr.Cameras[CurrentSelectedCamera].LightPortChannel;
                    LightBase lightControl = LigtMgr.Instance.FindInstrumentByChannelIndex(LightChannel);
                    if (lightControl == null)
                        return;
                    if (OpenLightSource)
                        lightControl.OpenLight(LightChannel, LightBrightness);
                    else
                        lightControl.CloseLight(LightChannel, LightBrightness);
                });
            }
        }

        public RelayCommand DebugFindLineCommand
        {
            get { return new RelayCommand(()=> {
                try
                {
                    //找直线
                    HalconVision.Instance.Debug_FindLine(0,EnumEdgeType.DarkToLight,30,50);
                }
                catch (Exception ex)
                {
                    UC_MessageBox.ShowMsgBox("Error", ex.Message, MsgType.Error);
                }
            }); }
        }
        
    }
    #endregion


}
