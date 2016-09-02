using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.SQLite;
using System.Threading;

namespace Y3K_GC_Quant
{
    public partial class Form1 : Form
    {
        public Dictionary<string, Batch> BatchDictionary;
        public List<Replicate> ReplicateList;
        public Master master;
        public SQLiteConnection conn; //Quant database connection
        public int currentReplicate = 0;
        public QuantMethods currentQM;
        public Thread CurrentThread;

        public Form1()
        {
            InitializeComponent();
        }

        private void gcMastBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "GC Master Files (*.gcmast)|*.gcmast";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                gcMastTextBox.Text = openFileDialog1.FileName;
            }
        }

        private void gcExpBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "GC Experiment Files (*.gcexp)|*.gcexp";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                gcExpTextBox.Text = openFileDialog1.FileName;
            }
        }

        private void outputBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                outputDirectoryTextBox.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            if (isReady())
            {
                foreach (Control control in this.Controls)
                {
                    control.Enabled = false;
                }
                toolStripProgressBar1.Enabled = true;
                toolStripStatusLabel1.Enabled = true;
                CreateBatchDictionary();
                PopulateProgressListBox();
                var newThread = new Thread(() => ReadInMasterFile());
                newThread.Start();
            }
        }

        public bool isReady()
        {
            if (string.IsNullOrEmpty(gcMastTextBox.Text))
            {
                MessageBox.Show("Specify Valid GC Master File");
                return false;
            }
            if (string.IsNullOrEmpty(gcExpTextBox.Text))
            {
                MessageBox.Show("Specify Valid GC Experiment File");
                return false;
            }
            if (string.IsNullOrEmpty(outputDirectoryTextBox.Text))
            {
                MessageBox.Show("Specify Valid Output Directory");
                return false;
            }
            return true;
        }
        public void CreateBatchDictionary()
        {
            BatchDictionary = new Dictionary<string, Batch>();
            var gcExpReader = new StreamReader(gcExpTextBox.Text);
            HashSet<string> batchNamesHashSet = new HashSet<string>();
            List<Replicate> replicates = new List<Replicate>();
            List<Batch> batches = new List<Batch>();
            while (gcExpReader.Peek() > -1)
            {
                var currLine = gcExpReader.ReadLine();
                if (!currLine.Equals("\t\t\t\t") && !string.IsNullOrEmpty(currLine))
                {
                    string[] parts = currLine.Split('\t');
                    var gcFeatPath = parts[0];
                    var name = parts[1];
                    var batchName = parts[2];
                    var controlBatchName = parts[3];
                    if (string.IsNullOrEmpty(batchName))
                    {
                        batchName = name;
                    }
                    var newRep = new Replicate();
                    newRep.name = name;
                    newRep.gcFeatFilePath = gcFeatPath;
                    batchNamesHashSet.Add(batchName);
                    batchNamesHashSet.Add(controlBatchName);
                    newRep.controlName = controlBatchName;
                    newRep.batchName = batchName;
                    replicates.Add(newRep);
                }
            }
            foreach (var batch in batchNamesHashSet.ToList())
            {
                var newBatch = new Batch();
                newBatch.name = batch;
                batches.Add(newBatch);
                BatchDictionary.Add(newBatch.name, newBatch);
            }
            foreach (var rep in replicates)
            {
                var currBatch = BatchDictionary[rep.batchName];
                currBatch.replicates.Add(rep);
                rep.control = BatchDictionary[rep.controlName];
            }
            foreach (var batch in BatchDictionary.Where(x => x.Value.replicates.Count == 0).ToList())
            {
                BatchDictionary.Remove(batch.Key);
            }
            ReplicateList = new List<Replicate>();
            foreach (var batch in BatchDictionary.Values)
            {
                foreach (var rep in batch.replicates)
                {
                    if (!BatchDictionary.ContainsKey(rep.controlName))
                    {
                        rep.control = null;
                        rep.controlName = null;
                    }
                    ReplicateList.Add(rep);
                }
            }
        }
        public void PopulateProgressListBox()
        {
            rawFileListBox.Items.Clear();
            foreach (var batch in BatchDictionary.Values)
            {
                foreach (var rep in batch.replicates)
                {
                    rawFileListBox.Items.Add(rep.name);
                }
            }
        }
        public void ReadInMasterFile()
        {
            UpdateText("Reading GC Master File...");
            master = new Master(gcMastTextBox.Text);
            master.ReadInMaster();
            UpdateText("Preparing GC Results Database...");
            SQLiteIOMethods.CreateQuantDatabase(outputDirectoryTextBox.Text, out conn);
            SQLiteIOMethods.AddBatchStructure(BatchDictionary, conn);
            SQLiteIOMethods.AddMasterInformationToDatabase(conn, gcMastTextBox.Text);
            UpdateText("Starting Quantitation...");
            MasterReadInFinished();
        }

        public void UpdateText(string text)
        {
            Invoke(new Action(() =>
            {
                this.Text = text;
            }));
        }
        public void MasterReadInFinished()
        {
            Invoke(new Action(() =>
            {
                QuantifyNewReplicate();
            }));
        }

        public void QuantifyNewReplicate()
        {
            if (currentReplicate >= ReplicateList.Count)
            {
                //this.Text = "Done!";
                this.Text = currentReplicate + "\\" + ReplicateList.Count + " Raw Files Quantified...";
                toolStripStatusLabel1.Text = "Normalizing Extracted Data...";
                DataNormalizationMethods.ProcessData(conn);
                foreach (Control control in Controls)
                {
                    control.Enabled = true;
                }
                UpdateText("Done!");
                toolStripStatusLabel1.Text = "Done!";
                toolStripProgressBar1.Value = 200;
                return;
            }
            else
            {
                var currRepObject = ReplicateList[currentReplicate];
                currentQM = new QuantMethods(currRepObject, master, conn);
                currentQM.Progress += quantify_Progress;
                currentQM.Finished += quantify_Finished;
                rawFileListBox.ClearSelected();
                rawFileListBox.SelectedItem = currRepObject.name;
                toolStripStatusLabel1.Text = "Quantifying " + currRepObject.name + "...";
                CurrentThread = new Thread(() => currentQM.Quantify());
                CurrentThread.Start();
            }
        }

        public void quantify_Progress(object sender, ProgressStatusEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, ProgressStatusEventArgs>(quantify_Progress), sender, e);
                return;
            }
            toolStripProgressBar1.Value = (int)(e.Percent);
        }
        public void quantify_Finished(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, EventArgs>(quantify_Finished), sender, e);
                return;
            }
            toolStripProgressBar1.Value = (int)0;
            currentQM.ReplicateDBConnection.Close();
            currentQM.ReplicateDBConnection.Dispose();
            currentQM.Finished -= quantify_Finished;
            currentQM.Progress -= quantify_Progress;
            currentQM = null;
            GC.Collect();
            currentReplicate++;
            QuantifyNewReplicate();
            this.Text = currentReplicate + "\\" + ReplicateList.Count + " Raw Files Quantified...";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CurrentThread != null)
            {
                if (CurrentThread.IsAlive)
                {
                    CurrentThread.Abort();
                }
            }
        }
    }
}
