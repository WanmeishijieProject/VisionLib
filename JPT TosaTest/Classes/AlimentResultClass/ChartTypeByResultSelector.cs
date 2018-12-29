using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace JPT_TosaTest.Classes.AlimentResultClass
{
    public class ChartTypeByResultSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            IAlimentResult Result = item as IAlimentResult;

            if (Result != null)
            {
                if (Result.GetType() == typeof(AlimentResult2D))
                {
                    return (DataTemplate)element.TryFindResource("ResultChart2D");
                }
                else
                {
                    return (DataTemplate)element.TryFindResource("ResultChart3D");
                }
            }
            else
            {
                return null;
            }
        }
    }
}
