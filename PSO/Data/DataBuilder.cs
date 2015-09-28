using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DataManagement
{
    public class DataBuilder
    {
        public DataBuilder(string timeSeriesFilePath)
        {
            _ReadTimeSeries(timeSeriesFilePath);
        }

        public DataBuilder(List<double>timeSeries)
        {
            _TimeSeries = timeSeries;
        }

        private List<double> _TimeSeries;
        private double[][] _Input;
        private double[][] _Output;

        /// <summary>
        /// Defasa os dados da série temporal
        /// Para as entradas, faz a defasagem de "inputLags" para trás (Ex: -1, -2, -3... valores para trás)
        /// Para a saída, faz a defasagem de "outputLags" a frente com apenas um valor (EX: saída = +1 ou +2 ou +3... valor a frente) 
        /// </summary> 
        /// <param name="inputLags"></param>
        /// <param name="outputLags"></param>
        public void LagData(int inputLags, int outputLags)
        {
            _Input = _Input.InitializeMatrix(_TimeSeries.Count - inputLags - outputLags, inputLags);
            _Output = _Output.InitializeMatrix(_TimeSeries.Count - inputLags - outputLags, outputLags);

            for (int i = 0; i < _TimeSeries.Count - inputLags - outputLags; i++)
            {
                for (int j = 0; j < inputLags; j++)
                    _Input[i][j] = _TimeSeries[i + j];

                for (int k = 0 ; k < outputLags; k++)
                    _Output[i][k] = _TimeSeries[i + inputLags + k];
            }
        }

        public Data[] BuildDataSet()
        {
            if (_Input == null || _Output == null)
                throw new ArgumentNullException("Os dados entrada/saída precisam ser preenchidos");
            else if (_Input.Length != _Output.Length)
                throw new ArgumentOutOfRangeException("A quantidade de elementos de entrada/saída precisam ser iguais");

            Data[] dataSet = new Data[_Input.Length];
            Data ex = null;
            for (int i = 0; i < _Input.Length; i++)
            {
                ex = new Data(_Input[i], _Output[i]);
                dataSet[i] = ex;
            }
            return dataSet;
        }

        private void _ReadTimeSeries(string filePath)
        {
            String line = null;
            _TimeSeries = new List<double>();
            double val = double.NaN;

            using (StreamReader reader = new StreamReader(filePath))
            {
                do
                {
                    line = reader.ReadLine();

                    double.TryParse(line, out val);

                    if (double.IsNaN(val))
                        throw new Exception();

                    _TimeSeries.Add(val);
                    val = double.NaN;
                } 
                while (!string.IsNullOrWhiteSpace(line));
            }
        }

        public void SaveToFile(string filePath)
        {
            Data[] set = BuildDataSet();
            StreamWriter file = new System.IO.StreamWriter(filePath, false);

            file.WriteLine(set.Length);
            file.WriteLine(set[0].Input.Length + set[0].Output.Length);
            file.WriteLine(set[0].Input.Length);
            file.WriteLine(set[0].Output.Length);

            string line = string.Empty;
            for (int i = 0; i < set.Length; i++)
            {
                line = string.Empty;
                for (int j = 0; j < set[0].Input.Length; j++)
                {
                    line += set[i].Input[j] + "; ";
                }
                for (int j = 0; j < set[0].Output.Length; j++)
                {
                    line += set[i].Output[j] + "; ";
                }
                line = line.Substring(0, line.Length - 2);
                file.WriteLine(line);
            }
            file.Close();
        }
    }
}
