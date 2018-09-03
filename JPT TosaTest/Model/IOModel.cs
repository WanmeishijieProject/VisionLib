using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Model
{
    public enum EnumIoType
    {
        InPut,
        Output
    }
    public class IOModel : INotifyPropertyChanged
    {
        private string _ioName;
        private int _startIndex;
        private bool _isChecked;
        private EnumIoType _ioType;
        private int _index;
        private string _cardName;

        public string IOName
        {
            get { return _ioName; }
            set
            {
                if (value != _ioName)
                {
                    _ioName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IOName"));
                }
            }
        }
        public int StartIndex
        {
            get { return _startIndex; }
            set
            {
                if (value != _startIndex)
                {
                    _startIndex = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StartIndex"));
                }
            }
        }
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value != _isChecked)
                {
                    _isChecked = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked"));

                }
            }
        }
        public EnumIoType IOType
        {
            get { return _ioType; }
            set
            {
                if (value != _ioType)
                {
                    _ioType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IOType"));
                }
            }
        }

        public int Index
        {
            get { return _index; }
            set
            {
                if (value != _index)
                {
                    _index = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Index"));
                }
            }
        }
        public string CardName
        {
            get { return _cardName; }
            set
            {
                if (value != _cardName)
                {
                    _cardName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CardName"));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
   
    }
}
