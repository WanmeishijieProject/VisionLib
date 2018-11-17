using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GalaSoft.MvvmLight.Messaging;
using HalconDotNet;
using JPT_TosaTest.Classes;
using JPT_TosaTest.UserCtrl;
using JPT_TosaTest.UserCtrl.VisionDebugTool;
using JPT_TosaTest.Vision.ProcessStep;

namespace JPT_TosaTest.Vision
{
  
    public class HalconVision : VisionDataOperateSet
    {
        #region constructor
        private HalconVision()
        {
            HOperatorSet.GenEmptyObj(out HObject emptyImage);
            for (int i = 0; i < 10; i++)
            {

                HoImageList.Add(emptyImage);
                AcqHandleList.Add(new HTuple());
                KList.Add(1);
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
       
        private List<HObject> HoImageList = new List<HObject>(10);    //Image
        private List<HTuple> AcqHandleList = new List<HTuple>(10);    //Aqu
        private List<double> KList = new List<double>(10);
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
                    if (HwindowDic[nCamID][Name] != -1)
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
                        HOperatorSet.OpenFramegrabber(CamCfgDic.ElementAt(nCamID).Value.Item2, 1, 1, 0, 0, 0, 0, "default", 8, "gray",
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
        /// <summary>
        /// 采集图像
        /// </summary>
        /// <param name="nCamID"></param>
        /// <param name="bDispose"></param>
        /// <param name="bContinus"></param>
        public void GrabImage(int nCamID, bool bDispose = true, bool bContinus = false)
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
                    if (!bContinus)
                        HOperatorSet.GrabImage(out image, AcqHandleList[nCamID]);
                    else
                        HOperatorSet.GrabImageAsync(out image, AcqHandleList[nCamID], -1);

                    HOperatorSet.GetImageSize(image, out HTuple width, out HTuple height);

                    HOperatorSet.GenEmptyObj(out Region);

                    if (HoImageList[nCamID] != null)
                    {
                        HoImageList[nCamID].Dispose();
                        HoImageList[nCamID] = null;
                    }
                    HoImageList[nCamID] = image.SelectObj(1);
                    if (!SyncEvent.WaitOne(20))
                    {
                        foreach (var it in HwindowDic[nCamID])
                            if (it.Value != -1)
                            {
                                //HOperatorSet.SetPart(it.Value, height/2-500, width/2-500, height/2+500, width/2+500);
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
        /// 传入的处理步骤参数
        /// </summary>
        /// <param name="ProcessStep"></param>
        /// <returns></returns>
        public bool ProcessImage(VisionProcessStepBase ProcessStep)
        {
            int nCamID = ProcessStep.CamID;
            if (nCamID < 0)
                return false;
            ProcessStep.Image = HoImageList[nCamID];     
            return ProcessStep.Process();
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
                    switch (nStep)
                    {
                        case IMAGEPROCESS_STEP.T1:  //第一步 找出模板并获取第二步与第三步的ROI数据
                            {
                                string strModelFileName = para.ToString();
                                bRet = FindModelAndGetData(HoImageList[nCamID], strModelFileName, out HTuple hom_2D, out HTuple ModelPos);
                                if (hom_2D.Length != 0 && ModelPos.Length != 0)
                                    result = new List<object>() { hom_2D, ModelPos };
                                else
                                {
                                    //TO DO
                                }
                            }
                            break;
                        case IMAGEPROCESS_STEP.T2:  //第二步，找出顶部与底部的线线
                            {
                                var originPara = para as List<object>;
                                HTuple hom_2D = originPara[0] as HTuple;
                                HTuple ModelPos = originPara[1] as HTuple;
                                List<string> listParas = originPara[2] as List<string>;
                                bRet = FindLineBasedModelRoi(HoImageList[nCamID], listParas, hom_2D, ModelPos, out List<object> lineList);   //只需要显示
                                result = lineList;
                            }
                            break;
                        case IMAGEPROCESS_STEP.T3:  //Top后期处理
                            {
                                //转换像素与实际关系
                                HTuple SelectLineIndex = 0;
                                double CenterOffset = (Config.ConfigMgr.Instance.ProcessData.CenterLineOffset / KList[nCamID]);
                                List<object> list = para as List<object>;
                                List<Tuple<HTuple, HTuple, HTuple, HTuple>> TupleList = new List<Tuple<HTuple, HTuple, HTuple, HTuple>>();
                                foreach (var it in list)
                                {
                                    object obj = it;
                                    TupleList.Add(obj as Tuple<HTuple, HTuple, HTuple, HTuple>);
                                }
                                if (list != null)
                                {
                                    DisplayLines(nCamID, TupleList);
                                    if (TupleList.Count >= 2)
                                    {
                                        if (TupleList[0].Item1 > 1000)
                                            SelectLineIndex = 0;
                                        else
                                            SelectLineIndex = 1;
                                            GetParallelLineFromDistance(TupleList[SelectLineIndex].Item1, TupleList[SelectLineIndex].Item2, TupleList[SelectLineIndex].Item3, TupleList[SelectLineIndex].Item4,
                                            CenterOffset, "row", -1, out HTuple hv_LineOutRow, out HTuple hv_LineOutCol, out HTuple hv_LineOutRow1, out HTuple hv_LineOutCol1,
                                            out HTuple hv_k, out HTuple hv_b);
                                        if (hv_LineOutRow != null && hv_LineOutRow1 != null)
                                        {
                                            DisplayLines(nCamID, new List<Tuple<HTuple, HTuple, HTuple, HTuple>>() { new Tuple<HTuple, HTuple, HTuple, HTuple>(hv_LineOutRow, hv_LineOutCol, hv_LineOutRow1, hv_LineOutCol1) });
                                        }
                                    }
                                }
                            }
                            break;
                        case IMAGEPROCESS_STEP.T4: //Bottom后期处理
                            {
                                //转换像素与实际关系
                                double PadOffset = Config.ConfigMgr.Instance.ProcessData.PadOffset / KList[nCamID];
                                List<object> list = para as List<object>;
                                List<Tuple<HTuple, HTuple, HTuple, HTuple>> TupleList = new List<Tuple<HTuple, HTuple, HTuple, HTuple>>();
                                foreach (var it in list)
                                {
                                    object obj = it;
                                    TupleList.Add(obj as Tuple<HTuple, HTuple, HTuple, HTuple>);
                                }
                                if (list != null)
                                {
                                    DisplayLines(nCamID, TupleList);
                                    int LineNum = TupleList.Count;
                                    if (LineNum >= 3)
                                    {
                                        List<Tuple<HTuple, HTuple, HTuple, HTuple>> listFinal = new List<Tuple<HTuple, HTuple, HTuple, HTuple>>();
                                        for (int i = 0; i < LineNum - 1; i++)
                                        {
                                            HOperatorSet.IntersectionLl(TupleList[i].Item1, TupleList[i].Item2, TupleList[i].Item3, TupleList[i].Item4,
                                                                    TupleList[LineNum - 1].Item1, TupleList[LineNum - 1].Item2, TupleList[LineNum - 1].Item3, TupleList[LineNum - 1].Item4, out HTuple row1, out HTuple col1, out HTuple isParallel1);
                                            GetVerticalFromDistance(row1, col1, TupleList[LineNum - 1].Item1, TupleList[LineNum - 1].Item2, TupleList[LineNum - 1].Item3, TupleList[LineNum - 1].Item4, PadOffset, "row", -1,
                                                               out HTuple TargetRow1, out HTuple TargetCol1, out HTuple k, out HTuple b, out HTuple kIn, out HTuple bIn);
                                            listFinal.Add(new Tuple<HTuple, HTuple, HTuple, HTuple>(row1, col1, TargetRow1, TargetCol1));
                                        }

                                        for (int i = 0; i < LineNum / 2; i++)
                                        {
                                            HTuple rows = new HTuple();
                                            HTuple cols = new HTuple();
                                            rows[0] = listFinal[2 * i].Item1;
                                            rows[1] = listFinal[2 * i].Item3;
                                            rows[2] = listFinal[2 * i + 1].Item3;
                                            rows[3] = listFinal[2 * i + 1].Item1;
                                            cols[0] = listFinal[2 * i].Item2;
                                            cols[1] = listFinal[2 * i].Item4;
                                            cols[2] = listFinal[2 * i + 1].Item4;
                                            cols[3] = listFinal[2 * i + 1].Item2;
                                            DisplayPolygonRegion(0, rows, cols);
                                        }

                                        //画最后一条平行线
                                        GetParallelLineFromDistance(TupleList[LineNum - 1].Item1, TupleList[LineNum - 1].Item2, TupleList[LineNum - 1].Item3, TupleList[LineNum - 1].Item4, PadOffset, "row", -1, out HTuple hv_PLineRow, out HTuple hv_PLineCol,
                                                                    out HTuple hv_PLineRow1, out HTuple hv_PLineCol1, out HTuple k1, out HTuple b1);

                                        DisplayLines(0, new List<Tuple<HTuple, HTuple, HTuple, HTuple>>() { new Tuple<HTuple, HTuple, HTuple, HTuple>(hv_PLineRow, hv_PLineCol, hv_PLineRow1, hv_PLineCol1) });
                                    }
                                }
                            }
                            break;

                        //通过两条平行线来标定
                        case IMAGEPROCESS_STEP.T5:  //标定图像
                            {
                                result= SetKValueOfCam(nCamID, 6600, para as List<Object>);
                            }

                            break;


                        //画几何标记
                        case IMAGEPROCESS_STEP.T6:
                            {
                                bRet = true;
                                string[] originList = para.ToString().Split(',');
                                if (originList.Length >= 4)
                                {
                                    //先找模板
                                    string strModelFileName = originList[0];
                                    bRet &= FindModelAndGetData(HoImageList[nCamID], strModelFileName, out HTuple hom_2D, out HTuple ModelPos);

                                    //找参考线，根据线的名字找线
                                    Enum.TryParse(originList[3].ToString(),out EnumGeometryType GeometryType);
                                    List<string> listParas = new List<string> { originList[1], originList[2] };
                                    bRet &= FindLineBasedModelRoi(HoImageList[nCamID], listParas, hom_2D, ModelPos, out List<object> lineList);   //只需要显示

                                    //画区域
                                    
                                    if (lineList.Count == 2)
                                    {
                                        List<Tuple<HTuple, HTuple, HTuple, HTuple>> TupleList = new List<Tuple<HTuple, HTuple, HTuple, HTuple>>();
                                        foreach (var it in lineList)
                                        {
                                            object obj = it;
                                            TupleList.Add(obj as Tuple<HTuple, HTuple, HTuple, HTuple>);
                                        }

                                        if (TupleList.Count >= 2)
                                        {
                                            DrawGeometry(HwindowDic[nCamID][DebugWindowName], HoImageList[nCamID], TupleList[0].Item1, TupleList[0].Item2, TupleList[0].Item3, TupleList[0].Item4,
                                                                        TupleList[1].Item1, TupleList[1].Item2, TupleList[1].Item3, TupleList[1].Item4, GeometryType);
                                        }
                                    }
                                }
                                
                            }
                            break;


                        //显示几何标记
                        case IMAGEPROCESS_STEP.T7:
                            {
                                List<object> list = para as List<object>;
                                List<Tuple<HTuple, HTuple, HTuple, HTuple>> TupleList = new List<Tuple<HTuple, HTuple, HTuple, HTuple>>();
                                if (list.Count >= 2)
                                {
                                    object lines = list[0];

                                    foreach(var it in lines as List<object>)
                                    {
                                        TupleList.Add(it as Tuple<HTuple, HTuple, HTuple, HTuple>);
                                    }
                                    GeometryPose = list[1] as HTuple;
                                   //线，后一个是Pose

                                        //显示直线的矩形框
                                    DisplayLines(nCamID, TupleList);    //显示Tia的参考线

                                    if (TupleList.Count >= 2)
                                    {
                                        //利用参考线画出原来画的区域
                                        GetGeometryRegionBy2Lines(GeometryRegion, TupleList[0].Item1, TupleList[0].Item2, TupleList[0].Item3, TupleList[0].Item4,
                                                                TupleList[1].Item1, TupleList[1].Item2, TupleList[1].Item3, TupleList[1].Item4, GeometryPose, out HObject NewRegion);
                                        foreach (var it in HwindowDic[nCamID])
                                        {
                                            HOperatorSet.DispRegion(NewRegion, it.Value);
                                        }
                                        if (NewRegion.IsInitialized())
                                            NewRegion.Dispose();
                                    }
                                    
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                result = null;
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
                return false;
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
                UC_MessageBox.ShowMsgBox($"{ex.Message}", "错误", MsgType.Error);
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
                UC_MessageBox.ShowMsgBox($"{ex.Message}", "错误", MsgType.Error);
                return false;
            }
        }
        public bool CloseCamera()
        {
            HOperatorSet.CloseAllFramegrabbers();
            return true;
        }
        public Dictionary<string, Tuple<string, string>> FindCamera(EnumCamType camType, List<string> acturalNameList, out List<string> ErrorList)
        {
            Dictionary<string, Tuple<string, string>> dic = new Dictionary<string, Tuple<string, string>>();
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
                    return dic;
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
                if (hv_Row1.Length == 0)
                {
                    foreach (var it in HwindowDic[nCamID])
                    {
                        disp_message(it.Value, "查找模板失败", "window", 10, 10, "red", "true");
                    }
                    return false;
                }
                else
                {
                    foreach (var it in HwindowDic[nCamID])
                    {
                        HOperatorSet.SetPart(it.Value, 0, 0, ImageHeight, ImageWidth);
                        HOperatorSet.SetColor(it.Value, "red");
                        disp_message(it.Value, $"模板位置:({ hv_Row1},{ hv_Column1})", "window", 10, 10, "red", "true");
                        disp_message(it.Value, $"分数: { hv_Score}", "window", 50, 10, "red", "true");
                        HOperatorSet.DispCross(it.Value, hv_Row1, hv_Column1, 80, hv_Angle);
                        ModelPos[0] = hv_Row1;
                        ModelPos[1] = hv_Column1;
                        ModelPos[2] = hv_Angle;
                    }

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

        public bool FindLineBasedModelRoi(HObject image, List<string> LineParaList, HTuple hom_2D, HTuple ModelPos, out List<object> lineList)
        {
            List<object> lineListRawData = new List<object>();
            lineList = new List<object>();

            //ReadRectangle
            try
            {
                foreach (var it in HwindowDic[0])
                {
                    HOperatorSet.SetSystem("flush_graphic", "false");
                    HOperatorSet.ClearWindow(it.Value);
                    HOperatorSet.DispObj(image, it.Value);
                    HOperatorSet.SetDraw(it.Value, "margin");
                    HOperatorSet.SetSystem("flush_graphic", "true");
                }
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
                                    Enum.TryParse(strRtPara[1], out EnumEdgeType EdgeType);
                                    Enum.TryParse(strRtPara[2], out EnumSelectType SelectType);
                                    double.TryParse(strRtPara[3], out double fContrast);
                                    HTuple Contrast = Math.Round(fContrast);
                                    FindLine(image, EdgeType, SelectType, CaliperNum, Contrast, outRoiRow, outRoiCol, Phi + ModelPos[2], L1, L2, out HTuple StartRow, out HTuple StartCol, out HTuple EndRow, out HTuple EndCol);
                                    lineList.Add(new Tuple<HTuple, HTuple, HTuple, HTuple>(StartRow, StartCol, EndRow, EndCol));
                                }
                                break;
                            case "PAIRTOOL":
                                {
                                    HTuple CaliperNum = int.Parse(strRtPara[0]);
                                    HTuple ExpectedPairNum = int.Parse(strRtPara[1]);
                                    Enum.TryParse(strRtPara[2], out EnumPairType PairType);
                                    Enum.TryParse(strRtPara[3], out EnumSelectType SelectType);
                                    double.TryParse(strRtPara[4], out double fContrast);
                                    HTuple Contrast = Math.Round(fContrast);
                                    FindPair(image, ExpectedPairNum, PairType, SelectType, CaliperNum, Contrast, outRoiRow, outRoiCol, Phi + ModelPos[2], L1, L2,
                                       out HTuple OutFirstRowStart, out HTuple FirstColStart, out HTuple OutFirstRowEnd, out HTuple OutFirstColEnd,
                                       out HTuple OutSecondRowStart, out HTuple SecondColStart, out HTuple OutSecondRowEnd, out HTuple OutSecondColEnd);
                                    for (HTuple index = 0; index < ExpectedPairNum; index++)
                                    {
                                        lineList.Add(new Tuple<HTuple, HTuple, HTuple, HTuple>(OutFirstRowStart[index], FirstColStart[index], OutFirstRowEnd[index], OutFirstColEnd[index]));
                                        lineList.Add(new Tuple<HTuple, HTuple, HTuple, HTuple>(OutSecondRowStart[index], SecondColStart[index], OutSecondRowEnd[index], OutSecondColEnd[index]));
                                    }
                                }
                                break;
                            default:
                                throw new Exception("invalid TOOL");
                        }

                        //Display绿色是原始直线
                        foreach (var it in HwindowDic[0])
                        {
                            HOperatorSet.SetColor(it.Value, "green");
                            HOperatorSet.DispObj(rect, it.Value);
                            HOperatorSet.SetLineWidth(it.Value, 1);
                            foreach (var obj in lineList)
                            {
                                Tuple<HTuple, HTuple, HTuple, HTuple> line = obj as Tuple<HTuple, HTuple, HTuple, HTuple>;
                                if (line != null)
                                    HOperatorSet.DispLine(it.Value, line.Item1, line.Item2, line.Item3, line.Item4);
                            }

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
        public bool DisplayLines(int nCamID, List<Tuple<HTuple, HTuple, HTuple, HTuple>> lineList, string Color = "red")
        {
            try
            {
                lock (_lockList[nCamID])
                {
                    if (lineList == null)
                        return false;
                    foreach (var it in HwindowDic[nCamID])
                    {
                        HOperatorSet.SetColor(it.Value, Color);
                        foreach (var line in lineList)
                        {
                            HOperatorSet.DispLine(it.Value, line.Item1, line.Item2, line.Item3, line.Item4);
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
            lock (_lockList[nCamID])
            {
                HOperatorSet.GenRegionPolygonFilled(out HObject region, Row, Col);

                foreach (var it in HwindowDic[nCamID])
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
        private bool SetKValueOfCam(int nCamID, double RealDistance, List<object> PLineList)
        {
            if (PLineList == null || PLineList.Count < 2 || RealDistance==0.0f)
                return false;
            Tuple<HTuple, HTuple, HTuple, HTuple> line1 = PLineList[0] as Tuple<HTuple, HTuple, HTuple, HTuple>;
            Tuple<HTuple, HTuple, HTuple, HTuple> line2 = PLineList[1] as Tuple<HTuple, HTuple, HTuple, HTuple>;
            if (line1 == null || line2 == null)
                return false;
            HOperatorSet.DistancePl(line1.Item1, line1.Item2, line2.Item1, line2.Item2, line2.Item3, line2.Item4, out HTuple D1);
            HOperatorSet.DistancePl(line1.Item3, line1.Item4, line2.Item1, line2.Item2, line2.Item3, line2.Item4, out HTuple D2);
            KList[nCamID] = (2 * RealDistance) / (D1 + D2);
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

        public void Debug_FindLine(int nCamID, EnumEdgeType Plority, EnumSelectType SelectType, int Contrast, int CaliperNum)
        {
            try
            {
                HTuple Row=new HTuple(),Col=new HTuple(),Phi=new HTuple(),L1=new HTuple(),L2=new HTuple();
                HTuple WindowHandle = HwindowDic[nCamID][DebugWindowName];
                HOperatorSet.SetColor(WindowHandle, "green");
                HOperatorSet.SetLineWidth(WindowHandle, 1);
                if (string.IsNullOrEmpty(LineRoiData)) //如果是首次就画一个矩形
                {
                    Debug_DrawRectangle2(WindowHandle, out Row, out Col, out Phi, out L1, out L2);
                    GetRectData(EnumToolType.LineTool, Row, Col, Phi, L1, L2);
                }
                else
                {
                    string[] paraList = LineRoiData.Split('&');
                    Row = double.Parse(paraList[0]);
                    Col = double.Parse(paraList[1]);
                    Phi = double.Parse(paraList[2]);
                    L1=double.Parse(paraList[3]);
                    L2 = double.Parse(paraList[4]);
                }

                FindLine(HoImageList[nCamID], Plority,SelectType, CaliperNum, Contrast, Row, Col, Phi, L1, L2, out HTuple hv_OutRowStart, out HTuple hv_OutColStart, out HTuple hv_OutRowEnd, out HTuple hv_OutColEnd);

                HOperatorSet.SetSystem("flush_graphic", "false");
                HOperatorSet.ClearWindow(WindowHandle);
                HOperatorSet.DispObj(HoImageList[nCamID],WindowHandle);

                HOperatorSet.SetColor(WindowHandle, "red");
                HOperatorSet.SetLineWidth(WindowHandle, 3);

                HOperatorSet.SetSystem("flush_graphic", "true");
                HOperatorSet.DispLine(WindowHandle, hv_OutRowStart, hv_OutColStart, hv_OutRowEnd, hv_OutColEnd);
      

                //HOperatorSet.AngleLx(hv_OutRowStart, hv_OutColStart, hv_OutRowEnd, hv_OutColEnd, out HTuple angle);
                //angle = angle < 0 ? (angle + 3.1415926) : angle;
                //disp_message(WindowHandle, "角度：" + angle * 180.0 / 3.1415926 + "度", "image", hv_OutRowStart, hv_OutColStart, "red", "false");
            }
            catch(Exception ex)
            {

            }
        }
        public void Debug_FindPair(int nCamID, EnumPairType Plority, EnumSelectType SelectType,int ExpectedPairNum, int Contrast, int CaliperNum)
        {
            try
            {
                HTuple Row = new HTuple(), Col = new HTuple(), Phi = new HTuple(), L1 = new HTuple(), L2 = new HTuple();
                HTuple WindowHandle = HwindowDic[nCamID][DebugWindowName];
                HOperatorSet.SetColor(WindowHandle, "green");
                HOperatorSet.SetLineWidth(WindowHandle, 1);
                if (string.IsNullOrEmpty(PairRoiData)) //如果是首次就画一个矩形
                {
                    Debug_DrawRectangle2(WindowHandle, out Row, out Col, out Phi, out L1, out L2);
                    GetRectData(EnumToolType.PairTool, Row, Col, Phi, L1, L2);
                }
                else
                {
                    string[] paraList = PairRoiData.Split('&');
                    Row = double.Parse(paraList[0]);
                    Col = double.Parse(paraList[1]);
                    Phi = double.Parse(paraList[2]);
                    L1 = double.Parse(paraList[3]);
                    L2 = double.Parse(paraList[4]);
                }

                FindPair(HoImageList[nCamID], ExpectedPairNum, Plority, SelectType, CaliperNum, Contrast, Row, Col, Phi, L1, L2,
                        out HTuple OutFirstRowStart, out HTuple FirstColStart, out HTuple OutFirstRowEnd, out HTuple OutFirstColEnd,
                        out HTuple OutSecondRowStart, out HTuple SecondColStart, out HTuple OutSecondRowEnd, out HTuple OutSecondColEnd);
                

                HOperatorSet.SetSystem("flush_graphic", "false");
                HOperatorSet.ClearWindow(WindowHandle);
                HOperatorSet.DispObj(HoImageList[nCamID], WindowHandle);

                HOperatorSet.SetColor(WindowHandle, "red");
                HOperatorSet.SetLineWidth(WindowHandle, 3);

                HOperatorSet.SetSystem("flush_graphic", "true");
                HOperatorSet.DispLine(WindowHandle, OutFirstRowStart, FirstColStart, OutFirstRowEnd, OutFirstColEnd);
                HOperatorSet.DispLine(WindowHandle, OutSecondRowStart, SecondColStart, OutSecondRowEnd, OutSecondColEnd);
            }
            catch
            {

            }
        }

   
        #region Private method
        private void FindLine(HObject ho_Image, EnumEdgeType Polarity, EnumSelectType selectType, HTuple hv_CaliperNum, HTuple hv_EdgeGrayValue, HTuple hv_RoiRow, HTuple hv_RoiCol, HTuple hv_RoiPhi, HTuple hv_RoiL1, HTuple hv_RoiL2, out HTuple hv_OutRowStart, out HTuple hv_OutColStart, out HTuple hv_OutRowEnd, out HTuple hv_OutColEnd)
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
            HTuple hv_Dist = new HTuple(),hv_Polarity=new HTuple(),hv_SelectType=new HTuple();
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
                case EnumEdgeType.LightToDark:
                    hv_Polarity = "negative"; 
                    break;
                case EnumEdgeType.DarkToLight:
                    hv_Polarity = "positive";
                    break;
                case EnumEdgeType.All:
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
        private void Debug_DrawRectangle2(HTuple WindowHandle, out HTuple Row, out HTuple Col, out HTuple Phi, out HTuple L1, out HTuple L2)
        {
            Row = Col = Phi = L1 = L2 = 0;
            HOperatorSet.DrawRectangle2Mod(WindowHandle, 100, 100, 100, 100, 100, out Row, out Col, out Phi, out L1, out L2);
        }
        private void FindPair(HObject ho_Image,HTuple ExpectPairNum, EnumPairType Polarity, EnumSelectType selectType, HTuple hv_CaliperNum, HTuple hv_EdgeGrayValue, HTuple hv_RoiRow, HTuple hv_RoiCol, HTuple hv_RoiPhi, HTuple hv_RoiL1, HTuple hv_RoiL2, 
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
            HTuple hv_Dist = new HTuple(), hv_Polarity = new HTuple(), hv_SelectType=new HTuple();
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
                case EnumPairType.Dark:
                    hv_Polarity = "negative";
                    break;
                case EnumPairType.Light:
                    hv_Polarity = "positive";
                    break;
                case EnumPairType.All:
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

                if (rowEdgeFirst.Length != 0 && rowEdgeFirst.Length==rowEdgeSecond.Length && ExpectPairNum== rowEdgeFirst.Length)
                {
                    for(HTuple i=0;i<ExpectPairNum;i++)
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
                if (RowFirstList[i].Length!= 0)
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
        public bool SaveFlagToolRegion(string FileFullPathName)
        {
            try
            {
                HOperatorSet.WriteRegion(GeometryRegion, FileFullPathName.Replace(".para",".reg"));
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        ///  确定一个点的唯一位置，通过两条相交直线确定
        /// </summary>
        /// <param name="WindowHandle"></param>
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
        /// <param name="geometryPose">输出参数</param>
        /// <param name="geometryRegion">几何区域</param>
        public void DrawGeometry(HTuple WindowHandle, HObject image, HTuple rowStart, HTuple colStart, HTuple rowEnd, HTuple colEnd, HTuple rowStart1,
            HTuple colStart1, HTuple rowEnd1, HTuple colEnd1, EnumGeometryType GeometryType)
        {
            switch (GeometryType)
            {
                case EnumGeometryType.CIRCLE:
                    {
                        HOperatorSet.DrawCircleMod(WindowHandle, 500, 500, 200, out HTuple row, out HTuple col, out HTuple radius);
                        HOperatorSet.GenCircle(out HObject Circle, row, col, radius);
                        HOperatorSet.GenRegionLine(out HObject region1, row - 30, col, row + 30, col);
                        HOperatorSet.GenRegionLine(out HObject region2, row, col - 30, row, col + 30);
                        HOperatorSet.Union2(region1, Circle, out HObject UnionRegion);
                        HOperatorSet.Union2(region2, UnionRegion, out UnionRegion);
                        HOperatorSet.Union2(GeometryRegion, UnionRegion, out UnionRegion);
                        GeometryRegion = UnionRegion.SelectObj(1);
                        UnionRegion.Dispose();
                        Circle.Dispose();
                        region1.Dispose();
                        region2.Dispose();
                    }
                    break;
                case EnumGeometryType.LINE:
                    {
                        HOperatorSet.DrawLineMod(WindowHandle, 20, 20, 200, 200, out HTuple row1, out HTuple col1, out HTuple row2, out HTuple col2);
                        HOperatorSet.GenRegionLine(out HObject Line, row1, col1, row2, col2);
                        HOperatorSet.Union2(GeometryRegion, Line, out HObject UnionRegion);
                        HOperatorSet.Union2(GeometryRegion, UnionRegion, out UnionRegion);
                        GeometryRegion = UnionRegion.SelectObj(1);

                        UnionRegion.Dispose();
                        Line.Dispose();

                    }
                    break;
                case EnumGeometryType.POINT:
                    {
                        HOperatorSet.DrawPointMod(WindowHandle,200,200, out HTuple row, out HTuple col);
                        HOperatorSet.GenRegionLine(out HObject region1, row - 30, col, row + 30, col);
                        HOperatorSet.GenRegionLine(out HObject region2, row, col - 30, row , col + 30);
                        HOperatorSet.Union2(region1, region2, out HObject UnionRegion);

                        HOperatorSet.Union2(GeometryRegion, UnionRegion, out UnionRegion);
                        GeometryRegion = UnionRegion.SelectObj(1);

                        UnionRegion.Dispose();
                        region1.Dispose();
                        region2.Dispose();

                    }
                    break;
                case EnumGeometryType.RECTANGLE1:
                    {
                        HOperatorSet.DrawRectangle1Mod(WindowHandle,20,20,200,200, out HTuple row1, out HTuple col1, out HTuple row2, out HTuple col2);
                        HOperatorSet.GenRectangle1(out HObject Rectangle, row1, col1, row2, col2);

                        HOperatorSet.Union2(GeometryRegion, Rectangle, out HObject UnionRegion);
                        GeometryRegion = UnionRegion.SelectObj(1);

                        UnionRegion.Dispose();
                        Rectangle.Dispose();
                    }
                    break;
                case EnumGeometryType.RECTANGLE2:
                    {
                        HOperatorSet.DrawRectangle2Mod(WindowHandle, 100,100,0,100,100, out HTuple row, out HTuple col, out HTuple phi, out  HTuple L1, out HTuple L2);
                        HOperatorSet.GenRectangle2(out HObject Rectangle, row, col, phi, L1,L2);

                        HOperatorSet.Union2(GeometryRegion, Rectangle, out HObject UnionRegion);
                        GeometryRegion = UnionRegion.SelectObj(1);

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
           
            GeometryPose[0] = rowSection;
            GeometryPose[1] = colSection;
            GeometryPose[2] = angle;

            HOperatorSet.SetColor(WindowHandle,"green");
            HOperatorSet.DispObj(GeometryRegion, WindowHandle);
            HOperatorSet.DispRegion(GeometryRegion, WindowHandle);       
        }

        public void GetGeometryRegionBy2Lines(HObject region ,HTuple rowStart, HTuple colStart, HTuple rowEnd, HTuple colEnd, HTuple rowStart1,
            HTuple colStart1, HTuple rowEnd1, HTuple colEnd1, HTuple GeometryPose, out HObject regionOut)
        {
            regionOut = new HObject();
            //原始数据
            HTuple originRow = GeometryPose[0];
            HTuple originCol = GeometryPose[1];
            HTuple originAngle= GeometryPose[2];

            //计算新的数据
            HOperatorSet.IntersectionLl(rowStart, colStart, rowEnd, colEnd, rowStart1, colStart1, rowEnd1, colEnd1,
                                       out HTuple rowSection, out HTuple colSection, out HTuple isParallel);
            HOperatorSet.AngleLx(rowStart, colStart, rowEnd, colEnd, out HTuple angle);
            HOperatorSet.VectorAngleToRigid(originRow, originCol, originAngle, rowSection, colSection, angle, out HTuple homMat2D);
            //投影变换
            HOperatorSet.AffineTransRegion(region, out regionOut, homMat2D, "false");
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
