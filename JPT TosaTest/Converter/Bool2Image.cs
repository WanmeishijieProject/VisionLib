using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace JPT_TosaTest.Converter
{
    public class Bool2Image : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage bitmap = null;
            if((bool)value)
                    bitmap = new BitmapImage(new Uri(@"..\Images\lightOn.png", UriKind.Relative));
               else
                    bitmap = new BitmapImage(new Uri(@"..\Images\lightOff.png", UriKind.Relative));
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
