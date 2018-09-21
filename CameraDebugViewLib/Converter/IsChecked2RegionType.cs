
using CameraDebugLib.Vision;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CameraDebugLib.Converter
{
    class IsChecked2RegionType : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Enum_REGION_TYPE)value == Enum_REGION_TYPE.CIRCLE;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Enum_REGION_TYPE.CIRCLE : Enum_REGION_TYPE.RECTANGLE;
        }
    }
}
