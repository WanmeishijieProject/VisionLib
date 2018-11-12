using JPT_TosaTest.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace JPT_TosaTest.Converter
{
    public class SystemState2Enable : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter.ToString().ToUpper().Equals("START"))
            {
                return (EnumSystemState)value == EnumSystemState.Idle;
            }
            else if (parameter.ToString().ToUpper().Equals("PAUSE"))
            {
                return (EnumSystemState)value == EnumSystemState.Pause ||
                    (EnumSystemState)value == EnumSystemState.Running;
            }
            else if (parameter.ToString().ToUpper().Equals("STOP"))
            {
                return (EnumSystemState)value == EnumSystemState.Running ||
                   (EnumSystemState)value == EnumSystemState.Pause;
            }
            else
                return false;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
