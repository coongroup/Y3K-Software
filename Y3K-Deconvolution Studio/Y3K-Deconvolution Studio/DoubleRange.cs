using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Deconvolution_Studio
{
    public class DoubleRange : Range<double>
    {
        public DoubleRange()
            : base(0, 0)
        {
        }

        public DoubleRange(double minimum, double maximum)
            : base(minimum, maximum)
        {
        }

        public DoubleRange(IRange<double> range)
            : base(range.Minimum, range.Maximum)
        {
        }

        public DoubleRange(double mean, Tolerance tolerance)
        {
            SetTolerance(mean, tolerance);
        }

        private void SetTolerance(double mean, Tolerance tolerance)
        {
            if (tolerance == null)
            {
                Minimum = Maximum = mean;
                return;
            }

            double value = Math.Abs(tolerance.Value);

            if (tolerance.Type == ToleranceType.PlusAndMinus)
                value *= 2;

            switch (tolerance.Unit)
            {
                default:
                    Minimum = mean - value / 2.0;
                    Maximum = mean + value / 2.0;
                    break;

                case ToleranceUnit.MMU:
                    Minimum = mean - value / 2000.0;
                    Maximum = mean + value / 2000.0;
                    break;

                case ToleranceUnit.PPM:
                    Minimum = mean * (1 - (value / 2e6));
                    Maximum = mean * (1 + (value / 2e6));
                    break;
            }
        }

        public double Mean
        {
            get { return (Maximum + Minimum) / 2.0; }
        }

        public double Width
        {
            get { return Maximum - Minimum; }
        }

        public double ToPPM()
        {
            return 1e6 * Width / Mean;
        }

        public double OverlapFraction(DoubleRange otherRange)
        {
            DoubleRange shorter, longer;
            if (Width < otherRange.Width)
            {
                shorter = this;
                longer = otherRange;
            }
            else
            {
                shorter = otherRange;
                longer = this;
            }

            double coveredWidth = 0;
            if (shorter.Minimum > longer.Minimum)
            {
                if (shorter.Minimum < longer.Maximum)
                {
                    coveredWidth = Math.Min(longer.Maximum, shorter.Maximum) - shorter.Minimum;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                if (shorter.Maximum < longer.Minimum)
                {
                    coveredWidth = Math.Min(shorter.Maximum, longer.Maximum) - shorter.Maximum;
                }
                else
                {
                    return 0;
                }
            }
            return coveredWidth / shorter.Width;
        }

        public virtual string ToString(string format)
        {
            return string.Format("[{0} - {1}]", Minimum.ToString(format), Maximum.ToString(format));
        }

        #region Static

        public static DoubleRange FromPPM(double mean, double ppmTolerance)
        {
            return new DoubleRange(mean, new Tolerance(ToleranceUnit.PPM, ppmTolerance));
        }

        public static DoubleRange FromDa(double mean, double daTolerance)
        {
            return new DoubleRange(mean, new Tolerance(ToleranceUnit.DA, daTolerance));
        }

        #endregion Static
    }
}
