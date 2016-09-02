using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Y3K_Deconvolution_Engine
{
    public class FeatureDetectionMethods : IDisposable
    {
        private List<FeatureGroup> finalGroups;
        private List<EISpectrum> eiSpectra;
        private ThermoRawFile rawFile;
        private bool written;
        private List<Feature> currentValidFeatures;
        bool disposed = false;
        public int ID = 1;
        public Normalization norm;

        public List<FeatureGroup> FinalGroups
        {
            get { return this.finalGroups; }
        }
        public List<EISpectrum> EISpectra
        {
            get { return this.eiSpectra; }
        }

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

        private void OnProgressUpdate(List<Feature> features, double percent)
        {
            var handler = Progress;
            if (handler != null)
            {
                handler(this, new ProgressStatusEventArgs(features, percent));
            }
        }

        private void OnFinish()
        {
            this.finalGroups = null;
            this.eiSpectra = null;
            this.rawFile.ClearCachedScans();
            this.norm = null;
            this.currentValidFeatures = null;
            GC.Collect();
            var handler = Finished;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public FeatureDetectionMethods(ThermoRawFile rawFile)
        {
            this.finalGroups = new List<FeatureGroup>();
            this.eiSpectra = new List<EISpectrum>();
            this.rawFile = rawFile;
            this.norm = new Normalization();
        }


        public void StepwiseSetupFinalGroups()
        {
            StepwiseSetupFinalGroups(this.rawFile);
        }

        public void StepwiseSetupFinalGroups(ThermoRawFile rawfile, SQLiteConnection conn)
        {
            List<int> list = new List<int>();
            foreach (var scan in rawFile.GetMsScans())
            {
                if (scan.MsnOrder == 1)
                {
                    list.Add(scan.SpectrumNumber);
                }
            }
            var extractedFeatures = ExtractFeatures(list, 10).ToList();

            //Features have been extracted
            //Check which features are valid and which are not based on how their max intensity changes

            //nwk temp code
            var tmpFeatures = extractedFeatures.Where(x => x.ApexTime > 13.55 && x.apexTime < 13.70 && x.AverageMZ > 267.082 && x.averageMZ < 267.095).ToList();
            var q = "";



            CheckPeakRise(extractedFeatures);
            //Smooth features now
            foreach (Feature feature in extractedFeatures)
            {
                feature.SmoothRTPeaks = GetRollingAveragePeaks(feature.RawRTPeaks, 9);
            }
            CheckPeakRiseSmooth(extractedFeatures);
            List<Feature> finalFeatures = GetFinalFeatures(extractedFeatures);
            extractedFeatures.Clear();
            CheckPeakRiseSmooth(finalFeatures);
            int count = 1;
            foreach (var feat in finalFeatures)
            {
                feat.ID_Number = count;
                count++;
            }
            //push features to database
            PushFeaturesToDatabase(rawfile, finalFeatures, conn);


            List<FeatureGroup> groups = GroupFeatures(finalFeatures, .04, 2).ToList();
            DoubleBack(groups, finalFeatures);
            finalFeatures.Clear();
            List<FeatureGroup> allSubGroups = new List<FeatureGroup>();
            foreach (FeatureGroup group in groups)
            {
                GetMainSubgroup(group);
                GetFeatureSubGroups(group);
                if (group.mainSubGroup.allFeatures.Count > 1)
                {
                    allSubGroups.Add(group.mainSubGroup);
                }
                foreach (FeatureGroup subGroup in group.SubGroups)
                {
                    allSubGroups.Add(subGroup);
                }
            }
            this.finalGroups = GetFinalFeatures(allSubGroups);
            foreach (FeatureGroup group in this.finalGroups)
            {
                group.finalPeaks = GetPeaksFromNearestSpectrum(group, rawFile);
            }
            foreach (FeatureGroup group in this.finalGroups)
            {
                EISpectrum spec = new EISpectrum();
                spec.ApexTimeEI = group.ApexTime;
                spec.FeatureGroup = group;
                spec.FinalEIPeaks = group.finalPeaks;
                spec.FinalEIPeaks = spec.FinalEIPeaks.OrderByDescending(x => x.Intensity).ToList();
                if (group.finalPeaks.Count >= 2) //here as well (changed to remove num feature restriction)
                {
                    spec.BasePeak = spec.FinalEIPeaks[0];
                    this.eiSpectra.Add(spec);
                }
                spec.FinalNormalizedEIPeaks.AddRange(spec.FinalEIPeaks);
                spec.FinalNormalizedEIPeaks = spec.FinalNormalizedEIPeaks.OrderBy(x => x.MZ).ToList();
                norm.CombineLikeMZPeaks(spec.FinalNormalizedEIPeaks);
                spec.AdjustedPeaks.AddRange(spec.FinalNormalizedEIPeaks);
                norm.GetAdjustedPeaks(spec.AdjustedPeaks);
                spec.DenominatorTerm = norm.GetDemoninatorTerm(spec.AdjustedPeaks);
            }
            //this.eiSpectra = CleanUpSpectra(eiSpectra);
            this.eiSpectra = eiSpectra.Where(x => x.FinalEIPeaks.Count >= 2).ToList();
            AddFeatureGroupsToDatabase(this.eiSpectra, conn);
            SQLiteIOMethods.ExtractionComplete(conn);
            SQLiteIOMethods.GroupingComplete(conn);
            OnFinish();
        }

        public void SetupGroupsGetFeatures(ThermoRawFile rawfile, out List<Feature> allFeatures)
        {
            List<int> list = new List<int>();
            foreach (var scan in rawFile.GetMsScans())
            {
                if (scan.RetentionTime < 7)
                {
                    list.Add(scan.SpectrumNumber);
                }
            }
            var extractedFeatures = ExtractFeatures(list, 10).ToList();

            //Features have been extracted
            //Check which features are valid and which are not based on how their max intensity changes
            CheckPeakRise(extractedFeatures);
            //Smooth features now
            foreach (Feature feature in extractedFeatures)
            {
                feature.SmoothRTPeaks = GetRollingAveragePeaks(feature.RawRTPeaks, 9);
            }
            CheckPeakRiseSmooth(extractedFeatures);
            List<Feature> finalFeatures = GetFinalFeatures(extractedFeatures);
            extractedFeatures.Clear();
            CheckPeakRiseSmooth(finalFeatures);
            List<FeatureGroup> groups = GroupFeatures(finalFeatures, .04, 2).ToList();
            DoubleBack(groups, finalFeatures);
            allFeatures = finalFeatures;
            //finalFeatures.Clear();
            List<FeatureGroup> allSubGroups = new List<FeatureGroup>();
            foreach (FeatureGroup group in groups)
            {
                GetMainSubgroup(group);
                GetFeatureSubGroups(group);
                if (group.mainSubGroup.allFeatures.Count > 1)
                {
                    allSubGroups.Add(group.mainSubGroup);
                }
                foreach (FeatureGroup subGroup in group.SubGroups)
                {
                    allSubGroups.Add(subGroup);
                }
            }
            this.finalGroups = GetFinalFeatures(allSubGroups);
            foreach (FeatureGroup group in this.finalGroups)
            {
                group.finalPeaks = GetPeaksFromNearestSpectrum(group, rawFile);
            }
            foreach (FeatureGroup group in this.finalGroups)
            {
                EISpectrum spec = new EISpectrum();
                spec.ApexTimeEI = group.ApexTime;
                spec.FeatureGroup = group;
                spec.FinalEIPeaks = group.finalPeaks;
                spec.FinalEIPeaks = spec.FinalEIPeaks.OrderByDescending(x => x.Intensity).ToList();
                if (group.finalPeaks.Count > 6) //here as well (changed to remove num feature restriction)
                {
                    spec.BasePeak = spec.FinalEIPeaks[0];
                    this.eiSpectra.Add(spec);
                }
                spec.FinalNormalizedEIPeaks.AddRange(spec.FinalEIPeaks);
                spec.FinalNormalizedEIPeaks = spec.FinalNormalizedEIPeaks.OrderBy(x => x.MZ).ToList();
                norm.CombineLikeMZPeaks(spec.FinalNormalizedEIPeaks);
                spec.AdjustedPeaks.AddRange(spec.FinalNormalizedEIPeaks);
                norm.GetAdjustedPeaks(spec.AdjustedPeaks);
                spec.DenominatorTerm = norm.GetDemoninatorTerm(spec.AdjustedPeaks);
            }

        }

        private void StepwiseSetupFinalGroups(ThermoRawFile rawFile)
        {
            List<int> list = new List<int>();
            foreach (var scan in rawFile.GetMsScans())
            {
                if (scan.RetentionTime < 7)
                {
                    list.Add(scan.SpectrumNumber);
                }
            }
            var extractedFeatures = ExtractFeatures(list, 10).ToList();

            //Features have been extracted
            //Check which features are valid and which are not based on how their max intensity changes
            CheckPeakRise(extractedFeatures);
            //Smooth features now
            foreach (Feature feature in extractedFeatures)
            {
                feature.SmoothRTPeaks = GetRollingAveragePeaks(feature.RawRTPeaks, 9);
            }
            CheckPeakRiseSmooth(extractedFeatures);
            List<Feature> finalFeatures = GetFinalFeatures(extractedFeatures);
            extractedFeatures.Clear();
            CheckPeakRiseSmooth(finalFeatures);
            List<FeatureGroup> groups = GroupFeatures(finalFeatures, .04, 2).ToList();
            DoubleBack(groups, finalFeatures);
            finalFeatures.Clear();
            List<FeatureGroup> allSubGroups = new List<FeatureGroup>();
            foreach (FeatureGroup group in groups)
            {
                GetMainSubgroup(group);
                GetFeatureSubGroups(group);
                if (group.mainSubGroup.allFeatures.Count > 1)
                {
                    allSubGroups.Add(group.mainSubGroup);
                }
                foreach (FeatureGroup subGroup in group.SubGroups)
                {
                    allSubGroups.Add(subGroup);
                }
            }
            this.finalGroups = GetFinalFeatures(allSubGroups);
            foreach (FeatureGroup group in this.finalGroups)
            {
                group.finalPeaks = GetPeaksFromNearestSpectrum(group, rawFile);
            }
            foreach (FeatureGroup group in this.finalGroups)
            {
                EISpectrum spec = new EISpectrum();
                spec.ApexTimeEI = group.ApexTime;
                spec.FeatureGroup = group;
                spec.FinalEIPeaks = group.finalPeaks;
                spec.FinalEIPeaks = spec.FinalEIPeaks.OrderByDescending(x => x.Intensity).ToList();
                if (group.finalPeaks.Count > 6) //here as well (changed to remove num feature restriction)
                {
                    spec.BasePeak = spec.FinalEIPeaks[0];
                    this.eiSpectra.Add(spec);
                }
                spec.FinalNormalizedEIPeaks.AddRange(spec.FinalEIPeaks);
                spec.FinalNormalizedEIPeaks = spec.FinalNormalizedEIPeaks.OrderBy(x => x.MZ).ToList();
                norm.CombineLikeMZPeaks(spec.FinalNormalizedEIPeaks);
                spec.AdjustedPeaks.AddRange(spec.FinalNormalizedEIPeaks);
                norm.GetAdjustedPeaks(spec.AdjustedPeaks);
                spec.DenominatorTerm = norm.GetDemoninatorTerm(spec.AdjustedPeaks);
            }

            OnFinish();
        }

        public IEnumerable<Feature> ExtractFeatures(IList<int> scans, double ppmTolerance, int minConsecutiveScans = 5)
        {
            List<Feature> activeFeatures = new List<Feature>();
            List<ThermoMzPeak> outPeaks = new List<ThermoMzPeak>();
            rawFile.GetLabeledSpectrum(scans[0]).TryGetPeaks(0, 1000, out outPeaks);
            double currRT = rawFile.GetRetentionTime(scans[0]);
            //MZPeak[] currentPeaks = currentScan.MassSpectrum.GetPeaks(0, 1000).ToArray();
            MZPeak[] currentPeaks = outPeaks.ToArray();
            activeFeatures.AddRange(currentPeaks.Select(p => new Feature(p, currRT)));
            int counter = 0;
            int percent = (scans.Count) / 100;
            for (int i = 1; i < scans.Count; i++)
            {
                rawFile.ClearCachedScans();
                if (i % percent == 0)
                {
                    counter++;
                    if (counter < 100)
                    {
                        OnProgressUpdate(0.01);
                    }
                }
                MSDataScan<ThermoSpectrum> currentScan = rawFile.GetMsScan(scans[i]);
                //currentScan = scans[i];

                // outPeaks = new List<ThermoMzPeak>();
                // double[,] peakArray = currentScan.MassSpectrum.ToArray();
                //for (int j = 0; j < peakArray.Length / 2; j++)
                //{
                //    outPeaks.Add(new MZPeak(peakArray[0, j], peakArray[1, j]));
                //}
                rawFile.GetLabeledSpectrum(currentScan.SpectrumNumber).TryGetPeaks(0, 1000, out outPeaks);
                outPeaks = outPeaks.OrderBy(x => x.MZ).ToList();
                //currentPeaks = currentScan.MassSpectrum.GetPeaks(0, 1000).ToArray();
                currentPeaks = outPeaks.ToArray();

                double rt = currentScan.RetentionTime;

                // Order features based on their average m/z which may change each round as new peaks are added
                activeFeatures = activeFeatures.OrderBy(feat => feat.AverageMZ).ToList();

                // Match all the active features to the current spectrum at a given ppm tolerance
                MatchPeaks(activeFeatures, currentPeaks, ppmTolerance, rt);

                // Find features that are finished and return them if they pass the filters
                int f = 0;
                while (f < activeFeatures.Count)
                {
                    var feature = activeFeatures[f];
                    //DJB
                    //if (feature.LastAddedTime < rt || feature.CurrentState == PeakState.Tailing)
                    //NWK
                    if (feature.MaxRT < rt)
                    {
                        //DJB
                        //if (feature.Count >= minConsecutiveScans && (feature.TotalStates & PeakState.Tailing) == PeakState.Tailing)
                        //NWK
                        if (feature.Count >= minConsecutiveScans && CheckPeakRise(feature))
                        {
                            yield return feature;
                        }
                        activeFeatures.RemoveAt(f);
                    }
                    else
                    {
                        f++;
                    }
                }
            }

            //DJB
            //foreach (var feature in activeFeatures.Where(f => f.Count > minConsecutiveScans && f.CurrentState != PeakState.Random))
            //NWK
            foreach (var feature in activeFeatures.Where(f => f.Count > minConsecutiveScans))
                if (CheckPeakRise(feature))
                {
                    yield return feature;
                }
        }

        private void MatchPeaks(List<Feature> features, MZPeak[] currentPeaks, double ppmTolerance, double rt, bool allowMultipleFeatureMatches = false)
        {
            if (features == null || currentPeaks == null)
                return;

            int fCount = features.Count;

            if (fCount == 0)
            {
                features.AddRange(currentPeaks.Select(p => new Feature(p, rt)));
                return;
            }

            int cCount = currentPeaks.Length;

            if (cCount == 0)
                return;

            int c = 0;
            int f = 0;
            Feature feature = features[f];
            double featureMZ = feature.AverageMZ;

            MZPeak cPeak = currentPeaks[c];

            ppmTolerance /= 2e6;

            double tolerance = cPeak.MZ * ppmTolerance;
            double lowmz = cPeak.MZ - tolerance;

            List<Feature> featuresToAdd = new List<Feature>(cCount);

            while (true)
            {
                if (featureMZ < lowmz)
                {
                    if (++f == fCount) // Feature mz is to small, go to the next one 
                        break; // no more features, so just break out now
                    feature = features[f];
                    featureMZ = feature.AverageMZ;
                    continue; // keep the current peak, so continue the loop right away
                }

                if (featureMZ < cPeak.MZ + tolerance)
                {
                    feature.AddPeak(cPeak, rt); // Peak matches feature, add it to the feature
                    if (!allowMultipleFeatureMatches)
                    {
                        if (++f == fCount) // Feature mz is to small, go to the next one 
                            break; // no more features, so just break out now
                        feature = features[f];
                        featureMZ = feature.AverageMZ;
                    }
                }
                else
                {
                    // Peak didn't match any features, so save this peak to start a new feature
                    featuresToAdd.Add(new Feature(cPeak, rt));
                }

                if (++c == cCount)
                    break; // no more current peaks, so just break out now
                cPeak = currentPeaks[c];
                tolerance = cPeak.MZ * ppmTolerance;
                lowmz = cPeak.MZ - tolerance;
            }

            // Append unmatched peaks as new features
            features.AddRange(featuresToAdd);

            // Add left over peaks as new features
            while (c < cCount)
            {
                features.Add(new Feature(currentPeaks[c++], rt));
            }
        }


        public void CheckPeakRise(List<Feature> features)
        {
            for (int i = features.Count - 1; i >= 0; i--)
            {
                Feature currFeat = features[i];
                if (currFeat.MaxPeak.Intensity < currFeat.FirstIntensity * 2 ||
                    currFeat.MaxPeak.Intensity < currFeat.LastIntensity * 2)
                {
                    features.RemoveAt(i);
                }

            }
        }

        public bool CheckPeakRise(Feature feature)
        {
            if (feature.MaxPeak.Intensity < feature.FirstIntensity * 2 ||
                feature.MaxPeak.Intensity < feature.LastIntensity * 2)
            {
                return false;
            }
            return true;
        }

        public void CheckPeakRiseSmooth(List<Feature> features)
        {

            for (int i = features.Count - 1; i >= 0; i--)
            {
                Feature currFeat = features[i];
                if (currFeat.SmoothRTPeaks.Count > 0)
                {
                    double minIntensity = currFeat.SmoothRTPeaks.First().Intensity;
                    if (currFeat.SmoothRTPeaks.Last().Intensity < minIntensity)
                    {
                        minIntensity = currFeat.SmoothRTPeaks.Last().Intensity;
                    }
                    double threshold = minIntensity * 2;
                    bool remove = true;
                    foreach (RTPeak peak in currFeat.SmoothRTPeaks)
                    {
                        if (peak.Intensity > threshold)
                        {
                            remove = false;

                            break;
                        }
                    }
                    if (remove)
                    {
                        features.RemoveAt(i);
                    }
                }
                else
                {
                    features.RemoveAt(i);
                }
            }
        }

        public List<RTPeak> GetRollingAveragePeaks(List<RTPeak> peaks, int period)
        {
            List<RTPeak> outPeaks = new List<RTPeak>();
            for (int i = 0; i < peaks.Count; i++)
            {
                if (i >= period)
                {
                    double total = 0;
                    for (int x = i; x > (i - period); x--)
                    {
                        total += peaks[x].Intensity;
                    }
                    double average = total / (double)period;
                    RTPeak newPeak = new RTPeak(peaks[i].MZ, average, peaks[i - (period / 2)].RT);
                    outPeaks.Add(newPeak);
                }
            }
            return outPeaks;
        }

        public List<Feature> GetFinalFeatures(List<Feature> features)
        {
            List<Feature> finalFeatures = new List<Feature>();
            foreach (Feature feature in features)
            {
                if (feature.SmoothRTPeaks != null && feature.SmoothRTPeaks.Count > 0)
                {
                    finalFeatures.AddRange(GetApexTimePoints(feature));
                }
            }
            return finalFeatures;
        }

        public List<Feature> GetApexTimePoints(Feature feature)
        {
            List<RTPeak> rtPeaks = feature.SmoothRTPeaks;
            List<double> apexPoints = new List<double>();
            List<int> apexIndexes = new List<int>();
            List<Feature> returnFeatures = new List<Feature>();
            double peakRise = 1.05;
            double peakFall = 0.9;
            if (rtPeaks.Count > 0)
            {
                double intensityLeft = rtPeaks.First().Intensity;
                double intensityRight = rtPeaks.Last().Intensity;
                double threshold = intensityLeft * 2.5;
                if (intensityRight < intensityLeft)
                {
                    threshold = intensityRight * 2.5;
                }
                bool rising = false;
                bool falling = false;
                RTPeak holdingMaxPeak = new RTPeak(0, 0, 0);
                int holdingMaxIndex = 0;
                for (int i = 0; i < rtPeaks.Count - 1; i++)
                {
                    RTPeak currentPoint = rtPeaks[i];
                    RTPeak nextPoint = rtPeaks[i + 1];
                    if (!rising && !falling)
                    {
                        if (nextPoint.Intensity >= (currentPoint.Intensity * peakRise) && nextPoint.Intensity > threshold)
                        {
                            rising = true;
                        }
                    }
                    if (rising)
                    {
                        if (nextPoint.Intensity > currentPoint.Intensity)
                        {
                            continue;
                        }
                        else
                        {
                            holdingMaxPeak = currentPoint;
                            holdingMaxIndex = i;
                            rising = false;
                            falling = true;
                        }
                    }
                    if (falling)
                    {
                        if (nextPoint.Intensity < holdingMaxPeak.Intensity)
                        {
                            if (nextPoint.Intensity < holdingMaxPeak.Intensity * peakFall)
                            {
                                apexPoints.Add(holdingMaxPeak.RT);
                                apexIndexes.Add(holdingMaxIndex);
                                falling = false;
                            }
                        }
                        else
                        {
                            holdingMaxPeak = nextPoint;
                            holdingMaxIndex = i + 1;
                        }
                    }
                }
                var subFeatures = GetSubFeatures(feature, apexIndexes).ToList();
                return subFeatures;
            }
            return null;
        }

        public IEnumerable<Feature> GetSubFeatures(Feature feature, List<int> apexIndexes)
        {
            List<Feature> returnList = new List<Feature>();
            foreach (int apexIndex in apexIndexes)
            {
                double threshold = feature.SmoothRTPeaks[apexIndex].Intensity * 0.99;
                int leftIndex = GetLeftStopIndex(apexIndex, feature.SmoothRTPeaks, threshold);
                int rightIndex = GetRightStopIndex(apexIndex, feature.SmoothRTPeaks, threshold);

                Feature newFeature = new Feature();
                newFeature.ApexTime = feature.SmoothRTPeaks[apexIndex].RT;
                for (int i = leftIndex; i <= rightIndex; i++)
                {
                    newFeature.AddSmoothPeak(feature.SmoothRTPeaks[i]);
                }
                yield return newFeature;
            }
        }

        public int GetLeftStopIndex(int startIndex, List<RTPeak> smoothPeaks, double threshold)
        {
            if (startIndex - 1 >= 0)
            {
                RTPeak previousPeak = smoothPeaks[startIndex];
                RTPeak currentpeak = smoothPeaks[startIndex - 1];

                if (currentpeak.Intensity > previousPeak.Intensity && currentpeak.Intensity < threshold)
                {
                    return startIndex;
                }
                return GetLeftStopIndex(startIndex - 1, smoothPeaks, threshold);
            }
            else
            {
                return startIndex;
            }
        }

        public int GetRightStopIndex(int startIndex, List<RTPeak> smoothPeaks, double threshold)
        {
            if (startIndex + 1 < smoothPeaks.Count)
            {
                RTPeak previousPeak = smoothPeaks[startIndex];
                RTPeak currentpeak = smoothPeaks[startIndex + 1];

                if (currentpeak.Intensity > previousPeak.Intensity && currentpeak.Intensity < threshold)
                {
                    return startIndex;
                }
                return GetRightStopIndex(startIndex + 1, smoothPeaks, threshold);
            }
            return startIndex;
        }

        private IEnumerable<FeatureGroup> GroupFeatures(IEnumerable<Feature> features, double time, int minNumberFragments = 5)
        {
            List<Feature> totalFeatures = features.OrderByDescending(f => f.MaxIntensity).ToList();
            while (totalFeatures.Count > 0)
            {
                Feature maxfeature = totalFeatures[0];
                totalFeatures.RemoveAt(0);
                double maxFeatureApex = maxfeature.ApexTime;
                FeatureGroup group = new FeatureGroup(maxfeature);
                group.ApexTime = maxFeatureApex;
                double minTime = maxFeatureApex - time / 2;
                double maxTime = maxFeatureApex + time / 2;
                int i = 0;
                while (i < totalFeatures.Count)
                {
                    var feature = totalFeatures[i];
                    if (minTime <= feature.ApexTime && feature.ApexTime <= maxTime)
                    {
                        group.AddFeature(feature);
                        totalFeatures.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
                if (group.Count >= minNumberFragments)
                {
                    yield return group;
                }

            }
        }

        private void DoubleBack(List<FeatureGroup> groups, List<Feature> features)
        {
            groups = groups.OrderBy(x => x.ApexTime).ToList();
            features = features.OrderBy(x => x.ApexTime).ToList();

            int minGroupIndex = 0;

            foreach (Feature feature in features)
            {
                double minTime = feature.ApexTime - .04;
                double maxTime = feature.ApexTime;
                while (groups[minGroupIndex].ApexTime < minTime && minGroupIndex < groups.Count - 1)
                {
                    minGroupIndex++;
                }

                for (int i = minGroupIndex; i < groups.Count; i++)
                {
                    if (groups[i].ApexTime > maxTime)
                    {
                        break;
                    }
                    if (!groups[i].featureMZHashSet.Contains(feature.AverageMZ))
                    {
                        groups[i].AddFeature(feature);
                    }
                }
            }
        }

        public void GetMainSubgroup(FeatureGroup group)
        {
            FeatureGroup mainSubGroup = new FeatureGroup();
            double startTime = group.ApexTime - .005;
            double stopTime = group.ApexTime + .005;

            startTime = group.ApexTime;
            stopTime = group.ApexTime;

            Tuple<double, double> apexWindow = GetApexTimeWindow(group.allFeatures.First(), GetRTPeakIndex(group.allFeatures.First(), group.allFeatures.First().MaxPeak));
            startTime = apexWindow.Item1;
            stopTime = apexWindow.Item2;

            foreach (Feature feature in group.allFeatures)
            {
                if (ApexCheck(feature, startTime, stopTime))
                {
                    mainSubGroup.AddFeature(feature);
                }
            }
            mainSubGroup.ApexTime = group.ApexTime;
            mainSubGroup.DoApexCalculations();
            group.mainSubGroup = mainSubGroup;
            group.SubGroups.Add(mainSubGroup);
        }

        public void GetFeatureSubGroups(FeatureGroup group)
        {
            group.allFeatures = group.allFeatures.OrderByDescending(x => x.MaxPeak.Intensity).ToList();
            foreach (Feature feature in group.allFeatures)
            {
                feature.includedInSubGroup = false;
            }
            group.maxFeature = group.allFeatures[0];
            GetFeatureSubGroups(group, 0, 0);
        }

        public void GetFeatureSubGroups(FeatureGroup group, int nextSubGroupPointer, int numFeaturesInSubGroups)
        {
            bool nextSubGroupPointerFound = false;
            if (numFeaturesInSubGroups != group.allFeatures.Count)
            {
                Feature maxFeature = group.allFeatures[nextSubGroupPointer];
                // if (maxFeature.MaxPeak.Intensity < group.maxFeature.MaxPeak.Intensity * .06 || maxFeature.MaxPeak.Intensity < 100000 )
                if (maxFeature.MaxPeak.Intensity < group.maxFeature.MaxPeak.Intensity * .06)
                {
                    return;
                }
                //Binary search to the appropriate index (you'll need to store the actual RTPeak instead of just the intensity and time)
                int startIndex = GetRTPeakIndex(maxFeature, maxFeature.MaxPeak);
                //Get the apex time window for 93.5% intensity threshold
                Tuple<double, double> ApexTimeWindow = GetApexTimeWindow(maxFeature, startIndex);
                //Create a new subgroup, add the feature to the subgroup.
                FeatureGroup newSubGroup = new FeatureGroup(maxFeature);
                newSubGroup.ApexTime = maxFeature.ApexTime;
                //Check whether this has been included in a sub group. If not set that flag to true and increment numFeaturesInSubGroups.
                if (!maxFeature.includedInSubGroup)
                {
                    maxFeature.includedInSubGroup = true;
                    numFeaturesInSubGroups++;
                }
                maxFeature.numSubGroups++;
                //Iterate over all features in the group and see if their apexes align
                for (int i = 0; i < group.allFeatures.Count; i++)
                {
                    //Check whether there is an apex for the feature if (ApexCheck(feature, 93.5 start, 93.5 stop))... Add to subgroup, if Feature hasn't been in a subgroup yet
                    if (i != nextSubGroupPointer)
                    {
                        Feature currFeature = group.allFeatures[i];
                        if (ApexCheck(currFeature, ApexTimeWindow.Item1, ApexTimeWindow.Item2))
                        {
                            newSubGroup.AddFeature(currFeature);
                            if (!currFeature.includedInSubGroup)
                            {
                                currFeature.includedInSubGroup = true;
                                numFeaturesInSubGroups++;
                            }
                            currFeature.numSubGroups++;
                        }
                        else
                        {
                            if (i > nextSubGroupPointer && !nextSubGroupPointerFound && !currFeature.includedInSubGroup)
                            {
                                nextSubGroupPointer = i;
                                nextSubGroupPointerFound = true;
                            }
                        }
                    }
                }
                newSubGroup.DoApexCalculations();
                if (newSubGroup.allFeatures.Count > 1)
                {
                    group.SubGroups.Add(newSubGroup);
                }
                GetFeatureSubGroups(group, nextSubGroupPointer, numFeaturesInSubGroups);
            }
        }

        public bool ApexCheck(Feature feature, double startTime, double stopTime)
        {
            var relevantPeaks = feature.SmoothRTPeaks.Where(x => x.RT <= stopTime && x.RT >= startTime).ToList();
            if (relevantPeaks.Count > 0)
            {
                double startIntensity = relevantPeaks.First().Intensity;
                double stopIntensity = relevantPeaks.Last().Intensity;
                double maxIntensity = 0;
                RTPeak maxPeak = null;
                bool valid = false;
                foreach (RTPeak peak in relevantPeaks)
                {
                    if (peak.Intensity > startIntensity && peak.Intensity > stopIntensity)
                    {
                        valid = true;
                        if (peak.Intensity > maxIntensity)
                        {
                            maxIntensity = peak.Intensity;
                            maxPeak = peak;
                            feature.MaxPeak = peak;
                        }
                    }
                }
                return valid;
            }
            return false;

        }

        public int GetRTPeakIndex(Feature feature, RTPeak peak)
        {
            int returnIndex = Array.BinarySearch(feature.SmoothRTPeaks.ToArray(), peak);
            if (returnIndex < 0)
            {
                returnIndex = ~returnIndex;
            }
            return returnIndex;
        }

        public Tuple<double, double> GetApexTimeWindow(Feature feature, int startIndex)
        {
            double threshold = feature.MaxPeak.Intensity * 0.99;
            double start = 0;
            double finish = 0;
            for (int i = startIndex - 1; i >= 0; i--)
            {
                start = feature.SmoothRTPeaks[i].RT;
                if (feature.SmoothRTPeaks[i].Intensity <= threshold)
                {
                    break;
                }
            }
            for (int i = startIndex + 1; i < feature.SmoothRTPeaks.Count; i++)
            {
                finish = feature.SmoothRTPeaks[i].RT;
                if (feature.SmoothRTPeaks[i].Intensity <= threshold)
                {
                    break;
                }
            }
            return new Tuple<double, double>(start, finish);
        }

        public List<FeatureGroup> GetFinalFeatures(List<FeatureGroup> groups)
        {
            List<FeatureGroup> returnList = new List<FeatureGroup>();
            groups = groups.OrderBy(x => x.ApexTime).ToList();
            for (int i = 0; i < groups.Count - 1; i++)
            {
                double difference = groups[i + 1].ApexTime - groups[i].ApexTime;
                if (difference < 0.001)
                {
                    //Need to find the next value where time difference is > 0.005
                    bool needToSearch = true;
                    double startTime = groups[i].ApexTime;
                    List<FeatureGroup> possibleGroups = new List<FeatureGroup>();
                    possibleGroups.Add(groups[i]);
                    possibleGroups.Add(groups[i + 1]);
                    i++;
                    if (i + 1 < groups.Count && Math.Abs(groups[i + 1].ApexTime - startTime) > 0.001)
                    {
                        needToSearch = false;
                    }

                    while (needToSearch && i < groups.Count)
                    {
                        if (Math.Abs(groups[i].ApexTime - startTime) < 0.001)
                        {
                            possibleGroups.Add(groups[i]);
                            i++;
                        }
                        else
                        {
                            needToSearch = false;
                            i--;
                        }
                    }
                    possibleGroups = possibleGroups.OrderByDescending(x => x.TotalIntensity).ToList();
                    if (possibleGroups[0].allFeatures.Count > 4) //changed these values to remove 8 peak restriction
                    {
                        returnList.Add(possibleGroups[0]);
                    }
                }
                else
                {
                    if (groups[i].allFeatures.Count > 1)
                    {
                        returnList.Add(groups[i]);
                    }
                }
            }
            return returnList;
        }

        public List<MZPeak> GetPeaksFromNearestSpectrum(FeatureGroup group, ThermoRawFile rawFile)
        {
            int scanNum = rawFile.GetSpectrumNumber(group.ApexTime);
            List<MZPeak> returnPeaks = new List<MZPeak>();
            ThermoSpectrum spectrum = rawFile.GetLabeledSpectrum(scanNum);
            HashSet<double> peakMZs = new HashSet<double>();
            foreach (Feature feature in group.allFeatures)
            {
                MZPeak outPeak = spectrum.GetClosestPeak(MassRange.FromPPM(feature.AverageMZ, 10));
                if (outPeak != null)
                {
                    if (!peakMZs.Contains(outPeak.Intensity))
                    {
                        returnPeaks.Add(new MZPeak(feature.AverageMZ, outPeak.Intensity));
                        peakMZs.Add(outPeak.Intensity);
                    }
                }
            }
            returnPeaks = returnPeaks.OrderBy(x => x.MZ).ToList();
            return returnPeaks;
        }
        private static List<EISpectrum> CleanUpSpectra(List<EISpectrum> spectra)
        {
            List<EISpectrum> returnList = new List<EISpectrum>();
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

        public static void AddFeatureGroupsToDatabase(List<EISpectrum> spectra, SQLiteConnection conn)
        {
            using (var transaction = conn.BeginTransaction())
            {
                int count = 1;
                foreach (var spec in spectra)
                {
                    var group = spec.FeatureGroup;
                    //(GroupID INT, NumPeaks INT, ApexRT DOUBLE, IncludedFeatures TEXT)
                    var insertText = "INSERT INTO featureGroupTable (GroupID, NumPeaks, ApexRT, IncludedFeatures, PeakList)"
                                     + " VALUES (@GroupID, @NumPeaks, @ApexRT, @IncludedFeatures, @PeakList)";
                    var insertCommand = new SQLiteCommand(insertText, conn);
                    insertCommand.Parameters.AddWithValue("@GroupID", count);
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
                    count++;
                    insertCommand.Dispose();
                }
                transaction.Commit();
                transaction.Dispose();
            }
        }

        public void PushFeaturesToDatabase(ThermoRawFile rawFile, List<Feature> allFeatures, SQLiteConnection conn)
        {
            //AddRawFileEntry(rawFile, conn);
            using (var transaction = conn.BeginTransaction())
            {
                //if (!ContainsRawFileFeatures(rawFile, conn))
                {
                    int id = GetRawFileID(rawFile, conn);
                    int count = 0;
                    int onePercentCount = (int)(0.01 * allFeatures.Count);
                    onePercentCount++;
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
                        var insertText = "INSERT INTO featureTable (ID, Name, mz, ApexRT, ApexIntensity, ID_Number, SmoothFeatureString, RawFeatureString)"
                            + " VALUES (@ID, @Name, @mz, @ApexRT, @ApexIntensity, @ID_Number, @SmoothFeatureString, @RawFeatureString)";
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
                        insertCommand.Dispose();
                        //if (count % onePercentCount == 0)
                        //{
                        //    OnProgressUpdate(.01);
                        //}
                    }
                }
                transaction.Commit();
                transaction.Dispose();
                // OnFinish();
            }
        }

        public int GetRawFileID(ThermoRawFile raw, SQLiteConnection conn)
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
            reader.Close();
            reader.Dispose();
            queryCommand.Dispose();
            return id;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            disposed = true;
        }
    }
}
