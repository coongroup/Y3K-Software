using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_GC_Quant
{
    public class Replicate
    {
        public string name;
        public string gcFeatFilePath;
        public string batchName;
        public string controlName;
        public int replicateID;
        public string replicateName;

        public int batchID;
        public Batch control;
        public Dictionary<int, QuantPoint> quantDictionary;
        public List<InternalStandard> internalStandards;

        public Replicate()
        {
            quantDictionary = new Dictionary<int, QuantPoint>();
            internalStandards = new List<InternalStandard>();
        }
    }
}
