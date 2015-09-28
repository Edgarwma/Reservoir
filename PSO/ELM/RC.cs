using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jama;
using Jama.SVD;
using ReservoirComputing.Configuration;
using DataManagement;

namespace ReservoirComputing
{
    public class RC : IDisposable
    {
        #region Constructor(s)
        #region Train
        public RC(Data[] dataSet, Data[] valSet, Data[] testSet, RCConfiguration config)
            : this(dataSet, valSet, testSet, config.HidenNodesNumber, config.WarmUpCicles, config.Interconnectivity, config.SpectralRadious, config.Seed, config.ActivationFunctionType)
        {
            _Initialize();
        }       
        #endregion

        #region Execution
        public RC(Data[] dataSet, Data[] valSet, Data[] testSet, RCRunnableConfiguration config)
            : this(dataSet, valSet, testSet, config.HidenNodesNumber, config.ActivationFunctionType, config.B, config.w, config.I, config.H0)
        {
        }
        private RC(Data[] dataSet, Data[] valSet, Data[] testSet, int hidenNodesNumber, ERCActivationFunctionType activationFunctionType, double[][] B, double[][] w, double[][] I, double[] H0)
            : this(dataSet, valSet, testSet, hidenNodesNumber, 1, 0, 0, 0, activationFunctionType)
        {
            this._B = B;
            this._w = w;
            this._I = I;
            this._H0 = H0;

            _InitializeForExecution();
        }        
        #endregion

        private RC(Data[] trainSet, Data[] valSet, Data[] testSet, int hidenNodesNumber, int warmUpCicles, double interConnectivity, double spectralRadious, int seed, ERCActivationFunctionType activationFunctionType)
        {
            _TrainSet = trainSet;
            _ValidationSet = valSet;
            _TestSet = testSet;
            _ActivationFunctionType = activationFunctionType;
            _Ñ = hidenNodesNumber;
            _WarmUpCicles = warmUpCicles;
            _InterConnectivity = interConnectivity;
            _SpectralRadious = spectralRadious;
            _Seed = seed;
            _Random = new Random(_Seed);
        }
        #endregion

        #region Attributes
        /// <summary>
        /// Matriz com as entradas de treinamento para a fase de Treinamento.
        /// Tamanho TrainNxn
        /// </summary>
        private double[][] _Trainx = null;

        /// <summary>
        /// Matriz com as entradas de treinamento para a fase de Validação.
        /// Tamanho ValidationNxn
        /// </summary>
        private double[][] _Validationx = null;

        /// <summary>
        /// Matriz com as entradas de treinamento para a fase de Teste.
        /// Tamanho TesteNxn
        /// </summary>
        private double[][] _Testx = null;

        /// <summary>
        /// Matriz com as saídas da rede na fase de Treinamento.
        /// Tamanho TrainNxm
        /// </summary>
        private double[][] _TrainT = null;

        /// <summary>
        /// Matriz com as saídas da rede na fase de Validação.
        /// Tamanho ValidationNxm
        /// </summary>
        private double[][] _ValidationT = null;

        /// <summary>
        /// Matriz com as saídas da rede na fase de Teste.
        /// Tamanho ValidationNxm
        /// </summary>
        private double[][] _TestT = null;

        /// <summary>
        /// Matriz de pesos resultante.
        /// Tamanho Ñxm
        /// </summary>
        private double[][] _B = null;

        /// <summary>
        /// Matriz de saída da camada escondida para a fase de Treinamento.
        /// Tamanho TrainNxÑ
        /// </summary>
        private double[][] _TrainH = null;

        /// <summary>
        /// Matriz de saída da camada escondida para a fase de Validação.
        /// Tamanho ValidationNxÑ
        /// </summary>
        private double[][] _ValidationH = null;

        /// <summary>
        /// Matriz de saída da camada escondida para a fase de Teste.
        /// Tamanho TestNxÑ
        /// </summary>
        private double[][] _TestH = null;

        /// <summary>
        /// Matriz de interconexões da camada escondida.
        /// Tamanho ÑxÑ
        /// </summary>
        private double[][] _I = null;

        /// <summary>
        /// Vetor de saída da camada escondida. 
        /// </summary>
        private double[] _H0 = null;
        
        /// <summary>
        /// Matriz de pesos da camada de entrada.
        /// Tamanho Ñxn.
        /// </summary>
        private double[][] _w = null;

        /// <summary>
        /// Bias da camada escondida.
        /// Tamanho Ñ.
        /// </summary>
        private double[] _b = null;

        /// <summary>
        /// Número de exemplos de treinamento para a fase de Treinamento.
        /// </summary>
        private int _TrainN = -1;

        /// <summary>
        /// Número de exemplos de treinamento para a fase de Validação.
        /// </summary>
        private int _ValidationN = -1;

        /// <summary>
        /// Número de exemplos de treinamento para a fase de Teste.
        /// </summary>
        private int _TestN = -1;

        /// <summary>
        /// Número de neurônios da camada escondida.
        /// </summary>
        private int _Ñ = -1;

        /// <summary>
        /// Número de neurônios da camada de entrada.
        /// </summary>
        private int _n = -1;

        /// <summary>
        /// Número de neurônios da camda de saída.
        /// </summary>
        private int _m = -1;

        /// <summary>
        /// Número de cíclos de aquecimento.
        /// </summary>
        private int _WarmUpCicles = 1;

        /// <summary>
        /// Percentual de interconectividade entre os neurônios da camada escondida.
        /// </summary>
        private double _InterConnectivity = 0;

        /// <summary>
        /// Raio spectral dos pesos do Reservoir
        /// </summary>
        private double _SpectralRadious = 0;

        #region Configuration
        private ERCActivationFunctionType _ActivationFunctionType;
        /// <summary>
        /// Base de dados para treinamento/execução.
        /// </summary>
        private Data[] _TrainSet = null;
        /// <summary>
        /// Base de dados para validação do treinamento.
        /// </summary>
        private Data[] _ValidationSet = null;
        /// <summary>
        /// Base de dados para teste de execução.
        /// </summary>
        private Data[] _TestSet = null;
        private int _Seed = -1;
        private Random _Random = null;
        #endregion
        #endregion

        #region Properties
        /// <summary>
        /// Matriz de pesos resultante(Camada de saída).
        /// </summary>
        public double[][] GetB { get { return _B; } }

        /// <summary>
        /// Matriz de pesos da camada de entrada.
        /// </summary>
        public double[][] GetW { get { return _w; } }        

        public int HiddenNodesNumber { get { return this._Ñ; } }

        public double InterConnectivity { get { return this._InterConnectivity; } }

        public int WarmUpCicles { get { return this._WarmUpCicles; } }

        public double[][] GetI { get { return this._I; } }

        public double[] GetH0 { get { return this._H0; } }

        public double[][] TrainT { get { return this._TrainT; } }

        public double[][] ValidationT { get { return this._ValidationT; } }

        public double[][] TestT { get { return this._TestT; } }

        public Data[] TrainSet { get { return this._TrainSet; } }

        public Data[] ValidationSet { get { return this._ValidationSet; } }

        public Data[] TestSet { get { return this._TestSet; } }
        #endregion
               
        #region Private Methods
        /// <summary>
        /// Inicializa as variáveis da rede.
        /// </summary>
        private void _Initialize()
        {
            Data traindData = _TrainSet[0];
            _n = traindData.Input.Length;
            _m = traindData.Output.Length;
            _TrainN = _TrainSet.Length;
            _b = new double[_Ñ];
            _w = _w.InitializeMatrix(_Ñ, _n);
            _Trainx = _Trainx.InitializeMatrix(_TrainN, _n);
            _TrainT = _TrainT.InitializeMatrix(_TrainN, _m);
            _TrainH = _TrainH.InitializeMatrix(_TrainN, _Ñ);
            _I = _I.InitializeMatrix(_Ñ, _Ñ);


            _GenerateTMatrixForTrain();
            _GenerateXMatrixForTrain();
        }

        private void _InitializeForExecution()
        {
            Data execData = _TrainSet[0];
            _n = execData.Input.Length;
            _m = execData.Output.Length;
            _TrainN = _TrainSet.Length;
            _b = new double[_Ñ];

            for (int i = 0; i < _Ñ; i++)
                _b[i] = 1;

            _Trainx = _Trainx.InitializeMatrix(_TrainN, _n);
            _TrainH = _TrainH.InitializeMatrix(_TrainN, _Ñ);

            _GenerateXMatrixForExecution();
            _CalculateHMatrixForTrainExecution();
            _CalculateTForTrainUsingJama();
        }

        private void _GenerateTMatrixForTrain()
        {
            __GenerateTMatrix(_TrainN, ref _TrainT, _TrainSet); 
        }

        private void _GenerateTMatrixForValidation()
        {
            __GenerateTMatrix(_ValidationN, ref _ValidationT, _ValidationSet);
        }

        private void _GenerateTMatrixForTest()
        {
            __GenerateTMatrix(_TestN, ref _TestT, _TestSet);
        }

        /// <summary>
        /// Gera a matrix T a partir da base de dados.
        /// </summary>
        private void __GenerateTMatrix(int N, ref double[][] T, Data[] set)
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < _m; j++)
                {
                    T[i][j] = set[i].Output[j];
                }
            }
        }

        private void __GenerateXMatrix(int N, ref double[][] x, Data[] set)
        {
            for (int i = 0; i < N ; i++)
            {
                for (int j = 0 ; j < _n; j++)
                {
                    x[i][j] = set[i].Input[j];
                }
            }
        }

        private void _GenerateXMatrixForTrain()
        {
            __GenerateXMatrix(_TrainN, ref _Trainx, _TrainSet);            
        }

        private void _GenerateXMatrixForValidation()
        {
            __GenerateXMatrix(_ValidationN, ref _Validationx, _ValidationSet);            
        }

        private void _GenerateXMatrixForTest()
        {
            __GenerateXMatrix(_TestN, ref _Testx, _TestSet);
        }

        private void _GenerateXMatrixForExecution()
        {
            for (int i = 0; i < _TrainN; i++)
            {
                for (int j = 0; j < _n; j++)
                {
                    _Trainx[i][j] = _TrainSet[i].Input[j];
                }
            }
        }

        /// <summary>
        /// Gera valores aleatórios para as matrizes de peso de entrada
        /// entre -0.5 e 0.5.
        /// </summary>
        private void _RandomlyAssignInputWeights()
        {
            double rand = 0;

            for (int i = 0; i < _Ñ; i++)
            {              
                _b[i] = 1;

                for (int j = 0; j < _n; j++)
                {
                    do
                    {
                        rand = _Random.NextDouble();
                    } while (rand > 0.5);

                    if (_Random.NextDouble() <= 0.5)
                        rand = - (rand);

                    _w[i][j] = rand;
                }

                //testar svd na matriz I (divide pelo maior e multiplica por 0.95) raio espectral
                for (int k = 0; k < _Ñ; k++)
                {
                    if (_Random.NextDouble() <= _InterConnectivity)
                    {
                        do
                        {
                            rand = _Random.NextDouble();
                        }
                        while (rand > 0.5);

                        if (_Random.NextDouble() <= 0.5)
                            rand = -(rand);

                        _I[i][k] = rand;
                    }
                    else
                        _I[i][k] = 0;
                }                			
            }

            if (_SpectralRadious > 0)
            {
                Matrix normalizedSVDWeights = new Matrix(_I);
                SingularValueDecomposition svd = normalizedSVDWeights.svd();
                Matrix s = svd.getS();

                double maxValue = s.getArray()[0][0];
                for (int l = 0; l < s.getArray().Length; l++)
                {
                    for (int m = 0; m < s.getArray()[0].Length; m++)
                    {
                        if (s.getArray()[l][m] > maxValue)
                            maxValue = s.getArray()[l][m];
                    }
                }

                for (int i = 0; i < _I.Length; i++)
                {
                    for (int j = 0; j < _I[i].Length; j++)
                    {
                        _I[i][j] = (_I[i][j] / maxValue) * _SpectralRadious;
                    }
                }
            }
        }

        private void __CalculateHMatrix(int N, ref double[][] H, double[][] x, int warmUpCicles, bool updateH0)
        {          
            double sumOfInterconnections = 0;
            double[] temp_H0 = null;

            if (_H0 == null)
                temp_H0 = new double[_Ñ];
            else
                temp_H0 = _H0;

            for (int l = 1; l <= warmUpCicles; l++)
            {
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < _Ñ; j++)
                    { 
                        sumOfInterconnections = temp_H0.InnerProduct(_I[j]);

                        H[i][j] = _G(_w[j].InnerProduct(x[i]) + _b[j] + sumOfInterconnections);

                        sumOfInterconnections = 0;
                    }
                    if(updateH0)
                        temp_H0 = H[i].Clone() as double[];
                }               
            }

            if(updateH0)
                _H0 = temp_H0.Clone() as double[];
        }      

        private void _CalculateHMatrixForTrainExecution()
        {
            __CalculateHMatrix(_TrainN, ref _TrainH, _Trainx, _WarmUpCicles, true);            
        }

        private void _CalculateHMatrixForValidation()
        {
            __CalculateHMatrix(_ValidationN, ref _ValidationH, _Validationx, 1, true);            
        }

        private void _CalculateHMatrixForTest()
        {
            __CalculateHMatrix(_TestN, ref _TestH, _Testx, 1, true);  
        }
        
        /// <summary>
        /// Função de ativação
        /// </summary>
        private double _G(double value)
        {
            double result = 0;

            switch (_ActivationFunctionType)
            {
                case ERCActivationFunctionType.Linear:
                    result = value;
                    break;
                case ERCActivationFunctionType.SigmoidLogistic:
                    result = 1 / (1 + Math.Exp(-value));
                    break;
                case ERCActivationFunctionType.HyperbolicTangent:
                    result = (Math.Exp(2 * value) - 1) / (Math.Exp(2 * value) + 1);
                    break;
            }
            return result;
        }

        /// <summary>
        /// Calcula a Matriz de pesos B utilizando a biblioteca jama
        /// B = H*.T
        /// </summary>
        private void _CalculateBUsingJama()
        {
            Matrix h = new Matrix(_TrainH, (int)_TrainN, (int)_Ñ);
            SingularValueDecomposition svd = h.svd();

            Matrix V = svd.getV();
            Matrix S = svd.getS();
            Matrix U = svd.getU();

            Matrix U_T = U.transpose();

            Matrix S_T = S.transpose();
            Matrix S_TS = S_T.times(S);
            Matrix S_TS_1 = S_TS.inverse();

            Matrix inversa = V.times(S_TS_1).times(S_T).times(U_T);
            Matrix T = new Matrix((double[][]) this._TrainT.Clone(), (int)_TrainN, (int)_m);

            Matrix B = inversa.times(T);
            this._B = B.getArray();
        }

        /// <summary>
        /// Calcula a Matriz T utilizando a biblioteca jama
        /// T = HB;
        /// </summary>
        private void __CalculateTUsingJama(double[][] H, ref double[][] T, int N)
        {
            Matrix h = new Matrix(H, (int)N, (int)_Ñ);
            Matrix b = new Matrix(_B, (int)_Ñ, (int)_m);

            Matrix TMatrix = null;
            TMatrix = h.times(b);
            T = TMatrix.getArray();
        }

        private void _CalculateTForTrainUsingJama()
        {
            __CalculateTUsingJama(_TrainH, ref _TrainT, _TrainN); 
        }

        private void _CalculateTForValidationUsingJama()
        {
            __CalculateTUsingJama(_ValidationH, ref _ValidationT, _ValidationN);
        }

        private void _CalculateTForTestUsingJama()
        {
            __CalculateTUsingJama(_TestH, ref _TestT, _TestN); 
        }
        #endregion

        #region Public Methods
        public void Run()
        {
            _Train();
            _Validate();
            _Test();
        }

        private void _Train()
        {
            _RandomlyAssignInputWeights();
            _CalculateHMatrixForTrainExecution();
            _CalculateBUsingJama();

            _CalculateTForTrainUsingJama();
        }

        private void _Validate()
        {
            Data validateData = _ValidationSet[0];
            _ValidationN = _ValidationSet.Length;
            _Validationx = _Validationx.InitializeMatrix(_ValidationN, _n);
            _ValidationH = _ValidationH.InitializeMatrix(_ValidationN, _Ñ);
            _ValidationT = _ValidationT.InitializeMatrix(_ValidationN, _m);

            _GenerateXMatrixForValidation();
            _GenerateTMatrixForValidation();
            _CalculateHMatrixForValidation();
            _CalculateTForValidationUsingJama();
        }

        private void _Test()
        {
            Data testData = _TestSet[0];
            _TestN = _TestSet.Length;
            _Testx = _Testx.InitializeMatrix(_TestN, _n);
            _TestH = _TestH.InitializeMatrix(_TestN, _Ñ);
            _TestT = _TestT.InitializeMatrix(_TestN, _m);

            _GenerateXMatrixForTest();
            _GenerateTMatrixForTest();
            _CalculateHMatrixForTest();
            _CalculateTForTestUsingJama();
 
        }
        
        #endregion

        public void Dispose()
        {
            
        }
    }
}
