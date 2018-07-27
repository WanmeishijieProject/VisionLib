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

        private string _itemValue;
        public string ItemValue
        {
            set
            {
                if (_itemValue != value)
                {
                    _itemValue = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ItemValue"));
                }
            }
            get { return _itemValue; }
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
