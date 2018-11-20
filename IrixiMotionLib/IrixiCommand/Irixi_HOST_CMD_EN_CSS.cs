using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_EN_CSS : ZigBeePackage
    {
        public Irixi_HOST_CMD_EN_CSS()
        {
            FrameLength = 0x07;
        }
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_EN_CSS);
            writer.Write((byte)CssChannel);
            writer.Write(IsEnable?(byte)1 : (byte)0);
        }

        public EnumCssChannel CssChannel { get; set; }
        public bool IsEnable { get; set; }

    }
}
