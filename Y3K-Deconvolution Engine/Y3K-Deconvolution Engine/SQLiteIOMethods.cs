using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Y3K_Deconvolution_Engine
{
    public class SQLiteIOMethods
    {
        public event EventHandler<ProgressStatusEventArgs> Progress;

        public event EventHandler Finished;

        private void OnProgressUpdate(double percent)
        {
            var handler = Progress;
            if (handler != null)
            {
                handler(this, new ProgressStatusEventArgs(percent));
            }
        }

        private void OnFinish()
        {
            var handler = Finished;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public static void CreateTablesInDatabase(SQLiteConnection conn)
        {
            var createTableCommand = "CREATE TABLE IF NOT EXISTS featureTable (ID INT, Name TEXT, mz DOUBLE, ApexRT DOUBLE, ApexIntensity DOUBLE, ID_Number INT, SmoothFeatureString TEXT, RawFeatureString TEXT)";
            var createCommand = new SQLiteCommand(createTableCommand, conn);
            createCommand.ExecuteNonQuery();

            createTableCommand = "CREATE TABLE IF NOT EXISTS rawFileNameTable (ID INT, Name TEXT, Extracted BOOLEAN, Grouped BOOLEAN)";
            createCommand = new SQLiteCommand(createTableCommand, conn);
            createCommand.ExecuteNonQuery();

            createTableCommand = "CREATE TABLE IF NOT EXISTS ticTable (ID INT, Name TEXT, TICChroma TEXT)";
            createCommand = new SQLiteCommand(createTableCommand, conn);
            createCommand.ExecuteNonQuery();

            createTableCommand = "CREATE TABLE IF NOT EXISTS rawFileQuantTable (ID INT, Name TEXT, FeatureRT DOUBLE, QuantIonMZ DOUBLE, ApexIntensity DOUBLE, OffsetRT DOUBLE, FeatureString TEXT)";
            createCommand = new SQLiteCommand(createTableCommand, conn);
            createCommand.ExecuteNonQuery();

            createTableCommand =
                "CREATE TABLE IF NOT EXISTS featureGroupTable (GroupID INT, NumPeaks INT, ApexRT DOUBLE, IncludedFeatures TEXT, PeakList TEXT)";
            createCommand = new SQLiteCommand(createTableCommand, conn);
            createCommand.ExecuteNonQuery();

            var createIndexCommand = "CREATE INDEX IF NOT EXISTS featureIDIndex ON featureTable (ID_Number)";
            createCommand = new SQLiteCommand(createIndexCommand, conn);
            createCommand.ExecuteNonQuery();

            createCommand.Dispose();
        }

        public static void AddRawFileEntry(ThermoRawFile rawFile, SQLiteConnection conn)
        {
            List<int> ids = new List<int>();
            var queryCommandText = "SELECT s.ID FROM rawFileNameTable s";
            var queryCommand = new SQLiteCommand(queryCommandText, conn);
            var reader = queryCommand.ExecuteReader();
            while (reader.Read())
            {
                ids.Add(int.Parse(reader["ID"].ToString()));
            }
            ids = ids.OrderBy(x => x).ToList();
            if (ids.Count == 0)
            {
                int id = 1;
                queryCommandText = "INSERT INTO rawFileNameTable (ID, Name, Extracted, Grouped) VALUES (@ID, @Name, @Extracted, @Grouped)";
                queryCommand = new SQLiteCommand(queryCommandText, conn);
                queryCommand.Parameters.AddWithValue("@ID", id);
                queryCommand.Parameters.AddWithValue("@Name", rawFile.Name);
                queryCommand.Parameters.AddWithValue("@Extracted", false);
                queryCommand.Parameters.AddWithValue("@Grouped", false);
                queryCommand.ExecuteNonQuery();
                queryCommand.Dispose();
            }
            else
            {
                int nextID = ids.Last() + 1;
                if (!ContainsRawFile(conn, rawFile))
                {
                    var insertText = "INSERT INTO rawFileNameTable (ID, Name, Extracted) VALUES (@ID, @Name, @Extracted, @Grouped)";
                    var insertCommand = new SQLiteCommand(insertText, conn);
                    insertCommand.Parameters.AddWithValue("@ID", nextID);
                    insertCommand.Parameters.AddWithValue("@Name", rawFile.Name);
                    insertCommand.Parameters.AddWithValue("@Extracted", false);
                    insertCommand.Parameters.AddWithValue("@Grouped", false);
                    insertCommand.ExecuteNonQuery();
                    nextID++;
                }
            }
        }

        public static void AddRawFileTICChroma(ThermoRawFile rawFile, SQLiteConnection conn)
        {
            if (!ContainsRawFileTIC(conn, rawFile))
            {
                var ticChroma = rawFile.GetTICChroma();
                var line = "";
                foreach (var peak in ticChroma)
                {
                    line += peak.Time + "," + peak.Intensity + ";";
                }
                int id = GetRawFileID(rawFile, conn);
                var insertText = "INSERT INTO ticTable (ID, Name, TICChroma) VALUES (@ID, @Name, @TICChroma)";
                var insertCommand = new SQLiteCommand(insertText, conn);
                insertCommand.Parameters.AddWithValue("@ID", id);
                insertCommand.Parameters.AddWithValue("@Name", rawFile.Name);
                insertCommand.Parameters.AddWithValue("@TICChroma", line);
                insertCommand.ExecuteNonQuery();
            }
        }

        public static bool ContainsRawFile(SQLiteConnection conn, ThermoRawFile raw)
        {
            var queryText = "SELECT s.ID FROM rawFileNameTable s WHERE s.Name=@Name";
            var queryCommand = new SQLiteCommand(queryText, conn);
            queryCommand.Parameters.AddWithValue("@Name", raw.Name);
            var reader = queryCommand.ExecuteReader();
            var hasRow = reader.HasRows;
            reader.Close();
            reader.Dispose();
            return hasRow;
        }

        public static bool ContainsRawFileTIC(SQLiteConnection conn, ThermoRawFile raw)
        {
            var queryText = "SELECT s.ID FROM ticTable s WHERE s.Name=@Name";
            var queryCommand = new SQLiteCommand(queryText, conn);
            queryCommand.Parameters.AddWithValue("@Name", raw.Name);
            var reader = queryCommand.ExecuteReader();
            var hasRow = reader.HasRows;
            reader.Close();
            reader.Dispose();
            return hasRow;
        }

        public static bool ContainsRawFileFeatures(SQLiteConnection conn, ThermoRawFile raw)
        {
            var queryText = "SELECT s.ID FROM featureTable s WHERE s.Name=@Name";
            var queryCommand = new SQLiteCommand(queryText, conn);
            queryCommand.Parameters.AddWithValue("@Name", raw.Name);
            var reader = queryCommand.ExecuteReader();
            var hasRow = reader.HasRows;
            reader.Close();
            reader.Dispose();
            return hasRow;
        }

        public static int GetRawFileID(ThermoRawFile raw, SQLiteConnection conn)
        {
            var queryText = "SELECT s.ID FROM rawFileNameTable s WHERE s.Name=@Name";
            var queryCommand = new SQLiteCommand(queryText, conn);
            queryCommand.Parameters.AddWithValue("@Name", raw.Name);
            var reader = queryCommand.ExecuteReader();
            int id = 0;
            while (reader.Read())
            {
                id = int.Parse(reader["ID"].ToString());
            }
            return id;
        }

        public static int GetRawFileID(string name, SQLiteConnection conn)
        {
            var queryText = "SELECT s.ID FROM rawFileNameTable s WHERE s.Name=@Name";
            var queryCommand = new SQLiteCommand(queryText, conn);
            queryCommand.Parameters.AddWithValue("@Name", name);
            var reader = queryCommand.ExecuteReader();
            int id = 0;
            while (reader.Read())
            {
                id = int.Parse(reader["ID"].ToString());
            }
            return id;
        }

        public bool ContainsRawFileFeatures(ThermoRawFile rawFile, SQLiteConnection conn)
        {
            var queryText = "SELECT s.Name FROM featureTable s WHERE s.Name=@Name";
            var queryCommand = new SQLiteCommand(queryText, conn);
            queryCommand.Parameters.AddWithValue("@Name", rawFile.Name);
            var reader = queryCommand.ExecuteReader();
            return reader.HasRows;
        }

        public static bool ContainsQuantFeature(double FeatureRT, string rawName, SQLiteConnection conn)
        {
            int id = GetRawFileID(rawName, conn);
            var queryText = "SELECT s.ApexIntensity FROM rawFileQuantTable s WHERE s.FeatureRT=@FeatureRT AND s.ID=@ID";
            var queryCommand = new SQLiteCommand(queryText, conn);
            queryCommand.Parameters.AddWithValue("@ID", id);
            queryCommand.Parameters.AddWithValue("@FeatureRT", FeatureRT);
            var reader = queryCommand.ExecuteReader();
            return reader.HasRows;
        }

        public static bool ContainsSpecificFeature(int ID_Number, SQLiteConnection conn)
        {
            var queryText = "SELECT s.ApexIntensity FROM featureTable s WHERE s.ID_Number=@ID_Number";
            var queryCommand = new SQLiteCommand(queryText, conn);
            queryCommand.Parameters.AddWithValue("@ID_Number", ID_Number);
            var reader = queryCommand.ExecuteReader();
            return reader.HasRows;
        }

        public static bool IsExtractionDone(SQLiteConnection conn)
        {
            var queryText = "SELECT s.Extracted FROM rawFileNameTable s";
            var queryCommand = new SQLiteCommand(queryText, conn);
            var reader = queryCommand.ExecuteReader();
            while (reader.Read())
            {
                bool done = bool.Parse(reader["Extracted"].ToString());
                return done;
            }
            return false;
        }

        public static bool IsGroupingDone(SQLiteConnection conn)
        {
            var queryText = "SELECT s.Grouped FROM rawFileNameTable s";
            var queryCommand = new SQLiteCommand(queryText, conn);
            var reader = queryCommand.ExecuteReader();
            while (reader.Read())
            {
                bool done = bool.Parse(reader["Grouped"].ToString());
                return done;
            }
            return false;
        }

        public static List<Feature> GetAllFeaturesFromDatabase(SQLiteConnection conn)
        {
            List<Feature> returnFeatures = new List<Feature>();
            var queryText =
                "SELECT s.mz, s.ApexRT, s.ApexIntensity, s.ID_Number, s.SmoothFeatureString, s.RawFeatureString FROM featureTable s";
            var queryCommand = new SQLiteCommand(queryText, conn);
            var reader = queryCommand.ExecuteReader();
            while (reader.Read())
            {
                var apexRT = double.Parse(reader["ApexRT"].ToString());
                var mz = double.Parse(reader["mz"].ToString());
                var id = int.Parse(reader["ID_Number"].ToString());
                var smoothString = reader["SmoothFeatureString"].ToString();
                var rawString = reader["RawFeatureString"].ToString();
                List<RTPeak> smoothPeaks = ConvertFeatureStringToPeakList(smoothString);
                List<RTPeak> rawPeaks = ConvertFeatureStringToPeakList(rawString);
                var feat = new Feature();
                feat.ApexTime = apexRT;
                feat.AverageMZ = mz;
                feat.ID_Number = id;
                foreach (var peak in rawPeaks)
                {
                    feat.AddPeak(peak);
                }
                foreach (var peak in smoothPeaks)
                {
                    feat.AddSmoothPeak(peak);
                }
                returnFeatures.Add(feat);
                //feat.ApexTime = apexRT;
                feat.AverageMZ = mz;
            }
            return returnFeatures;
        }

        public void PushFeaturesToDatabase(ThermoRawFile rawFile, List<Feature> allFeatures, SQLiteConnection conn)
        {
            AddRawFileEntry(rawFile, conn);
            using (var transaction = conn.BeginTransaction())
            {
                if (!ContainsRawFileFeatures(rawFile, conn))
                {
                    int id = GetRawFileID(rawFile, conn);
                    int count = 0;
                    int onePercentCount = (int)(0.01 * allFeatures.Count);
                    onePercentCount++;
                    foreach (var feature in allFeatures)
                    {
                        if (!ContainsSpecificFeature(feature.ID_Number, conn))
                        {
                            double apexIntensity = 0;
                            double apexRT = 0;
                            var smoothLine = "";
                            var rawLine = "";
                            foreach (var peak in feature.smoothRTPeaks)
                            {
                                if (peak.Intensity > apexIntensity)
                                {
                                    apexIntensity = peak.Intensity;
                                    apexRT = peak.RT;
                                }
                                smoothLine += peak.RT + "," + peak.Intensity + ";";
                            }
                            foreach (var peak in feature.RawRTPeaks)
                            {
                                rawLine += peak.RT + "," + peak.Intensity + ";";
                            }
                            var insertText = "INSERT INTO featureTable (ID, Name, mz, ApexRT, ApexIntensity, ID_Number, SmoothFeatureString, RawFeatureString)"
                                             +
                                             " VALUES (@ID, @Name, @mz, @ApexRT, @ApexIntensity, @ID_Number, @SmoothFeatureString, @RawFeatureString)";
                            var insertCommand = new SQLiteCommand(insertText, conn);
                            insertCommand.Parameters.AddWithValue("@ID", id);
                            insertCommand.Parameters.AddWithValue("@Name", rawFile.Name);
                            insertCommand.Parameters.AddWithValue("@mz", feature.averageMZ);
                            insertCommand.Parameters.AddWithValue("@ApexRT", apexRT);
                            insertCommand.Parameters.AddWithValue("@ApexIntensity", apexIntensity);
                            insertCommand.Parameters.AddWithValue("@SmoothFeatureString", smoothLine);
                            insertCommand.Parameters.AddWithValue("@RawFeatureString", rawLine);
                            insertCommand.Parameters.AddWithValue("@ID_Number", feature.ID_Number);
                            insertCommand.ExecuteNonQuery();
                            count++;
                            if (count % onePercentCount == 0)
                            {
                                OnProgressUpdate(.01);
                            }
                        }
                    }
                }
                transaction.Commit();
                transaction.Dispose();
                OnFinish();
            }
        }

        public static void AddQuantPointToDatabase(EISpectrum masterSpecies, double quantIonMZ, double apexIntensity, double offset, Feature quantFeature, string rawName, SQLiteConnection conn)
        {
            if (!ContainsQuantFeature(masterSpecies.ApexTimeEI, rawName, conn))
            {
                int id = GetRawFileID(rawName, conn);
                //(ID INT, Name TEXT, FeatureRT DOUBLE, QuantIonMZ DOUBLE, ApexIntensity DOUBLE, OffsetRT DOUBLE, FeatureString TEXT)
                var insertText = "INSERT INTO rawFileQuantTable (ID, Name, FeatureRT, QuantIonMZ, ApexIntensity, OffsetRT, FeatureString)" +
                    " VALUES (@ID, @Name, @FeatureRT, @QuantIonMZ, @ApexIntensity, @OffsetRT, @FeatureString)";
                var insertCommand = new SQLiteCommand(insertText, conn);
                insertCommand.Parameters.AddWithValue("@ID", id);
                insertCommand.Parameters.AddWithValue("@Name", rawName);
                insertCommand.Parameters.AddWithValue("@QuantIonMZ", quantIonMZ);
                insertCommand.Parameters.AddWithValue("@ApexIntensity", apexIntensity);
                insertCommand.Parameters.AddWithValue("@OffsetRT", offset);
                insertCommand.Parameters.AddWithValue("@FeatureRT", masterSpecies.ApexTimeEI);
                var featString = "";
                if (quantFeature != null)
                {
                    foreach (var peak in quantFeature.smoothRTPeaks)
                    {
                        featString += peak.RT + "," + peak.Intensity + ";";
                    }
                }
                insertCommand.Parameters.AddWithValue("@FeatureString", featString);
                insertCommand.ExecuteNonQuery();
            }
        }

        public static void AddFeatureGroupToDatabase(FeatureGroup group, int ID_Num, SQLiteConnection conn)
        {
            //(GroupID INT, NumPeaks INT, ApexRT DOUBLE, IncludedFeatures TEXT)
            var insertText = "INSERT INTO featureGroupTable (GroupID, NumPeaks, ApexRT, IncludedFeatures, PeakList)"
                             + " VALUES (@GroupID, @NumPeaks, @ApexRT, @IncludedFeatures, @PeakList)";
            var insertCommand = new SQLiteCommand(insertText, conn);
            insertCommand.Parameters.AddWithValue("@GroupID", ID_Num);
            insertCommand.Parameters.AddWithValue("@NumPeaks", group.allFeatures.Count);
            insertCommand.Parameters.AddWithValue("@ApexRT", group.ApexTime);
            var includedFeatures = "";
            foreach (var feat in group.allFeatures)
            {
                includedFeatures += feat.ID_Number + ";";
            }
            insertCommand.Parameters.AddWithValue("@IncludedFeatures", includedFeatures);
            var peakList = "";
            foreach (var peak in group.finalPeaks)
            {
                peakList += peak.MZ + "," + peak.Intensity + ";";
            }
            insertCommand.Parameters.AddWithValue("@PeakList", peakList);
            insertCommand.ExecuteNonQuery();
        }

        public static void CreateIndices(SQLiteConnection conn)
        {
            using (var transaction = conn.BeginTransaction())
            {
                var indexText = "CREATE INDEX IF NOT EXISTS nameIndex ON featureTable (Name)";
                var indexCommand = new SQLiteCommand(indexText, conn);
                indexCommand.ExecuteNonQuery();

                indexText = "CREATE INDEX IF NOT EXISTS timeIndex on featureTable (ApexRT)";
                indexCommand = new SQLiteCommand(indexText, conn);
                indexCommand.ExecuteNonQuery();

                transaction.Commit();
                transaction.Dispose();
            }
        }

        public static List<Feature> GetFeatures(string name, SQLiteConnection conn, double minTime, double maxTime)
        {
            var queryText = "SELECT s.ID, s.mz, s.Name, s.ApexRT, s.ApexIntensity, s.SmoothFeatureString FROM featureTable s WHERE s.Name=@Name AND s.ApexRT<@Max AND s.ApexRT>@Min";
            var queryCommand = new SQLiteCommand(queryText, conn);
            queryCommand.Parameters.AddWithValue("@Name", name);
            queryCommand.Parameters.AddWithValue("@Min", minTime);
            queryCommand.Parameters.AddWithValue("@Max", maxTime);

            List<Feature> returnFeatures = new List<Feature>();

            var reader = queryCommand.ExecuteReader();
            while (reader.Read())
            {
                var id = reader["ID"].ToString();
                double mz = double.Parse(reader["mz"].ToString());
                var apexRT = double.Parse(reader["ApexRT"].ToString());
                var apexIntensity = double.Parse(reader["ApexIntensity"].ToString());
                var featString = reader["SmoothFeatureString"].ToString();
                string[] parts = featString.Split('|');
                //double mz = double.Parse(parts[0]);
                //double rt = double.Parse(parts[1]);
                string[] peaks = parts[0].Split(';');
                var feature = new Feature();
                // feature.AverageMZ = mz;
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
                returnFeatures.Add(feature);
            }
            return returnFeatures;
        }

        public static void ExtractionComplete(SQLiteConnection conn)
        {
            var updateText = "UPDATE rawFileNameTable SET Extracted = @Extracted";
            var updateCommand = new SQLiteCommand(updateText, conn);
            updateCommand.Parameters.AddWithValue("@Extracted", true);
            updateCommand.ExecuteNonQuery();
            updateCommand.Dispose();
        }

        public static void GroupingComplete(SQLiteConnection conn)
        {
            var updateText = "UPDATE rawFileNameTable SET Grouped = @Grouped";
            var updateCommand = new SQLiteCommand(updateText, conn);
            updateCommand.Parameters.AddWithValue("@Grouped", true);
            updateCommand.ExecuteNonQuery();
            updateCommand.Dispose();
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
    }
}
