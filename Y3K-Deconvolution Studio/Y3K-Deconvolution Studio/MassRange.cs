using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Deconvolution_Studio
{
    public class MassRange : DoubleRange
    {
        public MassRange()
        {
        }

        public MassRange(double minMass, double maxMass)
            : base(minMass, maxMass)
        {
        }

        public MassRange(double meanMass, Tolerance toleranceWidth)
            : base(meanMass, toleranceWidth)
        {
        }

        public override string ToString()
        {
            return ToString("G9");
        }

        public override string ToString(string format)
        {
            return string.Format("{0} - {1} Da", Minimum.ToString(format), Maximum.ToString(format));
        }

        #region Static

        public new static MassRange FromPPM(double mean, double ppmTolerance)
        {
            return new MassRange(mean, new Tolerance(ToleranceUnit.PPM, ppmTolerance));
        }

        #endregion Static
    }
}
