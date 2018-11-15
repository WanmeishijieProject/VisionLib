using JPT_TosaTest.Vision;
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
    public class EdgeTool2Image : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage bitmap = null;
            switch ((EnumToolType)value)
            {
                case EnumToolType.LineTool:
                    bitmap= new BitmapImage(new Uri(@"..\Images\Line.png", UriKind.Relative));
                    break;
                case EnumToolType.PairTool:
                    bitmap = new BitmapImage(new Uri(@"..\Images\Pair.png", UriKind.Relative));
                    break;
                case EnumToolType.CircleTool:
                    bitmap = new BitmapImage(new Uri(@"..\Images\Circle.png", UriKind.Relative));
                    break;
                case EnumToolType.FlagTool:
                    bitmap = new BitmapImage(new Uri(@"..\Images\Draw.png", UriKind.Relative));
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
