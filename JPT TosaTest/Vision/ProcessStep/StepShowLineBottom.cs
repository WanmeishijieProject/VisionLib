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
        public List<Tuple<double, double, double, double>> Lines {get;set;}
        public override bool Process()
        {
            try
            {
                //转换像素与实际关系
                double PadOffset = Config.ConfigMgr.Instance.ProcessData.PadOffset / PixGainFactor;
                List<Tuple<HTuple, HTuple, HTuple, HTuple>> TupleList = new List<Tuple<HTuple, HTuple, HTuple, HTuple>>();
                foreach (var it in Lines)
                {
                    TupleList.Add(new Tuple<HTuple, HTuple, HTuple, HTuple>(it.Item1, it.Item2, it.Item3, it.Item4));
                }
                HalconVision.Instance.DisplayLines(CamID, TupleList);
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
                        HalconVision.Instance.DisplayPolygonRegion(0, rows, cols);
                    }

                    //画最后一条平行线
                    HalconVision.Instance.GetParallelLineFromDistance(TupleList[LineNum - 1].Item1, TupleList[LineNum - 1].Item2, TupleList[LineNum - 1].Item3, TupleList[LineNum - 1].Item4, PadOffset, "row", -1, out HTuple hv_PLineRow, out HTuple hv_PLineCol,
                                                out HTuple hv_PLineRow1, out HTuple hv_PLineCol1, out HTuple k1, out HTuple b1);

                    HalconVision.Instance.DisplayLines(CamID, new List<Tuple<HTuple, HTuple, HTuple, HTuple>>() { new Tuple<HTuple, HTuple, HTuple, HTuple>(hv_PLineRow, hv_PLineCol, hv_PLineRow1, hv_PLineCol1) });
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
