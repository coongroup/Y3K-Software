using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Y3K_GC_Quant
{
    public class QuantMethods
    {
        public Master Master;
        public SQLiteConnection QuantDbConnection;
        public List<RTPeak> ClippedMasterTicChroma;
        public List<RTPeak> ClippedReplicateTicChroma;
        public Replicate CurrentReplicate;
        public SQLiteConnection ReplicateDBConnection;
        public double CurrentTICStartTime;
        public double CurrentTICStopTime;

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
            GC.Collect();
            var handler = Finished;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public QuantMethods(Replicate replicate, Master master, SQLiteConnection quantDbConnection)
        {
            CurrentReplicate = replicate;
            Master = master;
            ReplicateDBConnection = new SQLiteConnection(@"Data Source=" + replicate.gcFeatFilePath);
            ReplicateDBConnection.Open();
            QuantDbConnection = quantDbConnection;
        }

        public void Quantify()
        {
            List<EISpectrum> curatedSpectra = Master.allSpectra.Where(x => x.isValid).ToList();
            List<RTPeak> replicateTIC = SQLiteIOMethods.GetTicChromaFromGCFeat(ReplicateDBConnection);
            ClippedMasterTicChroma = RemoveSolventFrontFromTIC(Master.chroma);
            ClippedReplicateTicChroma = RemoveSolventFrontFromTIC(replicateTIC, replicateTIC.First().RT);
            int completed = 0;
            using (var transaction = QuantDbConnection.BeginTransaction())
            {
                foreach (var currentSpectrum in curatedSpectra)
                {
                    if (!SQLiteIOMethods.IsQuantified(CurrentReplicate.replicateID, QuantDbConnection))
                    {
                        double apexTime = currentSpectrum.ApexTimeEI;
                        double startTime = apexTime - .15;
                        double stopTime = apexTime + .15;
                        GetValidTICSegment(startTime, stopTime);
                        CurrentTICStartTime = startTime;
                        CurrentTICStopTime = stopTime;
                        var ticMaster =
                            ClippedMasterTicChroma.Where(x => x.RT >= CurrentTICStartTime && x.RT <= CurrentTICStopTime)
                                .ToList();
                        var ticReplicate =
                            ClippedReplicateTicChroma.Where(x => x.RT >= CurrentTICStartTime && x.RT <= CurrentTICStopTime)
                                .ToList();
                        var ticMasterArray = GetExpandedChromatogram(ticMaster, CurrentTICStartTime, CurrentTICStopTime);
                        var ticReplicateArray = GetExpandedChromatogram(ticReplicate, CurrentTICStartTime,
                            CurrentTICStopTime);
                        double offset = GetAdjustedChromatogramOffset(ticMasterArray, ticReplicateArray);
                        DoubleRange quantRange = new DoubleRange(currentSpectrum.ApexTimeEI - offset - .15,
                            currentSpectrum.ApexTimeEI - offset + .15);
                        List<Feature> replicateRangeFeatures = SQLiteIOMethods.GetFeatures(quantRange.Minimum,
                            quantRange.Maximum, ReplicateDBConnection);
                        List<Feature> currSpecQuantFeatures = new List<Feature>();
                        foreach (var ion in currentSpectrum.quantIons)
                        {
                            var ppmRange = DoubleRange.FromPPM(ion.MZ, 5);
                            currSpecQuantFeatures.AddRange(replicateRangeFeatures.Where(x => ppmRange.Contains(x.AverageMZ)));
                        }
                        foreach (var ion in currentSpectrum.quantIons)
                        {
                            var ppmRange = DoubleRange.FromPPM(ion.MZ, 5);
                            Feature quantFeature = null;
                            double timeDifference = double.MaxValue;
                            foreach (var feature in currSpecQuantFeatures)
                            {
                                if (ppmRange.Contains(feature.AverageMZ))
                                {
                                    var currTimeDiff = Math.Abs(feature.apexTime - currentSpectrum.ApexTimeEI - offset);
                                    if (currTimeDiff < timeDifference)
                                    {
                                        timeDifference = currTimeDiff;
                                        quantFeature = feature;
                                    }
                                }
                            }
                            if (quantFeature != null)
                            {
                                if (CurrentReplicate.control != null)
                                {
                                    SQLiteIOMethods.AddReplicateQuantPointToDatabase(CurrentReplicate.replicateID, CurrentReplicate.name, CurrentReplicate.batchID, CurrentReplicate.control.batchID,
                                        currentSpectrum.FeatureGroup.groupID, offset, quantFeature, QuantDbConnection);
                                }
                                else
                                {
                                    SQLiteIOMethods.AddReplicateQuantPointToDatabase(CurrentReplicate.replicateID, CurrentReplicate.name, CurrentReplicate.batchID, 0, currentSpectrum.FeatureGroup.groupID,
                                        offset, quantFeature, QuantDbConnection);
                                }
                            }
                            else
                            {
                                if (CurrentReplicate.control != null)
                                {
                                    SQLiteIOMethods.AddNullReplicateQuantPointToDatabase(CurrentReplicate.replicateID, CurrentReplicate.name, CurrentReplicate.batchID, CurrentReplicate.control.batchID,
                                        currentSpectrum.FeatureGroup.groupID, offset, QuantDbConnection);
                                }
                                else
                                {
                                    SQLiteIOMethods.AddNullReplicateQuantPointToDatabase(CurrentReplicate.replicateID, CurrentReplicate.name, CurrentReplicate.batchID, 0,
                                        currentSpectrum.FeatureGroup.groupID, offset, QuantDbConnection);
                                }
                            }
                        }
                        completed++;
                        double percent = ((double)completed) / ((double)curatedSpectra.Count) * 200;
                        OnProgressUpdate(percent);
                    }
                }
                SQLiteIOMethods.SetReplicateToQuantified(CurrentReplicate.replicateID, QuantDbConnection);
                transaction.Commit();
                transaction.Dispose();
            }
            OnFinish();
        }

        public List<RTPeak> RemoveSolventFrontFromTIC(List<RTPeak> ticChroma, double firstScanTime)
        {
            //added this line for the sake of MJR
            return ticChroma;

            //assume the solvent front comes off and ends within the first four minutes of the run
            var timeRange = new DoubleRange(firstScanTime, firstScanTime + 4);

            //rank order all tic points by intensity
            ticChroma = ticChroma.OrderByDescending(x => x.Intensity).ToList();

            //find first tic point that occurs within that first two minutes and store the peak
            RTPeak holdPeak = null;
            foreach (var peak in ticChroma)
            {
                if (timeRange.Contains(peak.RT))
                {
                    holdPeak = peak;
                    break;
                }
            }

            //reorder the tic chromatogram by time
            ticChroma = ticChroma.OrderBy(x => x.RT).ToList();

            //binary search your way to the solvent front peak you just found
            int index = ticChroma.BinarySearch(holdPeak);
            int nextIndex = index;

            //start riding the curve to the right (increased time)
            for (int i = index; i < ticChroma.Count; i++)
            {
                var currPeak = ticChroma[i];
                //once you are at .2 intensity as what you were at the apex start clocking
                if (currPeak.Intensity < holdPeak.Intensity * .2)
                {
                    nextIndex = i;
                    break;
                }
            }

            //keep riding the curve until you hit an point where the intensity increases from the previous point
            RTPeak prevPeak = ticChroma[nextIndex];
            int lastIndex = nextIndex;
            for (int i = nextIndex; i < ticChroma.Count; i++)
            {
                var currPeak = ticChroma[i];
                if (currPeak.Intensity > prevPeak.Intensity)
                {
                    lastIndex = i;
                    break;
                }
                else
                {
                    prevPeak = currPeak;
                }
            }

            //every point after that should be free of the solvent front.
            ticChroma.RemoveRange(0, lastIndex);
            return ticChroma;
        }
        public List<RTPeak> RemoveSolventFrontFromTIC(Chromatogram chroma)
        {
            double firstTime = chroma.First().Time;
            List<RTPeak> nativeTic = new List<RTPeak>();
            foreach (var peak in chroma)
            {
                nativeTic.Add(new RTPeak(0, peak.Intensity, peak.Time));
            }
            return RemoveSolventFrontFromTIC(nativeTic, firstTime);
        }
        public void GetValidTICSegment(double startTime, double stopTime)
        {
            List<RTPeak> outList = new List<RTPeak>();
            outList = ClippedMasterTicChroma.Where(x => x.RT <= stopTime && x.RT >= startTime).ToList();

            outList = outList.OrderByDescending(x => x.Intensity).ToList();
            var max = outList[0].Intensity;
            var threshold = max * .1;
            outList = outList.OrderBy(x => x.RT).ToList();

            var startList = outList.Where(x => x.RT <= startTime + .05).ToList();
            var stopList = outList.Where(x => x.RT >= stopTime - .05).ToList();

            double newStartTime = startTime;
            double newStopTime = stopTime;
            if (!SatistfiesThreshold(startList, threshold))
            {
                newStartTime -= .01;
                if (newStartTime < ClippedMasterTicChroma.First().RT)
                {
                    newStartTime = startTime;
                }
            }
            if (!SatistfiesThreshold(stopList, threshold))
            {
                newStopTime += .01;
                if (newStopTime > ClippedMasterTicChroma.Last().RT)
                {
                    newStopTime = stopTime;
                }
            }

            if (Math.Round(newStartTime, 5) != Math.Round(startTime, 5) || Math.Round(newStopTime, 5) != Math.Round(stopTime, 5))
            {
                GetValidTICSegment(newStartTime, newStopTime);
            }
            else
            {
                CurrentTICStartTime = newStartTime;
                CurrentTICStopTime = newStopTime;
                return;
            }
        }
        public bool SatistfiesThreshold(List<RTPeak> peaks, double threshold)
        {
            peaks = peaks.OrderByDescending(x => x.Intensity).ToList();
            if (peaks.First().Intensity > threshold)
            {
                return false;
            }
            return true;
        }
        private double[,] GetExpandedChromatogram(List<RTPeak> peaks, double minTime, double maxTime, int numPoints = 1000)
        {
            double[,] newChroma = new double[numPoints, 2];
            double increment = (maxTime - minTime) / numPoints;
            int oldIndex = 0;

            for (int i = 0; i < numPoints; i++)
            {
                double currTime = (increment * i) + minTime;
                double oldTime = peaks[oldIndex].RT;
                while (currTime > oldTime)
                {
                    oldIndex++;
                    if (oldIndex == peaks.Count)
                    {
                        oldIndex--;
                        break;
                    }
                    oldTime = peaks[oldIndex].RT; ;
                }
                if (oldIndex == 0)
                {
                    newChroma[i, 0] = currTime;
                    newChroma[i, 1] = SolveForY(minTime, 0, peaks[0].RT, peaks[0].Intensity, currTime);
                }
                else if (oldIndex == peaks.Count - 1)
                {
                    newChroma[i, 0] = currTime;
                    newChroma[i, 1] = SolveForY(peaks[oldIndex].RT, peaks[oldIndex].Intensity, maxTime, 0, currTime);
                }
                else
                {
                    newChroma[i, 0] = currTime;
                    newChroma[i, 1] = SolveForY(peaks[oldIndex - 1].RT, peaks[oldIndex - 1].Intensity, peaks[oldIndex].RT, peaks[oldIndex].Intensity, currTime);
                }
            }
            return newChroma;
        }
        private double SolveForY(double x1, double y1, double x2, double y2, double xOfInterest)
        {
            double m = ((y2 - y1) / (x2 - x1));
            double b = y2 - (m * x2);
            return ((m * xOfInterest) + b);
        }
        private double GetDenominator(double[,] raw)
        {
            double denom = 0;
            for (int i = 0; i < raw.Length / 2; i++)
            {
                denom += (1 * raw[i, 1]);
            }
            return denom;
        }
        private double GetAdjustedChromatogramOffset(double[,] repChroma, double[,] compChroma)
        {
            double maxProd = 0;
            int bestOffset = 0;
            for (int i = -100; i <= 100; i++)
            {
                double currProd = 0;
                for (int j = 0; j < compChroma.Length / 2; j++)
                {
                    int compIndex = j + i;
                    if (compIndex < 0 || compIndex >= compChroma.Length / 2)
                    {
                        continue;
                    }
                    currProd += (compChroma[compIndex, 1] * repChroma[j, 1]);
                }
                if (currProd > maxProd)
                {
                    maxProd = currProd;
                    bestOffset = i;
                }
            }
            return (bestOffset * (compChroma[2, 0] - compChroma[1, 0]));
        }
    }
}
