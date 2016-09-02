using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;
using System.Threading;

namespace Y3K_Deconvolution_Studio
{
    public partial class Form1 : Form
    {
        public string analyteGCFeatFile;
        public string backgroundGCFeatFile;
        public string analyteGCMasterFile;
        public string backgroundGCMasterFile;
        public Master noiseMaster;
        public Master analyteMaster;
        public LineItem highlightedCurateFeature;
        public Feature highlightedCurateFeatureObject;
        public EISpectrum currentCurateGroup;
        public List<Feature> currentCurateFeatures;
        public int currentCurateGroupIndex;
        public int currentCurateBackgroudIndex;
        public List<EISpectrum> currentAlignBackgroundFeatureGroups;
        public List<LineItem> highlightedAlignBackgroundFeatureGroup;
        public int currentAlignGroupBackgroundIndex;
        public LineItem highlightedQuantIonPeak;
        public LineItem highlightedQuantIonExtractedFeature;
        public LineItem highlightedQuantIonRawFeature;
        public Dictionary<MZPeak, List<RTPeak>> quantIonRawFeatureDict;
        public ThermoRawFile quantIonRawFile;

        public List<Feature> TimeRangeFeatures;
        public Feature MatchingMZFeature;
        public static int lastFeatureGroupID;

        public static int currentAlignmentGridRow = -1;

        public Form1()
        {
            InitializeComponent();
            MakeTextBoxesGray();
            zedGraphControl5.GraphPane.BarSettings.Type = BarType.Overlay;
            zedGraphControl8.GraphPane.BarSettings.Type = BarType.Overlay;
            ClearHashMarks(zedGraphControl2);
            ClearHashMarks(zedGraphControl3);
            ClearHashMarks(zedGraphControl4);
            ClearHashMarks(zedGraphControl5);
            ClearHashMarks(zedGraphControl6);
            ClearHashMarks(zedGraphControl7);
            zedGraphControl2.GraphPane.Title.Text = "Selected Spectrum Feature Group XICs";
            zedGraphControl2.GraphPane.XAxis.Title.Text = "Retention Time (Minutes)";
            zedGraphControl2.GraphPane.YAxis.Title.Text = "Intensity";
            zedGraphControl3.GraphPane.Title.Text = "Selected Spectrum";
            zedGraphControl3.GraphPane.XAxis.Title.Text = "M/Z";
            zedGraphControl3.GraphPane.YAxis.Title.Text = "Intensity";
            zedGraphControl4.GraphPane.Title.Text = "Analyte XICs";
            zedGraphControl4.GraphPane.XAxis.Title.Text = "Retention Time (Minutes)";
            zedGraphControl4.GraphPane.YAxis.Title.Text = "Intensity";
            zedGraphControl5.GraphPane.Title.Text = "Analyte v. Background Spectral Overlap";
            zedGraphControl5.GraphPane.XAxis.Title.Text = "M/Z";
            zedGraphControl5.GraphPane.YAxis.Title.Text = "Intensity";
            zedGraphControl6.GraphPane.Title.Text = "Background XICs";
            zedGraphControl6.GraphPane.XAxis.Title.Text = "Retention Time (Minutes)";
            zedGraphControl6.GraphPane.YAxis.Title.Text = "Intensity";
            zedGraphControl7.GraphPane.Title.Text = "Analyte v. Background XIC Overlap";
            zedGraphControl7.GraphPane.XAxis.Title.Text = "Retention Time (Minutes)";
            zedGraphControl7.GraphPane.YAxis.Title.Text = "Intensity";
            zedGraphControl8.GraphPane.Title.Text = "Extracted Spectrum";
            zedGraphControl8.GraphPane.XAxis.Title.Text = "M/Z";
            zedGraphControl8.GraphPane.YAxis.Title.Text = "Intensity";
            zedGraphControl9.GraphPane.Title.Text = "Extracted Features";
            zedGraphControl9.GraphPane.XAxis.Title.Text = "Retention Time (Minutes)";
            zedGraphControl9.GraphPane.YAxis.Title.Text = "Intensity";
            zedGraphControl10.GraphPane.Title.Text = "Raw Features";
            zedGraphControl10.GraphPane.XAxis.Title.Text = "Retention Time (Minutes)";
            zedGraphControl10.GraphPane.YAxis.Title.Text = "Intensity";
            tabControl1.TabPages[1].Enabled = false;
            tabControl1.TabPages[2].Enabled = false;
            tabControl1.TabPages[3].Enabled = false;
        }

        public void ClearHashMarks(ZedGraphControl control)
        {
            control.GraphPane.XAxis.MajorTic.IsInside = false;
            control.GraphPane.YAxis.MajorTic.IsInside = false;
            control.GraphPane.XAxis.MinorTic.IsInside = false;
            control.GraphPane.YAxis.MinorTic.IsInside = false;
            zedGraphControl5.GraphPane.BarSettings.Type = BarType.Overlay;
        }

        public void MakeTextBoxesGray()
        {
            loadAnalyteTextBox.ForeColor = Color.Gray;
            loadBackgroundTextBox.ForeColor = Color.Gray;
            createAnalyteTextBox.ForeColor = Color.Gray;
            createBackgroundTextBox.ForeColor = Color.Gray;
            quantRawTextBox.ForeColor = Color.Gray;
        }
        private void browseCreateAnalyteButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Extracted GC Feature Files (*.gcfeat) | *.gcfeat";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                createAnalyteTextBox.Text = openFileDialog1.FileName;
                createAnalyteTextBox.ForeColor = Color.Black;
                analyteGCFeatFile = createAnalyteTextBox.Text;
            }
        }
        private void browseCreateBackgroundButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Extracted GC Feature Files (*.gcfeat) | *.gcfeat";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                createBackgroundTextBox.Text = openFileDialog1.FileName;
                createBackgroundTextBox.ForeColor = Color.Black;
                backgroundGCFeatFile = createAnalyteTextBox.Text;
            }
        }

        private void createMasterButton_Click(object sender, EventArgs e)
        {
            foreach (Control control in this.Controls)
            {
                control.Enabled = false;
            }
            var newThread = new Thread(() => CreateMaster());
            newThread.Start();
        }
        private void CreateMaster()
        {
            UpdateText("Loading GC Feature Data...");
            var noisePath = createBackgroundTextBox.Text;
            var analytePath = createAnalyteTextBox.Text;
            CreateMaster cm = new CreateMaster(noisePath, analytePath, out noiseMaster, out analyteMaster);
            UpdateText("Creating GC Master Files...");
            noiseMaster.WriteGCMasterFile();
            analyteMaster.WriteGCMasterFile();
            EnableAfterMasterCreate();
        }
        public void EnableAfterMasterCreate()
        {
            Invoke(new Action(() =>
            {
                this.Text = "Deconvolution Studio - Master Files Created";
                ZedgraphMethods.PlotReflection(analyteMaster.chroma, noiseMaster.chroma, zedGraphControl1, false);
                this.Enabled = true;
                foreach (Control control in this.Controls)
                {
                    control.Enabled = true;
                }
            }));
        }
        private void UpdateText(string text)
        {
            Invoke(new Action(() =>
            {
                this.Text = text;
            }));
        }

        private void browseLoadAnalyteButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "GC Master Files (*.gcmast) | *.gcmast";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                loadAnalyteTextBox.ForeColor = Color.Black;
                loadAnalyteTextBox.Text = openFileDialog1.FileName;
                analyteGCMasterFile = loadAnalyteTextBox.Text;
            }
        }
        private void browseLoadBackgroundButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "GC Master Files (*.gcmast | *.gcmast";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                loadBackgroundTextBox.ForeColor = Color.Black;
                loadBackgroundTextBox.Text = openFileDialog1.FileName;
                backgroundGCMasterFile = loadBackgroundTextBox.Text;
            }
        }

        private void loadMasterButton_Click(object sender, EventArgs e)
        {
            foreach (Control control in this.Controls)
            {
                control.Enabled = false;
            }
            Thread newThread = new Thread(() => LoadMaster());
            newThread.Start();
        }

        private void LoadMaster()
        {
            UpdateText("Loading Analyte File...");
            analyteMaster = new Master(analyteGCMasterFile);
            analyteMaster.ReadInMaster();
            analyteMaster.allSpectra = analyteMaster.allSpectra.OrderBy(x => x.ApexTimeEI).ToList();
            UpdateText("Loading Background File...");
            noiseMaster = new Master(backgroundGCMasterFile);
            noiseMaster.ReadInMaster();
            EnableAfterMasterLoad();
        }

        private void EnableAfterMasterLoad()
        {
            Invoke(new Action(() =>
            {
                this.Text = "Deconvolution Studio - Data Loaded";
                this.Enabled = true;
                foreach (Control control in this.Controls)
                {
                    control.Enabled = true;
                }
                tabControl1.TabPages[1].Enabled = true;
                tabControl1.TabPages[2].Enabled = true;
                tabControl1.TabPages[3].Enabled = true;
                ZedgraphMethods.PlotReflection(analyteMaster.chroma, noiseMaster.chroma, zedGraphControl1);
                groupCurateDropDown.SelectedIndex = 0;
                PopulateEISpectraLists();
            }));
        }

        private void PopulateEISpectraLists()
        {
            ZedgraphMethods.ClearZedGraph(zedGraphControl2);
            ZedgraphMethods.ClearZedGraph(zedGraphControl3);
            highlightedCurateFeature = null;
            highlightedCurateFeatureObject = null;
            ZedgraphMethods.UpdateZedGraph(zedGraphControl2);
            ZedgraphMethods.UpdateZedGraph(zedGraphControl3);
            curateListBox.Items.Clear();
            quantSelectionGroupList.Items.Clear();
            alignmentGridView.Rows.Clear();
            if (groupCurateDropDown.SelectedItem.ToString().Equals("Analyte"))
            {
                foreach (var spec in analyteMaster.allSpectra)
                {
                    if (spec.isValid)
                    {
                        curateListBox.Items.Add(spec);
                    }
                }
            }
            else
            {
                foreach (var spec in noiseMaster.allSpectra)
                {
                    if (spec.isValid)
                    {
                        curateListBox.Items.Add(spec);
                    }
                }
            }
            foreach (var spec in analyteMaster.allSpectra)
            {
                if (spec.isValid)
                {
                    quantSelectionGroupList.Items.Add(spec);
                }
            }
            int index = 0;
            foreach (var spec in analyteMaster.allSpectra)
            {
                alignmentGridView.Rows.Add(spec.ToString());
                if (spec.isValid)
                {
                    alignmentGridView.Rows[index].Cells[1].Value = true;
                }
                else
                {
                    alignmentGridView.Rows[index].Cells[1].Value = false;
                }
                index++;
            }
            alignmentGridView.CurrentCell = null;
            if (currentAlignmentGridRow != -1)
            {
                alignmentGridView.CurrentCell = alignmentGridView[0, currentAlignmentGridRow];
                alignmentGridView_SelectionChanged(null, null);
            }
        }

        private void groupCurateDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            ZedgraphMethods.ClearZedGraph(zedGraphControl2);
            ZedgraphMethods.ClearZedGraph(zedGraphControl3);
            PopulateEISpectraLists();
        }

        private void curateListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //UPDATE THIS SO THAT IT HANDLES BOTH THE ANALYTE AND THE NOISE!!!!!!!!! RELEVANT FEATURES
            if (curateListBox.SelectedItem != null)
            {
                MatchingMZFeature = null;
                TimeRangeFeatures = new List<Feature>();
                newGroupButton.Enabled = false;
                highlightedCurateFeatureObject = null;
                highlightedCurateFeature = null;
                currentCurateGroupIndex = 0;
                currentCurateBackgroudIndex = 0;
                currentAlignGroupBackgroundIndex = 0;
                var currSpec = (EISpectrum)curateListBox.SelectedItem;
                ZedgraphMethods zm = new ZedgraphMethods();
                ZedgraphMethods.ClearZedgraph(zedGraphControl2);
                ZedgraphMethods.ClearZedgraph(zedGraphControl3);
                List<Feature> relevantFeatures =
                    analyteMaster.allFeatures.Where(x => x.ApexTime > currSpec.ApexTimeEI - .05
                                                         && x.ApexTime < currSpec.ApexTimeEI + .05).ToList();
                relevantFeatures = relevantFeatures.OrderBy(x => x.totalIntensity).ToList();
                currentCurateFeatures = new List<Feature>();
                currentCurateFeatures.AddRange(relevantFeatures);
                currentCurateGroup = currSpec;
                zm.PlotXICs(currSpec.FeatureGroup.allFeatures, zedGraphControl2, Color.Red);
                zm.PlotXICs(relevantFeatures, zedGraphControl2, Color.LightGray);
                zedGraphControl2.GraphPane.XAxis.Scale.Min = currSpec.ApexTimeEI - .05;
                zedGraphControl2.GraphPane.XAxis.Scale.Max = currSpec.ApexTimeEI + .05;
                zm.PlotSpectrum(zedGraphControl3, currSpec.FinalEIPeaks, Color.Red);
                curateButton.Enabled = false;
                curateButton.Text = "";

                quantSelectionGroupList.SelectedItem = currSpec;
                var index = analyteMaster.allSpectra.IndexOf(currSpec);
                alignmentGridView.CurrentCell = alignmentGridView.Rows[index].Cells[0];

                ZedgraphMethods.UpdateZedgraph(zedGraphControl2);
                ZedgraphMethods.UpdateZedgraph(zedGraphControl3);
            }
        }

        private void alignmentGridView_SelectionChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            //grab the current feature group from the alignment window
            if (alignmentGridView.CurrentCell != null && alignmentGridView.SelectedCells != null && alignmentGridView.Rows.Count > 0)
            {
                if (alignmentGridView.SelectedCells.Count > 0)
                {
                    if (alignmentGridView.SelectedCells[0].ColumnIndex == 0)
                    {
                        //clear zedgraph windows
                        var zm = new ZedgraphMethods();
                        ZedgraphMethods.ClearZedgraph(zedGraphControl4);
                        ZedgraphMethods.ClearZedgraph(zedGraphControl5);
                        ZedgraphMethods.ClearZedgraph(zedGraphControl7);
                        ZedgraphMethods.ClearZedgraph(zedGraphControl6);

                        int row = alignmentGridView.SelectedCells[0].RowIndex;
                        var currSpec = analyteMaster.allSpectra[row];
                        if (!string.IsNullOrEmpty(currSpec.UserName))
                        {
                            zedGraphControl4.GraphPane.Title.Text = currSpec.UserName + " XICs";
                        }
                        else
                        {
                            zedGraphControl4.GraphPane.Title.Text = "Analyte XICs";
                        }

                        List<Feature> relevantFeatures =
                            analyteMaster.allFeatures.Where(x => x.ApexTime > currSpec.ApexTimeEI - .05
                                                                 && x.ApexTime < currSpec.ApexTimeEI + .05 &&
                                                                 !currSpec.featureHashes.Contains(x.curatorLine))
                                .ToList();
                        relevantFeatures = relevantFeatures.OrderBy(x => x.totalIntensity).ToList();

                        currSpec.FinalEIPeaks = currSpec.FinalEIPeaks.OrderByDescending(x => x.Intensity).ToList();
                        double threshold = currSpec.FinalEIPeaks[0].Intensity * .05;
                        if (!optimizedDrawCheck.Checked)
                        {
                            threshold = 0;
                        }

                        //if isValid make red, else make dark gray
                        double maxIntensity = 0;
                        if (currSpec.isValid)
                        {
                            foreach (var feat in currSpec.FeatureGroup.allFeatures)
                            {
                                double currMax = 0;
                                foreach (var peak in feat.smoothRTPeaks)
                                {
                                    if (peak.Intensity > maxIntensity)
                                    {
                                        maxIntensity = peak.Intensity;
                                    }
                                    if (peak.Intensity > currMax)
                                    {
                                        currMax = peak.Intensity;
                                    }
                                }
                                if (currMax > threshold)
                                {
                                    zm.PlotXICs(feat.smoothRTPeaks, zedGraphControl4, Color.Red);
                                }
                            }
                        }
                        else
                        {
                            foreach (var feat in currSpec.FeatureGroup.allFeatures)
                            {
                                double currMax = 0;
                                foreach (var peak in feat.smoothRTPeaks)
                                {
                                    if (peak.Intensity > maxIntensity)
                                    {
                                        maxIntensity = peak.Intensity;
                                    }
                                    if (peak.Intensity > currMax)
                                    {
                                        currMax = peak.Intensity;
                                    }
                                }
                                if (currMax > threshold)
                                {
                                    zm.PlotXICs(feat.smoothRTPeaks, zedGraphControl4, Color.DimGray);
                                }
                            }
                        }

                        //plot background features from analyte as well
                        foreach (var feat in relevantFeatures)
                        {
                            zm.PlotXICs(feat.smoothRTPeaks, zedGraphControl4, Color.LightGray, threshold);
                        }
                        PlotCorrespondingBackgroundFeatures();
                        if (currSpec.isInternalStandard)
                        {
                            internalStandardCheck.Checked = true;
                        }
                        else
                        {
                            internalStandardCheck.Checked = false;
                        }
                        zedGraphControl4.GraphPane.XAxis.Scale.Min = currSpec.ApexTimeEI - .075;
                        zedGraphControl4.GraphPane.XAxis.Scale.Max = currSpec.ApexTimeEI + .075;
                        zedGraphControl4.GraphPane.YAxis.Scale.Max = maxIntensity * 1.25;

                        curateListBox.SelectedItem = currSpec;
                        quantSelectionGroupList.SelectedItem = currSpec;

                        userNameTextBox.Text = currSpec.UserName;
                        chebiIDBox.Text = currSpec.chebiID;

                        ZedgraphMethods.UpdateZedgraph(zedGraphControl4);
                        ZedgraphMethods.UpdateZedgraph(zedGraphControl5);
                        ZedgraphMethods.UpdateZedgraph(zedGraphControl6);
                        ZedgraphMethods.UpdateZedgraph(zedGraphControl7);
                    }
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void PlotCorrespondingBackgroundFeatures()
        {
            ZedgraphMethods.ClearZedGraph(zedGraphControl6);
            ZedgraphMethods.ClearZedGraph(zedGraphControl7);
            int row = alignmentGridView.SelectedCells[0].RowIndex;
            var currSpec = analyteMaster.allSpectra[row];
            List<EISpectrum> backgroundSpectra = noiseMaster.allSpectra.Where(x => x.ApexTimeEI < currSpec.ApexTimeEI + 0.1
                && x.ApexTimeEI > currSpec.ApexTimeEI - .1 && x.isValid).ToList();

            currentAlignBackgroundFeatureGroups = new List<EISpectrum>();
            currentAlignBackgroundFeatureGroups.AddRange(backgroundSpectra);

            List<Feature> relevantFeatures = analyteMaster.allFeatures.Where(x => x.ApexTime > currSpec.ApexTimeEI - .05
                    && x.ApexTime < currSpec.ApexTimeEI + .05 && !currSpec.featureHashes.Contains(x.curatorLine)).ToList();
            relevantFeatures = relevantFeatures.OrderBy(x => x.totalIntensity).ToList();
            double threshold = currSpec.FinalEIPeaks[0].Intensity * .05;
            if (!optimizedDrawCheck.Checked)
            {
                threshold = 0;
            }

            //highlight the closest spectrum
            var highlightedGroup = GetClosestTimeSpectrum(currSpec, backgroundSpectra);
            double maxIntensity = 0;
            if (highlightedGroup != null)
            {
                List<LineItem> xics = new List<LineItem>();
                {
                    foreach (var feat in highlightedGroup.FeatureGroup.allFeatures)
                    {
                        var ptList = new PointPairList();
                        double currMax = 0;
                        foreach (var peak in feat.smoothRTPeaks)
                        {
                            ptList.Add(new PointPair(peak.RT, peak.Intensity));
                            if (peak.Intensity > maxIntensity)
                            {
                                maxIntensity = peak.Intensity;
                            }
                            if (peak.Intensity > currMax)
                            {
                                currMax = peak.Intensity;
                            }
                        }
                        if (currMax > threshold)
                        {
                            var line = new LineItem("", ptList, Color.Blue, SymbolType.None);
                            line.Tag = feat.ID_Number;
                            xics.Add(line);
                        }
                    }
                }
                highlightedAlignBackgroundFeatureGroup = new List<LineItem>();
                highlightedAlignBackgroundFeatureGroup.AddRange(xics);
                foreach (var group in backgroundSpectra)
                {
                    foreach (var feat in group.FeatureGroup.allFeatures)
                    {
                        var ptList = new PointPairList();
                        double currMax = 0;
                        foreach (var peak in feat.smoothRTPeaks)
                        {
                            ptList.Add(new PointPair(peak.RT, peak.Intensity));
                            if (peak.Intensity > currMax)
                            {
                                currMax = peak.Intensity;
                            }
                        }
                        if (currMax > threshold)
                        {
                            var line = new LineItem("", ptList, Color.LightGray, SymbolType.None);
                            line.Tag = feat.ID_Number;
                            zedGraphControl6.GraphPane.CurveList.Add(line);
                        }
                    }
                }
                foreach (var line in xics)
                {
                    zedGraphControl6.GraphPane.CurveList.Add(line);
                    zedGraphControl6.GraphPane.CurveList.Move(zedGraphControl6.GraphPane.CurveList.Count - 1, -9999);
                }
                var zm = new ZedgraphMethods();
                zm.PlotSpectralOverlap(zedGraphControl5, currSpec.FinalNormalizedEIPeaks, highlightedGroup.FinalNormalizedEIPeaks, Color.Red, Color.Blue);
                zm.PlotTICOverlap(zedGraphControl7, currSpec.FeatureGroup, highlightedGroup.FeatureGroup, relevantFeatures,
                       currentAlignBackgroundFeatureGroups, Color.Red, Color.Blue, currSpec.ApexTimeEI, threshold);

                zedGraphControl6.GraphPane.XAxis.Scale.Min = highlightedGroup.ApexTimeEI - .075;
                zedGraphControl6.GraphPane.XAxis.Scale.Max = highlightedGroup.ApexTimeEI + .075;
                if (maxIntensity != 0)
                {
                    zedGraphControl6.GraphPane.YAxis.Scale.Max = maxIntensity * 1.25;
                }

                ZedgraphMethods.UpdateZedgraph(zedGraphControl7);
                ZedgraphMethods.UpdateZedgraph(zedGraphControl6);
            }
        }

        public EISpectrum GetClosestTimeSpectrum(EISpectrum analyte, List<EISpectrum> background)
        {
            EISpectrum returnSpec = null;
            double diff = double.MaxValue;
            foreach (var spec in background)
            {
                double currDiff = Math.Abs(spec.ApexTimeEI - analyte.ApexTimeEI);
                if (currDiff < diff)
                {
                    diff = currDiff;
                    returnSpec = spec;
                }
            }
            return returnSpec;
        }

        private void alignmentGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (alignmentGridView.SelectedCells[0] != null && alignmentGridView.SelectedCells[0].ColumnIndex == 1)
            {
                currentAlignGroupBackgroundIndex = 0;
                alignmentGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
                int row = alignmentGridView.SelectedCells[0].RowIndex;
                analyteMaster.allSpectra[row].isValid = bool.Parse(alignmentGridView.SelectedCells[0].Value.ToString());
                alignmentGridView.Rows[row].Cells[0].Selected = true;
                currentAlignmentGridRow = row;
                PopulateEISpectraLists();
            }
        }

        private void zedGraphControl2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (curateListBox.SelectedItem != null)
            {
                CurveItem outCurve = null;
                int index;
                zedGraphControl2.GraphPane.FindNearestPoint(new PointF(e.X, e.Y), zedGraphControl2.GraphPane.CurveList,
                    out outCurve, out index);
                var currentSpectrum = (EISpectrum)curateListBox.SelectedItem;
                if (outCurve != null)
                {
                    var selected = new LineItem(outCurve as LineItem);
                    int tag = int.Parse(selected.Tag.ToString());

                    if (highlightedCurateFeature != null)
                    {
                        zedGraphControl2.GraphPane.CurveList.Remove(highlightedCurateFeature);
                    }
                    //var selectedLine = outCurve as LineItem;
                    zedGraphControl2.GraphPane.CurveList.Add(selected);
                    highlightedCurateFeature = selected;
                    selected.Line.Width = 3;
                    highlightedCurateFeatureObject = null;
                    if (groupCurateDropDown.SelectedItem.Equals("Analyte"))
                    {
                        highlightedCurateFeatureObject =
                            analyteMaster.allFeatures.Where(x => x.ID_Number == tag).ToList()[0];
                    }
                    else
                    {
                        highlightedCurateFeatureObject =
                            noiseMaster.allFeatures.Where(x => x.ID_Number == tag).ToList()[0];
                    }
                    if (currentSpectrum.FeatureGroup.includedFeatureIDs.Contains(tag))
                    {
                        curateButton.Text = "Exclude";
                        curateButton.Enabled = true;
                        selected.Color = Color.LimeGreen;
                    }
                    else
                    {
                        curateButton.Text = "Include";
                        curateButton.Enabled = true;
                        selected.Color = Color.DodgerBlue;
                    }

                    var currFeature = analyteMaster.allFeatures.Where(x => x.ID_Number == tag).ToList();
                    dataGridView1.Rows.Clear();
                    string[] array = new string[] { currFeature[0].averageMZ.ToString(), currFeature[0].ApexTime.ToString() };
                    dataGridView1.Rows.Add(array);
                    dataGridView1.ClearSelection();

                    zedGraphControl2.GraphPane.CurveList.Move(zedGraphControl2.GraphPane.CurveList.Count - 1, -9999);
                    ZedgraphMethods.UpdateZedGraph(zedGraphControl2);
                }
            }
            else
            {
                if (zedGraphControl2.GraphPane.CurveList.Count > 0)
                {
                    CurveItem outCurve = null;
                    int index;
                    zedGraphControl2.GraphPane.FindNearestPoint(new PointF(e.X, e.Y), zedGraphControl2.GraphPane.CurveList,
                        out outCurve, out index);
                    // var currentSpectrum = (EISpectrum)curateListBox.SelectedItem;
                    if (outCurve != null)
                    {
                        var selected = new LineItem(outCurve as LineItem);
                        int tag = int.Parse(selected.Tag.ToString());

                        if (highlightedCurateFeature != null)
                        {
                            zedGraphControl2.GraphPane.CurveList.Remove(highlightedCurateFeature);
                        }
                        //var selectedLine = outCurve as LineItem;
                        zedGraphControl2.GraphPane.CurveList.Add(selected);
                        highlightedCurateFeature = selected;
                        selected.Line.Width = 3;
                        highlightedCurateFeatureObject = null;
                        if (groupCurateDropDown.SelectedItem.Equals("Analyte"))
                        {
                            highlightedCurateFeatureObject =
                                analyteMaster.allFeatures.Where(x => x.ID_Number == tag).ToList()[0];
                        }
                        else
                        {
                            highlightedCurateFeatureObject =
                                noiseMaster.allFeatures.Where(x => x.ID_Number == tag).ToList()[0];
                        }
                        //if (currentSpectrum.FeatureGroup.includedFeatureIDs.Contains(tag))
                        //{
                        //    curateButton.Text = "Exclude";
                        //    curateButton.Enabled = true;
                        //    selected.Color = Color.LimeGreen;
                        //}
                        //else
                        {
                            // curateButton.Text = "Include";
                            //curateButton.Enabled = true;
                            selected.Color = Color.DodgerBlue;
                        }

                        var currFeature = analyteMaster.allFeatures[tag];
                        dataGridView1.Rows.Clear();
                        string[] array = new string[] { currFeature.averageMZ.ToString(), currFeature.ApexTime.ToString() };
                        dataGridView1.Rows.Add(array);
                        dataGridView1.ClearSelection();

                        zedGraphControl2.GraphPane.CurveList.Move(zedGraphControl2.GraphPane.CurveList.Count - 1, -9999);
                        ZedgraphMethods.UpdateZedGraph(zedGraphControl2);
                    }
                }
            }
        }

        private void curateButton_Click(object sender, EventArgs e)
        {
            var currSpec = (EISpectrum)curateListBox.SelectedItem;
            var norm = new Normalization();
            if (curateButton.Text.Equals("Include"))
            {
                currSpec.FeatureGroup.allFeatures.Add(highlightedCurateFeatureObject);
                currSpec.FeatureGroup.includedFeatureIDs.Add(highlightedCurateFeatureObject.ID_Number);
                var newPeak = new MZPeak(highlightedCurateFeatureObject.averageMZ, highlightedCurateFeatureObject.maxIntensity);
                currSpec.FinalEIPeaks.Add(newPeak);
                //currSpec.FinalNormalizedEIPeaks.Add(newPeak); //Here you should renormalize the high-res peaks
                currSpec.FinalNormalizedEIPeaks.Clear();
                currSpec.FinalNormalizedEIPeaks.AddRange(currSpec.FinalEIPeaks);
                currSpec.FinalNormalizedEIPeaks = currSpec.FinalNormalizedEIPeaks.OrderBy(x => x.MZ).ToList();
                norm.CombineLikeMZPeaks(currSpec.FinalNormalizedEIPeaks);
                currSpec.isDirty = true;
            }
            else
            {
                currSpec.FeatureGroup.allFeatures.Remove(highlightedCurateFeatureObject);
                currSpec.FeatureGroup.includedFeatureIDs.Remove(highlightedCurateFeatureObject.ID_Number);
                List<MZPeak> finalPeaks =
                    currSpec.FinalEIPeaks.Where(x => Math.Round(x.MZ, 3) == Math.Round(highlightedCurateFeatureObject.averageMZ, 3)).ToList();
                currSpec.FinalEIPeaks.Remove(finalPeaks[0]);
                // currSpec.FinalNormalizedEIPeaks.Remove(finalPeaks[0]);
                currSpec.FinalNormalizedEIPeaks.Clear();
                currSpec.FinalNormalizedEIPeaks.AddRange(currSpec.FinalEIPeaks);
                currSpec.FinalNormalizedEIPeaks = currSpec.FinalNormalizedEIPeaks.OrderBy(x => x.MZ).ToList();
                norm.CombineLikeMZPeaks(currSpec.FinalNormalizedEIPeaks);
                currSpec.isDirty = true;
            }
            curateListBox_SelectedIndexChanged(sender, e);
        }

        private void internalStandardCheck_CheckedChanged(object sender, EventArgs e)
        {
            int row = alignmentGridView.SelectedCells[0].RowIndex;
            var currSpec = analyteMaster.allSpectra[row];
            if (internalStandardCheck.Checked)
            {
                currSpec.isDirty = true;
                currSpec.isInternalStandard = true;
                currSpec.isValid = true;
            }
            else
            {
                currSpec.isDirty = true;
                currSpec.isInternalStandard = false;
                currSpec.isValid = false; //perhaps you need to update this to reflect the initial determination
            }
            alignmentGridView_SelectionChanged(sender, e);
        }

        private void userNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                e.Handled = e.SuppressKeyPress = true;
                int row = alignmentGridView.SelectedCells[0].RowIndex;
                var currSpec = analyteMaster.allSpectra[row];
                currSpec.UserName = userNameTextBox.Text;
                currSpec.isDirty = true;
                currSpec.isValid = true;
                zedGraphControl4.GraphPane.Title.Text = currSpec.UserName + " XICs";
                ZedgraphMethods.UpdateZedGraph(zedGraphControl4);
                alignmentGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
                var checkBox = alignmentGridView.Rows[row].Cells[1] as DataGridViewCheckBoxCell;
                checkBox.Value = true;
                PopulateEISpectraLists();
                alignmentGridView_SelectionChanged(sender, e);
            }
        }

        private void zedGraphControl7_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (currentAlignBackgroundFeatureGroups.Count > 0)
            {
                int row = alignmentGridView.SelectedCells[0].RowIndex;
                var currSpec = analyteMaster.allSpectra[row];
                CurveItem outCurve = null;
                int index;
                zedGraphControl7.GraphPane.FindNearestPoint(new PointF(e.X, e.Y), zedGraphControl7.GraphPane.CurveList,
                    out outCurve, out index);
                if (outCurve != null)
                {
                    var selected = outCurve as LineItem;
                    var tag = int.Parse(selected.Tag.ToString());
                    EISpectrum matchingSpectrum = null;
                    foreach (var spec in currentAlignBackgroundFeatureGroups)
                    {
                        if (spec.FeatureGroup.includedFeatureIDs.Contains(tag))
                        {
                            matchingSpectrum = spec;
                        }
                    }
                    foreach (var line in zedGraphControl7.GraphPane.CurveList)
                    {
                        var tmpLine = line as LineItem;
                        if (tmpLine.Color != Color.Red)
                        {
                            tmpLine.Line.Width = 1;
                            tmpLine.Line.Color = Color.LightGray;
                        }
                    }
                    foreach (var line in zedGraphControl6.GraphPane.CurveList)
                    {
                        var tmpLine = line as LineItem;
                        if (tmpLine.Color != Color.Red)
                        {
                            tmpLine.Line.Width = 1;
                            tmpLine.Line.Color = Color.LightGray;
                        }
                    }
                    if (matchingSpectrum != null)
                    {
                        ZedgraphMethods.ClearZedgraph(zedGraphControl5);
                        List<CurveItem> curves =
                            zedGraphControl7.GraphPane.CurveList.Where(
                                x =>
                                    matchingSpectrum.FeatureGroup.includedFeatureIDs.Contains(int.Parse(x.Tag.ToString()))
                                    && x.Points[0].Y < 0)
                                .ToList();
                        foreach (var curve in curves)
                        {
                            var tmpCurve = curve as LineItem;
                            tmpCurve.Line.Color = Color.Blue;
                            zedGraphControl7.GraphPane.CurveList.Move(
                            zedGraphControl7.GraphPane.CurveList.IndexOf(curve), -9999);
                        }
                        var zm = new ZedgraphMethods();
                        zm.PlotSpectralOverlap(zedGraphControl5, currSpec.FinalNormalizedEIPeaks, matchingSpectrum.FinalNormalizedEIPeaks, Color.Red, Color.Blue);

                        curves =
                            zedGraphControl6.GraphPane.CurveList.Where(
                                x =>
                                    matchingSpectrum.FeatureGroup.includedFeatureIDs.Contains(int.Parse(x.Tag.ToString())))
                                .ToList();
                        foreach (var curve in curves)
                        {
                            var tmpCurve = curve as LineItem;
                            tmpCurve.Color = Color.Blue;
                            zedGraphControl6.GraphPane.CurveList.Move(
                                zedGraphControl6.GraphPane.CurveList.IndexOf(curve), -9999);
                        }
                    }
                }
                ZedgraphMethods.UpdateZedGraph(zedGraphControl5);
                ZedgraphMethods.UpdateZedGraph(zedGraphControl6);
                ZedgraphMethods.UpdateZedGraph(zedGraphControl7);
            }
            Cursor.Current = Cursors.Default;
        }

        private void quantSelectionGroupList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var zm = new ZedgraphMethods();
            ZedgraphMethods.ClearZedgraph(zedGraphControl8);
            ZedgraphMethods.ClearZedgraph(zedGraphControl9);
            ZedgraphMethods.ClearZedgraph(zedGraphControl10);
            quantAllIonList.Items.Clear();
            quantSelectedIonList.Items.Clear();
            highlightedQuantIonPeak = null;
            highlightedQuantIonExtractedFeature = null;
            highlightedQuantIonRawFeature = null;
            var currSpec = (EISpectrum)quantSelectionGroupList.SelectedItem;
            foreach (var peak in currSpec.FinalEIPeaks)
            {
                if (!currSpec.quantIons.Contains(peak))
                {
                    quantAllIonList.Items.Add(peak);
                }
                else
                {
                    quantSelectedIonList.Items.Add(peak);
                }
            }
            zm.PlotSpectrum(zedGraphControl8, currSpec.quantIons, Color.Red);
            zm.PlotSpectrum(zedGraphControl8, currSpec.FinalEIPeaks, Color.FromArgb(89, Color.DodgerBlue));
            List<Feature> xics = new List<Feature>();
            foreach (var peak in currSpec.quantIons)
            {
                foreach (var feat in currSpec.FeatureGroup.allFeatures)
                {
                    if (Math.Round(peak.MZ, 5) == Math.Round(feat.AverageMZ, 5))
                    {
                        xics.Add(feat);
                    }
                }
            }
            int count = 0;
            foreach (var feat in xics)
            {
                zm.PlotXICs(feat.smoothRTPeaks, zedGraphControl9, count);
                count++;
            }

            PlotFullXICs();

            ZedgraphMethods.UpdateZedgraph(zedGraphControl8);
            ZedgraphMethods.UpdateZedgraph(zedGraphControl9);
            ZedgraphMethods.UpdateZedgraph(zedGraphControl10);

            curateListBox.SelectedItem = currSpec;
            var index = analyteMaster.allSpectra.IndexOf(currSpec);
            alignmentGridView.CurrentCell = alignmentGridView.Rows[index].Cells[0];

            Cursor.Current = Cursors.Default;
        }

        private void PlotFullXICs()
        {
            if (quantIonRawFile != null)
            {
                var currSpec = (EISpectrum)quantSelectionGroupList.SelectedItem;
                var startTime = Math.Max(quantIonRawFile.GetRetentionTime(quantIonRawFile.FirstSpectrumNumber), currSpec.ApexTimeEI - .3);
                var stopTime = Math.Min(quantIonRawFile.GetRetentionTime(quantIonRawFile.LastSpectrumNumber), currSpec.ApexTimeEI + .3);
                var startScan = quantIonRawFile.GetSpectrumNumber(startTime);
                var stopScan = quantIonRawFile.GetSpectrumNumber(stopTime);

                List<RTPeak> firstIon = new List<RTPeak>();
                List<RTPeak> secondIon = new List<RTPeak>();
                List<RTPeak> thirdIon = new List<RTPeak>();

                quantIonRawFeatureDict = new Dictionary<MZPeak, List<RTPeak>>();

                var zm = new ZedgraphMethods();

                double apexIntensity = 0;

                currSpec.quantIons = currSpec.quantIons.OrderByDescending(x => x.Intensity).ToList();
                foreach (var ion in currSpec.quantIons)
                {
                    quantIonRawFeatureDict.Add(ion, new List<RTPeak>());
                }

                for (int i = startScan; i <= stopScan; i++)
                {
                    var spec = quantIonRawFile.GetSpectrum(i);
                    var rt = quantIonRawFile.GetRetentionTime(i);
                    List<ThermoMzPeak> outPeaks = new List<ThermoMzPeak>();
                    foreach (var ion in currSpec.quantIons)
                    {
                        outPeaks = new List<ThermoMzPeak>();
                        if (spec.TryGetPeaks(DoubleRange.FromPPM(ion.MZ, 10), out outPeaks))
                        {
                            var newRTPeak = new RTPeak(outPeaks[0], rt);
                            quantIonRawFeatureDict[ion].Add(newRTPeak);
                        }
                        else
                        {
                            var newRTPeak = new RTPeak(ion.MZ, 0, rt);
                            quantIonRawFeatureDict[ion].Add(newRTPeak);
                        }
                    }
                }

                int count = 0;
                foreach (var val in quantIonRawFeatureDict.Values)
                {
                    zm.PlotXICs(val, zedGraphControl10, count);
                    count++;
                }
                ZedgraphMethods.UpdateZedGraph(zedGraphControl10);
            }
        }

        private void quantAllIonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (highlightedQuantIonPeak != null)
            {
                zedGraphControl8.GraphPane.CurveList.Remove(highlightedQuantIonPeak);
                highlightedQuantIonPeak = null;
            }
            var zm = new ZedgraphMethods();
            double xMin = zedGraphControl8.GraphPane.XAxis.Scale.Min;
            double xMax = zedGraphControl8.GraphPane.XAxis.Scale.Max;

            //grab peak to highlight
            var currPeak = (MZPeak)quantAllIonList.SelectedItem;

            //plot big peak and store reference
            List<MZPeak> tmpList = new List<MZPeak>();
            tmpList.Add(currPeak);
            zm.PlotSpectrum(zedGraphControl8, tmpList, Color.FromArgb(200, Color.Blue));
            this.highlightedQuantIonPeak = (LineItem)zedGraphControl8.GraphPane.CurveList.Last();
            //highlightedQuantIonPeak.Bar.Border.IsVisible = true;
            highlightedQuantIonPeak.Line.Width = 3;
            highlightedQuantIonPeak.Line.Color = Color.FromArgb(200, Color.Blue);
            //move to front
            zedGraphControl8.GraphPane.CurveList.Move(zedGraphControl8.GraphPane.CurveList.Count - 1, -999);
            zedGraphControl8.GraphPane.XAxis.Scale.Min = xMin;
            zedGraphControl8.GraphPane.XAxis.Scale.Max = xMax;

            List<CurveItem> ghostCurves =
                zedGraphControl9.GraphPane.CurveList.Where(x => x.Tag != null).ToList();

            foreach (var curve in ghostCurves)
            {
                zedGraphControl9.GraphPane.CurveList.Remove(curve);
            }

            //grab xic and plot ghost version in 
            if (quantSelectionGroupList.SelectedItem != null)
            {
                var currSpec = (EISpectrum)quantSelectionGroupList.SelectedItem;
                var features = currSpec.FeatureGroup.allFeatures.Where(x => x.AverageMZ == currPeak.MZ).ToList();
                if (features.Count > 0)
                {
                    //plot ghost xic
                    var ptList = new PointPairList();
                    foreach (var peak in features[0].smoothRTPeaks)
                    {
                        ptList.Add(peak.RT, peak.Intensity);
                    }
                    var line = new LineItem("", ptList, Color.FromArgb(200, Color.Blue), SymbolType.None);
                    line.Line.Width = 3;
                    line.Tag = "Ghost";
                    zedGraphControl9.GraphPane.CurveList.Add(line);
                }
            }

            //remove ghost xics from zg10
            List<CurveItem> curves = zedGraphControl10.GraphPane.CurveList.Where(x => x.Tag != null).ToList();
            foreach (var curve in curves)
            {
                zedGraphControl10.GraphPane.CurveList.Remove(curve);
            }

            // plot raw ghost xic
            if (quantIonRawFile != null)
            {
                var currSpec = (EISpectrum)quantSelectionGroupList.SelectedItem;
                var startTime = Math.Max(quantIonRawFile.GetRetentionTime(quantIonRawFile.FirstSpectrumNumber),
                    currSpec.ApexTimeEI - .3);
                var stopTime = Math.Min(quantIonRawFile.GetRetentionTime(quantIonRawFile.LastSpectrumNumber),
                    currSpec.ApexTimeEI + .3);
                var startScan = quantIonRawFile.GetSpectrumNumber(startTime);
                var stopScan = quantIonRawFile.GetSpectrumNumber(stopTime);
                List<RTPeak> currPeakList = new List<RTPeak>();
                for (int i = startScan; i <= stopScan; i++)
                {
                    var spec = quantIonRawFile.GetSpectrum(i);
                    var rt = quantIonRawFile.GetRetentionTime(i);
                    List<ThermoMzPeak> outPeaks = new List<ThermoMzPeak>();
                    if (spec.TryGetPeaks(DoubleRange.FromPPM(currPeak.MZ, 10), out outPeaks))
                    {
                        var newRTPeak = new RTPeak(outPeaks[0], rt);
                        currPeakList.Add(newRTPeak);
                    }
                    else
                    {
                        var newRTpeak = new RTPeak(currPeak.MZ, 0, rt);
                        currPeakList.Add(newRTpeak);
                    }
                }
                var ptList = new PointPairList();
                foreach (var peak in currPeakList)
                {
                    ptList.Add(new PointPair(peak.RT, peak.Intensity));
                }
                var rawGhost = new LineItem("", ptList, Color.FromArgb(200, Color.Blue), SymbolType.None);
                rawGhost.Tag = "Ghost";
                rawGhost.Line.Width = 3;
                zedGraphControl10.GraphPane.CurveList.Add(rawGhost);
            }
            ZedgraphMethods.UpdateZedgraph(zedGraphControl8);
            ZedgraphMethods.UpdateZedGraph(zedGraphControl9);
            ZedgraphMethods.UpdateZedGraph(zedGraphControl10);
        }

        private void quantSelectedIonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (quantSelectedIonList.SelectedItem != null)
            {
                if (highlightedQuantIonPeak != null)
                {
                    zedGraphControl8.GraphPane.CurveList.Remove(highlightedQuantIonPeak);
                    highlightedQuantIonPeak = null;
                }
                if (highlightedQuantIonExtractedFeature != null)
                {
                    zedGraphControl9.GraphPane.CurveList.Remove(highlightedQuantIonExtractedFeature);
                    highlightedQuantIonExtractedFeature = null;
                }
                if (highlightedQuantIonRawFeature != null)
                {
                    zedGraphControl10.GraphPane.CurveList.Remove(highlightedQuantIonRawFeature);
                    highlightedQuantIonRawFeature = null;
                }
                var zm = new ZedgraphMethods();
                double xMin = zedGraphControl8.GraphPane.XAxis.Scale.Min;
                double xMax = zedGraphControl8.GraphPane.XAxis.Scale.Max;

                //grab peak to highlight
                var currPeak = (MZPeak)quantSelectedIonList.SelectedItem;

                //grap feature to highlight
                var currSpec = (EISpectrum)quantSelectionGroupList.SelectedItem;
                Feature holdFeature = null;
                foreach (var feat in currSpec.FeatureGroup.allFeatures)
                {
                    if (Math.Round(feat.AverageMZ, 5) == Math.Round(currPeak.MZ, 5))
                    {
                        holdFeature = feat;
                        break;
                    }
                }

                if (zedGraphControl8.GraphPane.CurveList.Count > 0)
                {
                    //plot big peak and store reference
                    List<MZPeak> tmpList = new List<MZPeak>();
                    tmpList.Add(currPeak);
                    zm.PlotSpectrum(zedGraphControl8, tmpList, Color.FromArgb(200, Color.DarkOrchid));
                    this.highlightedQuantIonPeak = (LineItem)zedGraphControl8.GraphPane.CurveList.Last();
                    //highlightedQuantIonPeak.Bar.Border.IsVisible = true;
                    highlightedQuantIonPeak.Line.Width = 3;
                    highlightedQuantIonPeak.Line.Color = Color.FromArgb(200, Color.DarkOrchid);
                    //move to front
                    zedGraphControl8.GraphPane.CurveList.Move(zedGraphControl8.GraphPane.CurveList.Count - 1, -999);
                    zedGraphControl8.GraphPane.XAxis.Scale.Min = xMin;
                    zedGraphControl8.GraphPane.XAxis.Scale.Max = xMax;

                    zm.PlotXICs(holdFeature.smoothRTPeaks, zedGraphControl9);
                    this.highlightedQuantIonExtractedFeature = (LineItem)zedGraphControl9.GraphPane.CurveList.Last();
                    highlightedQuantIonExtractedFeature.Line.Width = 3;
                    highlightedQuantIonExtractedFeature.Line.Color = Color.FromArgb(200, Color.DarkOrchid);
                    zedGraphControl9.GraphPane.CurveList.Move(zedGraphControl9.GraphPane.CurveList.Count - 1, -999);

                    if (quantIonRawFile != null && quantIonRawFeatureDict != null)
                    {
                        var currList = quantIonRawFeatureDict[currPeak];
                        zm.PlotXICs(currList, zedGraphControl10);
                        this.highlightedQuantIonRawFeature = (LineItem)zedGraphControl10.GraphPane.CurveList.Last();
                        highlightedQuantIonRawFeature.Line.Color = Color.FromArgb(200, Color.DarkOrchid);
                        highlightedQuantIonRawFeature.Line.Width = 3;
                        zedGraphControl10.GraphPane.CurveList.Move(zedGraphControl10.GraphPane.CurveList.Count - 1, -999);
                    }

                    ZedgraphMethods.UpdateZedgraph(zedGraphControl9);
                    ZedgraphMethods.UpdateZedgraph(zedGraphControl8);
                    ZedgraphMethods.UpdateZedgraph(zedGraphControl10);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (quantSelectedIonList.SelectedItem != null)
            {
                //move the selected item off of the current list and back to the other list
                var currPeak = (MZPeak)quantSelectedIonList.SelectedItem;
                var currSpec = (EISpectrum)quantSelectionGroupList.SelectedItem;
                if (currSpec.quantIons.Count > 1)
                {
                    currSpec.quantIons.Remove(currPeak);
                    quantSelectedIonList.Items.Remove(currPeak);
                    quantAllIonList.Items.Clear();
                    foreach (var peak in currSpec.FinalEIPeaks)
                    {
                        if (!currSpec.quantIons.Contains(peak))
                        {
                            quantAllIonList.Items.Add(peak);
                        }
                    }
                    quantSelectionGroupList_SelectedIndexChanged(sender, e);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (quantAllIonList.SelectedItem != null)
            {
                var currPeak = (MZPeak)quantAllIonList.SelectedItem;
                var currSpec = (EISpectrum)quantSelectionGroupList.SelectedItem;
                currSpec.quantIons.Add(currPeak);
                quantSelectedIonList.Items.Add(currPeak);
                quantAllIonList.Items.Clear();
                foreach (var peak in currSpec.FinalEIPeaks)
                {
                    if (!currSpec.quantIons.Contains(peak))
                    {
                        quantAllIonList.Items.Add(peak);
                    }
                }
                quantSelectionGroupList_SelectedIndexChanged(sender, e);
            }
        }

        private void quantRawBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Thermo Raw File | *.raw";
            if (openFileDialog1.ShowDialog() == DialogResult.OK) // Test result.
            {
                quantRawTextBox.Text = openFileDialog1.FileName;
                quantRawTextBox.ForeColor = Color.Black;
                quantIonRawFile = new ThermoRawFile(quantRawTextBox.Text);
                quantIonRawFile.Open();
            }
        }

        private void chebiIDBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                e.Handled = e.SuppressKeyPress = true;
                int row = alignmentGridView.SelectedCells[0].RowIndex;
                var currSpec = analyteMaster.allSpectra[row];
                currSpec.chebiID = chebiIDBox.Text;
                currSpec.isDirty = true;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveChangesButton.Enabled = false;
            var newThread = new Thread(() => UpdateAnalyteMaster());
            newThread.Start();
        }

        private void UpdateAnalyteMaster()
        {
            UpdateMaster.UpdateGCMastDatabase(analyteMaster);
            EnableSaveChangesButton();
        }

        private void EnableSaveChangesButton()
        {
            Invoke(new Action(() =>
            {
                saveChangesButton.Enabled = true;
                this.Text = "Most Recent Save: " + DateTime.Now.ToString();
            }));
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (mzLookupBox.Text.Equals(" M/Z") || string.IsNullOrEmpty(mzLookupBox.Text))
            {
                mzLookupBox.ForeColor = Color.Gray;
                if (string.IsNullOrEmpty(mzLookupBox.Text))
                {
                    mzLookupBox.Text = " M/Z";
                }
            }
            else
            {
                mzLookupBox.ForeColor = Color.Black;
            }
        }

        private void mzLookupBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (mzLookupBox.Text.Equals(" M/Z"))
            {
                mzLookupBox.Text = " ";
            }
        }

        private void rtLookupBox_TextChanged(object sender, EventArgs e)
        {
            if (rtLookupBox.Text.Equals(" Retention Time") || string.IsNullOrEmpty(rtLookupBox.Text))
            {
                rtLookupBox.ForeColor = Color.Gray;
                if (string.IsNullOrEmpty(rtLookupBox.Text))
                {
                    rtLookupBox.Text = " Retention Time";
                }
            }
            else
            {
                rtLookupBox.ForeColor = Color.Black;
            }
        }

        private void rtLookupBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (rtLookupBox.Text.Equals(" Retention Time"))
            {
                rtLookupBox.Text = " ";
            }
        }

        private void findFeatureButton_Click(object sender, EventArgs e)
        {
            if (LookupParametersSatisfied())
            {
                newGroupButton.Enabled = false;
                curateListBox.ClearSelected();
                ZedgraphMethods.ClearZedGraph(zedGraphControl2);
                ZedgraphMethods.ClearZedGraph(zedGraphControl3);
                var rt = double.Parse(rtLookupBox.Text);
                var mz = double.Parse(mzLookupBox.Text);
                var rtRange = new DoubleRange(rt - .05, rt + .05);
                var mzRange = DoubleRange.FromPPM(mz, 10);
                //grab features falling in these ranges
                List<Feature> timeRangeFeatures =
                    analyteMaster.allFeatures.Where(x => x.ApexTime <= rtRange.Maximum && x.ApexTime >= rtRange.Minimum)
                        .ToList();
                List<Feature> matchingMZFeatures = timeRangeFeatures.Where(x => mzRange.Contains(x.averageMZ)).ToList();
                if (matchingMZFeatures.Count > 0)
                {
                    newGroupButton.Enabled = true;
                    MatchingMZFeature = null;
                    if (matchingMZFeatures.Count > 1)
                    {
                        double timeDiff = double.MaxValue;
                        foreach (var feat in matchingMZFeatures)
                        {
                            var currDiff = Math.Abs(feat.ApexTime - rt);
                            if (currDiff < timeDiff)
                            {
                                timeDiff = currDiff;
                                MatchingMZFeature = feat;
                            }
                        }
                    }
                    else
                    {
                        MatchingMZFeature = matchingMZFeatures[0];
                    }
                }
                var zm = new ZedgraphMethods();
                zm.PlotXICs(matchingMZFeatures, zedGraphControl2, Color.LimeGreen);
                zm.PlotXICs(timeRangeFeatures, zedGraphControl2, Color.LightGray);
                foreach (LineItem curve in zedGraphControl2.GraphPane.CurveList)
                {
                    if (curve.Line.Color.Equals(Color.LimeGreen))
                    {
                        curve.Line.Width = 3;
                    }
                }
                dataGridView1.Rows.Clear();
                if (MatchingMZFeature != null)
                {
                    string[] array = new string[] { MatchingMZFeature.averageMZ.ToString(), MatchingMZFeature.ApexTime.ToString() };
                    dataGridView1.Rows.Add(array);
                    dataGridView1.ClearSelection();
                }
                zedGraphControl2.GraphPane.XAxis.Scale.Min = rtRange.Minimum;
                zedGraphControl2.GraphPane.XAxis.Scale.Max = rtRange.Maximum;
                ZedgraphMethods.UpdateZedGraph(zedGraphControl2);
                ZedgraphMethods.UpdateZedGraph(zedGraphControl3);
            }
        }

        private bool LookupParametersSatisfied()
        {
            try
            {
                var a = double.Parse(mzLookupBox.Text);
                var b = double.Parse(rtLookupBox.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Specify Valid Lookup Parameters");
                return false;
            }
            return true;
        }

        private void newGroupButton_Click(object sender, EventArgs e)
        {
            if (MatchingMZFeature != null)
            {
                foreach (var spec in analyteMaster.allSpectra)
                {
                    if (spec.FeatureGroup.groupID > lastFeatureGroupID)
                    {
                        lastFeatureGroupID = spec.FeatureGroup.groupID;
                    }
                }
                lastFeatureGroupID++;
                var newEISpec = new EISpectrum();
                var newFeatureGroup = new FeatureGroup(MatchingMZFeature);
                var newMZPeak = new MZPeak(MatchingMZFeature.averageMZ, MatchingMZFeature.maxIntensity);
                newEISpec.FeatureGroup = newFeatureGroup;
                newEISpec.ApexTimeEI = MatchingMZFeature.ApexTime;
                newEISpec.FinalEIPeaks.Add(newMZPeak);
                newEISpec.NumPeaks = 1;
                newEISpec.isValid = true;
                newEISpec.userAdded = true;
                analyteMaster.allSpectra.Add(newEISpec);
                newEISpec.FeatureGroup.includedFeatureIDs = new HashSet<int>();
                newEISpec.FeatureGroup.includedFeatureIDs.Add(MatchingMZFeature.ID_Number);
                var norm = new Normalization();
                newEISpec.quantIons.Add(newMZPeak);
                newEISpec.FinalNormalizedEIPeaks.AddRange(newEISpec.FinalEIPeaks);
                norm.NormalizePeaks(newEISpec.FinalNormalizedEIPeaks, 1000);
                newGroupButton.Enabled = false;
                analyteMaster.allSpectra = analyteMaster.allSpectra.OrderBy(x => x.ApexTimeEI).ToList();
                newEISpec.spectrumID = lastFeatureGroupID;
                newFeatureGroup.groupID = lastFeatureGroupID;
                PopulateEISpectraLists();
                rtLookupBox.Text = " Retention Time";
                mzLookupBox.Text = " M/Z";
            }
        }

        private void zedGraphControl2_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            if (zedGraphControl2.GraphPane.YAxis.Scale.Min < 0)
            {
                zedGraphControl2.GraphPane.YAxis.Scale.Min = 0;
            }
        }
    }
}
