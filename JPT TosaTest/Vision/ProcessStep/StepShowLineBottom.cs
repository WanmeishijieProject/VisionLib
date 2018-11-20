using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Vision.ProcessStep
{
    public class StepShowLineBottom : VisionProcessStepBase
    {
        public List<Tuple<double, double, double, double>> In_Lines {get;set;}
        public double In_PixGainFactor { get; set; }

        public List<Tuple<HTuple, HTuple, HTuple, HTuple>> Out_Lines { get; private set; }

        /// <summary>
        /// 多个多边形
        /// </summary>
        public List<HTuple> Out_RowsList { get; private set; }
        public List<HTuple> Out_ColsList { get; private set; }
        public override bool Process()
        {
            try
            {
                //转换像素与实际关系
                double PadOffset = Config.ConfigMgr.Instance.ProcessData.PadOffset / In_PixGainFactor;
                List<Tuple<HTuple, HTuple, HTuple, HTuple>> TupleList = new List<Tuple<HTuple, HTuple, HTuple, HTuple>>();
                foreach (var it in In_Lines)
                {
                    TupleList.Add(new Tuple<HTuple, HTuple, HTuple, HTuple>(it.Item1, it.Item2, it.Item3, it.Item4));
                }
                
                int LineNum = TupleList.Count;
                if (LineNum >= 3)
                {
                    List<Tuple<HTuple, HTuple, HTuple, HTuple>> listFinal = new List<Tuple<HTuple, HTuple, HTuple, HTuple>>();
                    for (int i = 0; i < LineNum - 1; i++)
                    {
                        HOperatorSet.IntersectionLl(TupleList[i].Item1, TupleList[i].Item2, TupleList[i].Item3, TupleList[i].Item4,
                                                TupleList[LineNum - 1].Item1, TupleList[LineNum - 1].Item2, TupleList[LineNum - 1].Item3, TupleList[LineNum - 1].Item4, out HTuple row1, out HTuple col1, out HTuple isParallel1);
                        HalconVision.Instance.GetVerticalFromDistance(row1, col1, TupleList[LineNum - 1].Item1, TupleList[LineNum - 1].Item2, TupleList[LineNum - 1].Item3, TupleList[LineNum - 1].Item4, PadOffset, "row", -1,
                                           out HTuple TargetRow1, out HTuple TargetCol1, out HTuple k, out HTuple b, out HTuple kIn, out HTuple bIn);
                        listFinal.Add(new Tuple<HTuple, HTuple, HTuple, HTuple>(row1, col1, TargetRow1, TargetCol1));
                    }
                    Out_RowsList = new List<HTuple>();
                    Out_ColsList = new List<HTuple>();

                    //HalconVision.Instance.SetRefreshWindow(In_CamID,false);
                    for (int i = 0; i < LineNum / 2; i++)
                    {
                        HTuple Out_Rows = new HTuple();
                        HTuple Out_Cols = new HTuple();
                        Out_Rows[0] = listFinal[2 * i].Item1;
                        Out_Rows[1] = listFinal[2 * i].Item3;
                        Out_Rows[2] = listFinal[2 * i + 1].Item3;
                        Out_Rows[3] = listFinal[2 * i + 1].Item1;
                        Out_Cols[0] = listFinal[2 * i].Item2;
                        Out_Cols[1] = listFinal[2 * i].Item4;
                        Out_Cols[2] = listFinal[2 * i + 1].Item4;
                        Out_Cols[3] = listFinal[2 * i + 1].Item2;
                        Out_RowsList.Add(Out_Rows);
                        Out_ColsList.Add(Out_Cols);
                        HalconVision.Instance.DisplayPolygonRegion(0, Out_Rows, Out_Cols);
                    }
                    //画最后一条平行线
                    HalconVision.Instance.GetParallelLineFromDistance(TupleList[LineNum - 1].Item1, TupleList[LineNum - 1].Item2, TupleList[LineNum - 1].Item3, TupleList[LineNum - 1].Item4, PadOffset, "row", -1, out HTuple hv_PLineRow, out HTuple hv_PLineCol,
                                                out HTuple hv_PLineRow1, out HTuple hv_PLineCol1, out HTuple k1, out HTuple b1);                
                    //平行线
                    Out_Lines = new List<Tuple<HTuple, HTuple, HTuple, HTuple>> {new Tuple<HTuple, HTuple, HTuple, HTuple> (hv_PLineRow, hv_PLineCol, hv_PLineRow1, hv_PLineCol1) };
                    HalconVision.Instance.DisplayLines(In_CamID, Out_Lines);
                   // HalconVision.Instance.SetRefreshWindow(In_CamID,true);
                    //原始输入线
                    HalconVision.Instance.DisplayLines(In_CamID, TupleList);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            
        }
    }
}
