using System.Collections;
using System.Collections.ObjectModel;


namespace JPT_TosaTest.Classes.AlimentClass.ScanCure
{
    public class ScanCurveGroup : ObservableCollection<IScanCurve>
    {
        public void ClearCurvesContent()
        {
            foreach(var c in this)
            {
                c.Clear();
            }
        }

        public void ChangeDisplayName(string DisplayName)
        {
            foreach (var c in this)
            {
                c.DisplayName = DisplayName;
            }
        }

        public void ChangePrefix(string Prefix)
        {
            foreach (var c in this)
            {
                c.Prefix = Prefix;
            }

        }

        public void ChangeSuffix(string Suffix)
        {
            foreach (var c in this)
            {
                c.Suffix = Suffix;
            }

        }
    }
}
