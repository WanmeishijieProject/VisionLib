using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JPT_TosaTest.Classes;
using JPT_TosaTest.Config;
using JPT_TosaTest.Models;
using JPT_TosaTest.UserCtrl;
using JPT_TosaTest.Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JPT_TosaTest.ViewModel
{
 
    
    public class CamDebugViewModel : ViewModelBase
    {
        public CamDebugViewModel()
        {
            #region CameraInit
            CameraCollection = new ObservableCollection<CameraItem>();
            int i = 0;
            List<string> camList = new List<string>();
            foreach (var it in ConfigMgr.Instance.HardwareCfgMgr.Cameras)
                camList.Add(it.NameForVision);
            foreach (var it in HalconVision.Instance.FindCamera(EnumCamType.GigEVision, camList))
            {
                bool bOpen = HalconVision.Instance.OpenCam(i++);
                CameraCollection.Add(new CameraItem() { CameraName = it.Key, StrCameraState = bOpen ? "Connected" : "DisConnected" });
            }
            #endregion
        }

        private ObservableCollection<CameraItem> _cameraCollection = new ObservableCollection<CameraItem>();
        private ObservableCollection<RoiItem> _roiCollection = new ObservableCollection<RoiItem>();
        private ObservableCollection<ModelItem> _modelCollection = new ObservableCollection<ModelItem>();
        private FileHelper ModelFileHelper = new FileHelper(FileHelper.GetCurFilePathString() + "VisionData\\Model");
        private FileHelper RoiFileHelper = new FileHelper(FileHelper.GetCurFilePathString() + "VisionData\\Roi");
        private int _maxThre = 0, _minThre = 0;
        private int _currentSelectRoiModel;
        public EnumCamSnapState _camSnapState;

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
        public int CurrentSelectRoiModel
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
        public Enum_REGION_OPERATOR RegionOperator
        {
            get { return HalconVision.Instance.RegionOperator; }
            set { HalconVision.Instance.RegionOperator = value; }
        }
        public Enum_REGION_TYPE RegionType
        {
            get { return HalconVision.Instance.RegionType; }
            set { HalconVision.Instance.RegionType = value; }
        }
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
        public ObservableCollection<CameraItem> CameraCollection
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
                                    UC_MessageBox.ShowMsgBox("该文件已经存在，请重新命名");
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
                                    UC_MessageBox.ShowMsgBox("该文件已经存在，请重新命名");
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

        public RelayCommand<Tuple<RoiModelBase, int>> PreDrawModelRoiCommand     //调整Model的ROI
        {
            get
            {
                return new RelayCommand<Tuple<RoiModelBase, int>>(tuple =>
                {
                    List<string> fileList = FileHelper.GetProfileList($"VisionData\\ModelTemp");
                    ModelItem item = tuple.Item1 as ModelItem;
                    int nCamID = tuple.Item2;
                    if (fileList.Count == 0)
                    {
                        if (item != null)      //判断编辑现有的还是编辑新模板
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
        public RelayCommand<Tuple<RoiModelBase, int>> PreViewRoiCommand     //只是动态显示，不绘图
        {
            get
            {
                return new RelayCommand<Tuple<RoiModelBase, int>>(tuple =>
                {
                    List<string> fileList = FileHelper.GetProfileList($"VisionData\\ModelTemp");
                    ModelItem item = tuple.Item1 as ModelItem;
                    int nCamID = tuple.Item2;
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
        public RelayCommand<Tuple<RoiModelBase, int>> SaveModelParaCommand
        {
            get
            {
                return new RelayCommand<Tuple<RoiModelBase, int>>(tuple =>
                {
                    List<string> fileList = FileHelper.GetProfileList($"VisionData\\ModelTemp");
                    ModelItem item = tuple.Item1 as ModelItem;
                    int nCamID = tuple.Item2;
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
        public RelayCommand<int> TestModelParaCommand
        {
            get
            {
                return new RelayCommand<int>(nCamID =>
                {
                    if (nCamID >= 0)
                    {
                        double angle = 0.0f;
                        string strRecParaFileName = "";
                        string strModelFileName = "";
                        if (strRecParaFileName == "" || strModelFileName == "")
                        {
                            UC_MessageBox.ShowMsgBox("请确认当前使用的配方选择了Roi和Model");
                            return;
                        }
                        else
                        {
                            strRecParaFileName = $"VisionData\\Roi\\Cam{nCamID}_{strRecParaFileName}.tup";
                            strModelFileName = $"VisionData\\Model\\Cam{nCamID}_{strModelFileName}.shm";
                        }    
                        try
                        {
                            bool bRet = HalconVision.Instance.ProcessImage(HalconVision.IMAGEPROCESS_STEP.GET_ANGLE_TUNE1, nCamID, $"{strRecParaFileName}&{strModelFileName}", out object result);
                            if (bRet)
                                angle = double.Parse(result.ToString()) * 180;
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
                        HalconVision.Instance.ProcessImage(HalconVision.IMAGEPROCESS_STEP.GET_ANGLE_TUNE2, nCamID, null, out object result);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"测试Roi发生错误{ex.Message}");
                    }
                });
            }
        }


    }
    #endregion


}
