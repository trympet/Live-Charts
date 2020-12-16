//The MIT License(MIT)

//Copyright(c) 2016 Alberto Rodriguez & LiveCharts Contributors

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using LiveCharts.Defaults;
using LiveCharts.Definitions.Points;
using LiveCharts.Definitions.Series;
using LiveCharts.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace LiveCharts.SeriesAlgorithms
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="LiveCharts.SeriesAlgorithm" />
    /// <seealso cref="LiveCharts.Definitions.Series.ICartesianSeries" />
    public class CandleAlgorithm : SeriesAlgorithm, ICartesianSeries
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CandleAlgorithm"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        public CandleAlgorithm(ISeriesView view) : base(view)
        {
            SeriesOrientation = SeriesOrientation.Horizontal;
            PreferredSelectionMode = TooltipSelectionMode.SharedXValues;
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public override void Update()
        {
            var castedSeries = (IFinancialSeriesView) View;
            
            const double padding = 1.2;

            var totalSpace = ChartFunctions.GetUnitWidth(AxisOrientation.X, Chart, View.ScalesXAt) * castedSeries.Interval - padding;

            double exceed = 0;
            double candleWidth;

            if (totalSpace > castedSeries.MaxColumnWidth)
            {
                exceed = totalSpace - castedSeries.MaxColumnWidth;
                candleWidth = castedSeries.MaxColumnWidth;
            }
            else
            {
                candleWidth = totalSpace;
            }

            ChartPoint previousDrawn = null;

            var interval = castedSeries.Interval;
            var points = View.ActualValues.GetPoints(View);

            for (int i = 0; i < points.Count(); i += interval)
            {
                var firstPoint = points.ElementAt(i);
                var view = View.GetPointView(firstPoint,
                    View.DataLabels ? View.GetLabelPointFormatter()(firstPoint) : null);
                var x = ChartFunctions.ToDrawMargin(firstPoint.X, AxisOrientation.X, Chart, View.ScalesXAt);

                double open, high, low, close;
                GetOHLC(points, interval, i, out open, out high, out low, out close);

                firstPoint.View = View.GetPointView(firstPoint,
                    View.DataLabels ? View.GetLabelPointFormatter()(firstPoint) : null);

                firstPoint.SeriesView = View;

                var candeView = (IOhlcPointView) firstPoint.View;

                candeView.Open = ChartFunctions.ToDrawMargin(open, AxisOrientation.Y, Chart, View.ScalesYAt);
                candeView.Close = ChartFunctions.ToDrawMargin(close, AxisOrientation.Y, Chart, View.ScalesYAt);
                candeView.High = ChartFunctions.ToDrawMargin(high, AxisOrientation.Y, Chart, View.ScalesYAt);
                candeView.Low = ChartFunctions.ToDrawMargin(low, AxisOrientation.Y, Chart, View.ScalesYAt);

                candeView.Width = candleWidth - padding > 0 ? candleWidth - padding : 0;
                candeView.Left = x + exceed/2 + padding;
                candeView.StartReference = (candeView.High + candeView.Low)/2;

                firstPoint.ChartLocation = new CorePoint(x + exceed/2, (candeView.High + candeView.Low)/2);

                firstPoint.View.DrawOrMove(previousDrawn, firstPoint, 0, Chart);

                previousDrawn = firstPoint;
            }
        }

        private static void GetOHLC(IEnumerable<ChartPoint> points, int interval, int startIndex, out double open, out double high, out double low, out double close)
        {
            var firstElement = points.ElementAt(startIndex);
            open = firstElement.Open;
            high = firstElement.High;
            low = firstElement.Low;
            if (interval == 1)
            {
                close = firstElement.Close;
                return;
            }

            var length = points.Count();

            // Interval + startindex could be greater than length,
            // so we declare j outside of the loop scope to get our
            // close value.
            int j = 0;
            for (; j < interval; j++)
            {
                if (startIndex + j == length)
                {
                    break;
                }
                if (points.ElementAtOrDefault(startIndex + j) is ChartPoint point)
                {
                    if (point.High > high)
                    {
                        high = point.High;
                    }
                    if (point.Low < low)
                    {
                        low = point.Low;
                    }
                }
            }
            close = points.ElementAt(startIndex + j - 1).Close;
        }

        double ICartesianSeries.GetMinX(AxisCore axis)
        {
            return AxisLimits.StretchMin(axis);
        }

        double ICartesianSeries.GetMaxX(AxisCore axis)
        {
            return AxisLimits.UnitRight(axis);
        }

        double ICartesianSeries.GetMinY(AxisCore axis)
        {
            return AxisLimits.SeparatorMin(axis);
        }

        double ICartesianSeries.GetMaxY(AxisCore axis)
        {
            return AxisLimits.SeparatorMaxRounded(axis);
        }
    }
}
