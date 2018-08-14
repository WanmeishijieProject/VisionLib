using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.ViewModel
{
    public class SettingVM : ViewModelBase
    {

        #region Commmand
        public RelayCommand SaveCommand
        {
            get
            {
                return new RelayCommand(() => {
                    Console.WriteLine("Save");
                });
            }
        }
        public RelayCommand ApplyCommand
        {
            get
            {
                return new RelayCommand(() => {
                    Console.WriteLine("Apply");
                });
            }
        }
        #endregion

    }
   
}
