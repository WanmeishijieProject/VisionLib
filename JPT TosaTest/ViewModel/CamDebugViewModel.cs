using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using JPT_TosaTest.Classes;
using JPT_TosaTest.Config;
using JPT_TosaTest.Model;
using JPT_TosaTest.Model.ToolData;
using JPT_TosaTest.Models;
using JPT_TosaTest.UserCtrl;
using JPT_TosaTest.UserCtrl.VisionDebugTool;
using JPT_TosaTest.Vision;
using JPT_TosaTest.Vision.Light;
using JPT_TosaTest.Vision.ProcessStep;
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
        private ObservableCollection<CameraItem> _cameraCollection = new ObservableCollection<CameraItem>();
        private ObservableCollection<RoiItem> _roiCollection = new ObservableCollection<RoiItem>();
        private ObservableCollection<ModelItem> _modelCollection = new ObservableCollection<ModelItem>();
        private ObservableCollection<ModelItem> _allModelCollection = new ObservableCollection<ModelItem>();
        private FileHelper ModelFileHelper = new FileHelper(FileHelper.GetCurFilePathString() + "VisionData\\Model");
        private FileHelper RoiFileHelper = new FileHelper(FileHelper.GetCurFilePathString() + "VisionData\\Roi");
        private int _maxThre = 0, _minThre = 0;
        private EnumRoiModelType _currentSelectRoiModel;
        private CancellationTokenSource cts = null;
        private int _currentSelectedCamera = 0;
        private bool _saveImageType = true;
        private IEnumerable<RoiModelBase> _roiModelList = null;
        private Task GrabTask = null;
        public EnumCamSnapState _camSnapState;
        private Storyboard RoiSb = null, TemplateSb = null;
        private string PATH_DEFAULT_IMAGEPATH = @"C:\";
        private string PATH_TOOLPATH =  @"VisionData\ToolData\";
        private string PATH_MODELPATH=  @"VisionData\Model\";

        private int _lightBrightness = 0;
        private bool _openLightSource = false;
        private ModelItem _curUsedModel;
        


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
            UpdateModelCollect(0);
            RoiModelList = ModelCollection;
            Messenger.Default.Register<int>(this, "UpdateRoiFiles", nCamID => UpdateRoiCollect(nCamID));
            Messenger.Default.Register<int>(this, "UpdateModelFiles", nCamID => UpdateModelCollect(nCamID));

            //初始化EdgeTool工具列表
            EdgeToolItemCollect = new ObservableCollection<LvEdgeToolItem>();

            EdgeToolItemCollect.Add(new LvEdgeToolItem() { ToolType=EnumToolType.LineTool, ToolName="LineTool"});
            EdgeToolItemCollect.Add(new LvEdgeToolItem() { ToolType = EnumToolType.CircleTool, ToolName = "CircleTool" });
            EdgeToolItemCollect.Add(new LvEdgeToolItem() { ToolType = EnumToolType.PairTool, ToolName = "PairTool" });
            EdgeToolItemCollect.Add(new LvEdgeToolItem() { ToolType = EnumToolType.FlagTool, ToolName = "FlagTool" });

            //初始化ToolFile
            EdgeFileCollect = new ObservableCollection<string>();
            UpdateToolFileCollect();

        }


        ~CamDebugViewModel()
        {
            HalconVision.Instance.CloseCamera();
            Messenger.Default.Unregister<string>("UpdateRoiFiles");
            Messenger.Default.Unregister<string>("UpdateModelFiles");

        }

   


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
            AllModelCollection.Clear();
            var FileList = ModelFileHelper.GetWorkDictoryProfileList(new string[] { "shm" });
            foreach (var it in FileList)
            {
                AllModelCollection.Add(new ModelItem() { StrName = it, StrFullName = it });
            }
            foreach (var it in Vision.VisionDataHelper.GetTemplateListForSpecCamera(nCamID, FileList))
            {
                ModelCollection.Add(new ModelItem() { StrName = it.Replace(string.Format("Cam{0}_", nCamID), ""), StrFullName = it });
            }
            RaisePropertyChanged("ModelCollection");
            RaisePropertyChanged("AllModelCollection");
        }
        private void ThreadFunc(int nCamID)
        {
            CamSnapState = EnumCamSnapState.BUSY;
            while (!cts.Token.IsCancellationRequested)
            {
                HalconVision.Instance.GrabImage(nCamID,true,true);
                //Thread.Sleep(30);
                //break;
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
        private void UpdateToolFileCollect()
        {
            
            List<string> fileList= FileHelper.GetProfileList(PATH_TOOLPATH);
            foreach (var file in fileList)
            {
                string strContent = File.ReadAllText(PATH_TOOLPATH+file+".para");
                string[] list= strContent.Split('|');
                if (list.Count() >= 2)
                {
                    if (Enum.TryParse(list[0], out EnumToolType ToolType))
                    {
                        if (ToolType == EnumToolType.LineTool && !EdgeFileCollect.Contains(file))
                        {
                            EdgeFileCollect.Add(file);
                        }
                    }
                }
            }           
        }

        private bool DebugTool(ToolDataBase para)
        {
            try
            {
                switch (para.ToolType)
                {
                    case EnumToolType.LineTool:
                        {
                            var ToolData = para as LineToolData;
                            HalconVision.Instance.Debug_FindLine(0, ToolData.Polarity, ToolData.SelectType, ToolData.Contrast, ToolData.CaliperNum);
                            return true;
                        }
                    case EnumToolType.CircleTool:
                        {

                        }
                        break;
                    case EnumToolType.PairTool:
                        {
                            PairToolData data = para as PairToolData;
                            HalconVision.Instance.Debug_FindPair(0, data.Polarity, data.SelectType, data.ExpectPairNum, data.Contrast, data.CaliperNum);
                            return true;
                        }
                    case EnumToolType.FlagTool:
                        {
                            FlagToolDaga data = para as FlagToolDaga;
                            AddFlag(data);
                        }
                        break;
                    default:
                        break;

                
                }
                return true;
                
            }
            catch (Exception ex)
            {
                UC_MessageBox.ShowMsgBox("Error", ex.Message, MsgType.Error);
                return false;
            }
        }



        private void AddFlag(ToolDataBase para)
        {
            try
            {
                int nCamID = 0;
                FlagToolDaga data = para as FlagToolDaga;
                var L1Data = File.ReadAllText($"{PATH_TOOLPATH}{data.L1Name}.para");
                var L2Data = File.ReadAllText($"{PATH_TOOLPATH}{data.L2Name}.para");

                List<string> LineListString = new List<string>() { L1Data, L2Data };
                string ModelName = L1Data.Split('|')[1].Split('&')[4];

                StepFindModel FindModelTool = new StepFindModel()
                {
                    In_CamID = nCamID,
                    In_ModelNameFullPath = $"{PATH_MODELPATH}Cam0_{ModelName}.shm"
                };
                HalconVision.Instance.ProcessImage(FindModelTool);

                //FindLine
                StepFindeLineByModel FindLineTool = new StepFindeLineByModel()
                {
                    In_CamID = nCamID,
                    In_Hom_mat2D = FindModelTool.Out_Hom_mat2D,
                    In_ModelRow = FindModelTool.Out_ModelRow,
                    In_ModelCOl = FindModelTool.Out_ModelCol,
                    In_ModelPhi = FindModelTool.Out_ModelPhi,
                    In_LineRoiPara = LineListString
                };
                HalconVision.Instance.ProcessImage(FindLineTool);


                //DrawFlag
                var LineListForDraw = new List<Tuple<double, double, double, double>>();
                foreach (var it in FindLineTool.Out_Lines)
                    LineListForDraw.Add(new Tuple<double, double, double, double>(it.Item1.D, it.Item2.D, it.Item3.D, it.Item4.D));
                StepDrawFlag DrawFlagTool = new StepDrawFlag()
                {
                    In_CamID = nCamID,
                    In_Geometry = data.GeometryType,
                    In_HLine = LineListForDraw[0],
                    In_VLine = LineListForDraw[1]
                };

                HalconVision.Instance.ProcessImage(DrawFlagTool);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DebugShowFlagTia(ToolDataBase para)
        {
            try
            {
                int nCamID = 0;
                FlagToolDaga data = para as FlagToolDaga;
                var L1Data = File.ReadAllText($"{PATH_TOOLPATH}{data.L1Name}.para");
                var L2Data = File.ReadAllText($"{PATH_TOOLPATH}{data.L2Name}.para");
                List<string> LineListString = new List<string>() { L1Data, L2Data };
                string ModelName = L1Data.Split('|')[1].Split('&')[4];

                StepFindModel FindModelTool = new StepFindModel()
                {
                    In_CamID = nCamID,
                    In_ModelNameFullPath = $"{PATH_MODELPATH}Cam0_{ModelName}.shm"
                };
                HalconVision.Instance.ProcessImage(FindModelTool);

                //FindLine
                StepFindeLineByModel FindLineTool = new StepFindeLineByModel()
                {
                    In_CamID = nCamID,
                    In_Hom_mat2D = FindModelTool.Out_Hom_mat2D,
                    In_ModelRow = FindModelTool.Out_ModelRow,
                    In_ModelCOl = FindModelTool.Out_ModelCol,
                    In_ModelPhi = FindModelTool.Out_ModelPhi,
                    In_LineRoiPara = LineListString
                };
                HalconVision.Instance.ProcessImage(FindLineTool);


                //ShowFlag
                var LineListForDraw = new List<Tuple<double, double, double, double>>();
                foreach (var it in FindLineTool.Out_Lines)
                    LineListForDraw.Add(new Tuple<double, double, double, double>(it.Item1.D, it.Item2.D, it.Item3.D, it.Item4.D));
                StepShowFlag ShowFlagTool = new StepShowFlag()
                {
                    In_CamID = nCamID,
                    In_CenterRow = data.Halcon_Row,
                    In_CenterCol = data.Halcon_Col,
                    In_Phi = data.Halcon_Phi,
                    In_HLine = LineListForDraw[0],
                    In_VLine = LineListForDraw[1],
                    In_RegionFullPathFileName = $"{PATH_TOOLPATH}Flag.reg}}"
                };
                HalconVision.Instance.ProcessImage(ShowFlagTool);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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

        public ObservableCollection<ModelItem> AllModelCollection
        {
            get { return _allModelCollection; }
            set
            {
                if (_allModelCollection != value)
                {
                    _allModelCollection = value;
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

        //当前选择的是哪个Model
        public ModelItem CurrentUsedModel
        {
            set {
                if (_curUsedModel != value)
                {
                    _curUsedModel = value;
                    RaisePropertyChanged();
                }
            }
            get { return _curUsedModel; }
        }

        public ObservableCollection<LvEdgeToolItem> EdgeToolItemCollect
        { get; set; }

        /// <summary>
        /// 为手绘工具做准备
        /// </summary>
        public ObservableCollection<string> EdgeFileCollect
        {
            get;
            set;
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
                    if (item != null && item.GetType().Equals(typeof(ModelItem)))
                    {
                        CurrentUsedModel = item as ModelItem;
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
                    if (nCamID < 0)
                        nCamID = 0;
                    if (item == null)
                    {
                        UC_MessageBox.ShowMsgBox("请选择一个模板进行操作", "请选择模板", MsgType.Error);
                        return;
                    }
                    string strModelFileName = $"VisionData\\Model\\Cam{nCamID}_{item.StrName}.shm";    //Model
                    StepFindModel FindModelStep = new StepFindModel()
                    {
                        In_CamID = 0,
                        In_ModelNameFullPath = strModelFileName
                    };
                    HalconVision.Instance.ProcessImage(FindModelStep);
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
                    ofd.InitialDirectory = PATH_DEFAULT_IMAGEPATH;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        PATH_DEFAULT_IMAGEPATH = ofd.FileName;
                        HalconVision.Instance.OpenImageInWindow(CurrentSelectedCamera, PATH_DEFAULT_IMAGEPATH, hWindow);
                    }

                });
            }
        }


        public RelayCommand DistanceCalibCommand
        {
            get { return new RelayCommand(()=> {
                //HalconVision.Instance.SetCamKValue(CurrentSelectedCamera, 66045);
            }); }
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
        public RelayCommand<ToolDataBase> DebugRunToolCommand
        {
            get
            {
                return new RelayCommand<ToolDataBase>(para => {
                    try
                    {
                        HalconVision.Instance.ClearToolRoiData(para.ToolType);
                        DebugTool(para);
                    }
                    catch (Exception ex)
                    {
                        UC_MessageBox.ShowMsgBox("Error", ex.Message, MsgType.Error);
                    }
                });
            }
        }
        public RelayCommand<ToolDataBase> SaveEdgeToolCommand
        {
            get
            {
                return new RelayCommand<ToolDataBase>(para => {
                    try
                    {           
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Filter = "文本文件(*.para)|*.para|所有文件|*.*";//设置文件类型
                        sfd.FileName = para.ToolType.ToString();//设置默认文件名
                        sfd.DefaultExt = "para";//设置默认格式（可以不设）
                        sfd.AddExtension = true;//设置自动在文件名中添加扩展名
                        sfd.RestoreDirectory = true;
                        sfd.InitialDirectory =FileHelper.GetCurFilePathString()+PATH_TOOLPATH;
                      
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            if(para.ToolType== EnumToolType.LineTool)
                                UpdateToolFileCollect();
                            //保存Flag的区域
                            if (para.ToolType == EnumToolType.FlagTool)
                                HalconVision.Instance.SaveFlagToolRegion(sfd.FileName);
                            File.WriteAllText(sfd.FileName, para.ToString());
                        }
                 
                    }
                    catch (Exception ex)
                    {
                        UC_MessageBox.ShowMsgBox("Error", ex.Message, MsgType.Error);
                    }
                });
            }
        }
        public RelayCommand<ToolDataBase> UpdateEdgeToolCommand
        {
            get
            {
                return new RelayCommand<ToolDataBase>(para => {
                    try
                    {
                        switch (para.ToolType)
                        {
                            case EnumToolType.LineTool:
                                {
                                    var Data = para as LineToolData;
                                    if(HalconVision.Instance.LineRoiData!="")
                                        HalconVision.Instance.Debug_FindLine(0, Data.Polarity, Data.SelectType, Data.Contrast, Data.CaliperNum);
                                }
                                break;
                            case EnumToolType.CircleTool:
                                {
                                    var Data = para as CircleToolData;
                                }
                                break;
                            case EnumToolType.PairTool:
                                {
                                    var Data = para as PairToolData;
                                    if (HalconVision.Instance.PairRoiData != "")
                                        HalconVision.Instance.Debug_FindPair(0, Data.Polarity, Data.SelectType, Data.ExpectPairNum, Data.Contrast, Data.CaliperNum);
                                }
                                break;
                            case EnumToolType.FlagTool:
                                {
                                    var Data = para as FlagToolDaga;

                                }
                                break;
                        }




                      

                    }
                    catch (Exception ex)
                    {
                        UC_MessageBox.ShowMsgBox("Error", ex.Message, MsgType.Error);
                    }

                });
            }
        }
        public RelayCommand<ToolDataBase> AddFlagCommand
        {
            get { return new RelayCommand<ToolDataBase>(para=> {
                AddFlag(para);
                }); }
        }
        public RelayCommand<ToolDataBase> ShowFlagCommand
        {
            get
            {
                return new RelayCommand<ToolDataBase>(para => {

                    DebugShowFlagTia(para);
          
                });
            }
        }

        public RelayCommand DebugCommand
        {
            get { return new RelayCommand(()=> {

                FlagToolDaga para = new FlagToolDaga();
                para.FromString(File.ReadAllText(PATH_TOOLPATH+"Flag.para"));
                DebugShowFlagTia(para);

            }); }
        }

        /// <summary>
        /// 对图像进行缩放
        /// </summary>
        public RelayCommand ZoomCommand
        {
            get
            {
                return new RelayCommand(() => {
                    //CurrentSelectedCamera
                    HalconVision.Instance.ZoomImage(CurrentSelectedCamera);
                });
            }
        }

        //停止缩放
        public RelayCommand ResetZoomCommand
        {
            get
            {
                return new RelayCommand(() => {
                    HalconVision.Instance.ResetZoomImage(CurrentSelectedCamera);
                });
            }
        }
        

      


    }
    #endregion


}
