using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using MathNet.Numerics;

namespace JPT_TosaTest.Classes.AlimentClass.ScanCure
{
    public class ScanCurve2D : ScanCurveBase<Point>
    {
        #region Variables

        string displayName = "";

        #endregion

        #region Constructors

        public ScanCurve2D() : base()
        {
            Construct();
        }

        public ScanCurve2D(string DisplayName) : base(DisplayName)
        {
            Construct();

            this.DisplayName = DisplayName;
        }


        private void Construct()
        {
            FittingCurve = new ScanCurveBase<Point>
            {
                Suffix = "Fitting"
            };
            FittingCurve.LineStyle.Thickness = 1;
            FittingCurve.LineStyle.DashStyle = new DashStyle(new double[] { 1, 5 }, 0);

            MaxPowerConstantLine = new ScanCurveBase<Point>
            {
                Suffix = "Max",
                Visible = false
            };
            MaxPowerConstantLine.LineStyle.Thickness = 1;
            MaxPowerConstantLine.LineStyle.DashStyle = new DashStyle(new double[] { 1, 5 }, 0);

            this.CollectionChanged += ((s, e)=>
            {
                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    FittingCurve.Clear();
                    FittingCurve.Visible = false;

                    MaxPowerConstantLine.Clear();
                    MaxPowerConstantLine.Visible = false;
                }
            });

            // generate some fake points to debug
            Random r = new Random((int)DateTime.Now.Ticks);
            var offset = r.NextDouble();
            for (double i = -2; i < 2; i += 0.2)
            {
                this.Add(new Point(i, GaussianDistribution.Invoke(i, 1, offset + r.NextDouble() / 10)));
            }

            FindMaximalPosition();

            X_Title = "X轴";
            Y_Title = "Y轴";
        }

        #endregion

        #region Properties

        /// <summary>
        /// The fitting curve
        /// </summary>
        public ScanCurveBase<Point> FittingCurve
        {
            private set;
            get;
        }

        public string X_Title { get; set; }
        public string Y_Title { get; set; }
        /// <summary>
        /// The constant line indicates where is the max power of the Scan Curve
        /// </summary>
        public ScanCurveBase<Point> MaxPowerConstantLine
        {
            private set;
            get;
        }

        /// <summary>
        /// Get the name of the curve
        /// </summary>
        new public string DisplayName
        {
            set
            {
                FittingCurve.DisplayName = value;
                MaxPowerConstantLine.DisplayName = value;
                // UpdateProperty(ref displayName, value);
                displayName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DisplayName"));
            }
            get
            {
                return string.Join(" ", new object[] { Prefix, displayName, Suffix });
            }
        }
        
        /// <summary>
        /// Get the polynomial equation
        /// </summary>
        /// <returns></returns>
        Func<double, double> PolyFittingEqution
        {
            get
            {
                return new Func<double, double>(x =>
                {
                    double y = 0;
                    for (int p = 0; p < PolyFittingEquationFactors.Length; p++)
                        y += PolyFittingEquationFactors[p] * Math.Pow(x, p);
                    return y;
                });
            }
        }

        double FittingDomainMin
        {
            get;
            set;
        }

        double FittingDomainMax
        {
            get;
            set;
        }
        
        /// <summary>
        /// Get the factors list of the polynomial
        /// </summary>
        public double[] PolyFittingEquationFactors { private set; get; }

        #endregion

        #region Methods

        /// <summary>
        /// Fit the scan curve to a polynomial with the order defined inside of the methods
        /// </summary>
        /// <returns></returns>
        double[] PolyFit()
        {
            const int FITTING_ORDER = 3;

            if (this.Count >= 7)
            {
                List<double> xData = new List<double>();
                List<double> yData = new List<double>();

                // collect the points with Y value is greater than Max Y * 0.6
                double max_y = this.Max(a => a.Y);
                double thr_y = max_y * 0.5;
                foreach (var p in this)
                {
                    if (p.Y > thr_y)
                    {
                        xData.Add(p.X);
                        yData.Add(p.Y);
                    }
                }

                // insure that there are at least 7 points to fit to the polynomial
                if (xData.Count < 7)
                {

                    xData.Clear();
                    yData.Clear();

                    // get the position of the point with maximum Y value
                    int id_max_point = this.IndexOf(this.OrderBy(a => a.Y).Last());

                    int start = id_max_point - 3;
                    int end = id_max_point + 3;
                    int last = this.Count - 1;

                    // judge how many points are there on both side of the maximum point
                    if (start < 0)
                    {
                        end += -start;
                        if (end > last)
                            end = last;
                        start = 0;
                    }
                    else if (end > last)
                    {
                        start -= (end - last);
                        if (start < 0)
                            start = 0;
                        end = last;
                    }

                    for (int i = start; i <= end; i++)
                    {
                        xData.Add(this[i].X);
                        yData.Add(this[i].Y);
                    }
                }


                FittingDomainMin = xData[0];
                FittingDomainMax = xData[xData.Count - 1];

                PolyFittingEquationFactors = Fit.Polynomial(xData.ToArray(), yData.ToArray(), FITTING_ORDER);
                return PolyFittingEquationFactors;
            }
            else
            {
                PolyFittingEquationFactors = null;
                throw new InvalidOperationException(string.Format("There are not enough points in the scanned curve #{0} to fit, at least 7 points are needed.", DisplayName));
            }
        }

        /// <summary>
        /// Calculate the equation of the fitting curve and return the fitting curve by a list
        /// </summary>
        /// <returns></returns>
        void GetBeautifiedCurve()
        {
            // the fitting curve consist of these points
            const double POINTS_IN_BEAUTIFY_CURVE = 20;
            

            // fitting the curve
            double[] equation = PolyFit();

            // the range to draw the fitting curve
            double range = FittingDomainMax - FittingDomainMin;

            // the start point to draw the fitting curve
            double start = FittingDomainMin;

            // the steps to draw the fitting curve
            double step = range / POINTS_IN_BEAUTIFY_CURVE;

            for (int i = 0; i < POINTS_IN_BEAUTIFY_CURVE; i++)
            {
                double x = start + i * step;
                double y = PolyFittingEqution(x);
                FittingCurve.Add(new Point(x, y));
            }

            FittingCurve.Visible = true;
        }

        /// <summary>
        /// Calculate the position where the maximal value is in the scan curve.
        /// The 3-order polynomal are used to fit the scan curve.
        /// </summary>
        /// <returns></returns>
        public override Point FindMaximalPosition()
        {
            // Do Poly fit
            GetBeautifiedCurve();

            //! NOTE:
            //! The function base on the condition that the polynomial order to fit is 3
            //! See the function this.PolyFit() for the detail

            // calculate the roots of the derivation of the 3-order polynomial to find the two extremum of the polynomial
            // build the factor of the derivation, the format of the equation is like ax^2 + bx + c = 0,
            // the roots of the equation are x = [-b ± sqrt(b^2 - 4ac)] / 2a
            double a = 3 * PolyFittingEquationFactors[3];
            double b = 2 * PolyFittingEquationFactors[2];
            double c = PolyFittingEquationFactors[1];

            double[] root = new double[2];
            root[0] = (-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
            root[1] = (-b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a);

            // the condition of the maximal value: f'(x) = 0 and f''(x) < 0
            // calculate the factors of f''(x)
            a = 2 * PolyFittingEquationFactors[2];
            b = 6 * PolyFittingEquationFactors[3];
            double x_at_maximal = 0;
            if ((a + b * root[0]) < 0)
                x_at_maximal = root[0];
            else if ((a + b * root[1]) < 0)
                x_at_maximal = root[1];
            else
                throw new InvalidOperationException(string.Format(
                    "unable to find the maximal value of the 3-order polynomial, the factors of the poly func are [{0}, {1}, {2}, {3}], the roots of secondary derivation are [{4}, {5}].",
                    new object[] { PolyFittingEquationFactors[0], PolyFittingEquationFactors[1], PolyFittingEquationFactors[2], PolyFittingEquationFactors[3], root[0], root[1] }));

            // return the position where the maximal value is
            Point maxPoint;
            if (this.First().X > x_at_maximal)
                maxPoint = this.First();
            else if (this.Last().X < x_at_maximal)
                maxPoint = this.Last();
            else
            {
                // calulate extremum
                double extremum = PolyFittingEqution(x_at_maximal);
                maxPoint = new Point(x_at_maximal, extremum);
            }

            MaxPowerConstantLine.Clear();
            MaxPowerConstantLine.Add(new Point(maxPoint.X, 0));
            MaxPowerConstantLine.Add(maxPoint);
            MaxPowerConstantLine.Visible = true;

            return maxPoint;
        }
        

        #endregion

    }
}
