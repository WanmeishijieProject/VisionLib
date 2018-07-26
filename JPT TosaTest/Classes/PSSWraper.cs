using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Classes
{
    public class PSSWraper
    {
        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "GetDllInfo")]
        public static extern void GetDllInfo(StringBuilder Name, StringBuilder Version);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "GetLIV_5_DllInfo")]
        public static extern void GetLIV_5_DllInfo(StringBuilder Name, StringBuilder Version);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "GetLIV_TF_DllInfo")]
        public static extern void GetLIV_TF_DllInfo(StringBuilder Name, StringBuilder Version);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3CommunicationLink")]
        public static extern void LIV3CommunicationLink(string ComName, UInt32 Rate, ref int Error);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3SystemCorrectSet")]
        public static extern void LIV3SystemCorrectSet(double PowerSysFact, double VoltageSysFact, double ImSysFact, double IthSysFact);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3SystemCorrectGet")]
        public static extern void LIV3SystemCorrectGet(ref double PowerSysFact, ref double VoltageSysFact, ref double ImSysFact, ref double IthSysFact);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3SetTECCtrlTemp")]
        public static extern void LIV3SetTECCtrlTemp(double Temp, ref UInt32 Error);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3ReadTECTemp")]
        public static extern void LIV3ReadTECTemp(ref double Temp, ref UInt32 Error);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3ConfLDProVol")]
        public static extern void LIV3ConfLDProVol(double LDProVol, ref UInt32 Error);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIVConfEAProCur")]
        public static extern void LIVConfEAProCur(double PosiProCur, double NegaProCur, ref UInt32 Error);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3ConfEAScanMode")]
        public static extern void LIV3ConfEAScanMode(UInt32 EAScanMode, ref UInt32 Error);

        //
        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3EAScanTest")]
        public static extern int LIV3EAScanTest(double VolStart, double VolStep, double VolStop, double LDCur, UInt32 PowerUnitCalcVinf, ref double EAVol, ref double EAPower, ref UInt32 Error);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3GetEAVinf")]
        public static extern void LIV3GetEAVinf(UInt32 VinfDiffPointCont, ref double Vinf, ref UInt32 Error);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3GetPeaAtVea")]
        public static extern void  LIV3GetPeaAtVea(double Vea, ref double Pea);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3TestIdp")]
        public static extern UInt32 LIV3TestIdp(double Vpd, ref double Idp);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3ConfScanMode")]
        public static extern void LIV3ConfScanMode(UInt32 ScanMode, ref UInt32 Error);

        //
        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3IthTest")]
        public static extern double LIV3IthTest(double StartCur, double StopCur, double StepCur, double Vea, UInt32 LDWaveLength, UInt32 LDType, UInt32 TestMode, UInt32 Ithmethod, ref double IthTestCond, ref UInt32 Error);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3FixedCurrentTest")]
        public static extern void LIV3FixedCurrentTest(double FixedCurrent, ref double Power, ref double Voltage, ref double Im);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3FixedPowerTest")]
        public static extern void LIV3FixedPowerTest(double FixedPower, ref double Current, ref double Voltage, ref double Im);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3FixedImTest")]
        public static extern void LIV3FixedImTest(double FixedIm, ref double Current, ref double Power, ref double Voltage);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3LdRsTest")]
        public static extern double LIV3LdRsTest(double CurrentPoint);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3LdPslopTest")]
        public static extern double LIV3LdPslopTest(double CurrentPoint1, double CurrentPoint2);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3LdKinkTest")]
        public static extern UInt32 LIV3LdKinkTest(UInt32 Mode, ref double TestCondition, ref double KinkInformation);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3ScanResult")]
        public static extern UInt32 LIV3ScanResult(ref double Power, ref double Current, ref double Voltage, ref double Im);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3TETest")]
        public static extern double LIV3TETest(UInt32 TestMode, double Power_TestPoint, double Im_TestPoint);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3ConstantStart")]
        public static extern void LIV3ConstantStart(UInt32 LDWaveLength, UInt32 LDType, double DCCur, double Vea,ref double Power, ref double Vol, ref double Im, ref UInt32 Error);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3SetTxPowerCalData")]
        public static extern void LIV3SetTxPowerCalData(double PowerUserRead, double PowerDeviceRead);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3SetTxVoltageCalData")]
        public static extern void LIV3SetTxVoltageCalData(double VoltageUserRead, double VoltageDeviceRead);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3SetTxImCalData")]
        public static extern void LIV3SetTxImCalData(double ImUserRead, double ImDeviceRead);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3CalDataGet")]
        public static extern void LIV3CalDataGet(ref double PowerCal, ref double VoltageCal, ref double ImCal);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3CalDataSet")]
        public static extern void LIV3CalDataSet(double PowerCal, double VoltageCal, double ImCal);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3OutputDCCur")]
        public static extern void LIV3OutputDCCur(UInt32 LDWaveLength, UInt32 LDType, double DCCur, double Vea, ref UInt32 Error);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3GetDCTestData")]
        public static extern void LIV3GetDCTestData(ref double Power, ref double Vol, ref double Im, ref UInt32 Error);

        [DllImport("PSS_LIV-EML_Test_DLL.dll", EntryPoint = "LIV3CommunicationLinkClose")]
        public static extern void LIV3CommunicationLinkClose();

    }
}
