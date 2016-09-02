using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Y3K_Deconvolution_Studio
{
    class CreateMaster
    {
        public List<Feature> noiseFeatures = new List<Feature>();
        public List<Feature> AnalyteFeatures = new List<Feature>();
        public List<FeatureGroup> NoiseFeatureGroups = new List<FeatureGroup>();
        public List<FeatureGroup> AnalyteFeatureGroups = new List<FeatureGroup>();
        public List<EISpectrum> NoiseSpectra = new List<EISpectrum>();
        public List<EISpectrum> AnalyteSpectra = new List<EISpectrum>();
        public Chromatogram NoiseChromatogram;
        public Chromatogram AnalyteChromatogram;
        public string NoiseDatabasePath;
        public string AnalyteDatabasePath;
        public string NoiseName;
        public string AnalyteName;

        public CreateMaster(string noisePath, string analytePath, out Master noiseMaster, out Master analyteMaster)
        {
            NoiseDatabasePath = noisePath;
            AnalyteDatabasePath = analytePath;
            PopulateDataLists();
            var noiseMast = new Master();
            noiseMast.parentFile = NoiseDatabasePath;
            noiseMast.allFeatures = noiseFeatures;
            noiseMast.allSpectra = CleanUpSpectra(NoiseSpectra);
            noiseMast.chroma = NoiseChromatogram;
            noiseMast.name = NoiseName;
            var analyteMast = new Master();
            analyteMast.parentFile = AnalyteDatabasePath;
            analyteMast.allFeatures = AnalyteFeatures;
            analyteMast.allSpectra = CleanUpSpectra(AnalyteSpectra);
            analyteMast.chroma = AnalyteChromatogram;
            analyteMast.name = AnalyteName;
            noiseMaster = noiseMast;
            analyteMaster = analyteMast;
        }

        public void PopulateDataLists()
        {
            var conn = new SQLiteConnection(@"Data Source=" + NoiseDatabasePath);
            conn.Open();
            ReadInGCFeat(noiseFeatures, NoiseFeatureGroups, NoiseSpectra, out NoiseChromatogram, out NoiseName, conn);
            conn.Close();
            conn = new SQLiteConnection(@"Data Source=" + AnalyteDatabasePath);
            conn.Open();
            ReadInGCFeat(AnalyteFeatures, AnalyteFeatureGroups, AnalyteSpectra, out AnalyteChromatogram, out AnalyteName, conn);
            MarkNoiseSpectra(NoiseSpectra, AnalyteSpectra);
        }

        public static void ReadInGCFeat(List<Feature> features, List<FeatureGroup> groups, List<EISpectrum> spectra, out Chromatogram chroma, out string name, SQLiteConnection conn)
        {
            var queryText = "SELECT s.mz, s.ApexRT, s.ApexIntensity, s.SmoothFeatureString, s.RawFeatureString, s.ID_Number FROM featureTable s";
            var queryCommand = new SQLiteCommand(queryText, conn);
            var reader = queryCommand.ExecuteReader();
            Dictionary<int, Feature> featDict = new Dictionary<int, Feature>();
            while (reader.Read())
            {
                var mz = double.Parse(reader["mz"].ToString());
                var apexIntensity = double.Parse(reader["ApexIntensity"].ToString());
                var apexRT = double.Parse(reader["ApexRT"].ToString());
                var smoothString = reader["SmoothFeatureString"].ToString();
                var rawString = reader["RawFeatureString"].ToString();
                var idNum = int.Parse(reader["ID_Number"].ToString());
                List<RTPeak> rawPeaks = ConvertFeatureStringToPeakList(rawString);
                List<RTPeak> smoothPeaks = ConvertFeatureStringToPeakList(smoothString);
                var feat = new Feature();
                feat.ApexTime = apexRT;
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
                feat.ID_Number = idNum;
                features.Add(feat);
                featDict.Add(idNum, feat);
            }
            reader.Close();

            queryText = "SELECT s.ApexRT, s.IncludedFeatures, s.PeakList, s.GroupID FROM featureGroupTable s";
            queryCommand = new SQLiteCommand(queryText, conn);
            reader = queryCommand.ExecuteReader();
            while (reader.Read())
            {
                var apexRT = double.Parse(reader["ApexRT"].ToString());
                var includedString = reader["IncludedFeatures"].ToString();
                var peakList = reader["PeakList"].ToString();
                var groupID = int.Parse(reader["GroupID"].ToString());
                List<MZPeak> peaks = ConvertPeakListString(peakList);
                string[] includedFeatures = includedString.Split(';');

                var featureGroup = new FeatureGroup();
                featureGroup.ApexTime = apexRT;
                featureGroup.finalPeaks = peaks;
                featureGroup.groupID = groupID;
                foreach (var feat in includedFeatures)
                {
                    if (!string.IsNullOrEmpty(feat))
                    {
                        int id = int.Parse(feat);
                        featureGroup.allFeatures.Add(featDict[id]);
                    }
                }
                var norm = new Normalization();
                var eiSpectrum = new EISpectrum();
                eiSpectrum.FinalEIPeaks = peaks;
                eiSpectrum.FinalNormalizedEIPeaks = peaks;
                eiSpectrum.FeatureGroup = featureGroup;
                eiSpectrum.ApexTimeEI = apexRT;
                groups.Add(featureGroup);
                spectra.Add(eiSpectrum);
            }

            queryText = "SELECT s.TICChroma, s.Name FROM ticTable s";
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
            conn.Close();
        }

        public static void MarkNoiseSpectra(List<EISpectrum> noise, List<EISpectrum> yeast)
        {
            foreach (var spec in yeast)
            {
                spec.FinalNormalizedEIPeaks = spec.FinalNormalizedEIPeaks.OrderByDescending(x => x.Intensity).ToList();
                for (int i = 0; i < 1; i++)
                {
                    var peak = spec.FinalNormalizedEIPeaks[1];
                    var mz = peak.MZ;
                    var intensity = peak.Intensity;

                    var mzRange = DoubleRange.FromPPM(mz, 5);
                    var intensityRange = new DoubleRange(intensity * .33, intensity * 3);

                    bool sameMZ = false;
                    bool sameIntensity = false;

                    double startTime = spec.ApexTimeEI - .025;
                    double stopTime = spec.ApexTimeEI + .025;

                    foreach (var candidate in noise)
                    {
                        if (candidate.ApexTimeEI < stopTime && candidate.ApexTimeEI > startTime)
                        {
                            sameIntensity = false;
                            sameMZ = false;
                            candidate.FinalNormalizedEIPeaks = candidate.FinalNormalizedEIPeaks.OrderByDescending(x => x.Intensity).ToList();
                            int considered = 0;
                            foreach (var otherPeak in candidate.FinalNormalizedEIPeaks)
                            {
                                if (mzRange.Contains(otherPeak.MZ))
                                {
                                    if (intensityRange.Contains(otherPeak.Intensity))
                                    {
                                        sameMZ = true;
                                        sameIntensity = true;
                                    }
                                }
                                considered++;
                            }
                            if (sameMZ && sameIntensity)
                            {
                                spec.isValid = false;
                                break;
                            }
                        }
                    }
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

        private static List<EISpectrum> CleanUpSpectra(List<EISpectrum> spectra)
        {
            List<EISpectrum> returnList = new List<EISpectrum>();
            foreach (var spec in spectra)
            {
                foreach (var peak in spec.FinalEIPeaks)
                {
                    spec.totalIntensity += peak.Intensity;
                }
            }

            spectra = spectra.OrderByDescending(x => x.totalIntensity).ToList();
            HashSet<EISpectrum> remove = new HashSet<EISpectrum>();
            for (int i = 0; i < spectra.Count; i++)
            {
                var bigSpec = spectra[i];
                if (!remove.Contains(bigSpec))
                {
                    var range = new DoubleRange(bigSpec.ApexTimeEI - .0025, bigSpec.ApexTimeEI + .0025);
                    for (int j = i + 1; j < spectra.Count; j++)
                    {
                        bool keepGoing = true;
                        var littleSpec = spectra[j];
                        if (!range.Contains(littleSpec.ApexTimeEI))
                        {
                            continue;
                        }
                        else
                        {
                            remove.Add(littleSpec);
                        }
                    }
                }
            }
            foreach (var rSpec in remove.ToList())
            {
                spectra.Remove(rSpec);
            }
            returnList.AddRange(spectra);
            returnList = returnList.OrderBy(x => x.ApexTimeEI).ToList();
            return returnList;
        }
    }
}
