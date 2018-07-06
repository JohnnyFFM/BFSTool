using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFS4WIN
{
    public partial class Form1 : Form
    {
        LowLevelDiskAccess llda;
        static AutoResetEvent[] autoEvents;
        static UInt64 startOffset;
        static UInt64 scoopOffset;
        static Boolean halt1 = false;
        static Boolean halt2 = false;

        public Form1()
        {
            InitializeComponent();
            llda = new LowLevelDiskAccess();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Query drives on load
            btn_QueryDrives_Click(null,null);
        }

        //query drives and get drive info
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
                UInt32 sectorsL = llda.BytesPerSector(i);
                item.SubItems.Add(sectorsL.ToString());
                UInt32 sectorsP = llda.GetPhysicalSectors(x);
                item.SubItems.Add(sectorsP > 0 ?  sectorsP.ToString():sectorsL.ToString());
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

        //Format drive to BFS
        private void btn_format_Click(object sender, EventArgs e)
        {
            if (drivesView.SelectedItems.Count > 0)
            {
                String drive = drivesView.SelectedItems[0].SubItems[1].Text;

                UInt64 id = 0;
                if (Dialogs.ShowInputDialog("Please enter numeric ID", ref id) != DialogResult.OK)
                {
                    return;
                }

                //Issue Warning
                DialogResult result = MessageBox.Show("WARNING: Formatting will erase ALL data on this disk." + "\n" + "To format the disk, press OK. To quit, click CANCEL.", "Format Local Disk ( "+ drive + ")", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Cancel) return;

                UInt32 bytesPerSector = (UInt32.Parse(drivesView.SelectedItems[0].SubItems[3].Text));
                UInt64 totalSectors = (UInt64.Parse(drivesView.SelectedItems[0].SubItems[2].Text));

                //Format Drive using GPT, MBR would only make sense for small drives and small drives dont make sense for Burst
                BFS.FormatDriveGPT(drive, totalSectors, bytesPerSector, id);
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

        //Parse PoC1 and PoC2 plot file names. For PoC2 stagger will be set to 0;
        private PlotFile ParsePlotFileName(string name)
        {
            string[] temp = name.Split('\\');
            string[] pfn = temp[temp.GetLength(0) - 1].Split('_');
            PlotFile result = new PlotFile();
            result.id = Convert.ToUInt64(pfn[0]);
            result.startNonce = Convert.ToUInt64(pfn[1]);
            result.nonces = Convert.ToUInt32(pfn[2]);
            result.stagger = 0;
            if (pfn.Length == 4)
            {
                result.stagger = Convert.ToUInt32(pfn[3]);
            }
            return result;
        }

        //Task description for dual-threadding
        public struct TaskInfo
        {
            public ScoopReadWriter reader;
            public ScoopReadWriter writer;
            public String drive;
            public int file;
            public int y;
            public int z;
            public int x;
            public UInt64 scoopoffset;
            public UInt64 startoffset;
            public int limit;
            public PlotFile src;
            public BFSPlotFile tar;
            public Scoop scoop1;
            public Scoop scoop2;
            public Scoop scoop3;
            public Scoop scoop4;
            public bool shuffle;
            public long end;
            public int startOffset64;
            public int endOffset64;
        }

        //upload a file to a BFS formatted drive
        private void btn_upload_Click(object sender, EventArgs e)
        {
            String drive = drivesView.SelectedItems[0].SubItems[1].Text;
            Int32 PbytesPerSector = (Int32.Parse(drivesView.SelectedItems[0].SubItems[4].Text));
            halt1 = false;
            halt2 = false;
            Boolean shuffle = false;
            PlotFile temp;
            //Let user select plotfile
            string filter = "PoC2 plot files | " + tb_id.Text + "_*_*.*| PoC1 plot files|" + tb_id.Text + "_*_*_*.*";
            openFileDialog.Filter = filter;
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
            if (temp.stagger > 0 && temp.stagger != temp.nonces) return;

            //always convert PoC1 files
            if (temp.stagger == temp.nonces) shuffle = true;

            //check ID
            if (temp.id.ToString() != tb_id.Text) return;

            //calc startoffset and endoffset for 64 nonces alignment
            UInt64 startNonceR = temp.startNonce / 64 * 64;
            UInt64 startOffset64 = startNonceR < temp.startNonce ? startNonceR + 64 - temp.startNonce : 0;
            UInt32 endOffset64 = temp.nonces - (UInt32)startOffset64 - (temp.nonces-(UInt32)startOffset64) / 64 * 64;

            //check last file and offer to insert dummy file
            //Todo



            //Create file in bfsTOC
            int file = BFS.AddPlotFile(drive, temp.startNonce+startOffset64,temp.nonces - (UInt32)startOffset64 - endOffset64,2,0);
            if (file > -1)
            {
                FillBFSView(drive);
            }
            else
            {
                return;
            }

            //Get offsets
            startOffset = BFS.bfsTOC.plotFiles[file].startPos;
            scoopOffset = BFS.bfsTOC.diskspace / 4096;

            //transfer file
            //open source handle
            ScoopReadWriter reader; 
            reader = new ScoopReadWriter(openFileDialog.FileName);
            reader.OpenR(true);

            ScoopReadWriter writer; 
            writer = new ScoopReadWriter(drive);
            writer.OpenW();

            //Allocate memory
            int limit = 4096 * 4096/1024; //Write cache, 1MB 
            Scoop scoop1 = new Scoop(Math.Min((Int32)temp.nonces, limit));  //space needed for one partial scoop
            Scoop scoop2 = new Scoop(Math.Min((Int32)temp.nonces, limit));  //space needed for one partial scoop
            Scoop scoop3 = new Scoop(Math.Min((Int32)temp.nonces, limit));  //space needed for one partial scoop
            Scoop scoop4 = new Scoop(Math.Min((Int32)temp.nonces, limit));  //space needed for one partial scoop      

  
            //create masterplan     
            int loops = (int)Math.Ceiling((double)(temp.nonces) / limit);
            TaskInfo[] masterplan = new TaskInfo[2048 * loops];

            for (int y = 0; y < 2048; y++)
            {
                int zz = 0;
                //loop partial scoop               
                for (int z = (int)startOffset64; (ulong)z < temp.nonces-endOffset64; z += limit)
                {
                    masterplan[y * loops + zz] = new TaskInfo();
                    masterplan[y * loops + zz].reader = reader;
                    masterplan[y * loops + zz].writer = writer;
                    masterplan[y * loops + zz].drive = drive;
                    masterplan[y * loops + zz].file = file;
                    masterplan[y * loops + zz].y = y;
                    masterplan[y * loops + zz].z = z;
                    masterplan[y * loops + zz].x = y * loops + zz;
                    masterplan[y * loops + zz].limit = limit;
                    masterplan[y * loops + zz].src = temp;
                    masterplan[y * loops + zz].tar = BFS.bfsTOC.plotFiles[file];
                    masterplan[y * loops + zz].scoop1 = scoop1;
                    masterplan[y * loops + zz].scoop2 = scoop2;
                    masterplan[y * loops + zz].scoop3 = scoop3;
                    masterplan[y * loops + zz].scoop4 = scoop4;
                    masterplan[y * loops + zz].shuffle = shuffle;
                    masterplan[y * loops + zz].end = masterplan.LongLength;
                    masterplan[y * loops + zz].startOffset64 = (int)startOffset64;
                    masterplan[y * loops + zz].endOffset64 = (int)endOffset64;

                    zz += 1;
                }
            }

            //enable stats
            tbl_x.Visible = true;
            tbl_status.Visible = true;
            tbl_progress.Visible = true;
            tbl_progress.Maximum = 2048;
            var task1 = Task.Factory.StartNew(() => Th_copy(masterplan));

        }

        public void Th_copy(TaskInfo[] masterplan)
        {

            //execute taskplan in separate thread

            //initialise stats
            DateTime start = DateTime.Now;
            TimeSpan elapsed;
            TimeSpan togo;

            //perform first read
            Th_read(masterplan[0]);

            autoEvents = new AutoResetEvent[]
                {
                new AutoResetEvent(false),
                new AutoResetEvent(false)
                };

            //perform reads and writes parallel
            long x;
            for (x = 1; x < masterplan.LongLength; x++)
            {
                // ThreadPool.QueueUserWorkItem(new WaitCallback(Th_write), masterplan[x - 1]);
                //ThreadPool.QueueUserWorkItem(new WaitCallback(Th_read), masterplan[x]);
                //WaitHandle.WaitAll(autoEvents);

                var task1 = Task.Factory.StartNew(() => Th_write(masterplan[x - 1]));
                var task2 = Task.Factory.StartNew(() => Th_read(masterplan[x]));
                Task.WaitAll(task1, task2);

                if (halt1 || halt2)
                {
                    break;
                }

                //update status
                elapsed = DateTime.Now.Subtract(start);
                togo = TimeSpan.FromTicks(elapsed.Ticks / (masterplan[x].y + 1) * (2048 - masterplan[x].y - 1));
                string completed = Math.Round((double)(masterplan[x].y + 1) / 2048 * 100).ToString() + "%";
                string speed1 = Math.Round((double)masterplan[x].src.nonces / 4096 * 2 * (masterplan[x].y + 1) * 60 / (elapsed.TotalSeconds + 1)).ToString() + " nonces/m ";
                string speed2 = "(" + (Math.Round((double)masterplan[x].src.nonces / (2 << 12) * (masterplan[x].y + 1) / (elapsed.TotalSeconds + 1))).ToString() + "MB/s)";
                string speed = speed1 + speed2; lock (statusStrip)
                setStatus(masterplan[x].y + 1, "Completed: " + completed + ", Elapsed: " + TimeSpanToString(elapsed) + ", Remaining: " + TimeSpanToString(togo) + ", Speed: " + speed);

            }
            //perform last write
            if (!halt1 && !halt2) Th_write(masterplan[masterplan.LongLength - 1]);
            // close reader/writer
            masterplan[0].reader.Close();
            masterplan[0].writer.Close();


            if (halt1 || halt2)
            {
                setStatus(masterplan[x].y + 1, "Abort.");
            }


            //mark as finished
            if (!halt1 && !halt2) BFS.SetStatus(masterplan[masterplan.LongLength - 1].drive, masterplan[masterplan.LongLength - 1].file, 1);
            if (halt1 || halt2) BFS.SetPos(masterplan[masterplan.LongLength - 1].drive, masterplan[masterplan.LongLength - 1].file, (uint)x);
            if (!halt1 && !halt2) setStatus(masterplan[masterplan.LongLength - 1].y + 1, "Completed.");

        }

        private void setStatus(int progress, string text)
        {
            if (statusStrip.InvokeRequired)
            {
                statusStrip.Invoke(new MethodInvoker(() => { setStatus(progress, text); }));
            }
            else
            {
                tbl_progress.Value = progress;
                tbl_status.Text = text;
                Application.DoEvents();
            }
        }

        public static void Th_read(object stateInfo)
        {

            TaskInfo ti = (TaskInfo)stateInfo;

            //determine cache cycle and front scoop back scoop cycle to alternate
            if (ti.x % 2 == 0)
            {
                if (!halt1) halt1 = halt1 || !ti.reader.ReadScoop(ti.y, ti.src.nonces, ti.z, ti.scoop1, Math.Min((int)ti.src.nonces / 64 * 64 - ti.z, ti.limit));
                if (!halt1) halt1 = halt1 || !ti.reader.ReadScoop(4095 - ti.y, ti.src.nonces, ti.z, ti.scoop2, Math.Min((int)ti.src.nonces / 64 * 64 - ti.z, ti.limit));
                if (ti.shuffle) Poc1poc2shuffle(ti.scoop1, ti.scoop2, Math.Min((int)ti.src.nonces - ti.z, ti.limit));
            }
            else
            {
                if (!halt1) halt1 = halt1 || !ti.reader.ReadScoop(4095 - ti.y, ti.src.nonces, ti.z, ti.scoop4, Math.Min((int)ti.src.nonces / 64 * 64 - ti.z, ti.limit));
                if (!halt1) halt1 = halt1 || !ti.reader.ReadScoop(ti.y, ti.src.nonces, ti.z, ti.scoop3, Math.Min((int)ti.src.nonces / 64 * 64 - ti.z, ti.limit));
                if (ti.shuffle) Poc1poc2shuffle(ti.scoop3, ti.scoop4, Math.Min((int)ti.src.nonces - ti.z, ti.limit));
            }

        }

        public static void Th_write(object stateInfo)
        {

            TaskInfo ti = (TaskInfo)stateInfo;
            if (ti.x % 2 == 0)
            {
                if (!halt2) halt2 = halt2 || !ti.writer.WriteScoop(ti.y, (Int64)scoopOffset*64,ti.z, ti.scoop1, Math.Min((int)ti.src.nonces - ti.startOffset64 - ti.endOffset64 - ti.z, ti.limit),(Int64) startOffset*64*64);
                if (!halt2) halt2 = halt2 || !ti.writer.WriteScoop(4095 - ti.y, (Int64)scoopOffset * 64, ti.z, ti.scoop2, Math.Min((int)ti.src.nonces - ti.startOffset64 - ti.endOffset64 - ti.z, ti.limit), (Int64)startOffset * 64 * 64);
            }
            else
            {
                if (!halt2) halt2 = halt2 || !ti.writer.WriteScoop(4095 - ti.y, (Int64)scoopOffset * 64, (Int64)startOffset * 64, ti.scoop4, Math.Min((int)ti.src.nonces - ti.startOffset64 - ti.endOffset64 - ti.z, ti.limit), (Int64)startOffset * 64 * 64);
                if (!halt2) halt2 = halt2 || !ti.writer.WriteScoop(ti.y, (Int64)scoopOffset * 64, ti.z, ti.scoop3, Math.Min((int)ti.src.nonces - ti.startOffset64 - ti.endOffset64 - ti.z, ti.limit), (Int64)startOffset * 64 * 64);
            }
  
        }

        //Pretty Print Timespan
        private static string TimeSpanToString(TimeSpan timeSpan)
        {
            if (timeSpan.ToString().LastIndexOf(".") > -1)
            {
                return timeSpan.ToString().Substring(0, timeSpan.ToString().LastIndexOf("."));
            }
            else
            {
                return timeSpan.ToString();
            }
        }


        //Convert Poc1>Poc2 and vice versa
        private static void Poc1poc2shuffle(Scoop scoop1, Scoop scoop2, int limit)
        {
            byte[] buffer = new byte[32];
            for (int i = 0; i < limit; i++)
            {
                Buffer.BlockCopy(scoop1.byteArrayField, 64 * i + 32, buffer, 0, 32);
                Buffer.BlockCopy(scoop2.byteArrayField, 64 * i + 32, scoop1.byteArrayField, 64 * i + 32, 32);
                Buffer.BlockCopy(buffer, 0, scoop2.byteArrayField, 64 * i + 32, 32);
            }
        }

        private void btn_CreateEmptyPlotFile_Click(object sender, EventArgs e)
        {
            String drive = drivesView.SelectedItems[0].SubItems[1].Text;

            UInt64 sn = 0;
            UInt32 nonces = 0;
            //show dialog
            if (Dialogs.ShowInputDialog2(drive, "Create new plot file...", ref sn, ref nonces) == DialogResult.OK)
            {
                //create file
                if (BFS.AddPlotFile(drive, sn, nonces, 3, 0) > -1)
                {
                    FillBFSView(drive);
                }
                else
                {
                    MessageBox.Show("File Creation failed.\t\t\t", "Create file on" + drive, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

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

        private void drivesView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (drivesView.SelectedItems.Count > 0)
            {
                if (drivesView.SelectedItems[0].SubItems[8].Text == "Yes")
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
                                item.SubItems.Add("In Creation. ScoopPairs transferred: " + x.pos.ToString());
                                break;
                            case 3:
                                item.SubItems.Add("In Creation. Nonces plotted: " + x.pos.ToString());
                                break;
                            case 4:
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


        private void button1_Click_1(object sender, EventArgs e)
        {
            String drive = drivesView.SelectedItems[0].SubItems[1].Text;
            UInt64 id = 0;
            if (Dialogs.ShowInputDialog("Please enter numeric ID", ref id) == DialogResult.OK) {
                BFS.SetID(drive,id);
                //update ID
                tb_id.Text = id.ToString();
            }

        }

        private void btn_plot_Click(object sender, EventArgs e)
        {

        }

        private void tbl_x_Click(object sender, EventArgs e)
        {
            if (tbl_x.Visible)halt1 = true;
        }

        private void tbl_status_TextChanged(object sender, EventArgs e)
        {
            String drive = drivesView.SelectedItems[0].SubItems[1].Text;
            if (tbl_status.Text == "Completed.")
            {
                tbl_progress.Visible = false;
                tbl_x.Visible = false;
                FillBFSView(drive);
            }
        }
    }
}
