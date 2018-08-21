using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.HardwareManager
{
    public class HardwareCfgManager
    {
        public InstrumentCfg[] Instruments { get; set; }
        public MotionCardCfg[] MotionCards { get; set; }
        public IOCardCfg[] IOCards { get; set; }
        public CameraCfg[] Cameras { get; set; }
        public LightCfg[] Lights { get; set; }

        public ComportCfg[] Comports { get; set; }
        public EtherNetCfg[] EtherNets { get; set; }
        public NIVasaCfg[] NIVisas { get; set; }
    }
}
