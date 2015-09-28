using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DataManagement;

namespace GUI
{
    public partial class UGridData : UserControl
    {
        public UGridData()
        {
            InitializeComponent();
        }

        public UGridData(Data[] dataSet)
            : this()
        {
            LoadData(dataSet);
        }

        private DataTable _DataTable;

        private const string INPUT = "input";
        private const string ENTRADA = "Entrada";
        private const string OUTPUT = "output";
        private const string SAIDA = "Saída";

        public void LoadData(Data[] dataSet)
        {
            int inputColumns = dataSet[0].Input.Length;
            int outputColumns = dataSet[0].Output.Length;

            _BuildGridColumns(inputColumns, outputColumns);
            _BuildDataSet(dataSet);
            this.grid.AutoResizeColumns();
        }

        private void _BuildGridColumns(int inputColumns, int outputColumns)
        {
            DataGridViewTextBoxColumn col = null;
            grid.Columns.Clear();
            
            _DataTable = new DataTable();
            
            for (int i = inputColumns; i >= 1; i--)
            {
                col = new DataGridViewTextBoxColumn();
                col.ReadOnly = true;
                col.Name = INPUT + i;
                col.HeaderText = ENTRADA + " " + i;
                col.DataPropertyName = INPUT + i;
                grid.Columns.Add(col);

                _DataTable.Columns.Add(INPUT + i, typeof(double));
            }

            for(int i = outputColumns; i >= 1; i--)
            {
                col = new DataGridViewTextBoxColumn();
                col.ReadOnly = true;                
                col.Name = OUTPUT + i;
                col.HeaderText = SAIDA + " " + i;
                col.DataPropertyName = OUTPUT + i;
                grid.Columns.Add(col);

                _DataTable.Columns.Add(OUTPUT + i, typeof(double));
            }


            
        }

        private void _BuildDataSet(Data[] dataSet)
        {
            DataRow dr = null;
            object[] item = null;
            for(int i = 0; i < dataSet.Length; i++)
            {
                dr = _DataTable.NewRow();
                item = new object[dataSet[i].Input.Length + dataSet[i].Output.Length];
                for (int j = 0; j < dataSet[i].Input.Length; j++)
                    item[j] = dataSet[i].Input[j];

                for (int j = dataSet[i].Input.Length, k = 0; j < dataSet[i].Input.Length + dataSet[i].Output.Length; j++, k++)
                    item[j] = dataSet[i].Output[k];

                dr.ItemArray = item;
                _DataTable.Rows.Add(dr);  
            }

            grid.DataSource = _DataTable;
        }
    }
}
