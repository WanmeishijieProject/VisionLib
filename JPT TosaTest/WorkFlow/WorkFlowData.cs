using JPT_TosaTest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.WorkFlow
{
    public class WorkFlowData
    {
        private List<WFPointModel> PointList = new List<WFPointModel>();
        public void ClearPt()
        {
            PointList.Clear();
        }
        public void AddPoint(WFPointModel pt)
        {
            bool bFound = false;
            for (int i = 0; i < PointList.Count; i++)
            {
                if (PointList[i].PointName == pt.PointName)
                {
                    PointList[i] = pt;
                    bFound = true;
                    break;
                }
            }
            if (bFound)
                PointList.Add(pt);

        }
        public WFPointModel GetPoint(string Name)
        {
            var pts = from points in PointList where points.PointName == Name select points;
            if (pts.Count() != 0)
                return pts.First();
            else
                return null;
        }
    }
}
