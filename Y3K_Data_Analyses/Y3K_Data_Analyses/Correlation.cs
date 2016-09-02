using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Data_Analyses
{
    //Data object created to facilitate molecule covariance network analysis (MCNA)
    class Correlation
    {
        //Unique molecule identifier (first molecule being compared)
        public int mol_id_a;

        //Unique molecule identifier (second molecule being compared)
        public int mol_id_b;

        //Number of strains wherein both molecules were quantified (within a single growth condition)
        public int data_points;

        //Spearman's rho test statistic
        public double correlation;

        //P-value from a two-tailed t-test
        public double p_value;

        //Bonferroni-adjusted p-value
        public double bonferroni_p_value;
    }
}
