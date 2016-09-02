using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Deconvolution_Engine
{
    public class Feature
    {
        public List<RTPeak> allRTPeaks;
        public List<RTPeak> smoothRTPeaks;
        private RTPeak maxPeak;
        public double maxIntensity;
        public double apexTime;
        public double averageMZ;
        private double totalMZTimesIntensity;
        public double totalIntensity;
        private double minRT;
        private double maxRT;
        public bool includedInSubGroup;
        public int numSubGroups;
        public double RawMZ;
        public double formulaError;
        public string curatorLine;
        public int ID_Number;
        public string polarity;

        public Feature()
        {
            this.allRTPeaks = new List<RTPeak>();
            this.smoothRTPeaks = new List<RTPeak>();
        }

        public Feature(MZPeak firstPeak, double RT, double HCDEnergy = 0)
        {
            this.allRTPeaks = new List<RTPeak>();
            this.smoothRTPeaks = new List<RTPeak>();
            this.minRT = RT;
            this.maxRT = RT;
            RTPeak newRTPeak = new RTPeak(firstPeak, RT);
            this.allRTPeaks.Add(newRTPeak);
            this.maxPeak = newRTPeak;
            this.totalMZTimesIntensity += (firstPeak.Intensity * firstPeak.MZ);
            this.totalIntensity += (firstPeak.Intensity);
            this.averageMZ = firstPeak.MZ;
            newRTPeak.HCDEnergy = HCDEnergy;
        }

        public void AddPeak(MZPeak peak, double RT, double HCDEnergy = 0)
        {
            RTPeak newRTPeak = new RTPeak(peak, RT);
            this.allRTPeaks.Add(newRTPeak);
            if (peak.Intensity > maxPeak.Intensity)
            {
                apexTime = RT;
                maxPeak = newRTPeak;
            }
            this.maxRT = RT;
            this.totalMZTimesIntensity += (peak.Intensity * peak.MZ);
            this.totalIntensity += (peak.Intensity);
            this.averageMZ = (this.totalMZTimesIntensity / this.totalIntensity);
            newRTPeak.HCDEnergy = HCDEnergy;
        }

        public void AddPeak(RTPeak peak)
        {
            this.allRTPeaks.Add(peak);
            if (maxPeak == null)
            {
                MaxPeak = peak;
            }
            if (peak.Intensity > maxPeak.Intensity)
            {
                apexTime = peak.RT;
                maxPeak = peak;
            }
            this.maxRT = peak.RT;
            this.totalMZTimesIntensity += (peak.Intensity * peak.MZ);
            this.totalIntensity += (peak.Intensity);
            this.averageMZ = (this.totalMZTimesIntensity / this.totalIntensity);
        }

        public void AddSmoothPeak(RTPeak peak)
        {
            this.smoothRTPeaks.Add(peak);
            if (this.maxPeak == null)
            {
                this.maxPeak = peak;
            }

            if (peak.Intensity > this.maxPeak.Intensity)
            {
                this.maxPeak = peak;
            }
            this.totalMZTimesIntensity += (peak.Intensity * peak.MZ);
            this.totalIntensity += (peak.Intensity);
            this.averageMZ = (this.totalMZTimesIntensity / this.totalIntensity);
        }


        public List<RTPeak> RawRTPeaks
        {
            get { return this.allRTPeaks; }
        }

        public List<RTPeak> SmoothRTPeaks
        {
            set { this.smoothRTPeaks = value; }
            get { return this.smoothRTPeaks; }
        }

        public double ApexTime
        {
            get { return this.apexTime; }
            set { this.apexTime = value; }
        }

        public double AverageMZ
        {
            set { this.averageMZ = value; }
            get { return this.averageMZ; }
        }

        public double MinRT
        {
            get { return this.minRT; }
        }

        public double MaxRT
        {
            get { return this.maxRT; }
        }

        public RTPeak MaxPeak
        {
            get { return this.maxPeak; }
            set { this.maxPeak = value; }
        }

        public int Count
        {
            get { return this.allRTPeaks.Count; }
        }

        public double FirstIntensity
        {
            get { return this.allRTPeaks.First().Intensity; }
        }

        public double LastIntensity
        {
            get { return this.allRTPeaks.Last().Intensity; }
        }

        public double TotalSignal
        {
            get { return this.totalIntensity; }
        }

        public double MaxIntensity
        {
            get { return this.maxIntensity; }
            set { this.maxIntensity = value; }
        }
    }
}
