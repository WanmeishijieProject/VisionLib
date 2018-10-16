using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GalaSoft.MvvmLight.Messaging;
using HalconDotNet;
using JPT_TosaTest.UserCtrl;

namespace JPT_TosaTest.Vision
{
    public enum Enum_REGION_OPERATOR { ADD, SUB }
    public enum Enum_REGION_TYPE { RECTANGLE, CIRCLE }
    public enum EnumCamSnapState
    {
        IDLE,
        BUSY,
        DISCONNECTED

    }
    public enum EnumCamType
    {
        GigEVision,
        DirectShow,
        uEye,
        HuaRay
    }
    public enum EnumImageType
    {
        Window,
        Image
    }
    public enum EnumShapeModelType
    {
        Gray,
        Shape,
        XLD
    };
    public enum EnumRoiType
    {
        ModelRegionReduce,
    }
    public class HalconVision
    {
        #region constructor
        private HalconVision()
        {
            HOperatorSet.GenEmptyObj(out HObject emptyImage);
            for (int i = 0; i < 10; i++)
            {

                HoImageList.Add(emptyImage);
                AcqHandleList.Add(new HTuple());
                _lockList.Add(new object());
            }
            HOperatorSet.GenEmptyObj(out Region);
            //CamCfgDic = FindCamera(EnumCamType.GigEVision,new List<string>() { "Cam_Up","Cam_Down"});
        }
        private static readonly Lazy<HalconVision> _instance = new Lazy<HalconVision>(() => new HalconVision());
        public static HalconVision Instance
        {
            get { return _instance.Value; }
        }
        #endregion

        #region  Field
        public List<object> _lockList = new List<object>();
        public enum IMAGEPROCESS_STEP
        {
            T1,
            T2,
            T3,
        }
        private List<HObject> HoImageList = new List<HObject>(10);    //Image
        private List<HTuple> AcqHandleList = new List<HTuple>(10);    //Aqu
        private Dictionary<int, Dictionary<string, HTuple>> HwindowDic = new Dictionary<int, Dictionary<string, HTuple>>();    //Hwindow
        private Dictionary<int, Tuple<HTuple, HTuple>> ActiveCamDic = new Dictionary<int, Tuple<HTuple, HTuple>>();
        private Dictionary<string, Tuple<string, string>> CamCfgDic = new Dictionary<string, Tuple<string, string>>();
        private HObject Region = null;
        public Enum_REGION_OPERATOR RegionOperator = Enum_REGION_OPERATOR.ADD;
        public Enum_REGION_TYPE RegionType = Enum_REGION_TYPE.CIRCLE;
        private AutoResetEvent SyncEvent = new AutoResetEvent(false);
        private string DebugWindowName = "CameraDebug";
        #endregion

        public bool AttachCamWIndow(int nCamID, string Name, HTuple hWindow)
        {
            if (nCamID < 0)
                return false;
            lock (_lockList[nCamID])
            {

                //关联当前窗口
                if (HwindowDic.Keys.Contains(nCamID))
                {
                    var its = from hd in HwindowDic[nCamID] where hd.Key == Name select hd;
                    if (its.Count() == 0)
                        HwindowDic[nCamID].Add(Name, hWindow);
                    else
                        HwindowDic[nCamID][Name] = hWindow;
                }
                else
                    HwindowDic.Add(nCamID, new Dictionary<string, HTuple>() { { Name, hWindow } });
                if (ActiveCamDic.Keys.Contains(nCamID))
                {
                    if(HwindowDic[nCamID][Name]!=-1)
                        HOperatorSet.SetPart(HwindowDic[nCamID][Name], 0, 0, ActiveCamDic[nCamID].Item2, ActiveCamDic[nCamID].Item1);
                }


                //需要解除此窗口与其他相机的关联
                foreach (var kps in HwindowDic)
                {
                    if (kps.Key == nCamID)
                        continue;
                    foreach (var kp in kps.Value)
                    {
                        if (kp.Key == Name)
                        {
                            kps.Value.Remove(Name);
                            break;
                        }
                    }
                }
                return true;
            }

        }
        public bool DetachCamWindow(int nCamID, string Name)
        {
            if (nCamID < 0)
                return false;
            lock (_lockList[nCamID])
            {
                if (HwindowDic.Keys.Contains(nCamID))
                {
                    if (HwindowDic[nCamID].Keys.Contains(Name))
                        HwindowDic[nCamID].Remove(Name);
                }
                return true;
            }
        }
        public void GetSyncSp(out AutoResetEvent Se, out object Lock, int CamID)
        {
            Se = SyncEvent;
            Lock = _lockList[CamID];

        }

        public bool OpenCam(int nCamID)
        {
            if (nCamID < 0)
                return false;
            HObject image = null;
            HTuple hv_AcqHandle = null;
            HTuple width = 0, height = 0;
            try
            {
                lock (_lockList[nCamID])
                {
                    if (!IsCamOpen(nCamID))
                    {
                        //HOperatorSet.OpenFramegrabber("DirectShow", 1, 1, 0, 0, 0, 0, "default", 8, "rgb",
                        //                        -1, "false", "default", "Integrated Camera", 0, -1, out hv_AcqHandle);
                        HOperatorSet.OpenFramegrabber(CamCfgDic.ElementAt(nCamID).Value.Item2, 1, 1, 0, 0, 0, 0, "default", 8, "rgb",
                                                   -1, "false", "default", CamCfgDic.ElementAt(nCamID).Value.Item1, 0, -1, out hv_AcqHandle);
                        HOperatorSet.GrabImage(out image, hv_AcqHandle);
                        HOperatorSet.GetImageSize(image, out width, out height);
                        ActiveCamDic.Add(nCamID, new Tuple<HTuple, HTuple>(width, height));
                        AcqHandleList[nCamID] = hv_AcqHandle;
                    }
                    if (IsCamOpen(nCamID))
                    {
                        if (HwindowDic.Keys.Contains(nCamID))
                        {
                            foreach (var it in HwindowDic[nCamID])
                            {
                                HOperatorSet.SetPart(it.Value, 0, 0, ActiveCamDic[nCamID].Item2, ActiveCamDic[nCamID].Item1);
                                HOperatorSet.DispObj(image, it.Value);
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>($"Open Camera Error:{CamCfgDic.ElementAt(nCamID)}:{ex.Message}", "ShowError");
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
            if (nCamID < 0)
                return false;
            try
            {
                lock (_lockList[nCamID])
                {
                    if (ActiveCamDic.Keys.Contains(nCamID))
                    {
                        HOperatorSet.CloseFramegrabber(AcqHandleList[nCamID]);
                        ActiveCamDic.Remove(nCamID);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>(ex.Message, "ShowError");
                return false;
            }
        }
        public bool IsCamOpen(int nCamID)
        {
            if (nCamID < 0)
                return false;
            lock (_lockList[nCamID])
            {
                return ActiveCamDic.Keys.Contains(nCamID);
            }
        }
        public void GrabImage(int nCamID, bool bDispose = true)
        {
            if (nCamID < 0)
                return;
            HObject image = null;
            try
            {
                lock (_lockList[nCamID])
                {
                    if (!HwindowDic.Keys.Contains(nCamID))
                    {
                        Messenger.Default.Send<string>(string.Format("请先给相机{0}绑定视觉窗口", nCamID), "ShowError");
                        return;
                    }
                    if (!IsCamOpen(nCamID))
                        OpenCam(nCamID);
                    if (!IsCamOpen(nCamID))
                        return;

                    HOperatorSet.GrabImage(out image, AcqHandleList[nCamID]);
                    HOperatorSet.GetImageSize(image, out HTuple width, out HTuple height);
                  
                    HOperatorSet.GenEmptyObj(out Region);

                    if (HoImageList[nCamID] != null)
                    {
                        HoImageList[nCamID].Dispose();
                        HoImageList[nCamID] = null;
                    }
                    HoImageList[nCamID] = image.SelectObj(1);
                    if (!SyncEvent.WaitOne(10))
                    {
                        foreach (var it in HwindowDic[nCamID])
                            if (it.Value != -1)
                            {
                                HOperatorSet.SetPart(it.Value, 0, 0, height, width);
                                HOperatorSet.DispObj(HoImageList[nCamID], it.Value);
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>(ex.Message, "ShowError");
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
         /// 最终的供客户端使用的接口，处理不同的任务
         /// </summary>
         /// <param name="nStep"></param>
         /// <param name="nCamID"></param>
         /// <param name="para"></param> 形式是 ROIName & ModelName
         /// <param name="result"></param>
         /// <returns></returns>
        public bool ProcessImage(IMAGEPROCESS_STEP nStep, int nCamID, object para, out object result)
        {
            bool bRet = false;
            result = null;
            if (nCamID < 0)
                return false;
            HObject image = null;
            try
            {
                lock (_lockList[nCamID])
                {
                    double Angle = 0.0f;
                    switch (nStep)
                    {
                        case IMAGEPROCESS_STEP.T1:  //第一步 找出模板并获取第二步与第三步的ROI数据
                            bRet = FindModelAndGetData();
                            result = Angle;
                            return bRet;
                        case IMAGEPROCESS_STEP.T2:  //第二步，根据模板找出线（有好几个高度差需要切换）
                            bRet = FindLineTop();   //只需要显示
                            break;
                        case IMAGEPROCESS_STEP.T3:  //第三步，备用
                            bRet = FindLineBottom();    //只需要显示
                            break;
                        default:
                            break;
                    }
                    return true;
                }
            }
            catch
            {
                result = null;
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
                throw;
            }

        }

        public bool DrawRoi(int nCamID, EnumRoiType type, out object outRegion, string fileName = null)
        {
            outRegion = null;
            if (nCamID < 0)
                return false;
            try
            {
                switch (type)
                {
                    case EnumRoiType.ModelRegionReduce:
                        if (fileName != null)   //如果是现有的Model
                        {
                            string[] strList = fileName.Split('.');
                            if (strList.Length == 2)
                            {
                                if (HwindowDic.Keys.Contains(nCamID) && HwindowDic[nCamID].Keys.Contains(DebugWindowName))
                                {
                                    HTuple window = HwindowDic[nCamID][DebugWindowName];
                                    HTuple row, column, phi, length1, length2, radius;
                                    HObject newRegion = null;
                                    HOperatorSet.SetColor(window, "green");

                                    if (File.Exists($"{strList[0]}.reg"))
                                        HOperatorSet.ReadRegion(out Region, $"{strList[0]}.reg");   //如果已经有了就先都出来
                                    switch (RegionType)
                                    {
                                        case Enum_REGION_TYPE.RECTANGLE:
                                            HOperatorSet.DrawRectangle2Mod(window, 100, 100, 100, 100, 100, out row, out column, out phi, out length1, out length2);
                                            HOperatorSet.GenRectangle2(out newRegion, row, column, phi, length1, length2);
                                            break;
                                        case Enum_REGION_TYPE.CIRCLE:
                                            HOperatorSet.DrawCircleMod(window, 100, 100, 100, out row, out column, out radius);
                                            HOperatorSet.GenCircle(out newRegion, row, column, radius);
                                            break;
                                        default:
                                            break;
                                    }

                                    if (RegionOperator == Enum_REGION_OPERATOR.ADD)
                                    {
                                        HOperatorSet.Union2(Region, newRegion, out Region);
                                    }
                                    else
                                    {
                                        HOperatorSet.Difference(Region, newRegion, out Region);
                                    }
                                    HOperatorSet.SetDraw(window, "margin");
                                    HOperatorSet.SetColor(window, "red");
                                    HOperatorSet.ClearWindow(window);
                                    HOperatorSet.DispObj(HoImageList[nCamID], window);
                                    HOperatorSet.DispObj(Region, window);

                                    //存储
                                    HOperatorSet.WriteRegion(Region, $"{strList[0]}.reg");
                                    outRegion = Region;
                                    return true;
                                }
                                Messenger.Default.Send<String>("绘制模板窗口没有打开,或者该相机未关联绘制模板窗口", "ShowError");
                            }
                        }
                        else  //如果是新建的Model
                        {
                            if (HwindowDic.Keys.Contains(nCamID) && HwindowDic[nCamID].Keys.Contains(DebugWindowName))
                            {
                                HTuple window = HwindowDic[nCamID][DebugWindowName];
                                HTuple row, column, phi, length1, length2, radius;
                                HObject newRegion = null;
                                HOperatorSet.SetColor(window, "green");
                                switch (RegionType)
                                {
                                    case Enum_REGION_TYPE.RECTANGLE:
                                        HOperatorSet.DrawRectangle2Mod(window, 100, 100, 100, 100, 100, out row, out column, out phi, out length1, out length2);
                                        HOperatorSet.GenRectangle2(out newRegion, row, column, phi, length1, length2);
                                        break;
                                    case Enum_REGION_TYPE.CIRCLE:
                                        HOperatorSet.DrawCircleMod(window, 100, 100, 100, out row, out column, out radius);
                                        HOperatorSet.GenCircle(out newRegion, row, column, radius);
                                        break;
                                    default:
                                        break;
                                }

                                if (RegionOperator == Enum_REGION_OPERATOR.ADD)
                                {
                                    HOperatorSet.Union2(Region, newRegion, out Region);
                                }
                                else
                                {
                                    HOperatorSet.Difference(Region, newRegion, out Region);
                                }
                                HOperatorSet.SetDraw(window, "margin");
                                HOperatorSet.SetColor(window, "red");
                                HOperatorSet.ClearWindow(window);
                                HOperatorSet.DispObj(HoImageList[nCamID], window);
                                HOperatorSet.DispObj(Region, window);
                                outRegion = Region;
                            }
                        }

                        break;
                }
                return false;
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<String>(string.Format("DrawRoi出错:{0}", ex.Message), "ShowError");
                return false;
            }
        }
        public bool NewRoi(int nCamID, string strRoiName)     //就在建立找边ROI的时候有用到
        {
            if (nCamID < 0)
                return false;
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];
            long SP_O = 0;

            // Local iconic variables 

            HObject ho_Rectangle, ho_Rectangle1, ho_EmptyRegion;

            // Local control variables 

            HTuple hv_Row, hv_Column;
            HTuple hv_Phi, hv_Length1, hv_Length2, hv_Row1, hv_Column1;
            HTuple hv_Phi1, hv_Length11, hv_Length21, hv_data;

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_EmptyRegion);

            try
            {
                if (HwindowDic.Keys.Contains(nCamID) && HwindowDic[nCamID].Keys.Contains(DebugWindowName))
                {
                    HTuple window = HwindowDic[nCamID][DebugWindowName];
                    HOperatorSet.SetWindowAttr("background_color", "black");
                    HOperatorSet.SetDraw(window, "margin");
                    disp_message(window, "请绘制第一个ROI，以右键结束绘制", "window",
                        12, 12, "black", "true");
                    HOperatorSet.DrawRectangle2Mod(window, 100, 100, 100, 100, 100, out hv_Row,
                        out hv_Column, out hv_Phi, out hv_Length1, out hv_Length2);
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle2(out ho_Rectangle, hv_Row, hv_Column, hv_Phi, hv_Length1,
                        hv_Length2);
                    HOperatorSet.DispObj(ho_Rectangle, window);
                    disp_message(window, "请绘制第二个ROI，以右键结束绘制", "window", 12, 12, "black", "true");
                    HOperatorSet.DrawRectangle2Mod(window, 100, 100, 100, 100, 100, out hv_Row1, out hv_Column1, out hv_Phi1, out hv_Length11, out hv_Length21);
                    ho_Rectangle1.Dispose();
                    HOperatorSet.GenRectangle2(out ho_Rectangle1, hv_Row1, hv_Column1, hv_Phi1, hv_Length11, hv_Length21);
                    HOperatorSet.DispObj(ho_Rectangle1, window);
                    hv_data = new HTuple();
                    hv_data[0] = hv_Row;
                    hv_data[1] = hv_Column;
                    hv_data[2] = hv_Phi;
                    hv_data[3] = hv_Length1;
                    hv_data[4] = hv_Length2;
                    //第二个矩形的参数
                    hv_data[5] = hv_Row1;
                    hv_data[6] = hv_Column1;
                    hv_data[7] = hv_Phi1;
                    hv_data[8] = hv_Length11;
                    hv_data[9] = hv_Length21;
                    ho_EmptyRegion.Dispose();
                    HOperatorSet.GenEmptyRegion(out ho_EmptyRegion);
                    OTemp[SP_O] = ho_EmptyRegion.CopyObj(1, -1);
                    SP_O++;
                    ho_EmptyRegion.Dispose();
                    HOperatorSet.ConcatObj(OTemp[SP_O - 1], ho_Rectangle, out ho_EmptyRegion);
                    OTemp[SP_O - 1].Dispose();
                    SP_O = 0;
                    OTemp[SP_O] = ho_EmptyRegion.CopyObj(1, -1);
                    SP_O++;
                    ho_EmptyRegion.Dispose();
                    HOperatorSet.ConcatObj(OTemp[SP_O - 1], ho_Rectangle1, out ho_EmptyRegion);
                    OTemp[SP_O - 1].Dispose();
                    SP_O = 0;

                    //创建实际的Roi文件
                    HOperatorSet.WriteTuple(hv_data, $"{strRoiName}.tup");
                    HOperatorSet.WriteRegion(ho_EmptyRegion, $"{strRoiName}.reg");


                    ho_Rectangle.Dispose();
                    ho_Rectangle1.Dispose();
                    ho_EmptyRegion.Dispose();
                    return true;
                }
                return false;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Rectangle.Dispose();
                ho_Rectangle1.Dispose();
                ho_EmptyRegion.Dispose();
                throw new Exception($"创建Roi时候出现错误:{HDevExpDefaultException.Message}");

            }



        }

        public bool ShowRoi(string RoiFilePathName)     //显示ROI
        {
            string[] splitString = RoiFilePathName.Split('\\');
            if (splitString.Length > 2)
            {
                int nCamID = Convert.ToInt16(splitString[splitString.Length - 1].Substring(3, 1));
                HOperatorSet.ReadRegion(out HObject region, RoiFilePathName);
                foreach (var it in HwindowDic[nCamID])
                {
                    if (it.Value != -1)
                    {
                        //HOperatorSet.ClearWindow(it.Value);
                        //if (HoImageList[nCamID] != null)
                        //HOperatorSet.DispObj(HoImageList[nCamID], it.Value);
                        HOperatorSet.SetDraw(it.Value, "margin");
                        HOperatorSet.SetColor(it.Value, "green");
                        HOperatorSet.DispObj(region, it.Value);
                    }
                }
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

                //三个文件同时读取
                splitString = ModelFilePathName.Split('.');
                HOperatorSet.ReadShapeModel(ModelFilePathName, out HTuple ModelID);
                HOperatorSet.ReadRegion(out HObject ModelRoiRegion, $"{splitString[0]}.reg");
                HOperatorSet.ReadTuple($"{splitString[0]}.tup", out HTuple ModelOriginPos);

                foreach (var it in HwindowDic[nCamID])
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
        public bool SaveImage(int nCamID, EnumImageType type, string filePath, string fileName, HTuple hWindow)
        {
            try
            {
                if (nCamID < 0)
                    return false;
                if (!Directory.Exists(filePath))
                    return false;
                switch (type)
                {
                    case EnumImageType.Image:
                        HOperatorSet.WriteImage(HoImageList[nCamID], "jpeg", 0, $"{filePath}\\{fileName}");
                        break;
                    case EnumImageType.Window:
                        HOperatorSet.DumpWindow(hWindow, "jpeg", $"{filePath}\\{fileName}");
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                UC_MessageBox.ShowMsgBox($"{ex.Message}","错误", MsgType.Error);
                return false;
            }
        }

        public bool OpenImageInWindow(int nCamID, string imageFilePath, HTuple hwindow)
        {
            try
            {
                HOperatorSet.ReadImage(out HObject image, imageFilePath);
                if (nCamID >= 0)
                {
                    if (HoImageList[nCamID] != null)
                    {
                        HoImageList[nCamID].Dispose();
                        HoImageList[nCamID] = null;
                    }
                    HoImageList[nCamID] = image.SelectObj(1);
                }
                HOperatorSet.GetImageSize(image, out HTuple width, out HTuple height);
                HOperatorSet.SetPart(hwindow, 0, 0, height, width);
                HOperatorSet.DispObj(image, hwindow);
                image.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                UC_MessageBox.ShowMsgBox($"{ex.Message}","错误",MsgType.Error);
                return false;
            }
        }
        public bool CloseCamera()
        {
            HOperatorSet.CloseAllFramegrabbers();
            return true;       
        }
        public Dictionary<string, Tuple<string, string>> FindCamera(EnumCamType camType, List<string> acturalNameList,out List<string> ErrorList)
        {
            Dictionary<string, Tuple<string, string>> dic = new Dictionary<string, Tuple<string, string>>();
            ErrorList = new List<string>();
            try
            {
                HOperatorSet.InfoFramegrabber(camType.ToString(), "info_boards", out HTuple hv_Information, out HTuple hv_ValueList);
                if (0 == hv_ValueList.Length)
                    return dic;
                for (int i = 0; i < acturalNameList.Count; i++)
                {
                    bool bFind = false;
                    foreach (var dev in hv_ValueList.SArr)
                    {
                        var listAttr = dev.Split('|').Where(a=>a.Contains("device:"));
                        if (listAttr != null && listAttr.Count() > 0)
                        {
                            string Name = listAttr.First().Trim().Replace("device:","");
                            if (Name.Contains(acturalNameList[i]))
                            {
                                dic.Add(acturalNameList[i], new Tuple<string, string>(Name.Trim(), camType.ToString()));
                                bFind = true;
                                break;
                            }
                        }
                    }
                    if (!bFind)
                        ErrorList.Add($"相机:{ acturalNameList[i]}未找到硬件，请检查硬件连接或者配置");
                }
                CamCfgDic = dic;
                return dic;
            }
            catch (Exception ex)
            {
                ErrorList.Add($"FIndCamera error:{ex.Message}");
                return dic;
            }

        }

        HTuple s_hv_ModelID = null;

        #region 专用
        public bool GetAngleTune1(HObject imageIn, string ModelFileName, string RectParaFileName, out double fAngle, HTuple hwindow = null)
        {
            fAngle = 0;
            try
            {
                string[] strRoiListDot = RectParaFileName.Split('.');
                if (strRoiListDot.Length < 2)
                    return false;

                string[] strRoiListIta = RectParaFileName.Split('\\');
                if (strRoiListIta.Length < 2)
                    return false;

                string[] strModelListDot = ModelFileName.Split('.');
                if (strRoiListDot.Length < 2)
                    return false;

                string[] strModelListIta = ModelFileName.Split('\\');
                if (strRoiListIta.Length < 2)
                    return false;

                int nCamID = Convert.ToInt16(strRoiListIta[strRoiListIta.Length - 1].Substring(3, 1));
                // Local iconic variables
                HObject ho_ImageScaled = null, ho_ImageMean = null;
                // Local control variables 
                HTuple hv_EdgeGrayValue = new HTuple();
                HTuple hv_nSegment = new HTuple(), hv_Width = new HTuple();
                HTuple hv_Height = new HTuple();
                HTuple hv_Row = new HTuple(), hv_Column = new HTuple(), hv_Phi = new HTuple();
                HTuple hv_Length1 = new HTuple(), hv_Length2 = new HTuple();
                HTuple hv_RectanglePara = new HTuple();
                HTuple hv_ModelPos = new HTuple(), hv_ModelID = new HTuple();
                HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple(), hv_Angle = new HTuple();
                HTuple hv_Score = new HTuple(), hv_HomMat2D1 = new HTuple();
                HTuple hv_QRow = new HTuple(), hv_QColumn = new HTuple(), hv_Row2 = new HTuple();
                HTuple hv_Column2 = new HTuple(), hv_Phi2 = new HTuple(), hv_QRow2 = new HTuple();
                HTuple hv_QColumn2 = new HTuple(), hv_OutRowStart = new HTuple();
                HTuple hv_OutColStart = new HTuple(), hv_OutRowEnd = new HTuple();
                HTuple hv_OutColEnd = new HTuple(), hv_OutRowStart1 = new HTuple();
                HTuple hv_OutColStart1 = new HTuple(), hv_OutRowEnd1 = new HTuple();
                HTuple hv_OutColEnd1 = new HTuple();

                // Initialize local and output iconic variables 
                HOperatorSet.GenEmptyObj(out ho_ImageScaled);
                HOperatorSet.GenEmptyObj(out ho_ImageMean);

                HObject image = imageIn.SelectObj(1);
                HOperatorSet.GetImageSize(image, out HTuple imageWidth, out HTuple imageHeight);
                hv_EdgeGrayValue = 8;
                hv_nSegment = 20;
                ho_ImageScaled.Dispose();
                scale_image_range(image, out ho_ImageScaled, 100, 200);
                HOperatorSet.GetImageSize(image, out hv_Width, out hv_Height);
                ho_ImageMean.Dispose();
                HOperatorSet.MeanImage(image, out ho_ImageMean, 200, 200);
                HOperatorSet.Threshold(ho_ImageMean, out HObject RegionThreshold, 100, 255);
                HOperatorSet.FillUp(RegionThreshold, out HObject RegionFilledUp);
                HOperatorSet.Connection(RegionFilledUp, out HObject RegionConnected);
                HOperatorSet.SelectShapeStd(RegionConnected, out HObject RegionSelected, "max_area", 70);
                HOperatorSet.DilationCircle(RegionSelected, out HObject RegionDilationed, 200);
                HOperatorSet.ReduceDomain(image, RegionDilationed, out HObject imageReduced);

                HOperatorSet.GetImageSize(ho_ImageMean, out hv_Width, out hv_Height);
                HOperatorSet.ReadTuple(RectParaFileName, out hv_RectanglePara);
                //如果指定了窗口，那么就显示出来
                if (hwindow == null)
                {
                    if (HwindowDic.Keys.Contains(nCamID))
                    {
                        foreach (var it in HwindowDic[nCamID])
                        {

                            HOperatorSet.ClearWindow(it.Value);
                            HOperatorSet.DispObj(image, it.Value);
                            HOperatorSet.SetDraw(it.Value, "margin");
                            HOperatorSet.SetColor(it.Value, "red");
                            HOperatorSet.SetLineWidth(it.Value, 3);
                        }
                    }
                    else
                        throw new Exception("没有找到合适的窗口执行操作");
                }
    
                //读取ROI的region区域
                HOperatorSet.ReadRegion(out HObject RectRegion, $"{strRoiListDot[0]}.reg");


                //读取矩形ROI—1的值
                hv_Row = hv_RectanglePara[0];
                hv_Column = hv_RectanglePara[1];
                hv_Phi = hv_RectanglePara[2];
                hv_Length1 = hv_RectanglePara[3];
                hv_Length2 = hv_RectanglePara[4];
                //图像预处理


                //读取模板与它的起始位置
                HOperatorSet.ReadTuple($"{strModelListDot[0]}.tup", out hv_ModelPos);
                //  HOperatorSet.ReadShapeModel($"{strModelListDot[0]}.shm", out hv_ModelID);
                if(s_hv_ModelID==null)
                    HOperatorSet.ReadShapeModel($"{strModelListDot[0]}.shm", out s_hv_ModelID);//读一次

                HOperatorSet.FindShapeModel(imageReduced, 
                                             s_hv_ModelID, 
                                             (new HTuple(0)).TupleRad(), 
                                             (new HTuple(90)).TupleRad(), 0.5, 1, 0.5, 
                                             "least_squares", 0, 0.9,
                                             out hv_Row1, out hv_Column1, out hv_Angle, out hv_Score);
               
                if (hv_Row1.Length == 0)
                {
                    foreach (var it in HwindowDic[nCamID])
                    {
                        disp_message(it.Value, "查找模板失败", "window", 10, 10, "red", "true");
                    }
                    throw new Exception("查找模板失败");
                }
                else
                {
                    foreach (var it in HwindowDic[nCamID])
                    {
                        disp_message(it.Value, $"模板位置:({ hv_Column1},{ hv_Row1})", "window", 10, 10, "red", "true");
                        disp_message(it.Value, $"分数: { hv_Score}", "window", 50, 10, "red", "true");
                        HOperatorSet.DispCross(it.Value, hv_Row1, hv_Column1, 60, hv_Angle);
                    }
                }
                //HOperatorSet.ClearShapeModel(s_hv_ModelID);

                //模板偏移
                HOperatorSet.VectorAngleToRigid(hv_ModelPos.TupleSelect(0), hv_ModelPos.TupleSelect(1), 0, hv_Row1, hv_Column1, hv_Angle, out hv_HomMat2D1);
                HOperatorSet.AffineTransPoint2d(hv_HomMat2D1, hv_Row, hv_Column, out hv_QRow, out hv_QColumn);
                //Region偏移
                HOperatorSet.AffineTransRegion(RectRegion, out HObject regionTrans, hv_HomMat2D1, "false");


                hv_Row = hv_QRow.Clone();
                hv_Column = hv_QColumn.Clone();
                hv_Phi = hv_Phi + hv_Angle;

                //*
                //读取矩形ROI—2的值
                hv_Row2 = hv_RectanglePara[5];
                hv_Column2 = hv_RectanglePara[6];
                hv_Phi2 = hv_RectanglePara[7];
                HTuple hv_Length21 = hv_RectanglePara[8];
                HTuple hv_Length22 = hv_RectanglePara[9];

                HOperatorSet.AffineTransPoint2d(hv_HomMat2D1, hv_Row2, hv_Column2, out hv_QRow2, out hv_QColumn2);
                hv_Phi2 = hv_Phi2 + hv_Angle;
                hv_Row2 = hv_QRow2.Clone();
                hv_Column2 = hv_QColumn2.Clone();

                FindLine(ho_ImageScaled, hv_nSegment, hv_EdgeGrayValue, hv_Row, hv_Column, hv_Phi, hv_Length1, hv_Length2, out hv_OutRowStart, out hv_OutColStart, out hv_OutRowEnd, out hv_OutColEnd);
                FindLine(ho_ImageScaled, hv_nSegment, hv_EdgeGrayValue, hv_Row2, hv_Column2, hv_Phi2, hv_Length21, hv_Length22, out hv_OutRowStart1, out hv_OutColStart1, out hv_OutRowEnd1, out hv_OutColEnd1);

                foreach (var it in HwindowDic[nCamID])
                {
                    HOperatorSet.SetPart(it.Value, 0, 0, imageHeight, imageWidth);
                    HOperatorSet.SetColor(it.Value, "green");
                    HOperatorSet.SetLineWidth(it.Value, 3);
                    HOperatorSet.SetDraw(it.Value, "margin");
                    HOperatorSet.DispObj(regionTrans, it.Value);
                    HOperatorSet.SetColor(it.Value, "red");
                    HOperatorSet.SetLineWidth(it.Value, 3);
                    HOperatorSet.DispLine(it.Value, hv_OutRowStart, hv_OutColStart, hv_OutRowEnd, hv_OutColEnd);
                    HOperatorSet.DispLine(it.Value, hv_OutRowStart1, hv_OutColStart1, hv_OutRowEnd1, hv_OutColEnd1);
                }
               
                HOperatorSet.AngleLx(hv_OutRowStart, hv_OutColStart, hv_OutRowEnd, hv_OutColEnd, out HTuple hv_Angle1);
                HOperatorSet.AngleLx(hv_OutRowStart1, hv_OutColStart1, hv_OutRowEnd1, hv_OutColEnd1, out HTuple hv_Angle2);
                fAngle = (hv_Angle1 + hv_Angle2) / 2;
                HTuple hv_StratRow = hv_OutRowEnd.Clone();
                HTuple hv_StartCol = hv_OutColEnd.Clone();
                if ((int)(new HTuple(hv_Angle1.TupleLess(0))) != 0)
                {
                    hv_Angle1 = hv_Angle1 + 3.14159;
                    hv_StratRow = hv_OutRowStart.Clone();
                    hv_StartCol = hv_OutColStart.Clone();
                }

                

                HTuple hv_CenterRow = (hv_OutRowStart + hv_OutRowEnd) / 2;
                HTuple hv_CenterCol = (hv_OutColStart + hv_OutColEnd) / 2;

                foreach (var it in HwindowDic[nCamID])
                {
                    HOperatorSet.SetColor(it.Value, "red");
                    HOperatorSet.DispArc(it.Value, hv_CenterRow, hv_CenterCol, hv_Angle1, hv_StratRow, hv_StartCol);
                    HOperatorSet.DispLine(it.Value, hv_CenterRow, hv_CenterCol, hv_CenterRow, hv_CenterCol + 500);
                    disp_message(it.Value, ((hv_Angle1 * 180) / 3.14159) + "度", "image", hv_OutRowEnd + 50, hv_OutColEnd, "red", "true");
                }


                HTuple hv_StratRow1 = hv_OutRowEnd1.Clone();
                HTuple hv_StartCol1 = hv_OutColEnd1.Clone();
                if ((int)(new HTuple(hv_Angle2.TupleLess(0))) != 0)
                {
                    hv_Angle2 = hv_Angle2 + 3.14159;
                    hv_StratRow1 = hv_OutRowStart1.Clone();
                    hv_StartCol1 = hv_OutColStart1.Clone();
                }
                HTuple hv_CenterRow1 = (hv_OutRowStart1 + hv_OutRowEnd1) / 2;
                HTuple hv_CenterCol1 = (hv_OutColStart1 + hv_OutColEnd1) / 2;

                foreach (var it in HwindowDic[nCamID])
                {
                    HOperatorSet.DispArc(it.Value, (hv_OutRowStart1 + hv_OutRowEnd1) / 2, (hv_OutColStart1 + hv_OutColEnd1) / 2, hv_Angle2, hv_StratRow1, hv_StartCol1);
                    HOperatorSet.DispLine(it.Value, hv_CenterRow1, hv_CenterCol1, hv_CenterRow1, hv_CenterCol1 + 500);
                    disp_message(it.Value, ((hv_Angle2 * 180) / 3.14159) + "度", "image", hv_OutRowEnd1 + 50, hv_OutColEnd1, "red", "true");
                }

                image.Dispose();
                ho_ImageScaled.Dispose();
                ho_ImageMean.Dispose();
                regionTrans.Dispose();
                RegionThreshold.Dispose();
                RegionFilledUp.Dispose();
                RegionConnected.Dispose();
                RegionSelected.Dispose();
                RegionDilationed.Dispose();
                imageReduced.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"处理图片是发生错误{ex.Message}");
            }
        }
        public bool GetAngleTune1(int nCamID, string ModelFileName, string RectParaFileName, out double fAngle, HTuple hwindow = null)
        {

            fAngle = 0;
            try
            {
                string[] strRoiListDot = RectParaFileName.Split('.');
                if (strRoiListDot.Length < 2)
                    return false;

                string[] strRoiListIta = RectParaFileName.Split('\\');
                if (strRoiListIta.Length < 2)
                    return false;

                string[] strModelListDot = ModelFileName.Split('.');
                if (strRoiListDot.Length < 2)
                    return false;

                string[] strModelListIta = ModelFileName.Split('\\');
                if (strRoiListIta.Length < 2)
                    return false;

                // Local iconic variables
                HObject ho_ImageScaled = null, ho_ImageMean = null;
                // Local control variables 
                HTuple hv_EdgeGrayValue = new HTuple();
                HTuple hv_nSegment = new HTuple(), hv_Width = new HTuple();
                HTuple hv_Height = new HTuple();
                HTuple hv_Row = new HTuple(), hv_Column = new HTuple(), hv_Phi = new HTuple();
                HTuple hv_Length1 = new HTuple(), hv_Length2 = new HTuple();
                HTuple hv_RectanglePara = new HTuple();
                HTuple hv_ModelPos = new HTuple(), hv_ModelID = new HTuple();
                HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple(), hv_Angle = new HTuple();
                HTuple hv_Score = new HTuple(), hv_HomMat2D1 = new HTuple();
                HTuple hv_QRow = new HTuple(), hv_QColumn = new HTuple(), hv_Row2 = new HTuple();
                HTuple hv_Column2 = new HTuple(), hv_Phi2 = new HTuple(), hv_QRow2 = new HTuple();
                HTuple hv_QColumn2 = new HTuple(), hv_OutRowStart = new HTuple();
                HTuple hv_OutColStart = new HTuple(), hv_OutRowEnd = new HTuple();
                HTuple hv_OutColEnd = new HTuple(), hv_OutRowStart1 = new HTuple();
                HTuple hv_OutColStart1 = new HTuple(), hv_OutRowEnd1 = new HTuple();
                HTuple hv_OutColEnd1 = new HTuple();

                // Initialize local and output iconic variables 
                HOperatorSet.GenEmptyObj(out ho_ImageScaled);
                HOperatorSet.GenEmptyObj(out ho_ImageMean);

                HObject image = HoImageList[nCamID].SelectObj(1);
                HOperatorSet.GetImageSize(image, out HTuple imageWidth, out HTuple imageHeight);
                hv_EdgeGrayValue = 8;
                hv_nSegment = 20;
                ho_ImageScaled.Dispose();
                scale_image_range(image, out ho_ImageScaled, 100, 200);
                HOperatorSet.GetImageSize(image, out hv_Width, out hv_Height);
                ho_ImageMean.Dispose();
                HOperatorSet.MeanImage(image, out ho_ImageMean, 200, 200);
                HOperatorSet.Threshold(ho_ImageMean, out HObject RegionThreshold, 100, 255);
                HOperatorSet.FillUp(RegionThreshold, out HObject RegionFilledUp);
                HOperatorSet.Connection(RegionFilledUp, out HObject RegionConnected);
                HOperatorSet.SelectShapeStd(RegionConnected, out HObject RegionSelected, "max_area", 70);
                HOperatorSet.DilationCircle(RegionSelected, out HObject RegionDilationed, 200);
                HOperatorSet.ReduceDomain(image, RegionDilationed, out HObject imageReduced);

                HOperatorSet.GetImageSize(ho_ImageMean, out hv_Width, out hv_Height);
                HOperatorSet.ReadTuple(RectParaFileName, out hv_RectanglePara);
                //如果指定了窗口，那么就显示出来
                if (hwindow == null)
                {
                    if (HwindowDic.Keys.Contains(nCamID))
                    {
                        foreach (var it in HwindowDic[nCamID])
                        {

                            HOperatorSet.ClearWindow(it.Value);
                            HOperatorSet.DispObj(image, it.Value);
                            HOperatorSet.SetDraw(it.Value, "margin");
                            HOperatorSet.SetColor(it.Value, "red");
                            HOperatorSet.SetLineWidth(it.Value, 3);
                        }
                    }
                    else
                        throw new Exception("没有找到合适的窗口执行操作");
                }

                //读取ROI的region区域
                HOperatorSet.ReadRegion(out HObject RectRegion, $"{strRoiListDot[0]}.reg");


                //读取矩形ROI—1的值
                hv_Row = hv_RectanglePara[0];
                hv_Column = hv_RectanglePara[1];
                hv_Phi = hv_RectanglePara[2];
                hv_Length1 = hv_RectanglePara[3];
                hv_Length2 = hv_RectanglePara[4];
                //图像预处理


                //读取模板与它的起始位置
                HOperatorSet.ReadTuple($"{strModelListDot[0]}.tup", out hv_ModelPos);
                HOperatorSet.ReadShapeModel($"{strModelListDot[0]}.shm", out hv_ModelID);

                HOperatorSet.FindShapeModel(imageReduced, hv_ModelID, (new HTuple(0)).TupleRad(), (new HTuple(360)).TupleRad(), 0.5, 1, 0.5, "least_squares", 0, 0.9, out hv_Row1, out hv_Column1, out hv_Angle, out hv_Score);
                if (hv_Row1.Length == 0)
                {
                    foreach (var it in HwindowDic[nCamID])
                    {
                        disp_message(it.Value, "查找模板失败", "window", 10, 10, "red", "true");
                    }
                    throw new Exception("查找模板失败");
                }
                else
                {
                    foreach (var it in HwindowDic[nCamID])
                    {
                        disp_message(it.Value, $"模板位置:({ hv_Column1},{ hv_Row1})", "window", 10, 10, "red", "true");
                        disp_message(it.Value, $"分数: { hv_Score}", "window", 50, 10, "red", "true");
                        HOperatorSet.DispCross(it.Value, hv_Row1, hv_Column1, 60, hv_Angle);
                    }
                }

                //模板偏移
                HOperatorSet.VectorAngleToRigid(hv_ModelPos.TupleSelect(0), hv_ModelPos.TupleSelect(1), 0, hv_Row1, hv_Column1, hv_Angle, out hv_HomMat2D1);
                HOperatorSet.AffineTransPoint2d(hv_HomMat2D1, hv_Row, hv_Column, out hv_QRow, out hv_QColumn);
                //Region偏移
                HOperatorSet.AffineTransRegion(RectRegion, out HObject regionTrans, hv_HomMat2D1, "false");


                hv_Row = hv_QRow.Clone();
                hv_Column = hv_QColumn.Clone();
                hv_Phi = hv_Phi + hv_Angle;

                //*
                //读取矩形ROI—2的值
                hv_Row2 = hv_RectanglePara[5];
                hv_Column2 = hv_RectanglePara[6];
                hv_Phi2 = hv_RectanglePara[7];
                HTuple hv_Length21 = hv_RectanglePara[8];
                HTuple hv_Length22 = hv_RectanglePara[9];

                HOperatorSet.AffineTransPoint2d(hv_HomMat2D1, hv_Row2, hv_Column2, out hv_QRow2, out hv_QColumn2);
                hv_Phi2 = hv_Phi2 + hv_Angle;
                hv_Row2 = hv_QRow2.Clone();
                hv_Column2 = hv_QColumn2.Clone();

                FindLine(ho_ImageScaled, hv_nSegment, hv_EdgeGrayValue, hv_Row, hv_Column, hv_Phi, hv_Length1, hv_Length2, out hv_OutRowStart, out hv_OutColStart, out hv_OutRowEnd, out hv_OutColEnd);
                FindLine(ho_ImageScaled, hv_nSegment, hv_EdgeGrayValue, hv_Row2, hv_Column2, hv_Phi2, hv_Length21, hv_Length22, out hv_OutRowStart1, out hv_OutColStart1, out hv_OutRowEnd1, out hv_OutColEnd1);

                foreach (var it in HwindowDic[nCamID])
                {
                    HOperatorSet.SetPart(it.Value, 0, 0, imageHeight, imageWidth);
                    HOperatorSet.SetColor(it.Value, "green");
                    HOperatorSet.SetLineWidth(it.Value, 3);
                    HOperatorSet.SetDraw(it.Value, "margin");
                    HOperatorSet.DispObj(regionTrans, it.Value);
                    HOperatorSet.SetColor(it.Value, "red");
                    HOperatorSet.SetLineWidth(it.Value, 3);
                    HOperatorSet.DispLine(it.Value, hv_OutRowStart, hv_OutColStart, hv_OutRowEnd, hv_OutColEnd);
                    HOperatorSet.DispLine(it.Value, hv_OutRowStart1, hv_OutColStart1, hv_OutRowEnd1, hv_OutColEnd1);
                }

                HOperatorSet.AngleLx(hv_OutRowStart, hv_OutColStart, hv_OutRowEnd, hv_OutColEnd, out HTuple hv_Angle1);
                HOperatorSet.AngleLx(hv_OutRowStart1, hv_OutColStart1, hv_OutRowEnd1, hv_OutColEnd1, out HTuple hv_Angle2);
                fAngle = (hv_Angle1 + hv_Angle2) / 2;
                HTuple hv_StratRow = hv_OutRowEnd.Clone();
                HTuple hv_StartCol = hv_OutColEnd.Clone();
                if ((int)(new HTuple(hv_Angle1.TupleLess(0))) != 0)
                {
                    hv_Angle1 = hv_Angle1 + 3.14159;
                    hv_StratRow = hv_OutRowStart.Clone();
                    hv_StartCol = hv_OutColStart.Clone();
                }



                HTuple hv_CenterRow = (hv_OutRowStart + hv_OutRowEnd) / 2;
                HTuple hv_CenterCol = (hv_OutColStart + hv_OutColEnd) / 2;

                foreach (var it in HwindowDic[nCamID])
                {
                    HOperatorSet.SetColor(it.Value, "red");
                    HOperatorSet.DispArc(it.Value, hv_CenterRow, hv_CenterCol, hv_Angle1, hv_StratRow, hv_StartCol);
                    HOperatorSet.DispLine(it.Value, hv_CenterRow, hv_CenterCol, hv_CenterRow, hv_CenterCol + 500);
                    disp_message(it.Value, ((hv_Angle1 * 180) / 3.14159) + "度", "image", hv_OutRowEnd + 50, hv_OutColEnd, "red", "true");
                }


                HTuple hv_StratRow1 = hv_OutRowEnd1.Clone();
                HTuple hv_StartCol1 = hv_OutColEnd1.Clone();
                if ((int)(new HTuple(hv_Angle2.TupleLess(0))) != 0)
                {
                    hv_Angle2 = hv_Angle2 + 3.14159;
                    hv_StratRow1 = hv_OutRowStart1.Clone();
                    hv_StartCol1 = hv_OutColStart1.Clone();
                }
                HTuple hv_CenterRow1 = (hv_OutRowStart1 + hv_OutRowEnd1) / 2;
                HTuple hv_CenterCol1 = (hv_OutColStart1 + hv_OutColEnd1) / 2;

                foreach (var it in HwindowDic[nCamID])
                {
                    HOperatorSet.DispArc(it.Value, (hv_OutRowStart1 + hv_OutRowEnd1) / 2, (hv_OutColStart1 + hv_OutColEnd1) / 2, hv_Angle2, hv_StratRow1, hv_StartCol1);
                    HOperatorSet.DispLine(it.Value, hv_CenterRow1, hv_CenterCol1, hv_CenterRow1, hv_CenterCol1 + 500);
                    disp_message(it.Value, ((hv_Angle2 * 180) / 3.14159) + "度", "image", hv_OutRowEnd1 + 50, hv_OutColEnd1, "red", "true");
                }

                image.Dispose();
                ho_ImageScaled.Dispose();
                ho_ImageMean.Dispose();
                regionTrans.Dispose();
                RegionThreshold.Dispose();
                RegionFilledUp.Dispose();
                RegionConnected.Dispose();
                RegionSelected.Dispose();
                RegionDilationed.Dispose();
                imageReduced.Dispose();
                return true;
            }
            catch (HalconException hex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public bool GetAngleTune2(int nCamID, out double fAngle, HTuple hwindow = null)
        {
            HObject ho_Image111Jpg, ho_ImageScaled, ho_Regions;
            HObject ho_RegionErosion, ho_RegionFillUp, ho_ConnectedRegions;
            HObject ho_SelectedRegions, ho_RegionTrans, ho_RegionErosion1;
            HObject ho_RegionDilation, ho_RegionDifference, ho_ImageReduced;
            HObject ho_Regions1, ho_RegionErosion2, ho_RegionTrans1;
            HObject ho_Skeleton, ho_Contours, ho_SelectedContours;

            // Local control variables 

            HTuple hv_RowBegin, hv_ColBegin;
            HTuple hv_RowEnd, hv_ColEnd, hv_Nr, hv_Nc, hv_Dist, hv_Angle;

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image111Jpg);
            HOperatorSet.GenEmptyObj(out ho_ImageScaled);
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionTrans);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion1);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_Regions1);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion2);
            HOperatorSet.GenEmptyObj(out ho_RegionTrans1);
            HOperatorSet.GenEmptyObj(out ho_Skeleton);
            HOperatorSet.GenEmptyObj(out ho_Contours);
            HOperatorSet.GenEmptyObj(out ho_SelectedContours);
            fAngle = 0;
            try
            {
                if (hwindow == null)
                {
                    if (!HwindowDic.Keys.Contains(nCamID))
                    {
                        throw new Exception("没有合适的窗口执行此操作");
                    }
                }
                
                ho_Image111Jpg = HoImageList[nCamID].SelectObj(1);
                HOperatorSet.GetImageSize(ho_Image111Jpg, out HTuple imageWidth, out HTuple imageHeight);
                ho_ImageScaled.Dispose();
                scale_image_range(ho_Image111Jpg, out ho_ImageScaled, 100, 200);
                ho_Regions.Dispose();
                HOperatorSet.Threshold(ho_ImageScaled, out ho_Regions, 207, 255);
                ho_RegionErosion.Dispose();
                HOperatorSet.ErosionCircle(ho_Regions, out ho_RegionErosion, 3.5);
                ho_RegionFillUp.Dispose();
                HOperatorSet.FillUp(ho_RegionErosion, out ho_RegionFillUp);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_RegionFillUp, out ho_ConnectedRegions);
                ho_SelectedRegions.Dispose();
                HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "area", "and", 458034, 1000675);
                ho_RegionTrans.Dispose();
                HOperatorSet.ShapeTrans(ho_SelectedRegions, out ho_RegionTrans, "outer_circle");
                ho_RegionErosion1.Dispose();
                HOperatorSet.ErosionCircle(ho_RegionTrans, out ho_RegionErosion1, 5);
                ho_RegionDilation.Dispose();
                HOperatorSet.DilationCircle(ho_RegionTrans, out ho_RegionDilation, 120);
                ho_RegionDifference.Dispose();
                HOperatorSet.Difference(ho_RegionDilation, ho_RegionErosion1, out ho_RegionDifference);
                ho_ImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_ImageScaled, ho_RegionDifference, out ho_ImageReduced);
                ho_Regions1.Dispose();
                HOperatorSet.Threshold(ho_ImageReduced, out ho_Regions1, 124, 255);
                ho_RegionErosion2.Dispose();
                HOperatorSet.ErosionCircle(ho_Regions1, out ho_RegionErosion2, 25);
                ho_RegionTrans1.Dispose();
                HOperatorSet.ShapeTrans(ho_RegionErosion2, out ho_RegionTrans1, "rectangle2");
       
                ho_Skeleton.Dispose();
                HOperatorSet.Skeleton(ho_RegionTrans1, out ho_Skeleton);
                ho_Contours.Dispose();
                HOperatorSet.GenContoursSkeletonXld(ho_Skeleton, out ho_Contours, 1, "filter");
                ho_SelectedContours.Dispose();
                HOperatorSet.SelectContoursXld(ho_Contours, out ho_SelectedContours, "contour_length", 1000, 2000000, -0.5, 0.5);
                HOperatorSet.FitLineContourXld(ho_SelectedContours, "tukey", -1, 0, 5, 2, out hv_RowBegin, out hv_ColBegin, out hv_RowEnd, out hv_ColEnd, out hv_Nr, out hv_Nc, out hv_Dist);
               
                HOperatorSet.AngleLx(hv_RowBegin, hv_ColBegin, hv_RowEnd, hv_ColEnd, out hv_Angle);
                if ((int)(new HTuple(hv_Angle.TupleLess(0))) != 0)
                {
                    hv_Angle = hv_Angle + 3.14159;
                }
                foreach (var it in HwindowDic[nCamID])
                {
                    HOperatorSet.SetPart(it.Value, 0, 0, imageHeight, imageWidth);
                    HOperatorSet.DispObj(ho_Image111Jpg, it.Value);
                    HOperatorSet.SetLineWidth(it.Value, 3);
                    HOperatorSet.SetColor(it.Value, "red");
                    HOperatorSet.DispLine(it.Value, hv_RowBegin, hv_ColBegin, hv_RowEnd, hv_ColEnd);
                    HOperatorSet.DispLine(it.Value, hv_RowEnd, hv_ColEnd - 4000, hv_RowEnd, hv_ColEnd + 4000);
                    HOperatorSet.DispArc(it.Value, hv_RowEnd, hv_ColEnd, hv_Angle, (hv_RowBegin + hv_RowEnd) / 2, (hv_ColBegin + hv_ColEnd) / 2);
                    disp_message(it.Value, ((hv_Angle * 180) / 3.14159) + "度", "image", hv_RowEnd + 50, hv_ColEnd, "red", "true");
                }
                fAngle = (hv_Angle * 180) / 3.1415926;
                return true;
            }
            catch (HalconException ex)
            {
                ho_Image111Jpg.Dispose();
                ho_ImageScaled.Dispose();
                ho_Regions.Dispose();
                ho_RegionErosion.Dispose();
                ho_RegionFillUp.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_SelectedRegions.Dispose();
                ho_RegionTrans.Dispose();
                ho_RegionErosion1.Dispose();
                ho_RegionDilation.Dispose();
                ho_RegionDifference.Dispose();
                ho_ImageReduced.Dispose();
                ho_Regions1.Dispose();
                ho_RegionErosion2.Dispose();
                ho_RegionTrans1.Dispose();
                ho_Skeleton.Dispose();
                ho_Contours.Dispose();
                ho_SelectedContours.Dispose();
                throw new Exception($"执行获取镜头角度时发生错误:{ex.Message}");
            }
        }

        private bool FindModelAndGetData()
        {
            return true;
        }

        private bool FindLineTop()
        {
            return true;
        }

        private bool FindLineBottom()
        {
            return true;
        }
        #endregion

        public bool PreCreateShapeModel(int nCamID, int MinThre, int MaxThre, EnumShapeModelType modelType, string regionFilePath, object regionIn = null)
        {
            if (nCamID < 0 || MaxThre < MinThre)
                return false;
            if (HwindowDic.Keys.Contains(nCamID) && HwindowDic[nCamID].Keys.Contains(DebugWindowName))
            {
                HTuple window = HwindowDic[nCamID][DebugWindowName];
                switch (modelType)
                {
                    case EnumShapeModelType.Gray:
                        break;
                    case EnumShapeModelType.Shape:
                        break;
                    case EnumShapeModelType.XLD:
                        HObject region = regionIn as HObject;
                        return PreProcessShapeMode(HoImageList[nCamID], window, MinThre, MaxThre, region, regionFilePath, true);
                    default:
                        return false;
                }
            }
            return true;
        }
        public bool SaveShapeModel(int nCamID, int MinThre, int MaxThre, EnumShapeModelType modelType, string regionFilePath, object regionIn = null)
        {
            if (nCamID < 0 || MaxThre < MinThre)
                return false;
            if (HwindowDic.Keys.Contains(nCamID) && HwindowDic[nCamID].Keys.Contains(DebugWindowName))
            {
                HTuple window = HwindowDic[nCamID][DebugWindowName];
                switch (modelType)
                {
                    case EnumShapeModelType.Gray:
                        break;
                    case EnumShapeModelType.Shape:
                        break;
                    case EnumShapeModelType.XLD:
                        HObject region = regionIn as HObject;
                        return PreProcessShapeMode(HoImageList[nCamID], window, MinThre, MaxThre, region, regionFilePath, false);
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
        private void FindLine(HObject ho_Image, HTuple hv_CaliperNum, HTuple hv_EdgeGrayValue, HTuple hv_RoiRow, HTuple hv_RoiCol, HTuple hv_RoiPhi, HTuple hv_RoiL1, HTuple hv_RoiL2, out HTuple hv_OutRowStart, out HTuple hv_OutColStart, out HTuple hv_OutRowEnd, out HTuple hv_OutColEnd)
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
            HTuple hv_Dist = new HTuple();

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
            for (hv_Index = 1; hv_Index.Continue(hv_CaliperNum_COPY_INP_TMP, 1); hv_Index = hv_Index.TupleAdd(1))
            {
                HOperatorSet.GenMeasureRectangle2(hv_newRow, hv_newCol, hv_RoiPhi, hv_newL1,
                    hv_newL2, hv_Width, hv_Height, "nearest_neighbor", out hv_MeasureHandle);
                HOperatorSet.MeasurePos(ho_Image, hv_MeasureHandle, 1, hv_EdgeGrayValue, "negative",
                    "first", out hv_RowEdge, out hv_ColumnEdge, out hv_Amplitude, out hv_Distance);
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
        private void disp_message(HTuple hv_WindowHandle, HTuple hv_String, HTuple hv_CoordSystem, HTuple hv_Row, HTuple hv_Column, HTuple hv_Color, HTuple hv_Box)
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
            foreach (var it in fileListInDataDirection)
            {
                if (it.Contains(string.Format("Cam{0}_", nCamID)))
                    list.Add(it);
            }
            return list;
        }

    }
}
