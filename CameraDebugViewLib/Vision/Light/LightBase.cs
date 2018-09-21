using CameraDebugLib.Vision.LightConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraDebugLib.Vision.Light
{
    public abstract class LightBase
    {
        protected object _lock = new object();
        protected LightCfg lightCfg = null;
        protected int MAXCH, MINCH;
        public int Index = 0;
        public abstract bool Init(LightCfg cfg, ICommunicationPortCfg communicationPort);
        public abstract bool Deint();
        public abstract bool OpenLight(int nCh,int nValue=0);
        public abstract bool CloseLight(int nCh,int nValue=0);
        public abstract bool SetLightValue(int nCh,int nValue);
        public abstract int GetLightValue(int nCh);
        public abstract bool IsInRange(int nCh);
       
    }
}
