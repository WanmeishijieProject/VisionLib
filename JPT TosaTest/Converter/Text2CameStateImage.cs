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
    public class Text2CameStateImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strCameraState = value.ToString().ToUpper();
            BitmapImage bitmap = null;
            switch (strCameraState)
            {
                case "CONNECTED":
                    bitmap = new BitmapImage(new Uri(@"..\Images\Connected.png", UriKind.Relative));
                    break;
                case "DISCONNECTED":
                    bitmap = new BitmapImage(new Uri(@"..\Images\Disconnected.png", UriKind.Relative));
                    break;
                default:
                    break;
            }
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
