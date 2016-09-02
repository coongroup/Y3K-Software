using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using MathNet.Numerics.Statistics;

namespace Y3K_GC_Quant
{
    class DataNormalizationMethods
    {
        public static void ProcessData(SQLiteConnection conn)
        {
            Dictionary<int, Batch> batchDict = GetBatchDictionary(conn);
            PopulateReplicateQuantDict(batchDict, conn);
            //NEW
            var refBatch = GetMedianReferenceExperiment(batchDict);
            foreach (var batch in batchDict.Values)
            {
                foreach (var rep in batch.replicates)
                {
                    //NormalizeRepToReferenceExperiment(refBatch, rep);
                    NormalizeRepToRefExperimentLinear(refBatch, rep);
                }
            }
            GetAveragedIntensityValues(batchDict);
            GetBatchLessControlValues(batchDict);
            SQLiteIOMethods.AddBarChartInfoToDatabase(batchDict, conn);
            SQLiteIOMethods.AddBatchQuantDataToDatabase(batchDict, conn);
            SQLiteIOMethods.AddBatchBatchCorrelationsToDatabase(batchDict, conn);
            //OLD
            //GetAveragedIntensityValues(batchDict);
            //GetBatchLessControlValues(batchDict);
            //GetInternalStandards(batchDict, conn);
            //List<InternalStandard> standards = new List<InternalStandard>();
            //foreach (var batch in batchDict.Values)
            //{
            //    foreach (var rep in batch.replicates)
            //    {
            //        foreach (var standard in rep.internalStandards)
            //        {
            //            standards.Add(standard);
            //        }
            //    }
            //}
            //SQLiteIOMethods.AddInternalStandardsToDatabase(standards, conn);
            //SQLiteIOMethods.AddBatchQuantDataToDatabase(batchDict, conn);
        }

        public static Dictionary<int, Batch> GetBatchDictionary(SQLiteConnection conn)
        {
            Dictionary<int, Batch> batchDict = new Dictionary<int, Batch>();
            var queryText = "SELECT s.Name, s.Batch_ID FROM Batch_Table s";
            var command = new SQLiteCommand(queryText, conn);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var name = reader["Name"].ToString();
                var id = int.Parse(reader["Batch_ID"].ToString());
                var newBatch = new Batch();
                newBatch.batchID = id;
                newBatch.name = name;
                batchDict.Add(id, newBatch);
            }
            reader.Close();

            queryText = "SELECT s.Name, s.Replicate_ID, s.GCFeatPath, s.BatchName, s.Batch_ID, s.ControlBatchName, s.ControlBatch_ID FROM Replicate_Table s";
            command = new SQLiteCommand(queryText, conn);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                var name = reader["Name"].ToString();
                var id = int.Parse(reader["Replicate_ID"].ToString());
                var path = reader["GCFeatPath"].ToString();
                var batchName = reader["BatchName"].ToString();
                var batchID = int.Parse(reader["Batch_ID"].ToString());
                var controlName = reader["ControlBatchName"].ToString();
                var controlID = reader["ControlBatch_ID"].ToString();
                var newRep = new Replicate();
                newRep.batchID = batchID;
                newRep.batchName = batchName;
                newRep.gcFeatFilePath = path;
                newRep.name = name;
                newRep.replicateID = id;
                var currBatch = batchDict[batchID];
                currBatch.replicates.Add(newRep);
                if (!string.IsNullOrEmpty(controlID))
                {
                    var control_ID = int.Parse(controlID);
                    //var currBatch = batchDict[batchID];
                    //currBatch.replicates.Add(newRep);
                    newRep.controlName = controlName;
                    newRep.control = batchDict[control_ID];
                    currBatch.control = batchDict[control_ID];
                    currBatch.controlID = control_ID;
                }
            }
            return batchDict;
        }

        public static void PopulateReplicateQuantDict(Dictionary<int, Batch> batchDict, SQLiteConnection conn)
        {
            foreach (var batch in batchDict.Values)
            {
                foreach (var rep in batch.replicates)
                {
                    var queryText = "SELECT s.GCMasterGroup_ID, s.QuantFeature_ID, s.ApexRT, s.QuantFeatureMZ, s.RTOffset, s.ApexIntensity FROM ReplicateQuant_Table s WHERE s.Replicate_ID=@ID";
                    var query = new SQLiteCommand(queryText, conn);
                    query.Parameters.AddWithValue("@ID", rep.replicateID);
                    var reader = query.ExecuteReader();
                    while (reader.Read())
                    {
                        var quantFeatureString = reader["QuantFeature_ID"].ToString();
                        if (!string.IsNullOrEmpty(quantFeatureString))
                        {
                            var quantFeatureID = int.Parse(quantFeatureString);
                            var gcGroupId = int.Parse(reader["GCMasterGroup_ID"].ToString());
                            var apexRT = double.Parse(reader["ApexRT"].ToString());
                            var quantFeatureMZ = double.Parse(reader["QuantFeatureMZ"].ToString());
                            var offset = double.Parse(reader["RTOffset"].ToString());
                            var apexIntensity = double.Parse(reader["ApexIntensity"].ToString());
                            var groupID = int.Parse(reader["GCMasterGroup_ID"].ToString());
                            //var name = reader["Name"].ToString();

                            var quantPt = new QuantPoint();
                            quantPt.ApexIntensity = apexIntensity;
                            quantPt.ApexIntensity_Normalized = apexIntensity;
                            quantPt.ApexRT = apexRT;
                            quantPt.BatchID = batch.batchID;
                            quantPt.ControlID = batch.controlID;
                            quantPt.FeatureID = quantFeatureID;
                            //quantPt.Name = name;
                            quantPt.MZ = quantFeatureMZ;
                            quantPt.ReplicateID = rep.replicateID;
                            rep.quantDictionary.Add(groupID, quantPt);
                        }
                    }
                }
            }
        }

        public static void GetAveragedIntensityValues(Dictionary<int, Batch> batchDict)
        {
            foreach (var batch in batchDict.Values)
            {
                HashSet<int> keys = new HashSet<int>();
                foreach (var rep in batch.replicates)
                {
                    foreach (var key in rep.quantDictionary.Keys)
                    {
                        keys.Add(key);
                    }
                }
                foreach (var key in keys)
                {
                    var avgQuantPt = new AvgQuantPoint();
                    avgQuantPt.GCMasterGroupID = key;
                    foreach (var rep in batch.replicates)
                    {
                        if (rep.quantDictionary.ContainsKey(key))
                        {
                            avgQuantPt.AllIntensities.Add(rep.quantDictionary[key].ApexIntensity);
                            avgQuantPt.BatchID = batch.batchID;
                            avgQuantPt.ControlID = batch.controlID;
                            avgQuantPt.GCMasterGroup_ApexRT = rep.quantDictionary[key].ApexRT;
                            avgQuantPt.Name = rep.quantDictionary[key].Name;
                            avgQuantPt.ChEBI_ID = rep.quantDictionary[key].ChEBI_ID;
                            avgQuantPt.PreferredName = "";
                            if (!string.IsNullOrEmpty(avgQuantPt.Name))
                            {
                                avgQuantPt.PreferredName = avgQuantPt.Name + " | ";
                            }
                            if (avgQuantPt.ChEBI_ID != 0)
                            {
                                avgQuantPt.PreferredName += "ChEBI ID: " + avgQuantPt.ChEBI_ID + " | ";
                            }
                            avgQuantPt.PreferredName += "Apex RT: " + Math.Round(avgQuantPt.GCMasterGroup_ApexRT, 4);
                        }
                    }
                    avgQuantPt.AvgIntensity = avgQuantPt.AllIntensities.Average();
                    avgQuantPt.AvgIntensity_StdDev = avgQuantPt.AllIntensities.StandardDeviation();
                    avgQuantPt.AvgIntensity_Normalized_StdDev = avgQuantPt.AllIntensities.StandardDeviation();
                    avgQuantPt.AvgIntensity_Normalized = avgQuantPt.AllIntensities.Average();
                    batch.avgQuantDict.Add(key, avgQuantPt);
                }
            }
        }

        public static void GetBatchLessControlValues(Dictionary<int, Batch> batchDict)
        {
            foreach (var batch in batchDict.Values)
            {
                if (batch.control != null)
                {
                    var controlBatch = batch.control;
                    List<int> validKeys = controlBatch.avgQuantDict.Keys.Where(x => batch.avgQuantDict.ContainsKey(x)).ToList();
                    foreach (var key in validKeys)
                    {
                        var avgValLessControl = batch.avgQuantDict[key].AvgIntensity - controlBatch.avgQuantDict[key].AvgIntensity;
                        batch.avgQuantDict[key].AvgIntensity_LessControl = avgValLessControl;
                        // batch.avgQuantDict[key].AvgIntensity_LessControl_PValue = Statistics.GetWelchsTTestPValue(batch.avgQuantDict[key].AllIntensities, controlBatch.avgQuantDict[key].AllIntensities);
                        var pVal = Statistics.GetWelchsTTestPValue(batch.avgQuantDict[key].AllIntensities, controlBatch.avgQuantDict[key].AllIntensities);
                        pVal = Math.Pow(10, -pVal);
                        batch.avgQuantDict[key].AvgIntensity_LessControl_PValue = pVal;
                        batch.avgQuantDict[key].AvgIntensity_Normalized_LessControl = avgValLessControl;
                        batch.avgQuantDict[key].AvgIntensity_Normalized_LessControl_PValue = pVal;
                        batch.avgQuantDict[key].AvgIntensity_Normalized_StdDev = batch.avgQuantDict[key].AllIntensities.StandardDeviation();
                    }
                }
            }
        }

        public static void GetInternalStandards(Dictionary<int, Batch> batchDict, SQLiteConnection conn)
        {
            List<int> returnList = new List<int>();
            var queryText = "SELECT s.GroupID FROM GCMaster_FeatureGroup_Table s WHERE s.IsInternalStandard=1";
            var queryCommand = new SQLiteCommand(queryText, conn);
            var reader = queryCommand.ExecuteReader();
            while (reader.Read())
            {
                var id = int.Parse(reader["GroupID"].ToString());
                returnList.Add(id);
            }
            List<double> internalStandardIntensities = new List<double>();
            foreach (var id in returnList)
            {
                //go find that id in each replicate and make a new internal standard, also add the intensity to a list
                foreach (var batch in batchDict.Values)
                {
                    foreach (var rep in batch.replicates)
                    {
                        if (rep.quantDictionary.ContainsKey(id))
                        {
                            var internalStandardFeature = rep.quantDictionary[id];
                            internalStandardIntensities.Add(internalStandardFeature.ApexIntensity);
                            var newInternalStandard = new InternalStandard();
                            newInternalStandard.ApexIntensity = rep.quantDictionary[id].ApexIntensity;
                            newInternalStandard.ApexRT = internalStandardFeature.ApexRT;
                            newInternalStandard.GCMasterGroup_ID = id;
                            newInternalStandard.ReplicateName = rep.name;
                            newInternalStandard.Feature_ID = internalStandardFeature.FeatureID;
                            rep.internalStandards.Add(newInternalStandard);
                        }
                    }
                }
                internalStandardIntensities = internalStandardIntensities.OrderBy(x => x).ToList();
                var median = internalStandardIntensities.Median();
                foreach (var batch in batchDict.Values)
                {
                    foreach (var rep in batch.replicates)
                    {
                        foreach (var internalStandard in rep.internalStandards)
                        {
                            if (internalStandard.GCMasterGroup_ID == id)
                            {
                                internalStandard.CorrectionFactor = median / internalStandard.ApexIntensity;
                            }
                        }
                    }
                }
            }

            foreach (var batch in batchDict.Values)
            {
                foreach (var rep in batch.replicates)
                {
                    foreach (var key in rep.quantDictionary.Keys)
                    {
                        var qPt = rep.quantDictionary[key];
                        var nearestStandard = GetClosestInternalStandard(qPt, rep.internalStandards);
                        if (nearestStandard != null)
                        {
                            qPt.ApexIntensity_Normalized = qPt.ApexIntensity * nearestStandard.CorrectionFactor;
                        }
                        else
                        {

                        }
                    }
                }
            }

            foreach (var batch in batchDict.Values)
            {
                HashSet<int> keys = new HashSet<int>();
                foreach (var rep in batch.replicates)
                {
                    foreach (var key in rep.quantDictionary.Keys)
                    {
                        keys.Add(key);
                    }
                }
                foreach (var key in keys)
                {
                    var currAvgPt = batch.avgQuantDict[key];
                    List<double> normalizedPts = new List<double>();
                    foreach (var rep in batch.replicates)
                    {
                        if (rep.quantDictionary.ContainsKey(key))
                        {
                            var normalizedPt = rep.quantDictionary[key].ApexIntensity_Normalized;
                            if (normalizedPt != 0)
                            {
                                normalizedPts.Add(normalizedPt);
                            }
                        }
                    }
                    if (normalizedPts.Count > 0)
                    {
                        currAvgPt.Allintensities_Normalized = normalizedPts;
                        currAvgPt.AvgIntensity_Normalized = normalizedPts.Average();
                        currAvgPt.AvgIntensity_Normalized_StdDev = normalizedPts.StandardDeviation();
                    }
                }
            }

            foreach (var batch in batchDict.Values)
            {
                if (batch.control != null)
                {
                    var controlBatch = batch.control;
                    List<int> validKeys = controlBatch.avgQuantDict.Keys.Where(x => batch.avgQuantDict.ContainsKey(x)).ToList();
                    foreach (var key in validKeys)
                    {
                        var avgValLessControl = batch.avgQuantDict[key].AvgIntensity_Normalized - controlBatch.avgQuantDict[key].AvgIntensity_Normalized;
                        batch.avgQuantDict[key].AvgIntensity_Normalized_LessControl = avgValLessControl;
                        // batch.avgQuantDict[key].AvgIntensity_LessControl_PValue = Statistics.GetWelchsTTestPValue(batch.avgQuantDict[key].AllIntensities, controlBatch.avgQuantDict[key].AllIntensities);
                        var pVal = Statistics.GetWelchsTTestPValue(batch.avgQuantDict[key].Allintensities_Normalized, controlBatch.avgQuantDict[key].Allintensities_Normalized);
                        pVal = Math.Pow(10, -pVal);
                        batch.avgQuantDict[key].AvgIntensity_Normalized_LessControl_PValue = pVal;
                    }
                }
            }
        }

        public static InternalStandard GetClosestInternalStandard(QuantPoint qPt, List<InternalStandard> internalStandards)
        {
            if (internalStandards.Count == 0)
            {
                return null;
            }
            if (internalStandards.Count == 1)
            {
                return internalStandards.First();
            }
            if (internalStandards.Count > 1)
            {
                double timeDiff = double.MaxValue;
                InternalStandard holdStandard = null;
                foreach (var standard in internalStandards)
                {
                    var currDiff = Math.Abs(standard.ApexRT - qPt.ApexRT);
                    if (currDiff < timeDiff)
                    {
                        timeDiff = currDiff;
                        holdStandard = standard;
                    }
                }
                return holdStandard;
            }
            return null;
        }

        public static Batch GetMedianReferenceExperiment(Dictionary<int, Batch> batchDictionary)
        {
            Batch returnBatch = new Batch();
            HashSet<int> allIds = new HashSet<int>();
            foreach (var batch in batchDictionary.Values)
            {
                foreach (var rep in batch.replicates)
                {
                    foreach (var key in rep.quantDictionary.Keys)
                    {
                        allIds.Add(key);
                    }
                }
            }
            foreach (var key in allIds.ToList())
            {
                List<double> allMeasurements = new List<double>();
                foreach (var batch in batchDictionary.Values)
                {
                    foreach (var rep in batch.replicates)
                    {
                        if (rep.quantDictionary.ContainsKey(key))
                        {
                            allMeasurements.Add(rep.quantDictionary[key].ApexIntensity_Normalized);
                        }
                    }
                }
                var avgIntensity = allMeasurements.Median();
                var avgPoint = new AvgQuantPoint();
                avgPoint.AvgIntensity = avgIntensity;
                returnBatch.avgQuantDict.Add(key, avgPoint);
            }
            return returnBatch;
        }

        public static void NormalizeRepToReferenceExperiment(Batch referenceBatch, Replicate rep)
        {
            List<Tuple<double, double, int>> bothVals = new List<Tuple<double, double, int>>();
            foreach (var key in rep.quantDictionary.Keys)
            {
                var lfqOne = referenceBatch.avgQuantDict[key].AvgIntensity;
                var lfqTwo = rep.quantDictionary[key].ApexIntensity_Normalized;

                //calculate m
                var m = (lfqOne / lfqTwo);
                m = Math.Log(m, 2);

                //calculate a
                var a = (lfqTwo * lfqOne);
                a = Math.Log(a, 2);
                a /= 2;
                bothVals.Add(new Tuple<double, double, int>(a, m, key));
            }

            List<double> xVals = new List<double>();
            List<double> yVals = new List<double>();
            var li = new LowessInterpolator(.4, 0);
            bothVals = bothVals.OrderBy(t => t.Item1).ToList();
            foreach (var val in bothVals)
            {
                xVals.Add(val.Item1);
                yVals.Add(val.Item2);
            }
            var smoothed = li.smooth(xVals.ToArray(), yVals.ToArray());
            for (int q = 0; q < smoothed.Length; q++)
            {
                var residual = smoothed[q];
                var key = bothVals[q].Item3;
                var lfqOne = referenceBatch.avgQuantDict[key].AvgIntensity;
                var lfqTwo = rep.quantDictionary[key].ApexIntensity_Normalized;

                //calculate m
                var m = (lfqOne / lfqTwo);
                m = Math.Log(m, 2);

                //calculate a
                var a = (lfqTwo * lfqOne);
                a = Math.Log(a, 2);
                a /= 2;
                var corrFactor = -residual;
                //repAQuantDict[key].NormalizedIntensity = Math.Pow(2, (corrFactor + (2 * a)) / 2);
                rep.quantDictionary[key].ApexIntensity_Normalized = Math.Pow(2, -(corrFactor - (2 * a)) / 2);
                rep.quantDictionary[key].ApexIntensity = Math.Pow(2, -(corrFactor - (2 * a)) / 2);
            }
        }

        public static void NormalizeRepToRefExperimentLinear(Batch referenceBatch, Replicate rep)
        {
            List<Tuple<double, double, int>> bothVals = new List<Tuple<double, double, int>>();
            foreach (var key in rep.quantDictionary.Keys)
            {
                var lfqOne = referenceBatch.avgQuantDict[key].AvgIntensity;
                var lfqTwo = rep.quantDictionary[key].ApexIntensity_Normalized;

                //calculate m
                var m = (lfqOne / lfqTwo);
                m = Math.Log(m, 2);

                //calculate a
                var a = (lfqTwo * lfqOne);
                a = Math.Log(a, 2);
                a /= 2;
                bothVals.Add(new Tuple<double, double, int>(a, m, key));
            }

            List<double> xVals = new List<double>();
            List<double> yVals = new List<double>();
            foreach (var val in bothVals)
            {
                xVals.Add(val.Item1);
                yVals.Add(val.Item2);
            }

            double slope;
            double yIntercept;
            double rSquared;

            Statistics.LinearRegression(xVals.ToArray(), yVals.ToArray(), 0, xVals.Count(), out rSquared, out yIntercept, out slope);
            foreach (var key in rep.quantDictionary.Keys)
            {
                var lfqOne = referenceBatch.avgQuantDict[key].AvgIntensity;
                var lfqTwo = rep.quantDictionary[key].ApexIntensity_Normalized;

                //calculate m
                var m = (lfqOne / lfqTwo);
                m = Math.Log(m, 2);

                //calculate a
                var a = (lfqTwo * lfqOne);
                a = Math.Log(a, 2);
                a /= 2;


                //get predicted mi-star from 
                var mistar = (a * slope) + yIntercept;
                var corrFactor = m - mistar;

                rep.quantDictionary[key].ApexIntensity_Normalized = Math.Pow(2, -(corrFactor - (2 * a)) / 2);
                rep.quantDictionary[key].ApexIntensity = Math.Pow(2, -(corrFactor - (2 * a)) / 2);
            }
        }
    }
}
