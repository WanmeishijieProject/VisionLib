using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Model
{
    public class ProductStateModel :ViewModelBase, INotifyPropertyChanged
    {
        private bool _isChecked = true;

        public string ProductName
        {
            get; set;

        }
        public bool IsChecked
        {
            get { return _isChecked; }
            set {
                if (value != _isChecked)
                {
                    _isChecked = value;
                    RaisePropertyChange();
                }
            }
        }
        public int ProductIndex { get; set; }

        public RelayCommand CommandSetCheckedProduct
        {
            get
            {
                return new RelayCommand (() =>
                {
                    IsChecked = !IsChecked;
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChange([CallerMemberName]string PropertyName="")
        {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(PropertyName));
        }
    }
}
