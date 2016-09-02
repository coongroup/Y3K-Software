using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Deconvolution_Engine
{
    public class RTPeak : IComparable
    {
        public MZPeak _peak;
        private double _RT;
        private double _Intensity;
        private double _MZ;
        public double HCDEnergy;
        public double _SN;

        public RTPeak()
        {
        }

        public RTPeak(MZPeak peak, double RT)
        {
            this._Intensity = peak.Intensity;
            this._MZ = peak.MZ;
            this._RT = RT;
            this._peak = peak;
        }

        public RTPeak(double MZ, double Intensity, double RT)
        {
            this._MZ = MZ;
            this._Intensity = Intensity;
            this._RT = RT;
        }

        public RTPeak(double MZ, double Intensity, double SN, double RT)
        {
            this._MZ = MZ;
            this._Intensity = Intensity;
            this._SN = SN;
            this._RT = RT;
        }

        public double RT
        {
            get { return this._RT; }
            set { this._RT = value; }
        }

        public double Intensity
        {
            get { return this._Intensity; }
            set { this._Intensity = value; }
        }

        public double MZ
        {
            get { return this._MZ; }
            set { this._MZ = value; }
        }

        public double SN
        {
            get { return this._SN; }
            set { this._SN = value; }
        }

        public MZPeak MZPeak
        {
            get { return new MZPeak(this._MZ, this._Intensity); }
        }

        public int Compare(double other)
        {
            return RT.CompareTo(other);
        }

        public int CompareTo(RTPeak other)
        {
            return RT.CompareTo(other.RT);
        }

        public int CompareTo(Object other)
        {
            RTPeak otherPeak = (RTPeak)other;
            return RT.CompareTo(otherPeak.RT);
        }

        public bool Equals(RTPeak obj)
        {
            return obj is RTPeak && Equals((RTPeak)obj);
        }
    }
}
