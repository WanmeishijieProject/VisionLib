using JPT_TosaTestPAS.Config.SoftwareManager;
using JPT_TosaTest.Classes;
using JPT_TosaTest.Config.HardwareManager;
using JPT_TosaTest.Config.SoftwareManager;
using JPT_TosaTest.Config.SystemCfgManager;
using JPT_TosaTest.Config.UserManager;
using JPT_TosaTest.Instrument;
using JPT_TosaTest.IOCards;
using JPT_TosaTest.MotionCards;
using JPT_TosaTest.Vision.Light;
using JPT_TosaTest.WorkFlow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO.Ports;
using JPT_TosaTest.Communication;
using JPT_TosaTest.Config.ProcessParaManager;

namespace JPT_TosaTest.Config
{
    public enum EnumConfigType
    {
        HardwareCfg,
        SoftwareCfg,
        SystemParaCfg,
        UserCfg,
        ProcessPara,
    }
    public class ConfigMgr
    {
        private ConfigMgr()
        {

        }
        private static readonly Lazy<ConfigMgr> _instance = new Lazy<ConfigMgr>(() => new ConfigMgr());
        public static ConfigMgr Instance
        {
            get { return _instance.Value; }
        }
        private readonly string File_HardwareCfg = FileHelper.GetCurFilePathString() + "Config\\HardwareCfg.json";
        private readonly string File_SoftwareCfg = FileHelper.GetCurFilePathString() + "Config\\SoftwareCfg.json";
        private readonly string File_SystemParaCfg = FileHelper.GetCurFilePathString() + "Config\\SystemParaCfg.json";
        private readonly string File_UserCfg = FileHelper.GetCurFilePathString() + "User.json";
        private readonly string File_ProcessPara = FileHelper.GetCurFilePathString() + "Config\\ProcessPara.json";


        public  HardwareCfgManager HardwareCfgMgr = null;
        public  SoftwareCfgManager SoftwareCfgMgr = null;
        public  SystemParaCfgManager SystemParaCfgMgr =null;
        public UserCfgManager UserCfgMgr = null;
        public ProcessParaMgr ProcessDataMgr = new ProcessParaManager.ProcessParaMgr();


        //public static 
        public void LoadConfig(out List<string> errList)
        {
            #region >>>>Hardware init
            errList = new List<string>();
            try
            {
                var json_string = File.ReadAllText(File_HardwareCfg);
                HardwareCfgMgr = JsonConvert.DeserializeObject<HardwareCfgManager>(json_string);
            }
            catch (Exception ex)
            {
                errList.Add($"Unable to load config file { File_HardwareCfg}, { ex.Message}");
            }
            IMotion motionBase = null;
            IIO ioBase = null;
            InstrumentBase instrumentBase = null;
            LightBase lightBase = null;

            Type hardWareMgrType = HardwareCfgMgr.GetType();

            //先初始化通信端口
            foreach (var it in hardWareMgrType.GetProperties())
            {
                switch (it.Name)
                {

                    case "Comports":
                        foreach (var comportCfg in HardwareCfgMgr.Comports)
                        {
                            CommunicationPortBase port = new Comport(comportCfg);
                            CommunicationMgr.Instance.AddCommunicationPort(comportCfg.PortName, port);
                        }
                        break;
                    case "Ethernets":
                    case "Gpibs":
                    case "Visas":
                        break;
                    default:
                        break;
                }
            }

            foreach (var it in hardWareMgrType.GetProperties())
            {
                switch (it.Name)
                {             
                    case "MotionCards":
                        var motionCfgs = it.GetValue(HardwareCfgMgr) as MotionCardCfg[];
                        if (motionCfgs == null)
                            break;
                        foreach (var motionCfg in motionCfgs)
                        {
                            if (motionCfg.Enabled)
                            {
                                motionBase = hardWareMgrType.Assembly.CreateInstance("JPT_TosaTest.MotionCards." + motionCfg.Name.Substring(0, motionCfg.Name.IndexOf("[")), true, BindingFlags.CreateInstance, null, /*new object[] { motionCfg }*/null, null, null) as IMotion;
                                if (motionBase != null)
                                {
                                    if (motionCfg.ConnectMode.ToLower() != "none")
                                    {
                                        var p = hardWareMgrType.GetProperty($"{motionCfg.ConnectMode}s");
                                        var portCfgs = p.GetValue(HardwareCfgMgr) as ICommunicationPortCfg[];
                                        var ports = from portCfg in portCfgs where portCfg.PortName == motionCfg.PortName select portCfg;
                                        if (ports != null && ports.Count() > 0)
                                        {
                                            if (motionBase.Init(motionCfg, ports.ElementAt(0)))
                                            {
                                                //设置单位，轴类型， 软限位等
                                                for (int i = 0; i < motionBase.MAX_AXIS - motionBase.MIN_AXIS+1; i++)
                                                {
                                                    var settings = HardwareCfgMgr.AxisSettings.Where(a => a.AxisNo == i + motionBase.MIN_AXIS);
                                                    try
                                                    {
                                                        motionBase.SetAxisPara(i, settings == null ? null : settings.First());
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        errList.Add($"{ex.Message}");
                                                    }
                                                }
                                                MotionMgr.Instance.AddMotionCard(motionCfg.Name, motionBase);
                                            }
                                            else
                                                errList.Add($"{motionCfg.Name} init failed");
                                        }
                                        else
                                            errList.Add($"{motionCfg.Name} init failed");
                                    }
                                    else  //无需选择通信端口
                                    {
                                    
                                        if (motionBase.Init(motionCfg, null))
                                        {
                                            //设置单位，轴类型， 软限位等
                                            for (int i = 0; i < motionBase.MAX_AXIS - motionBase.MIN_AXIS; i++)
                                            {
                                                var settings = HardwareCfgMgr.AxisSettings.Where(a => a.AxisNo == i + motionBase.MIN_AXIS);
                                                try
                                                {
                                                    motionBase.SetAxisPara(i, settings == null ? null : settings.First());
                                                }
                                                catch (Exception ex)
                                                {
                                                    errList.Add($"{ex.Message}");
                                                }
                                            }
                                            MotionMgr.Instance.AddMotionCard(motionCfg.Name, motionBase);
                                        }
                                        else
                                            errList.Add($"{motionCfg.Name} init failed");
                                    }
                                    
                                }
                                else
                                {
                                    errList.Add($"{motionCfg.Name} Create instanse failed");
                                }
                            }
                        }
                        break;
                    case "IOCards":
                        var ioCfgs = it.GetValue(HardwareCfgMgr) as IOCardCfg[];
                        if (ioCfgs == null)
                            break;
                        foreach (var ioCfg in ioCfgs)
                        {
                            if (ioCfg.Enabled)
                            {
                                ioBase = hardWareMgrType.Assembly.CreateInstance("JPT_TosaTest.IOCards." + ioCfg.Name.Substring(0, ioCfg.Name.IndexOf("[")), true, BindingFlags.CreateInstance, null, null, null, null) as IIO;
                                if (ioBase != null)
                                {
                                    ioBase.ioCfg = ioCfg;
                                    if (ioCfg.ConnectMode.ToLower() != "none")  //没有屏蔽端口
                                    {
                                        var p = hardWareMgrType.GetProperty($"{ioCfg.ConnectMode}s");
                                        var portCfgs = p.GetValue(HardwareCfgMgr) as ICommunicationPortCfg[];
                                        var ports = from portCfg in portCfgs where portCfg.PortName == ioCfg.PortName select portCfg;
                                        if (ports != null && ports.Count() > 0)
                                        {
                                            if (ioBase.Init(ioCfg, ports.ElementAt(0)))
                                                IOCardMgr.Instance.AddIOCard(ioCfg.Name, ioBase);
                                            else
                                                errList.Add($"{ioCfg.Name} init failed");
                                        }
                                        else
                                        {
                                            errList.Add($"{ioCfg.Name} init failed");
                                        }
                                    }
                                    else  //无需选择通信端口
                                    {
                                        if (ioBase.Init(ioCfg,null))
                                            IOCardMgr.Instance.AddIOCard(ioCfg.Name, ioBase);
                                        else
                                            errList.Add($"{ioCfg.Name} init failed");
                                    }
                                }
                                else
                                {
                                    errList.Add($"{ioCfg.Name} Create instanse failed");
                                }
                            }
                        }
                        break;
                    case "Instruments":
                        var instrumentCfgs = it.GetValue(HardwareCfgMgr) as InstrumentCfg[];
                        if (instrumentCfgs == null)
                            break;
                        foreach (var instrumentCfg in instrumentCfgs)
                        {
                            if (instrumentCfg.Enabled)
                            {
                                instrumentBase = hardWareMgrType.Assembly.CreateInstance("JPT_TosaTest.Instruments." + instrumentCfg.InstrumentName.Substring(0, instrumentCfg.InstrumentName.IndexOf("[")), true, BindingFlags.CreateInstance, null, null, null, null) as InstrumentBase;
                                if (instrumentBase != null)
                                {
                                    if (instrumentBase.Init())
                                    {

                                    }
                                }
                            }
                        }
                        break;
                    case "Cameras":
                        var cameraCfgs = it.GetValue(HardwareCfgMgr) as CameraCfg[];
                        break;
                    case "Lights":
                        var lightCfgs=it.GetValue(HardwareCfgMgr) as LightCfg[];
                        foreach (var lightCfg in lightCfgs)
                        {
                            if (lightCfg.Enabled)
                            {
                                lightBase = hardWareMgrType.Assembly.CreateInstance("JPT_TosaTest.Vision.Light." + lightCfg.Name.Substring(0, lightCfg.Name.IndexOf("[")), true, BindingFlags.CreateInstance, null, null, null, null) as LightBase;
                                if (lightBase != null)
                                {  
                                    if (lightCfg.ConnectMode.ToLower() != "none")
                                    {
                                        var p = hardWareMgrType.GetProperty($"{lightCfg.ConnectMode}s");
                                        var portCfgs = p.GetValue(HardwareCfgMgr) as ICommunicationPortCfg[];
                                        var ports = from portCfg in portCfgs where portCfg.PortName == lightCfg.PortName select portCfg;
                                        if (ports != null && ports.Count() > 0)
                                        {
                                            if (lightBase.Init(lightCfg, ports.ElementAt(0))) //如果不需要初始化就直接加入字典
                                                LigtMgr.Instance.AddLight(lightCfg.Name, lightBase);
                                            else
                                                errList.Add($"{lightCfg.Name} init failed");
                                        }
                                        else
                                        {
                                            errList.Add($"{lightCfg.Name} init failed");
                                        }
                                    }
                                    else //无需选择通信端口
                                    {
                                        if (lightBase.Init(lightCfg, null))
                                            LigtMgr.Instance.AddLight(lightCfg.Name, lightBase);
                                        else
                                            errList.Add($"{lightCfg.Name} init failed");
                                    }
                                }
                                else
                                {
                                    errList.Add($"{lightCfg.Name} Create instanse failed");
                                }
                            }
                        }
                        break;
                   
                    default:
                        break;

                }
            }

            #endregion

            #region >>>>Software init
            try
            {
                var json_string = File.ReadAllText(File_SoftwareCfg);
                SoftwareCfgMgr = JsonConvert.DeserializeObject<SoftwareCfgManager>(json_string);
            }
            catch (Exception ex)
            {
                errList.Add(String.Format("Unable to load config file {0}, {1}", File_SoftwareCfg, ex.Message));
            }

            Type tStationCfg = SoftwareCfgMgr.GetType();
            PropertyInfo[] pis = tStationCfg.GetProperties();
            WorkFlowConfig[] WorkFlowCfgs = null;
            WorkFlowBase workFlowBase = null;
            foreach (PropertyInfo pi in pis)
            {
                if (pi.Name == "WorkFlowConfigs")
                {
                    WorkFlowCfgs = pi.GetValue(SoftwareCfgMgr) as SoftwareManager.WorkFlowConfig[];
                    foreach (var it in WorkFlowCfgs)
                    {
                        if (it.Enable)
                        {
                            workFlowBase = tStationCfg.Assembly.CreateInstance("JPT_TosaTest.WorkFlow." + it.Name, true, BindingFlags.CreateInstance, null, new object[] { it }, null, null) as WorkFlowBase;
                            if (workFlowBase == null)
                                errList.Add($"Station: {it.Name} Create instance failed!");
                            else
                                WorkFlowMgr.Instance.AddStation(it.Name, workFlowBase);
                        }
                    }
                }
            }
            #endregion

            #region >>>>SystemCfg
            try
            {
                var json_string = File.ReadAllText(File_SystemParaCfg);
                SystemParaCfgMgr = JsonConvert.DeserializeObject<SystemParaCfgManager>(json_string);
            }
            catch (Exception ex)
            {
                errList.Add(String.Format("Unable to load config file {0}, {1}", File_SystemParaCfg, ex.Message));
            }
            #endregion

            #region >>>> UserCfg init
            try
            {
                var json_string = File.ReadAllText(File_UserCfg);
                UserCfgMgr = JsonConvert.DeserializeObject<UserCfgManager>(json_string);
            }
            catch (Exception ex)
            {
                errList.Add(String.Format("Unable to load config file {0}, {1}", File_UserCfg, ex.Message));
            }
            #endregion

            #region >>>>ProcessPara
            //从文件中读取参数
            try
            {
                var json_string = File.ReadAllText(File_ProcessPara);
                ProcessDataMgr = JsonConvert.DeserializeObject<ProcessParaMgr>(json_string);
            }
            catch (Exception ex)
            {
                errList.Add(String.Format("Unable to load config file {0}, {1}", File_UserCfg, ex.Message));
            }
            #endregion
        }
        public void SaveConfig(EnumConfigType cfgType, object[] listObj)
        {
            if (listObj == null)
                throw new Exception(string.Format("保存的{0}数据为空", cfgType.ToString()));
            string fileSaved = null;
            object objSaved = null;
            switch (cfgType)
            {
                case EnumConfigType.HardwareCfg:
                    //fileSaved = File_HardwareCfg;
                    //objSaved=new HardwareCfgManager() {  }
                    break; 
                case EnumConfigType.SoftwareCfg:
                    fileSaved = File_SoftwareCfg;

                    break;
                case EnumConfigType.SystemParaCfg:
                    fileSaved = File_SystemParaCfg;
                    objSaved = new SystemParaCfgManager();
                    (objSaved as SystemParaCfgManager).SystemParaModels = listObj as SystemParaModel[];
                    SystemParaCfgMgr = objSaved as SystemParaCfgManager;
                    break;
                case EnumConfigType.UserCfg:
                    fileSaved = File_UserCfg;
                    objSaved = new UserCfgManager();
                    (objSaved as UserCfgManager).Users= listObj as UserModel[];
                    break;
                case EnumConfigType.ProcessPara:
                    //File.WriteAllText(File_ProcessPara, ProcessData.ToString());
                    return;
                default:
                    break;
            }
            string json_str = JsonConvert.SerializeObject(objSaved);
            File.WriteAllText(fileSaved, json_str);
        }
    }
}
