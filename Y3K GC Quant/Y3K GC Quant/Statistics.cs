using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Distributions;

namespace Y3K_GC_Quant
{
    public class Statistics
    {
        public static double GetWelchsTTestPValue(List<double> dataOne, List<double> dataTwo)
        {
            var chart = new Chart();

            if (dataOne.Count > 0 && dataTwo.Count > 0)
            {
                double meanOne = dataOne.Average();
                double meanTwo = dataTwo.Average();

                double v1 = dataOne.Variance();
                double v2 = dataTwo.Variance();

                var s2 = Math.Sqrt((v1 / dataOne.Count) + (v2 / dataTwo.Count));

                var num = (meanOne - meanTwo);
                var tStatistic = (num / s2);

                var df = Math.Pow(((v1 / dataOne.Count) + (v2 / dataTwo.Count)), 2);

                var dfDenom = ((Math.Pow((v1 / dataOne.Count), 2) / (dataOne.Count - 1)) + (Math.Pow((v2 / dataTwo.Count), 2) / (dataTwo.Count - 1)));
                df /= dfDenom;

                if (!double.IsNaN(tStatistic) && !double.IsNaN(df))
                {
                    double res = chart.DataManipulator.Statistics.TDistribution(tStatistic, (int)Math.Round(df), false);
                    return -Math.Log(res, 10);
                }
            }
            return 0;
        }

        public static void LinearRegression(double[] xVals, double[] yVals,
                                           int inclusiveStart, int exclusiveEnd,
                                           out double rsquared, out double yintercept,
                                           out double slope)
        {
            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double ssX = 0;
            double ssY = 0;
            double sumCodeviates = 0;
            double sCo = 0;
            double count = exclusiveEnd - inclusiveStart;

            for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
            {
                double x = xVals[ctr];
                double y = yVals[ctr];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }
            ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
            double RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            double RDenom = (count * sumOfXSq - (sumOfX * sumOfX))
             * (count * sumOfYSq - (sumOfY * sumOfY));
            sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            double meanX = sumOfX / count;
            double meanY = sumOfY / count;
            double dblR = RNumerator / Math.Sqrt(RDenom);
            rsquared = dblR * dblR;
            yintercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }
    }
}
