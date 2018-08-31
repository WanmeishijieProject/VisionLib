using System;
using System.Collections.Generic;
using System.IO.Ports;
using JPT_TosaTest.Communication;
using JPT_TosaTest.Config.HardwareManager;
using JPT_TosaTest.MotionCards;

namespace JPT_TosaTest.IOCards
{
    public class IO_IrixiEE0017 : IOBase
    {
        private List<byte> FrameRecvByteList = new List<byte>();

        private Comport comport = null;
        private CommandStruct CommandSetDout = new CommandStruct();
        private CommandStruct CommandReadDout = new CommandStruct();
        private CommandStruct CommandReadDin = new CommandStruct();

        public override bool Deinit()
        {
            return true;
        }

        public override bool Init(IOCardCfg ioCfg, ICommunicationPortCfg communicationPortCfg)
        {
            this.ioCfg = ioCfg;
            comport = CommunicationMgr.Instance.FindPortByPortName(ioCfg.PortName) as Comport;
            if (comport == null)
                return false;
            else
            {
                comport.OnDataReceived += Comport_OnDataReceived;
                try
                {
                    if (this.ioCfg.NeedInit)
                    {
                        comport.OpenPort();
                        return comport.IsOpen();
                    }
                    else
                        return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public override bool ReadIoInBit(int Index, out bool value)
        {
            throw new NotImplementedException();
        }

        public override bool ReadIoInWord(int StartIndex, out int value)
        {
            value = 0;
            try
            {
                CommandReadDin.CommandType = Enumcmd.ReadDin;
                CommandReadDin.FrameLength = 4;
                byte[] cmd = CommandReadDin.ToBytes();
                comport.Write(cmd, 0, cmd.Length);
                bool bRet=CommandReadDin.SyncEvent.WaitOne(1000);
                value = (int)CommandReadDin.ReturnObj;
                return bRet;
            }
            catch
            {
                return false;
            }
        }

        public override bool ReadIoOutBit(int Index, out bool value)
        {
            throw new NotImplementedException();
        }

        public override bool ReadIoOutWord(int StartIndex, out int value)
        {
            value = 0;
            try
            {
                CommandReadDout.CommandType = Enumcmd.ReadDout;
                CommandReadDout.FrameLength = 4;
                byte[] cmd = CommandReadDout.ToBytes();
                comport.Write(cmd, 0, cmd.Length);
                bool bRet = CommandReadDout.SyncEvent.WaitOne(1000);
                value = (int)CommandReadDout.ReturnObj;
                return bRet;
            }
            catch
            {
                return false;
            }
        }

        public override bool WriteIoOutBit(int Index, bool value)
        {
            try
            {
                CommandSetDout.CommandType = Enumcmd.SetDout;
                CommandSetDout.FrameLength = 6;
                CommandSetDout.GPIOChannelFlags = (byte)Index;
                CommandSetDout.GPIOStatus = value ? (byte)1 : (byte)0;
                byte[] cmd = CommandSetDout.ToBytes();
                comport.Write(cmd, 0, cmd.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override bool WriteIoOutWord(int StartIndex, ushort value)
        {
            try
            {
                for (int i = StartIndex; i < StartIndex+8; i++)
                {
                    CommandSetDout.CommandType = Enumcmd.SetDout;
                    CommandSetDout.FrameLength = 6;
                    CommandSetDout.GPIOChannelFlags = (byte)StartIndex;
                    CommandSetDout.GPIOStatus = (byte)((value>>(i- StartIndex)) & 0x01);
                    byte[] cmd = CommandSetDout.ToBytes();
                    comport.Write(cmd, 0, cmd.Length);
                }
                return true;
            }
            catch
            {
                return false;
            }
           
        }

        private void Comport_OnDataReceived(SerialPort sp)
        {
            int n = sp.BytesToRead;
            byte[] recvTemp = new byte[n];
            comport.Read(recvTemp, 0, n);
            FrameRecvByteList.AddRange(recvTemp);//连续接收
            while (FrameRecvByteList.Count > 3)  //寻找包头  7E + L + H //有包头与长度
            {
                if (FrameRecvByteList[0] == 0x7E)
                {
                    int Framelength = FrameRecvByteList[1] + (FrameRecvByteList[2] << 8);
                    if (FrameRecvByteList.Count < Framelength + 4)   //7E+ L + H + SUM 共四个字节
                    {
                        break;  //没有接收完继续接收，否则就解析数据
                    }
                    byte[] byteRecvShort = new byte[Framelength + 4];
                    FrameRecvByteList.CopyTo(0, byteRecvShort, 0, Framelength + 4);
                    byte cmd = byteRecvShort[6];
                    byte sum = byteRecvShort[byteRecvShort.Length - 1];
                    int CalcSum = CheckSum(byteRecvShort, 0, byteRecvShort.Length - 1);
                    if (sum == CalcSum) //和校验成功
                    {
                        switch (cmd)
                        {
                            case (byte)Enumcmd.ReadDin:
                                //7E 04 00 4D FF FF 0C CC
                                //7E 05 00 69 AD 03 0C 00 A8   return
                                CommandReadDin.ReturnObj = byteRecvShort[7];
                                CommandReadDin.SyncEvent.Set();
                                break;
                            case (byte)Enumcmd.ReadDout:
                                CommandReadDout.ReturnObj = byteRecvShort[7];
                                CommandReadDout.SyncEvent.Set();
                                break;

                        }
                        FrameRecvByteList.RemoveRange(0, Framelength + 4);   //清空Buff
                    }   //和校验
                    else
                    {
                        FrameRecvByteList.RemoveAt(0);  //如果和校验不成功丢掉
                    }
                }
                else
                {
                    FrameRecvByteList.RemoveAt(0);
                }
            }

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
