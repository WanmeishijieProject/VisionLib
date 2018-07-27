using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Instrument
{

    public class InstrumentMgr
    {
        private InstrumentMgr() { }
        private static readonly Lazy<InstrumentMgr> _instance = new Lazy<InstrumentMgr>(() => new InstrumentMgr());
        public static InstrumentMgr Instance
        {
            get { return _instance.Value; }
        }
        private Dictionary<string, InstrumentBase> instrumentDic = new Dictionary<string, InstrumentBase>();
        public void AddInstrument(string instrumentName, InstrumentBase instrument)
        {
            if (instrumentName == null || instrument == null)
                return;
            foreach (var it in instrumentDic)
            {
                if (it.Key == instrumentName)
                    return;
            }
            instrument.Index = instrumentDic.Count;
            instrumentDic.Add(instrumentName, instrument);
        }
        public int GetInstrumentCount() { return instrumentDic.Count; }
        public InstrumentBase FindInstrumentByName(string strName)
        {
            if (strName == null)
                return null;
            foreach (var it in instrumentDic)
                if (it.Key == strName)
                    return it.Value;
            return null;
        }
        public InstrumentBase FindInstrumentByIndex(int index)
        {
            if (index < 0 || index > instrumentDic.Count)
                return null;
            foreach (var it in instrumentDic)
                if (it.Value.Index == index)
                    return it.Value;
            return null;
        }
    }
}
