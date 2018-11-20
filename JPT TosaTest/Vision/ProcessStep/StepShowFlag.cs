using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Vision.ProcessStep
{
    public class StepShowFlag : VisionProcessStepBase
    {
        /// <summary>
        /// 垂直参考线
        /// </summary>
        public Tuple<double, double, double, double> In_VLine { get; set; }

        /// <summary>
        /// 水平参考线
        /// </summary>
        public Tuple<double, double, double, double> In_HLine { get; set; }

        /// <summary>
        /// 转换矩阵的Row
        /// </summary>
        public double In_CenterRow { get; set; }

        /// <summary>
        /// 转换矩阵的Col
        /// </summary>
        public double In_CenterCol { get; set; }

        /// <summary>
        /// 转换矩阵的Phi
        /// </summary>
        public double In_Phi { get; set; }

        public HObject Out_Region { get; private set; }

        /// <summary>
        /// 保存的FlagRegion的全路径名称
        /// </summary>
        public string In_RegionFullPathFileName { get; set; }

        public override bool Process()
        {
            
            List<Tuple<HTuple, HTuple, HTuple, HTuple>> TupleList = new List<Tuple<HTuple, HTuple, HTuple, HTuple>>();
            TupleList.Add(new Tuple<HTuple, HTuple, HTuple, HTuple>(In_VLine.Item1,In_VLine.Item2,In_VLine.Item3,In_VLine.Item4));
            TupleList.Add(new Tuple<HTuple, HTuple, HTuple, HTuple>(In_HLine.Item1, In_HLine.Item2, In_HLine.Item3, In_HLine.Item4));


            HTuple GeometryPose = new HTuple();
            GeometryPose[0] = In_CenterRow;
            GeometryPose[1] = In_CenterCol;
            GeometryPose[2] = In_Phi;

            //显示直线的矩形框
            HalconVision.Instance.DisplayLines(In_CamID, TupleList);    //显示Tia的参考线
        
            //利用参考线画出原来画的区域
            HOperatorSet.ReadRegion(out HObject OldRegion, @"VisionData\ToolData\Flag.reg");

            HalconVision.Instance.GetGeometryRegionBy2Lines(In_CamID,OldRegion, TupleList[0].Item1, TupleList[0].Item2, TupleList[0].Item3, TupleList[0].Item4,
                                    TupleList[1].Item1, TupleList[1].Item2, TupleList[1].Item3, TupleList[1].Item4, GeometryPose, out HObject NewRegion);


            if (NewRegion.IsInitialized())
            {
                Out_Region = NewRegion.SelectObj(1);
                NewRegion.Dispose();
            }
            if (OldRegion.IsInitialized())
                OldRegion.Dispose();
            
            //显示结果
 
           

            return true;
            
        }
    }
}
