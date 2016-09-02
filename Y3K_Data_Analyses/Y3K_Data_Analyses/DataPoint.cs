using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Data_Analyses
{
    class DataPoint
    {
        //Data object used for the unique genotype-phenotype scanning process

        //Unique identifier of the associated strain 
        public int strain_id;

        //Log2 fold change (triplicate abundance measurements normalized to WT)
        public double lfq;

        //P-value (two-sided t-test, triplicate KO measurements compared to triplicate WT measurements)
        public double p_value;

        //Normalized Log2 fold change (max fold change set to 1/-1, respectively)
        public double lfq_relative;

        //Normalized P-value (max p-value set to 1/-1, respectively)
        public double p_value_relative;

        //Distance to nearest neighbor in normalized lfq space
        public double lfq_relative_difference;

        //Distance to nearest neighbor in normalized p-value space 
        public double p_value_relative_difference;

        //Distance to nearest neighbor in Euclidean space
        public double minimum_distance;
    }
}
