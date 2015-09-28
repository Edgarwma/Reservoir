using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataManagement;

namespace ReservoirComputing.Evaluation
{
    public class RCEvaluator : IDisposable
    {
        public RCEvaluator(RC rc, DataProvider prov, EEvaluationInfo evalInfo)
            : this(rc, prov, evalInfo, false)
        {}

        public RCEvaluator(RC rc, DataProvider prov, EEvaluationInfo evalInfo, bool keepDataNormalized)
        {
            _RC = rc;
            _Prov = prov;
            _EvaluationInfo = evalInfo;
            _EvaluateLikeONSDoes = (_EvaluationInfo & EEvaluationInfo.EvaluateLikeONS) == EEvaluationInfo.EvaluateLikeONS;
            _KeepDataNormalized = keepDataNormalized;
            
            NTrain = _RC.TrainSet.Length;
            mTrain = _RC.TrainSet[0].Output.Length;

            NVal = _RC.ValidationSet.Length;
            mVal = _RC.ValidationSet[0].Output.Length;

            NTest = _RC.TestSet.Length;
            mTest = _RC.TestSet[0].Output.Length;
            
            if (_EvaluateLikeONSDoes && mTrain < 12 && mVal < 12 && mTest < 12)
                throw new ArgumentException("O número de saídas para avaliação segundo o ONS deve ser 12");
        }

        private RC _RC;
        private DataProvider _Prov;
        private EEvaluationInfo _EvaluationInfo;
        int NTrain;
        int mTrain;
        int NVal;
        int mVal;
        int NTest;
        int mTest;
        bool _EvaluateLikeONSDoes;
        bool _KeepDataNormalized;

        public RC RC { get {return _RC; }}

        #region Train Evaluations
        public double[] TrainEPMA { get; private set; }
        public double[] TrainEPMAForONS { get; private set; }
        public double[] TrainEPMAForPowerPlant { get; private set; }
        public double[] TrainEMQ { get; private set; }
        public double[] TrainRMSE { get; private set; }
        public double[] TrainSR { get; private set; }
        public double[] TrainDEV { get; private set; }
        #endregion

        #region Validation Evaluations
        public double[] ValidationEPMA { get; private set; }
        public double[] ValidationEPMAForONS { get; private set; }
        public double[] ValidationEPMAForPowerPlant { get; private set; }
        public double[] ValidationEMQ { get; private set; }
        public double[] ValidationRMSE { get; private set; }
        public double[] ValidationSR { get; private set; }
        public double[] ValidationDEV { get; private set; }
        #endregion

        #region Test Evaluations
        public double[] TestEPMA { get; private set; }
        public double[] TestEPMAForONS { get; private set; }
        public double[] TestEPMAForPowerPlant { get; private set; }
        public double[] TestEMQ { get; private set; }
        public double[] TestRMSE { get; private set; }
        public double[] TestSR { get; private set; }
        public double[] TestDEV { get; private set; }
        #endregion
        
        #region Performance Evaluation
        public void Evaluate()
        {
            this.Evaluate(0);
        }

        public void Evaluate(double powerPlantPotency)
        {
            if ((_EvaluationInfo & EEvaluationInfo.EMQ) != EEvaluationInfo.None)
            {
                TrainEMQ = _GenerateEMQForTrain();
                ValidationEMQ = _GenerateEMQForValidation();
                TestEMQ = _GenerateEMQForTest();
            }

            if ((_EvaluationInfo & EEvaluationInfo.RMSE) != EEvaluationInfo.None)
            {
                TrainRMSE = _GenerateRMSEForTrain();
                ValidationRMSE = _GenerateRMSEForValidation();
                TestRMSE = _GenerateRMSEForTest();
            }

            if ((_EvaluationInfo & EEvaluationInfo.SR) != EEvaluationInfo.None)
            {
                TrainSR = _GenerateSRForTrain();
                ValidationSR = _GenerateSRForValidation();
                TestSR = _GenerateSRForTest();
            }

            if ((_EvaluationInfo & EEvaluationInfo.EPMA) != EEvaluationInfo.None)
            {
                TrainEPMA = _GenerateEPMAForTrain(false);
                ValidationEPMA = _GenerateEPMAForValidation(false);
                TestEPMA = _GenerateEPMAForTest(false);
            }

            if ((_EvaluationInfo & EEvaluationInfo.EvaluateLikeONS) != EEvaluationInfo.None)
            {
                TrainEPMAForONS = _GenerateEPMAForTrain(true);
                ValidationEPMAForONS = _GenerateEPMAForValidation(true);
                TestEPMAForONS = _GenerateEPMAForTest(true);
            }

            if ((_EvaluationInfo & EEvaluationInfo.EPMAForPowerPlant) != EEvaluationInfo.None)
            {
                if (powerPlantPotency == 0)
                    throw new ArgumentException("A potência instalada da usina deve ser maior que ZERO");

                TrainEPMAForPowerPlant = _GenerateEPMAForPowerPlantForTrain(powerPlantPotency);
                ValidationEPMAForPowerPlant = _GenerateEPMAForPowerPlantForValidation(powerPlantPotency);
                TestEPMAForPowerPlant = _GenerateEPMAForPowerPlantForTest(powerPlantPotency);
            }

            if ((_EvaluationInfo & EEvaluationInfo.DEV) != EEvaluationInfo.None)
            {
                TrainDEV = _GenerateDEVForTrain();
                ValidationDEV = _GenerateDEVForValidation();
                TestDEV = _GenerateDEVForTest();
            }
        }

        #region EPMA
        private double[] _GenerateEPMAForTrain(bool evaluateLikeONS)
        {
            return _GenerateEPMA(NTrain, mTrain, _RC.TrainSet, _RC.TrainT, evaluateLikeONS);
        }
       
        private double[] _GenerateEPMAForValidation(bool evaluateLikeONS)
        {
            return _GenerateEPMA(NVal, mVal, _RC.ValidationSet, _RC.ValidationT, evaluateLikeONS);
        }

        private double[] _GenerateEPMAForTest(bool evaluateLikeONS)
        {
            return _GenerateEPMA(NTest, mTest, _RC.TestSet, _RC.TestT, evaluateLikeONS);
        }

        private double[] _GenerateEPMA(int N, int m, Data[] dataSet, double[][] predictionOutput, bool evaluateLikeONS)
        {
            int epmaLength = evaluateLikeONS ? 7 : m;
            double[] EPMA = new double[epmaLength];
            double error = 0;
            double divisor = 0;

            for (int i = 0; i < N; i++)
            {
                for (int j = evaluateLikeONS ? 3 : 0, k = 0; j < (evaluateLikeONS ? 10 : m); j++, k++)
                {
                    if(_KeepDataNormalized)
                        error = predictionOutput[i][j] - dataSet[i].Output[j];
                    else
                        error = _Prov.DeNormalizeOutputData(predictionOutput[i][j], j) - dataSet[i].RealOutput[j];
                    
                    if(_KeepDataNormalized)
                        divisor = dataSet[i].Output[j] == 0 ? Double.Epsilon : dataSet[i].Output[j];
                    else
                        divisor = dataSet[i].RealOutput[j] == 0 ? Double.Epsilon : dataSet[i].RealOutput[j];

                    EPMA[k] += Math.Abs(error / divisor);
                }
            }

            for (int i = 0; i < epmaLength; i++)
                EPMA[i] = EPMA[i] / N * 100;

            return EPMA;
        }
        #endregion

        #region EPMAForPowerPlaint
        private double[] _GenerateEPMAForPowerPlantForTrain(double powerPlantPotency)
        {
            return _GenerateEPMAForPowerPlant(NTrain, mTrain, _RC.TrainSet, _RC.TrainT, powerPlantPotency);
        }

        private double[] _GenerateEPMAForPowerPlantForValidation(double powerPlantPotency)
        {
            return _GenerateEPMAForPowerPlant(NVal, mVal, _RC.ValidationSet, _RC.ValidationT, powerPlantPotency);
        }

        private double[] _GenerateEPMAForPowerPlantForTest(double powerPlantPotency)
        {
            return _GenerateEPMAForPowerPlant(NTest, mTest, _RC.TestSet, _RC.TestT, powerPlantPotency);
        }

        private double[] _GenerateEPMAForPowerPlant(int N, int m, Data[] dataSet, double[][] predicitionOutput, double powerPlantPotency)
        {
            double[] EPMA = new double[m];
            double Error = 0;

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    Error =  _Prov.DeNormalizeOutputData(predicitionOutput[i][j], j) - dataSet[i].RealOutput[j];

                    EPMA[j] += Math.Abs(Error / powerPlantPotency);
                }
            }

            for(int i = 0 ; i < m; i++)
                EPMA[i] = EPMA[i] / N * 100;

            return EPMA;
        }
        
        #endregion

        #region EMQ
        private double[] _GenerateEMQForTrain()
        {
            return _GenerateEMQ(NTrain, mTrain, _RC.TrainSet, _RC.TrainT);
        }

        private double[] _GenerateEMQForValidation()
        {
            return _GenerateEMQ(NVal, mVal, _RC.ValidationSet, _RC.ValidationT);
        }

        private double[] _GenerateEMQForTest()
        {
            return _GenerateEMQ(NTest, mTest, _RC.TestSet, _RC.TestT);
        }

        private double[] _GenerateEMQ(int N, int m, Data[] dataSet, double[][] predictionOutput)
        {
            double[] EMQ = new double[m];
            double error = 0;

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if(_KeepDataNormalized)
                        error = dataSet[i].Output[j] - predictionOutput[i][j];
                    else
                        error = dataSet[i].RealOutput[j] - _Prov.DeNormalizeOutputData(predictionOutput[i][j], j);

                    EMQ[j] += Math.Pow(error, 2);
                    error = 0;
                }
            }

            for (int i = 0; i < m; i++)
                EMQ[i] = EMQ[i] / N;

            return EMQ; 
        }
        #endregion

        #region RMSE
        private double[] _GenerateRMSEForTrain()
        {
            double[] emq = _GenerateEMQForTrain();

            for (int i = 0; i < mTrain; i++)
                emq[i] = Math.Sqrt(emq[i]);

            return emq;
        }

        private double[] _GenerateRMSEForValidation()
        {
            double[] emq = _GenerateEMQForValidation();

            for (int i = 0; i < mVal; i++)
                emq[i] = Math.Sqrt(emq[i]);

            return emq;
        }

        private double[] _GenerateRMSEForTest()
        {
            double[] emq = _GenerateEMQForTest();

            for (int i = 0; i < mVal; i++)
                emq[i] = Math.Sqrt(emq[i]);

            return emq;
        }
        #endregion

        #region SR
        private double[] _GenerateSRForTrain()
        {
            return _GenerateSR(NTrain, mTrain, _RC.TrainSet, _RC.TrainT);
        }
        
        private double[] _GenerateSRForValidation()
        {
            return _GenerateSR(NVal, mVal, _RC.ValidationSet, _RC.ValidationT);
        }

        private double[] _GenerateSRForTest()
        {
            return _GenerateSR(NTest, mTest, _RC.TestSet, _RC.TestT);
        }

        private double[] _GenerateSR(int N, int m, Data[] dataSet, double[][] predictionOutput)
        {
            double[] successCount = new double[m];
            double[] sr = new double[m];

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (dataSet[i].RealOutput[j] == Math.Round(_Prov.DeNormalizeOutputData(predictionOutput[i][j],j)))
                        successCount[j]++;
                }
            }

            for (int i = 0; i < m; i++)
                sr[i] = successCount[i] * 100 / N;

            return sr;
        }
        #endregion

        #region DEV
        private double[] _GenerateDEVForTrain()
        {
            return _GenerateDEV(NTrain, mTrain, _RC.TrainSet, _RC.TrainT);
        }

        private double[] _GenerateDEVForValidation()
        {
            return _GenerateDEV(NVal, mVal, _RC.ValidationSet, _RC.ValidationT);
        }

        private double[] _GenerateDEVForTest()
        {
            return _GenerateDEV(NTest, mTest, _RC.TestSet, _RC.TestT);
        }

        private double[] _GenerateDEV(int N, int m, Data[] dataSet, double[][] predictionOutput)
        {
            double[] Mean = new double[m];
            double[] DEV = new double[m];
            double error = 0;

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if(_KeepDataNormalized)
                        error = dataSet[i].Output[j] - predictionOutput[i][j];
                    else
                        error = dataSet[i].RealOutput[j] - _Prov.DeNormalizeOutputData(predictionOutput[i][j], j);

                    Mean[j] += error;

                    error = 0;
                }
            }

            for (int i = 0; i < m; i++)
                Mean[i] = Mean[i] / N;

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (_KeepDataNormalized)
                        error = dataSet[i].Output[j] - predictionOutput[i][j];
                    else
                        error = dataSet[i].RealOutput[j] - _Prov.DeNormalizeOutputData(predictionOutput[i][j], j);
                       

                    DEV[j] += Math.Pow(error - Mean[j], 2);

                    error = 0;
                }
            }

            for (int i = 0; i < m; i++)
                DEV[i] = DEV[i] / (N - 1);

            return DEV;

        }
        #endregion
        #endregion

        public void Dispose()
        {
            
        }
    }
}
