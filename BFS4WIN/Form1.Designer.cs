namespace BFS4WIN
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.drivesView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.btn_queryDrives = new System.Windows.Forms.Button();
            this.btn_format = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.bfsView = new System.Windows.Forms.ListView();
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button4 = new System.Windows.Forms.Button();
            this.btn_deleteFile = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.btn_CreateEmptyPlotFile = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_version = new System.Windows.Forms.TextBox();
            this.tb_capa1 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tb_capa2 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.memLimit = new System.Windows.Forms.NumericUpDown();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.label8 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.memLimit)).BeginInit();
            this.SuspendLayout();
            // 
            // drivesView
            // 
            this.drivesView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8});
            this.drivesView.FullRowSelect = true;
            this.drivesView.HideSelection = false;
            this.drivesView.Location = new System.Drawing.Point(12, 25);
            this.drivesView.MultiSelect = false;
            this.drivesView.Name = "drivesView";
            this.drivesView.Size = new System.Drawing.Size(796, 179);
            this.drivesView.TabIndex = 0;
            this.drivesView.UseCompatibleStateImageBehavior = false;
            this.drivesView.View = System.Windows.Forms.View.Details;
            this.drivesView.SelectedIndexChanged += new System.EventHandler(this.drivesView_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "ID";
            this.columnHeader1.Width = 26;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Drive";
            this.columnHeader2.Width = 121;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Total Sectors";
            this.columnHeader3.Width = 78;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Sector Size";
            this.columnHeader4.Width = 77;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Caption";
            this.columnHeader5.Width = 229;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Capacity [GiB]";
            this.columnHeader6.Width = 82;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Capacity [Nonces]";
            this.columnHeader7.Width = 120;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "BFS";
            this.columnHeader8.Width = 59;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Drives";
            // 
            // btn_queryDrives
            // 
            this.btn_queryDrives.Location = new System.Drawing.Point(12, 210);
            this.btn_queryDrives.Name = "btn_queryDrives";
            this.btn_queryDrives.Size = new System.Drawing.Size(165, 23);
            this.btn_queryDrives.TabIndex = 2;
            this.btn_queryDrives.Text = "Refresh Drives";
            this.btn_queryDrives.UseVisualStyleBackColor = true;
            this.btn_queryDrives.Click += new System.EventHandler(this.btn_QueryDrives_Click);
            // 
            // btn_format
            // 
            this.btn_format.Location = new System.Drawing.Point(183, 210);
            this.btn_format.Name = "btn_format";
            this.btn_format.Size = new System.Drawing.Size(165, 23);
            this.btn_format.TabIndex = 5;
            this.btn_format.Text = "Format Drive to BFS";
            this.btn_format.UseVisualStyleBackColor = true;
            this.btn_format.Click += new System.EventHandler(this.btn_format_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 245);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "BFS Contents";
            // 
            // bfsView
            // 
            this.bfsView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader13});
            this.bfsView.FullRowSelect = true;
            this.bfsView.HideSelection = false;
            this.bfsView.Location = new System.Drawing.Point(12, 261);
            this.bfsView.Name = "bfsView";
            this.bfsView.Size = new System.Drawing.Size(796, 163);
            this.bfsView.TabIndex = 6;
            this.bfsView.UseCompatibleStateImageBehavior = false;
            this.bfsView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "ID";
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "StartNonce";
            this.columnHeader10.Width = 94;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "#Nonces";
            this.columnHeader11.Width = 99;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "StartSector";
            this.columnHeader12.Width = 96;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "Status";
            this.columnHeader13.Width = 222;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(12, 484);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(165, 23);
            this.button4.TabIndex = 8;
            this.button4.Text = "Upload File";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // btn_deleteFile
            // 
            this.btn_deleteFile.Location = new System.Drawing.Point(354, 484);
            this.btn_deleteFile.Name = "btn_deleteFile";
            this.btn_deleteFile.Size = new System.Drawing.Size(165, 23);
            this.btn_deleteFile.TabIndex = 9;
            this.btn_deleteFile.Text = "Delete Last File";
            this.btn_deleteFile.UseVisualStyleBackColor = true;
            this.btn_deleteFile.Click += new System.EventHandler(this.btn_deleteFile_Click);
            // 
            // button6
            // 
            this.button6.Enabled = false;
            this.button6.Location = new System.Drawing.Point(12, 513);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(165, 23);
            this.button6.TabIndex = 10;
            this.button6.Text = "Download Selected File";
            this.button6.UseVisualStyleBackColor = true;
            // 
            // btn_CreateEmptyPlotFile
            // 
            this.btn_CreateEmptyPlotFile.Location = new System.Drawing.Point(183, 484);
            this.btn_CreateEmptyPlotFile.Name = "btn_CreateEmptyPlotFile";
            this.btn_CreateEmptyPlotFile.Size = new System.Drawing.Size(165, 23);
            this.btn_CreateEmptyPlotFile.TabIndex = 12;
            this.btn_CreateEmptyPlotFile.Text = "Create Empty Plot";
            this.btn_CreateEmptyPlotFile.UseVisualStyleBackColor = true;
            this.btn_CreateEmptyPlotFile.Click += new System.EventHandler(this.btn_CreateEmptyPlotFile_Click);
            // 
            // button9
            // 
            this.button9.Enabled = false;
            this.button9.Location = new System.Drawing.Point(183, 513);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(165, 23);
            this.button9.TabIndex = 13;
            this.button9.Text = "Plot Selected File";
            this.button9.UseVisualStyleBackColor = true;
            // 
            // button10
            // 
            this.button10.Enabled = false;
            this.button10.Location = new System.Drawing.Point(354, 513);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(165, 23);
            this.button10.TabIndex = 14;
            this.button10.Text = "Merge File Into Selected File";
            this.button10.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 433);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "BFS version";
            // 
            // tb_version
            // 
            this.tb_version.Location = new System.Drawing.Point(77, 430);
            this.tb_version.Name = "tb_version";
            this.tb_version.Size = new System.Drawing.Size(78, 20);
            this.tb_version.TabIndex = 17;
            // 
            // tb_capa1
            // 
            this.tb_capa1.Location = new System.Drawing.Point(312, 430);
            this.tb_capa1.Name = "tb_capa1";
            this.tb_capa1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tb_capa1.Size = new System.Drawing.Size(78, 20);
            this.tb_capa1.TabIndex = 19;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(165, 433);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(124, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Formatted Capacity (GiB)";
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(504, 430);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(78, 20);
            this.textBox4.TabIndex = 21;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(403, 433);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 20;
            this.label5.Text = "Available (GiB)";
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(504, 454);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(78, 20);
            this.textBox5.TabIndex = 25;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(403, 457);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(96, 13);
            this.label6.TabIndex = 24;
            this.label6.Text = "Available (Nonces)";
            // 
            // tb_capa2
            // 
            this.tb_capa2.Location = new System.Drawing.Point(312, 454);
            this.tb_capa2.Name = "tb_capa2";
            this.tb_capa2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tb_capa2.Size = new System.Drawing.Size(78, 20);
            this.tb_capa2.TabIndex = 23;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(165, 457);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(144, 13);
            this.label7.TabIndex = 22;
            this.label7.Text = "Formatted Capacity (Nonces)";
            // 
            // textBox8
            // 
            this.textBox8.Location = new System.Drawing.Point(77, 454);
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(78, 20);
            this.textBox8.TabIndex = 27;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 457);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 13);
            this.label9.TabIndex = 26;
            this.label9.Text = "Bad Sectors";
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            this.openFileDialog.Filter = "PoC2 plot files|*_*_*.*|PoC1 plot files|*_*_*_*.*";
            // 
            // memLimit
            // 
            this.memLimit.Increment = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.memLimit.Location = new System.Drawing.Point(713, 513);
            this.memLimit.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.memLimit.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.memLimit.Name = "memLimit";
            this.memLimit.Size = new System.Drawing.Size(62, 20);
            this.memLimit.TabIndex = 29;
            this.memLimit.Value = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 543);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(820, 22);
            this.statusStrip1.TabIndex = 30;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(669, 515);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(38, 13);
            this.label8.TabIndex = 31;
            this.label8.Text = "Cache";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(781, 515);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(23, 13);
            this.label10.TabIndex = 32;
            this.label10.Text = "MB";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(354, 210);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(165, 23);
            this.button1.TabIndex = 33;
            this.button1.Text = "Set Numeric ID";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 565);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.memLimit);
            this.Controls.Add(this.textBox8);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tb_capa2);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tb_capa1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tb_version);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.btn_CreateEmptyPlotFile);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.btn_deleteFile);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.bfsView);
            this.Controls.Add(this.btn_format);
            this.Controls.Add(this.btn_queryDrives);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.drivesView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Johnny\'s BFSTool v.0.1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.memLimit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView drivesView;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_queryDrives;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.Button btn_format;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView bfsView;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button btn_deleteFile;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.Button btn_CreateEmptyPlotFile;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_version;
        private System.Windows.Forms.TextBox tb_capa1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tb_capa2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.NumericUpDown memLimit;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button button1;
    }
}

