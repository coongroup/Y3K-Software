using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y3K_Data_Analyses
{
    class Molecule
    {
        //Unique molecule identifier
        public int molecule_id;

        //Classifier indicating whether the molecule is a PROTEIN, METABOLITE, or LIPID
        public string molecule_type;

        //Full name given to the molecule
        public string molecule_name;

        //Standard gene name of the molecule (applicable to proteins)
        public string gene_name;

        //Systematic gene name of the molecule (applicable to proteins)
        public string systematic_name;

        //FASTA header associated with the molecule (applicable to proteins)
        public string fasta_header;

        //Uniprot identifier of the molecule (applicable to proteins)
        public string uniprot_id;

        //0/1 indicating whether the molecule is associated with the respiration deficient response signature
        public int is_rd;

        //Average log2 fold change (triplicate measurements normalized to WT) considering all strains classified as RD where quantified 
        public double avg_rd_response;

        public Molecule(int ID, string Type, string Name, string Gene, string Sys, string Uniprot)
        {
            molecule_id = ID;
            molecule_type = Type;
            molecule_name = Name;
            gene_name = Gene;
            systematic_name = Sys;
            uniprot_id = Uniprot;
        }

        public Molecule() { }
    }
}
