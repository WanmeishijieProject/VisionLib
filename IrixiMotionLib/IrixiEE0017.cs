using JPT_TosaTest.MotionCards.IrixiCommand;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards
{
    public class IrixiEE0017
    {
        private int AXIS_NUM = 12;
        private List<AxisArgs> AxisStateList = new List<AxisArgs>();
        private static object ComportLock = new object();   //是有问题的
        SerialPort Sp = null;
        private Queue<byte> FrameRecvByteQueue = new Queue<byte>();
        private List<short> ADCRawDataList = new List<short>();
        private static Dictionary<string, IrixiEE0017> InstanceDic = new Dictionary<string, IrixiEE0017>();
        public delegate void FrameDataRecieved(byte[] data);
        public delegate void OutputStateChanged(UInt16 OldValue, UInt16 NewValue);
        public event FrameDataRecieved OnFrameDataRecieved;
        public event OutputStateChanged OnOutputStateChanged;




        private Irixi_Home CommandHome = new Irixi_Home();
        private Irixi_Move CommandMove = new Irixi_Move();
        private Irixi_MoveTrigger CommandMoveTrigger = new Irixi_MoveTrigger();
        private Irixi_Stop CommandStop = new Irixi_Stop();
        private Irixi_GetMcsuSta CommandGetMcsuSta = new Irixi_GetMcsuSta();
        private Irixi_GetSysSta CommandSysSta = new Irixi_GetSysSta();
        private Irixi_GetMemLen CommandGetMemLen = new Irixi_GetMemLen();
        private Irixi_ReadMem CommandReadMem = new Irixi_ReadMem();
        private Irixi_ClearMem CommandClearMem = new Irixi_ClearMem();
        private Irixi_ReadAD CommandReadAd = new Irixi_ReadAD();
        
        private Irixi_ConfigAdcTrig CommandConfigAdcTrigger = new Irixi_ConfigAdcTrig();
        private Irixi_SetDout CommandSetDout = new Irixi_SetDout();
        private Irixi_ReadDout CommandReadDout = new Irixi_ReadDout();
        private Irixi_ReadDin CommandReadDin = new Irixi_ReadDin();
        private Irixi_RunBlindSearch CommandRunBlindSearch = new Irixi_RunBlindSearch();

        private IrixiEE0017()
        {
            for (int i = 0; i < AXIS_NUM; i++)
                AxisStateList.Add(new AxisArgs());
            OnFrameDataRecieved += IrixiEE0017_OnFrameDataRecieved;  
        }

        /// <summary>
        /// Just for test
        /// </summary>
        /// <param name="data"></param>
        private void IrixiEE0017_OnFrameDataRecieved(byte[] data)
        {
            byte Cmd = data[6];
            int RealLen = data.Length;
            switch (Cmd)
            {
                case (byte)Enumcmd.GetMcsuSta:      //读取状态值
                    byte AxisState = data[8];  //AxisState
                    byte Error = data[RealLen - 2];
                    Int16 Acc = (Int16)(data[RealLen - 4] + (data[RealLen - 3] << 8));
                    Int32 AbsPos = (Int32)((data[RealLen - 8]) + (data[RealLen - 7] << 8) + (data[RealLen - 6] << 16) + (data[RealLen - 5] << 24));
                    byte axisIndex = data[7];
                    lock (AxisStateList[axisIndex - 1].AxisLock)
                    {
                        AxisStateList[axisIndex - 1].IsHomed = ((AxisState >> 1) & 0x01) == 1;
                        AxisStateList[axisIndex - 1].IsBusy = ((AxisState >> 2) & 0x01) == 1;
                        AxisStateList[axisIndex - 1].ErrorCode = Error;
                        AxisStateList[axisIndex - 1].CurAbsPos = (double)AbsPos / (double)AxisStateList[axisIndex - 1].GainFactor;
                        CommandGetMcsuSta.SetSyncFlag();
                    }
                    break;
                case (byte)Enumcmd.GetMemLength:
                    UInt32 DataLengthRecv = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        DataLengthRecv += (UInt32)(data[7 + i] << (8 * i));
                    }
                    CommandGetMemLen.ReturnObject = DataLengthRecv;
                    CommandGetMemLen.SetSyncFlag();
                    break;
                case (byte)Enumcmd.ReadMem:
                    int PackageID = data[7] + (data[8] << 8);
                    int DataLength = data[9] + (data[10] << 8);
                    int nStartPos = 11;
                    for (int i = 0; i < DataLength; i++)
                    {
                        short value = (short)(data[2 * i + nStartPos] + (data[2 * i + 1 + nStartPos] << 8));
                        ADCRawDataList.Add(value);
                    }
                    CommandReadMem.ReturnObject = ADCRawDataList;
                    CommandReadMem.SetSyncFlag();
                    break;
                case (byte)Enumcmd.ReadDin:
                    CommandReadDin.ReturnObject = data[7];
                    CommandReadDin.SetSyncFlag();
                    break;
                case (byte)Enumcmd.ReadDout:
                    UInt16 OldValue = 0;
                    if (!UInt16.TryParse(CommandReadDout.ReturnObject.ToString(), out OldValue))
                        OldValue = 0;
                    OnOutputStateChanged(OldValue, data[7]);
                    CommandReadDout.ReturnObject = data[7];
                    CommandReadDout.SetSyncFlag();
                    break;
                
            }
        }

        public static IrixiEE0017 CreateInstance(string token)
        {
            InstanceDic.TryGetValue(token, out IrixiEE0017 value);
            if (value == null)
            {
                lock (ComportLock)
                {
                    InstanceDic.TryGetValue(token, out value);
                    if (value == null)
                    {
                        InstanceDic.Add(token, new IrixiEE0017());
                    }
                }
            }
            return InstanceDic[token];
        }

        public bool Init(int ComportNo)
        {
            lock (ComportLock)
            {
                Sp = new SerialPort();
                if (Sp == null)
                    return false;
                try
                {
                    Sp.DataReceived += Comport_DataReceived1; ;
                    Sp.BaudRate = 115200;
                    Sp.PortName = $"COM{ComportNo}";
                    Sp.DataBits = 8;
                    Sp.StopBits = StopBits.One;
                    Sp.Parity = Parity.None;
                    Sp.ReadTimeout = 1000;
                    Sp.WriteTimeout = 1000;
                    //Sp.ReadTimeout = portCfg.TimeOut;
                    //Sp.WriteTimeout = portCfg.TimeOut;
                    Sp.ReceivedBytesThreshold = 1;
                    if (Sp.IsOpen)
                        Sp.Close();
                    Sp.Open();
                    return Sp.IsOpen;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool Deinit()
        {
            lock (ComportLock)
            {
                try
                {
                    Sp.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public bool GetCurrentPos(int AxisNo, out double Pos)
        {
            lock (ComportLock)
            {
                Pos = 0;
                if (AxisNo > 12 || AxisNo < 1)
                {
                    return false;
                }
                if (GetMcsuState(AxisNo, out AxisArgs axisArgs))
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
        public bool Home(int AxisNo, int Dir, double Acc, double Speed1, double Speed2)    //
        {
   
            try
            {
                if (AxisNo > 12 || AxisNo < 1)
                {
                    return false;
                }
                CommandHome.AxisNo = (byte)AxisNo;
                byte[] cmd = CommandHome.ToBytes();
                this.ExcuteCmd(cmd);
                return true;
            }
            catch
            {
                return false;
            }
            

        }

        public bool IsHomeStop(int AxisNo)
        {

            if (AxisNo > 12 || AxisNo < 1)
            {
                return false;
            }
            if (GetMcsuState(AxisNo, out AxisArgs axisArgs))
            {
                if (axisArgs != null && axisArgs.ErrorCode == 0)
                {
                    return axisArgs.IsHomed && !axisArgs.IsBusy;
                }
            }
            return false;
            
        }

        public bool IsNormalStop(int AxisNo)
        {

            if (AxisNo > 12 || AxisNo < 1)
            {
                return false;
            }
            if (GetMcsuState(AxisNo, out AxisArgs axisArgs))
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
        public bool MoveAbs(int AxisNo, double Acc, double Speed, double Pos)
        {
            try
            {
                if (AxisNo > 12 || AxisNo < 1)
                {
                    return false;
                }
                var speedPuse = Speed * AxisStateList[AxisNo - 1].GainFactor;
                byte speedPer = Convert.ToByte(Math.Floor(speedPuse / 100));
                int PosTarget = Convert.ToInt32(Pos * AxisStateList[AxisNo - 1].GainFactor);
                int RelPos = 0;
                if (GetMcsuState(AxisNo, out AxisArgs axisArgs))
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
                if (AxisNo > 12 || AxisNo < 1)
                {
                    return false;
                }
                var speedPuse = Speed * AxisStateList[AxisNo - 1].GainFactor;
                byte speedPer = Convert.ToByte(Math.Floor(speedPuse / 100));


                int PosTarget = Convert.ToInt32(Pos * AxisStateList[AxisNo - 1].GainFactor);
                int RelPos = 0;
                if (GetMcsuState(AxisNo, out AxisArgs axisArgs))
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
        public bool MoveRel(int AxisNo, double Acc, double Speed, double Distance)
        {
            try
            {
                if (AxisNo > 12 || AxisNo < 1)
                {
                    return false;
                }
                var speedPuse = Speed * AxisStateList[AxisNo - 1].GainFactor;
                byte speedPer = Convert.ToByte(Math.Floor(speedPuse / 100));
                int distancePuse = Convert.ToInt32(Distance * AxisStateList[AxisNo - 1].GainFactor);

                CommandMove.AxisNo = (byte)AxisNo;
                CommandMove.Distance = distancePuse;
                CommandMove.SpeedPercent = speedPer;
                byte[] cmd = CommandMove.ToBytes();
                this.ExcuteCmd(cmd);
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
                if (AxisNo > 12 || AxisNo < 1)
                {
                    return false;
                }
                var speedPuse = Speed * AxisStateList[AxisNo - 1].GainFactor;
                byte speedPer = Convert.ToByte(Math.Floor(speedPuse / 100));
                int distancePuse = Convert.ToInt32(Distance * AxisStateList[AxisNo - 1].GainFactor);

                CommandMoveTrigger.TriggerType = TriggerType == EnumTriggerType.ADC ? Enumcmd.MoveTrigAdc : Enumcmd.MoveTrigOut;
                CommandMoveTrigger.AxisNo = (byte)AxisNo;
                CommandMoveTrigger.Distance = distancePuse;
                CommandMoveTrigger.SpeedPercent = speedPer;
                CommandMoveTrigger.TriggerInterval = Interval;
                byte[] cmd = CommandMoveTrigger.ToBytes();
                this.ExcuteCmd(cmd);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }

        public bool ReadIoInBit(int Index, out bool value)
        {
            value = false;
            bool bRet = ReadIoInWord(1, out int wordValue);
            value = (wordValue & (1 << (Index - 1))) == 1;
            return bRet;
        }

        public bool ReadIoInWord(int StartIndex, out int value)
        {
            value = 0;
            try
            {
                byte[] cmd = CommandReadDin.ToBytes();
                this.ExcuteCmd(cmd);
                bool bRet = CommandReadDin.WaitFinish(1000);
                bRet &= Int32.TryParse(CommandReadDin.ReturnObject.ToString(), out value);
                return bRet;
            }
            catch
            {
                return false;
            }
            
        }

        public bool ReadIoOutBit(int Index, out bool value)
        {

            value = false;
            try
            {
                bool bRet = ReadIoOutWord(1, out int wordValue);
                value = (wordValue & (1 << (Index-1))) != 0;
                return bRet;
            }
            catch
            {
                return false;
            }
            
        }

        public bool ReadIoOutWord(int StartIndex, out int value)
        {

            value = 0;
            try
            {
                byte[] cmd = CommandReadDout.ToBytes();
                Sp.Write(cmd, 0, cmd.Length);
                bool bRet = CommandReadDout.WaitFinish(500);
                bRet &= Int32.TryParse(CommandReadDout.ReturnObject.ToString(),out value);
                return bRet;
            }
            catch(Exception ex)
            {
                return false;
            }
            
        }

        public bool WriteIoOutBit(int Index, bool value)
        {

            try
            {
                    
                CommandSetDout.GPIOChannel = (byte)Index;
                CommandSetDout.GPIOState = value ? (byte)1 : (byte)0;
                byte[] cmd = CommandSetDout.ToBytes();
                this.ExcuteCmd(cmd);
                return true;
            }
            catch
            {
                return false;
            }
            
        }

        public bool WriteIoOutWord(int StartIndex, ushort value)
        {
           
            try
            {
                for (int i = StartIndex; i < StartIndex + 8; i++)
                {
                    CommandSetDout.GPIOChannel = (byte)StartIndex;
                    CommandSetDout.GPIOState = (byte)((value >> (i - StartIndex)) & 0x01);
                    lock (ComportLock)
                    {
                        byte[] cmd = CommandSetDout.ToBytes();
                        this.ExcuteCmd(cmd);
                    }
                }
                return true;
            }
            catch
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
            lock (ComportLock)
            {
                try
                {
                    CommandConfigAdcTrigger.ADCChannelFlags = ChannelFlags;
                    byte[] cmd = CommandConfigAdcTrigger.ToBytes();
                    this.ExcuteCmd(cmd);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool GetMemLength(out UInt32 Length)
        {
            lock (ComportLock)
            {
                Length = 0;
                try
                {
                    byte[] cmd = CommandGetMemLen.ToBytes();
                    this.ExcuteCmd(cmd);
                    CommandGetMemLen.WaitFinish(1000);
                    Length = (UInt32)CommandGetMemLen.ReturnObject;
                    return true;

                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool ReadMem(UInt32 Offset, UInt32 Length, out List<Int16> RawDataList)
        {
            lock (ComportLock)
            {
                RawDataList = null;
                try
                {
                    ADCRawDataList.Clear();
                    CommandReadMem.MemOffset = Offset;
                    CommandReadMem.MemLength = Length;
                    byte[] cmd = CommandReadMem.ToBytes();
                    Sp.Write(cmd, 0, cmd.Length);
                    CommandReadMem.WaitFinish(10000);
                    RawDataList = CommandReadMem.ReturnObject as List<Int16>;
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

        }

        public bool ClearMem()
        {
            try
            {
                byte[] cmd = CommandClearMem.ToBytes();
                Sp.Write(cmd, 0, cmd.Length);
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
                CommandReadAd.ADChannelFlags = ChannelFlags;
                byte[] cmd = CommandReadAd.ToBytes();
                this.ExcuteCmd(cmd);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
            
        }

        public bool SetCurrentPos(int AxisNo, double Pos)
        {
            if (AxisNo > 12 || AxisNo < 1)
            {
                return false;
            }
            return false;
        }

        public bool Stop()
        {
           
            try
            {
                CommandStop.AxisNo = (byte)0x00;
                byte[] cmd = CommandStop.ToBytes();
                this.ExcuteCmd(cmd);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            

        }

        public bool DoBindSearch(uint HAxis, uint VAxis, double Range, double Gap, double Speed, double Interval)
        {
          
            try
            {
                CommandRunBlindSearch.HAxisNo = (byte)HAxis;
                CommandRunBlindSearch.VAxisNo = (byte)VAxis;
                int AxisIndex = (int)HAxis;
                CommandRunBlindSearch.Range = (uint)(Range * AxisStateList[AxisIndex].GainFactor);
                CommandRunBlindSearch.Gap= (uint)(Gap * AxisStateList[AxisIndex].GainFactor);
                CommandRunBlindSearch.SpeedPercent = (byte)((Speed * AxisStateList[AxisIndex].GainFactor)/10000);
                CommandRunBlindSearch.Interval= (UInt16)(Interval * AxisStateList[AxisIndex].GainFactor);
                byte[] cmd = CommandStop.ToBytes();
                this.ExcuteCmd(cmd);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }


        #region Private
        private bool GetMcsuState(int AxisNo, out AxisArgs axisargs)
        {
          
                axisargs = null;
                try
                {
                    if (AxisNo > 12 || AxisNo < 1)
                    {
                        return false;
                    }
                    //Command
                    CommandGetMcsuSta.AxisNo = (byte)AxisNo;
                    byte[] cmd = CommandGetMcsuSta.ToBytes();
                    this.ExcuteCmd(cmd);
                    CommandGetMcsuSta.WaitFinish(1000);
                    axisargs = AxisStateList[AxisNo - 1];
                    return axisargs != null;
                }
                catch (Exception ex)
                {
                    return false;
                }
            
        }



        private void Comport_DataReceived1(object sender, SerialDataReceivedEventArgs e)
        {
            int n = Sp.BytesToRead;
            byte[] RecvTemp = new byte[n];
            Sp.Read(RecvTemp, 0, n);
            foreach(var bt in RecvTemp)
                FrameRecvByteQueue.Enqueue(bt);
            while (FrameRecvByteQueue.Count > 3)  //寻找包头  7E + L + H //有包头与长度
            {
                byte bt0 = FrameRecvByteQueue.Dequeue();
                if (bt0 == 0x7E)
                {
                    byte bt1 = FrameRecvByteQueue.Dequeue();
                    byte bt2 = FrameRecvByteQueue.Dequeue();
                    int Framelength = bt1 + (bt2 << 8);
                    if (FrameRecvByteQueue.Count < Framelength + 1)   //data + SUM
                    {
                        break;  //没有接收完继续接收，否则就解析数据
                    }
                    byte[] ByteRecvShort = new byte[Framelength + 4];  //为了计算Sum需要把头信息加上
                    ByteRecvShort[0] = bt0;
                    ByteRecvShort[1] = bt1;
                    ByteRecvShort[2] = bt2;
                    //FrameRecvByteQueue.CopyTo(ByteRecvShort, 3);    //将其放入数组
                    for (int i = 0; i < Framelength+1; i++) //全部取出一帧
                    {
                        ByteRecvShort[3 + i] = FrameRecvByteQueue.Dequeue();
                    }

                    byte Sum = ByteRecvShort[ByteRecvShort.Length - 1];
                    int CalcSum = CheckSum(ByteRecvShort, 0, ByteRecvShort.Length - 1);
                    if (Sum == CalcSum) //和校验成功
                    {
                        OnFrameDataRecieved?.Invoke(ByteRecvShort);
                    } 
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

        private void ExcuteCmd(byte[] Cmd)
        {
            lock (ComportLock)
            {
                Sp.Write(Cmd, 0, Cmd.Length);
            }
        }
        
        #endregion


    }
}
