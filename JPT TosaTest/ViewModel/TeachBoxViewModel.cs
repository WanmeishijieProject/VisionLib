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
        public RelayCommand StopAllAxisCommand { get; } = new RelayCommand(()=> {
            foreach (var it in MotionMgr.Instance.MotionDic)
            {
                it.Value.Stop();
            }
        });
   
        #endregion

        #region private
        private void ShowError(string msg)
        {
            Messenger.Default.Send<string>(msg, "Error");
        }
        #endregion
    }
}
