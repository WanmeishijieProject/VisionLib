using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using JPT_TosaTest.Model;
namespace JPT_TosaTest.Converter
{
    public class MsgType2ForeBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            EnumMessageType msg = (EnumMessageType)value;
            Brush brush = null;
            switch (msg)
            {
                case EnumMessageType.Info:
                    brush =new SolidColorBrush(Color.FromRgb(0,0,0));
                    break;
                case EnumMessageType.Warning:
                    brush = new SolidColorBrush(Color.FromRgb(200,200,0));
                    break;
                case EnumMessageType.Error:
                    brush = new SolidColorBrush(Color.FromRgb(255,0,0));
                    break;
                default:
                    break;
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
