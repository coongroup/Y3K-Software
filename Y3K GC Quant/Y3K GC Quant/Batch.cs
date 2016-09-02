using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_GC_Quant
{
    public class Batch
    {
        public List<Replicate> replicates;
        public Dictionary<int, AvgQuantPoint> avgQuantDict;
        public string name;
        public int batchID;
        public Batch control;
        public int controlID;

        public Batch()
        {
            replicates = new List<Replicate>();
            avgQuantDict = new Dictionary<int, AvgQuantPoint>();
        }
    }
}
