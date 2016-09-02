using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_GC_Quant
{
    public class Normalization
    {
        public Normalization()
        {
        }

        public void CombineLikeMZPeaks(List<MZPeak> peaks)
        {
            if (peaks.Count > 0)
            {
                double currentMZ = peaks[0].MZ;
                double totalIntensity = peaks[0].Intensity;
                List<MZPeak> combinedPeaks = new List<MZPeak>();
                for (int i = 1; i < peaks.Count; i++)
                {
                    MZPeak currentPeak = peaks[i];
                    if (Math.Round(currentPeak.MZ) == Math.Round(currentMZ))
                    {
                        totalIntensity += currentPeak.Intensity;
                    }
                    else
                    {
                        MZPeak newPeak = new MZPeak(Math.Round(currentMZ), totalIntensity);
                        combinedPeaks.Add(newPeak);
                        currentMZ = Math.Round(currentPeak.MZ);
                        totalIntensity = currentPeak.Intensity;
                    }
                }
                MZPeak peak = new MZPeak(currentMZ, totalIntensity);
                combinedPeaks.Add(peak);
                peaks.Clear();
                peaks.AddRange(combinedPeaks);
                NormalizePeaksOnlyIntensities(peaks, 999);
            }
        }

        public void CombineLikeMZPeaks(List<MZPeak> peaks, double value)
        {
            if (peaks.Count > 0)
            {
                double currentMZ = peaks[0].MZ;
                double totalIntensity = peaks[0].Intensity;
                List<MZPeak> combinedPeaks = new List<MZPeak>();
                for (int i = 1; i < peaks.Count; i++)
                {
                    MZPeak currentPeak = peaks[i];
                    if (Math.Round(currentPeak.MZ) == Math.Round(currentMZ))
                    {
                        totalIntensity += currentPeak.Intensity;
                    }
                    else
                    {
                        MZPeak newPeak = new MZPeak(Math.Round(currentMZ), totalIntensity);
                        combinedPeaks.Add(newPeak);
                        currentMZ = Math.Round(currentPeak.MZ);
                        totalIntensity = currentPeak.Intensity;
                    }
                }
                MZPeak peak = new MZPeak(currentMZ, totalIntensity);
                combinedPeaks.Add(peak);
                peaks.Clear();
                peaks.AddRange(combinedPeaks);
                NormalizePeaksOnlyIntensities(peaks, value);
            }
        }

        public void NormalizePeaks(List<MZPeak> peaks, double value)
        {
            double maxVal = 0;
            foreach (MZPeak peak in peaks)
            {
                if (peak.Intensity > maxVal)
                {
                    maxVal = peak.Intensity;
                }
            }
            if (maxVal != value)
            {
                double scaleFactor = value / maxVal;
                List<MZPeak> newList = new List<MZPeak>();
                foreach (MZPeak peak in peaks)
                {
                    newList.Add(new MZPeak(Math.Round(peak.MZ, MidpointRounding.AwayFromZero), Math.Round(peak.Intensity * scaleFactor)));
                }
                peaks.Clear();
                peaks.AddRange(newList);
            }
        }

        public void NormalizePeaksOnlyIntensities(List<MZPeak> peaks, double value)
        {
            double maxVal = 0;
            foreach (MZPeak peak in peaks)
            {
                if (peak.Intensity > maxVal)
                {
                    maxVal = peak.Intensity;
                }
            }
            if (maxVal != value)
            {
                double scaleFactor = value / maxVal;
                List<MZPeak> newList = new List<MZPeak>();
                foreach (MZPeak peak in peaks)
                {
                    newList.Add(new MZPeak(peak.MZ, Math.Round(peak.Intensity * scaleFactor)));
                }
                peaks.Clear();
                peaks.AddRange(newList);
            }
        }

        public double GetDemoninatorTerm(EISpectrum eiSpec)
        {
            double returnVal = 0;
            foreach (MZPeak peak in eiSpec.FinalNormalizedEIPeaks)
            {
                returnVal += (peak.MZ * peak.Intensity);
            }
            return returnVal;
        }

        public double GetDemoninatorTerm(List<MZPeak> peaks)
        {
            double returnVal = 0;
            foreach (MZPeak peak in peaks)
            {
                returnVal += (peak.MZ * peak.Intensity);
            }
            return returnVal;
        }

        public void GetAdjustedPeaks(List<MZPeak> peaks, double factor = .53)
        {
            // factor = 1.00;
            List<MZPeak> tmpList = new List<MZPeak>();
            foreach (MZPeak peak in peaks)
            {
                double newIntensity = peak.Intensity * (999 / (999 + (factor * peak.Intensity)));
                //newIntensity = peak.Intensity;
                double newMZ = peak.MZ * 1.3;
                //double newMZ = peak.MZ;

                //newIntensity = peak.Intensity;
                //newMZ = peak.MZ;

                tmpList.Add(new MZPeak(Math.Round(newMZ), Math.Round(newIntensity)));
            }
            peaks.Clear();
            peaks.AddRange(tmpList);
        }

        public List<MZPeak> AdjustPeaks(List<MZPeak> peaks, double factor = .53)
        {
            //factor = 1.00;
            List<MZPeak> tmpList = new List<MZPeak>();
            if (peaks.Count > 0)
            {
                foreach (MZPeak peak in peaks)
                {
                    double newIntensity = peak.Intensity * (999 / (999 + (factor * peak.Intensity)));
                    //  newIntensity = peak.Intensity;
                    double newMZ = peak.MZ * 1.3;
                    //double newMZ = peak.MZ;

                    tmpList.Add(new MZPeak(Math.Round(newMZ), Math.Round(newIntensity)));
                }
            }
            return tmpList;
        }
    }
}
