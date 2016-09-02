using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Y3K_Deconvolution_Studio
{
    class UpdateMaster
    {
        public static void UpdateGCMastDatabase(Master master)
        {
            using (var transaction = master.conn.BeginTransaction())
            {
                List<EISpectrum> newSpectra = master.allSpectra.Where(x => x.userAdded).ToList();
                foreach (var spec in newSpectra)
                {
                    var insertText = "INSERT INTO featureGroupTable (GroupID, NumPeaks, ApexRT, IncludedFeatures, PeakList)"
                            + " VALUES (@GroupID, @NumPeaks, @ApexRT, @IncludedFeatures, @PeakList)";
                    var insertCommand = new SQLiteCommand(insertText, master.conn);
                    insertCommand.Parameters.AddWithValue("@GroupID", spec.FeatureGroup.groupID);
                    insertCommand.Parameters.AddWithValue("@NumPeaks", spec.FinalEIPeaks.Count);
                    insertCommand.Parameters.AddWithValue("@ApexRT", spec.ApexTimeEI);
                    var includedFeatures = "";
                    foreach (var feat in spec.FeatureGroup.allFeatures)
                    {
                        includedFeatures += feat.ID_Number + ";";
                    }
                    insertCommand.Parameters.AddWithValue("@IncludedFeatures", includedFeatures);
                    var peakList = "";
                    foreach (var peak in spec.FinalEIPeaks)
                    {
                        peakList += peak.MZ + "," + peak.Intensity + ";";
                    }
                    insertCommand.Parameters.AddWithValue("@PeakList", peakList);
                    insertCommand.ExecuteNonQuery();
                    spec.userAdded = false;
                }
                foreach (var spec in master.allSpectra)
                {

                    if (string.IsNullOrEmpty(spec.UserName))
                    {
                        spec.UserName = null;
                    }

                    var updateText =
                        "UPDATE featureGroupTable SET NumPeaks=@NumPeaks, Name=@Name, ChEBI_ID=@chebiID, PeakList=@PeakList, IncludedFeatures=@includedFeatures, QuantIons=@quantIons, IsValid=@isValid, IsInternalStandard=@internalStandard WHERE GroupID=@groupID";
                    var updateCommand = new SQLiteCommand(updateText, master.conn);
                    updateCommand.Parameters.AddWithValue("@NumPeaks", spec.FinalEIPeaks.Count);
                    updateCommand.Parameters.AddWithValue("@Name", spec.UserName);
                    updateCommand.Parameters.AddWithValue("@chebiID", spec.chebiID);
                    var includedLine = "";
                    foreach (var feat in spec.FeatureGroup.allFeatures)
                    {
                        includedLine += feat.ID_Number + ";";
                    }
                    updateCommand.Parameters.AddWithValue("@includedFeatures", includedLine);
                    var quantIonLine = "";
                    foreach (var ion in spec.quantIons)
                    {
                        quantIonLine += ion.MZ + "," + ion.Intensity + ";";
                    }
                    var peakLine = "";
                    foreach (var peak in spec.FinalEIPeaks)
                    {
                        peakLine += peak.MZ + "," + peak.Intensity + ";";
                    }
                    updateCommand.Parameters.AddWithValue("@PeakList", peakLine);
                    updateCommand.Parameters.AddWithValue("@quantIons", quantIonLine);
                    updateCommand.Parameters.AddWithValue("@isValid", spec.isValid);
                    updateCommand.Parameters.AddWithValue("@internalStandard", spec.isInternalStandard);
                    updateCommand.Parameters.AddWithValue("@groupID", spec.FeatureGroup.groupID);
                    updateCommand.ExecuteNonQuery();
                }
                transaction.Commit();
                transaction.Dispose();
            }
        }
    }
}
