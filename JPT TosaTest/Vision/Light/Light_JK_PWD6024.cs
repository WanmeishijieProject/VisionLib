using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.Globalization;
using JPT_TosaTest.Config.HardwareManager;
using JPT_TosaTest.Config;
using JKLightSourceLib;
using JKLightSourceLib.Command;

namespace JPT_TosaTest.Vision.Light
{

    public class Light_JK_PWD6024 : LightBase
    {


        private JKLightSource LightController = null;


        public Light_JK_PWD6024()
        {
            MAXCH = 1;
            MINCH = 4;
        }
        public override bool Init(LightCfg cfg, ICommunicationPortCfg communicationPort)
        {
            try
            {
                this.lightCfg = cfg;
                MAXCH = this.lightCfg.MaxChannelNo;
                MINCH = this.lightCfg.MinChannelNo;
                if (lightCfg.NeedInit)
                {
                    ComportCfg portCfg = communicationPort as ComportCfg;
                    LightController = new JKLightSource(int.Parse(portCfg.Port.ToUpper().Replace("COM","")),9600);
                    LightController.Open();
                    return true;
               
                }
                else
                    return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        public override bool Deint()
        {
            LightController.Close();
            return true;
        }
        public override bool OpenLight(int Channel, int nValue)
        {
            int nCh = Channel- MINCH + 1;
            if (nCh < 1 || nCh > 4)
                return false;
            LightController.OpenChannelLight((EnumChannel)nCh.ToString()[0], (UInt16)nValue);
            return true;
        }
        public override bool CloseLight(int Channel, int nValue)
        {
            int nCh = Channel - MINCH + 1;
            if (nCh < 1 || nCh > 4)
                return false;
            LightController.CloseChannelLight((EnumChannel)nCh.ToString()[0]);
            return true;
        }
        public override int GetLightValue(int Channel)
        {
            int nCh = Channel - MINCH + 1;
            if (nCh < 1 || nCh > 4)
                return -1;
            return (int)LightController.ReadValue((EnumChannel)nCh.ToString()[0]);
        }
        public override bool SetLightValue(int Channel,int nValue)
        {
            int nCh = Channel - MINCH + 1;
            if (nCh < 1 || nCh > 4)
                return false;
            LightController.WriteValue((EnumChannel)nCh.ToString()[0], (UInt16)nValue);
            return true;
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
