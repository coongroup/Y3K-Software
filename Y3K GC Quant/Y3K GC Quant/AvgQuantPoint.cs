using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_GC_Quant
{
    public class AvgQuantPoint
    {
        public int BatchID;
        public int ControlID;
        public int GCMasterGroupID;
        public double GCMasterGroup_MZ;
        public List<int> ReplicateIDs;
        public List<QuantPoint> ReplicateQuantPoints;
        public List<double> AllIntensities;
        public List<double> Allintensities_Normalized;
        public double AvgIntensity;
        public double AvgIntensity_StdDev;
        public double AvgIntensity_Normalized;
        public double AvgIntensity_Normalized_StdDev;
        public double AvgIntensity_LessControl;
        public double AvgIntensity_Normalized_LessControl;
        public double AvgIntensity_LessControl_PValue;
        public double AvgIntensity_Normalized_LessControl_PValue;
        public double GCMasterGroup_ApexRT;
        public string Name;
        public int ChEBI_ID;
        public string PreferredName;

        public AvgQuantPoint()
        {
            ReplicateIDs = new List<int>();
            ReplicateQuantPoints = new List<QuantPoint>();
            AllIntensities = new List<double>();
            Allintensities_Normalized = new List<double>();
        }
    }
}
