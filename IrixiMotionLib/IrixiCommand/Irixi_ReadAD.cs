using Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_ReadAD : ZigBeePackage
    {
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.ReadAd);
            writer.Write(ADChannelFlags);
        }
        public override ZigBeePackage ByteArrToPackage(byte[] RawData)
        {
           return base.ByteArrToPackage(RawData);
   
        }
        public byte ADChannelFlags { get; set; }
    }
}
