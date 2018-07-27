using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.WorkFlow
{
    public class WorkFlowMgr
    {
        private WorkFlowMgr() { }
        private static readonly Lazy<WorkFlowMgr> _instance = new Lazy<WorkFlowMgr>(() => new WorkFlowMgr());
        public static WorkFlowMgr Instance
        {
            get { return _instance.Value; }
        }
        public Dictionary<string, WorkFlowBase> stationDic = new Dictionary<string, WorkFlowBase>();
        public void AddStation(string stationName, WorkFlowBase station)
        {
            if (stationName == null || station == null)
                return;
            foreach (var it in stationDic)
            {
                if (it.Key == stationName)
                    return;
            }
            station.StationIndex = stationDic.Count;
            stationDic.Add(stationName, station);
        }
        public WorkFlowBase FindStationByName(string strName)
        {
            if (strName == null)
                return null;
            foreach (var it in stationDic)
                if (it.Key == strName)
                    return it.Value;
            return null;
        }
        public WorkFlowBase FindStationByIndex(int index)
        {
            if (index < 0 || index > stationDic.Count)
                return null;
            foreach (var it in stationDic)
                if (it.Value.StationIndex == index)
                    return it.Value;
            return null;
        }
        public bool StartAllStation()
        {
            bool bRet = true;
            foreach (var it in stationDic)
                bRet &= it.Value.Start();
            return bRet;
        }
        public bool StopAllStation()
        {
            bool bRet = true;
            foreach (var it in stationDic)
                bRet &= it.Value.Stop();
            foreach (var it in stationDic)
                it.Value.WaitComplete();
            return bRet;
        }

    }
}
