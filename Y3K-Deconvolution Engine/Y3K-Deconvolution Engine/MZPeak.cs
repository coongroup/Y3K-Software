using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Deconvolution_Engine
{
    public class MZPeak : IPeak, IEquatable<MZPeak>
    {
        public double Intensity { get; private set; }

        public double MZ { get; private set; }

        public MZPeak(double mz = 0.0, double intensity = 0.0)
        {
            MZ = mz;
            Intensity = intensity;
        }

        public bool Equals(IPeak other)
        {
            if (ReferenceEquals(this, other)) return true;
            return MZ.Equals(other.X) && Intensity.Equals(other.Y);
        }

        public override string ToString()
        {
            return string.Format("({0:F4},{1:G5})", MZ, Intensity);
        }

        public int CompareTo(double other)
        {
            return MZ.CompareTo(other);
        }

        public int CompareTo(IPeak other)
        {
            if (other == null)
                return 1;
            return MZ.CompareTo(other.X);
        }

        public int CompareTo(object other)
        {
            if (other is double)
                return MZ.CompareTo((double)other);
            var peak = other as IPeak;
            if (peak != null)
                return CompareTo(peak);
            throw new InvalidOperationException("Unable to compare types");
        }

        protected double X
        {
            get { return MZ; }
        }

        protected double Y
        {
            get { return Intensity; }
        }

        double IPeak.X
        {
            get { return X; }
        }

        double IPeak.Y
        {
            get { return Y; }
        }

        public override bool Equals(object obj)
        {
            return obj is MZPeak && Equals((MZPeak)obj);
        }

        public override int GetHashCode()
        {
            return MZ.GetHashCode() ^ Intensity.GetHashCode();
        }

        public bool Equals(MZPeak other)
        {
            return MZ.Equals(other.MZ) && Intensity.Equals(other.Intensity);
        }
    }
}
