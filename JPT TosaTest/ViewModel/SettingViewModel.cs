using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using JPT_TosaTest.Classes;
using JPT_TosaTest.Config;
using JPT_TosaTest.Model;
using JPT_TosaTest.MotionCards;
using JPT_TosaTest.UserCtrl;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;



namespace JPT_TosaTest.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {

        private Dictionary<string, Tuple<HotKey, HotKey>> HotKeyDic = new Dictionary<string, Tuple<HotKey, HotKey>>();
        private string ProcessDataFilePath = FileHelper.GetCurFilePathString() + "Config\\";
       


        public SettingViewModel()
        {
            HotKeyCollect = new ObservableCollection<HotKeyModel>();
            foreach (var it in ConfigMgr.Instance.HardwareCfgMgr.AxisSettings)
            {
                var MotionCard = MotionMgr.Instance.FindMotionCardByAxisIndex(it.AxisNo);
                if (MotionCard != null)
                {
                    int axisNo = MotionCard.AxisArgsList[it.AxisNo - MotionCard.MIN_AXIS].AxisNo;
                    string axisName = MotionCard.AxisArgsList[it.AxisNo - MotionCard.MIN_AXIS].AxisName;
                    string backwardKey = "";
                    string forwardKey = "";
                    if (axisName == "X")
                    {
                        backwardKey = "Left";
                        forwardKey = "Right";
                    }
                    else if (axisName == "Y2")
                    {
                        backwardKey = "Up";
                        forwardKey = "Down";
                    }  
                    HotKeyCollect.Add(new HotKeyModel()
                    {
                        AxisName = axisName,
                        AxisNo = axisNo,
                        BackwardKeyValue = backwardKey,
                        ForwardKeyValue=forwardKey
                    });
                }
                else
                {
                    HotKeyCollect.Add(new HotKeyModel());
                }
            }

            //初始化Google控件皮肤
            Swatches= new SwatchesProvider().Swatches;
            PaletteHelper paletteHelper =new PaletteHelper();
            paletteHelper.ReplacePrimaryColor(Swatches.Where(a=>a.Name== "blue").First());


            //初始化Model
            Messenger.Default.Send("", "UpdateModelFiles");
            TiaColletion = new ObservableCollection<string>();
            for (int i = 0; i < 20; i++)
                TiaColletion.Add($"Tia_{i}");
        }
        
        #region Property
        public ObservableCollection<HotKeyModel> HotKeyCollect { get; set; }
        public IEnumerable<Swatch> Swatches { get; }
        public int CenterLineOffset
        {
            get { return ConfigMgr.Instance.ProcessData.CenterLineOffset; }
            set
            {
                if (value != ConfigMgr.Instance.ProcessData.CenterLineOffset)
                {
                    ConfigMgr.Instance.ProcessData.CenterLineOffset = value;
                    RaisePropertyChanged();
                }
            }
        }
        public int PadOffset
        {
            get { return ConfigMgr.Instance.ProcessData.PadOffset; }
            set
            {
                if (value != ConfigMgr.Instance.ProcessData.PadOffset)
                {
                    ConfigMgr.Instance.ProcessData.PadOffset = value;
                    RaisePropertyChanged();
                }
            }
        }
        public string TiaModelName
        {
            get { return ConfigMgr.Instance.ProcessData.TiaModelName; }
            set
            {
                if (value != ConfigMgr.Instance.ProcessData.TiaModelName)
                {
                    ConfigMgr.Instance.ProcessData.TiaModelName = value;
                    RaisePropertyChanged();
                }
            }
        }
        public string HsgModelName
        {
            get { return ConfigMgr.Instance.ProcessData.HsgModelName; }
            set
            {
                if (value != ConfigMgr.Instance.ProcessData.HsgModelName)
                {
                    ConfigMgr.Instance.ProcessData.HsgModelName = value;
                    RaisePropertyChanged();
                }
            }
        }
        public string TiaName
        {
            get { return ConfigMgr.Instance.ProcessData.TiaModelName; }
            set
            {
                if (value != ConfigMgr.Instance.ProcessData.TiaModelName)
                {
                    ConfigMgr.Instance.ProcessData.TiaModelName = value;
                    RaisePropertyChanged();
                }
            }
        }
        public double Pressure
        {
            get { return ConfigMgr.Instance.ProcessData.Presure; }
            set
            {
                if (value != ConfigMgr.Instance.ProcessData.Presure)
                {
                    ConfigMgr.Instance.ProcessData.Presure = value;
                    RaisePropertyChanged();
                }
            }
        }


        public ObservableCollection<string> TiaColletion { get; set; }
        #endregion


        #region Commmand
        public RelayCommand SaveCommand
        {
            get
            {
                return new RelayCommand(() => {
                    try
                    {
                        ConfigMgr.Instance.SaveConfig(EnumConfigType.ProcessPara, new object[] { new object()});
                        UC_MessageBox.ShowMsgBox("保存成功", "成功", MsgType.Info);
                    }
                    catch (Exception ex)
                    {
                        UC_MessageBox.ShowMsgBox(ex.Message, "Error", MsgType.Error);
                    }
                });
            }
        }
        public RelayCommand<Tuple<Window, bool>> RegisterHotKeyCommand
        {
            get
            {
                return new RelayCommand<Tuple<Window, bool>>(tuple => {
                    RegisterHotKey(tuple.Item2, tuple.Item1);
                });
            }
        }

        /// <summary>
        /// 将压力传感器里面的值读取出来写入
        /// </summary>
        public RelayCommand CalibPressure
        {
            get { return new RelayCommand(()=> {
                Pressure = 3.6;
            }); }
        }
        #endregion

        #region private

        private void ShowError(string msg)
        {
            Messenger.Default.Send<string>(msg, "Error");
        }

        private void RegisterHotKey(bool bRegister, System.Windows.Window win)
        {
            if (bRegister)
            {
                foreach (var it in HotKeyCollect)
                {
                    if (!string.IsNullOrEmpty(it.BackwardKeyValue) && !string.IsNullOrEmpty(it.ForwardKeyValue))
                    {
                        if (Enum.TryParse(it.BackwardKeyValue, out Keys backwardKey) && Enum.TryParse(it.ForwardKeyValue, out Keys forwardKey))
                        {
                            HotKey BackWardHotKey = new HotKey(win, HotKey.KeyFlags.MOD_NOREPEAT, backwardKey);
                            HotKey ForWardHotKey = new HotKey(win, HotKey.KeyFlags.MOD_NOREPEAT, forwardKey);
                            BackWardHotKey.OnHotKey += BackWardHotKey_OnHotKey;
                            ForWardHotKey.OnHotKey += ForWardHotKey_OnHotKey;
                            if (!HotKeyDic.ContainsKey(it.AxisName))
                                HotKeyDic.Add(it.AxisName, new Tuple<HotKey, HotKey>(BackWardHotKey, ForWardHotKey));
                            else
                                HotKeyDic[it.AxisName] = new Tuple<HotKey, HotKey>(BackWardHotKey, ForWardHotKey);
                        }
                    }
                }
            }
            else
            {
                foreach (var it in HotKeyDic)
                {
                    it.Value.Item1.UnRegisterHotKey();
                    it.Value.Item2.UnRegisterHotKey();
            
                }
            }
        }

        private void ForWardHotKey_OnHotKey(Keys key)
        {
            try
            {
                foreach (var it in HotKeyDic)
                {
                    if (it.Value.Item2.Key == (uint)key)
                    {
                        var Model = from models in HotKeyCollect where models.AxisName == it.Key select models;
                        if (Model != null && Model.Count() > 0)
                        {
                            SetCssThreshold(1000, 4500);
                            HotKeyModel hotkeyModel = Model.First();
                            var motion = MotionMgr.Instance.FindMotionCardByAxisIndex(hotkeyModel.AxisNo);
                            var arg = motion.AxisArgsList[hotkeyModel.AxisNo - motion.MIN_AXIS].MoveArgs;
                            MotionMgr.Instance.MoveRel(hotkeyModel.AxisNo, 100, arg.Speed, Math.Abs(arg.Distance / arg.Unit.Factor));
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>(ex.Message, "Error");
            }
        }

        private void BackWardHotKey_OnHotKey(Keys key)
        {
            try
            {
                foreach (var it in HotKeyDic)
                {
                    if (it.Value.Item1.Key == (uint)key)
                    {
                        var Model = from models in HotKeyCollect where models.AxisName == it.Key select models;
                        if (Model != null && Model.Count() > 0)
                        {
                            SetCssThreshold(1000, 4500);
                            HotKeyModel hotkeyModel = Model.First();
                            var motion = MotionMgr.Instance.FindMotionCardByAxisIndex(hotkeyModel.AxisNo);
                            var arg = motion.AxisArgsList[hotkeyModel.AxisNo - motion.MIN_AXIS].MoveArgs;
                            MotionMgr.Instance.MoveRel(hotkeyModel.AxisNo, 100, arg.Speed, -Math.Abs(arg.Distance / arg.Unit.Factor));
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>(ex.Message, "Error");
            }
        }

        private bool SetCssThreshold(UInt16 Low, UInt16 High)
        {
            return true;
            //Set sensor threshold
            Motion_IrixiEE0017 MotionCard = MotionMgr.Instance.FindMotionCardByCardName("Motion_IrixiEE0017[0]") as Motion_IrixiEE0017;
            if (MotionCard != null)
            {
                MotionCard.SetCssThreshold(MotionCards.IrixiCommand.EnumCssChannel.CSSCH1, Low, High);
                MotionCard.SetCssEnable(MotionCards.IrixiCommand.EnumCssChannel.CSSCH1, true);
                return true;
            }
            return false;
        }
        #endregion
    }

}
