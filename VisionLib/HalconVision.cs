using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using HalconDotNet;
using VisionLib.CommonVisionStep;
using static VisionLib.VisionDefinitions;

namespace VisionLib
{

    public class HalconVision
    {
        #region constructor
        private HalconVision()
        {
            HOperatorSet.GenEmptyObj(out Region);
           
        }
        private static readonly Lazy<HalconVision> _instance = new Lazy<HalconVision>(() => new HalconVision());
        public static HalconVision Instance
        {
            get { return _instance.Value; }
        }
        #endregion

        #region  Field
        private List<CameraInfoModel> CamInfoList = new List<CameraInfoModel>();
        private HObject Region = null;
        public Enum_REGION_OPERATOR RegionOperator = Enum_REGION_OPERATOR.ADD;
        public Enum_REGION_TYPE RegionType = Enum_REGION_TYPE.CIRCLE;
        private AutoResetEvent SyncEvent = new AutoResetEvent(false);
        private string WINDOW_DEBUG = "CameraDebug";
        #endregion

        public void AttachCamWIndow(int nCamID, string Name, HTuple hWindow)
        {
            CheckCamIDAvilible(nCamID);
            foreach (var it in CamInfoList)
            {
                lock (it.VisionLock)
                {
                    if (it.CamID == nCamID)
                    {
                        if (it.AttachedWindowDic.Keys.Contains(Name))
                        {
                            it.AttachedWindowDic[Name] = hWindow;
                        }
                        else
                        {
                            it.AttachedWindowDic.Add(Name, hWindow);
                        }
                    }
                    else
                    {
                        if (it.AttachedWindowDic.Keys.Contains(Name))
                        {
                            it.AttachedWindowDic.Remove(Name);
                        }
                    }
                }
            }
            }
   
        public void DetachCamWindow(int nCamID, string Name)
        {
            var Cam=CheckCamIDAvilible(nCamID);
            lock (Cam.VisionLock)
            {
                if (Cam.AttachedWindowDic.Keys.Contains(Name))
                {
                    Cam.AttachedWindowDic.Remove(Name);
                }
            }
        }
        public void GetSyncSp(out AutoResetEvent Se, out object Lock, int CamID)
        {
            CheckCamIDAvilible(CamID);
            Se = SyncEvent;
            var Cam = CheckCamIDAvilible(CamID);
            Lock = Cam.VisionLock; 
        }

        public bool OpenCam(int nCamID)
        {
            CheckCamIDAvilible(nCamID);
            HObject image = null;
            HTuple hv_AcqHandle = null;
            HTuple width = 0, height = 0;
            try
            {
                var Cam = CheckCamIDAvilible(nCamID);
                lock (Cam.VisionLock)
                {
                    if (!Cam.IsConnected)
                    {
                        //HOperatorSet.OpenFramegrabber("DirectShow", 1, 1, 0, 0, 0, 0, "default", 8, "rgb",
                        //                        -1, "false", "default", "Integrated Camera", 0, -1, out hv_AcqHandle);
                        HOperatorSet.OpenFramegrabber(Cam.Type.ToString(), 1, 1, 0, 0, 0, 0, "default", 8, "gray",
                                                    -1, "false", "default", Cam.NameForVision, 0, -1, out hv_AcqHandle);
                        Cam.AcqHandle = hv_AcqHandle;
                        Cam.IsActive = true;
                        Cam.IsConnected = true;
                    }
                    if (Cam.IsConnected)
                    {
                        HOperatorSet.GrabImage(out image, Cam.AcqHandle);
                        HOperatorSet.GetImageSize(image, out width, out height);
                        if (Cam.Image!=null)
                            Cam.Image.Dispose();
                        Cam.Image = image.SelectObj(1);
                        Cam.ImageWidth = width;
                        Cam.ImageHeight = height;

                        foreach (var dic in Cam.AttachedWindowDic)
                        {
                            HOperatorSet.SetPart(dic.Value, 0, 0, Cam.ImageWidth, Cam.ImageHeight);
                            HOperatorSet.DispObj(image, dic.Value);
                        }
                    }
                    return true;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
            finally
            {
                if (image != null)
                    image.Dispose();
            }
        }
        public bool CloseCam(int nCamID)
        {
            CheckCamIDAvilible(nCamID);
            foreach (var it in CamInfoList)
            {
                lock (it.VisionLock)
                {
                    if (it.IsConnected)
                    {
                        HOperatorSet.CloseFramegrabber(it.AcqHandle);
                        it.IsConnected = false;
                        it.IsActive = false;
                    }
                }
            }
            return true;
        }
        public bool IsCamOpen(int nCamID)
        {
            CheckCamIDAvilible(nCamID);
            foreach (var it in CamInfoList)
            {
                lock (it.VisionLock)
                {
                    if (it.CamID == nCamID)
                    {
                        return it.IsConnected;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 采集图像
        /// </summary>
        /// <param name="nCamID"></param>
        /// <param name="bDispose"></param>
        /// <param name="bContinus"></param>
        public void GrabImage(int nCamID, bool bDispose = true, bool bContinus = false)
        {
            CheckCamIDAvilible(nCamID);
            HObject image = null;
            try
            {
                var Cams = from cam in CamInfoList where cam.CamID == nCamID select cam;
                if (Cams.Count() > 0)
                {
                    var Cam = Cams.First();
                    lock (Cam.VisionLock)
                    {
                        if (IsCamOpen(nCamID))
                        {
                            if (!bContinus)
                            {

                                HOperatorSet.GrabImage(out image, Cam.AcqHandle);
                            }
                            else
                            {
                                HOperatorSet.GrabImageAsync(out image, Cam.AcqHandle, -1);
                            }
                            if (Cam.Image!=null)
                                Cam.Image.Dispose();
                            Cam.Image = image.SelectObj(1);

                            if (!SyncEvent.WaitOne(50))
                            {
                                foreach (var dic in Cam.AttachedWindowDic)
                                {
                                    if (dic.Value != -1)
                                    {
                                        HOperatorSet.SetPart(dic.Value, 0, 0, Cam.ImageHeight, Cam.ImageWidth);
                                        HOperatorSet.DispObj(Cam.Image, dic.Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"相机{nCamID}采集图片出错：{ex.Message}");
            }
            finally
            {
                if (bDispose && image != null)
                {
                    image.Dispose();
                }
            }
        }

        /// <summary>
        /// 传入的处理步骤参数
        /// </summary>
        /// <param name="ProcessStep"></param>
        /// <returns></returns>
        public bool ProcessImage(VisionProcessStepBase ProcessStep)
        {
            int nCamID = ProcessStep.In_CamID;
            CheckCamIDAvilible(nCamID);
            var Cams = from cam in CamInfoList where cam.CamID == nCamID select cam;
            if (Cams.Count() <= 0)
                throw new Exception($"错误的相机ID：相机{nCamID}未找到");
            var Cam = Cams.First();
            ProcessStep.In_Image =Cam.Image;
            return ProcessStep.Process();
        }
       

      
        public bool ShowRoi(int nCamID, object Region)     //显示ROI
        {
            var Cam = CheckCamIDAvilible(nCamID);
            HObject region = Region as HObject;
            foreach (var it in Cam.AttachedWindowDic)
            {
                if (it.Value != -1)
                {
                    HOperatorSet.SetDraw(it.Value, "margin");
                    HOperatorSet.SetColor(it.Value, "green");
                    HOperatorSet.DispObj(region, it.Value);
                }
            }

            return true;
        }


        public bool ShowRoi(string RoiFilePathName)     //显示ROI
        {
            string[] splitString = RoiFilePathName.Split('\\');
            if (splitString.Length > 2)
            {
                int nCamID = Convert.ToInt16(splitString[splitString.Length - 1].Substring(3, 1));
                var Cam = CheckCamIDAvilible(nCamID);
                HOperatorSet.ReadRegion(out HObject region, RoiFilePathName);
                foreach (var it in Cam.AttachedWindowDic)
                {
                    if (it.Value != -1)
                    {
                        HOperatorSet.SetDraw(it.Value, "margin");
                        HOperatorSet.SetColor(it.Value, "green");
                        HOperatorSet.DispObj(region, it.Value);
                    }
                }
                if (region.IsInitialized())
                    region.Dispose();
                return true;
            }
            return false;
        }
        public bool ShowModel(string ModelFilePathName)     //就在建立修改的时候有用到,同时更新多个窗口
        {
            HObject modelContours = null;
            string[] splitString = ModelFilePathName.Split('\\');
            if (splitString.Length > 1)
            {
                int nCamID = Convert.ToInt16(splitString[splitString.Length - 1].Substring(3, 1));
                var Cam = CheckCamIDAvilible(nCamID);
                //三个文件同时读取
                splitString = ModelFilePathName.Split('.');
                HOperatorSet.ReadShapeModel(ModelFilePathName, out HTuple ModelID);
                HOperatorSet.ReadRegion(out HObject ModelRoiRegion, $"{splitString[0]}.reg");
                HOperatorSet.ReadTuple($"{splitString[0]}.tup", out HTuple ModelOriginPos);

                foreach (var it in Cam.AttachedWindowDic)
                {
                    if (it.Value != -1)
                    {
                        HOperatorSet.SetDraw(it.Value, "margin");
                        HOperatorSet.SetColor(it.Value, "green");

                        HOperatorSet.GetShapeModelContours(out modelContours, ModelID, 1);
                        if (modelContours.CountObj() > 0)
                        {
                            HOperatorSet.VectorAngleToRigid(0, 0, 0, ModelOriginPos[0], ModelOriginPos[1], ModelOriginPos[2], out HTuple homMat2D);
                            HOperatorSet.AffineTransContourXld(modelContours, out HObject contoursAffinTrans, homMat2D);
                            HOperatorSet.DispObj(contoursAffinTrans, it.Value);
                            contoursAffinTrans.Dispose();
                        }

                        modelContours.Dispose();
                    }
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="nCamID">相机ID号</param>
        /// <param name="type">保存图片的类型Image-保存原图，Window-保存当前窗口</param>
        /// <param name="filePath">图片路径如 C:\\Folder</param>
        /// <param name="fileName">图片名称如 Image.jpg</param>
        /// <param name="hWindow">当type=Window的时候需要提供此参数，否则忽略</param>
        /// <returns></returns>
        public bool SaveImage(int nCamID, EnumImageType type, string filePath, string fileName, HTuple hWindow)
        {
            try
            {
                var Cam = CheckCamIDAvilible(nCamID);
                if (!Directory.Exists(filePath))
                    return false;
                switch (type)
                {
                    case EnumImageType.Image:
                        HOperatorSet.WriteImage(Cam.Image, "jpeg", 0, $"{filePath}\\{fileName}");
                        break;
                    case EnumImageType.Window:
                        HOperatorSet.DumpWindow(hWindow, "jpeg", $"{filePath}\\{fileName}");
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool OpenImageInWindow(int nCamID, string imageFilePath, HTuple hwindow)
        {
            try
            {
                HOperatorSet.ReadImage(out HObject image, imageFilePath);
                var Cam = CheckCamIDAvilible(nCamID);
                Cam.Image = image.SelectObj(1);
                HOperatorSet.GetImageSize(image, out HTuple width, out HTuple height);
                HOperatorSet.SetPart(hwindow, 0, 0, height, width);
                HOperatorSet.DispObj(image, hwindow);
                image.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool CloseCamera()
        {
            HOperatorSet.CloseAllFramegrabbers();
            return true;
        }
        public List<CameraInfoModel> FindCamera(EnumCamType camType, List<string> acturalNameList, out List<string> ErrorList)
        {
            
            ErrorList = new List<string>();
#if TEST
            dic.Add("DirectShow", new Tuple<string, string>("Integrated Camera", "DirectShow"));
            CamCfgDic = dic;
            return dic;
#endif
            try
            {
                HOperatorSet.InfoFramegrabber(camType.ToString(), "info_boards", out HTuple hv_Information, out HTuple hv_ValueList);
                if (0 == hv_ValueList.Length)
                    return CamInfoList;
                for (int i = 0; i < acturalNameList.Count; i++)
                {
                    bool bFind = false;
                    foreach (var dev in hv_ValueList.SArr)
                    {
                        var listAttr = dev.Split('|').Where(a => a.Contains("device:"));
                        if (listAttr != null && listAttr.Count() > 0)
                        {
                            string Name = listAttr.First().Trim().Replace("device:", "");
                            if (Name.Contains(acturalNameList[i]))
                            {
                                CamInfoList.Add(new CameraInfoModel() {
                                    ActualName= acturalNameList[i],
                                    NameForVision= Name.Trim(),
                                    Type= camType,
                                    CamID=i
                                });
                                bFind = true;
                                break;
                            }
                        }
                    }
                    if (!bFind)
                        ErrorList.Add($"相机:{ acturalNameList[i]}未找到硬件，请检查硬件连接或者配置");
                }
                return CamInfoList;
            }
            catch (Exception ex)
            {
                ErrorList.Add($"FIndCamera error:{ex.Message}");
                return CamInfoList;
            }

        }

        #region 专用
        /// <summary>
        /// 根据模板文件找出模板位置ModelPos，并输出hom_mat2D
        /// </summary>
        /// <param name="image">输入的图像</param>
        /// <param name="ModelFileName">模板名称，根据名称读取模板原始位置方便生成矩阵</param>
        /// <param name="hm_2D">生成的转换矩阵</param>
        /// <param name="ModelPos">模板位置</param>
        /// <returns></returns>
        public bool FindModelAndGetData(HObject image, string ModelFileName, out HTuple hm_2D, out HTuple ModelPos)
        {
            HOperatorSet.HomMat2dIdentity(out hm_2D);
            ModelPos = new HTuple();
            try
            {
                int nCamID = 0;
                var Cam = CheckCamIDAvilible(nCamID);
                string[] strModelListDot = ModelFileName.Split('.');
                if (strModelListDot.Length < 2)
                    return false;

                string[] strModelListIta = ModelFileName.Split('\\');
                if (strModelListIta.Length < 2)
                    return false;

                //读取模板与它的起始位置
                HOperatorSet.ReadTuple($"{strModelListDot[0]}.tup", out HTuple hv_ModelPos);
                HOperatorSet.ReadShapeModel($"{strModelListDot[0]}.shm", out HTuple hv_ModelID);
                HOperatorSet.FindShapeModel(image, hv_ModelID, (new HTuple(-20)).TupleRad(), (new HTuple(20)).TupleRad(), 0.5, 1, 0.5, "least_squares", 0, 0.9, out HTuple hv_Row1, out HTuple hv_Column1, out HTuple hv_Angle, out HTuple hv_Score);
                HOperatorSet.ClearShapeModel(hv_ModelID);

                //
                HOperatorSet.GetImageSize(image, out HTuple ImageWidth, out HTuple ImageHeight);
                if (hv_Row1.Length != 0)
                {
                    ModelPos[0] = hv_Row1;
                    ModelPos[1] = hv_Column1;
                    ModelPos[2] = hv_Angle;

                    //计算转换矩阵
                    HTuple originRow = hv_ModelPos[0];
                    HTuple originCol = hv_ModelPos[1];
                    HTuple originPhi = hv_ModelPos[2];
                    HOperatorSet.VectorAngleToRigid(originRow, originCol, 0, hv_Row1, hv_Column1, hv_Angle, out hm_2D);
                }
                return true;
            }
            catch
            {
                return false;
            }


        }
        /// <summary>
        /// 可以找直线和边缘对
        /// </summary>
        /// <param name="image"></param>
        /// <param name="LineParaList"></param>
        /// <param name="hom_2D"></param>
        /// <param name="ModelPos"></param>
        /// <param name="lineList"></param>
        /// <returns></returns>
        public bool FindLineBasedModelRoi(HObject image, List<string> LineParaList, HTuple hom_2D, HTuple ModelPos, out List<VisionLineData> lineList)
        {
            lineList = new List<VisionLineData>();
            //ReadRectangle
            try
            {
                foreach (var para in LineParaList)
                {


                    string[] paralist = para.Split('|');
                    if (paralist.Count() != 3)
                        continue;
                    string strToolDataType = paralist[0].ToUpper();
                    string[] strRtPara = paralist[1].Split('&');
                    string[] RectPara = paralist[2].Split('&');

                    if (RectPara.Count() == 5)
                    {
                        int i = 0;
                        //Read Rect Roi
                        HTuple Row = double.Parse(RectPara[i++]);
                        HTuple Col = double.Parse(RectPara[i++]);
                        HTuple Phi = double.Parse(RectPara[i++]);
                        HTuple L1 = double.Parse(RectPara[i++]);
                        HTuple L2 = double.Parse(RectPara[i++]);


                        HOperatorSet.AffineTransPoint2d(hom_2D, Row, Col, out HTuple outRoiRow, out HTuple outRoiCol);
                        HOperatorSet.GenRectangle2(out HObject rect, outRoiRow, outRoiCol, Phi + ModelPos[2], L1, L2);
                        switch (strToolDataType)
                        {
                            case "LINETOOL":
                                {
                                    HTuple CaliperNum = int.Parse(strRtPara[0]);
                                    Enum.TryParse(strRtPara[1], out EnumLinePolarityType EdgeType);
                                    Enum.TryParse(strRtPara[2], out EnumSelectType SelectType);
                                    double.TryParse(strRtPara[3], out double fContrast);
                                    HTuple Contrast = Math.Round(fContrast);
                                    FindLine(image, EdgeType, SelectType, CaliperNum, Contrast, outRoiRow, outRoiCol, Phi + ModelPos[2], L1, L2, out HTuple StartRow, out HTuple StartCol, out HTuple EndRow, out HTuple EndCol);
                                    lineList.Add(new VisionLineData(StartRow, StartCol, EndRow, EndCol));
                                }
                                break;
                            case "PAIRTOOL":
                                {
                                    HTuple CaliperNum = int.Parse(strRtPara[0]);
                                    HTuple ExpectedPairNum = int.Parse(strRtPara[1]);
                                    Enum.TryParse(strRtPara[2], out EnumPairPolarityType PairType);
                                    Enum.TryParse(strRtPara[3], out EnumSelectType SelectType);
                                    double.TryParse(strRtPara[4], out double fContrast);
                                    HTuple Contrast = Math.Round(fContrast);
                                    FindPair(image, ExpectedPairNum, PairType, SelectType, CaliperNum, Contrast, outRoiRow, outRoiCol, Phi + ModelPos[2], L1, L2,
                                       out HTuple OutFirstRowStart, out HTuple FirstColStart, out HTuple OutFirstRowEnd, out HTuple OutFirstColEnd,
                                       out HTuple OutSecondRowStart, out HTuple SecondColStart, out HTuple OutSecondRowEnd, out HTuple OutSecondColEnd);
                                    for (HTuple index = 0; index < ExpectedPairNum; index++)
                                    {
                                        lineList.Add(new VisionLineData(OutFirstRowStart[index], FirstColStart[index], OutFirstRowEnd[index], OutFirstColEnd[index]));
                                        lineList.Add(new VisionLineData(OutSecondRowStart[index], SecondColStart[index], OutSecondRowEnd[index], OutSecondColEnd[index]));
                                    }
                                }
                                break;
                            default:
                                throw new Exception("invalid TOOL");
                        }  
                    }
                }
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool FindTia(HObject image, List<string> LineParaList, HTuple hom_2D, HTuple ModelPos, out List<object> lineList)
        {
            lineList = new List<object>();
            return true;
        }
        public bool DisplayLines(int nCamID, List<VisionLineData> lineList, string Color = "red")
        {
            try
            {
                var Cam = CheckCamIDAvilible(nCamID);
                lock (Cam.VisionLock)
                {
                    if (lineList == null)
                        return false;
                    foreach (var it in Cam.AttachedWindowDic)
                    {
                        HOperatorSet.SetColor(it.Value, Color);
                        foreach (var line in lineList)
                        {
                            HOperatorSet.DispLine(it.Value, line.RowStart, line.ColStart, line.RowEnd, line.ColEnd);
                        }
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public bool DisplayPolygonRegion(int nCamID, HTuple Row, HTuple Col, string Color = "red")
        {
            var Cam = CheckCamIDAvilible(nCamID);
            lock (Cam.VisionLock)
            {
                HOperatorSet.GenRegionPolygonFilled(out HObject region, Row, Col);
                foreach (var it in Cam.AttachedWindowDic)
                {
                    HOperatorSet.SetColor(it.Value, Color);
                    HOperatorSet.SetDraw(it.Value, "fill");
                    HOperatorSet.DispObj(region, it.Value);
                }
                region.Dispose();
                return true;
            }
        }

        /// <summary>
        /// 设定相机的像素对应的实际尺寸
        /// </summary>
        /// <param name="nCamID">相机ID</param>
        /// <param name="RealDistance">实际距离um</param>
        /// <param name="PLineList">应该包含至少两条平行线用来计算像素距离</param>
        /// <returns></returns>
        public bool SetKValueOfCam(int nCamID, double RealDistance, List<VisionLineData> LineList, out double factor)
        {
            var Cam=CheckCamIDAvilible(nCamID);
            factor = 1;
            if (LineList == null || LineList.Count < 2 || RealDistance == 0.0f)
                return false;
         
            HOperatorSet.DistancePl(LineList[0].RowStart, LineList[0].ColStart, LineList[1].RowStart, LineList[1].ColStart, LineList[1].RowEnd, LineList[1].ColEnd, out HTuple D1);
            HOperatorSet.DistancePl(LineList[0].RowEnd, LineList[0].ColEnd, LineList[1].RowStart, LineList[1].ColStart, LineList[1].RowEnd, LineList[1].ColEnd, out HTuple D2);
            factor = (2 * RealDistance) / (D1 + D2);
            Cam.KX = factor;
            Cam.KY = factor;
            return true;
        }
        public void SetRefreshWindow(int nCamID, bool bRefresh)
        {
            HOperatorSet.SetSystem("flush_graphic", bRefresh ? "true" : "false");
        }
        #endregion

        /// <summary>
        /// 创建模板前的预览
        /// </summary>
        /// <param name="nCamID"></param>
        /// <param name="MinThre"></param>
        /// <param name="MaxThre"></param>
        /// <param name="modelType"></param>
        /// <param name="regionFilePath"></param>
        /// <param name="regionIn"></param>
        /// <returns></returns>
        public bool PreCreateShapeModel(int nCamID, int MinThre, int MaxThre, EnumShapeModelType modelType, string regionFilePath, object regionIn = null)
        {
            var Cam = CheckCamIDAvilible(nCamID);
            if (MaxThre < MinThre)
                return false;
            if (Cam.AttachedWindowDic.Keys.Contains(WINDOW_DEBUG))
            {
                HTuple window = Cam.AttachedWindowDic[WINDOW_DEBUG];
                switch (modelType)
                {
                    case EnumShapeModelType.Gray:
                        break;
                    case EnumShapeModelType.Shape:
                        break;
                    case EnumShapeModelType.XLD:
                        HObject region = regionIn as HObject;
                        return PreProcessShapeMode(Cam.Image, window, MinThre, MaxThre, region, regionFilePath, true);
                    default:
                        return false;
                }
            }
            return true;
        }
        public bool SaveShapeModel(int nCamID, int MinThre, int MaxThre, EnumShapeModelType modelType, string regionFilePath, object regionIn = null)
        {
            var Cam = CheckCamIDAvilible(nCamID);
            if (MaxThre < MinThre)
                return false;
            if (Cam.AttachedWindowDic.Keys.Contains(WINDOW_DEBUG))
            {
                HTuple window = Cam.AttachedWindowDic[WINDOW_DEBUG];
                switch (modelType)
                {
                    case EnumShapeModelType.Gray:
                        break;
                    case EnumShapeModelType.Shape:
                        break;
                    case EnumShapeModelType.XLD:
                        HObject region = regionIn as HObject;
                        return PreProcessShapeMode(Cam.Image, window, MinThre, MaxThre, region, regionFilePath, false);
                    default:
                        return false;
                }
            }
            return true;
        }
        public object ReadRegion(string regionPath)
        {
            if (File.Exists(regionPath))
            {
                try
                {
                    HOperatorSet.ReadRegion(out HObject region, regionPath);
                    return region;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return null;
        }

       

        #region Private method
        private void FindLine(HObject ho_Image, EnumLinePolarityType Polarity, EnumSelectType selectType, HTuple hv_CaliperNum, HTuple hv_EdgeGrayValue, HTuple hv_RoiRow, HTuple hv_RoiCol, HTuple hv_RoiPhi, HTuple hv_RoiL1, HTuple hv_RoiL2, out HTuple hv_OutRowStart, out HTuple hv_OutColStart, out HTuple hv_OutRowEnd, out HTuple hv_OutColEnd)
        {
            // Local iconic variables 
            HObject ho_Rectangle, ho_Contour = null;
            // Local control variables 
            HTuple hv_Width, hv_Height, hv_newL2, hv_newL1;
            HTuple hv_Sin, hv_Cos, hv_BaseRow, hv_BaseCol, hv_newRow;
            HTuple hv_newCol, hv_ptRow, hv_ptCol, hv_nCount, hv_Index;
            HTuple hv_MeasureHandle = new HTuple(), hv_RowEdge = new HTuple();
            HTuple hv_ColumnEdge = new HTuple(), hv_Amplitude = new HTuple();
            HTuple hv_Distance = new HTuple(), hv_RowBegin = new HTuple();
            HTuple hv_ColBegin = new HTuple(), hv_RowEnd = new HTuple();
            HTuple hv_ColEnd = new HTuple(), hv_Nr = new HTuple(), hv_Nc = new HTuple();
            HTuple hv_Dist = new HTuple(), hv_Polarity = new HTuple(), hv_SelectType = new HTuple();
            HTuple hv_CaliperNum_COPY_INP_TMP = hv_CaliperNum.Clone();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_Contour);


            hv_OutRowStart = new HTuple();
            hv_OutColStart = new HTuple();
            hv_OutRowEnd = new HTuple();
            hv_OutColEnd = new HTuple();
            ho_Rectangle.Dispose();
            HOperatorSet.GenRectangle2(out ho_Rectangle, hv_RoiRow, hv_RoiCol, hv_RoiPhi,
                hv_RoiL1, hv_RoiL2);
            HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
            //卡尺数量
            if ((int)(new HTuple(hv_CaliperNum_COPY_INP_TMP.TupleLessEqual(1))) != 0)
            {
                hv_CaliperNum_COPY_INP_TMP = 2;
            }
            hv_newL2 = hv_RoiL2 / (hv_CaliperNum_COPY_INP_TMP - 1);
            hv_newL1 = hv_RoiL1.Clone();
            HOperatorSet.TupleSin(hv_RoiPhi, out hv_Sin);
            HOperatorSet.TupleCos(hv_RoiPhi, out hv_Cos);

            hv_BaseRow = hv_RoiRow + (hv_RoiL2 * hv_Cos);
            hv_BaseCol = hv_RoiCol + (hv_RoiL2 * hv_Sin);

            hv_newRow = hv_BaseRow.Clone();
            hv_newCol = hv_BaseCol.Clone();
            hv_ptRow = new HTuple();
            hv_ptCol = new HTuple();
            hv_nCount = 0;

            switch (Polarity)
            {
                case EnumLinePolarityType.LightToDark:
                    hv_Polarity = "negative";
                    break;
                case EnumLinePolarityType.DarkToLight:
                    hv_Polarity = "positive";
                    break;
                case EnumLinePolarityType.All:
                    hv_Polarity = "all";
                    break;
            }
            switch (selectType)
            {
                case EnumSelectType.First:
                    hv_SelectType = "first";
                    break;
                case EnumSelectType.Last:
                    hv_SelectType = "last";
                    break;
                case EnumSelectType.All:
                    hv_SelectType = "all";
                    break;
            }
            for (hv_Index = 1; hv_Index.Continue(hv_CaliperNum_COPY_INP_TMP, 1); hv_Index = hv_Index.TupleAdd(1))
            {
                HOperatorSet.GenMeasureRectangle2(hv_newRow, hv_newCol, hv_RoiPhi, hv_newL1,
                    hv_newL2, hv_Width, hv_Height, "nearest_neighbor", out hv_MeasureHandle);

                HOperatorSet.MeasurePos(ho_Image, hv_MeasureHandle, 1, hv_EdgeGrayValue, hv_Polarity,
                    hv_SelectType, out hv_RowEdge, out hv_ColumnEdge, out hv_Amplitude, out hv_Distance);
                hv_newRow = hv_BaseRow - (((hv_newL2 * hv_Cos) * hv_Index) * 2);
                hv_newCol = hv_BaseCol - (((hv_newL2 * hv_Sin) * hv_Index) * 2);
                if ((int)(new HTuple((new HTuple(hv_RowEdge.TupleLength())).TupleGreater(0))) != 0)
                {
                    hv_ptRow[hv_nCount] = hv_RowEdge;
                    hv_ptCol[hv_nCount] = hv_ColumnEdge;
                    hv_nCount = hv_nCount + 1;
                }
                HOperatorSet.CloseMeasure(hv_MeasureHandle);
            }
            if ((int)(new HTuple((new HTuple(hv_ptRow.TupleLength())).TupleGreater(1))) != 0)
            {
                ho_Contour.Dispose();
                HOperatorSet.GenContourPolygonXld(out ho_Contour, hv_ptRow, hv_ptCol);
                HOperatorSet.FitLineContourXld(ho_Contour, "tukey", -1, 0, 5, 2, out hv_RowBegin,
                    out hv_ColBegin, out hv_RowEnd, out hv_ColEnd, out hv_Nr, out hv_Nc, out hv_Dist);
                hv_OutRowStart = hv_RowBegin.Clone();
                hv_OutColStart = hv_ColBegin.Clone();
                hv_OutRowEnd = hv_RowEnd.Clone();
                hv_OutColEnd = hv_ColEnd.Clone();
            }
            else
            {
                hv_OutRowStart = 0;
                hv_OutColStart = 0;
                hv_OutRowEnd = 0;
                hv_OutColEnd = 0;
            }
            ho_Rectangle.Dispose();
            ho_Contour.Dispose();

            return;
        }
        private void scale_image_range(HObject ho_Image, out HObject ho_ImageScaled, HTuple hv_Min, HTuple hv_Max)
        {
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];
            long SP_O = 0;

            // Local iconic variables 

            HObject ho_SelectedChannel = null, ho_LowerRegion = null;
            HObject ho_UpperRegion = null;

            HObject ho_Image_COPY_INP_TMP;
            ho_Image_COPY_INP_TMP = ho_Image.CopyObj(1, -1);


            // Local control variables 

            HTuple hv_LowerLimit = new HTuple(), hv_UpperLimit = new HTuple();
            HTuple hv_Mult, hv_Add, hv_Channels, hv_Index, hv_MinGray = new HTuple();
            HTuple hv_MaxGray = new HTuple(), hv_Range = new HTuple();

            HTuple hv_Max_COPY_INP_TMP = hv_Max.Clone();
            HTuple hv_Min_COPY_INP_TMP = hv_Min.Clone();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageScaled);
            HOperatorSet.GenEmptyObj(out ho_SelectedChannel);
            HOperatorSet.GenEmptyObj(out ho_LowerRegion);
            HOperatorSet.GenEmptyObj(out ho_UpperRegion);

            //Convenience procedure to scale the gray values of the
            //input image Image from the interval [Min,Max]
            //to the interval [0,255] (default).
            //Gray values < 0 or > 255 (after scaling) are clipped.
            //
            //If the image shall be scaled to an interval different from [0,255],
            //this can be achieved by passing tuples with 2 values [From, To]
            //as Min and Max.
            //Example:
            //scale_image_range(Image:ImageScaled:[100,50],[200,250])
            //maps the gray values of Image from the interval [100,200] to [50,250].
            //All other gray values will be clipped.
            //
            //input parameters:
            //Image: the input image
            //Min: the minimum gray value which will be mapped to 0
            //     If a tuple with two values is given, the first value will
            //     be mapped to the second value.
            //Max: The maximum gray value which will be mapped to 255
            //     If a tuple with two values is given, the first value will
            //     be mapped to the second value.
            //
            //output parameter:
            //ImageScale: the resulting scaled image
            //

            if ((int)(new HTuple((new HTuple(hv_Min_COPY_INP_TMP.TupleLength())).TupleEqual(2))) != 0)
            {
                hv_LowerLimit = hv_Min_COPY_INP_TMP[1];
                hv_Min_COPY_INP_TMP = hv_Min_COPY_INP_TMP[0];
            }
            else
            {
                hv_LowerLimit = 0.0;
            }
            if ((int)(new HTuple((new HTuple(hv_Max_COPY_INP_TMP.TupleLength())).TupleEqual(
                2))) != 0)
            {
                hv_UpperLimit = hv_Max_COPY_INP_TMP[1];
                hv_Max_COPY_INP_TMP = hv_Max_COPY_INP_TMP[0];
            }
            else
            {
                hv_UpperLimit = 255.0;
            }
            //
            //Calculate scaling parameters
            hv_Mult = (((hv_UpperLimit - hv_LowerLimit)).TupleReal()) / (hv_Max_COPY_INP_TMP - hv_Min_COPY_INP_TMP);
            hv_Add = ((-hv_Mult) * hv_Min_COPY_INP_TMP) + hv_LowerLimit;
            //
            //Scale image
            OTemp[SP_O] = ho_Image_COPY_INP_TMP.CopyObj(1, -1);
            SP_O++;
            ho_Image_COPY_INP_TMP.Dispose();
            HOperatorSet.ScaleImage(OTemp[SP_O - 1], out ho_Image_COPY_INP_TMP, hv_Mult, hv_Add);
            OTemp[SP_O - 1].Dispose();
            SP_O = 0;
            //
            //Clip gray values if necessary
            //This must be done for each channel separately
            HOperatorSet.CountChannels(ho_Image_COPY_INP_TMP, out hv_Channels);
            for (hv_Index = 1; hv_Index.Continue(hv_Channels, 1); hv_Index = hv_Index.TupleAdd(1))
            {
                ho_SelectedChannel.Dispose();
                HOperatorSet.AccessChannel(ho_Image_COPY_INP_TMP, out ho_SelectedChannel, hv_Index);
                HOperatorSet.MinMaxGray(ho_SelectedChannel, ho_SelectedChannel, 0, out hv_MinGray,
                    out hv_MaxGray, out hv_Range);
                ho_LowerRegion.Dispose();
                HOperatorSet.Threshold(ho_SelectedChannel, out ho_LowerRegion, ((hv_MinGray.TupleConcat(
                    hv_LowerLimit))).TupleMin(), hv_LowerLimit);
                ho_UpperRegion.Dispose();
                HOperatorSet.Threshold(ho_SelectedChannel, out ho_UpperRegion, hv_UpperLimit,
                    ((hv_UpperLimit.TupleConcat(hv_MaxGray))).TupleMax());
                OTemp[SP_O] = ho_SelectedChannel.CopyObj(1, -1);
                SP_O++;
                ho_SelectedChannel.Dispose();
                HOperatorSet.PaintRegion(ho_LowerRegion, OTemp[SP_O - 1], out ho_SelectedChannel,
                    hv_LowerLimit, "fill");
                OTemp[SP_O - 1].Dispose();
                SP_O = 0;
                OTemp[SP_O] = ho_SelectedChannel.CopyObj(1, -1);
                SP_O++;
                ho_SelectedChannel.Dispose();
                HOperatorSet.PaintRegion(ho_UpperRegion, OTemp[SP_O - 1], out ho_SelectedChannel,
                    hv_UpperLimit, "fill");
                OTemp[SP_O - 1].Dispose();
                SP_O = 0;
                if ((int)(new HTuple(hv_Index.TupleEqual(1))) != 0)
                {
                    ho_ImageScaled.Dispose();
                    HOperatorSet.CopyObj(ho_SelectedChannel, out ho_ImageScaled, 1, 1);
                }
                else
                {
                    OTemp[SP_O] = ho_ImageScaled.CopyObj(1, -1);
                    SP_O++;
                    ho_ImageScaled.Dispose();
                    HOperatorSet.AppendChannel(OTemp[SP_O - 1], ho_SelectedChannel, out ho_ImageScaled
                        );
                    OTemp[SP_O - 1].Dispose();
                    SP_O = 0;
                }
            }
            ho_Image_COPY_INP_TMP.Dispose();
            ho_SelectedChannel.Dispose();
            ho_LowerRegion.Dispose();
            ho_UpperRegion.Dispose();

            return;
        }
        public void disp_message(HTuple hv_WindowHandle, HTuple hv_String, HTuple hv_CoordSystem, HTuple hv_Row, HTuple hv_Column, HTuple hv_Color, HTuple hv_Box)
        {
            // Local control variables 
            HTuple hv_Red, hv_Green, hv_Blue, hv_Row1Part;
            HTuple hv_Column1Part, hv_Row2Part, hv_Column2Part, hv_RowWin;
            HTuple hv_ColumnWin, hv_WidthWin, hv_HeightWin, hv_MaxAscent;
            HTuple hv_MaxDescent, hv_MaxWidth, hv_MaxHeight, hv_R1 = new HTuple();
            HTuple hv_C1 = new HTuple(), hv_FactorRow = new HTuple(), hv_FactorColumn = new HTuple();
            HTuple hv_Width = new HTuple(), hv_Index = new HTuple(), hv_Ascent = new HTuple();
            HTuple hv_Descent = new HTuple(), hv_W = new HTuple(), hv_H = new HTuple();
            HTuple hv_FrameHeight = new HTuple(), hv_FrameWidth = new HTuple();
            HTuple hv_R2 = new HTuple(), hv_C2 = new HTuple(), hv_DrawMode = new HTuple();
            HTuple hv_Exception = new HTuple(), hv_CurrentColor = new HTuple();

            HTuple hv_Color_COPY_INP_TMP = hv_Color.Clone();
            HTuple hv_Column_COPY_INP_TMP = hv_Column.Clone();
            HTuple hv_Row_COPY_INP_TMP = hv_Row.Clone();
            HTuple hv_String_COPY_INP_TMP = hv_String.Clone();

            // Initialize local and output iconic variables 

            //This procedure displays text in a graphics window.
            //
            //Input parameters:
            //WindowHandle: The WindowHandle of the graphics window, where
            //   the message should be displayed
            //String: A tuple of strings containing the text message to be displayed
            //CoordSystem: If set to 'window', the text position is given
            //   with respect to the window coordinate system.
            //   If set to 'image', image coordinates are used.
            //   (This may be useful in zoomed images.)
            //Row: The row coordinate of the desired text position
            //   If set to -1, a default value of 12 is used.
            //Column: The column coordinate of the desired text position
            //   If set to -1, a default value of 12 is used.
            //Color: defines the color of the text as string.
            //   If set to [], '' or 'auto' the currently set color is used.
            //   If a tuple of strings is passed, the colors are used cyclically
            //   for each new textline.
            //Box: If set to 'true', the text is written within a white box.
            //
            //prepare window
            HOperatorSet.GetRgb(hv_WindowHandle, out hv_Red, out hv_Green, out hv_Blue);
            HOperatorSet.GetPart(hv_WindowHandle, out hv_Row1Part, out hv_Column1Part, out hv_Row2Part,
                out hv_Column2Part);
            HOperatorSet.GetWindowExtents(hv_WindowHandle, out hv_RowWin, out hv_ColumnWin,
                out hv_WidthWin, out hv_HeightWin);
            HOperatorSet.SetPart(hv_WindowHandle, 0, 0, hv_HeightWin - 1, hv_WidthWin - 1);
            //
            //default settings
            if ((int)(new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Row_COPY_INP_TMP = 12;
            }
            if ((int)(new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Column_COPY_INP_TMP = 12;
            }
            if ((int)(new HTuple(hv_Color_COPY_INP_TMP.TupleEqual(new HTuple()))) != 0)
            {
                hv_Color_COPY_INP_TMP = "";
            }
            //
            hv_String_COPY_INP_TMP = ((("" + hv_String_COPY_INP_TMP) + "")).TupleSplit("\n");
            //
            //Estimate extentions of text depending on font size.
            HOperatorSet.GetFontExtents(hv_WindowHandle, out hv_MaxAscent, out hv_MaxDescent,
                out hv_MaxWidth, out hv_MaxHeight);
            if ((int)(new HTuple(hv_CoordSystem.TupleEqual("window"))) != 0)
            {
                hv_R1 = hv_Row_COPY_INP_TMP.Clone();
                hv_C1 = hv_Column_COPY_INP_TMP.Clone();
            }
            else
            {
                //transform image to window coordinates
                hv_FactorRow = (1.0 * hv_HeightWin) / ((hv_Row2Part - hv_Row1Part) + 1);
                hv_FactorColumn = (1.0 * hv_WidthWin) / ((hv_Column2Part - hv_Column1Part) + 1);
                hv_R1 = ((hv_Row_COPY_INP_TMP - hv_Row1Part) + 0.5) * hv_FactorRow;
                hv_C1 = ((hv_Column_COPY_INP_TMP - hv_Column1Part) + 0.5) * hv_FactorColumn;
            }
            //
            //display text box depending on text size
            if ((int)(new HTuple(hv_Box.TupleEqual("true"))) != 0)
            {
                //calculate box extents
                hv_String_COPY_INP_TMP = (" " + hv_String_COPY_INP_TMP) + " ";
                hv_Width = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    HOperatorSet.GetStringExtents(hv_WindowHandle, hv_String_COPY_INP_TMP.TupleSelect(
                        hv_Index), out hv_Ascent, out hv_Descent, out hv_W, out hv_H);
                    hv_Width = hv_Width.TupleConcat(hv_W);
                }
                hv_FrameHeight = hv_MaxHeight * (new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                    ));
                hv_FrameWidth = (((new HTuple(0)).TupleConcat(hv_Width))).TupleMax();
                hv_R2 = hv_R1 + hv_FrameHeight;
                hv_C2 = hv_C1 + hv_FrameWidth;
                //display rectangles
                HOperatorSet.GetDraw(hv_WindowHandle, out hv_DrawMode);
                HOperatorSet.SetDraw(hv_WindowHandle, "fill");
                HOperatorSet.SetColor(hv_WindowHandle, "light gray");
                HOperatorSet.DispRectangle1(hv_WindowHandle, hv_R1 + 3, hv_C1 + 3, hv_R2 + 3, hv_C2 + 3);
                HOperatorSet.SetColor(hv_WindowHandle, "white");
                HOperatorSet.DispRectangle1(hv_WindowHandle, hv_R1, hv_C1, hv_R2, hv_C2);
                HOperatorSet.SetDraw(hv_WindowHandle, hv_DrawMode);
            }
            else if ((int)(new HTuple(hv_Box.TupleNotEqual("false"))) != 0)
            {
                hv_Exception = "Wrong value of control parameter Box";
                throw new HalconException(hv_Exception);
            }
            //Write text.
            for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                )) - 1); hv_Index = (int)hv_Index + 1)
            {
                hv_CurrentColor = hv_Color_COPY_INP_TMP.TupleSelect(hv_Index % (new HTuple(hv_Color_COPY_INP_TMP.TupleLength()
                    )));
                if ((int)((new HTuple(hv_CurrentColor.TupleNotEqual(""))).TupleAnd(new HTuple(hv_CurrentColor.TupleNotEqual(
                    "auto")))) != 0)
                {
                    HOperatorSet.SetColor(hv_WindowHandle, hv_CurrentColor);
                }
                else
                {
                    HOperatorSet.SetRgb(hv_WindowHandle, hv_Red, hv_Green, hv_Blue);
                }
                hv_Row_COPY_INP_TMP = hv_R1 + (hv_MaxHeight * hv_Index);
                HOperatorSet.SetTposition(hv_WindowHandle, hv_Row_COPY_INP_TMP, hv_C1);
                HOperatorSet.WriteString(hv_WindowHandle, hv_String_COPY_INP_TMP.TupleSelect(
                    hv_Index));
            }
            //reset changed window settings
            HOperatorSet.SetRgb(hv_WindowHandle, hv_Red, hv_Green, hv_Blue);
            HOperatorSet.SetPart(hv_WindowHandle, hv_Row1Part, hv_Column1Part, hv_Row2Part,
                hv_Column2Part);

            return;
        }
        private bool PreProcessShapeMode(HObject ImageIn, HTuple window, HTuple MinThre, HTuple MaxThre, HObject RegionDomain, string strRegionPath, bool bPreView = true)
        {
            try
            {
                if (RegionDomain == null)
                    HOperatorSet.GetDomain(ImageIn, out RegionDomain);

                // Local iconic variables 
                HObject ho_ImageReduced, ho_Regions2, ho_RegionFillUp1;
                HObject ho_Contours1;
                HObject emptObject = null;
                // Local control variables 
                HTuple hv_Width, hv_Height, hv_ModelID;

                // Initialize local and output iconic variables 
                HOperatorSet.GenEmptyObj(out ho_ImageReduced);
                HOperatorSet.GenEmptyObj(out ho_Regions2);
                HOperatorSet.GenEmptyObj(out ho_RegionFillUp1);
                HOperatorSet.GenEmptyObj(out ho_Contours1);


                HOperatorSet.GetImageSize(ImageIn, out hv_Width, out hv_Height);
                ho_ImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ImageIn, RegionDomain, out ho_ImageReduced);

                ho_Regions2.Dispose();
                HOperatorSet.Threshold(ho_ImageReduced, out ho_Regions2, MinThre, MaxThre);
                ho_RegionFillUp1.Dispose();
                HOperatorSet.FillUpShape(ho_Regions2, out ho_RegionFillUp1, "area", 1, 500);
                ho_Contours1.Dispose();
                HOperatorSet.GenContourRegionXld(ho_RegionFillUp1, out ho_Contours1, "border");
                if (bPreView == false)
                {
                    HOperatorSet.CreateShapeModelXld(ho_Contours1, "auto", (new HTuple(0)).TupleRad(), (new HTuple(360)).TupleRad(), "auto", "auto", "ignore_local_polarity", 5, out hv_ModelID);
                    HOperatorSet.FindShapeModel(ImageIn, hv_ModelID, (new HTuple(0)).TupleRad(), (new HTuple(360)).TupleRad(), 0.5, 1, 0.5, "least_squares", 0, 0.9, out HTuple hv_Row, out HTuple hv_Column, out HTuple hv_Angle, out HTuple hv_Score);

                    HTuple hv_ModelPos = new HTuple();
                    hv_ModelPos[0] = hv_Row;
                    hv_ModelPos[1] = hv_Column;
                    hv_ModelPos[2] = hv_Angle;
                    string[] strList = strRegionPath.Split('.');
                    HOperatorSet.WriteShapeModel(hv_ModelID, $"{strList[0]}.shm");
                    HOperatorSet.WriteTuple(hv_ModelPos, $"{strList[0]}.tup");
                    HOperatorSet.WriteRegion(RegionDomain, $"{strList[0]}.reg");
                }

                HOperatorSet.SetDraw(window, "fill");
                HOperatorSet.SetColor(window, "red");
                HOperatorSet.SetSystem("flush_graphic", "false");
                HOperatorSet.ClearWindow(window);
                HOperatorSet.DispObj(ImageIn, window);
                HOperatorSet.DispObj(ho_Contours1, window); //显示模板轮廓
                HOperatorSet.SetSystem("flush_graphic", "true");
                HOperatorSet.GenEmptyObj(out emptObject);
                HOperatorSet.DispObj(emptObject, window);


                emptObject.Dispose();
                RegionDomain.Dispose();
                ho_ImageReduced.Dispose();
                ho_Regions2.Dispose();
                ho_RegionFillUp1.Dispose();
                ho_Contours1.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void Debug_DrawRectangle2(HTuple WindowHandle, out HTuple Row, out HTuple Col, out HTuple Phi, out HTuple L1, out HTuple L2)
        {
            Row = Col = Phi = L1 = L2 = 0;
            HOperatorSet.GetPart(WindowHandle, out HTuple oldRow1, out HTuple oldCol1, out HTuple oldRow2, out HTuple oldCol2);
            HOperatorSet.DrawRectangle2Mod(WindowHandle, (oldRow1 + oldRow2) / 2, (oldCol1 + oldCol2) / 2, 0, 100, 100, out Row, out Col, out Phi, out L1, out L2);
        }
        private void FindPair(HObject ho_Image, HTuple ExpectPairNum, EnumPairPolarityType Polarity, EnumSelectType selectType, HTuple hv_CaliperNum, HTuple hv_EdgeGrayValue, HTuple hv_RoiRow, HTuple hv_RoiCol, HTuple hv_RoiPhi, HTuple hv_RoiL1, HTuple hv_RoiL2,
                              out HTuple hv_OutFirstRowStart, out HTuple hv_OutFirstColStart, out HTuple hv_OutFirstRowEnd, out HTuple hv_OutFirstColEnd,
                              out HTuple hv_OutSecondRowStart, out HTuple hv_OutSecondColStart, out HTuple hv_OutSecondRowEnd, out HTuple hv_OutSecondColEnd)
        {
            // Local iconic variables 
            HObject ho_Rectangle, ho_Contour = null;
            // Local control variables 
            HTuple hv_Width, hv_Height, hv_newL2, hv_newL1;
            HTuple hv_Sin, hv_Cos, hv_BaseRow, hv_BaseCol, hv_newRow;
            HTuple hv_newCol, hv_nCount, hv_Index;
            HTuple hv_MeasureHandle = new HTuple();
            HTuple hv_Distance = new HTuple(), hv_RowBegin = new HTuple();
            HTuple hv_ColBegin = new HTuple(), hv_RowEnd = new HTuple();
            HTuple hv_ColEnd = new HTuple(), hv_Nr = new HTuple(), hv_Nc = new HTuple();
            HTuple hv_Dist = new HTuple(), hv_Polarity = new HTuple(), hv_SelectType = new HTuple();
            HTuple hv_CaliperNum_COPY_INP_TMP = hv_CaliperNum.Clone();
            List<HTuple> RowFirstList = new List<HTuple>();
            List<HTuple> ColFirstList = new List<HTuple>();
            List<HTuple> RowSecondList = new List<HTuple>();
            List<HTuple> ColSecondList = new List<HTuple>();
            for (int i = 0; i < ExpectPairNum.I; i++)
            {
                RowFirstList.Add(new HTuple());
                ColFirstList.Add(new HTuple());
                RowSecondList.Add(new HTuple());
                ColSecondList.Add(new HTuple());

            }



            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_Contour);


            hv_OutFirstRowStart = new HTuple();
            hv_OutFirstColStart = new HTuple();
            hv_OutFirstRowEnd = new HTuple();
            hv_OutFirstColEnd = new HTuple();
            hv_OutSecondRowStart = new HTuple();
            hv_OutSecondColStart = new HTuple();
            hv_OutSecondRowEnd = new HTuple();
            hv_OutSecondColEnd = new HTuple();
            ho_Rectangle.Dispose();
            HOperatorSet.GenRectangle2(out ho_Rectangle, hv_RoiRow, hv_RoiCol, hv_RoiPhi,
                hv_RoiL1, hv_RoiL2);
            HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
            //卡尺数量
            if ((int)(new HTuple(hv_CaliperNum_COPY_INP_TMP.TupleLessEqual(1))) != 0)
            {
                hv_CaliperNum_COPY_INP_TMP = 2;
            }
            hv_newL2 = hv_RoiL2 / (hv_CaliperNum_COPY_INP_TMP - 1);
            hv_newL1 = hv_RoiL1.Clone();
            HOperatorSet.TupleSin(hv_RoiPhi, out hv_Sin);
            HOperatorSet.TupleCos(hv_RoiPhi, out hv_Cos);

            hv_BaseRow = hv_RoiRow + (hv_RoiL2 * hv_Cos);
            hv_BaseCol = hv_RoiCol + (hv_RoiL2 * hv_Sin);

            hv_newRow = hv_BaseRow.Clone();
            hv_newCol = hv_BaseCol.Clone();

            hv_nCount = 0;

            switch (Polarity)
            {
                case EnumPairPolarityType.Dark:
                    hv_Polarity = "negative";
                    break;
                case EnumPairPolarityType.Light:
                    hv_Polarity = "positive";
                    break;
                case EnumPairPolarityType.All:
                    hv_Polarity = "all";
                    break;
            }
            switch (selectType)
            {
                case EnumSelectType.First:
                    hv_SelectType = "first";
                    break;
                case EnumSelectType.Last:
                    hv_SelectType = "last";
                    break;
                case EnumSelectType.All:
                    hv_SelectType = "all";
                    break;
            }

            for (hv_Index = 1; hv_Index.Continue(hv_CaliperNum_COPY_INP_TMP, 1); hv_Index = hv_Index.TupleAdd(1))
            {
                HOperatorSet.GenMeasureRectangle2(hv_newRow, hv_newCol, hv_RoiPhi, hv_newL1,
                    hv_newL2, hv_Width, hv_Height, "nearest_neighbor", out hv_MeasureHandle);
                HOperatorSet.GenRectangle2(out HObject rectDebug, hv_newRow, hv_newCol, hv_RoiPhi, hv_newL1, hv_newL2);
                HOperatorSet.MeasurePairs(ho_Image, hv_MeasureHandle, 1, hv_EdgeGrayValue, hv_Polarity, hv_SelectType, out HTuple rowEdgeFirst, out HTuple columnEdgeFirst,
                                        out HTuple amplitudeFirst, out HTuple rowEdgeSecond, out HTuple columnEdgeSecond, out HTuple amplitudeSecond, out HTuple intraDistance, out HTuple interDistance);

                hv_newRow = hv_BaseRow - (((hv_newL2 * hv_Cos) * hv_Index) * 2);
                hv_newCol = hv_BaseCol - (((hv_newL2 * hv_Sin) * hv_Index) * 2);

                if (rowEdgeFirst.Length != 0 && rowEdgeFirst.Length == rowEdgeSecond.Length && ExpectPairNum == rowEdgeFirst.Length)
                {
                    for (HTuple i = 0; i < ExpectPairNum; i++)
                    {
                        RowFirstList[i][hv_nCount] = rowEdgeFirst[i];
                        ColFirstList[i][hv_nCount] = columnEdgeFirst[i];
                        RowSecondList[i][hv_nCount] = rowEdgeSecond[i];
                        ColSecondList[i][hv_nCount] = columnEdgeSecond[i];
                    }
                    hv_nCount = hv_nCount + 1;
                }
                HOperatorSet.CloseMeasure(hv_MeasureHandle);
            }
            for (int i = 0; i < RowFirstList.Count; i++)
            {
                if (RowFirstList[i].Length != 0)
                {
                    ho_Contour.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_Contour, RowFirstList[i], ColFirstList[i]);
                    HOperatorSet.FitLineContourXld(ho_Contour, "tukey", -1, 0, 5, 2, out hv_RowBegin,
                        out hv_ColBegin, out hv_RowEnd, out hv_ColEnd, out hv_Nr, out hv_Nc, out hv_Dist);
                    hv_OutFirstRowStart[i] = hv_RowBegin.Clone();
                    hv_OutFirstColStart[i] = hv_ColBegin.Clone();
                    hv_OutFirstRowEnd[i] = hv_RowEnd.Clone();
                    hv_OutFirstColEnd[i] = hv_ColEnd.Clone();

                    ho_Contour.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_Contour, RowSecondList[i], ColSecondList[i]);
                    HOperatorSet.FitLineContourXld(ho_Contour, "tukey", -1, 0, 5, 2, out hv_RowBegin,
                        out hv_ColBegin, out hv_RowEnd, out hv_ColEnd, out hv_Nr, out hv_Nc, out hv_Dist);
                    hv_OutSecondRowStart[i] = hv_RowBegin.Clone();
                    hv_OutSecondColStart[i] = hv_ColBegin.Clone();
                    hv_OutSecondRowEnd[i] = hv_RowEnd.Clone();
                    hv_OutSecondColEnd[i] = hv_ColEnd.Clone();
                }
                else
                {
                    hv_OutFirstRowStart[i] = 0;
                    hv_OutFirstColStart[i] = 0;
                    hv_OutFirstRowEnd[i] = 0;
                    hv_OutFirstColEnd[i] = 0;
                    hv_OutSecondRowStart[i] = 0;
                    hv_OutSecondColStart[i] = 0;
                    hv_OutSecondRowEnd[i] = 0;
                    hv_OutSecondColEnd[i] = 0;
                }
            }
            ho_Rectangle.Dispose();
            ho_Contour.Dispose();

            return;
        }

        /// <summary>
        ///  找指定距离的垂线
        /// </summary>
        /// <param name="hv_FootRow"></param>
        /// <param name="hv_FootCol"></param>
        /// <param name="hv_LineRowStart"></param>
        /// <param name="hv_LineColStart"></param>
        /// <param name="hv_LineRowEnd"></param>
        /// <param name="hv_LineColEnd"></param>
        /// <param name="hv_Distance"></param>
        /// <param name="hv_Direction"></param>
        /// <param name="hv_Polarity"></param>
        /// <param name="hv_TargetRow"></param>
        /// <param name="hv_TargetCol"></param>
        /// <param name="hv_k"></param>
        /// <param name="hv_b"></param>
        /// <param name="hv_kIn"></param>
        /// <param name="hv_bIn"></param>
        public void GetVerticalFromDistance(HTuple hv_FootRow, HTuple hv_FootCol, HTuple hv_LineRowStart,
            HTuple hv_LineColStart, HTuple hv_LineRowEnd, HTuple hv_LineColEnd, HTuple hv_Distance,
            HTuple hv_Direction, HTuple hv_Polarity, out HTuple hv_TargetRow, out HTuple hv_TargetCol,
            out HTuple hv_k, out HTuple hv_b, out HTuple hv_kIn, out HTuple hv_bIn)
        {


            // Local control variables 

            HTuple hv_RealDeltaCol, hv_RealDeltaRow, hv_Sqrt;
            HTuple hv_row1, hv_col1, hv_row2, hv_col2 = new HTuple();

            // Initialize local and output iconic variables 

            hv_TargetRow = new HTuple();
            hv_TargetCol = new HTuple();
            HOperatorSet.TupleReal(hv_LineColEnd - hv_LineColStart, out hv_RealDeltaCol);
            HOperatorSet.TupleReal(hv_LineRowStart - hv_LineRowEnd, out hv_RealDeltaRow);

            hv_kIn = hv_RealDeltaCol / hv_RealDeltaRow;
            hv_bIn = hv_FootCol - (hv_kIn * hv_FootRow);

            hv_k = -1 / hv_kIn;
            hv_b = ((hv_FootRow * hv_kIn) + hv_bIn) - (hv_k * hv_FootRow);

            //找出目标点
            HOperatorSet.TupleSqrt((hv_Distance * hv_Distance) / ((hv_k * hv_k) + 1), out hv_Sqrt);
            hv_row1 = hv_Sqrt + hv_FootRow;
            hv_col1 = (hv_k * hv_row1) + hv_b;

            hv_row2 = (-hv_Sqrt) + hv_FootRow;
            hv_col2 = (hv_k * hv_row2) + hv_b;

            if ((int)(new HTuple(hv_Direction.TupleEqual("row"))) != 0)
            {
                if ((int)(new HTuple(hv_Polarity.TupleGreaterEqual(0))) != 0)
                {
                    hv_TargetRow = hv_row1.Clone();
                    hv_TargetCol = hv_col1.Clone();
                }
                else
                {
                    hv_TargetRow = hv_row2.Clone();
                    hv_TargetCol = hv_col2.Clone();
                }
            }

            if ((int)(new HTuple(hv_Direction.TupleEqual("col"))) != 0)
            {
                if ((int)(new HTuple(hv_Polarity.TupleGreaterEqual(0))) != 0)
                {
                    if ((int)(new HTuple(hv_col1.TupleGreaterEqual(hv_FootCol))) != 0)
                    {
                        hv_TargetCol = hv_col1.Clone();
                        hv_TargetRow = hv_row1.Clone();
                    }
                    else
                    {
                        hv_TargetCol = hv_col2.Clone();
                        hv_TargetRow = hv_row1.Clone();
                    }
                }
                else
                {
                    if ((int)(new HTuple(hv_col1.TupleGreaterEqual(hv_FootCol))) != 0)
                    {
                        hv_TargetCol = hv_col2.Clone();
                        hv_TargetRow = hv_row2.Clone();
                    }
                    else
                    {
                        hv_TargetCol = hv_col1.Clone();
                        hv_TargetRow = hv_row1.Clone();
                    }
                }
            }


            return;
        }
        /// <summary>
        /// 求平行线
        /// </summary>
        /// <param name="hv_LineInRow">直线起点Row</param>
        /// <param name="hv_LineInCol">直线起点Col</param>
        /// <param name="hv_LineInRow1">直线终点Row</param>
        /// <param name="hv_LineInCol1">直线终点Col</param>
        /// <param name="hv_Distance">平行线距离</param>
        /// <param name="hv_Direction">方向"row"或者"col"</param>
        /// <param name="hv_Polarity">极性，-1代表减小方向，1代表增大方向</param>
        /// <param name="hv_LineOutRow">平行线起点Row</param>
        /// <param name="hv_LineOutCol">平行线起点Col</param>
        /// <param name="hv_LineOutRow1">平行线终点Row</param>
        /// <param name="hv_LineOutCol1">平行线终点Col</param>
        /// <param name="hv_k">平行线斜率</param>
        /// <param name="hv_b">平行线截距</param>
        public void GetParallelLineFromDistance(HTuple hv_LineInRow, HTuple hv_LineInCol,
          HTuple hv_LineInRow1, HTuple hv_LineInCol1, HTuple hv_Distance, HTuple hv_Direction,
          HTuple hv_Polarity, out HTuple hv_LineOutRow, out HTuple hv_LineOutCol, out HTuple hv_LineOutRow1,
          out HTuple hv_LineOutCol1, out HTuple hv_k, out HTuple hv_b)
        {


            // Local control variables 

            HTuple hv_RealDeltaCol, hv_RealDeltaRow, hv_kIn;
            HTuple hv_bIn, hv_k1, hv_b1, hv_k2, hv_b2, hv_Sqrt1, hv_Sqrt2;
            HTuple hv_row1, hv_col1, hv_rowTemp1, hv_colTemp1, hv_row2;
            HTuple hv_col2, hv_rowTemp2, hv_colTemp2;

            // Initialize local and output iconic variables 

            hv_LineOutRow = new HTuple();
            hv_LineOutCol = new HTuple();
            hv_LineOutRow1 = new HTuple();
            hv_LineOutCol1 = new HTuple();
            HOperatorSet.TupleReal(hv_LineInCol1 - hv_LineInCol, out hv_RealDeltaCol);
            HOperatorSet.TupleReal(hv_LineInRow1 - hv_LineInRow, out hv_RealDeltaRow);

            hv_kIn = hv_RealDeltaCol / hv_RealDeltaRow;
            hv_bIn = hv_LineInCol - (hv_kIn * hv_LineInRow);
            hv_k = hv_kIn.Clone();
            hv_b = hv_bIn.Clone();


            hv_k1 = -1 / hv_kIn;
            hv_b1 = ((hv_LineInRow * hv_kIn) + hv_bIn) - (hv_k1 * hv_LineInRow);

            hv_k2 = hv_k1.Clone();
            hv_b2 = ((hv_LineInRow1 * hv_kIn) + hv_bIn) - (hv_k2 * hv_LineInRow1);

            //找出目标点
            HOperatorSet.TupleSqrt((hv_Distance * hv_Distance) / ((hv_k1 * hv_k1) + 1), out hv_Sqrt1);
            HOperatorSet.TupleSqrt((hv_Distance * hv_Distance) / ((hv_k2 * hv_k2) + 1), out hv_Sqrt2);


            hv_row1 = hv_Sqrt1 + hv_LineInRow;
            hv_col1 = (hv_k1 * hv_row1) + hv_b1;
            hv_rowTemp1 = hv_Sqrt2 + hv_LineInRow1;
            hv_colTemp1 = (hv_k2 * hv_rowTemp1) + hv_b2;


            //另一条平行线
            hv_row2 = (-hv_Sqrt1) + hv_LineInRow;
            hv_col2 = (hv_k1 * hv_row2) + hv_b1;
            hv_rowTemp2 = (-hv_Sqrt2) + hv_LineInRow1;
            hv_colTemp2 = (hv_k2 * hv_rowTemp2) + hv_b2;



            if ((int)(new HTuple(hv_Direction.TupleEqual("row"))) != 0)
            {
                if ((int)(new HTuple(hv_Polarity.TupleGreaterEqual(0))) != 0)
                {
                    hv_LineOutRow = hv_row1.Clone();
                    hv_LineOutCol = hv_col1.Clone();
                    hv_LineOutRow1 = hv_rowTemp1.Clone();
                    hv_LineOutCol1 = hv_colTemp1.Clone();

                }
                else
                {
                    hv_LineOutRow = hv_row2.Clone();
                    hv_LineOutCol = hv_col2.Clone();
                    hv_LineOutRow1 = hv_rowTemp2.Clone();
                    hv_LineOutCol1 = hv_colTemp2.Clone();
                }
            }

            if ((int)(new HTuple(hv_Direction.TupleEqual("col"))) != 0)
            {
                if ((int)(new HTuple(hv_Polarity.TupleGreaterEqual(0))) != 0)
                {
                    if ((int)(new HTuple(hv_col1.TupleGreaterEqual(hv_LineInCol))) != 0)
                    {
                        hv_LineOutCol = hv_col1.Clone();
                        hv_LineOutRow = hv_row1.Clone();
                        hv_LineOutRow1 = hv_rowTemp1.Clone();
                        hv_LineOutCol1 = hv_colTemp1.Clone();
                    }
                    else
                    {
                        hv_LineOutCol = hv_col2.Clone();
                        hv_LineOutRow = hv_row2.Clone();
                        hv_LineOutRow1 = hv_rowTemp2.Clone();
                        hv_LineOutCol1 = hv_colTemp2.Clone();
                    }
                }
                else
                {
                    if ((int)(new HTuple(hv_col1.TupleGreaterEqual(hv_LineInCol))) != 0)
                    {
                        hv_LineOutCol = hv_col2.Clone();
                        hv_LineOutRow = hv_row2.Clone();
                        hv_LineOutRow1 = hv_rowTemp2.Clone();
                        hv_LineOutCol1 = hv_colTemp2.Clone();
                    }
                    else
                    {
                        hv_LineOutCol = hv_col1.Clone();
                        hv_LineOutRow = hv_row1.Clone();
                        hv_LineOutRow1 = hv_rowTemp1.Clone();
                        hv_LineOutCol1 = hv_colTemp1.Clone();
                    }
                }
            }
            return;
        }
 
        /// <summary>
        /// 确定一个点的唯一位置，通过两条相交直线确定
        /// </summary>
        /// <param name="nCamID"></param>
        /// <param name="image"></param>
        /// <param name="rowStart"></param>
        /// <param name="colStart"></param>
        /// <param name="rowEnd"></param>
        /// <param name="colEnd"></param>
        /// <param name="rowStart1"></param>
        /// <param name="colStart1"></param>
        /// <param name="rowEnd1"></param>
        /// <param name="colEnd1"></param>
        /// <param name="GeometryType"></param>
        /// <param name="OutRegion"></param>
        /// <param name="PoseOfRegion">保存了两条垂线的交点，以及第一条直线与X轴的夹角</param>
        public void DrawGeometry(int nCamID, HObject image, HTuple rowStart, HTuple colStart, HTuple rowEnd, HTuple colEnd, HTuple rowStart1,
            HTuple colStart1, HTuple rowEnd1, HTuple colEnd1, EnumRoiType GeometryType, out HObject OutRegion, out HTuple PoseOfRegion)
        {
            var Cam = CheckCamIDAvilible(nCamID);
            HTuple WindowHandle = Cam.AttachedWindowDic[WINDOW_DEBUG];
            OutRegion = new HObject();
            PoseOfRegion = new HTuple();
            HOperatorSet.GetPart(WindowHandle, out HTuple oldRow1, out HTuple oldCol1, out HTuple oldRow2, out HTuple oldCol2);
            HTuple OldCenterRow = (oldRow1 + oldRow2) / 2;
            HTuple OldCenterCol = (oldCol1 + oldCol2) / 2;
            switch (GeometryType)
            {
                case EnumRoiType.CIRCLE:
                    {
                        HOperatorSet.DrawCircleMod(WindowHandle, OldCenterRow, OldCenterCol, 200, out HTuple row, out HTuple col, out HTuple radius);
                        HOperatorSet.GenCircle(out HObject Circle, row, col, radius);
                        HOperatorSet.GenRegionLine(out HObject region1, row - 30, col, row + 30, col);
                        HOperatorSet.GenRegionLine(out HObject region2, row, col - 30, row, col + 30);
                        HOperatorSet.Union2(Circle, OutRegion, out HObject UnionRegion);
                        HOperatorSet.Union2(region1, UnionRegion, out UnionRegion);
                        HOperatorSet.Union2(region2, UnionRegion, out UnionRegion);
                        HOperatorSet.Union2(OutRegion, UnionRegion, out UnionRegion);
                        OutRegion = UnionRegion.SelectObj(1);
                        UnionRegion.Dispose();
                        Circle.Dispose();
                        region1.Dispose();
                        region2.Dispose();
                    }
                    break;
                case EnumRoiType.LINE:
                    {
                        HOperatorSet.DrawLineMod(WindowHandle, oldRow1, oldCol1, OldCenterRow, OldCenterCol, out HTuple row1, out HTuple col1, out HTuple row2, out HTuple col2);
                        HOperatorSet.GenRegionLine(out HObject Line, row1, col1, row2, col2);
                        HOperatorSet.Union2(OutRegion, Line, out HObject UnionRegion);
                        HOperatorSet.Union2(OutRegion, UnionRegion, out UnionRegion);
                        OutRegion = UnionRegion.SelectObj(1);

                        UnionRegion.Dispose();
                        Line.Dispose();

                    }
                    break;
                case EnumRoiType.POINT:
                    {
                        HOperatorSet.DrawPointMod(WindowHandle, OldCenterRow, OldCenterCol, out HTuple row, out HTuple col);
                        HOperatorSet.GenRegionLine(out HObject region1, row - 30, col, row + 30, col);
                        HOperatorSet.GenRegionLine(out HObject region2, row, col - 30, row, col + 30);
                        HOperatorSet.Union2(region1, region2, out HObject UnionRegion);

                        HOperatorSet.Union2(OutRegion, UnionRegion, out UnionRegion);
                        OutRegion = UnionRegion.SelectObj(1);

                        UnionRegion.Dispose();
                        region1.Dispose();
                        region2.Dispose();

                    }
                    break;
                case EnumRoiType.RECTANGLE1:
                    {
                        HOperatorSet.DrawRectangle1Mod(WindowHandle, OldCenterRow, OldCenterCol, OldCenterRow + 100, OldCenterCol + 100, out HTuple row1, out HTuple col1, out HTuple row2, out HTuple col2);
                        HOperatorSet.GenRectangle1(out HObject Rectangle, row1, col1, row2, col2);

                        HOperatorSet.Union2(OutRegion, Rectangle, out HObject UnionRegion);
                        OutRegion = UnionRegion.SelectObj(1);

                        UnionRegion.Dispose();
                        Rectangle.Dispose();
                    }
                    break;
                case EnumRoiType.RECTANGLE2:
                    {
                        HOperatorSet.DrawRectangle2Mod(WindowHandle, OldCenterRow, OldCenterCol, 0, 100, 100, out HTuple row, out HTuple col, out HTuple phi, out HTuple L1, out HTuple L2);
                        HOperatorSet.GenRectangle2(out HObject Rectangle, row, col, phi, L1, L2);

                        HOperatorSet.Union2(OutRegion, Rectangle, out HObject UnionRegion);
                        OutRegion = UnionRegion.SelectObj(1);

                        UnionRegion.Dispose();
                        Rectangle.Dispose();
                    }
                    break;
                default:
                    break;
            }

            //采用极坐标表示当前region的姿态
            //与其中一条线的夹角，与两条直线交点的距离
            HOperatorSet.IntersectionLl(rowStart, colStart, rowEnd, colEnd, rowStart1, colStart1, rowEnd1, colEnd1,
                                        out HTuple rowSection, out HTuple colSection, out HTuple isParallel);
            HOperatorSet.AngleLx(rowStart, colStart, rowEnd, colEnd, out HTuple angle);

            PoseOfRegion[0] = rowSection;
            PoseOfRegion[1] = colSection;
            PoseOfRegion[2] = angle;

        }

        /// <summary>
        /// 根据之前保存的模板位姿和两条直线，恢复所画的几何图形
        /// </summary>
        /// <param name="CamID"></param>
        /// <param name="region"></param>
        /// <param name="rowStart"></param>
        /// <param name="colStart"></param>
        /// <param name="rowEnd"></param>
        /// <param name="colEnd"></param>
        /// <param name="rowStart1"></param>
        /// <param name="colStart1"></param>
        /// <param name="rowEnd1"></param>
        /// <param name="colEnd1"></param>
        /// <param name="GeometryPose">两条直线的交点，第一条直线与X轴的夹角</param>
        /// <param name="regionOut">转换之后的Region</param>
        public void GetGeometryRegionBy2Lines(HObject region, HTuple rowStart, HTuple colStart, HTuple rowEnd, HTuple colEnd, HTuple rowStart1,
            HTuple colStart1, HTuple rowEnd1, HTuple colEnd1, HTuple GeometryPose, out HObject regionOut)
        {
            regionOut = new HObject();
            //原始数据
            HTuple originRow = GeometryPose[0];
            HTuple originCol = GeometryPose[1];
            HTuple originAngle = GeometryPose[2];

            //计算新的数据
            HOperatorSet.IntersectionLl(rowStart, colStart, rowEnd, colEnd, rowStart1, colStart1, rowEnd1, colEnd1,
                                       out HTuple rowSection, out HTuple colSection, out HTuple isParallel);
            HOperatorSet.AngleLx(rowStart, colStart, rowEnd, colEnd, out HTuple angle);
            HOperatorSet.VectorAngleToRigid(originRow, originCol, originAngle, rowSection, colSection, angle, out HTuple homMat2D);
            //投影变换
            HOperatorSet.AffineTransRegion(region, out regionOut, homMat2D, "false");

        }

        public CameraInfoModel CheckCamIDAvilible(int nCamID,[CallerMemberName] string CallerName="")
        {
            if (nCamID < 0)
                throw new Exception($"Wrong CamID:{nCamID} when {CallerName}");
            var Cams = from cam in CamInfoList where cam.CamID == nCamID select cam;
            if (Cams.Count() <= 0)
                throw new Exception($"Wrong CamID:{nCamID} when {CallerName}");
            return Cams.First();
        }
        #endregion
    }

    public class VisionDataHelper
    {
        public static List<string> GetRoiListForSpecCamera(int nCamID, List<string> fileListInDataDirection)
        {
            var list = new List<string>();
            foreach (var it in fileListInDataDirection)
            {
                if (it.Contains(string.Format("Cam{0}_", nCamID)))
                    list.Add(it);
            }
            return list;
        }
        public static List<string> GetTemplateListForSpecCamera(int nCamID, List<string> fileListInDataDirection)
        {
            var list = new List<string>();
            if (nCamID >= 0)
            {
                foreach (var it in fileListInDataDirection)
                {
                    if (it.Contains(string.Format("Cam{0}_", nCamID)))
                        list.Add(it);
                }
            }
            return list;
        }

       

    }
}
