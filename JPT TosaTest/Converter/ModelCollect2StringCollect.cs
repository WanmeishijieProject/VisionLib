using JPT_TosaTest.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace JPT_TosaTest.Converter
{
    public class ModelCollect2StringCollect : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<string> list = new ObservableCollection<string>();
            foreach (var it in value as ObservableCollection<ModelItem>)
            {
                list.Add(it.StrName);
            }
            return list;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
