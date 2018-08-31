using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Communication
{
    public class CommunicationMgr
    {
        private CommunicationMgr()
        {

        }
        private static readonly Lazy<CommunicationMgr> _instance = new Lazy<CommunicationMgr>(() => new CommunicationMgr());
        public static CommunicationMgr Instance
        {
            get { return _instance.Value; }
        }
        private Dictionary<string, CommunicationPortBase> CommucationDic = new Dictionary<string, CommunicationPortBase>();

        public void AddCommunicationPort(string PortName, CommunicationPortBase Port)
        {
            CommucationDic.TryGetValue(PortName, out CommunicationPortBase PortExist);
            if(PortExist==null)
                CommucationDic.Add(PortName, Port);
        }
        public void RemoveCommunicationPort(string PortName)
        {
            foreach (var it in CommucationDic)
            {
                if (it.Key == PortName)
                    CommucationDic.Remove(PortName);
            }
        }
        public CommunicationPortBase FindPortByPortName(string PortName)
        {
            CommucationDic.TryGetValue(PortName, out CommunicationPortBase Port);
            return Port;
        }

    }
}
