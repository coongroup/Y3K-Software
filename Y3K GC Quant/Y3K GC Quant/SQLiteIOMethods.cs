using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;

namespace Y3K_GC_Quant
{
    public class SQLiteIOMethods
    {
        private static string CreateReplicateTableText = "CREATE TABLE IF NOT EXISTS Replicate_Table (Name TEXT, Replicate_ID INT, GCFeatPath TEXT," +
                                                          " BatchName TEXT, Batch_ID INT, ControlBatchName TEXT, ControlBatch_ID INT, IsQuantified BOOLEAN)";
        private static string CreateBatchTableText = "CREATE TABLE IF NOT EXISTS Batch_Table (Name TEXT, Batch_ID INT, ReplicateIDs TEXT, ReplicateNames TEXT, ControlBatch_ID INT, ControlBatchName TEXT)";

        private static string CreateGCMasterFeatureGroupTableText =
            "CREATE TABLE IF NOT EXISTS GCMaster_FeatureGroup_Table (GroupID INT, IsValid BOOLEAN, NumPeaks INT, ApexRT DOUBLE, Name TEXT, NIST_ID TEXT," +
            " ChEBI_ID TEXT, IncludedFeatures TEXT, IsInternalStandard BOOLEAN, QuantIons TEXT, PeakList TEXT)";

        //replicate quant table
        private static string CreateReplicateQuantTableText = "CREATE TABLE IF NOT EXISTS ReplicateQuant_Table (Replicate_ID INT, ReplicateName TEXT, Batch_ID INT, Control_ID INT, GCMasterGroup_ID INT, QuantFeature_ID INT, ApexRT DOUBLE," +
            " QuantFeatureMZ DOUBLE, RTOffset DOUBLE, ApexIntensity DOUBLE, ApexIntensity_Normalized DOUBLE)";

        private static string CreateBatchQuantTableText = "CREATE TABLE IF NOT EXISTS BatchQuant_Table (Batch_ID, BatchName TEXT, Replicate_IDs TEXT, Control_ID INT, GCMasterGroup_ID INT,"
            + " GCMasterGroup_MZ DOUBLE, GCMasterGroup_ApexRT DOUBLE, AllIntensities TEXT, AvgIntensity DOUBLE, AvgIntensity_StdDev DOUBLE, AllIntensities_Normalized TEXT, AvgIntensity_Normalized DOUBLE,"
            + " AvgIntensity_Normalized_StdDev DOUBLE, AvgIntensity_LessControl DOUBLE, AvgIntensity_LessControl_PValue DOUBLE, AvgIntensity_LessControl_Normalized DOUBLE, AvgIntensity_LessControl_Normalized_PValue DOUBLE)";


        private static string CreateInternalStandardTable = "CREATE TABLE IF NOT EXISTS InternalStandard_Table (Replicate_ID INT, ReplicateName TEXT, GCMasterGroup_ID INT, QuantFeature_ID INT, ApexIntensity DOUBLE,"
            + " ApexIntensity_Normalized DOUBLE, NormalizationFactor DOUBLE)";

        private static string CreateBarChartCompTable = "CREATE TABLE IF NOT EXISTS BarChartComp_Table (GCMasterGroup_ID INT, GCMasterGroup_ApexRT DOUBLE, GCMasterGroup_MZ DOUBLE, Name TEXT, ChEBI_ID INT, PreferredName TEXT,"
            + "Batch_ID, Control_ID, Log2Intensity DOUBLE, StdDev DOUBLE, Color TEXT, Height DOUBLE)";

        private static string CreateBatchBatchCorrelationTable =
            "CREATE TABLE IF NOT EXISTS BatchVsBatch_Correlation_Table (BatchA_ID INT, BatchB_ID INT, BatchA_Name TEXT, BatchB_Name TEXT, RSquared DOUBLE)";

        private static string InsertIntoBatchBatchCorrelationTable = "INSERT INTO BatchVsBatch_Correlation_Table (BatchA_ID, BatchB_ID, BatchA_Name, BatchB_Name, RSquared)"
                                                                     +
                                                                     " VALUES(@BatchA_ID, @BatchB_ID, @BatchA_Name, @BatchB_Name, @RSquared)";

        private static string InsertIntoBarChartCompTable = "INSERT INTO BarChartComp_Table (GCMasterGroup_ID, GCMasterGroup_ApexRT, GCMasterGroup_MZ, Name, ChEBI_ID, PreferredName, Batch_ID, Control_ID, Log2Intensity, StdDev, Color, Height)"
            + " VALUES (@GCMasterGroup_ID, @GCMasterGroup_ApexRT, @GCMasterGroup_MZ, @Name, @ChEBI_ID, @PreferredName, @Batch_ID, @Control_ID, @Log2Intensity, @StdDev, @Color, @Height)";

        private static string AddBatchToDatabaseCommandText = "INSERT INTO Batch_Table (Name, Batch_ID, ReplicateIDs, ReplicateNames, ControlBatch_ID, ControlBatchName)" +
                                                            " VALUES (@Name, @Batch_ID, @ReplicateIDs, @ReplicateNames, @ControlBatch_ID, @ControlBatchName)";

        private static string AddReplicateToDatabaseCommandText = "INSERT INTO Replicate_Table (Name, Replicate_ID, GCFeatPath, BatchName, Batch_ID, ControlBatchName, ControlBatch_ID, IsQuantified)"
                                                                  + " VALUES (@Name, @Replicate_ID, @GCFeatPath, @BatchName, @Batch_ID, @ControlBatchName, @ControlBatch_ID, @IsQuantified)";

        private static string UpdateBatchTableReplicateInfoCommandText = "UPDATE Batch_Table SET ReplicateIDs=@ReplicateIDs, ReplicateNames=@ReplicateNames WHERE Batch_ID=@BatchID";

        private static string UpdateBatchTableControlInfoCommandText = "UPDATE Batch_Table SET ControlBatch_ID=@ControlBatch_ID, ControlBatchName=@ControlBatchName WHERE Batch_ID=@Batch_ID";

        private static string CreateBarChartBatchIDIndex = "CREATE INDEX IF NOT EXISTS BarChartComp_Table_BatchID_Index ON BarChartComp_Table (Batch_ID)";

        public static string CreateBatchVsBatchCorrelationBatchAIndex =
            "CREATE INDEX IF NOT EXISTS BatchVsBatch_Correlation_Table_BatchAIndex ON BatchVsBatch_Correlation_Table (BatchA_ID)";

        public static string CreateBatchVsBatchCorrelationBatchBIndex = "CREATE INDEX IF NOT EXISTS BatchVsBatch_Correlation_Table_BatchBIndex ON BatchVsBatch_Correlation_Table (BatchB_ID)";

        public static void CreateQuantDatabase(string outputPath, out SQLiteConnection conn)
        {
            var path = outputPath + "\\" + "GC-Quant_ResultDatabase_" + GetTimestamp(DateTime.Now) + ".gcresults";

            //path = @"C:\Users\Nick\Desktop\Test\Quant Test\GC-Quant_ResultDatabase_201505082112.gcresults";

            conn = new SQLiteConnection(@"Data Source=" + path);
            conn.Open();

            var command = new SQLiteCommand(CreateReplicateTableText, conn);
            command.ExecuteNonQuery();

            command = new SQLiteCommand(CreateBatchTableText, conn);
            command.ExecuteNonQuery();

            command = new SQLiteCommand(CreateGCMasterFeatureGroupTableText, conn);
            command.ExecuteNonQuery();

            command = new SQLiteCommand(CreateBatchQuantTableText, conn);
            command.ExecuteNonQuery();

            command = new SQLiteCommand(CreateReplicateQuantTableText, conn);
            command.ExecuteNonQuery();

            command = new SQLiteCommand(CreateInternalStandardTable, conn);
            command.ExecuteNonQuery();

            command = new SQLiteCommand(CreateBarChartCompTable, conn);
            command.ExecuteNonQuery();

            command = new SQLiteCommand(CreateBarChartBatchIDIndex, conn);
            command.ExecuteNonQuery();

            command = new SQLiteCommand(CreateBatchBatchCorrelationTable, conn);
            command.ExecuteNonQuery();

            command = new SQLiteCommand(CreateBatchVsBatchCorrelationBatchAIndex, conn);
            command.ExecuteNonQuery();

            command = new SQLiteCommand(CreateBatchVsBatchCorrelationBatchBIndex, conn);
            command.ExecuteNonQuery();
        }

        public static void AddBatchStructure(Dictionary<string, Batch> batchDictionary, SQLiteConnection conn)
        {
            using (var transaction = conn.BeginTransaction())
            {
                int batchCount = 0;
                int replicateCount = 0;
                foreach (var batch in batchDictionary.Values)
                {
                    batchCount++;
                    var command = new SQLiteCommand(AddBatchToDatabaseCommandText, conn);
                    command.Parameters.AddWithValue("@Name", batch.name);
                    command.Parameters.AddWithValue("@Batch_ID", batchCount);
                    command.Parameters.AddWithValue("@ReplicateIDs", null);
                    command.Parameters.AddWithValue("@ReplicateNames", null);
                    command.Parameters.AddWithValue("@ControlBatch_ID", null);
                    command.Parameters.AddWithValue("@ControlBatchName", null);
                    batch.batchID = batchCount;
                    command.ExecuteNonQuery();
                }
                foreach (var batch in batchDictionary.Values)
                {
                    SQLiteCommand command;
                    foreach (var rep in batch.replicates)
                    {
                        replicateCount++;
                        command = new SQLiteCommand(AddReplicateToDatabaseCommandText, conn);
                        command.Parameters.AddWithValue("@Name", rep.name);
                        command.Parameters.AddWithValue("@Replicate_ID", replicateCount);
                        command.Parameters.AddWithValue("@GCFeatPath", rep.gcFeatFilePath);
                        command.Parameters.AddWithValue("@BatchName", batch.name);
                        command.Parameters.AddWithValue("@Batch_ID", batch.batchID);
                        command.Parameters.AddWithValue("@IsQuantified", false);
                        if (rep.control != null)
                        {
                            command.Parameters.AddWithValue("@ControlBatchName", rep.control.name);
                            command.Parameters.AddWithValue("@ControlBatch_ID", rep.control.batchID);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@ControlBatchName", null);
                            command.Parameters.AddWithValue("@ControlBatch_ID", null);
                        }
                        rep.replicateID = replicateCount;
                        rep.batchID = batch.batchID;
                        command.ExecuteNonQuery();
                    }
                    foreach (var rep in batch.replicates)
                    {
                        if (rep.control != null)
                        {
                            var updateCommand = new SQLiteCommand(UpdateBatchTableControlInfoCommandText, conn);
                            updateCommand.Parameters.AddWithValue("@ControlBatch_ID", rep.control.batchID);
                            updateCommand.Parameters.AddWithValue("@ControlBatchName", rep.control.name);
                            updateCommand.Parameters.AddWithValue("@Batch_ID", batch.batchID);
                            updateCommand.ExecuteNonQuery();
                            updateCommand.Dispose();
                        }
                        break; //go through once
                    }
                    var replicateIDLine = "";
                    var replicateNameLine = "";
                    foreach (var rep in batch.replicates)
                    {
                        replicateIDLine += rep.replicateID + ";";
                        replicateNameLine += rep.name + ";";
                    }
                    command = new SQLiteCommand(UpdateBatchTableReplicateInfoCommandText, conn);
                    command.Parameters.AddWithValue("@ReplicateIDs", replicateIDLine);
                    command.Parameters.AddWithValue("@ReplicateNames", replicateNameLine);
                    command.Parameters.AddWithValue("@BatchID", batch.batchID);
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
        }

        public static void AddMasterInformationToDatabase(SQLiteConnection conn, string masterPath)
        {
            var masterConn = new SQLiteConnection(@"Data Source=" + masterPath);
            masterConn.Open();
            DataTable DbTable = new DataTable("featureGroupTable");
            string selectQuery = "SELECT * FROM featureGroupTable";
            var command = new SQLiteCommand(selectQuery, masterConn);
            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
            {
                adapter.Fill(DbTable);
            }
            masterConn.Close();
            masterConn.Dispose();
            using (var transaction = conn.BeginTransaction())
            {
                foreach (DataRow row in DbTable.Rows)
                {
                    var insertText = "INSERT INTO GCMaster_FeatureGroup_Table (GroupID, IsValid, NumPeaks, ApexRT, Name, NIST_ID, ChEBI_ID, IncludedFeatures, IsInternalStandard, QuantIons, PeakList)"
                                     + " VALUES (@GroupID, @IsValid, @NumPeaks, @ApexRT, @Name, @NIST_ID, @ChEBI_ID, @IncludedFeatures, @IsInternalStandard, @QuantIons, @PeakList)";
                    var insertCommand = new SQLiteCommand(insertText, conn);
                    insertCommand.Parameters.AddWithValue("@GroupID", row["GroupID"]);
                    insertCommand.Parameters.AddWithValue("@IsValid", row["IsValid"]);
                    insertCommand.Parameters.AddWithValue("@NumPeaks", row["NumPeaks"]);
                    insertCommand.Parameters.AddWithValue("@ApexRT", row["ApexRT"]);
                    insertCommand.Parameters.AddWithValue("@Name", row["Name"]);
                    insertCommand.Parameters.AddWithValue("@NIST_ID", row["NIST_ID"]);
                    insertCommand.Parameters.AddWithValue("@ChEBI_ID", row["ChEBI_ID"]);
                    insertCommand.Parameters.AddWithValue("@IncludedFeatures", row["IncludedFeatures"]);
                    insertCommand.Parameters.AddWithValue("@IsInternalStandard", row["IsInternalStandard"]);
                    insertCommand.Parameters.AddWithValue("@QuantIons", row["QuantIons"]);
                    insertCommand.Parameters.AddWithValue("@PeakList", row["PeakList"]);
                    insertCommand.ExecuteNonQuery();
                }
                transaction.Commit();
            }
        }

        private static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmm");
        }

        public static List<RTPeak> GetTicChromaFromGCMaster(SQLiteConnection conn)
        {
            var queryText = "SELECT s.TICChroma FROM rawFileTable s";
            var command = new SQLiteCommand(queryText, conn);
            var reader = command.ExecuteReader();
            var ticString = "";
            while (reader.Read())
            {
                ticString = reader["TICChroma"].ToString();
            }
            reader.Close();
            reader.Dispose();
            command.Dispose();
            return ConvertFeatureStringToPeakList(ticString);
        }

        public static List<RTPeak> GetTicChromaFromGCFeat(SQLiteConnection conn)
        {
            var queryText = "SELECT s.TICChroma FROM ticTable s";
            var command = new SQLiteCommand(queryText, conn);
            var reader = command.ExecuteReader();
            var ticString = "";
            while (reader.Read())
            {
                ticString = reader["TICChroma"].ToString();
            }
            reader.Close();
            reader.Dispose();
            command.Dispose();
            return ConvertFeatureStringToPeakList(ticString);
        }

        public static List<RTPeak> ConvertFeatureStringToPeakList(string featureString)
        {
            List<RTPeak> returnList = new List<RTPeak>();
            string[] parts = featureString.Split(';');
            foreach (var part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    string[] subparts = part.Split(',');
                    double rt = double.Parse(subparts[0]);
                    double intensity = double.Parse(subparts[1]);
                    var newRTPeak = new RTPeak();
                    newRTPeak.RT = rt;
                    newRTPeak.Intensity = intensity;
                    returnList.Add(newRTPeak);
                }
            }
            return returnList;
        }

        public static List<Feature> GetFeatures(double minTime, double maxTime, SQLiteConnection conn)
        {
            var queryText =
                "SELECT s.mz, s.ApexRT, s.ApexIntensity, s.ID_Number, s.SmoothFeatureString, s.RawFeatureString FROM featureTable s WHERE s.ApexRT>@minTime AND s.ApexRT<@maxTime";
            var command = new SQLiteCommand(queryText, conn);
            command.Parameters.AddWithValue("@minTime", minTime);
            command.Parameters.AddWithValue("@maxtime", maxTime);
            List<Feature> returnFeatures = new List<Feature>();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var id = int.Parse(reader["ID_Number"].ToString());
                double mz = double.Parse(reader["mz"].ToString());
                var apexRT = double.Parse(reader["ApexRT"].ToString());
                var apexIntensity = double.Parse(reader["ApexIntensity"].ToString());
                var featString = reader["SmoothFeatureString"].ToString();
                string[] parts = featString.Split('|');
                string[] peaks = parts[0].Split(';');
                var feature = new Feature();
                feature.ApexTime = apexRT;
                foreach (var peak in peaks)
                {
                    if (!string.IsNullOrEmpty(peak))
                    {
                        string[] peakParts = peak.Split(',');
                        double time = double.Parse(peakParts[0]);
                        double intensity = double.Parse(peakParts[1]);
                        var newPeak = new RTPeak(mz, intensity, time);
                        feature.AddSmoothPeak(newPeak);
                        if (Math.Round(newPeak.RT, 3) == Math.Round(apexRT, 3))
                        {
                            feature.totalIntensity += newPeak.Intensity;
                        }
                    }
                }
                feature.AverageMZ = mz;
                feature.maxIntensity = apexIntensity;
                feature.ID_Number = id;
                returnFeatures.Add(feature);
            }
            reader.Close();
            reader.Dispose();
            command.Dispose();
            return returnFeatures;
        }

        public static void SetReplicateToQuantified(int replicateID, SQLiteConnection conn)
        {
            var commandText = "UPDATE Replicate_Table SET IsQuantified=1 WHERE Replicate_ID=@ID";
            var command = new SQLiteCommand(commandText, conn);
            command.Parameters.AddWithValue("@ID", replicateID);
            command.ExecuteNonQuery();
            command.Dispose();
        }

        public static bool IsQuantified(int replicateID, SQLiteConnection conn)
        {
            var queryText = "SELECT s.IsQuantified FROM Replicate_Table s WHERE s.Replicate_ID=@ID";
            var query = new SQLiteCommand(queryText, conn);
            query.Parameters.AddWithValue("@ID", replicateID);
            var reader = query.ExecuteReader();
            bool isQuantified = false;
            while (reader.Read())
            {
                isQuantified = bool.Parse(reader["IsQuantified"].ToString());
            }
            reader.Close();
            reader.Dispose();
            query.Dispose();
            return isQuantified;
        }

        public static void AddReplicateQuantPointToDatabase(int replicateID, string replicateName, int batchID, int controlID, int gcMastGroupID, double rtOffset, Feature quantFeature, SQLiteConnection conn)
        {
            var insertText = "INSERT INTO ReplicateQuant_Table (Replicate_ID, ReplicateName, Batch_ID, Control_ID, GCMasterGroup_ID, QuantFeature_ID, ApexRT, QuantFeatureMZ, RTOffset, ApexIntensity, ApexIntensity_Normalized)"
                + " VALUES(@Replicate_ID, @ReplicateName, @Batch_ID, @Control_ID, @GCMasterGroup_ID, @QuantFeature_ID, @ApexRT, @QuantFeatureMZ, @RTOffset, @ApexIntensity, @ApexIntensity_Normalized)";
            var insertCommand = new SQLiteCommand(insertText, conn);
            insertCommand.Parameters.AddWithValue("@Replicate_ID", replicateID);
            insertCommand.Parameters.AddWithValue("@ReplicateName", replicateName);
            insertCommand.Parameters.AddWithValue("@Batch_ID", batchID);
            if (controlID == 0)
            {
                insertCommand.Parameters.AddWithValue("@Control_ID", null);
            }
            else
            {
                insertCommand.Parameters.AddWithValue("@Control_ID", controlID);
            }
            insertCommand.Parameters.AddWithValue("@GCMasterGroup_ID", gcMastGroupID);
            insertCommand.Parameters.AddWithValue("@QuantFeature_ID", quantFeature.ID_Number);
            insertCommand.Parameters.AddWithValue("@ApexRT", quantFeature.apexTime);
            insertCommand.Parameters.AddWithValue("@QuantFeatureMZ", quantFeature.averageMZ);
            insertCommand.Parameters.AddWithValue("@RTOffset", rtOffset);
            insertCommand.Parameters.AddWithValue("@ApexIntensity", Math.Log(quantFeature.maxIntensity, 2));
            insertCommand.Parameters.AddWithValue("@ApexIntensity_Normalized", null);
            insertCommand.ExecuteNonQuery();
            insertCommand.Dispose();
        }

        public static void AddNullReplicateQuantPointToDatabase(int replicateID, string replicateName, int batchID, int controlID, int gcMastGroupID, double rtOffset, SQLiteConnection conn)
        {
            var insertText = "INSERT INTO ReplicateQuant_Table (Replicate_ID, ReplicateName, Batch_ID, Control_ID, GCMasterGroup_ID, QuantFeature_ID, ApexRT, QuantFeatureMZ, RTOffset, ApexIntensity, ApexIntensity_Normalized)"
                + " VALUES(@Replicate_ID, @ReplicateName, @Batch_ID, @Control_ID, @GCMasterGroup_ID, @QuantFeature_ID, @ApexRT, @QuantFeatureMZ, @RTOffset, @ApexIntensity, @ApexIntensity_Normalized)";
            var insertCommand = new SQLiteCommand(insertText, conn);
            insertCommand.Parameters.AddWithValue("@Replicate_ID", replicateID);
            insertCommand.Parameters.AddWithValue("@ReplicateName", replicateName);
            insertCommand.Parameters.AddWithValue("@Batch_ID", batchID);
            if (controlID == 0)
            {
                insertCommand.Parameters.AddWithValue("@Control_ID", null);
            }
            else
            {
                insertCommand.Parameters.AddWithValue("@Control_ID", controlID);
            }
            insertCommand.Parameters.AddWithValue("@GCMasterGroup_ID", gcMastGroupID);
            insertCommand.Parameters.AddWithValue("@QuantFeature_ID", null);
            insertCommand.Parameters.AddWithValue("@ApexRT", null);
            insertCommand.Parameters.AddWithValue("@QuantFeatureMZ", null);
            insertCommand.Parameters.AddWithValue("@RTOffset", rtOffset);
            insertCommand.Parameters.AddWithValue("@ApexIntensity", null);
            insertCommand.Parameters.AddWithValue("@ApexIntensity_Normalized", null);
            insertCommand.ExecuteNonQuery();
            insertCommand.Dispose();
        }

        public static void AddBatchQuantDataToDatabase(Dictionary<int, Batch> batchDict, SQLiteConnection conn)
        {
            var insertText = "INSERT INTO BatchQuant_Table (Batch_ID, BatchName, Replicate_IDs, Control_ID, GCMasterGroup_ID, GCMasterGroup_MZ, GCMasterGroup_ApexRT, AllIntensities,"
                + "AvgIntensity, AvgIntensity_StdDev, AllIntensities_Normalized, AvgIntensity_Normalized, AvgIntensity_Normalized_StdDev, AvgIntensity_LessControl,"
            + "AvgIntensity_LessControl_PValue, AvgIntensity_LessControl_Normalized, AvgIntensity_LessControl_Normalized_PValue) VALUES (@Batch_ID, @BatchName, @Replicate_IDs, @Control_ID, @GCMasterGroup_ID,"
            + "@GCMasterGroup_MZ, @GCMasterGroup_ApexRT, @AllIntensities, @AvgIntensity, @AvgIntensity_StdDev, @AllIntensities_Normalized, @AvgIntensity_Normalized, @AvgIntensity_Normalized_StdDev, @AvgIntensity_LessControl,"
            + "@AvgIntensity_LessControl_PValue, @AvgIntensity_LessControl_Normalized, @AvgIntensity_LessControl_Normalized_PValue)";

            using (var transaction = conn.BeginTransaction())
            {
                foreach (var batch in batchDict.Values)
                {
                    foreach (var qPt in batch.avgQuantDict.Values)
                    {
                        var command = new SQLiteCommand(insertText, conn);
                        command.Parameters.AddWithValue("@Batch_ID", batch.batchID);
                        command.Parameters.AddWithValue("@BatchName", batch.name);
                        var repString = "";
                        foreach (var rep in batch.replicates)
                        {
                            repString += rep.replicateID + ";";
                        }
                        command.Parameters.AddWithValue("@Replicate_IDs", repString);
                        //        @Replicate_IDs, 
                        command.Parameters.AddWithValue("@Control_ID", batch.controlID);
                        //        @Control_ID,
                        command.Parameters.AddWithValue("@GCMasterGroup_ID", qPt.GCMasterGroupID);
                        //        @GCMasterGroup_ID,"
                        command.Parameters.AddWithValue("@GCMasterGroup_MZ", qPt.GCMasterGroup_MZ);
                        //+ "GCMasterGroup_MZ, 
                        command.Parameters.AddWithValue("@GCMasterGroup_ApexRT", 0);
                        //        GC_MasterGroup_ApexRT,
                        var allIntensityLine = "";
                        foreach (var intensity in qPt.AllIntensities)
                        {
                            allIntensityLine += intensity + ";";
                        }
                        command.Parameters.AddWithValue("@AllIntensities", allIntensityLine);
                        //        AllIntensites, 
                        command.Parameters.AddWithValue("@AvgIntensity", qPt.AvgIntensity);
                        //@AvgIntensity, 
                        command.Parameters.AddWithValue("@AvgIntensity_StdDev", qPt.AvgIntensity_StdDev);
                        //        @AvgIntensity_StdDev, 
                        var normalizedIntensityLine = "";
                        foreach (var intensity in qPt.Allintensities_Normalized)
                        {
                            normalizedIntensityLine += intensity + ";";
                        }
                        command.Parameters.AddWithValue("@AllIntensities_Normalized", normalizedIntensityLine);
                        //        @AllIntensities_Normalized,
                        command.Parameters.AddWithValue("@AvgIntensity_Normalized", qPt.AvgIntensity_Normalized);
                        //        @AvgIntensity_Normalized, 
                        command.Parameters.AddWithValue("@AvgIntensity_Normalized_StdDev", qPt.AvgIntensity_Normalized_StdDev);
                        //        @AvgIntensity_Normalized_StdDev,
                        command.Parameters.AddWithValue("@AvgIntensity_LessControl", qPt.AvgIntensity_LessControl);
                        //        @AvgIntensity_LessControl,"
                        command.Parameters.AddWithValue("@AvgIntensity_LessControl_PValue", qPt.AvgIntensity_LessControl_PValue);
                        //+ "@AvgIntensity_LessControl_PValue, 
                        command.Parameters.AddWithValue("@AvgIntensity_LessControl_Normalized", qPt.AvgIntensity_Normalized_LessControl);
                        //        @AvgIntensity_LessControl_Normalized, 
                        command.Parameters.AddWithValue("@AvgIntensity_LessControl_Normalized_PValue", qPt.AvgIntensity_Normalized_LessControl_PValue);
                        //        @AvgIntensity_LessControl_Normalized_PValue
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                }
                transaction.Commit();
                transaction.Dispose();
            }
        }

        public static void AddInternalStandardsToDatabase(List<InternalStandard> standards, SQLiteConnection conn)
        {
            using (var transaction = conn.BeginTransaction())
            {
                //InternalStandard_Table (Replicate_ID INT, ReplicateName TEXT, GCMasterGroup_ID INT, QuantFeature_ID INT, ApexIntensity DOUBLE,"
                //+ " ApexIntensity_Normalized DOUBLE, NormalizationFactor DOUBLE)
                var commandText = "INSERT INTO InternalStandard_Table (Replicate_ID, ReplicateName, GCMasterGroup_ID, QuantFeature_ID, ApexIntensity, NormalizationFactor)"
                    + " VALUES (@Replicate_ID, @ReplicateName, @GCMasterGroup_ID, @QuantFeature_ID, @ApexIntensity, @NormalizationFactor)";
                foreach (var standard in standards)
                {
                    var command = new SQLiteCommand(commandText, conn);
                    command.Parameters.AddWithValue("@Replicate_ID", standard.ReplicateID);
                    command.Parameters.AddWithValue("@ReplicateName", standard.ReplicateName);
                    command.Parameters.AddWithValue("@GCMasterGroup_ID", standard.GCMasterGroup_ID);
                    command.Parameters.AddWithValue("@ApexIntensity", standard.ApexIntensity);
                    command.Parameters.AddWithValue("@NormalizationFactor", standard.CorrectionFactor);
                    command.Parameters.AddWithValue(@"QuantFeature_ID", standard.Feature_ID);
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
                transaction.Dispose();
            }
        }

        public static void AddBarChartInfoToDatabase(Dictionary<int, Batch> batchDict, SQLiteConnection conn)
        {
            //(@GCMasterGroup_ID, @GCMasterGroup_ApexRT, @GCMasterGroup_MZ, @Name, @ChEBI_ID, @PreferredName, @Batch_ID, @Control_ID, @Log2Intensity, @StdDev, @Color, @Height)
            using (var transaction = conn.BeginTransaction())
            {
                Dictionary<int, AvgQuantPoint> quantPtDict = new Dictionary<int, AvgQuantPoint>();
                foreach (var batch in batchDict.Values)
                {
                    foreach (var quantPt in batch.avgQuantDict)
                    {
                        if (!quantPtDict.ContainsKey(quantPt.Key))
                        {
                            quantPtDict.Add(quantPt.Key, quantPt.Value);
                        }
                    }
                }
                foreach (var batch in batchDict.Values)
                {
                    if (batch.controlID != 0)
                    {
                        foreach (var val in batch.avgQuantDict)
                        {
                            var command = new SQLiteCommand(InsertIntoBarChartCompTable, conn);
                            command.Parameters.AddWithValue("@GCMasterGroup_ID", val.Value.GCMasterGroupID);
                            command.Parameters.AddWithValue("@GCMasterGroup_ApexRT", val.Value.GCMasterGroup_ApexRT);
                            command.Parameters.AddWithValue("@Name", val.Value.Name);
                            command.Parameters.AddWithValue("@GCMasterGroup_MZ", val.Value.GCMasterGroup_MZ);
                            command.Parameters.AddWithValue("@ChEBI_ID", val.Value.ChEBI_ID);
                            command.Parameters.AddWithValue("@PreferredName", val.Value.PreferredName);
                            command.Parameters.AddWithValue("@Batch_ID", val.Value.BatchID);
                            command.Parameters.AddWithValue("@Control_ID", val.Value.ControlID);
                            if (val.Value.AvgIntensity_Normalized != 0)
                            {
                                command.Parameters.AddWithValue("@Log2Intensity", val.Value.AvgIntensity_Normalized);
                                command.Parameters.AddWithValue("@StdDev", val.Value.AvgIntensity_Normalized_StdDev);
                                command.Parameters.AddWithValue("@Height", val.Value.AvgIntensity_Normalized_LessControl);
                                if (val.Value.AvgIntensity_Normalized_LessControl_PValue < 0.05)
                                {
                                    if (Math.Abs(val.Value.AvgIntensity_Normalized_LessControl) > 1)
                                    {
                                        command.Parameters.AddWithValue("@Color", "GREEN");
                                    }
                                    else
                                    {
                                        command.Parameters.AddWithValue("@Color", "BLUE");
                                    }
                                }
                                else//red
                                {
                                    command.Parameters.AddWithValue("@Color", "RED");
                                }
                            }
                            else
                            {
                                command.Parameters.AddWithValue("@Log2Intensity", 0);
                                command.Parameters.AddWithValue("@StdDev", 0);
                                command.Parameters.AddWithValue("@Height", 0);
                                command.Parameters.AddWithValue("@Color", "RED");
                            }
                            command.ExecuteNonQuery();
                            command.Dispose();
                        }
                    }
                }
                transaction.Commit();
                transaction.Dispose();
            }
        }

        public static void AddBatchBatchCorrelationsToDatabase(Dictionary<int, Batch> batchDict, SQLiteConnection conn)
        {
            List<Batch> relevantBatches = batchDict.Values.Where(x => x.controlID != 0).ToList();
            using (var transaction = conn.BeginTransaction())
            {
                for (int i = 0; i < relevantBatches.Count; i++)
                {
                    for (int j = i + 1; j < relevantBatches.Count; j++)
                    {
                        var batchA = relevantBatches[i];
                        var batchB = relevantBatches[j];
                        List<double> xVals = new List<double>();
                        List<double> yVals = new List<double>();
                        List<int> sharedKeys =
                            batchA.avgQuantDict.Keys.Where(x => batchB.avgQuantDict.ContainsKey(x)).ToList();
                        foreach (var key in sharedKeys)
                        {
                            if (batchA.avgQuantDict[key].AllIntensities.Count == 3 && //YOU MAY WANT TO CHANGE THIS SO THAT ITS THREE BATCH AND THREE CONTROL MEASUREMENTS
                                batchB.avgQuantDict[key].AllIntensities.Count == 3)
                            {
                                xVals.Add(batchA.avgQuantDict[key].AvgIntensity_Normalized_LessControl);
                                yVals.Add(batchB.avgQuantDict[key].AvgIntensity_Normalized_LessControl);
                            }
                        }
                        double slope;
                        double rSquared;
                        double yIntercept;
                        Statistics.LinearRegression(xVals.ToArray(), yVals.ToArray(), 0, xVals.Count, out rSquared, out yIntercept, out slope);
                        var command = new SQLiteCommand(InsertIntoBatchBatchCorrelationTable, conn);
                        command.Parameters.AddWithValue("@BatchA_ID", batchA.batchID);
                        command.Parameters.AddWithValue("@BatchB_ID", batchB.batchID);
                        command.Parameters.AddWithValue("@BatchA_Name", batchA.name);
                        command.Parameters.AddWithValue("@BatchB_Name", batchB.name);
                        command.Parameters.AddWithValue("@RSquared", rSquared);
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                }
                transaction.Commit();
                transaction.Dispose();
            }
        }
    }
}
