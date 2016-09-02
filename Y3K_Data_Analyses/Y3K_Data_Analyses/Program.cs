using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Meta.Numerics.Statistics;
using System.IO;

namespace Y3K_Data_Analyses
{
    class Program
    {
        //Dictionary containing all descriptive data associated with molecules quantified across all strains in the Y3K dataset
        public static Dictionary<int, Molecule> molecule_dictionary = new Dictionary<int,Molecule>();

        //Dictionary containing all quantitative data associated with profiled strains in the Y3K dataset
        public static Dictionary<int, Strain> strain_dictionary = new Dictionary<int,Strain>();

        static void Main(string[] args)
        {

            //Populate molecule_dictionary and strain_dictionary with project data
            ReadInDataFiles();

            //Define which molecules are associated with the respiration deficiency response (RDR) and store that data in molecule_dictionary
            DefineRDRAssociatedMolecules();

            //Perform unique genotype-phenotype scanning procedure to identify target molecules of interest.
            DefineUniqueGenotypePhenotypeRelationships();

            //Perform molecule covariance network analysis of all quantified molecules.
            PerformMoleculeCovarianceNetworkAnalysis();

            //Perform KO-KO correlation analysis of all protein perturbation profiles
            PerformCorrelationAnalyses("PROTEIN");

            //Perform KO-KO correlation analysis of all metabolite perturbation profiles
            PerformCorrelationAnalyses("METABOLITE");

            //Perform KO-KO correlation analysis of all lipid perturbation profiles
            PerformCorrelationAnalyses("LIPID");
        }

        //Publc method designed to read in quantitative data from Y3K from tab-delimited text files (Resp_FoldChange, Resp_PValue, Ferm_FoldChange, Ferm_PValue, Strain_RDStatus)
        public static void ReadInDataFiles()
        {
            var resp_fold_change_filepath = @"C:\Users\Nick\Downloads\Y3K_Resp_FoldChange.txt";
            var resp_p_value_filepath = @"C:\Users\Nick\Downloads\Y3K_Resp_PValues.txt";
            var ferm_fold_change_filepath = @"C:\Users\Nick\Downloads\Y3K_Ferm_FoldChange.txt";
            var ferm_p_value_filepath = @"C:\Users\Nick\Downloads\Y3K_Ferm_PValues.txt";
            var strain_rd_filepath = @"C:\Users\Nick\Downloads\Y3K_Strain_RDStatus.txt";

            Dictionary<string, int> molecule_name_to_id_dict = new Dictionary<string, int>();
            Dictionary<string, int> strain_name_to_id_dict = new Dictionary<string, int>();
            Dictionary<int, int> strain_index_to_id_dict = new Dictionary<int, int>();
            Dictionary<int, string> molecule_index_to_name_dict = new Dictionary<int, string>();

            var reader = new StreamReader(resp_fold_change_filepath);
            var header = reader.ReadLine();

            int strain_index = 1;
            int molecule_index = 1;
            var header_parts = header.Split('\t').ToList();
            for (int i = 2; i < header_parts.Count; i++)
            {
                var curr_strain_name = header_parts[i].Replace("∆", "").Replace("?", "");
                if (!strain_name_to_id_dict.ContainsKey(curr_strain_name))
                {
                    var new_strain = new Strain();
                    new_strain.name = curr_strain_name;
                    new_strain.id = strain_index;
                    strain_dictionary.Add(strain_index, new_strain);
                    strain_index_to_id_dict.Add(i, strain_index);
                    strain_name_to_id_dict.Add(curr_strain_name, strain_index);
                    strain_index++;
                }
            }

            while (reader.Peek() > -1)
            {
                var current_line = reader.ReadLine();
                var current_line_parts = current_line.Split('\t').ToList();
                var molecule_name = current_line_parts[0];
                var molecule_type = current_line_parts[1].ToUpper();

                if (!molecule_name_to_id_dict.ContainsKey(molecule_name))
                {
                    var new_molecule = new Molecule();
                    new_molecule.molecule_name = molecule_name;
                    new_molecule.molecule_type = molecule_type;
                    molecule_dictionary.Add(molecule_index, new_molecule);
                    molecule_index_to_name_dict.Add(molecule_index, molecule_name);
                    molecule_name_to_id_dict.Add(molecule_name, molecule_index);
                    molecule_index++;
                }

                for (int i = 2; i < current_line_parts.Count; i++)
                {
                    if (!string.IsNullOrEmpty(current_line_parts[i]))
                    {
                        var fold_change = double.Parse(current_line_parts[i]);
                        var strain_id = strain_index_to_id_dict[i];
                        var molecule_id = molecule_name_to_id_dict[molecule_name];
                        strain_dictionary[strain_id].resp_lfq_dict.Add(molecule_id, fold_change);
                    }
                }
            }

            reader = new StreamReader(resp_p_value_filepath);
            reader.ReadLine();

            while (reader.Peek() > -1)
            {
                var current_line = reader.ReadLine();
                var current_line_parts = current_line.Split('\t').ToList();
                var molecule_name = current_line_parts[0];
                var molecule_type = current_line_parts[1].ToUpper();
                var molecule_id = molecule_name_to_id_dict[molecule_name];
                for (int i = 2; i < current_line_parts.Count; i++)
                {
                    if (!string.IsNullOrEmpty(current_line_parts[i]))
                    {
                        var p_value = double.Parse(current_line_parts[i]);
                        var strain_id = strain_index_to_id_dict[i];
                        strain_dictionary[strain_id].resp_p_val_dict.Add(molecule_id, p_value);
                    }
                }
            }

            reader = new StreamReader(ferm_fold_change_filepath);
            reader.ReadLine();

            while (reader.Peek() > -1)
            {
                var current_line = reader.ReadLine();
                var current_line_parts = current_line.Split('\t').ToList();
                var molecule_name = current_line_parts[0];
                var molecule_type = current_line_parts[1].ToUpper();

                if (!molecule_name_to_id_dict.ContainsKey(molecule_name))
                {
                    var new_molecule = new Molecule();
                    new_molecule.molecule_name = molecule_name;
                    new_molecule.molecule_type = molecule_type;
                    molecule_dictionary.Add(molecule_index, new_molecule);
                    molecule_index_to_name_dict.Add(molecule_index, molecule_name);
                    molecule_name_to_id_dict.Add(molecule_name, molecule_index);
                    molecule_index++;
                }

                for (int i = 2; i < current_line_parts.Count; i++)
                {
                    if (!string.IsNullOrEmpty(current_line_parts[i]))
                    {
                        var fold_change = double.Parse(current_line_parts[i]);
                        var strain_id = strain_index_to_id_dict[i];
                        var molecule_id = molecule_name_to_id_dict[molecule_name];
                        strain_dictionary[strain_id].ferm_lfq_dict.Add(molecule_id, fold_change);
                    }
                }
            }

            reader = new StreamReader(ferm_p_value_filepath);
            reader.ReadLine();

            while (reader.Peek() > -1)
            {
                var current_line = reader.ReadLine();
                var current_line_parts = current_line.Split('\t').ToList();
                var molecule_name = current_line_parts[0];
                var molecule_type = current_line_parts[1].ToUpper();
                var molecule_id = molecule_name_to_id_dict[molecule_name];
                for (int i = 2; i < current_line_parts.Count; i++)
                {
                    if (!string.IsNullOrEmpty(current_line_parts[i]))
                    {
                        var p_value = double.Parse(current_line_parts[i]);
                        var strain_id = strain_index_to_id_dict[i];
                        strain_dictionary[strain_id].ferm_p_val_dict.Add(molecule_id, p_value);
                    }
                }
            }

            reader = new StreamReader(strain_rd_filepath);
            reader.ReadLine();

            while (reader.Peek() > -1)
            {
                var current_line = reader.ReadLine();
                var current_line_parts = current_line.Split('\t').ToList();
                var strain_name = current_line_parts[0];
                var strain_rd_status = int.Parse(current_line_parts[1]);
                var strain_id = strain_name_to_id_dict[strain_name];
                strain_dictionary[strain_id].is_rd = strain_rd_status;
            }
        }

        //Public method designed to define all RDR associated molecules.
        public static void DefineRDRAssociatedMolecules()
        {
            //Identify strains classifed as respiration deficient
            var rd_strains = strain_dictionary.Values.Where(x => x.is_rd == 1).ToList();

            //Identify molecules quantified across RD strains (minimum of 1 RD strain) and store unique identifiers
            HashSet<int> all_profiled_molecule_ids = new HashSet<int>();
            foreach (var strain in rd_strains)
            {
                foreach (var molecule_id in strain.resp_lfq_dict.Keys)
                {
                    all_profiled_molecule_ids.Add(molecule_id);
                }
            }

            //For each molecule quantified in at least one RD strain, calculate the number of times that molecule was upregulated/downregulated relative to WT
            //Store the measured fold change for each quantified datapoint as well
            foreach (var molecule_id in all_profiled_molecule_ids)
            {
                List<double> all_fold_changes = new List<double>();
                int negative_fold_change = 0;
                int positive_fold_change = 0;

                foreach (var strain in rd_strains)
                {
                    if (strain.resp_lfq_dict.ContainsKey(molecule_id))
                    {
                        all_fold_changes.Add(strain.resp_lfq_dict[molecule_id]);
                        if (strain.resp_lfq_dict[molecule_id]<0)
                        {
                            negative_fold_change++;
                        }
                        else
                        {
                            positive_fold_change++;
                        }
                    }
                }

                int max_fold_changes = Math.Max(negative_fold_change, positive_fold_change);
                double percent_consistent_changes = ((double)max_fold_changes) / ((double)(negative_fold_change + positive_fold_change)) * 100;

                //If the molecule was consistently upregulated/downregulated (consistent regulation across 95+% of strains where quantified)
                //then classify the molecule as RDR-associated, and store the average fold change response across all RDR strain (where quantified)
                if (percent_consistent_changes >= 95)
                {
                    molecule_dictionary[molecule_id].is_rd = 1;
                    molecule_dictionary[molecule_id].avg_rd_response = all_fold_changes.Average();
                }
            }

            var rdr_molecules = molecule_dictionary.Values.Where(x => x.is_rd == 1).ToList();
        }

        //Public method to perform unique genotype-phenotype scanning analysis. No output is written in the current implementation of the method.
        public static void DefineUniqueGenotypePhenotypeRelationships()
        {
            //Create local dictionaries to hold sets of quantitative data points (stored as GenotypePhenotypeMolecule objects).
            var respiration_data_point_dictionary = new Dictionary<int, GenotypePhenotypeMolecule>();
            var fermentation_data_point_dictionary = new Dictionary<int, GenotypePhenotypeMolecule>();

            //Organize all quantitative data by molecule & growth condition. Each data point is stored as a DataPoint object with an associated strain identifier.
            foreach (var strain in strain_dictionary.Values)
            {
                foreach (var entry in strain.resp_lfq_dict)
                {
                    if (! respiration_data_point_dictionary.ContainsKey(entry.Key))
                    {
                        var new_genotype_phenotype_molecule = new GenotypePhenotypeMolecule();
                        new_genotype_phenotype_molecule.id = entry.Key;
                        respiration_data_point_dictionary.Add(entry.Key, new_genotype_phenotype_molecule);
                    }
                    var new_data_point = new DataPoint();
                    new_data_point.lfq = entry.Value;
                    new_data_point.strain_id = strain.id;
                    new_data_point.p_value = -Math.Log10(strain.resp_p_val_dict[entry.Key]);
                    respiration_data_point_dictionary[entry.Key].data_points.Add(new_data_point);
                }

                foreach (var entry in strain.ferm_lfq_dict)
                {
                    if (!fermentation_data_point_dictionary.ContainsKey(entry.Key))
                    {
                        var new_genotype_phenotype_molecule = new GenotypePhenotypeMolecule();
                        new_genotype_phenotype_molecule.id = entry.Key;
                        fermentation_data_point_dictionary.Add(entry.Key, new_genotype_phenotype_molecule);
                    }
                    var new_data_point = new DataPoint();
                    new_data_point.lfq = entry.Value;
                    new_data_point.strain_id = strain.id;
                    new_data_point.p_value = -Math.Log10(strain.ferm_p_val_dict[entry.Key]);
                    fermentation_data_point_dictionary[entry.Key].data_points.Add(new_data_point);
                }
            }

            //Perform all normalization & euclidean distance calculations, then return information on outlier distance/strain ID - respiration 
            foreach (var molecule in respiration_data_point_dictionary.Values)
            {
                var down_regulated_data_points = molecule.data_points.Where(x => x.lfq < 0).ToList();
                var up_regulated_data_points = molecule.data_points.Where(x => x.lfq > 0).ToList();

                var down_regulation_analysis_results = DoEuclideanDistanceCalculation(down_regulated_data_points);
                molecule.max_down_regulated_distance = down_regulation_analysis_results.Item1;
                molecule.max_down_regulated_strain = down_regulation_analysis_results.Item2;
                var up_regulation_analysis_results = DoEuclideanDistanceCalculation(up_regulated_data_points);
                molecule.max_up_regulated_distance = up_regulation_analysis_results.Item1;
                molecule.max_up_regulated_strain_id = up_regulation_analysis_results.Item2;
            }

            //Perform all normalization & euclidean distance calculations, then return information on outlier distance/strain ID - fermentation 
            foreach (var molecule in fermentation_data_point_dictionary.Values)
            {
                var down_regulated_data_points = molecule.data_points.Where(x => x.lfq < 0).ToList();
                var up_regulated_data_points = molecule.data_points.Where(x => x.lfq > 0).ToList();

                var down_regulation_analysis_results = DoEuclideanDistanceCalculation(down_regulated_data_points);
                molecule.max_down_regulated_distance = down_regulation_analysis_results.Item1;
                molecule.max_down_regulated_strain = down_regulation_analysis_results.Item2;
                var up_regulation_analysis_results = DoEuclideanDistanceCalculation(up_regulated_data_points);
                molecule.max_up_regulated_distance = up_regulation_analysis_results.Item1;
                molecule.max_up_regulated_strain_id = up_regulation_analysis_results.Item2;
            }

            //Identify molecules with unique genotype-phenotype relationships. Note, the code below does not exclude instances where the outlier molecule 
            //corresponds to the gene deleted.
            foreach (var molecule in respiration_data_point_dictionary.Values)
            {
                if (molecule.max_up_regulated_strain_id != 0)
                {
                    double up_regulated_p_value = molecule.data_points.Where(x => x.strain_id == molecule.max_up_regulated_strain_id).ToList().First().p_value;
                    if (up_regulated_p_value > -Math.Log10(0.05) && molecule.max_up_regulated_distance > 0.7)
                    {
                        molecule.has_unique_up_regulated_data_point = true;
                    }
                }
               if (molecule.max_down_regulated_strain != 0)
               {
                   double down_regulated_p_value = molecule.data_points.Where(x => x.strain_id == molecule.max_down_regulated_strain).ToList().First().p_value;
                   if (down_regulated_p_value > -Math.Log10(0.05) && molecule.max_down_regulated_distance > 0.7)
                   {
                       molecule.has_unique_down_regulated_data_point = true;
                   }
               }
            }

            foreach (var molecule in fermentation_data_point_dictionary.Values)
            {
                if (molecule.max_up_regulated_strain_id != 0)
                {
                    double up_regulated_p_value = molecule.data_points.Where(x => x.strain_id == molecule.max_up_regulated_strain_id).ToList().First().p_value;
                    if (up_regulated_p_value > -Math.Log10(0.05) && molecule.max_up_regulated_distance > 0.7)
                    {
                        molecule.has_unique_up_regulated_data_point = true;
                    }
                }
                if (molecule.max_down_regulated_strain != 0)
                {
                    double down_regulated_p_value = molecule.data_points.Where(x => x.strain_id == molecule.max_down_regulated_strain).ToList().First().p_value;
                    if (down_regulated_p_value > -Math.Log10(0.05) && molecule.max_down_regulated_distance > 0.7)
                    {
                        molecule.has_unique_down_regulated_data_point = true;
                    }
                }
            }
        }

        //Helper method for DefineUniqueGenotypePhenotypeRelationships(). Performs all required euclidean distance calculations to 
        //find unique outlers in the genotype-phenotype scanning approach. For any molecule, the method accepts all up-regulated, or
        //all down-regulated measurements with associated strain identifiers. The method returns the largest euclidean distance to a particular
        //outlier as well as that outlier's associated strain identifier.
        public static Tuple<double, int> DoEuclideanDistanceCalculation(List<DataPoint> data_points)
        {
            //Find max fold change and p-value (-log10 transformed)
            double max_fold_change = 0.0;
            double max_p_value = 0.0;
            foreach (var data_point in data_points)
            {
                max_fold_change = Math.Max(Math.Abs(data_point.lfq), max_fold_change);
                max_p_value = Math.Max(Math.Abs(data_point.p_value), max_p_value);
            }

            //Normalize all data points such that the largest fold change and p-value are equal to 1
            foreach (var data_point in data_points)
            {
                data_point.lfq_relative = 1 - ((max_fold_change - Math.Abs(data_point.lfq)) / max_fold_change);
                data_point.p_value_relative = 1 - ((max_p_value - Math.Abs(data_point.p_value)) / max_p_value);
            }

            //Calculate a distance from the origin for each data point
            foreach (var data_point in data_points)
            {
                data_point.minimum_distance = Math.Pow(data_point.lfq_relative, 2) + Math.Pow(data_point.p_value_relative, 2);
            }

            //Order all data points (descending) by distance from origin
            data_points = data_points.OrderByDescending(x => x.lfq_relative).ToList();

            var max_distance = 0.0;
            var max_distance_strain_id = 0;

            //For the three furthest data points, calculate the distance to their nearest (closer) neighbor
            if (data_points.Count > 2)
            {
                for (int i = 0; i < data_points.Count; i++)
                {
                    var current_minimum = double.MaxValue;
                    for (int j = i+1; j < data_points.Count; j++)
                    {
                        var data_point = data_points[j];
                        if (data_point.strain_id != data_points[i].strain_id)
                        {
                            var current_relative_lfq_distance = Math.Abs(data_point.lfq_relative - data_points[i].lfq_relative);
                            var current_relative_p_value = Math.Abs(data_point.p_value_relative - data_points[i].p_value_relative);
                            current_relative_lfq_distance = Math.Pow(current_relative_lfq_distance, 2);
                            current_relative_p_value = Math.Pow(current_relative_p_value, 2);
                            var current_relative_distance = current_relative_p_value + current_relative_lfq_distance;
                            current_relative_distance = Math.Sqrt(current_relative_distance);
                            current_minimum = Math.Min(current_minimum, current_relative_distance);
                        }
                    }
                    if (current_minimum > max_distance)
                    {
                        max_distance_strain_id = data_points[i].strain_id;
                    }
                    max_distance = Math.Max(current_minimum, max_distance);
                    if (i == 2)
                    {
                        break;
                    }
                }
            }
            //Return the largest outlier distance, and associated strain ID
            var return_tuple = new Tuple<double, int>(max_distance, max_distance_strain_id);
            return return_tuple;
        }


        //Public method for performing all molecule covariance analysis. This method calcualtes covariance between all pairs of molecules quantified in the same growth condition.
        public static void PerformMoleculeCovarianceNetworkAnalysis()
        {
            //Perform covariance analysis for all respiration data
            List<Correlation> respiration_correlations = new List<Correlation>();
            HashSet<int> respiration_molecule_identifiers = new HashSet<int>();
            foreach (var strain in strain_dictionary.Values)
            {
                foreach (var entry in strain.resp_lfq_dict)
                {
                    respiration_molecule_identifiers.Add(entry.Key);
                }
            }

            //Logic described for respiration covariance analysis holds for fermentation and respiration-rdr analyses

            //Iterate over all pairs of molecules quantified in at least one strain the respiration dataset
            for (int i = 0; i < respiration_molecule_identifiers.Count; i++)
            {
                Console.WriteLine(i);
                for (int j = i + 1; j < respiration_molecule_identifiers.Count; j++)
                {

                    var molecule_identifier_a = respiration_molecule_identifiers.ToList()[i];
                    var molecule_identifier_b = respiration_molecule_identifiers.ToList()[j];

                    List<double> molecule_a_fold_changes = new List<double>();
                    List<double> molecule_b_fold_changes = new List<double>();

                    //For each strain check whether both of the molecules in question were quantified in that particular strain.
                    foreach (var strain in strain_dictionary.Values)
                    {
                        //If yes, add measured fold changes to the lists above
                        if (strain.resp_lfq_dict.ContainsKey(molecule_identifier_a) && strain.resp_lfq_dict.ContainsKey(molecule_identifier_b))
                        {
                            molecule_a_fold_changes.Add(strain.resp_lfq_dict[molecule_identifier_a]);
                            molecule_b_fold_changes.Add(strain.resp_lfq_dict[molecule_identifier_b]);
                        }
                    }

                    //If both molecules in question were quantified in more than 9 strains profiled (under respirative conditions here), calculate a spearman's rho statistic and associated p-value (two-tailed t-test)
                    if (molecule_a_fold_changes.Count > 9)
                    {
                        var spearman_correlation = GetSpearmanCorrelation(molecule_a_fold_changes, molecule_b_fold_changes, molecule_identifier_a, molecule_identifier_b);
                        if (spearman_correlation != null)
                        {
                            respiration_correlations.Add(spearman_correlation);
                        }
                    }
                }
            }

            //To account for multiple testing, all p-values are adjusted using the Bonferroni correction
            foreach (var correlation in respiration_correlations)
            {
                correlation.bonferroni_p_value = correlation.p_value * ((double)respiration_correlations.Count);
            }

            //The correlations reported in the final dataset are those which meet the following criteria: Bonferroni-adjusted p-value < 0.001 and |spearman's rho| > 0.58 
            var valid_respiration_correlations = respiration_correlations.Where(x => x.bonferroni_p_value < 0.001 && (x.correlation > 0.58 || x.correlation < -0.58)).ToList();

            //All correlation data is cleared. IO commands should be added here if you wish to further analyze the data in question.
            respiration_correlations.Clear();
            valid_respiration_correlations.Clear();

            //Perform covariance analysis for all fermentation data
            List<Correlation> fermentation_correlations = new List<Correlation>();
            HashSet<int> fermentation_molecule_identifiers = new HashSet<int>();
            foreach (var strain in strain_dictionary.Values)
            {
                foreach (var entry in strain.ferm_lfq_dict)
                {
                    fermentation_molecule_identifiers.Add(entry.Key);
                }
            }

            for (int i = 0; i < fermentation_molecule_identifiers.Count; i++)
            {
                Console.WriteLine(i);
                for (int j = i + 1; j < fermentation_molecule_identifiers.Count; j++)
                {

                    var molecule_identifier_a = fermentation_molecule_identifiers.ToList()[i];
                    var molecule_identifier_b = fermentation_molecule_identifiers.ToList()[j];

                    List<double> molecule_a_fold_changes = new List<double>();
                    List<double> molecule_b_fold_changes = new List<double>();

                    foreach (var strain in strain_dictionary.Values)
                    {
                        if (strain.ferm_lfq_dict.ContainsKey(molecule_identifier_a) && strain.ferm_lfq_dict.ContainsKey(molecule_identifier_b))
                        {
                            molecule_a_fold_changes.Add(strain.ferm_lfq_dict[molecule_identifier_a]);
                            molecule_b_fold_changes.Add(strain.ferm_lfq_dict[molecule_identifier_b]);
                        }
                    }

                    if (molecule_a_fold_changes.Count > 9)
                    {
                        var spearman_correlation = GetSpearmanCorrelation(molecule_a_fold_changes, molecule_b_fold_changes, molecule_identifier_a, molecule_identifier_b);
                        if (spearman_correlation != null)
                        {
                            fermentation_correlations.Add(spearman_correlation);
                        }
                    }
                }
            }

            foreach (var correlation in fermentation_correlations)
            {
                correlation.bonferroni_p_value = correlation.p_value * ((double)fermentation_correlations.Count);
            }
            var valid_fermentation_correlations = fermentation_correlations.Where(x => x.bonferroni_p_value < 0.001 && (x.correlation > 0.58 || x.correlation < -0.58)).ToList();

            fermentation_correlations.Clear();
            valid_fermentation_correlations.Clear();

            //Perform covariance analysis for all respiration-rdr data
            List<Correlation> respiration_less_rdr_correlations = new List<Correlation>();
            HashSet<int> respiration_less_rdr_molecule_identifiers = new HashSet<int>();
            foreach (var strain in strain_dictionary.Values)
            {
                foreach (var entry in strain.resp_lfq_dict)
                {
                    respiration_less_rdr_molecule_identifiers.Add(entry.Key);
                }
            }

            for (int i = 0; i < respiration_less_rdr_molecule_identifiers.Count; i++)
            {
                Console.WriteLine(i);
                for (int j = i + 1; j < respiration_less_rdr_molecule_identifiers.Count; j++)
                {

                    var molecule_identifier_a = respiration_less_rdr_molecule_identifiers.ToList()[i];
                    var molecule_identifier_b = respiration_less_rdr_molecule_identifiers.ToList()[j];

                    List<double> molecule_a_fold_changes = new List<double>();
                    List<double> molecule_b_fold_changes = new List<double>();

                    foreach (var strain in strain_dictionary.Values)
                    {
                        if (strain.resp_lfq_dict.ContainsKey(molecule_identifier_a) && strain.resp_lfq_dict.ContainsKey(molecule_identifier_b))
                        {
                            if (molecule_dictionary[molecule_identifier_a].is_rd == 1 && strain.is_rd==1)
                            {
                                molecule_a_fold_changes.Add(strain.resp_lfq_dict[molecule_identifier_a] - molecule_dictionary[molecule_identifier_a].avg_rd_response);
                            }
                            else
                            {
                                molecule_a_fold_changes.Add(strain.resp_lfq_dict[molecule_identifier_a]);
                            }

                            if (molecule_dictionary[molecule_identifier_b].is_rd == 1 && strain.is_rd == 1)
                            {
                                molecule_b_fold_changes.Add(strain.resp_lfq_dict[molecule_identifier_b] - molecule_dictionary[molecule_identifier_b].avg_rd_response);
                            }
                            else
                            {
                                molecule_b_fold_changes.Add(strain.resp_lfq_dict[molecule_identifier_b]);
                            }
                        }
                    }

                    if (molecule_a_fold_changes.Count > 9)
                    {
                        var spearman_correlation = GetSpearmanCorrelation(molecule_a_fold_changes, molecule_b_fold_changes, molecule_identifier_a, molecule_identifier_b);
                        if (spearman_correlation != null)
                        {
                            respiration_less_rdr_correlations.Add(spearman_correlation);
                        }
                    }
                }
            }

            foreach (var correlation in respiration_less_rdr_correlations)
            {
                correlation.bonferroni_p_value = correlation.p_value * ((double)respiration_less_rdr_correlations.Count);
            }
            var valid_respiration_less_rdr_correlations = respiration_less_rdr_correlations.Where(x => x.bonferroni_p_value < 0.001 && (x.correlation > 0.58 || x.correlation < -0.58)).ToList();

            respiration_less_rdr_correlations.Clear();
            valid_respiration_less_rdr_correlations.Clear();
        }

        //Helpmer method for PerformMoleculeCovarianceNetworkAnalysis(). Takes paired fold changes for two molecules and calculates a spearman's rho coefficient and associated p-value.
        //These data are returned in a Corrleation data object.
        public static Correlation GetSpearmanCorrelation(List<double> molecule_a_fold_changes, List<double> molecule_b_fold_changes, int molecule_a_identifier, int molecule_b_identifier)
        {
            Correlation new_correlation = new Correlation();
            new_correlation.mol_id_a = molecule_a_identifier;
            new_correlation.mol_id_b = molecule_b_identifier;
            new_correlation.data_points = molecule_a_fold_changes.Count();
            var bs = new BivariateSample("dataOne", "dataTwo");
            int count = 0;

            for (int i = 0; i < molecule_a_fold_changes.Count; i++)
            {
                bs.Add(molecule_a_fold_changes[i], molecule_b_fold_changes[i]);
                count++;
            }
            var res = bs.SpearmanRhoTest();
            if (res.Statistic < 0)
            {
                new_correlation.correlation = res.Statistic;
                new_correlation.p_value = (2 * res.LeftProbability); //2-tailed t-test
            }
            else
            {
                new_correlation.correlation = res.Statistic;
                new_correlation.p_value = (2 * res.RightProbability); //2-tailed t-test
            }

            if (!double.IsNaN(new_correlation.correlation) && !double.IsNaN(new_correlation.p_value) && !double.IsInfinity(new_correlation.p_value) && !double.IsInfinity(new_correlation.correlation))
            {
                return new_correlation;
            }
            return null;
        }

        //Public method for calculating Pearson coefficients between all profiled KO strains in the specified ome (user will specify "PROTEIN", "METABOLITE", or "LIPID").
        public static void PerformCorrelationAnalyses(string ome)
        {
            var respiration_correlations = new List<Tuple<int, int, double>>();
            var fermentation_correlations = new List<Tuple<int, int, double>>();
            var respiration_less_rdr_correlations = new List<Tuple<int, int, double>>();

            //All logic for KO-KO correlation analysis described here holds for fermentation and respiration-RDR analyses as well
            for (int i = 0; i < strain_dictionary.Values.Count; i++)
            {
                for (int j = i + 1; j < strain_dictionary.Values.Count; j++)
                {
                    var strain_a = strain_dictionary.Values.ToList()[i];
                    var strain_b = strain_dictionary.Values.ToList()[j];

                    //Identify all molecules quantified in both strain_a and strain_b
                    var sharedKeys = strain_a.resp_lfq_dict.Keys.Where(x => strain_b.resp_lfq_dict.ContainsKey(x) && molecule_dictionary[x].molecule_type.Equals(ome)).ToList();

                    var strain_a_fold_changes = new List<double>();
                    var strain_b_fold_changes = new List<double>();

                    //Molecules with fold change >0.7 (log2 space) and p-value < 0.05 are stored and used for calculation of a Pearson coefficient
                    foreach (var key in sharedKeys)
                    {
                        var strain_a_fold_change = strain_a.resp_lfq_dict[key];
                        var strain_b_fold_change = strain_b.resp_lfq_dict[key];
                        var strain_a_p_value = strain_a.resp_p_val_dict[key];
                        var strain_b_p_value = strain_b.resp_p_val_dict[key];

                        if (Math.Abs(strain_a_fold_change) > 0.7 &&
                            Math.Abs(strain_b_fold_change) > 0.7 &&
                            strain_a_p_value < 0.05 &&
                            strain_b_p_value < 0.05)
                        {
                            strain_a_fold_changes.Add(strain_a_fold_change);
                            strain_b_fold_changes.Add(strain_b_fold_change);
                        }
                    }
                    //Minimum number of molecules required for calculation of a Pearson coefficient. Default value corresponds to "LIPID" type molecules.
                    int threshold = 5;
                    if (ome.Equals("METABOLITE"))
                    {
                        threshold = 10;
                    }
                    if (ome.Equals("PROTEIN"))
                    {
                        threshold = 20;
                    }

                    double slope = 0;
                    double r_squared = 0;
                    double y_intercept = 0;

                    //If a suitable number of molecules exist to allow for calculation of a Pearson coefficient, that value is calculated. Otherwise the correlation is reported as 0.
                    if (strain_a_fold_changes.Count >= threshold)
                    {
                        LinearRegression(strain_a_fold_changes.ToArray(), strain_b_fold_changes.ToArray(), 0, strain_a_fold_changes.Count, out r_squared, out y_intercept, out slope);
                    }

                    //If a negative correlation is calculated, the value is reported as zero.
                    if (r_squared < 0)
                    {
                        r_squared = 0;
                    }

                    //All pairs of correlations are stored as tuples (strain_a identifier, strain_b identifier, Pearson coefficient value).
                    respiration_correlations.Add(new Tuple<int, int, double>(strain_a.id, strain_b.id, r_squared));
                }
            }

            //Calculate all fermentation KO-KO correlations
            for (int i = 0; i < strain_dictionary.Values.Count; i++)
            {
                for (int j = i + 1; j < strain_dictionary.Values.Count; j++)
                {
                    var strain_a = strain_dictionary.Values.ToList()[i];
                    var strain_b = strain_dictionary.Values.ToList()[j];

                    var sharedKeys = strain_a.ferm_lfq_dict.Keys.Where(x => strain_b.ferm_lfq_dict.ContainsKey(x) && molecule_dictionary[x].molecule_type.Equals(ome)).ToList();

                    var strain_a_fold_changes = new List<double>();
                    var strain_b_fold_changes = new List<double>();

                    foreach (var key in sharedKeys)
                    {
                        var strain_a_fold_change = strain_a.ferm_lfq_dict[key];
                        var strain_b_fold_change = strain_b.ferm_lfq_dict[key];
                        var strain_a_p_value = strain_a.ferm_p_val_dict[key];
                        var strain_b_p_value = strain_b.ferm_p_val_dict[key];

                        if (Math.Abs(strain_a_fold_change) > 0.7 &&
                            Math.Abs(strain_b_fold_change) > 0.7 &&
                            strain_a_p_value < 0.05 &&
                            strain_b_p_value < 0.05)
                        {
                            strain_a_fold_changes.Add(strain_a_fold_change);
                            strain_b_fold_changes.Add(strain_b_fold_change);
                        }
                    }
                    //Minimum number of molecules required for calculation of a Pearson coefficient. Default value corresponds to "LIPID" type molecules.
                    int threshold = 5;
                    if (ome.Equals("METABOLITE"))
                    {
                        threshold = 10;
                    }
                    if (ome.Equals("PROTEIN"))
                    {
                        threshold = 20;
                    }

                    double slope = 0;
                    double r_squared = 0;
                    double y_intercept = 0;

                    if (strain_a_fold_changes.Count >= threshold)
                    {
                        LinearRegression(strain_a_fold_changes.ToArray(), strain_b_fold_changes.ToArray(), 0, strain_a_fold_changes.Count, out r_squared, out y_intercept, out slope);
                    }

                    if (r_squared < 0)
                    {
                        r_squared = 0;
                    }

                    fermentation_correlations.Add(new Tuple<int, int, double>(strain_a.id, strain_b.id, r_squared));
                }
            }

            //Calculate all respiration-RDR KO-KO correlations
            for (int i = 0; i < strain_dictionary.Values.Count; i++)
            {
                for (int j = i + 1; j < strain_dictionary.Values.Count; j++)
                {
                    var strain_a = strain_dictionary.Values.ToList()[i];
                    var strain_b = strain_dictionary.Values.ToList()[j];

                    var sharedKeys = strain_a.resp_lfq_dict.Keys.Where(x => strain_b.resp_lfq_dict.ContainsKey(x) && molecule_dictionary[x].molecule_type.Equals(ome)).ToList();

                    var strain_a_fold_changes = new List<double>();
                    var strain_b_fold_changes = new List<double>();

                    foreach (var key in sharedKeys)
                    {
                        var strain_a_fold_change = strain_a.resp_lfq_dict[key];
                        var strain_b_fold_change = strain_b.resp_lfq_dict[key];
                        var strain_a_p_value = strain_a.resp_p_val_dict[key];
                        var strain_b_p_value = strain_b.resp_p_val_dict[key];

                        //For R-RDR analysis we utilize the fold change and p-value prior to RDR normalization in our thresholding criteria
                        if (Math.Abs(strain_a_fold_change) > 0.7 &&
                            Math.Abs(strain_b_fold_change) > 0.7 &&
                            strain_a_p_value < 0.05 &&
                            strain_b_p_value < 0.05)
                        {
                            //RDR normalization is performed here.
                            if (strain_a.is_rd == 1 && molecule_dictionary[key].is_rd == 1)
                            {
                                strain_a_fold_changes.Add(strain_a_fold_change - molecule_dictionary[key].avg_rd_response);
                            }
                            else
                            {
                                strain_a_fold_changes.Add(strain_a_fold_change);
                            }

                            if (strain_b.is_rd == 1 && molecule_dictionary[key].is_rd == 1)
                            {
                                strain_b_fold_changes.Add(strain_b_fold_change - molecule_dictionary[key].avg_rd_response);
                            }
                            else
                            {
                                strain_b_fold_changes.Add(strain_b_fold_change);
                            }
                        }
                    }

                    //Minimum number of molecules required for calculation of a Pearson coefficient. Default value corresponds to "LIPID" type molecules.
                    int threshold = 5;
                    if (ome.Equals("METABOLITE"))
                    {
                        threshold = 10;
                    }
                    if (ome.Equals("PROTEIN"))
                    {
                        threshold = 20;
                    }

                    double slope = 0;
                    double r_squared = 0;
                    double y_intercept = 0;

                    if (strain_a_fold_changes.Count >= threshold)
                    {
                        LinearRegression(strain_a_fold_changes.ToArray(), strain_b_fold_changes.ToArray(), 0, strain_a_fold_changes.Count, out r_squared, out y_intercept, out slope);
                    }

                    if (r_squared < 0)
                    {
                        r_squared = 0;
                    }

                    respiration_correlations.Add(new Tuple<int, int, double>(strain_a.id, strain_b.id, r_squared));
                }
            }
        }

        //Helper method for PerformCorrelationAnalyses(string ome). Calculates a linear fit for all KO-KO comparisons and returns a slope, y-intercept, and Pearson coefficient (R-Squared value)
        public static void LinearRegression(double[] xVals, double[] yVals,
                                        int inclusiveStart, int exclusiveEnd,
                                        out double rsquared, out double yintercept,
                                        out double slope)
        {
            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double ssX = 0;
            double ssY = 0;
            double sumCodeviates = 0;
            double sCo = 0;
            double count = exclusiveEnd - inclusiveStart;

            for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
            {
                double x = xVals[ctr];
                double y = yVals[ctr];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }
            ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
            double RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            double RDenom = (count * sumOfXSq - (sumOfX * sumOfX))
             * (count * sumOfYSq - (sumOfY * sumOfY));
            sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            double meanX = sumOfX / count;
            double meanY = sumOfY / count;
            double dblR = RNumerator / Math.Sqrt(RDenom);
            rsquared = dblR * dblR;
            yintercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }
    }
}
