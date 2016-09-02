namespace Y3K_Deconvolution_Engine
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.runButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.outputFolderBrowseButton = new System.Windows.Forms.Button();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rawRemoveButton = new System.Windows.Forms.Button();
            this.rawClearButton = new System.Windows.Forms.Button();
            this.rawBrowseButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.rawFileListBox = new System.Windows.Forms.ListBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusBar = new System.Windows.Forms.ToolStripProgressBar();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // runButton
            // 
            this.runButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.runButton.Location = new System.Drawing.Point(15, 515);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(612, 23);
            this.runButton.TabIndex = 14;
            this.runButton.Text = "Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.outputFolderBrowseButton);
            this.groupBox2.Controls.Add(this.outputTextBox);
            this.groupBox2.Location = new System.Drawing.Point(6, 443);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(629, 68);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Output Location";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Output Folder";
            // 
            // outputFolderBrowseButton
            // 
            this.outputFolderBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.outputFolderBrowseButton.Location = new System.Drawing.Point(504, 36);
            this.outputFolderBrowseButton.Name = "outputFolderBrowseButton";
            this.outputFolderBrowseButton.Size = new System.Drawing.Size(117, 23);
            this.outputFolderBrowseButton.TabIndex = 5;
            this.outputFolderBrowseButton.Text = "Browse";
            this.outputFolderBrowseButton.UseVisualStyleBackColor = true;
            this.outputFolderBrowseButton.Click += new System.EventHandler(this.outputFolderBrowseButton_Click);
            // 
            // outputTextBox
            // 
            this.outputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputTextBox.Location = new System.Drawing.Point(9, 38);
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.Size = new System.Drawing.Size(486, 20);
            this.outputTextBox.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.rawRemoveButton);
            this.groupBox1.Controls.Add(this.rawClearButton);
            this.groupBox1.Controls.Add(this.rawBrowseButton);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.rawFileListBox);
            this.groupBox1.Location = new System.Drawing.Point(6, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(629, 435);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Raw Data Entry";
            // 
            // rawRemoveButton
            // 
            this.rawRemoveButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.rawRemoveButton.Location = new System.Drawing.Point(260, 403);
            this.rawRemoveButton.Name = "rawRemoveButton";
            this.rawRemoveButton.Size = new System.Drawing.Size(117, 23);
            this.rawRemoveButton.TabIndex = 4;
            this.rawRemoveButton.Text = "Remove";
            this.rawRemoveButton.UseVisualStyleBackColor = true;
            this.rawRemoveButton.Click += new System.EventHandler(this.rawRemoveButton_Click);
            // 
            // rawClearButton
            // 
            this.rawClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.rawClearButton.Location = new System.Drawing.Point(504, 403);
            this.rawClearButton.Name = "rawClearButton";
            this.rawClearButton.Size = new System.Drawing.Size(117, 23);
            this.rawClearButton.TabIndex = 3;
            this.rawClearButton.Text = "Clear";
            this.rawClearButton.UseVisualStyleBackColor = true;
            this.rawClearButton.Click += new System.EventHandler(this.rawClearButton_Click);
            // 
            // rawBrowseButton
            // 
            this.rawBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rawBrowseButton.Location = new System.Drawing.Point(8, 403);
            this.rawBrowseButton.Name = "rawBrowseButton";
            this.rawBrowseButton.Size = new System.Drawing.Size(117, 23);
            this.rawBrowseButton.TabIndex = 2;
            this.rawBrowseButton.Text = "Browse";
            this.rawBrowseButton.UseVisualStyleBackColor = true;
            this.rawBrowseButton.Click += new System.EventHandler(this.rawBrowseButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Thermo Raw Files (*.raw)";
            // 
            // rawFileListBox
            // 
            this.rawFileListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rawFileListBox.FormattingEnabled = true;
            this.rawFileListBox.Location = new System.Drawing.Point(9, 42);
            this.rawFileListBox.Name = "rawFileListBox";
            this.rawFileListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.rawFileListBox.Size = new System.Drawing.Size(612, 355);
            this.rawFileListBox.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.statusBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 549);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(642, 22);
            this.statusStrip1.TabIndex = 13;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(475, 17);
            this.statusLabel.Spring = true;
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusBar
            // 
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(150, 16);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(642, 571);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.Text = "Deconvolution Engine";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button outputFolderBrowseButton;
        private System.Windows.Forms.TextBox outputTextBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button rawRemoveButton;
        private System.Windows.Forms.Button rawClearButton;
        private System.Windows.Forms.Button rawBrowseButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox rawFileListBox;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar statusBar;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    }
}

