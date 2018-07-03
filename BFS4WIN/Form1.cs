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
                //item.SubItems.Add(llda.GetTotalSectors(i).ToString());
                Int64 sectors = llda.GetSectors(x)/512;
                item.SubItems.Add(sectors.ToString());
                item.SubItems.Add(llda.BytesPerSector(i).ToString());
                item.SubItems.Add(llda.GetCaption(i));
                item.SubItems.Add(((decimal)sectors * llda.BytesPerSector(i) / 1024/1024/1024).ToString("0.00")+" GiB");
                item.SubItems.Add(((long)(sectors-3) * llda.BytesPerSector(i) / 4096 / 64).ToString());
                //
                if (BFS.isBFS(x))
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
            //listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
           // listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }


        private void btn_format_Click(object sender, EventArgs e)
        {
            if (drivesView.SelectedItems.Count > 0)
            {
                //Warning
                DialogResult result = MessageBox.Show("WARNING: Formatting will erase ALL data on this disk." + "\n" + "To format the disk, press OK. To quit, click CANCEL.", "Format Local Disk ( "+ drivesView.SelectedItems[0].SubItems[1].Text + ")", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Cancel) return;

                UInt32 bytesPerSector = (UInt32.Parse(drivesView.SelectedItems[0].SubItems[3].Text));
                UInt64 totalSectors = (UInt64.Parse(drivesView.SelectedItems[0].SubItems[2].Text));
                String drive = drivesView.SelectedItems[0].SubItems[1].Text;

                //Format Drive using GPT, MBR would only make sense for small drives and small drives dont make sense for Burst
                BFS.FormatDriveGPT(drive, totalSectors, bytesPerSector);
                //Success
                MessageBox.Show("Format Complete.                   ", "Formatting " + drivesView.SelectedItems[0].SubItems[1].Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            int position = bfsTOC.AddPlotFile(temp.id, temp.startNonce, temp.nonces/64*64, 2, 0);
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
                    masterplan[y * loops + zz].tar = bfsTOC.plotFiles[position];
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

        private void button8_Click(object sender, EventArgs e)
        {
            //Read current bfsTOC
            BFSTOC bfsTOC = BFSTOC.FromSector(llda.ReadSector(drivesView.SelectedItems[0].SubItems[1].Text, 0, 4096));
            //Check CRC32
            //if fail try Mirror
            //if not fail, check if mirror is identical
            //if not copy over
            //count current files
            bfsTOC.AddPlotFile(13014439754249942082, 1857887936, 0, 2, 0);
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
            //Read current bfsTOC
            BFSTOC bfsTOC = BFSTOC.FromSector(llda.ReadSector(drivesView.SelectedItems[0].SubItems[1].Text, 5, 4096));
            //update bfsTOC
            int position = bfsTOC.AddPlotFile(1234, 0, 10000 / 64 * 64, 2, 0);
            if (position == -1) return;
            //save bfsTOC
            llda.WriteSector(drivesView.SelectedItems[0].SubItems[1].Text, 5, 4096, bfsTOC.ToByteArray());
            FillBFSView(drivesView.SelectedItems[0].SubItems[1].Text);

        }

        private void btn_deleteFile_Click(object sender, EventArgs e)
        {
            //Read current bfsTOC
            BFSTOC bfsTOC = BFSTOC.FromSector(llda.ReadSector(drivesView.SelectedItems[0].SubItems[1].Text, 1, 4096));
            //update bfsTOC if delete success
            if (bfsTOC.DeleteLastPlotFile()) llda.WriteSector(drivesView.SelectedItems[0].SubItems[1].Text, 1, 4096, bfsTOC.ToByteArray());
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


        private void FillBFSView(string drive)
        {
            if (drivesView.SelectedItems.Count > 0)
            {
                BFS.loadBFSTOC(drive);
                //Get BFS Infos
                tb_version.Text = Encoding.ASCII.GetString(BFS.bfsTOC.version);
                tb_capa1.Text = ((decimal)BFS.bfsTOC.diskspace * 4096 / 1024 / 1024 / 1024).ToString("0.00");
                tb_capa2.Text = ((decimal)BFS.bfsTOC.diskspace / 64).ToString("0");

                bfsView.Items.Clear();
                int i = 0;
                foreach (PlotFile x in BFS.bfsTOC.plotFiles)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = i.ToString();
                    item.Name = i.ToString();
                    if (x.id == 0)
                    {
                        item.SubItems.Add("Empty Slot");
                    }
                    else
                    {
                        item.SubItems.Add(x.id.ToString());
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
            }
        }

        private void ClearBFSView()
        {
            bfsView.Items.Clear();
            tb_version.Text = "";
            tb_capa1.Text = "";
            tb_capa2.Text = "";


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
    }
}
