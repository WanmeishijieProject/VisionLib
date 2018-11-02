using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.WorkFlow
{
    public class WorkFlowData
    {
        private Dictionary<string, List<double>> PointDic = new Dictionary<string, List<double>>();
        public void ClearPt()
        {
            PointDic.Clear();
        }
        public void AddPoint(string Name,List<double>PtList)
        {
            if (PointDic.ContainsKey(Name))
            {
                PointDic[Name] = PtList;
            }
            else
            {
                PointDic.Add(Name, PtList);
            }
        }
        public List<double> GetPoint(string Name)
        {
            if (PointDic.ContainsKey(Name))
                return PointDic[Name];
            return null;
        }
    }
}
