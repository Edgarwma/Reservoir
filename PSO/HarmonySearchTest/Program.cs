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

namespace HarmonnySearchTest
{
    class Program
    {
        #region Attributes
        static Random Rand = new Random(1);
        static StreamWriter File;
        static DateTime StartTime;
        static DateTime EndTime;
        static string DataSetPath;
        static string SimulationResultPath;
        static bool SavePrediciton = true;
        #endregion

        #region ConfigurationParameters
        static double inputA = -0.85;
        static double inputB = 0.85;
        static double outputA = -0.85;
        static double outputB = 0.85;
        static double trainSize = 0.50;
        static double validationSize = 0.25;
        static double testSize = 0.25;
        static int TestIterations = 30;
        static int MemorySize = 50;
        static int MaxHiddenNodes = 150;
        static int MaxEvaluations = 100;
        static int InterConnectionsNumber = 10; //Aumenta de 0.02 em 0.02 => 20%
        static int WarmUpCicles = 25; //Aumenta de 2 em 2 => 50
        static int SpectralRadious = 10; //diminui de 0.03 em 0.03 range 0.7-1.0
        static EExecutionType DataType = EExecutionType.Predction;
        static ERCActivationFunctionType ActivationFunction = ERCActivationFunctionType.HyperbolicTangent;
        static EHSEvaluationFunctionType EvaluationFunction = EHSEvaluationFunctionType.Weight;
        static EEvaluationInfo PerformanceInfo = EEvaluationInfo.EPMA | EEvaluationInfo.RMSE | EEvaluationInfo.EMQ | EEvaluationInfo.DEV;
        #endregion

        static void Main(string[] args)
        {
            #region Paths
            DataSetPath = @"C:\Users\Edgar\Desktop\Dados\Furnas\lag2.csv";
            SimulationResultPath = @"C:\Users\Edgar\Desktop\Dados\Furnas\Predicao\furnas_weight.csv";
            #endregion
            
            #region Ajustando arquivo de resultados
            File = new System.IO.StreamWriter(SimulationResultPath, false);

            string info = "Seed; Hiden Nodes number; WarmUpCicles; InterConnectivity; RestricSpectralRadious; ";

            if (DataType == EExecutionType.Predction)
                info = string.Concat(info, (PerformanceInfo & EEvaluationInfo.EMQ) != EEvaluationInfo.None ? "EMQ-Train; EMQ-Val; " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.DEV) != EEvaluationInfo.None ? "DEV-Train; DEV-Val; " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.RMSE) != EEvaluationInfo.None ? "RMSE-Train; RMSE-Val; " : string.Empty,
                    (PerformanceInfo & EEvaluationInfo.EPMA) != EEvaluationInfo.None ? "EPMA-Train; EPMA-Val; " : string.Empty);
            else if ((PerformanceInfo & EEvaluationInfo.SR) != EEvaluationInfo.None)
                info = string.Concat("SuccessRate-Train; SuccessRate-Val; ");
            
            info = string.Concat(info, "Time");
            info = string.Concat(info, "; StopCount");
            info = string.Concat(info, "; StopEvaluation");

            File.WriteLine(info);        
            File.Close();
            #endregion
            
            Console.WriteLine("Processando arquivo: " + SimulationResultPath);

            for (int i = 1; i <= TestIterations; i++)
            {
                Console.WriteLine("Início iteração: " + i);
                int seed = Rand.Next();
                Random _Random = new Random(seed);

                StartTime = DateTime.Now;

                DataProvider prov = new DataProvider(DataSetPath, DataType, seed, trainSize, validationSize, testSize);
                prov.ApplyLogToData();
                prov.NormalizeData(inputA, inputB, outputA, outputB);

                prov.ShuffleDataSet();
                prov.SplitData();
                
                RCHS hs = new RCHS(InterConnectionsNumber, WarmUpCicles, SpectralRadious, prov, seed,MemorySize,
                    MaxHiddenNodes, MaxEvaluations, ActivationFunction, EvaluationFunction, PerformanceInfo);

                HSResult<byte> returnVal = hs.Run();

                int stopIteration = returnVal.StopIteration;
                int stopEvaluation = returnVal.StopEvaluations;
                RCHSParticle bestParticle = (RCHSParticle)returnVal.BestParticle;

                EndTime = DateTime.Now;

                string resultString = seed + "; " +
                                      bestParticle.Config.HidenNodesNumber + "; "+
                                      bestParticle.Config.WarmUpCicles + "; " +
                                      bestParticle.Config.Interconnectivity.ToString("0.######") + "; " +
                                      bestParticle.Config.SpectralRadious.ToString() + "; ";

                if (DataType == EExecutionType.Predction)
                {
                    if ((PerformanceInfo & EEvaluationInfo.EMQ) != EEvaluationInfo.None)
                    {
                        resultString += bestParticle.Evaluator.TrainEMQ[0].ToString("0.######") + "; ";
                        resultString += bestParticle.Evaluator.ValidationEMQ[0].ToString("0.######") + "; ";
                    }

                    if ((PerformanceInfo & EEvaluationInfo.DEV) != EEvaluationInfo.None)
                    {
                        resultString += bestParticle.Evaluator.TrainDEV[0].ToString("0.######") + "; ";
                        resultString += bestParticle.Evaluator.ValidationDEV[0].ToString("0.######") + "; ";
                    }

                    if ((PerformanceInfo & EEvaluationInfo.RMSE) != EEvaluationInfo.None)
                    {
                        resultString += bestParticle.Evaluator.TrainRMSE[0].ToString("0.######") + "; ";
                        resultString += bestParticle.Evaluator.ValidationRMSE[0].ToString("0.######") + "; ";
                    }

                    if ((PerformanceInfo & EEvaluationInfo.EPMA) != EEvaluationInfo.None)
                    {
                        resultString += bestParticle.Evaluator.TrainEPMA[0].ToString("0.######") + "; ";
                        resultString += bestParticle.Evaluator.ValidationEPMA[0].ToString("0.######") + "; ";
                    }
                }
                else if ((PerformanceInfo & EEvaluationInfo.SR) != EEvaluationInfo.None)
                {
                    resultString += bestParticle.Evaluator.TrainSR[0].ToString("0.######") + "; ";
                    resultString += bestParticle.Evaluator.ValidationSR[0].ToString("0.######") + "; ";
                }

                resultString += EndTime.Subtract(StartTime).ToReadableString();
                resultString += "; " + stopIteration;
                resultString += "; " + stopEvaluation;

                Console.WriteLine(info);
                Console.WriteLine(resultString);

                File = new System.IO.StreamWriter(SimulationResultPath, true);
                File.WriteLine(resultString);
                File.Close();

                ////////////////// Salvando predição ///////////////////////////////
                if (SavePrediciton)
                {
                    double real = 0;
                    double prediction = 0;
                    double Error = 0;

                    File = new System.IO.StreamWriter(SimulationResultPath + "predicao" + i + ".csv", false);

                    for (int m = 0; m < prov.TestSetlines; m++)
                    {
                        for (int j = 0; j < prov.OutputsN; j++)
                        {
                            real = prov.TestSet[m].RealOutput[j];
                            prediction = prov.DeNormalizeOutputData(bestParticle.Evaluator.RC.TestT[m][j], j);

                            Error = real - prediction;
                            File.WriteLine(real + ";" + prediction + ";" + Error);
                        }
                    }
                    File.Close();
                }
                //////////////////////////////////////
                Console.WriteLine("Fim iteração: " + i);
            }
            Console.WriteLine("Fim simulação");
            Console.ReadKey();
        }
    }
}

