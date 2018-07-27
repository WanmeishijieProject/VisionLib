using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Config.HardwareManager
{
    public class HardwareCfgManager
    {
        public PowerMeteConfig[] PowerMeters { get; set; }

        public ComportCfg[] Comports { get; set; }
        public EtherNetCfg[] EtherNets { get; set; }
        public NIVasaCfg[] NIVisas { get; set; }
    }
}
