using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DevExpress.Xpf.Charts;
using M12.Base;

namespace JPT_TosaTest.Classes.AlimentResultClass
{
    public class AlimentResultBase<T> : INotifyPropertyChanged, IAlimentResult
    {
        private List<T> _dataList;
        public List<T> DataList
        {
            get { return _dataList; }
            set {
                if (_dataList != value)
                {
                    _dataList = value;
                    RaisePropertyChanged();
                }
            }
        }
        public string XTitle { get; set; }
        public string YTitle { get; set; }
        public string DisplayName { get; set; }

        public void AddElement(T t)
        {
            DataList.Add(t);
        }
        public void ClearDataList()
        {
            DataList.Clear();
        }
        public bool Visible { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public virtual Point2D GetMaxPoint2D(bool IsMax=true)
        {
            throw new NotImplementedException();
        }

        public virtual Point3D GetMaxPoint3D(bool IsMax=true)
        {
            throw new NotImplementedException();
        }


        #region NOTE the properties in block is used for the 2D series only

        public LineStyle LineStyle { set; get; }
        public Marker2DModel MarkerModel { get; set; }
        public int MarkerSize { get; set; }
        public bool MarkerVisible { get; set; }
        public SolidColorBrush Brush { set; get; }

        #endregion

        private void Constructor()
        {
            // Set the default style of the 2D series
            LineStyle = new LineStyle(2);
            MarkerVisible = false;
            MarkerSize = 7;
            MarkerModel = new CircleMarker2DModel();
            Visible = true;
        }
    }
}
