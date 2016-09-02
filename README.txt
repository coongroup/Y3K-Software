README

**Metabolomic Quantitation**
Included here are tools for performing metabolomic quantitation using Thermo Fisher Q-Exactive GC Data files.
Both C# solution files, as well as compiled executables for these software tools (Y3K-Deconvolution Engine,
Y3K-Deconvolution Studio, and Y3K GC Quant) are provided. Additionally, a guide for using these tools is
provided 'Y3K GC Quantitation Pipeline User Guide.pdf'.

**Y3K Data Analyses**
We have provided a single project solution (Y3K_Data_Analyses) which contains code for classification
of RDR-associated molecules, genotype-phenotype outlier analysis, molecule covariance network analysis,
and KO-KO correlation analysis. This software tools reads in data from tab-delimited text files which
are provide here in the 'Associated Data Files' directory. Note: The data within these text files
was taken directly from Supplementary Table 3. To enable proper execution, a single molecule name was 
changed in the fermentation data files as two entries were labeled 'Glyercic acid' without any notes on
the number of associated TMS tags. 

If you encounter any issues with these software tools please contact us at y3kcontact@gmail.com. Any necessary
updates will be pushed to a public repository at https://github.com/coongroup.

