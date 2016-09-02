using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Deconvolution_Engine
{
    public class ThermoMzPeak : MZPeak
    {
        public int Charge { get; private set; }

        public double Noise { get; private set; }

        public double Resolution { get; private set; }

        public double SignalToNoise
        {
            get
            {
                if (Noise.Equals(0)) return float.NaN;
                return Intensity / Noise;
            }
        }

        public bool IsHighResolution { get { return Resolution > 0; } }

        public override string ToString()
        {
            return string.Format("{0} z = {1:+#;-#;?} SN = {2:F2}", base.ToString(), Charge, SignalToNoise);
        }

        public ThermoMzPeak()
        {
        }

        public ThermoMzPeak(double mz, double intensity, int charge = 0, double noise = 0.0, double resolution = 0.0)
            : base(mz, intensity)
        {
            Charge = charge;
            Noise = noise;
            Resolution = resolution;
        }

        public double GetSignalToNoise()
        {
            return SignalToNoise;
        }

        public double GetDenormalizedIntensity(double injectionTime)
        {
            return Intensity * injectionTime;
        }
    }
}
