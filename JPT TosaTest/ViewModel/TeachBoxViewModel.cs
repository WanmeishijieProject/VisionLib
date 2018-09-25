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
using AxisParaLib.UnitManager;
using Microsoft.Practices.ServiceLocation;

namespace JPT_TosaTest.ViewModel
{
    public class TeachBoxViewModel : ViewModelBase
    {

        private Dictionary<string,Tuple<HotKey,HotKey>> HotKeyDic = new Dictionary<string, Tuple<HotKey, HotKey>>();
        private UnitBase _currentLengthUint, _currentAngleUint;

        //是否需要这样做
        private MonitorViewModel monitorVM= ServiceLocator.Current.GetInstance<MonitorViewModel>();


        public TeachBoxViewModel()
        {
            HotKeyCollect = new ObservableCollection<HotKeyModel>();
            foreach (var it in ConfigMgr.Instance.HardwareCfgMgr.AxisSettings)
            {
                var MotionCard = MotionMgr.Instance.FindMotionCardByAxisIndex(it.AxisNo);
                if (MotionCard != null)
                    HotKeyCollect.Add(new HotKeyModel()
                    {
                        AxisName = MotionCard.AxisArgsList[it.AxisNo - MotionCard.MIN_AXIS].AxisName,
                        AxisNo = MotionCard.AxisArgsList[it.AxisNo - MotionCard.MIN_AXIS].AxisNo,
                    });
                else
                    HotKeyCollect.Add(new HotKeyModel());
            }


            LengthUnitCollection = new ObservableCollection<UnitBase>()
            {
                new Millimeter(),
                new Micron(),
                new Nano()
            };
            AngleUnitCollection = new ObservableCollection<UnitBase>
            {
                new Degree(),
                new Radian(),
            };
            _currentLengthUint = LengthUnitCollection[0];
            _currentAngleUint = AngleUnitCollection[0];
        }
        #region Property
        public ObservableCollection<HotKeyModel> HotKeyCollect { get; set; }
        public ObservableCollection<UnitBase> LengthUnitCollection { get; set; }
        public ObservableCollection<UnitBase> AngleUnitCollection { get; set; }
        public UnitBase CurrentLengthUint
        {
            get { return _currentLengthUint; }
            set {
                if (_currentLengthUint != value)
                {
                    foreach (var it in monitorVM.AxisStateCollection)
                    {
                        if (it.Unit.Category == value.Category)
                        {
                            it.Unit = value;
                        }
                    }
                    _currentLengthUint = value;
                    RaisePropertyChanged();
                }
            }
        }
        public UnitBase CurrentAngleUint
        {
            get { return _currentAngleUint; }
            set
            {
                if (_currentAngleUint != value)
                {
                    foreach (var it in monitorVM.AxisStateCollection)
                    {
                        if (it.Unit.Category == value.Category)
                        {
                            it.Unit = value;
                        }
                    }
                    _currentAngleUint = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Command
        public RelayCommand<AxisArgs> HomeCommand
        {
            get
            {
                return new RelayCommand<AxisArgs>(args =>
                {
                    try
                    {
                        MotionMgr.Instance.Home(args.AxisNo,0, 500,5,10);
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
                        {
                            MotionMgr.Instance.MoveAbs(args.AxisNo, 200, args.MoveArgs.Speed, args.MoveArgs.Distance/args.Unit.Factor);
                        }
                        else
                            MotionMgr.Instance.MoveRel(args.AxisNo, 200, args.MoveArgs.Speed, -Math.Abs(args.MoveArgs.Distance/args.Unit.Factor));
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
                            MotionMgr.Instance.MoveAbs(args.AxisNo, 100, args.MoveArgs.Speed, args.MoveArgs.Distance/args.Unit.Factor);
                        else
                            MotionMgr.Instance.MoveRel(args.AxisNo, 100, args.MoveArgs.Speed, Math.Abs(args.MoveArgs.Distance/args.Unit.Factor));
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
        public RelayCommand WindowLoadCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                   //To do load HotKey
                });
            }
        }
        public RelayCommand WindowClosingCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //To do save HotKey
                });
            }
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
                            MotionMgr.Instance.MoveRel(hotkeyModel.AxisNo, 100, arg.Speed, Math.Abs(arg.Distance/arg.Unit.Factor));
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
                            HotKeyModel hotkeyModel = Model.First();
                            var motion = MotionMgr.Instance.FindMotionCardByAxisIndex(hotkeyModel.AxisNo);
                            var arg = motion.AxisArgsList[hotkeyModel.AxisNo - motion.MIN_AXIS].MoveArgs;
                            MotionMgr.Instance.MoveRel(hotkeyModel.AxisNo, 100, arg.Speed, -Math.Abs(arg.Distance/arg.Unit.Factor));
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
