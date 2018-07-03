using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// low level disk access taken from https://code.msdn.microsoft.com/windowsapps/CCS-LABS-C-Low-Level-Disk-91676ca9 

namespace BFS4WIN
{
    public partial class Form1 : Form
    {
        LowLevelDiskAccess llda;
        public Form1()
        {
            InitializeComponent();
            llda = new LowLevelDiskAccess();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btn_QueryDrives_Click(null,null);
        }

        private void btn_QueryDrives_Click(object sender, EventArgs e)
        {

            drivesView.Items.Clear();
            ArrayList result;
            result = llda.GetDriveList();
            int i = 0;
            foreach (string x in result)
            {
                ListViewItem item = new ListViewItem();
                item.Text = i.ToString();
                item.Name = i.ToString();
                item.SubItems.Add(x);
                Int64 sectors = llda.GetSectors(x)/512;
                item.SubItems.Add(sectors.ToString());
                item.SubItems.Add(llda.BytesPerSector(i).ToString());
                item.SubItems.Add(llda.GetCaption(i));
                item.SubItems.Add(((decimal)sectors * llda.BytesPerSector(i) / 1024/1024/1024).ToString("0.00")+" GiB");
                item.SubItems.Add((((long)(sectors-12*8) * llda.BytesPerSector(i) / 4096 / 64 /64)*64).ToString());
                //
                if (BFS.IsBFS(x))
                {
                    item.SubItems.Add("Yes");
                }
                else
                {
                    item.SubItems.Add("No");
                }
                drivesView.Items.Add(item);
                i += 1;
            }
            ClearBFSView();
        }



        private void btn_format_Click(object sender, EventArgs e)
        {
            if (drivesView.SelectedItems.Count > 0)
            {
                //Issue Warning
                DialogResult result = MessageBox.Show("WARNING: Formatting will erase ALL data on this disk." + "\n" + "To format the disk, press OK. To quit, click CANCEL.", "Format Local Disk ( "+ drivesView.SelectedItems[0].SubItems[1].Text + ")", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Cancel) return;

                UInt32 bytesPerSector = (UInt32.Parse(drivesView.SelectedItems[0].SubItems[3].Text));
                UInt64 totalSectors = (UInt64.Parse(drivesView.SelectedItems[0].SubItems[2].Text));
                String drive = drivesView.SelectedItems[0].SubItems[1].Text;

                //Format Drive using GPT, MBR would only make sense for small drives and small drives dont make sense for Burst
                BFS.FormatDriveGPT(drive, totalSectors, bytesPerSector);
                //Success
                MessageBox.Show("Format Complete.\t\t\t", "Formatting " + drivesView.SelectedItems[0].SubItems[1].Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                //Refresh drives
                btn_QueryDrives_Click(null, null);
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private PlotFile ParsePlotFileName(string name)
        {
            string[] temp = name.Split('\\');
            string[] pfn = temp[temp.GetLength(0) - 1].Split('_');
            PlotFile result = new PlotFile();
            result.id = Convert.ToUInt64(pfn[0]);
            result.startNonce = Convert.ToUInt64(pfn[1]);
            result.nonces = Convert.ToUInt32(pfn[2]);
            if (pfn.Length == 4)
            {

                //Todo conversion
                return result;
            }

            return result;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            PlotFile temp;
            //Let user select plotfile
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Parse Flotfilename
                temp = ParsePlotFileName(openFileDialog.FileName);
            }
            else
            {
                return;
            }

            //only support optimized files
            //if (temp.stagger > 0 && temp.stagger != temp.nonces) return;
            //Read current bfsTOC
            BFSTOC bfsTOC = BFSTOC.FromSector(llda.ReadSector(drivesView.SelectedItems[0].SubItems[1].Text, 1, 4096));
            //update bfsTOC
            int position = bfsTOC.AddPlotFile(temp.startNonce, temp.nonces/64*64, 2, 0);
            if (position == -1) return;
            //save bfsTOC
            llda.WriteSector(drivesView.SelectedItems[0].SubItems[1].Text, 1, 4096, bfsTOC.ToByteArray());
            //transfer file
            //open source handle
            ScoopReadWriter reader; ;
            reader = new ScoopReadWriter(openFileDialog.FileName);
            reader.OpenR(true);

            int limit = Convert.ToInt32(memLimit.Value) * 4096;

            //create masterplan     
            int loops = (int)Math.Ceiling((double)(temp.nonces) / limit);
            TaskInfo[] masterplan = new TaskInfo[2048 * loops];
            for (int y = 0; y < 2048; y++)
            {
                int zz = 0;
                //loop partial scoop               
                for (int z = 0; (ulong)z < temp.nonces; z += limit)
                {
                    masterplan[y * loops + zz] = new TaskInfo();
                    masterplan[y * loops + zz].reader = reader;
                    masterplan[y * loops + zz].target = drivesView.SelectedItems[0].SubItems[1].Text;
                    masterplan[y * loops + zz].y = y;
                    masterplan[y * loops + zz].z = z;
                    masterplan[y * loops + zz].x = y * loops + zz;
                    masterplan[y * loops + zz].limit = limit;
                    masterplan[y * loops + zz].src = temp;
                  //  masterplan[y * loops + zz].tar = bfsTOC.plotFiles[position];
                    //masterplan[y * loops + zz].scoop1 = scoop1;
                   // masterplan[y * loops + zz].scoop2 = scoop2;
                   // masterplan[y * loops + zz].scoop3 = scoop3;
                   // masterplan[y * loops + zz].scoop4 = scoop4;
                   // masterplan[y * loops + zz].shuffle = shuffle;
                    masterplan[y * loops + zz].end = masterplan.LongLength;
                    zz += 1;
                }
            }


            //execute taskplan
            //mark progress resume


            //mark as finished
            bfsTOC.plotFiles[position].status = 1;
            bfsTOC.plotFiles[position].pos = temp.nonces;
            bfsTOC.UpdateCRC32();
            //save bfsTOC
            llda.WriteSector(drivesView.SelectedItems[0].SubItems[1].Text, 0, 4096, bfsTOC.ToByteArray());
        }

        private void buttfgon8_Click(object sender, EventArgs e)
        {
            //Read current bfsTOC
            BFSTOC bfsTOC = BFSTOC.FromSector(llda.ReadSector(drivesView.SelectedItems[0].SubItems[1].Text, 0, 4096));
            //Check CRC32
            //if fail try Mirror
            //if not fail, check if mirror is identical
            //if not copy over
            //count current files
          //  bfsTOC.AddPlotFile(13014439754249942082, 1857887936, 0, 2, 0);
            llda.WriteSector(drivesView.SelectedItems[0].SubItems[1].Text, 0, 4096, bfsTOC.ToByteArray());
        }

        struct TaskInfo
        {
            public ScoopReadWriter reader;
            public string target;
            public int y;
            public int z;
            public int x;
            public int limit;
            public PlotFile src;
            public PlotFile tar;
            //public Scoop scoop1;
           // public Scoop scoop2;
           // public Scoop scoop3;
           // public Scoop scoop4;
            public bool shuffle;
            public long end;
        }

  

  

        private void btn_CreateEmptyPlotFile_Click(object sender, EventArgs e)
        {
            String drive = drivesView.SelectedItems[0].SubItems[1].Text;

            UInt64 sn = 0;
            UInt32 nonces = 0;
            if (ShowInputDialog2(drive,"Create new plot file...", ref sn, ref nonces) == DialogResult.OK)
            {
                //add
                BFS.AddPlotFile(drive, sn, nonces, 2, 0);
                //if()
                FillBFSView(drive);
              //             else {
                //    MessageBox.Show("File Creation failed.\t\t\t", "Create file on" + drive, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //}
            }

        }

        private void btn_deleteFile_Click(object sender, EventArgs e)
        {
            String drive = drivesView.SelectedItems[0].SubItems[1].Text;
            if (BFS.DeleteLastPlotFile(drive))
            {
                FillBFSView(drive);
            }
            else
            {
                MessageBox.Show("File Deletion failed.\t\t\t", "Delete last file on" + drive, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (drivesView.SelectedItems.Count > 0)
            {
                GPT gpt = GPT.FromSectors(llda.ReadSector(drivesView.SelectedItems[0].SubItems[1].Text, 0, 4096*5));
               // gpt.gptHeader.UpdateCRC32(CRC.CRC32(gpt.gptPartitionTable.ToByteArray()));

             

            }
        }

        private void drivesView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (drivesView.SelectedItems.Count > 0)
            {
                if (drivesView.SelectedItems[0].SubItems[7].Text == "Yes")
                {
                    FillBFSView(drivesView.SelectedItems[0].SubItems[1].Text);
                }
                else
                {
                    ClearBFSView(); 
                }
            }
        }

        //Read BFS Filesystem and fill controls
        private void FillBFSView(string drive)
        {
            //enable buttons
            btn_setID.Enabled = true;
            btn_createEmptyPlotFile.Enabled = true;
            btn_deleteFile.Enabled = true;
            btn_upload.Enabled = true;
            btn_download.Enabled = true;

            if (drivesView.SelectedItems.Count > 0)
            {
                BFS.LoadBFSTOC(drive);
                //Get BFS Infos
                tb_version.Text = Encoding.ASCII.GetString(BFS.bfsTOC.version);
                tb_id.Text = BFS.bfsTOC.id.ToString();
                capacity.Maximum = (int)(BFS.bfsTOC.diskspace / 64);

                //Clear Files
                bfsView.Items.Clear();
                int i = 0;
                uint totalNonces = 0;
                foreach (BFSPlotFile x in BFS.bfsTOC.plotFiles)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = i.ToString();
                    item.Name = i.ToString();
                    if (x.status == 0)
                    {
                        item.SubItems.Add("Empty Slot");
                    }
                    else
                    {
                        totalNonces += x.nonces;
                        item.SubItems.Add(x.startNonce.ToString());
                        item.SubItems.Add(x.nonces.ToString());
                        item.SubItems.Add(x.startPos.ToString());
                        switch (x.status)
                        {
                            case 1:
                                item.SubItems.Add("OK.");
                                break;
                            case 2:
                                item.SubItems.Add("In Creation. Nonces plotted: " + x.pos.ToString());
                                break;
                            case 3:
                                item.SubItems.Add("Converting. Scoop Pair Progress: " + x.pos.ToString());
                                break;
                        }
                    }
                    bfsView.Items.Add(item);
                    i += 1;
                }
                capacity.Value = (int)(totalNonces);
                lbl_capa.Text = ((decimal)((int)(BFS.bfsTOC.diskspace / 64) - (int)(totalNonces)) / 4 / 1024).ToString("0.00")+" GiB free of" +((decimal)BFS.bfsTOC.diskspace * 4096 / 1024 / 1024 / 1024).ToString("0.00")+" GiB";
                //tb_capa2.Text = ((decimal)BFS.bfsTOC.diskspace / 64).ToString("0");

            }
        }

        private void ClearBFSView()
        {

            //disable buttons
            btn_createEmptyPlotFile.Enabled = false;
            btn_deleteFile.Enabled = false;
            btn_setID.Enabled = false;
            btn_upload.Enabled = false;
            btn_download.Enabled = false;
            bfsView.Items.Clear();
            capacity.Value = 0;
            lbl_capa.Text = "";
            tb_version.Text = "";
            //tb_capa1.Text = "";
            //tb_capa2.Text = "";


        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (drivesView.SelectedItems.Count > 0)
            {
                byte[] x = new byte[512];
                llda.WriteSector(drivesView.SelectedItems[0].SubItems[1].Text, 1, 512,x);

                UInt64 totalSectors = (UInt64.Parse(drivesView.SelectedItems[0].SubItems[2].Text));
                GPT gpt =  GPT.FromSectors(llda.ReadSector(drivesView.SelectedItems[0].SubItems[1].Text, 0, 4096 * 5));
                GPT gpt2 = GPT.FromSectors(llda.ReadSectors(drivesView.SelectedItems[0].SubItems[1].Text, (long)totalSectors-2, 512,2));

                llda.refreshDrive(drivesView.SelectedItems[0].SubItems[1].Text);

                // gpt.gptHeader.UpdateCRC32(CRC.CRC32(gpt.gptPartitionTable.ToByteArray()));



            }

        }

        private static DialogResult ShowInputDialog(string message, ref UInt64 input)
        {
            System.Drawing.Size size = new System.Drawing.Size(200, 70);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.ControlBox = false;
            inputBox.StartPosition = FormStartPosition.CenterScreen;
            inputBox.Text = message;

            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);
            textBox.Text = "";
            textBox.MaxLength = 20;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            UInt64.TryParse(textBox.Text, out input);
            return result;
        }

        private static DialogResult ShowInputDialog2(string drive, string message, ref UInt64 input1, ref UInt32 input2)
        {
            //get boundaries
            BFS.LoadBFSTOC(drive);
            UInt32 totalNonces  = (UInt32)(BFS.bfsTOC.diskspace / 64);

            //Clear Files
            UInt32 noncesUsed = 0;
            foreach (BFSPlotFile x in BFS.bfsTOC.plotFiles)
            {
                if (x.status != 0)
                {
                    noncesUsed += x.nonces;
                }
            }
            
            
            System.Drawing.Size size = new System.Drawing.Size(300, 200);
            Form inputBox = new Form();
            
            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.ControlBox = false;
            inputBox.StartPosition = FormStartPosition.CenterScreen;
            inputBox.Text = message;

            System.Windows.Forms.Label lbl_sn = new Label();
            lbl_sn.Size = new System.Drawing.Size(70, 23);
            lbl_sn.Location = new System.Drawing.Point(10, 8);
            lbl_sn.Text = "Start Nonce: ";            
            inputBox.Controls.Add(lbl_sn);

            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 90, 23);
            textBox.Location = new System.Drawing.Point(80, 5);
            textBox.Text = "";
            textBox.TextAlign = HorizontalAlignment.Right;
            textBox.MaxLength = 20;
            inputBox.Controls.Add(textBox);

            System.Windows.Forms.Label lbl_sn2 = new Label();
            lbl_sn2.Size = new System.Drawing.Size(70, 23);
            lbl_sn2.Location = new System.Drawing.Point(10, 37);
            lbl_sn2.Text = "Nonces: ";
            inputBox.Controls.Add(lbl_sn2);

            System.Windows.Forms.NumericUpDown nonces = new NumericUpDown();
            nonces.Size = new System.Drawing.Size(size.Width - 90, 23);
            nonces.Location = new System.Drawing.Point(80, 34);
            nonces.Text = "";
            nonces.Maximum = 9999999;
            nonces.TextAlign = HorizontalAlignment.Right;
            nonces.Increment = 64;
            inputBox.Controls.Add(nonces);

            //Trackbar 
            System.Windows.Forms.TrackBar trackbar = new TrackBar();
            trackbar.Size = new System.Drawing.Size(size.Width - 80, 45);
            trackbar.Location = new System.Drawing.Point(75, 63);
            trackbar.SmallChange = 64;
            trackbar.LargeChange = (int)(totalNonces-noncesUsed)/64;
            trackbar.TickFrequency = trackbar.LargeChange;
            trackbar.Maximum = (int)(totalNonces - noncesUsed);
            trackbar.ValueChanged += new EventHandler(yourMethod);

            void yourMethod(object s, EventArgs e)
            {
                nonces.Value = trackbar.Value;
            }
            inputBox.Controls.Add(trackbar);


            System.Windows.Forms.Label lbl_sn3 = new Label();
            lbl_sn3.Size = new System.Drawing.Size(70, 23);
            lbl_sn3.Location = new System.Drawing.Point(10, 111);
            lbl_sn3.Text = "Capacity: ";
            inputBox.Controls.Add(lbl_sn3);

            //progressbar
            System.Windows.Forms.ProgressBar progress = new ProgressBar();
            progress.Size = new System.Drawing.Size(size.Width - 90, 16);
            progress.Location = new System.Drawing.Point(80, 110);
            inputBox.Controls.Add(progress);

            //lbl_capa.Text = ((decimal)((int)(BFS.bfsTOC.diskspace / 64) - (int)(totalNonces)) / 4 / 1024).ToString("0.00") + " GiB free of" + ((decimal)BFS.bfsTOC.diskspace * 4096 / 1024 / 1024 / 1024).ToString("0.00") + " GiB";

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 85 - 80, 150);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 85, 150);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            UInt64.TryParse(textBox.Text, out input1);
            input2 = (uint)nonces.Value;

            return result;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            String drive = drivesView.SelectedItems[0].SubItems[1].Text;
            UInt64 id = 0;
            if (ShowInputDialog("Please enter numeric ID", ref id) == DialogResult.OK) {
                BFS.SetID(drive,id);
                //update ID
                tb_id.Text = id.ToString();
            }

        }
    }
}
