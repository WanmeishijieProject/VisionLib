using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraDebugLib.Vision.Light
{
    public class LigtMgr
    {
        private LigtMgr() { }
        private static readonly Lazy<LigtMgr> _instance = new Lazy<LigtMgr>(() => new LigtMgr());
        public static LigtMgr Instance
        {
            get { return _instance.Value; }
        }

        private Dictionary<string, LightBase> LightBaseDic = new Dictionary<string, LightBase>();
        public void AddLight(string LightName, LightBase light)
        {
            if (string.IsNullOrEmpty(LightName) || light == null)
                return;
            foreach (var it in LightBaseDic)
            {
                if (it.Key == LightName)
                    return;
            }

            light.Index = LightBaseDic.Count;
            LightBaseDic.Add(LightName, light);
        }
        public int GetInstrumentCount() { return LightBaseDic.Count; }

        public LightBase FindInstrumentByName(string strName)
        {
            if (strName == null)
                return null;
            foreach (var it in LightBaseDic)
                if (it.Key == strName)
                    return it.Value;
            return null;
        }
        public LightBase FindInstrumentByControlIndex(int index)
        {
            if (index < 0 || index > LightBaseDic.Count)
                return null;
            foreach (var it in LightBaseDic)
                if (it.Value.Index == index)
                    return it.Value;
            return null;
        }
        public LightBase FindLightByChannelIndex(int Channel)
        {
            foreach (var it in LightBaseDic)
            {
                if (it.Value.IsInRange(Channel))
                    return it.Value;
            }
            return null;
        }
    }
}
