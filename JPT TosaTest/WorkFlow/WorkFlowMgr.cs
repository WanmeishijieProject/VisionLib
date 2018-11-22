using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.WorkFlow
{
    public class WorkFlowMgr : WorkFlowData
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

        public bool StartStationByIndex(int Index)
        {
            var station= FindStationByIndex(Index);
            if (station != null)
                return station.Start();
            return false;
        }
        public bool StartStationByName(string StrName)
        {
            var station = FindStationByName(StrName);
            if (station != null)
                return station.Start();
            return false;
        }
        public bool StopStationByIndex(int Index)
        {
            var station = FindStationByIndex(Index);
            if (station != null)
                return station.Stop();
            return false;
        }
        public bool StopStationByName(string StrName)
        {
            var station = FindStationByName(StrName);
            if (station != null)
                return station.Stop();
            return false;
        }
        public bool PauseStationByIndex(int Index)
        {
            var station = FindStationByIndex(Index);
            if (station != null)
            {
                return station.Pause();
            }
            return false;
        }
        public bool PauseStationByName(string StrName)
        {
            var station = FindStationByName(StrName);
            if (station != null)
                return station.Pause();
            return false;
        }
        public bool StartAllStation()
        {
            bool bRet = true;
            foreach (var it in stationDic)
                bRet &= it.Value.Start();
            return bRet;
        }
        public bool PauseAllStation()
        {
            bool bRet = true;
            foreach (var it in stationDic)
                bRet &= it.Value.Pause();
            return bRet;
        }
        public bool StopAllStation()
        {
            bool bRet = true;
            MotionCards.MotionMgr.Instance.StopAll();
            foreach (var it in stationDic)
                bRet &= it.Value.Stop();
            foreach (var it in stationDic)
                it.Value.WaitComplete();
            return bRet;
        }

    }
}
