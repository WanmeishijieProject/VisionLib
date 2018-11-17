using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
namespace JPT_TosaTest.Vision.ProcessStep
{
    public class StepShowLineTop : VisionProcessStepBase
    {
        public Tuple<double, double, double, double> Line1 { get; set; }
        public Tuple<double, double, double, double> Line2 { get; set; }
        

        public override bool Process()
        {
            try
            {
                HTuple SelectLineIndex = 0;
                double CenterOffset = (Config.ConfigMgr.Instance.ProcessData.CenterLineOffset / PixGainFactor);
                List<Tuple<HTuple, HTuple, HTuple, HTuple>> TupleList = new List<Tuple<HTuple, HTuple, HTuple, HTuple>>();
                TupleList.Add(new Tuple<HTuple, HTuple, HTuple, HTuple>(Line1.Item1, Line1.Item2, Line1.Item3, Line1.Item4));
                TupleList.Add(new Tuple<HTuple, HTuple, HTuple, HTuple>(Line2.Item1, Line2.Item2, Line2.Item3, Line2.Item4));
                HalconVision.Instance.DisplayLines(CamID, TupleList);
                if (TupleList.Count >= 2)
                {
                    if (TupleList[0].Item1 > 1000)
                        SelectLineIndex = 0;
                    else
                        SelectLineIndex = 1;
                    HalconVision.Instance.GetParallelLineFromDistance(TupleList[SelectLineIndex].Item1, TupleList[SelectLineIndex].Item2, TupleList[SelectLineIndex].Item3, TupleList[SelectLineIndex].Item4,
                    CenterOffset, "row", -1, out HTuple hv_LineOutRow, out HTuple hv_LineOutCol, out HTuple hv_LineOutRow1, out HTuple hv_LineOutCol1,
                    out HTuple hv_k, out HTuple hv_b);
                    if (hv_LineOutRow != null && hv_LineOutRow1 != null)
                    {
                        HalconVision.Instance.DisplayLines(CamID, new List<Tuple<HTuple, HTuple, HTuple, HTuple>>() { new Tuple<HTuple, HTuple, HTuple, HTuple>(hv_LineOutRow, hv_LineOutCol, hv_LineOutRow1, hv_LineOutCol1) });
                    }
                    return true;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
            
        }
    }
}
