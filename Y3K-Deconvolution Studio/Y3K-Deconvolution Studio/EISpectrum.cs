using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Deconvolution_Studio
{
    public class EISpectrum
    {
        public double ApexTimeEI;
        public List<MZPeak> FinalEIPeaks;
        public List<MZPeak> FinalNormalizedEIPeaks;
        public MZSpectrum FinalEISpectrum;
        public FeatureGroup FeatureGroup;
        public MZPeak BasePeak;
        public double MatchScore;
        public string Identification;
        public double DenominatorTerm;
        public List<MZPeak> AdjustedPeaks;
        public MZSpectrum rawSpectrum;
        public double StartXICTime;
        public double StopXICTime;
        public double LeftTimeSearch;
        public double RightTimeSearch;
        public int spectrumID;
        public double NumPeaks;
        public double totalIntensity;
        public EISpectrum bestOppositeIonSpectrum;
        public double ApexSN;
        public bool isValid = true;
        public double ApexTICIntensity = 0;
        public double retentionIndex;
        public HashSet<string> featureHashes;
        public List<MZPeak> quantIons;
        public string UserName;
        public string NISTName;
        public string chebiID;
        public bool isInternalStandard;
        public bool isDirty = true;
        public bool userAdded = false;

        public EISpectrum()
        {
            this.FinalEIPeaks = new List<MZPeak>();
            this.FinalNormalizedEIPeaks = new List<MZPeak>();
            this.AdjustedPeaks = new List<MZPeak>();
            this.featureHashes = new HashSet<string>();
            quantIons = new List<MZPeak>();
        }

        public override string ToString()
        {
            return Math.Round(ApexTimeEI, 5).ToString() + " : " + FinalEIPeaks.Count + " Peaks";
        }
    }
}
