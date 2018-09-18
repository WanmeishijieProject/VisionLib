using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using JPT_TosaTest.Classes;
using JPT_TosaTest.Config;
using JPT_TosaTest.Model;
using JPT_TosaTest.MotionCards;
using System;
using AxisParaLib;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows;
using System.Linq;

namespace JPT_TosaTest.ViewModel
{
    public class TeachBoxViewModel : ViewModelBase
    {

        private Dictionary<string,Tuple<HotKey,HotKey>> HotKeyDic = new Dictionary<string, Tuple<HotKey, HotKey>>();
        public TeachBoxViewModel()
        {
            HotKeyCollect = new ObservableCollection<HotKeyModel>();
            foreach (var it in ConfigMgr.Instance.HardwareCfgMgr.AxisSettings)
            {
                HotKeyCollect.Add(new HotKeyModel() {
                     AxisName=it.AxisName,
                     AxisNo=it.AxisNo,              
                });
            }

        }
        #region Property
        public ObservableCollection<HotKeyModel> HotKeyCollect { get; set; }
        #endregion
        #region Command
        public RelayCommand LeftCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        MotionMgr.Instance.MoveRel(3, 0, 10000, -10);
                    }
                    catch (Exception ex)
                    {
                        ShowError(ex.Message);
                    }
                });
            }
        }
        public RelayCommand RightCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        MotionMgr.Instance.MoveRel(3, 0, 10000, 10);
                    }
                    catch (Exception ex)
                    {
                        ShowError(ex.Message);
                    }
                });
            }
        }
        public RelayCommand UpCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        MotionMgr.Instance.MoveRel(2, 0, 10000, -10);
                    }
                    catch (Exception ex)
                    {
                        ShowError(ex.Message);
                    }
                });
            }
        }
        public RelayCommand DownCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        MotionMgr.Instance.MoveRel(2, 0, 10000, 10);
                    }
                    catch (Exception ex)
                    {
                        ShowError(ex.Message);
                    }
                });
            }
        }
        public RelayCommand<AxisArgs> HomeCommand
        {
            get
            {
                return new RelayCommand<AxisArgs>(args =>
                {
                    try
                    {
                        MotionMgr.Instance.Home(args.AxisNo, 0,0,0,0);
                    }
                    catch (Exception ex)
                    {
                        ShowError(ex.Message);
                    }
                });
            }
        }
        public RelayCommand<AxisArgs> BackWardCommand
        {
            get
            {
                return new RelayCommand<AxisArgs>(args =>
                {
                    try
                    {
                        if (args.MoveArgs.MoveMode == 0)
                            MotionMgr.Instance.MoveAbs(args.AxisNo, 0, args.MoveArgs.Speed, args.MoveArgs.Distance);
                        else
                            MotionMgr.Instance.MoveRel(args.AxisNo, 0, args.MoveArgs.Speed, -Math.Abs(args.MoveArgs.Distance));
                    }
                    catch (Exception ex)
                    {
                        ShowError(ex.Message);
                    }
                });
            }
        }
        public RelayCommand<AxisArgs> ForwardCommand
        {
            get
            {
                return new RelayCommand<AxisArgs>(args =>
                {
                    try
                    {
                        if (args.MoveArgs.MoveMode == 0)
                            MotionMgr.Instance.MoveAbs(args.AxisNo, 0, args.MoveArgs.Speed, args.MoveArgs.Distance);
                        else
                            MotionMgr.Instance.MoveRel(args.AxisNo, 0, args.MoveArgs.Speed, Math.Abs(args.MoveArgs.Distance));
                    }
                    catch (Exception ex)
                    {
                        ShowError(ex.Message);
                    }
                });
            }
        }

        public RelayCommand<Tuple<Window, bool>> RegisterHotKeyCommand
        {
            get { return new RelayCommand<Tuple<Window,bool>>(tuple=> {
                RegisterHotKey(tuple.Item2, tuple.Item1);
            }); }
        }
 

        private void ShowError(string msg)
        {
            Messenger.Default.Send<string>(msg,"Error");
        }

        private void RegisterHotKey(bool bRegister,System.Windows.Window win)
        {
            if (bRegister)
            {
                foreach (var it in HotKeyCollect)
                {
                    if (!string.IsNullOrEmpty(it.BackwardKeyValue) && !string.IsNullOrEmpty(it.ForwardKeyValue))
                    {
                        if (Enum.TryParse(it.BackwardKeyValue, out Keys backwardKey) && Enum.TryParse(it.ForwardKeyValue,out Keys forwardKey))
                        {
                            HotKey BackWardHotKey = new HotKey(win, HotKey.KeyFlags.MOD_NOREPEAT, backwardKey);
                            HotKey ForWardHotKey = new HotKey(win, HotKey.KeyFlags.MOD_NOREPEAT, forwardKey);
                            BackWardHotKey.OnHotKey += BackWardHotKey_OnHotKey;
                            ForWardHotKey.OnHotKey += ForWardHotKey_OnHotKey;
                            if(!HotKeyDic.ContainsKey(it.AxisName))
                                HotKeyDic.Add(it.AxisName,new Tuple<HotKey,HotKey>(BackWardHotKey,ForWardHotKey));
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
                            HotKeyModel hotkeyModel = Model.First();
                            var motion = MotionMgr.Instance.FindMotionCardByAxisIndex(hotkeyModel.AxisNo);
                            var arg = motion.AxisArgsList[hotkeyModel.AxisNo - motion.MIN_AXIS].MoveArgs;
                            MotionMgr.Instance.MoveRel(hotkeyModel.AxisNo, 0, arg.Speed, Math.Abs(arg.Distance));
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>(ex.Message,"Error");
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
                            HotKeyModel hotkeyModel = Model.First();
                            var motion = MotionMgr.Instance.FindMotionCardByAxisIndex(hotkeyModel.AxisNo);
                            var arg = motion.AxisArgsList[hotkeyModel.AxisNo - motion.MIN_AXIS].MoveArgs;
                            MotionMgr.Instance.MoveRel(hotkeyModel.AxisNo, 0, arg.Speed, -Math.Abs(arg.Distance));
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
        #endregion

    }
}
