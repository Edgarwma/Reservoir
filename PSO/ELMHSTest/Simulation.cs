using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReservoirComputing;
using System.IO;
using System.ComponentModel;
using DataManagement;
using ReservoirComputing.Configuration;
using HarmonnySearch.Evaluation;
using ReservoirComputing.Evaluation;
using HarmonnySearch.HS;
using HarmonnySearch.Particle;
using MLP;
using MLP.Evaluation;

namespace Simulator
{
    public class Simulation
    {

        #region Attributes
        Random Rand = new Random(1);
        StreamWriter File;
        string info;
        DateTime StartTime;
        DateTime EndTime;
        bool SavePrediciton = true;
        int ResultIndex = 6;
        DataBuilder db;
        Data[] set;
        int seed;
        ELMHS hs = null;
        RCHS rc = null;
        RedeMLP mlp;
        DataProvider prov = null;
        HSResult<byte> returnVal;
        int stopIteration;
        int stopEvaluation;
        ELMHSParticle ELMBestParticle;
        RCHSParticle RCBestParticle;
        double real = 0;
        double prediction = 0;
        double error = 0;
        double[][] entradasTreinamento = null;
        double[][] saidasTreinamento = null;
        double[][] entradasValidacao = null;
        double[][] saidasValidacao = null;
        double[][] entradasTeste = null;
        double[][] saidasTeste = null;
        MLPEvaluator eval = null;
        List<MLPEvaluator> mlps = null;
        #endregion

        #region ConfigurationParameters
        double alfa = 0.80;
        double beta = 0.20;
        double inputA = -0.85;
        double inputB = 0.85;
        double outputA = -0.85;
        double outputB = 0.85;
        double trainSize = 0.50;
        double validationSize = 0.25;
        double testSize = 0.25;
        int TestIterations = 30;
        int MemorySize = 10;
        int MaxHiddenNodes = 100;
        int MaxEvaluations = 50;
        int inputLag;
        int outputLag;
        int MaxWarmUpCicles = 150;
        double MaxInterConnectivity = 1;
        double MinSpectralRadious = 0.7;
        EExecutionType DataType = EExecutionType.Predction;
        ERCActivationFunctionType ActivationFunction = ERCActivationFunctionType.HyperbolicTangent;
        EEvaluationInfo PerformanceInfo = EEvaluationInfo.EPMA | EEvaluationInfo.EMQ | EEvaluationInfo.DEV | EEvaluationInfo.EvaluateLikeONS | EEvaluationInfo.RMSE;
        EMLPEvaluationInfo MLPPerformanceInfo = EMLPEvaluationInfo.EPMA | EMLPEvaluationInfo.EMQ | EMLPEvaluationInfo.DEV | EMLPEvaluationInfo.EvaluateLikeONS | EMLPEvaluationInfo.RMSE;
        #endregion

        public void Simulate(string dataSetPath, string simulationResultPath)
        {
            #region others
            #region ELM
            #region ELM-WEIGHT
            #region Ajustando arquivo de resultados
            if (!System.IO.Directory.Exists(string.Concat(simulationResultPath, @"ELM\weight\")))
                System.IO.Directory.CreateDirectory(string.Concat(simulationResultPath, @"ELM\weight\"));

            File = new System.IO.StreamWriter(string.Concat(simulationResultPath, @"ELM\weight\result.csv"), false);

            info = "Position; Seed; HidenNodes; InputNodes; ";

            if (DataType == EExecutionType.Predction)
                info = string.Concat(info, (PerformanceInfo & EEvaluationInfo.EMQ) != EEvaluationInfo.None ? "EMQ; " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.DEV) != EEvaluationInfo.None ? "DEV; " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.RMSE) != EEvaluationInfo.None ? "RMSE; " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.EPMA) != EEvaluationInfo.None ? "EPMA(1); EPMA(2); EPMA(3); EPMA(4); EPMA(5); EPMA(6); EPMA(7); EPMA(8); EPMA(9); EPMA(10); EPMA(11); EPMA(12); EPMA(13); EPMA(14); " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.EvaluateLikeONS) != EEvaluationInfo.None ? "EPMA-ONS(1); EPMA-ONS(2); EPMA-ONS(3); EPMA-ONS(4); EPMA-ONS(5); EPMA-ONS(6); EPMA-ONS(7); " : string.Empty);
            else if ((PerformanceInfo & EEvaluationInfo.SR) != EEvaluationInfo.None)
                info = string.Concat("SuccessRate; ");

            info = string.Concat(info, "Time");
            info = string.Concat(info, "; StopCount");
            info = string.Concat(info, "; StopEvaluation");

            File.WriteLine(info);
            File.Close();
            #endregion

            #region Processando
            Console.WriteLine(DateTime.Now.ToShortTimeString() + "[ELM-WEIGHT]Processando arquivo: " + dataSetPath);

            for (int i = 1; i <= TestIterations; i++)
            {
                Console.WriteLine(DateTime.Now.ToShortTimeString() + "[ELM-WEIGHT]Início iteração: " + i);
                seed = Rand.Next();

                inputLag = 30;
                outputLag = 14;

                db = new DataBuilder(@dataSetPath);
                db.LagData(inputLag, outputLag);

                set = db.BuildDataSet();

                prov = new DataProvider(set, DataType, seed, trainSize, validationSize, testSize);
                prov.ApplyLogToData();
                prov.NormalizeData(inputA, inputB, outputA, outputB);

                prov.ShuffleDataSet();
                prov.SplitData();

                StartTime = DateTime.Now;

                hs = new ELMHS(prov, seed, MemorySize,
                    MaxHiddenNodes, MaxEvaluations, ActivationFunction, EHSEvaluationFunctionType.Weight, PerformanceInfo);

                returnVal = hs.Run();

                stopIteration = returnVal.StopIteration;
                stopEvaluation = returnVal.StopEvaluations;
                ELMBestParticle = (ELMHSParticle)returnVal.BestParticle;

                EndTime = DateTime.Now;

                #region Result building
                string resultString = i + "; " +
                                      seed + "; " +
                                      ELMBestParticle.Config.HidenNodesNumber + "; " +
                                      ELMBestParticle.Config.Prov.InputsN + "; ";

                if (DataType == EExecutionType.Predction)
                {
                    if ((PerformanceInfo & EEvaluationInfo.EMQ) != EEvaluationInfo.None)
                        resultString += ELMBestParticle.Evaluator.TestEMQ[ResultIndex].ToString("0.####") + "; ";

                    if ((PerformanceInfo & EEvaluationInfo.DEV) != EEvaluationInfo.None)
                        resultString += ELMBestParticle.Evaluator.TestDEV[ResultIndex].ToString("0.####") + "; ";

                    if ((PerformanceInfo & EEvaluationInfo.RMSE) != EEvaluationInfo.None)
                        resultString += ELMBestParticle.Evaluator.TestRMSE[ResultIndex].ToString("0.####") + "; ";

                    if ((PerformanceInfo & EEvaluationInfo.EPMA) != EEvaluationInfo.None)
                    {
                        for (int k = 0; k < ELMBestParticle.Config.Prov.OutputsN; k++)
                            resultString += ELMBestParticle.Evaluator.TestEPMA[k].ToString("0.####") + "; ";
                    }

                    if ((PerformanceInfo & EEvaluationInfo.EvaluateLikeONS) != EEvaluationInfo.None)
                    {
                        for (int l = 0; l < 7; l++)
                            resultString += ELMBestParticle.Evaluator.TestEPMAForONS[l].ToString("0.####") + "; ";
                    }
                }
                else if ((PerformanceInfo & EEvaluationInfo.SR) != EEvaluationInfo.None)
                    resultString += ELMBestParticle.Evaluator.TestSR[ResultIndex].ToString("0.####") + "; ";

                resultString += EndTime.Subtract(StartTime).ToReadableString();
                resultString += "; " + stopIteration;
                resultString += "; " + stopEvaluation;
                #endregion

                Console.WriteLine(DateTime.Now.ToShortTimeString() + info);
                Console.WriteLine(DateTime.Now.ToShortTimeString() + resultString);

                File = new System.IO.StreamWriter(string.Concat(simulationResultPath, @"ELM\weight\result.csv"), true);
                File.WriteLine(resultString);
                File.Close();

                #region Saving Prediction
                if (SavePrediciton)
                {
                    real = 0;
                    prediction = 0;
                    error = 0;

                    File = new System.IO.StreamWriter(simulationResultPath + @"ELM\weight\predicao_" + i + ".csv", false);

                    for (int m = 0; m < ELMBestParticle.Config.Prov.TestSetlines; m++)
                    {
                        string temp = string.Empty;
                        for (int j = 0; j < ELMBestParticle.Config.Prov.OutputsN; j++)
                        {
                            real = ELMBestParticle.Config.Prov.TestSet[m].RealOutput[j];
                            prediction = ELMBestParticle.Config.Prov.DeNormalizeOutputData(ELMBestParticle.Evaluator.RC.TestT[m][j], j);

                            error = real - prediction;
                            temp = string.Concat(temp + ELMBestParticle.Config.Prov.TestSet[m].RealOutput[j] + ";" + prediction + ";" + error + ";");
                        }
                        File.WriteLine(temp);
                    }
                    File.Close();
                }
                #endregion

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Console.WriteLine(DateTime.Now.ToShortTimeString() + "[ELM-WEIGHT]Fim iteração: " + i);
            }
            #endregion
            #endregion

            #region ELM-PSE
            #region Ajustando arquivo de resultados
            if (!System.IO.Directory.Exists(string.Concat(simulationResultPath, @"ELM\pse\")))
                System.IO.Directory.CreateDirectory(string.Concat(simulationResultPath, @"ELM\pse\"));

            File = new System.IO.StreamWriter(string.Concat(simulationResultPath, @"ELM\pse\result.csv"), false);

            info = "Position; Seed; HidenNodes; InputNodes; ";

            if (DataType == EExecutionType.Predction)
                info = string.Concat(info, (PerformanceInfo & EEvaluationInfo.EMQ) != EEvaluationInfo.None ? "EMQ; " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.DEV) != EEvaluationInfo.None ? "DEV; " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.RMSE) != EEvaluationInfo.None ? "RMSE; " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.EPMA) != EEvaluationInfo.None ? "EPMA(1); EPMA(2); EPMA(3); EPMA(4); EPMA(5); EPMA(6); EPMA(7); EPMA(8); EPMA(9); EPMA(10); EPMA(11); EPMA(12); EPMA(13); EPMA(14); " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.EvaluateLikeONS) != EEvaluationInfo.None ? "EPMA-ONS(1); EPMA-ONS(2); EPMA-ONS(3); EPMA-ONS(4); EPMA-ONS(5); EPMA-ONS(6); EPMA-ONS(7); " : string.Empty);
            else if ((PerformanceInfo & EEvaluationInfo.SR) != EEvaluationInfo.None)
                info = string.Concat("SuccessRate; ");

            info = string.Concat(info, "Time");
            info = string.Concat(info, "; StopCount");
            info = string.Concat(info, "; StopEvaluation");

            File.WriteLine(info);
            File.Close();
            #endregion

            #region Processando
            Console.WriteLine(DateTime.Now.ToShortTimeString() + "[ELM-PSE]Processando arquivo: " + dataSetPath);

            for (int i = 1; i <= TestIterations; i++)
            {
                Console.WriteLine(DateTime.Now.ToShortTimeString() + "[ELM-PSE]Início iteração: " + i);
                seed = Rand.Next();

                inputLag = 30;
                outputLag = 14;

                db = new DataBuilder(@dataSetPath);
                db.LagData(inputLag, outputLag);

                set = db.BuildDataSet();

                prov = new DataProvider(set, DataType, seed, trainSize, validationSize, testSize);
                prov.ApplyLogToData();
                prov.NormalizeData(inputA, inputB, outputA, outputB);

                prov.ShuffleDataSet();
                prov.SplitData();

                StartTime = DateTime.Now;

                hs = new ELMHS(prov, seed, MemorySize,
                    MaxHiddenNodes, MaxEvaluations, ActivationFunction, EHSEvaluationFunctionType.PSE, PerformanceInfo);

                returnVal = hs.Run();

                stopIteration = returnVal.StopIteration;
                stopEvaluation = returnVal.StopEvaluations;
                ELMBestParticle = (ELMHSParticle)returnVal.BestParticle;

                EndTime = DateTime.Now;

                #region Result building
                string resultString = i + "; " +
                                      seed + "; " +
                                      ELMBestParticle.Config.HidenNodesNumber + "; " +
                                      ELMBestParticle.Config.Prov.InputsN + "; ";

                if (DataType == EExecutionType.Predction)
                {
                    if ((PerformanceInfo & EEvaluationInfo.EMQ) != EEvaluationInfo.None)
                        resultString += ELMBestParticle.Evaluator.TestEMQ[ResultIndex].ToString("0.####") + "; ";

                    if ((PerformanceInfo & EEvaluationInfo.DEV) != EEvaluationInfo.None)
                        resultString += ELMBestParticle.Evaluator.TestDEV[ResultIndex].ToString("0.####") + "; ";

                    if ((PerformanceInfo & EEvaluationInfo.RMSE) != EEvaluationInfo.None)
                        resultString += ELMBestParticle.Evaluator.TestRMSE[ResultIndex].ToString("0.####") + "; ";

                    if ((PerformanceInfo & EEvaluationInfo.EPMA) != EEvaluationInfo.None)
                    {
                        for (int k = 0; k < ELMBestParticle.Config.Prov.OutputsN; k++)
                            resultString += ELMBestParticle.Evaluator.TestEPMA[k].ToString("0.####") + "; ";
                    }

                    if ((PerformanceInfo & EEvaluationInfo.EvaluateLikeONS) != EEvaluationInfo.None)
                    {
                        for (int l = 0; l < 7; l++)
                            resultString += ELMBestParticle.Evaluator.TestEPMAForONS[l].ToString("0.####") + "; ";
                    }
                }
                else if ((PerformanceInfo & EEvaluationInfo.SR) != EEvaluationInfo.None)
                    resultString += ELMBestParticle.Evaluator.TestSR[ResultIndex].ToString("0.####") + "; ";

                resultString += EndTime.Subtract(StartTime).ToReadableString();
                resultString += "; " + stopIteration;
                resultString += "; " + stopEvaluation;
                #endregion

                Console.WriteLine(DateTime.Now.ToShortTimeString() + info);
                Console.WriteLine(DateTime.Now.ToShortTimeString() + resultString);

                File = new System.IO.StreamWriter(string.Concat(simulationResultPath, @"ELM\pse\result.csv"), true);
                File.WriteLine(resultString);
                File.Close();

                #region Saving Prediction
                if (SavePrediciton)
                {
                    real = 0;
                    prediction = 0;
                    error = 0;

                    File = new System.IO.StreamWriter(simulationResultPath + @"ELM\pse\predicao_" + i + ".csv", false);

                    for (int m = 0; m < ELMBestParticle.Config.Prov.TestSetlines; m++)
                    {
                        string temp = string.Empty;
                        for (int j = 0; j < ELMBestParticle.Config.Prov.OutputsN; j++)
                        {
                            real = ELMBestParticle.Config.Prov.TestSet[m].RealOutput[j];
                            prediction = ELMBestParticle.Config.Prov.DeNormalizeOutputData(ELMBestParticle.Evaluator.RC.TestT[m][j], j);

                            error = real - prediction;
                            temp = string.Concat(temp + ELMBestParticle.Config.Prov.TestSet[m].RealOutput[j] + ";" + prediction + ";" + error + ";");
                        }
                        File.WriteLine(temp);
                    }
                    File.Close();
                }
                #endregion

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Console.WriteLine(DateTime.Now.ToShortTimeString() + "[ELM-PSE]Fim iteração: " + i);
            }
            #endregion
            #endregion
            #endregion
            #region MLP
            #region Ajustando arquivo de resultados
            if (!System.IO.Directory.Exists(string.Concat(simulationResultPath, @"MLP\")))
                System.IO.Directory.CreateDirectory(string.Concat(simulationResultPath, @"MLP\"));

            File = new System.IO.StreamWriter(string.Concat(simulationResultPath, @"MLP\result.csv"), false);

            info = "Position; Seed; HidenNodes; InputNodes; Alfa; Beta; ";

            if (DataType == EExecutionType.Predction)
                info = string.Concat(info, (MLPPerformanceInfo & EMLPEvaluationInfo.EMQ) != EMLPEvaluationInfo.None ? "EMQ; " : string.Empty,
                    (MLPPerformanceInfo & EMLPEvaluationInfo.DEV) != EMLPEvaluationInfo.None ? "DEV; " : string.Empty,
                    (MLPPerformanceInfo & EMLPEvaluationInfo.RMSE) != EMLPEvaluationInfo.None ? "RMSE; " : string.Empty,
                    (MLPPerformanceInfo & EMLPEvaluationInfo.EPMA) != EMLPEvaluationInfo.None ? "EPMA(1); EPMA(2); EPMA(3); EPMA(4); EPMA(5); EPMA(6); EPMA(7); EPMA(8); EPMA(9); EPMA(10); EPMA(11); EPMA(12); EPMA(13); EPMA(14); " : string.Empty,
                    (MLPPerformanceInfo & EMLPEvaluationInfo.EvaluateLikeONS) != EMLPEvaluationInfo.None ? "EPMA-ONS(1); EPMA-ONS(2); EPMA-ONS(3); EPMA-ONS(4); EPMA-ONS(5); EPMA-ONS(6); EPMA-ONS(7); " : string.Empty);
            else if ((MLPPerformanceInfo & EMLPEvaluationInfo.SR) != EMLPEvaluationInfo.None)
                info = string.Concat("SuccessRate; ");

            info = string.Concat(info, "Time");
            info = string.Concat(info, "; StopCount");
            info = string.Concat(info, "; StopEvaluation");

            File.WriteLine(info);
            File.Close();
            #endregion

            #region Processando
            Console.WriteLine(DateTime.Now.ToShortTimeString() + "[MLP]Processando arquivo: " + dataSetPath);

            for (int i = 1; i <= TestIterations; i++)
            {
                Console.WriteLine(DateTime.Now.ToShortTimeString() + "[MLP]Início iteração: " + i);
                seed = Rand.Next();

                inputLag = 30;
                outputLag = 14;

                db = new DataBuilder(@dataSetPath);
                db.LagData(inputLag, outputLag);

                set = db.BuildDataSet();

                prov = new DataProvider(set, DataType, seed, trainSize, validationSize, testSize);
                //prov.ApplyLogToData();
                prov.NormalizeData(0.15, 0.85, 0.15, 0.85);

                prov.ShuffleDataSet();
                prov.SplitData();

                entradasTreinamento = new double[prov.TrainSetLines][];
                saidasTreinamento = new double[prov.TrainSetLines][];
                for (int p = 0; p < prov.TrainSetLines; p++)
                {
                    entradasTreinamento[p] = new double[prov.TrainSet[p].Input.Length];
                    saidasTreinamento[p] = new double[prov.TrainSet[p].Output.Length];

                    for (int j = 0; j < prov.TrainSet[p].Input.Length; j++)
                        entradasTreinamento[p][j] = prov.TrainSet[p].Input[j];

                    for (int j = 0; j < prov.TrainSet[p].Output.Length; j++)
                        saidasTreinamento[p][j] = prov.TrainSet[p].Output[j];
                }

                entradasValidacao = new double[prov.ValidationSetLines][];
                saidasValidacao = new double[prov.ValidationSetLines][];
                for (int p = 0; p < prov.ValidationSetLines; p++)
                {
                    entradasValidacao[p] = new double[prov.ValidationSet[p].Input.Length];
                    saidasValidacao[p] = new double[prov.ValidationSet[p].Output.Length];

                    for (int j = 0; j < prov.ValidationSet[p].Input.Length; j++)
                        entradasValidacao[p][j] = prov.ValidationSet[p].Input[j];

                    for (int j = 0; j < prov.ValidationSet[p].Output.Length; j++)
                        saidasValidacao[p][j] = prov.ValidationSet[p].Output[j];
                }

                entradasTeste = new double[prov.TestSetlines][];
                saidasTeste = new double[prov.TestSetlines][];
                for (int p = 0; p < prov.TestSetlines; p++)
                {
                    entradasTeste[p] = new double[prov.TestSet[p].Input.Length];
                    saidasTeste[p] = new double[prov.TestSet[p].Output.Length];

                    for (int j = 0; j < prov.TestSet[p].Input.Length; j++)
                        entradasTeste[p][j] = prov.TestSet[p].Input[j];

                    for (int j = 0; j < prov.TestSet[p].Output.Length; j++)
                        saidasTeste[p][j] = prov.TestSet[p].Output[j];
                }

                mlps = new List<MLPEvaluator>();

                StartTime = DateTime.Now;
                int count = 1;//MaxHiddenNodes / 10;
                int hidden = 60;
                for (int m = 0; m < count; m++)
                {
                    mlp = new RedeMLP(prov,
                        entradasTreinamento,//array de entradas de treinamento
                        saidasTreinamento,//array de saidas de treinamento
                        entradasValidacao,//array de entradas de validacao
                        saidasValidacao,//array de saidas de validacao
                        alfa,//valor de alpha
                        beta,//valor de beta
                        MaxEvaluations,//maximo de ciclos
                        hidden,//quatidade de neuronios escondida
                        EnumTipoExecucao.Previsao,//booleano para definir se é previsao(true) ou classificacao(false)
                        Algoritmos.BACKPROPAGATION,//constante q define o algoritmo a ser utilizado
                        seed);

                    mlp.testar(entradasTeste, saidasTeste);

                    eval = new MLPEvaluator(mlp, prov, MLPPerformanceInfo);
                    eval.Evaluate();

                    mlps.Add(eval);

                    hidden += 10;
                }

                EndTime = DateTime.Now;

                mlps.Sort(delegate(MLPEvaluator p1, MLPEvaluator p2)
                {
                    return (p1.ValidationEPMA[6] + p1.ValidationEPMA[13]).CompareTo(p2.ValidationEPMA[6] + p2.ValidationEPMA[13]);
                });

                #region Result building
                string resultString = i + "; " +
                                      seed + "; " +
                                      mlps[0].MLP.qtdEscondidos + "; " +
                                      prov.InputsN + "; " +
                                      alfa + "; " +
                                      beta + "; ";

                if (DataType == EExecutionType.Predction)
                {
                    if ((MLPPerformanceInfo & EMLPEvaluationInfo.EMQ) != EMLPEvaluationInfo.None)
                        resultString += mlps[0].TestEMQ[ResultIndex].ToString("0.####") + "; ";

                    if ((MLPPerformanceInfo & EMLPEvaluationInfo.DEV) != EMLPEvaluationInfo.None)
                        resultString += mlps[0].TestDEV[ResultIndex].ToString("0.####") + "; ";

                    if ((MLPPerformanceInfo & EMLPEvaluationInfo.RMSE) != EMLPEvaluationInfo.None)
                        resultString += mlps[0].TestRMSE[ResultIndex].ToString("0.####") + "; ";

                    if ((MLPPerformanceInfo & EMLPEvaluationInfo.EPMA) != EMLPEvaluationInfo.None)
                    {
                        for (int k = 0; k < prov.OutputsN; k++)
                            resultString += mlps[0].TestEPMA[k].ToString("0.####") + "; ";
                    }

                    if ((MLPPerformanceInfo & EMLPEvaluationInfo.EvaluateLikeONS) != EMLPEvaluationInfo.None)
                    {
                        for (int l = 0; l < 7; l++)
                            resultString += mlps[0].TestEPMAForONS[l].ToString("0.####") + "; ";
                    }
                }
                else if ((MLPPerformanceInfo & EMLPEvaluationInfo.SR) != EMLPEvaluationInfo.None)
                    resultString += mlps[0].TestSR[ResultIndex].ToString("0.####") + "; ";

                resultString += EndTime.Subtract(StartTime).ToReadableString();
                resultString += "; " + mlps[0].MLP.Ciclo;
                resultString += "; " + mlps[0].MLP.Ciclo;
                #endregion

                Console.WriteLine(DateTime.Now.ToShortTimeString() + info);
                Console.WriteLine(DateTime.Now.ToShortTimeString() + resultString);

                File = new System.IO.StreamWriter(string.Concat(simulationResultPath, @"MLP\result.csv"), true);
                File.WriteLine(resultString);
                File.Close();

                #region Saving Prediction
                if (SavePrediciton)
                {
                    real = 0;
                    prediction = 0;
                    error = 0;

                    File = new System.IO.StreamWriter(simulationResultPath + @"MLP\predicao_" + i + ".csv", false);

                    for (int m = 0; m < prov.TestSetlines; m++)
                    {
                        string temp = string.Empty;
                        for (int j = 0; j < prov.OutputsN; j++)
                        {
                            real = prov.TestSet[m].RealOutput[j];
                            prediction = prov.DeNormalizeOutputData(mlps[0].MLP.saidasCalculadasTeste[m][j], j);

                            error = real - prediction;
                            temp = string.Concat(temp + prov.TestSet[m].RealOutput[j] + ";" + prediction + ";" + error + ";");
                        }
                        File.WriteLine(temp);
                    }
                    File.Close();
                }
                #endregion

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Console.WriteLine(DateTime.Now.ToShortTimeString() + "[MLP]Fim iteração: " + i);
            }
            #endregion
            #endregion
            #endregion

            #region RC-WEIGHT
            #region Ajustando arquivo de resultados
            if (!System.IO.Directory.Exists(string.Concat(simulationResultPath, @"RC\weight\")))
                System.IO.Directory.CreateDirectory(string.Concat(simulationResultPath, @"RC\weight\"));

            File = new System.IO.StreamWriter(string.Concat(simulationResultPath, @"RC\weight\result.csv"), false);

            info = "Position; Seed; HidenNodes; InputNodes; WarmUpCicles; Interconnectivity; SpectralRadious; ";

            if (DataType == EExecutionType.Predction)
                info = string.Concat(info, (PerformanceInfo & EEvaluationInfo.EMQ) != EEvaluationInfo.None ? "EMQ; " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.DEV) != EEvaluationInfo.None ? "DEV; " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.RMSE) != EEvaluationInfo.None ? "RMSE; " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.EPMA) != EEvaluationInfo.None ? "EPMA(1); EPMA(2); EPMA(3); EPMA(4); EPMA(5); EPMA(6); EPMA(7); EPMA(8); EPMA(9); EPMA(10); EPMA(11); EPMA(12); EPMA(13); EPMA(14); " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.EvaluateLikeONS) != EEvaluationInfo.None ? "EPMA-ONS(1); EPMA-ONS(2); EPMA-ONS(3); EPMA-ONS(4); EPMA-ONS(5); EPMA-ONS(6); EPMA-ONS(7); " : string.Empty);
            else if ((PerformanceInfo & EEvaluationInfo.SR) != EEvaluationInfo.None)
                info = string.Concat("SuccessRate; ");

            info = string.Concat(info, "Time");
            info = string.Concat(info, "; StopCount");
            info = string.Concat(info, "; StopEvaluation");

            File.WriteLine(info);
            File.Close();
            #endregion

            #region Processando
            Console.WriteLine(DateTime.Now.ToShortTimeString() + "[RC-WEIGHT]Processando arquivo: " + dataSetPath);

            for (int i = 1; i <= TestIterations; i++)
            {
                Console.WriteLine(DateTime.Now.ToShortTimeString() + "[RC-WEIGHT]Início iteração: " + i);
                seed = Rand.Next();

                inputLag = 1;
                outputLag = 14;

                db = new DataBuilder(@dataSetPath);
                db.LagData(inputLag, outputLag);

                set = db.BuildDataSet();

                prov = new DataProvider(set, DataType, seed, trainSize, validationSize, testSize);
                prov.ApplyLogToData();
                prov.NormalizeData(inputA, inputB, outputA, outputB);

                prov.ShuffleDataSet();
                prov.SplitData();

                StartTime = DateTime.Now;

                rc = new RCHS(MaxInterConnectivity, MaxWarmUpCicles, MinSpectralRadious, prov, seed, MemorySize,
                    MaxHiddenNodes, MaxEvaluations, ActivationFunction, EHSEvaluationFunctionType.Weight, PerformanceInfo);

                returnVal = rc.Run();

                stopIteration = returnVal.StopIteration;
                stopEvaluation = returnVal.StopEvaluations;
                RCBestParticle = (RCHSParticle)returnVal.BestParticle;

                EndTime = DateTime.Now;

                #region Result building
                string resultString = i + "; " +
                                      seed + "; " +
                                      RCBestParticle.Config.HidenNodesNumber + "; " +
                                      RCBestParticle.Config.Prov.InputsN + "; " +
                                      RCBestParticle.Config.WarmUpCicles + "; " +
                                      RCBestParticle.Config.Interconnectivity + "; " +
                                      RCBestParticle.Config.SpectralRadious + "; ";

                if (DataType == EExecutionType.Predction)
                {
                    if ((PerformanceInfo & EEvaluationInfo.EMQ) != EEvaluationInfo.None)
                        resultString += RCBestParticle.Evaluator.TestEMQ[ResultIndex].ToString("0.####") + "; ";

                    if ((PerformanceInfo & EEvaluationInfo.DEV) != EEvaluationInfo.None)
                        resultString += RCBestParticle.Evaluator.TestDEV[ResultIndex].ToString("0.####") + "; ";

                    if ((PerformanceInfo & EEvaluationInfo.RMSE) != EEvaluationInfo.None)
                        resultString += RCBestParticle.Evaluator.TestRMSE[ResultIndex].ToString("0.####") + "; ";

                    if ((PerformanceInfo & EEvaluationInfo.EPMA) != EEvaluationInfo.None)
                    {
                        for (int k = 0; k < RCBestParticle.Config.Prov.OutputsN; k++)
                            resultString += RCBestParticle.Evaluator.TestEPMA[k].ToString("0.####") + "; ";
                    }

                    if ((PerformanceInfo & EEvaluationInfo.EvaluateLikeONS) != EEvaluationInfo.None)
                    {
                        for (int l = 0; l < 7; l++)
                            resultString += RCBestParticle.Evaluator.TestEPMAForONS[l].ToString("0.####") + "; ";
                    }
                }
                else if ((PerformanceInfo & EEvaluationInfo.SR) != EEvaluationInfo.None)
                    resultString += RCBestParticle.Evaluator.TestSR[ResultIndex].ToString("0.####") + "; ";

                resultString += EndTime.Subtract(StartTime).ToReadableString();
                resultString += "; " + stopIteration;
                resultString += "; " + stopEvaluation;
                #endregion

                Console.WriteLine(DateTime.Now.ToShortTimeString() + info);
                Console.WriteLine(DateTime.Now.ToShortTimeString() + resultString);

                File = new System.IO.StreamWriter(string.Concat(simulationResultPath, @"RC\weight\result.csv"), true);
                File.WriteLine(resultString);
                File.Close();

                #region Saving Prediction
                if (SavePrediciton)
                {
                    real = 0;
                    prediction = 0;
                    error = 0;

                    File = new System.IO.StreamWriter(simulationResultPath + @"RC\weight\predicao_" + i + ".csv", false);

                    for (int m = 0; m < RCBestParticle.Config.Prov.TestSetlines; m++)
                    {
                        string temp = string.Empty;
                        for (int j = 0; j < RCBestParticle.Config.Prov.OutputsN; j++)
                        {
                            real = RCBestParticle.Config.Prov.TestSet[m].RealOutput[j];
                            prediction = RCBestParticle.Config.Prov.DeNormalizeOutputData(RCBestParticle.Evaluator.RC.TestT[m][j], j);

                            error = real - prediction;
                            temp = string.Concat(temp + RCBestParticle.Config.Prov.TestSet[m].RealOutput[j] + ";" + prediction + ";" + error + ";");
                        }
                        File.WriteLine(temp);
                    }
                    File.Close();
                }
                #endregion

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Console.WriteLine(DateTime.Now.ToShortTimeString() + "[RC-WEIGHT]Fim iteração: " + i);
            }
            #endregion
            #endregion

            #region RC-PSE
            #region Ajustando arquivo de resultados
            if (!System.IO.Directory.Exists(string.Concat(simulationResultPath, @"RC\pse\")))
                System.IO.Directory.CreateDirectory(string.Concat(simulationResultPath, @"RC\pse\"));

            File = new System.IO.StreamWriter(string.Concat(simulationResultPath, @"RC\pse\result.csv"), false);

            info = "Position; Seed; HidenNodes; InputNodes; WarmUpCicles; Interconnectivity; SpectralRadious; ";

            if (DataType == EExecutionType.Predction)
                info = string.Concat(info, (PerformanceInfo & EEvaluationInfo.EMQ) != EEvaluationInfo.None ? "EMQ; " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.DEV) != EEvaluationInfo.None ? "DEV; " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.RMSE) != EEvaluationInfo.None ? "RMSE; " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.EPMA) != EEvaluationInfo.None ? "EPMA(1); EPMA(2); EPMA(3); EPMA(4); EPMA(5); EPMA(6); EPMA(7); EPMA(8); EPMA(9); EPMA(10); EPMA(11); EPMA(12); EPMA(13); EPMA(14); " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.EvaluateLikeONS) != EEvaluationInfo.None ? "EPMA-ONS(1); EPMA-ONS(2); EPMA-ONS(3); EPMA-ONS(4); EPMA-ONS(5); EPMA-ONS(6); EPMA-ONS(7); " : string.Empty);
            else if ((PerformanceInfo & EEvaluationInfo.SR) != EEvaluationInfo.None)
                info = string.Concat("SuccessRate; ");

            info = string.Concat(info, "Time");
            info = string.Concat(info, "; StopCount");
            info = string.Concat(info, "; StopEvaluation");

            File.WriteLine(info);
            File.Close();
            #endregion

            #region Processando
            Console.WriteLine(DateTime.Now.ToShortTimeString() + "[RC-PSE]Processando arquivo: " + dataSetPath);

            for (int i = 1; i <= TestIterations; i++)
            {
                Console.WriteLine(DateTime.Now.ToShortTimeString() + "[RC-PSE]Início iteração: " + i);
                seed = Rand.Next();

                inputLag = 1;
                outputLag = 14;

                db = new DataBuilder(@dataSetPath);
                db.LagData(inputLag, outputLag);

                set = db.BuildDataSet();

                prov = new DataProvider(set, DataType, seed, trainSize, validationSize, testSize);
                prov.ApplyLogToData();
                prov.NormalizeData(inputA, inputB, outputA, outputB);

                prov.ShuffleDataSet();
                prov.SplitData();

                StartTime = DateTime.Now;

                rc = new RCHS(MaxInterConnectivity, MaxWarmUpCicles, MinSpectralRadious, prov, seed, MemorySize,
                    MaxHiddenNodes, MaxEvaluations, ActivationFunction, EHSEvaluationFunctionType.PSE, PerformanceInfo);

                returnVal = rc.Run();

                stopIteration = returnVal.StopIteration;
                stopEvaluation = returnVal.StopEvaluations;
                RCBestParticle = (RCHSParticle)returnVal.BestParticle;

                EndTime = DateTime.Now;

                #region Result building
                string resultString = i + "; " +
                                      seed + "; " +
                                      RCBestParticle.Config.HidenNodesNumber + "; " +
                                      RCBestParticle.Config.Prov.InputsN + "; " +
                                      RCBestParticle.Config.WarmUpCicles + "; " +
                                      RCBestParticle.Config.Interconnectivity + "; " +
                                      RCBestParticle.Config.SpectralRadious + "; ";

                if (DataType == EExecutionType.Predction)
                {
                    if ((PerformanceInfo & EEvaluationInfo.EMQ) != EEvaluationInfo.None)
                        resultString += RCBestParticle.Evaluator.TestEMQ[ResultIndex].ToString("0.####") + "; ";

                    if ((PerformanceInfo & EEvaluationInfo.DEV) != EEvaluationInfo.None)
                        resultString += RCBestParticle.Evaluator.TestDEV[ResultIndex].ToString("0.####") + "; ";

                    if ((PerformanceInfo & EEvaluationInfo.RMSE) != EEvaluationInfo.None)
                        resultString += RCBestParticle.Evaluator.TestRMSE[ResultIndex].ToString("0.####") + "; ";

                    if ((PerformanceInfo & EEvaluationInfo.EPMA) != EEvaluationInfo.None)
                    {
                        for (int k = 0; k < RCBestParticle.Config.Prov.OutputsN; k++)
                            resultString += RCBestParticle.Evaluator.TestEPMA[k].ToString("0.####") + "; ";
                    }

                    if ((PerformanceInfo & EEvaluationInfo.EvaluateLikeONS) != EEvaluationInfo.None)
                    {
                        for (int l = 0; l < 7; l++)
                            resultString += RCBestParticle.Evaluator.TestEPMAForONS[l].ToString("0.####") + "; ";
                    }
                }
                else if ((PerformanceInfo & EEvaluationInfo.SR) != EEvaluationInfo.None)
                    resultString += RCBestParticle.Evaluator.TestSR[ResultIndex].ToString("0.####") + "; ";

                resultString += EndTime.Subtract(StartTime).ToReadableString();
                resultString += "; " + stopIteration;
                resultString += "; " + stopEvaluation;
                #endregion

                Console.WriteLine(DateTime.Now.ToShortTimeString() + info);
                Console.WriteLine(DateTime.Now.ToShortTimeString() + resultString);

                File = new System.IO.StreamWriter(string.Concat(simulationResultPath, @"RC\pse\result.csv"), true);
                File.WriteLine(resultString);
                File.Close();

                #region Saving Prediction
                if (SavePrediciton)
                {
                    real = 0;
                    prediction = 0;
                    error = 0;

                    File = new System.IO.StreamWriter(simulationResultPath + @"RC\pse\predicao_" + i + ".csv", false);

                    for (int m = 0; m < RCBestParticle.Config.Prov.TestSetlines; m++)
                    {
                        string temp = string.Empty;
                        for (int j = 0; j < RCBestParticle.Config.Prov.OutputsN; j++)
                        {
                            real = RCBestParticle.Config.Prov.TestSet[m].RealOutput[j];
                            prediction = RCBestParticle.Config.Prov.DeNormalizeOutputData(RCBestParticle.Evaluator.RC.TestT[m][j], j);

                            error = real - prediction;
                            temp = string.Concat(temp + RCBestParticle.Config.Prov.TestSet[m].RealOutput[j] + ";" + prediction + ";" + error + ";");
                        }
                        File.WriteLine(temp);
                    }
                    File.Close();
                }
                #endregion

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Console.WriteLine(DateTime.Now.ToShortTimeString() + "[RC-PSE]Fim iteração: " + i);
            }
            #endregion
            #endregion
        }
    }
}
