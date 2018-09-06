using System;
using System.Linq;
using System.Windows.Media.Media3D;

namespace JPT_TosaTest.Classes.AlimentClass.ScanCure
{
    public class ScanCurve3D : ScanCurveBase<Point3D>
    {
        #region Variables

        #endregion

        #region Constructors

        public ScanCurve3D() : base()
        {
            Construct();
        }

        public ScanCurve3D(string DisplayName) : base(DisplayName)
        {
            Construct();
        }

        private void Construct()
        {
            // generate some random points to debug

            Random r = new Random();

            for (double x = -Math.PI; x < Math.PI; x += 0.1)
            {
                for (double y = -Math.PI; y < Math.PI; y += 0.1)
                {
                    this.Add(new Point3D(x, y, Math.Sin(x * r.NextDouble()) * Math.Cos(y)));
                }
            }

            X_Title = "X轴";
            Y_Title = "Y轴";
            Z_Title = "强度值";
        }

        #endregion

        #region Properties



        #endregion

        #region Methods

        /// <summary>
        /// Get the position with the maximum indensity
        /// </summary>
        /// <returns></returns>
        public override Point3D FindMaximalPosition3D()
        {
            if (this.Count < 2)
                throw new InvalidOperationException(string.Format("There are no enough points in the scanned curve #{0}", this.DisplayName));

            var order = this.OrderByDescending(p => p.Z);
            var maxPosition = order.First();
            return maxPosition;
        }

        public string X_Title { get; set; }
        public string Y_Title { get; set; }
        public string Z_Title { get; set; }
        #endregion

    }
}
