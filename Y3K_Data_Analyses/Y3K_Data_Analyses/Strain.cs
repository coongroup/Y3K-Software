using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Data_Analyses
{
    class Strain
    {
        //Unique Strain Identifier
        public int id;

        //0/1 indicating whether the strain is respiration deficient or not.
        public int is_rd;

        //Standard delete gene name
        public string name;

        //Dictionary containing all triplicate averaged LFQ abundance fold change values (log2 transformed and normalized to WT) from the respiration dataset  [(molecule id, fold change)]
        public Dictionary<int, double> resp_lfq_dict = new Dictionary<int, double>();

        //Dictionary containing all p-values associated with fold changes in the above dictionary (Respiration) [(molecule id, p-value)]
        public Dictionary<int, double> resp_p_val_dict = new Dictionary<int, double>();

        //Dictionary containing all standard deviations associated with triplicate LFQ values in the above dictionary (Respiration)  [(molecule id, standard deviation)]
        public Dictionary<int, double> resp_std_dev_dict = new Dictionary<int, double>();

        //Dictionary containing all triplicate averaged LFQ abundance fold change values (log2 transformed and normalized to WT) from the fermentation dataset  [(molecule id, fold change)]
        public Dictionary<int, double> ferm_lfq_dict = new Dictionary<int, double>();

        //Dictionary containing all p-values associated with fold changes in the above dictionary (Fermentation)  [(molecule id, p-value)]
        public Dictionary<int, double> ferm_p_val_dict = new Dictionary<int, double>();

        //Dictionary containing all standard deviations associated with triplicate LFQ values in the above dictionary (Fermentation)  [(molecule id, standard deviation)]
        public Dictionary<int, double> ferm_std_dev_dict = new Dictionary<int, double>();

        //Dictionary containing all RDR normalized abundance values (only applicable if this strain is in fact respiration deficient)  [(molecule id, fold change)]
        public Dictionary<int, double> rdr_lfq_dict = new Dictionary<int, double>();
    }
}
