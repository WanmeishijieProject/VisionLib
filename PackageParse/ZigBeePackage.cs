using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Package
{

    public class ZigBeePackage : IPackage
    {
        protected MemoryStream stream = null;
        protected BinaryWriter writer = null;
        private CRC32 Crc32Ins = new CRC32();
        public ZigBeePackage()
        {
            ReturnObject = new object();
            Header = new byte[] { 0x7E };
            FrameLength = 0;
            APIIdentifier = 0x4D;
            FrameID = 0;
            Cmd = 0x00;
        }
        private AutoResetEvent SyncEvent = new AutoResetEvent(false);
        public byte[] Header { get; set; }
        public Int16 FrameLength { get; set; }
        protected byte APIIdentifier { get; set; }
        protected Int16 FrameID { get; set; }
        protected byte Cmd { get; set; }
        public object ReturnObject { get; set; }
        
        public String GetPackageType()
        {
            return APIIdentifier.ToString();
        }

        public byte[] ToBytes()
        {
            APIIdentifier = 0x4D;
            FrameID = 0x00;

            ResetSyncFlag();    
            byte[] data = new byte[30];

            stream = new MemoryStream(data);
            writer = new BinaryWriter(stream);

            writer.Write(Header);   //1
            writer.Write(FrameLength);  //2
            writer.Write((byte)APIIdentifier);  //1
            writer.Write(FrameID);  //2
            WriteData();
            writer.Write(Crc32Ins.Calculate(data, 0, data.Length));
            writer.Close();
            stream.Close();
            return data;
        }
        protected virtual void WriteData()
        {
            return;
        }

        public virtual ZigBeePackage ByteArrToPackage(byte[] RawData)
        {
            if (RawData != null && RawData.Length >= 7) //固定结构
            {
                this.FrameLength = (short)(RawData[1] + (RawData[2] << 8));
                this.APIIdentifier = RawData[3];
                this.FrameID = (short)(RawData[4] + (RawData[5] << 8));
                this.Cmd = RawData[6];
            }
            return this;
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        public  void SetSyncFlag()
        {
            SyncEvent.Set();
        }

        public  void ResetSyncFlag()
        {
            SyncEvent.Reset();
        }

        public bool WaitFinish(int TimeOut)
        {
            return SyncEvent.WaitOne(TimeOut);
        }



        public IPackage GetPackage(byte[] RawData)
        {
            return ByteArrToPackage(RawData);
        }
    }
}
