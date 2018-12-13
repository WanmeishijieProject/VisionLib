using AxisParaLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace JPT_TosaTest.Converter
{
    public class AxisType2Text : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EnumAxisType type = (EnumAxisType)value;
            if (parameter.ToString().ToLower() == "speed")
            {
                if (type == EnumAxisType.LinearAxis)
                    return "mm/s";
                else
                    return "deg/s";
            }
            else
            {
                if (type == EnumAxisType.LinearAxis)
                    return "mm";
                else
                    return "deg";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
