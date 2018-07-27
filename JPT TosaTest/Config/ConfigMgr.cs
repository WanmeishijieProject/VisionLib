using CJPT_TosaTestPAS.Config.SoftwareManager;
using GalaSoft.MvvmLight.Messaging;
using JPT_TosaTest.Classes;
using JPT_TosaTest.Config.HardwareManager;
using JPT_TosaTest.Config.SoftwareManager;
using JPT_TosaTest.Config.SystemCfgManager;
using JPT_TosaTest.Instrument;
using JPT_TosaTest.WorkFlow;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JPT_TosaTest.Config
{
    public enum EnumConfigType
    {
        HardwareCfg,
        SoftwareCfg,
        SystemParaCfg,
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


        private LogExcel logexcel = null;
        public  HardwareCfgManager HardwareCfgMgr = null;
        public  SoftwareCfgManager SoftwareCfgMgr = null;
        public  SystemParaCfgManager SystemParaCfgMgr =null;


        //public static 
        public void LoadConfig()
        {
            #region >>>>Hardware init
            try
            {
                var json_string = File.ReadAllText(File_HardwareCfg);
                HardwareCfgMgr = JsonConvert.DeserializeObject<HardwareCfgManager>(json_string);
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>(String.Format("Unable to load config file {0}, {1}", File_HardwareCfg, ex.Message), "ShowError");
                throw new Exception(ex.Message);
            }
            InstrumentBase inst = null;
            HardwareCfgLevelManager1[] instCfgs = null;
            string strClassName = "";
            Type t = HardwareCfgMgr.GetType();
            PropertyInfo[] PropertyInfos = t.GetProperties();
            for (int i = 0; i < PropertyInfos.Length; i++)
            {
                if (PropertyInfos[i].Name.ToUpper().Contains("COMPORT") || PropertyInfos[i].Name.ToUpper().Contains("ETHERNET") ||
                    PropertyInfos[i].Name.ToUpper().Contains("GPIB") || PropertyInfos[i].Name.ToUpper().Contains("NIVISA") ||
                     PropertyInfos[i].Name.ToUpper().Contains("CAMERACFG"))
                    continue;
                PropertyInfo pi = PropertyInfos[i];
                instCfgs = pi.GetValue(HardwareCfgMgr) as HardwareCfgLevelManager1[];
                strClassName = pi.Name.Substring(0, pi.Name.Length - 1);
                foreach (var it in instCfgs)
                {
                    if (!it.Enabled)
                        continue;
                    inst = t.Assembly.CreateInstance("CPAS.Instrument." + strClassName, true, BindingFlags.CreateInstance, null, new object[] { it }, null, null) as InstrumentBase;
                    if (inst != null && it.Enabled)
                    {
                        if (inst.Init())
                            InstrumentMgr.Instance.AddInstrument(it.InstrumentName, inst);
                        else
                            Messenger.Default.Send<string>(string.Format("Instrument :{0} init failed", it.InstrumentName), "ShowError");
                    }
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
                Messenger.Default.Send<string>(String.Format("Unable to load config file {0}, {1}", File_SoftwareCfg, ex.Message), "ShowError");
                throw new Exception(ex.Message);
            }

            Type tStationCfg = SoftwareCfgMgr.GetType();
            PropertyInfo[] pis = tStationCfg.GetProperties();
            WorkFlowConfig[] WorkFlowCfgs = null;
            WorkFlowBase workFlowBase = null;
            foreach (PropertyInfo pi in pis)
            {
                WorkFlowCfgs = pi.GetValue(SoftwareCfgMgr) as SoftwareManager.WorkFlowConfig[];
                foreach (var it in WorkFlowCfgs)
                {
                    if (it.Enable)
                    {
                        workFlowBase = tStationCfg.Assembly.CreateInstance("CPAS.WorkFlow." + it.Name, true, BindingFlags.CreateInstance, null, new object[] { it }, null, null) as WorkFlowBase;
                        WorkFlowMgr.Instance.AddStation(it.Name, workFlowBase);
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
                Messenger.Default.Send<string>(String.Format("Unable to load config file {0}, {1}", File_SystemParaCfg, ex.Message), "ShowError");
                throw new Exception(ex.Message);
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
                default:
                    break;
            }
            string json_str = JsonConvert.SerializeObject(objSaved);
            File.WriteAllText(fileSaved, json_str);
        }
    }
}
