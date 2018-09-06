using DevExpress.Mvvm;
using GalaSoft.MvvmLight.Command;
using JPT_TosaTest.MotionCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JPT_TosaTest.ViewModel
{
    public class TeachBoxViewModel : ViewModelBase
    {
        #region Command
        public RelayCommand LeftCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Console.WriteLine("Left");
                    //Thread.Sleep(1000);
                    MotionMgr.Instance.MoveRel(4, 0, 10000, -10);
                });
            }
        }
        public RelayCommand RightCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Console.WriteLine("Right");
                    //Thread.Sleep(1000);
                    MotionMgr.Instance.MoveRel(4, 0, 10000, 10);
                });
            }
        }
        public RelayCommand UpCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Console.WriteLine("Up");
                    MotionMgr.Instance.Home(4, 0, 0, 0, 0);
                    //Thread.Sleep(1000);
                });
            }
        }
        public RelayCommand DownCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Console.WriteLine("Down");
                    //Thread.Sleep(1000);
                });
            }
        }
        #endregion

    }
}
