using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DataManagement;

namespace GUITest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;
            openFileDialog1.Multiselect = false;

            DialogResult dr = this.openFileDialog1.ShowDialog();

            if (dr == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                EExecutionType execType = radioButton1.Checked ? EExecutionType.Predction : EExecutionType.Classification;

                bwLoadGrid.RunWorkerAsync(new BWLoadGridArgs(textBox1.Text, execType));
            }
        }

        private void bwLoadGrid_DoWork(object sender, DoWorkEventArgs e)
        {
            BWLoadGridArgs args = e.Argument as BWLoadGridArgs;
            if(args != null)
            {
                //DataProvider
            
            }

        }

        private void bwLoadGrid_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //DataProvider prov = new DataProvider(this.textBox1.Text, execType, -1);

            //this.uGridData1.LoadData(prov.DataSet);
        }
    }

    public class BWLoadGridArgs
    {
        public BWLoadGridArgs(string filePath, EExecutionType executionType)
        {
            FilePath = filePath;
            ExecutionType = executionType;
        }

        string FilePath;
        EExecutionType ExecutionType;
    }

}
