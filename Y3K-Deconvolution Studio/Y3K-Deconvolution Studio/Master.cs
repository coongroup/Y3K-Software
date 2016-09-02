using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Y3K_Deconvolution_Studio
{
    public class Master
    {
        public Chromatogram chroma;
        public List<Feature> allFeatures;
        public List<EISpectrum> allSpectra;
        public string parentFile;
        public string name;
        public SQLiteConnection conn;

        public Master(string fileName)
        {
            parentFile = fileName;
            allFeatures = new List<Feature>();
            allSpectra = new List<EISpectrum>();
        }

        public Master()
        {

        }

        public void WriteGCMasterFile()
        {
            var outPath = parentFile.Replace(".gcfeat", ".gcmast");
            var conn = new SQLiteConnection(@"Data Source=" + outPath);
            conn.Open();
            CreateGCMasterTables(conn);
            SelectQuantIons();
            if (!GCMasterIsComplete(conn))
            {
                AddFeaturesToDatabase(conn);
                AddSpectraToDatabase(conn);
                AddTICChromatogramToDatabase(conn);
            }
            conn.Close();
        }

        //Output methods
        public bool GCMasterIsComplete(SQLiteConnection conn)
        {
            var queryText = "SELECT s.Complete FROM rawFileTable s";
            var queryCommand = new SQLiteCommand(queryText, conn);
            var reader = queryCommand.ExecuteReader();
            bool isComplete = false;
            while (reader.Read())
            {
                isComplete = bool.Parse(reader["Complete"].ToString());
            }
            reader.Close();
            queryCommand.Dispose();
            return isComplete;
        }
        public void CreateGCMasterTables(SQLiteConnection conn)
        {
            var createTableText = "CREATE TABLE IF NOT EXISTS rawFileTable (Name TEXT, TICChroma TEXT, Complete BOOLEAN)";
            var createTableCommand = new SQLiteCommand(createTableText, conn);
            createTableCommand.ExecuteNonQuery();

            createTableText =
                "CREATE TABLE IF NOT EXISTS featureTable (Name TEXT, ID_Number INT, mz DOUBLE, ApexRT DOUBLE, ApexIntensity DOUBLE, SmoothFeatureString TEXT, RawFeatureString TEXT)";
            createTableCommand = new SQLiteCommand(createTableText, conn);
            createTableCommand.ExecuteNonQuery();

            createTableText =
                "CREATE TABLE IF NOT EXISTS featureGroupTable (GroupID INT, IsValid BOOLEAN, NumPeaks INT, ApexRT DOUBLE, Name TEXT, NIST_ID TEXT, ChEBI_ID TEXT, IncludedFeatures TEXT, IsInternalStandard BOOLEAN, QuantIons TEXT, PeakList TEXT)";
            createTableCommand = new SQLiteCommand(createTableText, conn);
            createTableCommand.ExecuteNonQuery();
        }
        public void AddTICChromatogramToDatabase(SQLiteConnection conn)
        {
            var insertText = "INSERT INTO rawFileTable (Name, TICChroma, Complete) VALUES (@Name, @TICChroma, @Complete)";
            var insertCommand = new SQLiteCommand(insertText, conn);

            var ticLine = "";
            foreach (var peak in chroma)
            {
                ticLine += peak.Time + "," + peak.Intensity + ";";
            }
            insertCommand.Parameters.AddWithValue("@Name", name);
            insertCommand.Parameters.AddWithValue("@TICChroma", ticLine);
            insertCommand.Parameters.AddWithValue("@Complete", true);
            insertCommand.ExecuteNonQuery();
        }
        public void AddFeaturesToDatabase(SQLiteConnection conn)
        {
            using (var transaction = conn.BeginTransaction())
            {
                foreach (var feature in allFeatures)
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

                    var insertText = "INSERT INTO featureTable (Name, mz, ApexRT, ApexIntensity, ID_Number, SmoothFeatureString, RawFeatureString)"
                                            + " VALUES (@Name, @mz, @ApexRT, @ApexIntensity, @ID_Number, @SmoothFeatureString, @RawFeatureString)";
                    var insertCommand = new SQLiteCommand(insertText, conn);
                    insertCommand.Parameters.AddWithValue("@Name", name);
                    insertCommand.Parameters.AddWithValue("@mz", feature.averageMZ);
                    insertCommand.Parameters.AddWithValue("@ApexRT", apexRT);
                    insertCommand.Parameters.AddWithValue("@ApexIntensity", apexIntensity);
                    insertCommand.Parameters.AddWithValue("@SmoothFeatureString", smoothLine);
                    insertCommand.Parameters.AddWithValue("@RawFeatureString", rawLine);
                    insertCommand.Parameters.AddWithValue("@ID_Number", feature.ID_Number);
                    insertCommand.ExecuteNonQuery();
                }
                transaction.Commit();
                transaction.Dispose();
            }
        }
        public void AddSpectraToDatabase(SQLiteConnection conn)
        {
            var insertText =
                    "INSERT INTO featureGroupTable (GroupID, IsValid, NumPeaks, ApexRT, NIST_ID, Name, IncludedFeatures, IsInternalStandard, QuantIons, PeakList)" +
                    " VALUES (@GroupID, @IsValid, @NumPeaks, @ApexRT, @NIST_ID, @Name, @IncludedFeatures, @IsInternalStandard, @QuantIons, @PeakList)";
            using (var transaction = conn.BeginTransaction())
            {
                foreach (var spec in allSpectra)
                {
                    var includedFeatures = "";
                    foreach (var group in spec.FeatureGroup.allFeatures)
                    {
                        includedFeatures += group.ID_Number + ";";
                    }
                    var peakList = "";
                    foreach (var peak in spec.FinalEIPeaks)
                    {
                        peakList += peak.MZ + "," + peak.Intensity + ";";
                    }
                    var quantIons = "";
                    foreach (var peak in spec.quantIons)
                    {
                        quantIons += peak.MZ + "," + peak.Intensity + ";";
                    }
                    var insertCommand = new SQLiteCommand(insertText, conn);
                    insertCommand.Parameters.AddWithValue("@GroupID", spec.FeatureGroup.groupID);
                    insertCommand.Parameters.AddWithValue("@IsValid", spec.isValid);
                    insertCommand.Parameters.AddWithValue("@NumPeaks", spec.FinalEIPeaks.Count);
                    insertCommand.Parameters.AddWithValue("@ApexRT", spec.ApexTimeEI);
                    insertCommand.Parameters.AddWithValue("@NIST_ID", spec.NISTName);
                    insertCommand.Parameters.AddWithValue("@Name", spec.UserName);
                    insertCommand.Parameters.AddWithValue("@IncludedFeatures", includedFeatures);
                    insertCommand.Parameters.AddWithValue("@IsInternalStandard", spec.isInternalStandard);
                    insertCommand.Parameters.AddWithValue("@QuantIons", quantIons);
                    insertCommand.Parameters.AddWithValue("@PeakList", peakList);
                    insertCommand.ExecuteNonQuery();
                }
                transaction.Commit();
            }
        }

        //Input method
        public void ReadInMaster()
        {
            conn = new SQLiteConnection(@"Data Source=" + parentFile);
            conn.Open();
            var queryText =
                "SELECT s.Name, s.mz, s.ApexRT, s.ApexIntensity, s.ID_Number, s.SmoothFeatureString, s.RawFeatureString FROM featureTable s";
            var queryCommand = new SQLiteCommand(queryText, conn);
            var reader = queryCommand.ExecuteReader();
            Dictionary<int, Feature> tmpFeatureDict = new Dictionary<int, Feature>();
            while (reader.Read())
            {
                name = reader["Name"].ToString();
                var mz = double.Parse(reader["mz"].ToString());
                var apexRT = double.Parse(reader["ApexRT"].ToString());
                var apexIntensity = double.Parse(reader["ApexIntensity"].ToString());
                var ID_Number = int.Parse(reader["ID_Number"].ToString());
                var smoothFeatureString = reader["SmoothFeatureString"].ToString();
                List<RTPeak> smoothPeaks = ConvertFeatureStringToPeakList(smoothFeatureString);
                var rawFeatureString = reader["RawFeatureString"].ToString();
                List<RTPeak> rawPeaks = ConvertFeatureStringToPeakList(rawFeatureString);
                var feat = new Feature();
                feat.AverageMZ = mz;
                feat.ApexTime = apexRT;
                feat.ID_Number = ID_Number;
                foreach (var peak in rawPeaks)
                {
                    feat.AddPeak(peak);
                }
                foreach (var peak in smoothPeaks)
                {
                    feat.AddSmoothPeak(peak);
                }
                feat.AverageMZ = mz;
                feat.maxIntensity = apexIntensity;
                tmpFeatureDict.Add(ID_Number, feat);
            }

            queryText =
                "SELECT s.GroupID, s.IsValid, s.NumPeaks, s.ApexRT, s.NIST_ID, s.Name, s.ChEBI_ID, s.IncludedFeatures, s.IsInternalStandard, s.QuantIons, s.PeakList FROM featureGroupTable s";
            queryCommand = new SQLiteCommand(queryText, conn);
            reader = queryCommand.ExecuteReader();
            while (reader.Read())
            {
                var groupID = int.Parse(reader["GroupID"].ToString());
                var isValid = bool.Parse(reader["IsValid"].ToString());
                //var isValid = false;
                //var isValidString = reader["IsValid"].ToString();
                //if (isValidString.Equals("1"))
                //{
                //    isValid = true;
                //}

                var numPeaks = int.Parse(reader["NumPeaks"].ToString());
                var apexRT = double.Parse(reader["ApexRT"].ToString());
                var chebiID = reader["ChEBI_ID"].ToString();
                var NISTID = reader["NIST_ID"].ToString();
                var userName = reader["Name"].ToString();
                //var isInternalStandard = bool.Parse(reader["IsInternalStandard"].ToString());
                var isInternalStandard = false;
                var isInternalStandardString = reader["IsInternalStandard"].ToString();
                if (isInternalStandardString.Equals("1"))
                {
                    isInternalStandard = true;
                }
                var includedFeatureString = reader["IncludedFeatures"].ToString();
                var quantIonString = reader["QuantIons"].ToString();
                List<MZPeak> quantIons = ConvertPeakListString(quantIonString);
                var peakListString = reader["PeakList"].ToString();
                List<MZPeak> peakList = ConvertPeakListString(peakListString);
                var featureGroup = new FeatureGroup();
                featureGroup.groupID = groupID;
                featureGroup.ApexTime = apexRT;
                featureGroup.finalPeaks = peakList;

                foreach (var feat in includedFeatureString.Split(';'))
                {
                    if (!string.IsNullOrEmpty(feat))
                    {
                        int id = int.Parse(feat);
                        featureGroup.allFeatures.Add(tmpFeatureDict[id]);
                    }
                }

                var eiSpectrum = new EISpectrum();
                //if (isInternalStandard==1)
                //{
                //    eiSpectrum.isInternalStandard = true;
                //}
                //else
                //{
                //    eiSpectrum.isInternalStandard = false;
                //}
                eiSpectrum.isInternalStandard = isInternalStandard;
                eiSpectrum.FinalEIPeaks.AddRange(peakList);
                eiSpectrum.FinalNormalizedEIPeaks.AddRange(peakList);
                var norm = new Normalization();
                norm.CombineLikeMZPeaks(eiSpectrum.FinalNormalizedEIPeaks);
                eiSpectrum.FeatureGroup = featureGroup;
                eiSpectrum.ApexTimeEI = apexRT;
                eiSpectrum.NISTName = NISTID;
                eiSpectrum.UserName = userName;
                eiSpectrum.chebiID = chebiID;
                //if (isValid == 1)
                //{
                //    eiSpectrum.isValid = true;
                //}

                //else
                //{
                //    eiSpectrum.isValid = false;
                //}
                eiSpectrum.isValid = isValid;
                eiSpectrum.spectrumID = groupID;
                eiSpectrum.quantIons = quantIons;
                allSpectra.Add(eiSpectrum);
            }

            queryText = "SELECT s.TICChroma, s.Name FROM rawFileTable s";
            queryCommand = new SQLiteCommand(queryText, conn);
            reader = queryCommand.ExecuteReader();
            List<double> rts = new List<double>();
            List<double> intensities = new List<double>();
            name = "";
            chroma = null;
            while (reader.Read())
            {
                var tic = reader["TICChroma"].ToString();
                name = reader["Name"].ToString();
                string[] parts = tic.Split(';');
                foreach (var part in parts)
                {
                    if (!string.IsNullOrEmpty(part))
                    {
                        string[] subparts = part.Split(',');
                        double rt = double.Parse(subparts[0]);
                        double intensity = double.Parse(subparts[1]);
                        rts.Add(rt);
                        intensities.Add(intensity);
                    }
                }
                chroma = new Chromatogram(rts.ToArray(), intensities.ToArray());
            }
            allFeatures = tmpFeatureDict.Values.ToList();
            foreach (var spec in allSpectra)
            {
                foreach (var feat in spec.FeatureGroup.allFeatures)
                {
                    spec.FeatureGroup.includedFeatureIDs.Add(feat.ID_Number);
                }
            }
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
        public static List<MZPeak> ConvertPeakListString(string peakList)
        {
            List<MZPeak> returnList = new List<MZPeak>();
            string[] parts = peakList.Split(';');
            foreach (var part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    string[] subparts = part.Split(',');
                    double mz = double.Parse(subparts[0]);
                    double intensity = double.Parse(subparts[1]);
                    var newPeak = new MZPeak(mz, intensity);
                    returnList.Add(newPeak);
                }
            }
            return returnList;
        }

        public void SelectQuantIons()
        {
            var _73Range = DoubleRange.FromPPM(73.0467, 10);
            var _147Range = DoubleRange.FromPPM(147.0654, 10);
            var _149Range = DoubleRange.FromPPM(149.0447, 10);
            foreach (var spec in allSpectra)
            {
                spec.FinalEIPeaks = spec.FinalEIPeaks.OrderByDescending(x => x.Intensity).ToList();
                foreach (var peak in spec.FinalEIPeaks)
                {
                    if (spec.quantIons.Count == 1)
                    {
                        break;
                    }
                    else
                    {
                        if (!_73Range.Contains(peak.MZ) && !_147Range.Contains(peak.MZ) && !_149Range.Contains(peak.MZ))
                        {
                            spec.quantIons.Add(peak);
                        }
                    }
                }
            }
        }
    }
}
