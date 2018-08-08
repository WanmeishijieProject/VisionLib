using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Model
{
    public class TestItemModel  : INotifyPropertyChanged
    {
        private string _itemName;
        public string ItemName
        {
            set
            {
                if (_itemName != value)
                {
                    _itemName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ItemName"));
                }
            }
            get { return _itemName; }
        }

        private double _posX;
        public double PosX
        {
            set
            {
                if (_posX != value)
                {
                    _posX = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PosX"));
                }
            }
            get { return _posX; }
        }

        private double _posY;
        public double PosY
        {
            set
            {
                if (_posY != value)
                {
                    _posY = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PosY"));
                }
            }
            get { return _posY; }
        }

        private double _posZ;
        public double PosZ
        {
            set
            {
                if (_posZ != value)
                {
                    _posZ = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PosZ"));
                }
            }
            get { return _posZ; }
        }

        private double _posR;
        public double PosR
        {
            set
            {
                if (_posR != value)
                {
                    _posR = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PosR"));
                }
            }
            get { return _posR; }
        }

        private string _itemColor;
        public string ItemColor
        {
            set
            {
                if (_itemColor != value)
                {
                    _itemColor = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ItemColor"));
                }
            }
            get { return _itemColor; }
        }

        public RelayCommand TeachCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Console.WriteLine("Click");
                });
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
