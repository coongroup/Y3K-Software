using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Data_Analyses
{
    //Data object useful for performing Unique Genotype-Phenotype Scanning process
    class GenotypePhenotypeMolecule
    {
        //Unique molecule identifier
        public int id;

        //List of all quantiative data measurements of this specific molecule
        public List<DataPoint> data_points = new List<DataPoint>();

        //Boolean indicating whether this molecule has a uniquely up-regulated data point(s). Helpful for quick sorting.
        public bool has_unique_up_regulated_data_point = false;

        //Boolean indicating whether this molecule has a unique down-regulated data point(s). Helpful for quick sorting.
        public bool has_unique_down_regulated_data_point = false;

        //Euclidean distance of the largest up-regulated outlier.
        public double max_up_regulated_distance;

        //Euclidean distance of the largest down-regulated outlier.
        public double max_down_regulated_distance;

        //Unique strain identifier of the largest up-regulated outlier.
        public int max_up_regulated_strain_id;

        //Unique strain identifier of the largest down-regualted outlier.
        public int max_down_regulated_strain;
    }
}
