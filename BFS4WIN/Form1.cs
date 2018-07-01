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
            listView1.Items.Clear();
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
                listView1.Items.Add(item);
                i += 1;
            }
            //listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
           // listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                BFSTOC bfsTOC = BFSTOC.FromSector(llda.ReadSector(listView1.SelectedItems[0].SubItems[1].Text, 5, 4096));
                //Get BFS Infos
                tb_version.Text = Encoding.ASCII.GetString(bfsTOC.version);
                tb_capa1.Text = ((decimal)bfsTOC.diskspace * 4096 / 1024 / 1024 / 1024).ToString("0.00");
                tb_capa2.Text = ((decimal)bfsTOC.diskspace / 64).ToString("0");

                listView2.Items.Clear();
                int i = 0;
                foreach (PlotFile x in bfsTOC.plotFiles)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = i.ToString();
                    item.Name = i.ToString();
                    if (x.id == 0) { item.SubItems.Add("Empty Slot");
                    } else
                    {
                        item.SubItems.Add(x.id.ToString());
                        item.SubItems.Add(x.startNonce.ToString());
                        item.SubItems.Add(x.nonces.ToString());
                        item.SubItems.Add(x.startPos.ToString());
                        switch(x.status){
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
                    listView2.Items.Add(item);
                    i += 1;
                }                        
             
            textBox1.Text = string.Join(" ", bfsTOC.ToByteArray().Select(x => x.ToString("X2")));

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                Boolean usegpt = false;
                UInt32 bytesPerSector = (UInt32.Parse(listView1.SelectedItems[0].SubItems[3].Text));
                UInt64 totalSectors = (UInt64.Parse(listView1.SelectedItems[0].SubItems[2].Text));

                //TODO check if drive is a virgin, deny formatting if not


                //TODO check GPT style for drives >2TB
                usegpt = true;


                if (usegpt)
                {
                    //create protective MBR
                    //create GPT
                    GPT gpt = new GPT(totalSectors, bytesPerSector);
                    byte[] test = gpt.ToByteArray();

                    //create BFSTOC
                    BFSTOC bfsTOC = BFSTOC.emptyToc(totalSectors, bytesPerSector, true);
                    //write GPT
                    llda.WriteSector(listView1.SelectedItems[0].SubItems[1].Text, 0, 4096*5, gpt.ToByteArray());
                    //write MirrorGPT
                    llda.WriteSector(listView1.SelectedItems[0].SubItems[1].Text, (Int64)totalSectors - 5, 4096 * 5, gpt.ToByteArray());
                    //write  BFSTOC
                    llda.WriteSector(listView1.SelectedItems[0].SubItems[1].Text, 5, 4096, bfsTOC.ToByteArray());
                    //TODO write mirror BFSTOC                   
                    //trigger OS partition table re-read
                    llda.refreshDrive(listView1.SelectedItems[0].SubItems[1].Text);
                    //Debug (to be removed)
                    textBox1.Text = string.Join(" ", gpt.ToByteArray().Select(x => x.ToString("X2")));
                    textBox1.Text += string.Join(" ", bfsTOC.ToByteArray().Select(x => x.ToString("X2")));
               

            }
                else
                {
                    //create classic MBR
                    MBR mbr = new MBR((UInt32)totalSectors, bytesPerSector,false);
                    //inflate to 4k
                    Array.Resize(ref mbr.mbr, 4096);
                    //create BFSTOC
                    BFSTOC bfsTOC = BFSTOC.emptyToc(totalSectors, bytesPerSector,false);
                    //write MBR
                    llda.WriteSector(listView1.SelectedItems[0].SubItems[1].Text, 0, 4096, mbr.ToByteArray());
                    //write  BFSTOC
                    llda.WriteSector(listView1.SelectedItems[0].SubItems[1].Text, 1, 4096, bfsTOC.ToByteArray());
                    //TODO write mirror BFSTOC                   
                    //trigger OS partition table re-read
                    llda.refreshDrive(listView1.SelectedItems[0].SubItems[1].Text);
                    //Debug (to be removed)
                    textBox1.Text = string.Join(" ", mbr.ToByteArray().Select(x => x.ToString("X2")));
                    textBox1.Text += string.Join(" ", bfsTOC.ToByteArray().Select(x => x.ToString("X2")));
                }
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
            BFSTOC bfsTOC = BFSTOC.FromSector(llda.ReadSector(listView1.SelectedItems[0].SubItems[1].Text, 1, 4096));
            //update bfsTOC
            int position = bfsTOC.AddPlotFile(temp.id, temp.startNonce, temp.nonces/64*64, 2, 0);
            if (position == -1) return;
            //save bfsTOC
            llda.WriteSector(listView1.SelectedItems[0].SubItems[1].Text, 1, 4096, bfsTOC.ToByteArray());
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
                    masterplan[y * loops + zz].target = listView1.SelectedItems[0].SubItems[1].Text;
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
            llda.WriteSector(listView1.SelectedItems[0].SubItems[1].Text, 0, 4096, bfsTOC.ToByteArray());
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //Read current bfsTOC
            BFSTOC bfsTOC = BFSTOC.FromSector(llda.ReadSector(listView1.SelectedItems[0].SubItems[1].Text, 0, 4096));
            //Check CRC32
            //if fail try Mirror
            //if not fail, check if mirror is identical
            //if not copy over
            //count current files
            bfsTOC.AddPlotFile(13014439754249942082, 1857887936, 0, 2, 0);
            llda.WriteSector(listView1.SelectedItems[0].SubItems[1].Text, 0, 4096, bfsTOC.ToByteArray());
            textBox1.Text = string.Join(" ", bfsTOC.ToByteArray().Select(x => x.ToString("X2")));
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
            BFSTOC bfsTOC = BFSTOC.FromSector(llda.ReadSector(listView1.SelectedItems[0].SubItems[1].Text, 5, 4096));
            //update bfsTOC
            int position = bfsTOC.AddPlotFile(1234, 0, 10000 / 64 * 64, 2, 0);
            if (position == -1) return;
            //save bfsTOC
            llda.WriteSector(listView1.SelectedItems[0].SubItems[1].Text, 5, 4096, bfsTOC.ToByteArray());

        }

        private void btn_deleteFile_Click(object sender, EventArgs e)
        {
            //Read current bfsTOC
            BFSTOC bfsTOC = BFSTOC.FromSector(llda.ReadSector(listView1.SelectedItems[0].SubItems[1].Text, 1, 4096));
            //update bfsTOC if delete success
            if (bfsTOC.DeleteLastPlotFile()) llda.WriteSector(listView1.SelectedItems[0].SubItems[1].Text, 1, 4096, bfsTOC.ToByteArray());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                GPT gpt = GPT.FromSectors(llda.ReadSector(listView1.SelectedItems[0].SubItems[1].Text, 0, 4096*5));
                gpt.gptHeader.UpdateCRC32(CRC.CRC32(gpt.gptPartitionTable.ToByteArray()));

             

            }
        }
    }
}
