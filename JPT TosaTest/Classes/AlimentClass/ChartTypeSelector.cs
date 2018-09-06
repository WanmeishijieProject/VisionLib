using JPT_TosaTest.Classes.AlimentClass.AlimentArgs;
using JPT_TosaTest.Classes.AlimentClass.ScanCure;
using System.Windows;
using System.Windows.Controls;

namespace JPT_TosaTest.Classes.AlimentClass
{
    public class ChartTypeSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            IAligmentArgs args = item as IAligmentArgs;

            ScanCurveGroup scg = null;
            if (args!=null)
                scg = args.Scg;

            if(scg != null)
            {
                if(scg.Count == 0 || scg[0].GetType() == typeof(ScanCurve2D))
                {
                    return (DataTemplate)element.TryFindResource("TemplateChart2D");
                }
                else
                {
                    return (DataTemplate)element.TryFindResource("TemplateChart3D");
                }
            }
            else
            {
                return null;
            }
        }
    }
}
