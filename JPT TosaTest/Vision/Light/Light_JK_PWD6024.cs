using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.Globalization;
using JPT_TosaTest.Config.HardwareManager;
using JPT_TosaTest.Config;

namespace JPT_TosaTest.Vision.Light
{

    public class Light_JK_PWD6024 : LightBase
    {

        private SerialPort Comport=null;
        private byte[] recvBuffer = new byte[255];

        public Light_JK_PWD6024()
        {
            MAXCH = 1;
            MINCH = 4;
        }
        public override bool Init(LightCfg cfg)
        {
            try
            {
                this.cfg = cfg;
                MAXCH = this.cfg.MaxChannelNo;
                MINCH = this.cfg.MinChannelNo;
                if (Comport == null)
                    Comport = new SerialPort();
                Comport.BaudRate = 9600;
                Comport.StopBits = StopBits.One;
                Comport.Parity = Parity.None;
                Comport.DataBits = 8;

                ComportCfg portCfg = null;
                foreach (var it in ConfigMgr.Instance.HardwareCfgMgr.Comports)
                {
                    if (it.PortName == cfg.PortName)
                    {
                        portCfg = it;
                        break;
                    }
                }
                if (portCfg == null)
                    return false;
                Comport.PortName = string.IsNullOrEmpty(portCfg.Port.ToUpper()) ? Comport.PortName : portCfg.Port.ToUpper();
                Comport.ReadTimeout = 1000;
                Comport.WriteTimeout = 1000;
                if (Comport.IsOpen)
                    Comport.Close();
                Comport.Open();
                return GetLightValue(MINCH)>=0;    
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        public override bool Deint()
        {
            if (Comport != null)
            {
                for (int i = 0; i < MAXCH; i++)
                {
                    CloseLight(i+1, 0);
                }
                Comport.Close();
                Comport.Dispose();
            }
            return true;
        }
        public override bool OpenLight(int Channel, int nValue)
        {
            int nCh = Channel- MINCH + 1;
            if (nCh < 1 || nCh > 4)
                return false;
            lock (_lock)
            {
                string strCmd = string.Format("#1{0}{1:X3}", nCh, nValue);
                string strCheck = ExclusiveOR(System.Text.Encoding.ASCII.GetBytes(strCmd));
                strCmd += strCheck;
                Comport.Write(strCmd);
                Thread.Sleep(30);
                int count = Comport.Read(recvBuffer, 0, 20);
                return count == 1 && recvBuffer[0] == 0x35;
            }
        }
        public override bool CloseLight(int Channel, int nValue)
        {
            int nCh = Channel - MINCH + 1;
            if (nCh < 1 || nCh > 4)
                return false;
            lock (_lock)
            {
                string strCmd = string.Format("#2{0}029", nCh);
                string strCheck = ExclusiveOR(System.Text.Encoding.ASCII.GetBytes(strCmd));
                strCmd += strCheck;
                Comport.Write(strCmd);
                Thread.Sleep(50);
                int count = Comport.Read(recvBuffer, 0, 10);
                return count == 1 && recvBuffer[0] == 0x35;
            }
        }
        public override int GetLightValue(int Channel)
        {
            int nCh = Channel - MINCH + 1;
            if (nCh < 1 || nCh > 4)
                return -1;
            short nValue = 0;
            lock (_lock)
            {
                string strCmd = string.Format("#4{0}064", nCh);
                string strCheck = ExclusiveOR(System.Text.Encoding.ASCII.GetBytes(strCmd));
                strCmd += strCheck;
                Comport.Write(strCmd);
                Thread.Sleep(30);
                int count = Comport.Read(recvBuffer, 0, 20);
                if (count == 8)
                {
                    string strValue = Encoding.ASCII.GetString(new byte[] { recvBuffer[3], recvBuffer[4], recvBuffer[5] });
                    NumberFormatInfo ni = new NumberFormatInfo();
                    strValue = "0x" + strValue;
                    nValue = Convert.ToInt16(strValue, 16);
                    return nValue;
                }
                return -1;
            }
        }
        public override bool SetLightValue(int Channel,int nValue)
        {
            int nCh = Channel - MINCH + 1;
            if (nCh < 1 || nCh > 4)
                return false;
            lock (_lock)
            {
                string strCmd = string.Format("#3{0}{1:X3}", nCh, nValue);
                string strCheck = ExclusiveOR(System.Text.Encoding.ASCII.GetBytes(strCmd));
                strCmd += strCheck;
                Comport.Write(strCmd);
                Thread.Sleep(30);
                Console.Write(strCmd);
                int count=Comport.Read(recvBuffer,0,20);
                return count == 1 && recvBuffer[0] == 0x35;
            }
        }
        private string ExclusiveOR(Byte[] data,int nStartPos=0,int nEndPos=-1)
        {
            int len = data.Length;
            if (len < 0)
                return "";
            int nSum = data[0];
            for (int i = 1; i < len; i++)
                nSum ^= data[i];
            return string.Format("{0:X2}",nSum);
        }

        public override bool IsInRange(int Channel)
        {
            int nCh = Channel - MINCH + 1;
            if (nCh < 1 || nCh > 4)
                return false;
            return Channel >= MINCH && Channel <= MAXCH; 
        }
    }
}
