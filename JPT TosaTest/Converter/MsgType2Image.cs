using JPT_TosaTest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
namespace JPT_TosaTest.Converter
{
    public class MsgType2Image : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            EnumMessageType msg = (EnumMessageType)value;
            BitmapImage bitmap = null;
            switch (msg)
            { 
                case EnumMessageType.Info:
                    bitmap = new BitmapImage(new Uri(@"..\Images\Info24_24.png",UriKind.Relative));
                    break;
                case EnumMessageType.Warning:
                    bitmap = new BitmapImage(new Uri(@"..\Images\Warning24_24.png", UriKind.Relative));
                    break;
                case EnumMessageType.Error:
                    bitmap = new BitmapImage(new Uri(@"..\Images\Error24_24.png", UriKind.Relative));
                    break;
                default:
                    break;
            }
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
