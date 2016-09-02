using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Threading;

namespace Y3K_Deconvolution_Engine
{
    public partial class Form1 : Form
    {
        public SQLiteConnection conn;
        public List<ThermoRawFile> rawFiles;
        public List<Feature> currentFeatures;
        public Thread currentThread;
        public FeatureDetectionMethods currFDM;
        public ThermoRawFile currRawFile;
        public SQLiteIOMethods sqliteIO;
        public List<string> allRawFileNames;
        public int currIndex = 0;
        public DatabasePrepMethods dbPrep;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] data = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (data == null)
                    return;
                foreach (string datum in data)
                {
                    if (datum.Contains(".raw"))
                    {
                        rawFileListBox.Items.Add(datum);
                    }
                }
            }
        }
        private void rawBrowseButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Thermo Raw Files (*.raw) | *.raw";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (var file in openFileDialog1.FileNames)
                {
                    rawFileListBox.Items.Add(file);
                }
            }
        }
        private void rawRemoveButton_Click(object sender, EventArgs e)
        {
            for (int i = rawFileListBox.SelectedIndices.Count - 1; i >= 0; i--)
            {
                rawFileListBox.Items.RemoveAt(rawFileListBox.SelectedIndices[i]);
            }
        }
        private void rawClearButton_Click(object sender, EventArgs e)
        {
            rawFileListBox.Items.Clear();
        }
        private void outputFolderBrowseButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                outputTextBox.Text = folderBrowserDialog1.SelectedPath;
            }
        }
        private void runButton_Click(object sender, EventArgs e)
        {
            if (rawFileListBox.Items.Count == 0)
            {
                MessageBox.Show("Add Thermo Raw Files");
                return;
            }
            if (string.IsNullOrEmpty(outputTextBox.Text))
            {
                MessageBox.Show("Specify Valid Output Directory");
                return;
            }
            foreach (Control control in this.Controls)
            {
                control.Enabled = false;
            }
            statusStrip1.Enabled = true;
            statusLabel.Enabled = true;
            statusLabel.ForeColor = Color.DarkGreen;
            DeconvolutionEngine();
        }

        private void DeconvolutionEngine()
        {
            allRawFileNames = new List<string>();
            statusBar.Value = 0;
            foreach (var item in rawFileListBox.Items)
            {
                allRawFileNames.Add(item.ToString());
            }
            if (allRawFileNames.Count > 0)
            {
                currIndex = 0;
                prepareDatabase();
            }
        }
        private void prepareDatabase()
        {
            if (currIndex < allRawFileNames.Count)
            {
                rawFileListBox.ClearSelected();
                rawFileListBox.SelectedIndex = currIndex;
                var currRawFileString = allRawFileNames[currIndex];
                statusBar.Value = (int)(((double)currIndex) / ((double)allRawFileNames.Count) * 100);
                statusLabel.Text = "Creating Database for " + currRawFileString + " ...";
                dbPrep = new DatabasePrepMethods();
                dbPrep.Finished += databasePrep_Finished;
                currentThread = new Thread(() => dbPrep.PrepDatabase(currRawFileString, outputTextBox.Text));
                currentThread.Start();
            }
            else
            {
                currIndex = 0;
                statusBar.Value = 0;
                statusLabel.Text = "Starting Feature Extraction...";
                extractFeatures();
            }
        }
        private void databasePrep_Finished(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, EventArgs>(databasePrep_Finished), sender, e);
                return;
            }
            dbPrep.Finished -= databasePrep_Finished;
            GC.Collect();
            currIndex++;
            prepareDatabase();
        }
        private void extractFeatures()
        {
            if (currIndex < allRawFileNames.Count)
            {
                var currRawFileString = allRawFileNames[currIndex];
                currRawFile = new ThermoRawFile(currRawFileString);
                currRawFile.Open();
                var path = outputTextBox.Text + "\\" + currRawFile.Name + "_ExtractedFeatures.gcfeat";
                conn = new SQLiteConnection(@"Data Source=" + path);
                conn.Open();
                rawFileListBox.ClearSelected();
                rawFileListBox.SelectedIndex = currIndex;
                this.Text = currIndex + "\\" + allRawFileNames.Count + " Deconvolved...";
                if (!SQLiteIOMethods.IsExtractionDone(conn))
                {
                    currFDM = new FeatureDetectionMethods(currRawFile);
                    statusLabel.Text = "Adding Features from " + currRawFile.Name + " to Database...";
                    currFDM.Progress += deconvolution_Progress;
                    currFDM.Finished += deconvolution_Finished;
                    currentThread = new Thread(() => currFDM.StepwiseSetupFinalGroups(currRawFile, conn));
                    currentThread.Start();
                }
                else
                {
                    conn.Close();
                    conn.Dispose();
                    currRawFile.Dispose();
                    currIndex++;
                    extractFeatures();
                }
            }
            else
            {
                currIndex = 0;
                //statusBar.Value = 0;
                //statusLabel.Text = "Starting Feature Grouping...";
                //GroupFeatures();
                
                currRawFile.ClearCachedScans();
                currRawFile.Dispose();
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
                conn = null;
                if (currFDM != null)
                {
                    currFDM.Dispose();
                    currFDM = null;
                }
                GC.Collect();
                rawFileListBox.ClearSelected();
                foreach (Control control in this.Controls)
                {
                    control.Enabled = true;
                }
                statusBar.Value = 100;
                statusLabel.Text = "Done!";
                this.Text = "Deconvolution Engine";
            }
        }
        public void deconvolution_Progress(object sender, ProgressStatusEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, ProgressStatusEventArgs>(deconvolution_Progress), sender, e);
                return;
            }
            statusBar.Value += (int)(statusBar.Maximum * e.Percent);
        }
        private void deconvolution_Finished(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, EventArgs>(deconvolution_Finished), sender, e);
                return;
            }
            statusBar.Value = (int)0;
            currRawFile.ClearCachedScans();
            currRawFile.Dispose();
            conn.Close();
            conn.Dispose();
            conn = null;
            currFDM.Finished -= deconvolution_Finished;
            currFDM.Progress -= deconvolution_Progress;
            currFDM.Dispose();
            currFDM = null;
            GC.Collect();
            currIndex++;
            extractFeatures();
        }
        private void GroupFeatures()
        {
            if (currIndex < allRawFileNames.Count)
            {
                var currRawFileString = allRawFileNames[currIndex];
                currRawFile = new ThermoRawFile(currRawFileString);
                currRawFile.Open();
                var path = outputTextBox.Text + "\\" + currRawFile.Name + "_ExtractedFeatures.gcfeat";
                conn = new SQLiteConnection(@"Data Source=" + path);
                conn.Open();
                rawFileListBox.ClearSelected();
                rawFileListBox.SelectedIndex = currIndex;
                if (!SQLiteIOMethods.IsGroupingDone(conn))
                {
                    currFDM = new FeatureDetectionMethods(currRawFile);
                    statusLabel.Text = "Grouping Features from " + currRawFile.Name + "...";
                    currFDM.Finished += grouping_Finished;
                    //currentThread = new Thread(() => currFDM.GroupFeatures(conn));
                    //currentThread.Start();
                }
                else
                {
                    currIndex++;
                    double percent = ((double)currIndex) / ((double)allRawFileNames.Count) * 100;
                    statusBar.Value = (int)percent;
                    GroupFeatures();
                }
            }
            else
            {
                currRawFile.ClearCachedScans();
                currRawFile.Dispose();
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
                conn = null;
                if (currFDM != null)
                {
                    currFDM.Dispose();
                    currFDM = null;
                }
                GC.Collect();
                rawFileListBox.ClearSelected();
                foreach (Control control in this.Controls)
                {
                    control.Enabled = true;
                }
                statusBar.Value = 100;
                statusLabel.Text = "Done!";
            }
        }
        private void grouping_Finished(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, EventArgs>(grouping_Finished), sender, e);
                return;
            }
            statusBar.Value = (int)0;
            currRawFile.ClearCachedScans();
            currRawFile.Dispose();
            conn.Close();
            conn.Dispose();
            conn = null;
            currFDM.Finished -= grouping_Finished;
            currFDM.Dispose();
            currFDM = null;
            GC.Collect();
            double percent = ((double)currIndex) / ((double)allRawFileNames.Count) * 100;
            statusBar.Value = (int)percent;
            currIndex++;
            GroupFeatures();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (currentThread != null)
            {
                if (currentThread.IsAlive)
                {
                    currentThread.Abort();
                }
            }
            if (currRawFile != null)
            {
                currRawFile.ClearCachedScans();
                currRawFile.Dispose();
            }
            if (currFDM != null)
            {
                currFDM.Dispose();
                currFDM = null;
            }
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
            //this.Close();
        }
    }
}
