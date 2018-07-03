using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFS4WIN
{
    class Dialogs
    {
        public static DialogResult ShowInputDialog(string message, ref UInt64 input)
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
            textBox.TextAlign = HorizontalAlignment.Right;
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

        public static DialogResult ShowInputDialog2(string drive, string message, ref UInt64 input1, ref UInt32 input2)
        {
            //get boundaries
            BFS.LoadBFSTOC(drive);
            UInt32 totalNonces = (UInt32)(BFS.bfsTOC.diskspace / 64);

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
            nonces.Maximum = totalNonces - noncesUsed;
            nonces.Value = totalNonces - noncesUsed;
            nonces.TextAlign = HorizontalAlignment.Right;
            nonces.Increment = 64;
            inputBox.Controls.Add(nonces);

            //Trackbar 
            System.Windows.Forms.TrackBar trackbar = new TrackBar();
            trackbar.Size = new System.Drawing.Size(size.Width - 80, 45);
            trackbar.Location = new System.Drawing.Point(75, 63);
            trackbar.SmallChange = 64;
            trackbar.LargeChange = (int)(totalNonces - noncesUsed) / 64;
            trackbar.TickFrequency = trackbar.LargeChange;
            trackbar.Maximum = (int)(totalNonces - noncesUsed);
            trackbar.Value = (int)(totalNonces - noncesUsed);

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
    }
}
