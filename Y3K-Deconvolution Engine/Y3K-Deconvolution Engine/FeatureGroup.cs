using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Deconvolution_Engine
{
    public class FeatureGroup
    {
        public List<Feature> allFeatures;
        public List<FeatureGroup> SubGroups;
        public FeatureGroup mainSubGroup;
        public double ApexTime;
        public double TotalIntensity;
        public Feature maxFeature;
        public HashSet<double> featureMZHashSet;
        public List<MZPeak> finalPeaks;
        public Feature BasePeakFeature;
        public HashSet<int> includedFeatureIDs;
        public int groupID;
        public string polarity;

        public FeatureGroup()
        {
            this.allFeatures = new List<Feature>();
            this.SubGroups = new List<FeatureGroup>(1);
            this.featureMZHashSet = new HashSet<double>();
            //this.finalPeaks = new List<MZPeak>();
            includedFeatureIDs = new HashSet<int>();
        }

        public FeatureGroup(Feature feature)
        {
            this.allFeatures = new List<Feature>();
            this.SubGroups = new List<FeatureGroup>(1);
            this.featureMZHashSet = new HashSet<double>();
            this.featureMZHashSet.Add(feature.AverageMZ);
            this.allFeatures.Add(feature);
            //this.finalPeaks = new List<MZPeak>();     
        }

        public void AddFeature(Feature feature)
        {
            this.allFeatures.Add(feature);
            this.featureMZHashSet.Add(feature.AverageMZ);
        }

        public int Count
        {
            get { return this.allFeatures.Count; }
        }

        public void DoApexCalculations()
        {
            this.TotalIntensity = 0;
            this.ApexTime = 0;
            foreach (Feature feature in allFeatures)
            {
                this.TotalIntensity += feature.TotalSignal;
                this.ApexTime += (feature.MaxPeak.RT * feature.TotalSignal);
            }
            this.ApexTime /= this.TotalIntensity;
            this.featureMZHashSet.Clear();
        }

        public List<MZPeak> GetMZPeaks()
        {
            List<MZPeak> returnList = new List<MZPeak>();
            foreach (Feature feature in this.allFeatures)
            {
                returnList.Add(feature.MaxPeak.MZPeak);
            }
            return returnList;
        }
    }
}
