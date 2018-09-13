using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using JPT_TosaTest.Classes;
using JPT_TosaTest.Config;
using JPT_TosaTest.Model;
using JPT_TosaTest.MotionCards;
using System;
using AxisParaLib;

namespace JPT_TosaTest.ViewModel
{
    public class TeachBoxViewModel : ViewModelBase
    {
        public TeachBoxViewModel()
        {
      
        }

    
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
        private void ShowError(string msg)
        {
            Messenger.Default.Send<string>(msg,"Error");
        }
        #endregion

    }
}
