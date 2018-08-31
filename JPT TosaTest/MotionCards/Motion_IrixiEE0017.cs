using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IrixiStepperControllerHelper;
using JPT_TosaTest.Communication;
using JPT_TosaTest.Config.HardwareManager;

namespace JPT_TosaTest.MotionCards
{
    public class Motion_IrixiEE0017 : MotionBase
    {
        private int AXIS_NUM = 12;

        private List<AxisArgs> AxisStateList = new List<AxisArgs>();
        private object ComportLock = new object();
        Comport comport = null;
        private List<byte> FrameRecvByteList = new List<byte>();
        private List<short> ADCRawDataList = new List<short>();

        private CommandStruct CommandHome = new CommandStruct();
        private CommandStruct CommandMove = new CommandStruct();
        private CommandStruct CommandMoveTrigger = new CommandStruct();
        private CommandStruct CommandStop = new CommandStruct();
        private CommandStruct CommandGetMcsuSta = new CommandStruct();
        private CommandStruct CommandSysSta = new CommandStruct();
        private CommandStruct CommandGetMemLen = new CommandStruct();
        private CommandStruct CommandReadMem = new CommandStruct();
        private CommandStruct CommandClearMem = new CommandStruct();
        private CommandStruct CommandReadAd = new CommandStruct();
        private CommandStruct CommandDin = new CommandStruct();
        private CommandStruct CommandConfigAdcTrigger = new CommandStruct();


        // public delegate void OnOneFrameArrived(Enumcmd cmd, );


        public Motion_IrixiEE0017()
        {
            for (int i = 0; i < AXIS_NUM; i++)
                AxisStateList.Add(new AxisArgs());
        }

        public override bool Init(MotionCardCfg motionCfg, ICommunicationPortCfg communicationPortCfg)
        {
            this.motionCfg = motionCfg;
            MAX_AXIS = motionCfg.MaxAxisNo;
            MIN_AXIS = motionCfg.MinAxisNo;
            comport = CommunicationMgr.Instance.FindPortByPortName(motionCfg.PortName) as Comport;
            if (comport == null)
                return false;
            else
            {
                comport.OnDataReceived += Comport_OnDataReceived;
                try
                {
                    if (this.motionCfg.NeedInit)
                    {
                        comport.OpenPort();
                    }
                    else
                        return true;

                    GetCurrentPos(MIN_AXIS, out double pos);    //先读一下看看是否有反应

                    return comport.IsOpen();
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }


        public override bool Deinit()
        {
            try
            {
                comport.ClosePort();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override bool GetCurrentPos(int AxisNo, out double Pos)
        {
            Pos = 0;
            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            int axisIndex = AxisNo - MIN_AXIS + 1;
            if (GetMcsuState(axisIndex, out AxisArgs axisArgs))
            {
                if (axisArgs != null && axisArgs.ErrorCode == 0)
                {
                    Pos = axisArgs.CurAbsPos;
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 回原点
        /// </summary>
        /// <param name="AxisNo"></param>
        /// <param name="Dir"></param>
        /// <param name="Acc"></param>
        /// <param name="Speed1"></param>
        /// <param name="Speed2"></param>
        /// <returns></returns>
        public override bool Home(int AxisNo, int Dir, double Acc, double Speed1, double Speed2)    //
        {
            try
            {
                if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
                {
                    return false;
                }
                int axisIndex = AxisNo - MIN_AXIS + 1;


                CommandHome.CommandType = Enumcmd.Home;
                CommandHome.FrameLength = 5;
                CommandHome.AxisNo = (byte)axisIndex;

                byte[] cmd = CommandHome.ToBytes();
                comport.Write(cmd, 0, cmd.Length);
                return true;
            }
            catch
            {
                return false;
            }

        }


        public override bool IsHomeStop(int AxisNo)
        {

            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            int axisIndex = AxisNo - MIN_AXIS + 1;
            if (GetMcsuState(axisIndex, out AxisArgs axisArgs))
            {
                if (axisArgs != null && axisArgs.ErrorCode == 0)
                {
                    return axisArgs.IsHomed && !axisArgs.IsBusy;
                }
            }
            return false;

        }

        public override bool IsNormalStop(int AxisNo)
        {
            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            int axisIndex = AxisNo - MIN_AXIS + 1;
            if (GetMcsuState(axisIndex, out AxisArgs axisArgs))
            {
                if (axisArgs != null && axisArgs.ErrorCode == 0)
                {
                    return axisArgs.IsBusy == false;
                }
            }
            return false;

        }

        /// <summary>
        /// 绝对运动
        /// </summary>
        /// <param name="AxisNo">映射到实际的轴号</param>
        /// <param name="Acc">绝对运动加速度</param>
        /// <param name="Speed">速度</param>
        /// <param name="Pos">绝对位置</param>
        /// <returns></returns>
        public override bool MoveAbs(int AxisNo, double Acc, double Speed, double Pos)
        {
            try
            {
                if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
                {
                    return false;
                }
                int axisIndex = AxisNo - MIN_AXIS + 1;
                var speedPuse = Speed * AxisStateList[AxisNo - 1].GainFactor;
                byte speedPer = Convert.ToByte(Math.Floor(speedPuse / 100));
                int PosTarget = Convert.ToInt32(Pos * AxisStateList[AxisNo - 1].GainFactor);
                int RelPos = 0;
                if (GetMcsuState(axisIndex, out AxisArgs axisArgs))
                {
                    if (axisArgs != null)
                    {
                        lock (axisArgs.AxisLock)
                        {
                            if (axisArgs.ErrorCode == 0)
                            {
                                RelPos = PosTarget - Convert.ToInt32(axisArgs.CurAbsPos * axisArgs.GainFactor);
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
                return MoveRel(AxisNo, Acc, Speed, RelPos);
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        /// <summary>
        /// 绝对运动,Trigger
        /// </summary>
        /// <param name="AxisNo">映射到实际的轴号</param>
        /// <param name="Acc">绝对运动加速度</param>
        /// <param name="Speed">速度</param>
        /// <param name="Pos">绝对位置</param>
        /// <returns></returns>
        public bool MoveAbs(int AxisNo, double Acc, double Speed, double Pos, EnumTriggerType TriggerType, UInt16 Interval)
        {

            try
            {
                if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
                {
                    return false;
                }
                int axisIndex = AxisNo - MIN_AXIS + 1;
                var speedPuse = Speed * AxisStateList[AxisNo - 1].GainFactor;
                byte speedPer = Convert.ToByte(Math.Floor(speedPuse / 100));


                int PosTarget = Convert.ToInt32(Pos * AxisStateList[AxisNo - 1].GainFactor);
                int RelPos = 0;
                if (GetMcsuState(axisIndex, out AxisArgs axisArgs))
                {
                    if (axisArgs != null)
                    {
                        lock (axisArgs.AxisLock)
                        {
                            if (axisArgs.ErrorCode == 0)
                            {
                                RelPos = PosTarget - Convert.ToInt32(axisArgs.CurAbsPos * axisArgs.GainFactor);
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
                return MoveRel(AxisNo, Acc, Speed, RelPos, TriggerType, Interval);
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        /// <summary>
        /// 相对运动
        /// </summary>
        /// <param name="AxisNo">映射到实际的轴</param>
        /// <param name="Acc">加速度</param>
        /// <param name="Speed">速度</param>
        /// <param name="Distance">相对距离</param>
        /// <returns></returns>
        public override bool MoveRel(int AxisNo, double Acc, double Speed, double Distance)
        {
            try
            {
                if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
                {
                    return false;
                }
                int axisIndex = AxisNo - MIN_AXIS + 1;
                var speedPuse = Speed * AxisStateList[AxisNo - 1].GainFactor;
                byte speedPer = Convert.ToByte(Math.Floor(speedPuse / 100));
                int distancePuse = Convert.ToInt32(Distance * AxisStateList[axisIndex - 1].GainFactor);

                CommandMove.CommandType = Enumcmd.Move;
                CommandMove.FrameLength = 10;
                CommandMove.AxisNo = (byte)AxisNo;
                CommandMove.Distance = distancePuse;
                CommandMove.SpeedPercent = speedPer;
                byte[] cmd = CommandMove.ToBytes();
                comport.Write(cmd, 0, cmd.Length);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        /// <summary>
        /// 相对运动 Trigger
        /// </summary>
        /// <param name="AxisNo">映射到实际的轴</param>
        /// <param name="Acc">加速度</param>
        /// <param name="Speed">速度</param>
        /// <param name="Distance">相对距离</param>
        /// <returns></returns>
        public bool MoveRel(int AxisNo, double Acc, double Speed, double Distance, EnumTriggerType TriggerType, UInt16 Interval)
        {

            try
            {
                if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
                {
                    return false;
                }
                int axisIndex = AxisNo - MIN_AXIS + 1;
                var speedPuse = Speed * AxisStateList[AxisNo - 1].GainFactor;
                byte speedPer = Convert.ToByte(Math.Floor(speedPuse / 100));
                int distancePuse = Convert.ToInt32(Distance * AxisStateList[axisIndex - 1].GainFactor);

                CommandMoveTrigger.CommandType = TriggerType == EnumTriggerType.ADC ? Enumcmd.MoveTrigAdc : Enumcmd.MoveTrigOut;
                CommandMoveTrigger.FrameLength = 12;
                CommandMoveTrigger.AxisNo = (byte)AxisNo;
                CommandMoveTrigger.Distance = distancePuse;
                CommandMoveTrigger.SpeedPercent = speedPer;
                CommandMoveTrigger.TriggerInterval = Interval;
                byte[] cmd = CommandMoveTrigger.ToBytes();
                comport.Write(cmd, 0, cmd.Length);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        /// <summary>
        /// 配置Trigger
        /// </summary>
        /// <param name="ChannelFlags"></param>
        /// <returns></returns>
        public bool SetTrigConfig(byte ChannelFlags)
        {
            try
            {
                CommandConfigAdcTrigger.CommandType = Enumcmd.ConfigAdcTrigger;
                CommandConfigAdcTrigger.FrameLength = 5;
                CommandConfigAdcTrigger.ADChannelFlags = ChannelFlags;
                byte[] cmd = CommandConfigAdcTrigger.ToBytes();
                comport.Write(cmd, 0, cmd.Length);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool GetMemLength(out UInt32 Length)
        {
            Length = 0;
            try
            {
                CommandGetMemLen.CommandType = Enumcmd.GetMemLength;
                CommandGetMemLen.FrameLength = 4;
                byte[] cmd = CommandGetMemLen.ToBytes();
                comport.Write(cmd, 0, cmd.Length);
                CommandGetMemLen.SyncEvent.WaitOne(1000);
                Length = (UInt32)CommandGetMemLen.ReturnObj;
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool ReadMem(UInt32 Offset, UInt32 Length, out List<Int16> RawDataList)
        {
            RawDataList = null;

            try
            {
                ADCRawDataList.Clear();
                CommandReadMem.CommandType = Enumcmd.ReadMem;
                CommandReadMem.FrameLength = 12;
                CommandReadMem.MemOffset = Offset;
                CommandReadMem.MemLength = Length;
                byte[] cmd = CommandReadMem.ToBytes();
                comport.Write(cmd, 0, cmd.Length);
                CommandReadMem.SyncEvent.WaitOne(5000);
                RawDataList = CommandReadMem.ReturnObj as List<Int16>;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }


        }

        public bool ClearMem()
        {
            try
            {
                CommandClearMem.CommandType = Enumcmd.ClearMem;
                CommandClearMem.FrameLength = 4;
                byte[] cmd = CommandClearMem.ToBytes();
                comport.Write(cmd, 0, cmd.Length);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool ReadAD(byte ChannelFlags)
        {
            try
            {

                CommandReadAd.CommandType = Enumcmd.ReadAd;
                CommandReadAd.ADChannelFlags = ChannelFlags;
                CommandReadAd.FrameLength = 5;
                byte[] cmd = CommandReadAd.ToBytes();
                comport.Write(cmd, 0, cmd.Length);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }



        public override bool SetCurrentPos(int AxisNo, double Pos)
        {
            if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
            {
                return false;
            }
            int axisIndex = AxisNo - MIN_AXIS + 1;
            return false;
        }

        public override bool Stop()
        {
            try
            {
                CommandStop.CommandType = Enumcmd.Stop;
                CommandStop.AxisNo = (byte)0x00;
                CommandStop.FrameLength = 5;
                byte[] cmd = CommandStop.ToBytes();
                comport.Write(cmd, 0, cmd.Length);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }



        private bool GetMcsuState(int AxisNo, out AxisArgs axisargs)
        {
            lock (ComportLock)
            {
                axisargs = null;
                try
                {
                    if (AxisNo > MAX_AXIS || AxisNo < MIN_AXIS)
                    {
                        return false;
                    }
                    int axisIndex = AxisNo - MIN_AXIS + 1;

                    //Command
                    CommandGetMcsuSta.CommandType = Enumcmd.GetMcsuSta;
                    CommandGetMcsuSta.AxisNo = (byte)AxisNo;
                    CommandGetMcsuSta.FrameLength = 5;
                    byte[] cmd = CommandGetMcsuSta.ToBytes();
                    comport.Write(cmd, 0, cmd.Length);
                    CommandGetMcsuSta.SyncEvent.WaitOne(1000);
                    axisargs = AxisStateList[axisIndex - 1];
                    return axisargs != null;
                }
                catch (Exception ex)
                {
                    return false;
                }
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
                            case (byte)Enumcmd.GetMcsuSta:      //读取状态值
                                byte AxisState = byteRecvShort[Framelength - 5];
                                byte Error = byteRecvShort[Framelength + 2];
                                Int16 Acc = (Int16)(byteRecvShort[Framelength] + (byteRecvShort[Framelength + 1] << 8));
                                Int32 AbsPos = (Int32)((byteRecvShort[Framelength - 4]) + (byteRecvShort[Framelength - 3] << 8) + (byteRecvShort[Framelength - 2] << 16) + (byteRecvShort[Framelength - 1] << 24));
                                byte axisIndex = FrameRecvByteList[7];
                                lock (AxisStateList[axisIndex - 1].AxisLock)
                                {
                                    AxisStateList[axisIndex - 1].IsHomed = ((AxisState >> 1) & 0x01) == 1;
                                    AxisStateList[axisIndex - 1].IsBusy = ((AxisState >> 2) & 0x01) == 1;
                                    AxisStateList[axisIndex - 1].ErrorCode = Error;
                                    AxisStateList[axisIndex - 1].CurAbsPos = (double)AbsPos / (double)AxisStateList[axisIndex - 1].GainFactor;
                                    CommandGetMcsuSta.SyncEvent.Set();
                                }
                                break;
                            case (byte)Enumcmd.GetMemLength:
                                UInt32 DataLengthRecv = 0;
                                for (int i = 0; i < 4; i++)
                                {
                                    DataLengthRecv += (UInt32)(byteRecvShort[7 + i] << (8 * i));
                                }
                                CommandGetMemLen.ReturnObj = DataLengthRecv;
                                CommandGetMemLen.SyncEvent.Set();
                                break;
                            case (byte)Enumcmd.ReadMem:
                                int PackageID = byteRecvShort[7] + (byteRecvShort[8] << 8);
                                int DataLength = byteRecvShort[9] + (byteRecvShort[10] << 8);
                                int nStartPos = 11;
                                for (int i = 0; i < DataLength; i++)
                                {
                                    short value = (short)(byteRecvShort[2 * i + nStartPos] + (byteRecvShort[2 * i + 1 + nStartPos] << 8));
                                    ADCRawDataList.Add(value);
                                }
                                CommandReadMem.ReturnObj = ADCRawDataList;
                                CommandReadMem.SyncEvent.Set();
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
