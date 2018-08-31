using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards
{
    public enum Enumcmd : byte
    {
        Home,
        Move,
        MoveTrigOut,
        MoveTrigAdc,
        Stop,
        GetMcsuSta,
        GetSysSta,
        GetMemLength,
        ReadMem,
        ClearMem,
        SetDout,
        ReadDout,
        ReadDin,
        ReadAd,
        ConfigAdcTrigger,
    }
    public enum EnumTriggerType
    {
        OUT,
        ADC
    }


    public class CommandStruct
    {
        public CommandStruct()
        {
            FrameID = 0;
            FrameLength = 0;
            Header = 0x7E;
            DataType = 0x4D;
            SyncEvent = new AutoResetEvent(false);
        }

        private int MAX_CMD_LEN = 30;
        private List<byte> cmdData = new List<byte>();
        public AutoResetEvent SyncEvent { private set; get; }


        public byte Header { get; } 
        public Int16 FrameLength { get; set; }
        public byte DataType { get; }
        public Int16 FrameID { get; set; }
        public Enumcmd CommandType { set; get; }


        public byte AxisNo { get; set; }
        public Int32 Distance { get; set; }
        public byte SpeedPercent { get; set; }
        public UInt16 TriggerInterval { get; set; }
        public byte ADChannelFlags { get; set; }
        public byte GPIOChannelFlags { get; set; }
        public byte GPIOStatus { get; set; }
        public UInt32 MemOffset { get; set; }
        public UInt32 MemLength { get; set; }
        public virtual object ReturnObj{ get; set; }

        public byte[] ToBytes()
        {
            SyncEvent.Reset();
            byte[] data = new byte[MAX_CMD_LEN];

            MemoryStream stream = new MemoryStream(data);
            BinaryWriter writer = new BinaryWriter(stream);

            // report ID
            writer.Write(Header);   //1
            writer.Write(FrameLength);  //2
            writer.Write(DataType);  //1
            writer.Write(FrameID);  //2
            writer.Write((byte)this.CommandType);   //1
            switch (this.CommandType)
            {
                case Enumcmd.Home:
                    writer.Write(AxisNo);                 
                    break;
                case Enumcmd.Move:
                    writer.Write(AxisNo);
                    writer.Write(Distance);
                    writer.Write(SpeedPercent);
                    break;
                case Enumcmd.MoveTrigAdc:
                case Enumcmd.MoveTrigOut:
                    writer.Write(AxisNo);
                    writer.Write(Distance);
                    writer.Write(SpeedPercent);
                    writer.Write(TriggerInterval);
                    break;
                case Enumcmd.Stop:
                    writer.Write(AxisNo);
                    break;
                case Enumcmd.ReadAd:
                    writer.Write(ADChannelFlags);
                    break;
                case Enumcmd.ClearMem:
                case Enumcmd.GetMemLength:
                case Enumcmd.ReadDin:
                case Enumcmd.ReadDout:
                    break;
                case Enumcmd.SetDout:
                    writer.Write(GPIOChannelFlags);
                    break;
                case Enumcmd.ReadMem:
                    writer.Write(MemOffset);
                    writer.Write(MemLength);
                    break;                
                case Enumcmd.GetMcsuSta:
                case Enumcmd.GetSysSta:
                    writer.Write(AxisNo);
                    break;
                case Enumcmd.ConfigAdcTrigger:
                    writer.Write(ADChannelFlags);
                    break;
                default:
                    break;
            }
            writer.Write(CheckSum(data, 0, data.Length));
            writer.Close();
            stream.Close();
            return data;
        }

        private byte CheckSum(byte[] buf, int offset, int count)
        {
            if (buf.Length < count)
                return 0;
            byte checksum = 0;
            for (int i = offset; i < offset + count; i++)
            {
                checksum += buf[i];
            }
            return (byte)(checksum & 0xFF);
        }
    }
}
