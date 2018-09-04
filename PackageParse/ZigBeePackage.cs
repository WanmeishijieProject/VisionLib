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
        protected Int16 FrameLength { get; set; }
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
            ResetSyncFlag();
            byte[] data = new byte[30];

            stream = new MemoryStream(data);
            writer = new BinaryWriter(stream);

            writer.Write(Header);   //1
            writer.Write(FrameLength);  //2
            writer.Write((byte)APIIdentifier);  //1
            writer.Write(FrameID);  //2
            WriteData();
            writer.Write(CheckSum(data, 0, data.Length));
            writer.Close();
            stream.Close();
            int len = data.Length-4;
            data[1] = (byte)(len & 0xFF);
            data[2] = (byte)((len >> 8) & 0xFF);
            return data;
        }
        protected virtual void WriteData()
        {
            
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
