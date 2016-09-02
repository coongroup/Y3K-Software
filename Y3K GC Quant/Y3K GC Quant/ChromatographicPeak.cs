using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_GC_Quant
{
    public sealed class ChromatographicPeak : IPeak
    {
        public double Time { get; private set; }

        public double Intensity { get; private set; }

        public ChromatographicPeak(double time, double intensity)
        {
            Time = time;
            Intensity = intensity;
        }

        public override string ToString()
        {
            return string.Format("({0:G4}, {1:G4})", Time, Intensity);
        }

        public int CompareTo(double time)
        {
            return Time.CompareTo(time);
        }

        public int CompareTo(IPeak other)
        {
            return Time.CompareTo(other.X);
        }

        public int CompareTo(ChromatographicPeak other)
        {
            return Time.CompareTo(other.Time);
        }

        public int CompareTo(object other)
        {
            return 0;
        }

        double IPeak.X
        {
            get { return Time; }
        }

        double IPeak.Y
        {
            get { return Intensity; }
        }
    }
}
