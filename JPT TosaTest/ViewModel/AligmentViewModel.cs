using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JPT_TosaTest.Classes.AlimentClass.AlimentArgs;
using JPT_TosaTest.Classes.AlimentClass.ScanCure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.ViewModel
{
    public class AligmentViewModel : ViewModelBase
    {
        private IAligmentArgs _aligmentArgs = null;
        private BlindSearchArgs blindSearchArgs = new BlindSearchArgs();
        private CenterAligmentArgs centerAligmentArgs = new CenterAligmentArgs();
        public AligmentViewModel()
        {
            _aligmentArgs = blindSearchArgs;
        }
        public IAligmentArgs AligmentArgs
        {
            get { return _aligmentArgs; }
            set
            {
                if (_aligmentArgs!=value)
                {
                    _aligmentArgs = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region Command
        public RelayCommand StartAlignCommand
        {
            get { return new RelayCommand(()=> {
                AligmentArgs = blindSearchArgs;
            }); }
        }
        public RelayCommand StopAlignCommand
        {
            get
            {
                return new RelayCommand(() => {
                    AligmentArgs = centerAligmentArgs;
                });
            }
        }
        #endregion

    }
}
