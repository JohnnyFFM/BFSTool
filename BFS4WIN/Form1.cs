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

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ArrayList result;
            result = llda.GetDriveList();
            int i = 0;
            foreach (string x in result)
            {
                ListViewItem item = new ListViewItem();
                item.Text = i.ToString();
                item.Name = i.ToString();
                item.SubItems.Add(x);
                item.SubItems.Add(llda.BytesPerSector(i).ToString());

                item.SubItems.Add(llda.GetTotalSectors(i).ToString());
                item.SubItems.Add(llda.GetCaption(i));
                item.SubItems.Add(((long)llda.GetTotalSectors(i)* llda.BytesPerSector(i)/1024/1024/1024).ToString()+" GiB");
                item.SubItems.Add(((long)(llda.GetTotalSectors(i)-1) * llda.BytesPerSector(i) / 4096 / 64).ToString());
                listView1.Items.Add(item);
                i += 1;
            }
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                BFSTOC bfsTOC = BFSTOC.FromSector(llda.ReadSector(listView1.SelectedItems[0].SubItems[1].Text, 0, 4096));
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
                        item.SubItems.Add(x.stagger.ToString());
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
                UInt32 bytesPerSector = (UInt32.Parse(listView1.SelectedItems[0].SubItems[2].Text));
                UInt64 totalSectors = (UInt64.Parse(listView1.SelectedItems[0].SubItems[3].Text));
                byte[] data = BFSTOC.emptyToc(totalSectors,bytesPerSector).ToByteArray();
                llda.WriteSector(listView1.SelectedItems[0].SubItems[1].Text, 0, 4096,data);
                textBox1.Text = string.Join(" ", data.Select(x => x.ToString("X2")));
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Read current bfsTOC
            BFSTOC bfsTOC = BFSTOC.FromSector(llda.ReadSector(listView1.SelectedItems[0].SubItems[1].Text, 0, 4096));
            //Check CRC32
            //if fail try Mirror
            //if not fail, check if mirror is identical
            //if not copy over
            //count current files
            bfsTOC.AddPlotFile(13014439754249942082, 1857887936, 1887936, 0, 2, 0);
            llda.WriteSector(listView1.SelectedItems[0].SubItems[1].Text, 0, 4096, bfsTOC.ToByteArray());
            textBox1.Text = string.Join(" ", bfsTOC.ToByteArray().Select(x => x.ToString("X2")));

        }
    }
}
