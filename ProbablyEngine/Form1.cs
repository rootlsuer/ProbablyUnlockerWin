using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

using MemoryControl;

namespace ProbablyEngine
{
    public partial class Form1 : Form
    {

        private int SelectedProcessID;
        private List<Processes> dataSource;

        public class Processes
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        long address = (long) 0x7B2FC8;
        byte[] patch = { 0xEB, 0x5A };

        public Form1()
        {
            InitializeComponent();
        }

        private void OnApplicationExit(object sender, EventArgs e) {
        }

        private void populateList()
        {
            Process[] processes = Process.GetProcessesByName("wow-64");
            if (processes.Length > 0)
            {
                dataSource = new List<Processes>();
                foreach (var process in processes)
                {
                    dataSource.Add(
                        new Processes()
                        {
                            Id = process.Id,
                            Name = String.Format("{0}.exe - {1}", process.ProcessName, process.Id)
                        }
                    );
                }
                comboBox1.DataSource = dataSource;
                comboBox1.DisplayMember = "Name";
                comboBox1.ValueMember = "Id";
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox1.Enabled = true;
            }
            else
            {
                comboBox1.Enabled = false;
                comboBox1.Text = "Game Not Open";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            populateList();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            populateList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // open the process
            MemC.cOpenProcessId(SelectedProcessID);

            // rebase the address for the real pointer
            long rebase = (long)Process.GetProcessById(SelectedProcessID).MainModule.BaseAddress + address;

            // pre-read before patch to check if we're patched or not
            byte[] preRead = MemC.readXBytes(rebase, 2);

            Console.WriteLine("Dump: " + BitConverter.ToString(preRead));
            if (preRead[0] == patch[0] && preRead[1] == patch[1])
            {
                statusText.Text = "Already Patched!";
            }
            else
            {
                // write patch
                MemC.WriteXBytes(rebase, patch);
                // post read to check if we patched
                byte[] postRead = MemC.readXBytes(rebase, 2);
                if (postRead[0] == patch[0] && postRead[1] == patch[1])
                {
                    statusText.Text = "Sucessfully Patched!";
                }
                else
                {
                    statusText.Text = "Unable to verify patch.";
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedProcessID = dataSource[comboBox1.SelectedIndex].Id;
            button2.Enabled = true;
        }

        private void statusText_Click(object sender, EventArgs e)
        {

        }
    }
}
