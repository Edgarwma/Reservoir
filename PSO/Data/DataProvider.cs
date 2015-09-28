using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace DataManagement
{
    /// <summary>
    /// Provedor de dados
    /// </summary>
    public class DataProvider
    {
        /// <summary>
        /// Base de dados
        /// </summary>
        public Data[] DataSet 
        {
            get;
            private set; 
        }

        public int DataSetLines;
        public int DataSetColumns;
        /// <summary>
        /// Número de entradas
        /// </summary>
        public int InputsN;
        /// <summary>
        /// Número de saídas
        /// </summary>
        public int OutputsN;

        public Data[] TrainSet;
        public int TrainSetLines;
        public int TrainSetColumns;

        public Data[] ValidationSet;
        public int ValidationSetLines;
        public int ValidationSetColumns;

        public Data[] TestSet;
        public int TestSetlines;
        public int TestSetColumns;
        
        private Random _Random = null;
        public EExecutionType ExecutionType;
        public double[] MaxValue = null;
        public double[] MinValue = null;

        private bool _LogAppliedToData = false;
        private bool _DataNormalized = false;

        private double _InputBoundA;
        private double _InputBoundB;
        private double _OutputBoundA;
        private double _OutputBoundB;
        private double _TrainSize = 0;
        private double _ValidationSize = 0;
        private double _TestSize = 0;

        public DataProvider(Data[] dataSet, EExecutionType executionType, int seed, double trainSize, double validationSize, double testSize)
        {
            _Random = new Random(seed);
            ExecutionType = executionType;
            DataSet = dataSet;
            DataSetLines = dataSet.Length;
            DataSetColumns = dataSet[0].Input.Length + dataSet[0].Output.Length;
            InputsN = dataSet[0].Input.Length;
            OutputsN = dataSet[0].Output.Length;

            MinValue = new double[DataSetColumns];
            MaxValue = new double[DataSetColumns];

            _TrainSize = trainSize;
            _ValidationSize = validationSize;
            _TestSize = testSize;

            if ((_TrainSize + _ValidationSize + _TestSize) != 1)
                throw new ArgumentException("A soma do percentual de particionamento dos dados deve ser 100%");
        }

        public DataProvider(String filePath, EExecutionType dataType, int seed, double trainSize, double validationSize, double testSize)
        {
            ExecutionType = dataType;
            _Random = new Random(seed);
           
            _ReadDataSetFromFile(filePath);

            _TrainSize = trainSize;
            _ValidationSize = validationSize;
            _TestSize = testSize;

            if ((_TrainSize + _ValidationSize + _TestSize) != 1)
                throw new ArgumentException("A soma do percentual de particionamento dos dados deve ser 100%");
        }

        public void ShuffleDataSet()
        {
            List<Data> list = new List<Data>();
            TrainSetLines = (int)(DataSetLines * _TrainSize); // 50% dos dados		
            ValidationSetLines = (int)(DataSetLines * _ValidationSize); // 25% dos dados
            TestSetlines = DataSetLines - (TrainSetLines + ValidationSetLines); // 25% dos dados

            for (int i = 0; i < TrainSetLines + ValidationSetLines; i++)
                list.Add(DataSet[i].Clone());

            list.Shuffle(_Random);

            for (int i = 0; i < TrainSetLines + ValidationSetLines; i++)
                DataSet[i] = list[i].Clone();
        }

        public void SplitData()
        {
            TrainSetLines = (int)(DataSetLines * _TrainSize); // 50% dos dados		
            ValidationSetLines = (int)(DataSetLines * _ValidationSize); // 25% dos dados
            TestSetlines = DataSetLines - (TrainSetLines + ValidationSetLines); // 25% dos dados

            TestSetColumns = ValidationSetColumns = TrainSetColumns = DataSetColumns; // número de entradas + saída		

            TrainSet = new Data[TrainSetLines];
            ValidationSet = new Data[ValidationSetLines];
            TestSet = new Data[TestSetlines];
                    
            for (int i = 0; i < TrainSetLines; i++)
                TrainSet[i] = DataSet[i].Clone();
            
            for (int i = TrainSetLines, l = 0; i < TrainSetLines + ValidationSetLines; i++, l++)
                ValidationSet[l] = DataSet[i].Clone();

            for (int i = TrainSetLines + ValidationSetLines, l = 0; i < DataSetLines; i++, l++)
                TestSet[l] = DataSet[i].Clone();
        }

        public DataProvider ShrinkProvider(int inputs)
        {
            DataProvider shrinkProv = null;

            if (inputs < InputsN)
            {
                Data[] set = new Data[DataSetLines];

                for (int i = 0; i < DataSetLines; i++)
                {
                    set[i] = new Data(new double[inputs], DataSet[i].RealOutput.Clone() as double[]);

                    for (int j = inputs - 1, k = InputsN - 1; j >= 0; j--, k--)
                    {
                        set[i].RealInput[j] = DataSet[i].RealInput[k];
                    }
                }

                shrinkProv = new DataProvider(set, ExecutionType, _Random.Next(), _TrainSize, _ValidationSize, _TestSize);

                if (_LogAppliedToData)
                    shrinkProv.ApplyLogToData();

                if (_DataNormalized)
                    shrinkProv.NormalizeData(_InputBoundA, _InputBoundB, _OutputBoundA, _OutputBoundB);

                shrinkProv.ShuffleDataSet();
                shrinkProv.SplitData();
            }
            else if (inputs == InputsN)
                shrinkProv = this;
            else
                throw new ArgumentOutOfRangeException();

            return shrinkProv;
        }

        public void ApplyLogToData()
        {
            if (_LogAppliedToData)
                throw new InvalidOperationException("Os dados já sofreram tranformação logarítmica");

            for (int i = 0; i < DataSetLines; i++)
            {
                for (int j = 0; j < DataSet[0].Input.Length; j++)
                {
                    if (DataSet[i].RealInput[j] == 0)
                        throw new ArgumentException("Parâmetro inválido na transformação logarítimica");

                    DataSet[i].Input[j] = Math.Log(DataSet[i].RealInput[j]);
                }
                

                for (int j = 0; j < DataSet[0].Output.Length; j++)
                {
                    if(DataSet[0].RealOutput[j] == 0)
                        throw new ArgumentException("Parâmetro inválido na transformação logarítimica");

                    DataSet[i].Output[j] = Math.Log(DataSet[i].RealOutput[j]);
                }
            }

            _LogAppliedToData = true;
        }

        public void NormalizeData(double inputBoundA, double inputBoundB, double outputBoundA, double outputBoundB)
        {
            if (_DataNormalized)
                throw new InvalidOperationException("Os dados já foram normalizados");

            _InputBoundA = inputBoundA;
            _InputBoundB = inputBoundB;
            _OutputBoundA = outputBoundA;
            _OutputBoundB = outputBoundB;
            
            double val = 0;

            MinValue = new double[DataSetColumns];
            for (int i = 0; i < MinValue.Length; i++)
                MinValue[i] = double.MaxValue;

            MaxValue = new double[DataSetColumns];
            for (int i = 0; i < MaxValue.Length; i++)
                MaxValue[i] = double.MinValue;


            // Determinando valores máximos e minimos de cada coluna da base de dados
            for(int i = 0; i < DataSetLines; i++)
            {
			    for (int j = 0; j < InputsN + OutputsN; j++) 
                {
                    if (j >= InputsN)
                    {
                        val = DataSet[i].Output[j - InputsN];

                        if (val > MaxValue[j])
                            MaxValue[j] = val;

                        if (val < MinValue[j])
                            MinValue[j] = val;
                    }
                    else
                    {                       
                        val = DataSet[i].Input[j];
                        
                        if (val > MaxValue[j])
                            MaxValue[j] = val;

                        if (val < MinValue[j])
                            MinValue[j] = val;
                    }
			    }
		    }
            
            for (int i = 0; i < DataSetLines; i++)
            {
                for (int j = 0; j < DataSet[0].Input.Length; j++)
                {
                    DataSet[i].Input[j] = _Normalize(DataSet[i].Input[j], _InputBoundA, _InputBoundB, j);
                    if (double.IsNaN(DataSet[i].Input[j]) || Math.Round(DataSet[i].Input[j], 2) < _InputBoundA || Math.Round(DataSet[i].Input[j], 2) > _InputBoundB)
                        throw new ArgumentOutOfRangeException("O valor após normalização ficou fora dos limites estabelecidos");
                }

                for (int j = 0; j < DataSet[0].Output.Length; j++)
                {
                    DataSet[i].Output[j] = _Normalize(DataSet[i].Output[j], _OutputBoundA, _OutputBoundB, (int)(j + InputsN));
                    if (double.IsNaN(DataSet[i].Output[j]) || Math.Round(DataSet[i].Output[j], 2) < _OutputBoundA || Math.Round(DataSet[i].Output[j], 2) > _OutputBoundB)
                        throw new ArgumentOutOfRangeException("O valor após normalização ficou fora dos limites estabelecidos");
                }
            }

            _DataNormalized = true;
        }

        public double DeNormalizeOutputData(double value, int columnIndex)
        {
            if (!_DataNormalized)
                throw new InvalidOperationException("Os valores precisam estar normalizados para realizar esta operação");

            double deNormalized = 0;         

            columnIndex += this.InputsN;

            double dividend = (value - _OutputBoundA) * (MaxValue[columnIndex] - MinValue[columnIndex]);
            double divisor = (_OutputBoundB - _OutputBoundA) == 0 ? Double.Epsilon : (_OutputBoundB - _OutputBoundA);

            deNormalized = (dividend / divisor) + MinValue[columnIndex];

            if (_LogAppliedToData)
                deNormalized = _RemoveLogFromValue(deNormalized);

            return deNormalized;
        }

        private double _RemoveLogFromValue(double value)
        {
            return Math.Pow(Math.E, value);
        }

        private double _Normalize(double value, double a, double b, int columnIndex)
        {
            double normalized = 0;
            double dividend = (b - a) * (value - MinValue[columnIndex]);
            double divisor = (MaxValue[columnIndex] - MinValue[columnIndex]) == 0 ? Double.Epsilon : (MaxValue[columnIndex] - MinValue[columnIndex]);

            normalized = (dividend / divisor) + a;
            return normalized;
        }

        private void _ReadDataSetFromFile(String filePath)
        {
            String line = null;
            using (StreamReader reader = new StreamReader(filePath))
            {
		        line = reader.ReadLine();
	            DataSetLines = int.Parse(line); // número de exemplos
		
		        line = reader.ReadLine();
		        DataSetColumns = int.Parse(line); // representa o número de entradas + número de saidas
		
		        line = reader.ReadLine();
		        InputsN = int.Parse(line);; // número de entradas da rede
		
		        line = reader.ReadLine();
		        OutputsN = int.Parse(line);; // número de saídas da rede
		
		        DataSet = new Data[DataSetLines];               

		        for(int i = 0; i < DataSetLines; i++)
                {
			        line = reader.ReadLine();

			        String[] values = line.Split(';');

			        DataSet[i] = new Data(new double[InputsN], new double[OutputsN]);
			
			        for (int j = 0; j < values.Length; j++) 
                    {
					    double val = double.Parse(values[j]);

                        if (double.IsNaN(val))
                            throw new Exception();

                        if (j >= InputsN)
                        {
                            DataSet[i].RealOutput[j - InputsN] = val;
                            DataSet[i].Output[j - InputsN] = val;
                        }
                        else
                        {
                            DataSet[i].RealInput[j] = val;
                            DataSet[i].Input[j] = val;
                        }
			        }
		        }
            }
	    }

        public void SaveToFile(string path)
        {
            StreamWriter file = new System.IO.StreamWriter(path+"_train", true);
            
            string line = string.Empty;
            for (int i = 0; i < TrainSet.Length; i++)
			{
                line = string.Empty;
                for (int j = 0; j < TrainSet[0].Output.Length; j++)
                {
                    line += TrainSet[i].Output[j] + "; "; 
                }

                for (int j = 0; j < TrainSet[0].Input.Length; j++)
                {
                    line += TrainSet[i].Input[j] + "; ";
                }
                file.WriteLine(line); 
			}
            file.Close();

            file = new System.IO.StreamWriter(path+"_test", true);
            for (int i = 0; i < ValidationSet.Length; i++)
            {
                line = string.Empty;
                for (int j = 0; j < ValidationSet[0].Output.Length; j++)
                {
                    line += ValidationSet[i].Output[j] + "; ";
                }

                for (int j = 0; j < ValidationSet[0].Input.Length; j++)
                {
                    line += ValidationSet[i].Input[j] + "; ";
                }
                file.WriteLine(line);
            }
            file.Close();           
        }
    }
}
